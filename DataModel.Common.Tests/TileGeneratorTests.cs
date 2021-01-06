using System;
using System.Collections.Generic;
using System.Text;
using DataModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using NUnit.Framework;
using Google.OpenLocationCode;
using Microsoft.Reactive.Testing;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using DataModel.Common;
using DataModel.Common.BiomeConfigs;
using Newtonsoft.Json;
using System.IO;

namespace DataModel.Common.Tests
{
    [TestFixture]
    class TileGeneratorTests
    {
        TileGenerator tileGen;

        [SetUp]
        public void SetUp()
        {
            tileGen = new TileGenerator();
        }




        [Test]
        public void PrintGeneratedArea()
        {

            List<int> tileTypeGen = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            List<int> miniTileTypeGen = new List<int>() { 0, 1, 2, 3, 3, 3, 3 };
            TileGenerator.GenerateArea(new PlusCode("8FX9WWV9+PR", 8), 1, tileTypeGen, miniTileTypeGen).ForEach(v =>
            {
                Debug.WriteLine(v.ToString() + "CONTENTS: \n ");
                v.MiniTiles.ForEach(v2 =>
                {
                    Debug.WriteLine(v2.ToString());


                });
            });


        }
        [Test]
        public void PrintGeneratedAreaWithoutSpecifyingTypes()
        {


            TileGenerator.GenerateArea(new PlusCode("8FX9WWV9+PR", 8), 1).ForEach(v =>
            {
                Debug.WriteLine(v.ToString() + "CONTENTS: \n ");
                v.MiniTiles.ForEach(v2 =>
                {
                    Debug.WriteLine(v2.ToString());


                });
            });


        }


        [Test]
        public void TileGeneratorGeneratesOneTileWithArea0()
        {
            var code = new PlusCode("8FX9WWV9+", 8);
            var list = TileGenerator.GenerateArea(code, 0);
            Assert.IsTrue(list.Count == 1);
            Assert.IsTrue(list[0].PlusCode.Code.Equals(code.Code));
        }

        [Test]
        public void GivenPlusTheNeighborsAreSame()
        {
            var code = new PlusCode("8FX9WWV9+", 8);
            var tileStrings = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
            Assert.IsTrue(tileStrings[0].Length == 9);
            Assert.IsTrue(tileStrings.Count == 9);

            //TODO: Make GetTileSection work for minitiles, too.
            var code2 = new PlusCode("8FX9WWV9+22", 10);
            var tileStrings2 = LocationCodeTileUtility.GetTileSection(code2.Code, 1, code2.Precision);
            Assert.IsTrue(tileStrings2[0].Length == 11);
            Assert.IsTrue(tileStrings2.Count == 9);


        }



        [Test]
        public void GetTileSectionWithinChebyshevDistanceAndSortTest()
        {

            //8FX9XW2F+XX
            List<Tile> tileList = TileGenerator.GenerateArea(new PlusCode("8FX9XW2F+XX", 8), 1);


            List<MiniTile> tileSect = TileUtility.GetTileSectionWithinChebyshevDistance(new PlusCode("8FX9XW2F+XX", 10), tileList, 1);
            tileSect = LocationCodeTileUtility.SortList(tileSect);

            List<string> tileSectCorrectCodes = new List<string>();
            tileSectCorrectCodes.Add("8FX9XW3F+2W");
            tileSectCorrectCodes.Add("8FX9XW3F+2X");
            tileSectCorrectCodes.Add("8FX9XW3G+22");
            tileSectCorrectCodes.Add("8FX9XW2F+XW");
            tileSectCorrectCodes.Add("8FX9XW2F+XX");
            tileSectCorrectCodes.Add("8FX9XW2G+X2");
            tileSectCorrectCodes.Add("8FX9XW2F+WW");
            tileSectCorrectCodes.Add("8FX9XW2F+WX");
            tileSectCorrectCodes.Add("8FX9XW2G+W2");

            for (int i = 0; i < tileSect.Count; i++)
            {
                Assert.AreEqual(tileSectCorrectCodes[i], tileSect[i].MiniTileId.Code);
            }


        }

        [Test]

        public void GenerateMiniTilesTest()
        {

            List<int> miniTileTypeGen = new List<int>() { 0, 1, 2, 3, 3, 3, 3 };
            List<int> worldObjectTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("8FX9XW2F", 8), miniTileTypeGen, worldObjectTypeList);

            Assert.AreEqual(400, miniTileList.Count);



            for (int i = 0; i < 10; i++)
            {
                WorldObject obj = (WorldObject)miniTileList[i].Content[0];
                String objTypeName = obj.Type + "";
                Debug.WriteLine(miniTileList[i].MiniTileId.Code + " " + miniTileList[i].TileType + " " + objTypeName);
            }
            Assert.AreEqual("8FX9XW2F+X2", miniTileList[0].MiniTileId.Code);
            Assert.AreEqual("8FX9XW2F+X3", miniTileList[1].MiniTileId.Code);
            Assert.AreEqual("8FX9XW2F+XX", miniTileList[19].MiniTileId.Code);
            Assert.AreEqual("8FX9XW2F+W2", miniTileList[20].MiniTileId.Code);
            Assert.AreEqual("8FX9XW2F+W3", miniTileList[21].MiniTileId.Code);
            Assert.AreEqual("8FX9XW2F+2X", miniTileList[399].MiniTileId.Code);


        }

