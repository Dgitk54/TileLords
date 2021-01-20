using DataModel.Common.BiomeConfigs;
using DataModel.Common.MiniTileTypes;
using DataModel.Common.WorldObjectTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// <param name="miniTileTypeList">An integer list of possible types the miniTile can have (see MiniTileType enum)</param>
        /// <param name="worldObjectTypeList">An integer list of possible worldObject types the miniTile can have (see WorldObjectType enum)</param>
        /// <returns>The MiniTile list</returns>
        public static List<MiniTile> GenerateMiniTiles(PlusCode tileCode, List<int> miniTileTypeList, List<int> worldObjectTypeList)
        {
            string code = tileCode.Code;
            if (code.Length > 9)
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

        /// <summary>
        /// Function which regenerates an area around a miniTile. Changes the old state of the area to the new one (keep duplicates and new tiles, remove old tiles)
        /// </summary>
        /// <param name="miniTileCode">The Pluscode of the miniTile to regenerate from</param>
        /// <param name="currentTiles">A list of the "old state" miniTiles</param>
        /// <param name="newTiles">A list of the "new state" miniTiles</param>
        /// <param name="radius">The radius of the regenerated area</param>
        /// <returns>The MiniTile list of all tiles in the regenerated area</returns>
        public static Dictionary<PlusCode, MiniTile> RegenerateArea(PlusCode miniTileCode, Dictionary<PlusCode, MiniTile> currentTiles, Dictionary<PlusCode, MiniTile> newTiles, int radius)
        {
            if (newTiles.Count == 0)
            {
                return currentTiles;
            }


            var allNewPlusCodes = LocationCodeTileUtility.GetTileSection(miniTileCode.Code, radius, miniTileCode.Precision);
            Dictionary<PlusCode, MiniTile> toReturn = new Dictionary<PlusCode, MiniTile>();

            foreach (var code in allNewPlusCodes)
            {
                if (currentTiles.TryGetValue(new PlusCode(code, 10), out MiniTile miniTile))
                {
                    toReturn.Add(miniTile.MiniTileId, miniTile);
                }
                else if (newTiles.TryGetValue(new PlusCode(code, 10), out MiniTile miniTileNew))
                {
                    toReturn.Add(miniTileNew.MiniTileId, miniTileNew);
                }
            }

            return toReturn;
            /* var oldArea = new ConcurrentBag<MiniTile>();
             var newArea = new ConcurrentBag<MiniTile>();
             var miniTileToAdd = new ConcurrentBag<MiniTile>();


             currentTiles.AsParallel().ForAll(v =>
             {
                 allNewPlusCodes.AsParallel()
                 .Where(e => v.MiniTileId.Code.Equals(e))
                 .ForAll(e => oldArea.Add(v));

             });



             newTiles.AsParallel().ForAll(v =>
             {
                 allNewPlusCodes.AsParallel()
                 .Where(e => v.MiniTileId.Code.Equals(e))
                 .ForAll(e => newArea.Add(v));
             });


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
        /// <param name="tileTypeList">An integer list of possible types the tile can have (see TileType enum)</param>
        /// <param name="miniTileTypeList">An integer list of possible types the miniTiles can have (see MiniTileType enum)</param>
        /// <param name="worldObjectTypeList">An integer list of possible worldObject types the miniTiles can have (see WorldObjectType enum)</param>
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
        /// <param name="tileTypeList">An integer list of possible types the tile can have (see TileType enum)</param>
        /// <param name="miniTileTypeList">An integer list of possible types the miniTiles can have (see MiniTileType enum)</param>
        /// <returns>The generated area as a list of Tiles</returns>
        public static List<Tile> GenerateArea(PlusCode code, int radius, List<int> tileTypeList, List<int> miniTileTypeList)
        {
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), tileTypeList, miniTileTypeList, null);
            return tiles.ToList();
        }


        /// <summary>
        /// Function which generates an area consisting of Tiles and Minitiles (with predefined types)
        /// </summary>
        /// <param name="code">The Pluscode of the Tile to generate from</param>
        /// <param name="radius">The radius to generate the Tiles</param>
        /// <returns>The generated area as a list of Tiles</returns>
        public static List<Tile> GenerateArea(PlusCode code, int radius)
        {
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            List<int> tileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            List<int> miniTileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<int> worldObjectTypeList = new List<int>();

            //@@@@@@@@@@@@@@@@@@
            //somewhat useless code that adds index for each worldObjectType to a list to select one at random (adding empty object many times to balance it), should be adjusted for a real generator to only use the index of relevant worldObjects
            //@@@@@@@@@@@@@@@@@@
            for (int i = 0; i < Enum.GetNames(typeof(WorldObjectType)).Length; i++)
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


        /// <summary>
        /// Function which returns a specific miniTileType given the parent tile (type / biome)
        /// </summary>
        /// <returns>A random type from the biome enums (see folder MiniTileTypes)</returns>
        public static Enum GetSpecificMiniTileType(Tile parentTile)
        {

            int length;
            Random r;
            int i;



            switch (parentTile.Ttype)
            {
                case TileType.Desert:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Desert)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Desert)i;
                    }
                case TileType.Grassland:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Grassland)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Grassland)i;
                    }
                case TileType.Swamp:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Swamp)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Swamp)i;
                    }
                case TileType.Jungle:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Jungle)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Jungle)i;
                    }
                case TileType.Mountains:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Mountains)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Mountains)i;
                    }
                case TileType.Snow:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Snow)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Snow)i;
                    }
                case TileType.Savanna:
                    {
                        length = Enum.GetNames(typeof(MiniTileType_Savanna)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (MiniTileType_Savanna)i;
                    }

            }
            return MiniTileType.Unknown_Tile;

        }

        /// <summary>
        /// Function which returns a specific worldObjectType given the parent tile (type / biome)
        /// </summary>
        /// <returns>A random worldObjectType from the biome enums (see folder WorldObjectTypes)</returns>
        public static Enum GetSpecificWorldObject(Tile parentTile)
        {

            int length;
            Random r;
            int i;



            switch (parentTile.Ttype)
            {
                case TileType.Desert:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Desert)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Desert)i;
                    }
                case TileType.Grassland:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Grassland)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Grassland)i;
                    }
                case TileType.Swamp:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Swamp)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Swamp)i;
                    }
                case TileType.Jungle:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Jungle)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Jungle)i;
                    }
                case TileType.Mountains:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Mountains)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Mountains)i;
                    }
                case TileType.Snow:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Snow)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Snow)i;
                    }
                case TileType.Savanna:
                    {
                        length = Enum.GetNames(typeof(WorldObjectType_Savanna)).Length;
                        r = new Random();
                        i = r.Next(0, length);
                        return (WorldObjectType_Savanna)i;
                    }

            }
            return WorldObjectType.Empty;

        }

        public static Enum GetSpecificWorldObject(Tile parentTile, Random r, List<WorldObjectType> weightList)
        {


            int length = weightList.Count;
            int i;


            switch (parentTile.Ttype)
            {
                case TileType.Desert:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }
                case TileType.Grassland:
                    {

                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Swamp:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }
                case TileType.Jungle:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }
                case TileType.Mountains:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }
                case TileType.Snow:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }
                case TileType.Savanna:
                    {


                        i = r.Next(0, length);
                        return weightList[i];
                    }

            }
            return WorldObjectType.Empty;

        }

        public static Enum GetSpecificMiniTileType(Tile parentTile, Random r, List<MiniTileType> weightList)
        {

            int length = weightList.Count;
            int i;

            switch (parentTile.Ttype)
            {
                case TileType.Desert:
                    {

                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Grassland:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Swamp:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Jungle:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Mountains:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Snow:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }
                case TileType.Savanna:
                    {


                        i = r.Next(0, length);

                        return weightList[i];
                    }

            }
            return MiniTileType.Unknown_Tile;

        }


        public static int GetRandomSeed()
        {
            Random r = new Random();
            int seed = r.Next(0, Int32.MaxValue);

            return seed;
        }


        public static int TestSeed(int seed)
        {
            Random seededRandom = new Random(seed);
            return seededRandom.Next(1, 100);


        }


        public static List<Tile> GenerateArea(PlusCode code, int radius, int seed, String fileLocation)
        {

            Random r = new Random(seed);
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), r, fileLocation);
            return tiles.ToList();
        }

        public static List<Tile> GenerateArea(PlusCode code, int radius, int seed)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string fileLocation = projectDir + @"\DataModel.Common\BiomeConfigs\";
            Random r = new Random(seed);
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), r, fileLocation);
            return tiles.ToList();
        }

        public static List<Tile> GenerateArea(PlusCode code, int radius, Random rand, String fileLocation)
        {

            Random r = rand;
            string c = code.Code;
            if (c.Length > 8)
            {
                c = c.Substring(0, 8);
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(c, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), r, fileLocation);
            return tiles.ToList();
        }



        public static List<MiniTile> GenerateMiniTiles(Tile tile, Random r, List<WorldObjectType> worldObjectWeightList, List<MiniTileType> typeWeightList)
        {
            string code = tile.PlusCode.Code;
            if (code.Length > 9)
            {
                code = code.Substring(0, 9);
            }
            if (!code.Contains("+"))
            {
                code += "+";
            }


      

            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), (MiniTileType)GetSpecificMiniTileType(tile, r, typeWeightList), new List<ITileContent>() { new WorldObject((WorldObjectType)GetSpecificWorldObject(tile, r, worldObjectWeightList)) });
    
            return tiles.ToList();

        }

        public static Tile GenerateTile(PlusCode code, Random r, String biomeConfigLocation)
        {
            TileType type = getRandomTileType(r);

            Tile tile = new Tile(code, type);
            IBiome biome = null;
            if (type == TileType.Grassland)
            {
                biome = (GrasslandBiome)JsonConvert.DeserializeObject<GrasslandBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Desert)
            {
                biome = (DesertBiome)JsonConvert.DeserializeObject<DesertBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Jungle)
            {
                biome = (JungleBiome)JsonConvert.DeserializeObject<JungleBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Mountains)
            {
                biome = (MountainsBiome)JsonConvert.DeserializeObject<MountainsBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Savanna)
            {
                biome = (SavannaBiome)JsonConvert.DeserializeObject<SavannaBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Snow)
            {
                biome = (SnowBiome)JsonConvert.DeserializeObject<SnowBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            if (type == TileType.Swamp)
            {
                biome = (SwampBiome)JsonConvert.DeserializeObject<SwampBiome>(File.ReadAllText(biomeConfigLocation + "" + type + ".json"));
            }
            /*if (type == TileType.WaterBody)
            {
                biome = (WaterBodyBiome)JsonConvert.DeserializeObject<WaterBodyBiome>(File.ReadAllText(fileLocation + "" + type + ".json"));
            }*/


            List<WorldObjectType> worldObjectWeightList = new List<WorldObjectType>();
            List<MiniTileType> typeWeightList = new List<MiniTileType>();


            for (int i = 0; i < biome.WorldObjectWeightDict.Count; i++)
            {
                for (int j = 0; j < biome.WorldObjectWeightDict.ElementAt(i).Value; j++)
                {
                    WorldObjectType adding = (WorldObjectType)Enum.Parse(typeof(WorldObjectType), biome.WorldObjectWeightDict.ElementAt(i).Key);
                    worldObjectWeightList.Add(adding);

                }

            }

            for (int i = 0; i < biome.TileTypeWeightDict.Count; i++)
            {
                for (int j = 0; j < biome.TileTypeWeightDict.ElementAt(i).Value; j++)
                {
                    MiniTileType adding = (MiniTileType)Enum.Parse(typeof(MiniTileType), biome.TileTypeWeightDict.ElementAt(i).Key);
                    typeWeightList.Add(adding);


                }

            }

            tile.MiniTiles = GenerateMiniTiles(tile, r, worldObjectWeightList, typeWeightList);
            Debug.WriteLine(tile.MiniTiles[0].TileType + " " + tile.MiniTiles[0].TileType + " " + tile.MiniTiles[0].TileType);


            return tile;
        }


        public static TileType getRandomTileType(Random r)
        {

            int length = Enum.GetNames(typeof(TileType)).Length;
            int i = r.Next(0, length);


            return (TileType)i;

        }

    }

}
