﻿using System;
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

        public void GeneratePlusCodeArea()
        {
            List<string> testCodeList = LocationCodeTileUtility.GetTileSection("8FX9WWV9+PR", 1, 10);
            Assert.AreEqual(9, testCodeList.Count);
            testCodeList = LocationCodeTileUtility.GetTileSection("8FX9WWV9+PR", 2, 10);
            Assert.AreEqual(25, testCodeList.Count);
            testCodeList = LocationCodeTileUtility.GetTileSection("8FX9WWV9+PR", 4, 10);
            Assert.AreEqual(81, testCodeList.Count);
            testCodeList = LocationCodeTileUtility.GetTileSection("8FX9WWV9+PR", 9, 10);
            Assert.AreEqual(361, testCodeList.Count);
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
         //   var code2 = new PlusCode("8FX9WWV9+22", 10);
         //   var tileStrings2 = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
         //   Assert.IsTrue(tileStrings2[0].Length == 11);
         //   Assert.IsTrue(tileStrings2.Count == 9);
            ;

        }


        [Test]
        public void CoordsFileTest()
        {
            //8FX9XW2F+XX
            //    List<Tile> tileList = TileGenerator.GenerateArea(new PlusCode("8FX9XW2F+XX", 8), 1);
            //    int tileCount = 0;
            //    foreach (Tile t in tileList)
            //   {
            //       TileUtility.ReadableMini2DArrayFile(TileUtility.GetMiniTile2DArray(t.MiniTiles, 20), @"C:\Users\Kat\Desktop\2DTileArray" + tileCount + ".txt");
            //        tileCount++;
            //    }
            //
            //   List<MiniTile> tileSect = TileUtility.GetTileSectionWithinChebyshevDistance(new PlusCode("8FX9XW2F+XX", 10), tileList, 1);
            //   tileSect = LocationCodeTileUtility.SortList(tileSect);
            //   TileUtility.ReadableMini2DArrayFile(TileUtility.GetMiniTile2DArray(tileSect, 3), @"C:\Users\Kat\Desktop\2DTileArrayTileSect.txt");

        }


        [Test]
        public void SomeCoordsTest()
        {

            List<int> miniTileTypeGen = new List<int>() { 0, 1, 2, 3, 3, 3, 3 };
            List<MiniTile> tileList = TileGenerator.GenerateMiniTiles(new PlusCode("8FX9WWV9+", 8), miniTileTypeGen);

            MiniTile[,] miniTile2D = TileUtility.GetMiniTile2DArray(tileList, 20);
            Assert.AreEqual("8FX9WWV9+2X", miniTile2D[19, 19].MiniTileId.Code);
            Assert.AreEqual("8FX9WWV9+4R", miniTile2D[17, 16].MiniTileId.Code);
            Assert.AreEqual("8FX9WWV9+22", miniTile2D[19, 0].MiniTileId.Code);
            Assert.AreEqual("8FX9WWV9+C5", miniTile2D[11, 3].MiniTileId.Code);
            Assert.AreEqual("8FX9WWV9+P7", miniTile2D[5, 5].MiniTileId.Code);


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


    }
}
