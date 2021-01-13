using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Diagnostics;
using DataModel.Common;
using System.Reactive.Linq;

namespace DataModel.Common.Tests
{
    [TestFixture]
    class TileUtilityTests
    {

        List<int> tileTypeList;
        List<int> miniTileTypeList;
        List<int> worldObjectTypeList;

        [SetUp]
        public void SetUp()
        {
            tileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            miniTileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            worldObjectTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        }
        [Test]
        public void GivenTileSectionTheChebyshevDistanceReturnsProperAmount()
        {
            var startCode = new PlusCode("8FX9XW2F+XX", 10);
            var tiles = TileGenerator.GenerateArea(startCode, 1);
            var miniTiles = TileUtility.GetTileSectionWithinChebyshevDistance(startCode, tiles, 1);

            Assert.IsTrue(miniTiles.Count == 3 * 3);
            miniTiles = TileUtility.GetTileSectionWithinChebyshevDistance(startCode, tiles, 0);
            Assert.IsTrue(miniTiles.Count == 1 * 1);
            miniTiles = TileUtility.GetTileSectionWithinChebyshevDistance(startCode, tiles, 2);
            Assert.IsTrue(miniTiles.Count == 5 * 5);
        }

        /*[Test]
         public void GetTileSectionReturnsCodesWithPlus()
        {
            
            var list = LocationCodeTileUtility.GetTileSection("8FX9XW2F+", 5, 8);
            
            
            Assert.IsTrue(list[0][8] == '+');
            var watch = Stopwatch.StartNew();
            var list2 = LocationCodeTileUtility.GetTileSection("8FX9XW2F+XX", 5, 10);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.WriteLine(elapsedMs);
            Assert.IsTrue(list2[0][8] == '+');

            watch = Stopwatch.StartNew();
            var list3 = LocationCodeTileUtility.GetTileSection("8FX9XW2F+XX", 50, 10);
            watch.Stop();
            var tileSection50Time = watch.ElapsedMilliseconds;

            watch = Stopwatch.StartNew();
            var list4 = LocationCodeTileUtility.GetTileSectionParallel("8FX9XW2F+XX", 50, 10);
            watch.Stop();
            var tileSection50ParallelTime = watch.ElapsedMilliseconds;

            Assert.IsTrue(list4.Count == list3.Count);
            ;
        } */

        [Test]
        public void GivenEdgeTileChebyshevAlsoFindsNeighborsInOtherTiles()
        {
            var startCode = new PlusCode("8FX9XW2F+XX", 10);
            var tiles = TileGenerator.GenerateArea(startCode, 1);
            var miniTiles = TileUtility.GetTileSectionWithinChebyshevDistance(startCode, tiles, 1);
            var startTile = startCode.ToLowerResolution(8);

            var neighborMiniTilesOnly = from tile in miniTiles
                                        where !DataModelFunctions.ToLowerResolution(tile.MiniTileId, 8).Equals(startTile)
                                        select tile;

            var tileParentList = new List<PlusCode>();
            neighborMiniTilesOnly.ToList().ForEach(v =>
            {
                var tileParent = DataModelFunctions.ToLowerResolution(v.MiniTileId, 8);
                if (!tileParentList.Contains(tileParent))
                    tileParentList.Add(tileParent);
            }
            );

            Assert.IsTrue(tileParentList.Count == 3);


        }


        [Test]
        public void TestGetTileByMiniTileList()
        {

            List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList, worldObjectTypeList);
            MiniTile miniTile = TileUtility.GetMiniTile(new PlusCode("9F4MGC94+MC", 10), miniTileList);
            Assert.AreEqual("9F4MGC94+MC", miniTile.MiniTileId.Code);
            MiniTile miniTile2 = TileUtility.GetMiniTile(new PlusCode("9F4MGC94+X2", 10), miniTileList);
            Assert.AreEqual("9F4MGC94+X2", miniTile2.MiniTileId.Code);

        }
        [Test]
        public void TestGetTilesByTileList()
        {

            List<Tile> tileList = TileGenerator.GenerateArea(new PlusCode("9F4MGC94+", 8), 1, tileTypeList, miniTileTypeList);
            MiniTile miniTile = TileUtility.GetMiniTile(new PlusCode("9F4MGC94+MC", 10), tileList);
            Assert.AreEqual("9F4MGC94+MC", miniTile.MiniTileId.Code);
            MiniTile miniTile2 = TileUtility.GetMiniTile(new PlusCode("9F4MGC94+X2", 10), tileList);
            Assert.AreEqual("9F4MGC94+X2", miniTile2.MiniTileId.Code);

        }

