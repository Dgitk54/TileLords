using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Common
{
    public class TileGenerator
    {
        /// <summary>
        /// Function which generates a list of MiniTiles 
        /// </summary>
        /// <param name="tileCode">The Pluscode of the parent Tile</param>
        /// <returns>The MiniTile list</returns>
        public static List<MiniTile> GenerateMiniTiles(PlusCode tileCode, List<int> miniTileTypeList, List<int> worldObjectTypeList)
        {
            string code = tileCode.Code;
            if(code.Length > 9)
            {
                code = code.Substring(0, 9);
            }
            if (!code.Contains("+"))
            {
                code += "+";
            }
          
            
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), getRandomMiniTileType(miniTileTypeList), new List<ITileContent>() { new WorldObject(getRandomWorldObject(worldObjectTypeList)) });

            return tiles.ToList();

        }


        public static List<MiniTile> RegenerateArea(PlusCode miniTileCode, List<MiniTile> currentTiles, IList<MiniTile> newTiles, int radius)
        {
            if(newTiles.Count == 0)
            {
                return currentTiles;
            }

    
            var allNewPlusCodes = LocationCodeTileUtility.GetTileSection(miniTileCode.Code, radius, miniTileCode.Precision);
            var oldArea = new ConcurrentBag<MiniTile>();
            var newArea = new ConcurrentBag<MiniTile>();
            var miniTileToAdd = new ConcurrentBag<MiniTile>();

            var task1 = Task.Run(() => {
                currentTiles.AsParallel().ForAll(v =>
                {
                    allNewPlusCodes.AsParallel()
                    .Where(e => v.MiniTileId.Code.Equals(e))
                    .ForAll(e => oldArea.Add(v));

                });
            });

            var task2 = Task.Run(() => 
            {
                newTiles.AsParallel().ForAll(v =>
                {
                    allNewPlusCodes.AsParallel()
                    .Where(e => v.MiniTileId.Code.Equals(e))
                    .ForAll(e => newArea.Add(v));
                });
            });

            
            Task.WhenAll(task1, task2).Wait();
            

            oldArea.AsParallel()
                .ForAll(oldTile =>
                {
                    bool allowedToAdd = false;

                    foreach (MiniTile newA in newArea)
                    {
                        if (oldTile.MiniTileId.Code.Equals(newA.MiniTileId.Code))
                        {
                            allowedToAdd = false;
                            break;
                        }
                        else
                        {
                            allowedToAdd = true;
                        }
                    }

                    if (allowedToAdd)
                    {
                        miniTileToAdd.Add(oldTile);
                    }

                });

            var toReturn = newArea.ToList();
            toReturn.AddRange(miniTileToAdd.ToList());
            return toReturn;

            /*

                List<MiniTile> oldArea = new List<MiniTile>();
                List<MiniTile> newArea = new List<MiniTile>();
                List<MiniTile> miniTileToAdd = new List<MiniTile>();

               foreach (MiniTile currentMiniTile in currentTiles)
               {
                   foreach(string s in allNewPlusCodes)
                   {
                       if (currentMiniTile.MiniTileId.Code.Equals(s))
                       {
                           oldArea.Add(currentMiniTile);
                       }
                   }
               }
            
            
            
            
            
               foreach (MiniTile newMiniTiles in newTiles)
               {
                   foreach (string s in allNewPlusCodes)
                   {
                       if (newMiniTiles.MiniTileId.Code.Equals(s))
                       {
                           newArea.Add(newMiniTiles);
                       }
                   }
               }

            foreach (MiniTile old in oldArea)
            {
                bool allowedToAdd = false;

                foreach (MiniTile newA in newArea)
                {
                    if (old.MiniTileId.Code.Equals(newA.MiniTileId.Code))
                    {
                        allowedToAdd = false;
                        break;
                    }
                    else
                    {
                        allowedToAdd = true;
                    }
                }

                if (allowedToAdd)
                {
                    miniTileToAdd.Add(old);
                }
            }

            foreach (MiniTile add in miniTileToAdd)
            {
                newArea.Add(add);
            }

            return newArea.ToList();

            */




        }


        /// <summary>
        /// Function which generates a Tile and the MiniTile children of it
        /// </summary>
        /// <param name="code">The Pluscode of the Tile</param>
        /// <returns>The Tile</returns>
        public static Tile GenerateTile(PlusCode code, List<int> tileTypeList, List<int> miniTileTypeList, List<int> worldObjectTypeList)
        {
            return new Tile(code, getRandomTileType(tileTypeList), GenerateMiniTiles(code, miniTileTypeList, worldObjectTypeList));
        }

        /// <summary>
        /// Function which generates an area consisting of Tiles and Minitiles 
        /// </summary>
        /// <param name="code">The Pluscode of the Tile to generate from</param>
        /// <param name="radius">The radius to generate the Tiles</param>
        /// <returns>The generated area as a list of Tiles</returns>
        public static List<Tile> GenerateArea(PlusCode code, int radius, List<int> tileTypeList, List<int> miniTileTypeList)
        {
            string c = code.Code;
            if(c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), tileTypeList, miniTileTypeList, null);
            return tiles.ToList();
        }

        public static List<Tile> GenerateArea(PlusCode code, int radius)
        {
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            List<int> tileTypeList = new List<int>() { 0,1,2,3,4,5,6,7};
            List<int> miniTileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<int> worldObjectTypeList = new List<int>();
            
            //@@@@@@@@@@@@@@@@@@
            //somewhat useless code that adds index for each worldObjectType to a list to select one at random (adding empty object many times to balance it), should be adjusted for a real generator to only use the index of relevant worldObjects
            //@@@@@@@@@@@@@@@@@@
            for(int i = 0; i < Enum.GetNames(typeof(WorldObjectType)).Length; i++)
            {
                worldObjectTypeList.Add(i);
            }
            for (int i = 0; i < 60; i++)
            {
                worldObjectTypeList.Add(0);
            }

            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), tileTypeList, miniTileTypeList, worldObjectTypeList);
            return tiles.ToList();
        }

        public static MiniTileType getRandomMiniTileType(List<int> tileTypeList)
        {
            Random r = new Random();
            int i = r.Next(0, tileTypeList.Count);
            return (MiniTileType)tileTypeList[i];
        }


        public static TileType getRandomTileType(List<int> tileTypeList)
        {
            Random r = new Random();
            int i = r.Next(0, tileTypeList.Count);


            return (TileType)tileTypeList[i];

        }

        public static WorldObjectType getRandomWorldObject(List<int> worldObjectTypeList)
        {
            if (worldObjectTypeList == null)
                return WorldObjectType.Empty;

            Random r = new Random();
            
            int i = r.Next(0, worldObjectTypeList.Count);


            return (WorldObjectType)worldObjectTypeList[i];

        }

    }
}
