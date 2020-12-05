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

        [SetUp]
        public void SetUp()
        {
            tileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            miniTileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
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

        [Test]
        public void GivenEdgeTileChebyshevAlsoFindsNeighborsInOtherTiles()
        {
            var startCode = new PlusCode("8FX9XW2F+XX", 10);
            var tiles = TileGenerator.GenerateArea(startCode, 1);
            var miniTiles = TileUtility.GetTileSectionWithinChebyshevDistance(startCode, tiles, 1);
            var startTile = DataModelFunctions.ToLowerResolution(startCode, 8);

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

            List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList);
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



        //TODO WRITE THE TEST PROPERLY!!!!!!!!!
        [Test]
        public void SortList()
        {



            List<MiniTile> miniTileList = new List<MiniTile>();
            miniTileList.Add(new MiniTile(new PlusCode("8F4MGC94+X2", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("9F4MGC94+X6", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F7MGC94+W2", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F4MGC94+H2", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F6MGC94+V2", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("3F4MGC94+22", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F4MGC94+Q2", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F4MGC97+X5", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("824MGC54+32", 10), MiniTileType.Grass_Tile, null));
            miniTileList.Add(new MiniTile(new PlusCode("8F4MGC94+2X", 10), MiniTileType.Grass_Tile, null));
            List<MiniTile> sortedList = LocationCodeTileUtility.SortList(miniTileList);
            foreach (MiniTile t in sortedList)
            {
                Debug.WriteLine(t.MiniTileId.Code);
            }

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

        [Test]

        public void TestRegenerateArea()
        {
            List<MiniTile> miniTileList = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList);
            List<MiniTile> miniTileList2 = TileGenerator.GenerateMiniTiles(new PlusCode("9F4MGC94+", 8), miniTileTypeList);
            List<MiniTile> newList = TileGenerator.RegenerateArea(new PlusCode("9F4MGC94+X2", 10), miniTileList, miniTileList2, 10);
            Debug.WriteLine(newList.Count);
            foreach (MiniTile m in newList)
            {
                Debug.WriteLine(m);
            }
        }
    }
}