        [Test]
        public void TestCodeBigger()
        {



            bool isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("9F4MGC94+X2", 10), new PlusCode("8F4MGC94+X2", 10));
            Assert.IsFalse(isBigger);
            isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("8F4MGC94+X2", 10), new PlusCode("9F4MGC94+X2", 10));
            Assert.IsTrue(isBigger);
            isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("9F4MGC94+X2", 10), new PlusCode("924MGC94+X2", 10));
            Assert.IsTrue(isBigger);
            isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("924MGC94+X2", 10), new PlusCode("9F4MGC94+X2", 10));
            Assert.IsFalse(isBigger);
            isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("9F4MGC94+2X", 10), new PlusCode("9F4MGC94+X2", 10));
            Assert.IsTrue(isBigger);
            isBigger = MiniTileComp.IsTileCodeBigger(new PlusCode("9F4MGC94+X2", 10), new PlusCode("9F4MGC94+2X", 10));
            Assert.IsFalse(isBigger);
        }




        [Test]
        public void SortList()
        {

            List<MiniTile> unsorted = new List<MiniTile>();
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWVC+W2", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWV9+XW", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWVC+X2", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWV9+WW", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWW9+2W", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWW9+2X", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWWC+22", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWV9+WX", 10), MiniTileType.Grass_Dirt, null));
            unsorted.Add(new MiniTile(new PlusCode("8FX9WWV9+XX", 10), MiniTileType.Grass_Dirt, null));
            unsorted = LocationCodeTileUtility.SortList(unsorted);

            List<MiniTile> sorted = new List<MiniTile>();
            sorted.Add(new MiniTile(new PlusCode("8FX9WWW9+2W", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWW9+2X", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWWC+22", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWV9+XW", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWV9+XX", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWVC+X2", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWV9+WW", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWV9+WX", 10), MiniTileType.Grass_Dirt, null));
            sorted.Add(new MiniTile(new PlusCode("8FX9WWVC+W2", 10), MiniTileType.Grass_Dirt, null));



            for (int i = 0; i < unsorted.Count; i++)
            {
                Debug.WriteLine(sorted[i].MiniTileId.Code + " " + unsorted[i].MiniTileId.Code);
                Assert.AreEqual(sorted[i].MiniTileId.Code, unsorted[i].MiniTileId.Code);
            }


        } 

        [Test]
        public void GetTileSectionsReturnsSameStringUnmodified()
        {
            var code = new PlusCode("8FX9XW2F+XX", 10);
            var strings = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
            Assert.IsTrue(strings.Contains(code.Code));

            code = new PlusCode("8FX9WWV9+", 8);
            strings = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
            Assert.IsTrue(strings.Contains(code.Code));

            code = new PlusCode("8FX9WW2F+", 8);
            strings = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
            Assert.IsTrue(strings.Contains(code.Code));
        }



        [Test]
        public void TestManhattenDistance()
        {

            int dist = PlusCodeUtils.GetManhattenDistance(new PlusCode("8FX9XW2F+XX", 10), new PlusCode("8FX9XW2G+X2", 10));
            Debug.WriteLine(dist);
            dist = PlusCodeUtils.GetManhattenDistance(new PlusCode("8FX9XW2F+22", 10), new PlusCode("8FX9XW2F+XX", 10));
            Debug.WriteLine(dist);
            dist = PlusCodeUtils.GetManhattenDistance(new PlusCode("8FX9XW2F+22", 10), new PlusCode("8FX9XW2F+2X", 10));
            Debug.WriteLine(dist);

        }


        //[Test]
        //takes 6 seconds
        /*  public void TestConcantWithReplaceOld()
          {
              List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList);
              List<MiniTile> miniTileList2 = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC84+", 8), miniTileTypeList);
              List<MiniTile> newList = TileUtility.ConcatWithReplaceOld(miniTileList, miniTileList2, new PlusCode("9F4MGC94+X2",10), 40);
              Debug.WriteLine(newList.Count);
          } */

     //   [Test]

     /*   public void TestRegenerateArea()
        {
            List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList, worldObjectTypeList);
            List<MiniTile> miniTileList2 = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList, worldObjectTypeList);
            List<MiniTile> newList = TileGenerator.RegenerateArea(new PlusCode("9F4MGC94+X2", 10), miniTileList, miniTileList2, 10);
            Debug.WriteLine(newList.Count);
            foreach (MiniTile m in newList)
            {
                Debug.WriteLine(m);
            }
        } */

        [Test]

        public void GeneratePlusCodeAreaCountTest()
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
        public void GeneratePlusCodeAreaProperNeighboursTest()
        {
            List<string> testCodeList = LocationCodeTileUtility.GetTileSection("8FX9WWV9+XX", 1, 10);

            List<string> testCodeList3 = LocationCodeTileUtility.GetTileSection("8FX9V9XX", 1, 8);

            List<string> realCodes = new List<String>();
            realCodes.Add("8FX9WWW9+2W");
            realCodes.Add("8FX9WWW9+2X");
            realCodes.Add("8FX9WWWC+22");
            realCodes.Add("8FX9WWV9+XW");
            realCodes.Add("8FX9WWV9+XX");
            realCodes.Add("8FX9WWVC+X2");
            realCodes.Add("8FX9WWV9+WW");
            realCodes.Add("8FX9WWV9+WX");
            realCodes.Add("8FX9WWVC+W2");

            int count = 0;
            foreach (string code in testCodeList)
            {
                Debug.WriteLine(code);
                if (realCodes.Contains(code))
                {
                    count++;
                }

            }

            Assert.AreEqual(9, count);

            List<string> realCodes2 = new List<String>();

            realCodes2.Add("8FXW92W");
            realCodes2.Add("8FXW92X");
            realCodes2.Add("8FXwC22");
            realCodes2.Add("8FXV9XW");
            realCodes2.Add("8FXV9XX");
            realCodes2.Add("8FXVCX2");
            realCodes2.Add("8FXV9WW");
            realCodes2.Add("8FXV9WX");
            realCodes2.Add("8FXVCW2");


            count = 0;
            foreach (string code in testCodeList3)
            {
                if (realCodes2.Contains(code))
                {
                    count++;
                }

            }


        }

        [Test]
        public void GetRandomMiniTileByTileCodeTest()
        {
            PlusCode code = TileUtility.GetRandomMiniTileByTileCode(new Tile(new PlusCode("8FXW92W", 8), TileType.Desert, null));
            Assert.IsTrue(code.Code.Contains("8FXW92W"));
            Assert.AreEqual("8FXW92W", code.Code.Substring(0, 7));
            Debug.WriteLine(code.Code);
            code = TileUtility.GetRandomMiniTileByTileCode(new Tile(new PlusCode("8FXW92W", 8), TileType.Desert, null));
            Assert.IsTrue(code.Code.Contains("8FXW92W"));
            Assert.AreEqual("8FXW92W", code.Code.Substring(0, 7));
            Debug.WriteLine(code.Code);
            code = TileUtility.GetRandomMiniTileByTileCode(new Tile(new PlusCode("8FXW92W", 8), TileType.Desert, null));
            Assert.IsTrue(code.Code.Contains("8FXW92W"));
            Assert.AreEqual("8FXW92W", code.Code.Substring(0, 7));
            Debug.WriteLine(code.Code);
        }
    }
}