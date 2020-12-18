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
      
            String type = TileGenerator.GetSpecificTileType(parent) + "";
            Debug.WriteLine(type);
            Assert.IsTrue(type == "Snow_Tile" || type == "Snow_River" || type == "Unknown_Type" || type == "Snow_PatchyGrass" || type == "Snow_River2");

    
            parent.Ttype = TileType.Swamp;
            type = TileGenerator.GetSpecificTileType(parent) + "";
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


        }
    }
}
