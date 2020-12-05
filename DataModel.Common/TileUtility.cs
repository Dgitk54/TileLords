using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DataModel.Common
{
    public class TileUtility
    {


        /// <summary>
        /// Given the List of possible tiles, this code returns all tiles within a chebyshev distance.
        /// </summary>
        /// <param name="locationCode"></param>
        /// <param name="tileList"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static List<MiniTile> GetTileSectionWithinChebyshevDistance(PlusCode locationCode, List<Tile> tileList, int distance)
        {
            var minitile =
             from tile in tileList
             from miniTile in tile.MiniTiles
             where PlusCodeUtils.GetChebyshevDistance(locationCode, miniTile.MiniTileId) <= distance
             select miniTile;

            return minitile.ToList();
        }
        public static List<MiniTile> GetMiniTileSectionWithinChebyshevDistance(PlusCode locationCode, IList<MiniTile> tileList, int distance)
        {
            var minitile = from miniTile in tileList
                           where PlusCodeUtils.GetChebyshevDistance(locationCode, miniTile.MiniTileId) <= distance
                           select miniTile;
            return minitile.ToList();
        }

        public static bool EqualsBasedOnPlusCode(MiniTile t1, MiniTile t2) => t1.MiniTileId.Code.Equals(t2.MiniTileId.Code);

        public static List<MiniTile> ConcatWithReplaceOld(IList<MiniTile> old, IList<MiniTile> newList, PlusCode currentLocation, int distanceCutoff)
        {
            //TODO: fix this ...
            IEnumerable<MiniTile> result;
            if(newList.Count != 0 && old.Count != 0)
            {
                result = from v1 in old
                         from v2 in newList
                         where (EqualsBasedOnPlusCode(v1,v2) || PlusCodeUtils.GetChebyshevDistance(currentLocation, v1.MiniTileId) > distanceCutoff)
                         select v1;
            }  
            else
            {
                result = GetMiniTileSectionWithinChebyshevDistance(currentLocation, old, 20);
            }
             
            var resultAsList = result.ToList();

            var updatedEnumeration = old.Except(result);
            
            var asList = updatedEnumeration.Concat(newList).ToList();



            var resultCount = result.Count();
            var resultAsListCount = resultAsList.Count();
            var updatedEnumerationCount = updatedEnumeration.Count();
            var asListCount = asList.Count();

            //breakpoint
            ;
            return asList;

        }

        public static List<MiniTile> ConcatWithReplaceOld(IList<MiniTile> old, IList<MiniTile> @new)
        {

            var result = from v1 in old
                         from v2 in @new
                         where (v1.MiniTileId.Equals(v2.MiniTileId))
                         select v1;

            var updatedEnumeration = old.Except(result);
            return updatedEnumeration.Concat(@new).ToList();
        }


        /// <summary>
        /// Function which creates a 2d array to represent the MiniTiles
        /// </summary>
        /// <param name="miniTiles">The list of MiniTiles for the 2d array.</param>
        /// <param name="squareSize">The size of the 2d array.</param>
        /// <returns>The MiniTile 2D array</returns>
        public static MiniTile[,] GetMiniTile2DArray(List<MiniTile> miniTiles, int squareSize)
        {

            MiniTile[,] miniTile2DArray = new MiniTile[squareSize, squareSize];
            int miniTileCounter = 0;
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    miniTile2DArray[i, j] = miniTiles[miniTileCounter];
                    miniTileCounter++;
                }
            }
            return miniTile2DArray;

        }


        /// <summary>
        /// Function which creates a 2d array to represent the Tiles
        /// </summary>
        /// <param name="miniTiles">The list of Tikes for the 2d array.</param>
        /// <param name="squareSize">The size of the 2d array.</param>
        /// <returns>The Tile 2D array</returns>

        public static Tile[,] GetTile2DArray(List<Tile> tile, int squareSize)
        {

            Tile[,] tile2DArray = new Tile[squareSize, squareSize];
            int tileCounter = 0;
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    tile2DArray[i, j] = tile[tileCounter];
                    tileCounter++;
                }
            }
            return tile2DArray;

        }


        /// <summary>
        /// Function which creates a textfile with a representation of the miniTile 2D array
        /// </summary>
        /// <param name="miniTileArray">The miniTile 2D array to be printed on the textfile.</param>
        /// <param name="fileLocation">The Location where the textfile will be stored.</param>

        public static void ReadableMini2DArrayFile(MiniTile[,] miniTileArray, String fileLocation)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fileLocation))
                for (int i = 0; i < miniTileArray.GetLength(0); i++)
                {

                    file.WriteLine("");
                    file.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------" +
                        "----------------------------------------------------------------------------------------------------------------------------------------------");
                    for (int j = 0; j < miniTileArray.GetLength(1); j++)
                    {

                        file.Write(miniTileArray[i, j].MiniTileId.Code + " (" + miniTileArray[i, j].TileType + ") | ");

                    }
                }

        }



        /// <summary>
        /// Function which finds a Minitile inside a given tileList
        /// </summary>
        /// <param name="locationCode">PlusCode to search for.</param>
        /// <param name="tileList">A 2 dimensional map of the world represented in tiles.</param>
        /// <returns>The MiniTile searched for</returns>
        public static MiniTile GetMiniTile(PlusCode locationCode, List<List<Tile>> tileList)
        {
            var minitile =
              from tileRows in tileList
              from tile in tileRows
              from miniTile in tile.MiniTiles
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }

        public static MiniTile GetMiniTile(PlusCode locationCode, List<Tile> tileList)
        {
            var minitile =
              from tile in tileList
              from miniTile in tile.MiniTiles
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }

        public static MiniTile GetMiniTile(PlusCode locationCode, List<MiniTile> miniTileList)
        {
            var minitile =
              from miniTile in miniTileList
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }


        public static string SerializeTile(Tile tile)
        {
            return JsonConvert.SerializeObject(tile);
        }


        public static Tile DeserializeTile(string text)
        {
            return JsonConvert.DeserializeObject<Tile>(text);
        }











    }
}