        [Test]

        public void GetSpecificTileTypeTest()
        {
            Tile parent = new Tile();
            parent.Ttype = TileType.Snow;

            String type = TileGenerator.GetSpecificMiniTileType(parent) + "";
            Debug.WriteLine(type);
            Assert.IsTrue(type == "Snow_Tile" || type == "Snow_River" || type == "Unknown_Type" || type == "Snow_PatchyGrass" || type == "Snow_River2");


            parent.Ttype = TileType.Swamp;
            type = TileGenerator.GetSpecificMiniTileType(parent) + "";
            Debug.WriteLine(type);
            Assert.IsTrue(type == "Mud_Marsh" || type == "Mud_Grass" || type == "Mud_Tile" || type == "Unknown_Tile");

        }
        [Test]
        public void GetSpecificWorldObjectPrintTest()
        {
            Tile parent = new Tile();
            parent.Ttype = TileType.Snow;

            String type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);



            parent.Ttype = TileType.Swamp;
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);

            parent.Ttype = TileType.WaterBody;
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);
            type = TileGenerator.GetSpecificWorldObject(parent) + "";
            Debug.WriteLine(type);

            parent.Ttype = TileType.Jungle;
            int seed = TileGenerator.GetRandomSeed();
            Debug.WriteLine("seed is " + seed);
            Random r = new Random(seed);
            /*  type = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type);

              String type2 = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type2);

              String type3 = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type3);
              r = new Random(seed);
              String type4 = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type);

              String type5 = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type2);

              String type6 = TileGenerator.GetSpecificWorldObject(parent, r) + "";
              Debug.WriteLine(type3);

              Assert.AreEqual(type, type4);
              Assert.AreEqual(type2, type5);
              Assert.AreEqual(type3, type6); */
        }

        [Test]
        public void SeedTest()
        {
            int seed = TileGenerator.GetRandomSeed();
            int test = TileGenerator.TestSeed(seed);
            int test2 = TileGenerator.TestSeed(seed);
            int test3 = TileGenerator.TestSeed(seed);
            Assert.AreEqual(test, test2);
            Assert.AreEqual(test, test3);
            int seed2 = TileGenerator.GetRandomSeed();
            test = TileGenerator.TestSeed(seed2);
            test2 = TileGenerator.TestSeed(seed2);
            test3 = TileGenerator.TestSeed(seed2);
            Assert.AreEqual(test, test2);
            Assert.AreEqual(test, test3);


        }

        [Test]
        public void GenerateTileWithJsonTest()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            Random r = new Random(11);
            Tile tile = TileGenerator.GenerateTile(new PlusCode("XXXXXX", 8), r, projectDir + @"\DataModel.Common\BiomeConfigs\");
            Debug.WriteLine(tile.Ttype + "");
            r = new Random(11);
            Tile tile2 = TileGenerator.GenerateTile(new PlusCode("XXXXXX", 8), r, projectDir + @"\DataModel.Common\BiomeConfigs\");
            r = new Random(13);
            Tile tile3 = TileGenerator.GenerateTile(new PlusCode("XXXXXX", 8), r, projectDir + @"\DataModel.Common\BiomeConfigs\");
            for (int i = 0; i < tile.MiniTiles.Count; i++)
            {
                WorldObject a = (WorldObject)tile.MiniTiles[i].Content[0];
                WorldObject b = (WorldObject)tile2.MiniTiles[i].Content[0];
                MiniTileType c = tile.MiniTiles[i].TileType;
                MiniTileType d = tile2.MiniTiles[i].TileType;
                Debug.WriteLine(a.Type + " " + b.Type + " " + c + " " + d);
                Assert.AreEqual(a.Type, b.Type);
                Assert.AreEqual(c, d);

                WorldObject e = (WorldObject)tile3.MiniTiles[i].Content[0];
                MiniTileType f = tile3.MiniTiles[i].TileType;
                Debug.WriteLine(e.Type + " " + f);
            }

        }

        [Test]
        public void GenerateAreaWithJsonTest()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            List<Tile> tile = TileGenerator.GenerateArea(new PlusCode("9MHQ8533", 8), 1, 15, projectDir + @"\DataModel.Common\BiomeConfigs\");
            List<Tile> tile2 = TileGenerator.GenerateArea(new PlusCode("9MHQ8534", 8), 1, 16, projectDir + @"\DataModel.Common\BiomeConfigs\");

            foreach(var miniT in tile[0].MiniTiles)
            {
                WorldObject a = (WorldObject)miniT.Content[0];
                Debug.WriteLine(miniT.Content[0] + " " + a.Type);
            }
            Debug.WriteLine("***************************************");
            foreach (var miniT in tile2[0].MiniTiles)
            {
                WorldObject a = (WorldObject)miniT.Content[0];
                Debug.WriteLine(miniT.Content[0] + " " + a.Type);
            }
        }
    }
}
