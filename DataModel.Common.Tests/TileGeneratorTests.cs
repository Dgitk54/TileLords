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

            List<int> tileTypeGen = new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7};
            List<int> miniTileTypeGen = new List<int>() { 0, 1, 2, 3, 3, 3, 3};
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
        public void CoordsFileTest()
        {
            //8FX9XW2F+XX
            List<Tile> tileList = TileGenerator.GenerateArea(new PlusCode("8FX9XW2F+XX", 8), 1);
            int tileCount = 0;
            foreach (Tile t in tileList)
           {
               TileUtility.ReadableMini2DArrayFile(TileUtility.GetMiniTile2DArray(t.MiniTiles, 20), @"C:\Users\Kat\Desktop\2DTileArray" + tileCount + ".txt");
                tileCount++;
            }
        
           List<MiniTile> tileSect = TileUtility.GetTileSectionWithinChebyshevDistance(new PlusCode("8FX9XW2F+XX", 10), tileList, 1);
           tileSect = LocationCodeTileUtility.SortList(tileSect);
           TileUtility.ReadableMini2DArrayFile(TileUtility.GetMiniTile2DArray(tileSect, 3), @"C:\Users\Kat\Desktop\2DTileArrayTileSect.txt");
            
        }


        [Test]
        public void SomeCoordsTest()
        {
           
            List<int> miniTileTypeGen = new List<int>() { 0, 1, 2, 3, 3, 3, 3 };
            List<MiniTile> tileList = TileGenerator.GenerateMiniTiles(new PlusCode("8FX9WWV9+", 8), miniTileTypeGen);

            MiniTile[,] miniTile2D = TileUtility.GetMiniTile2DArray(tileList, 20);
            Assert.AreEqual("8FX9WWV9+2X" , miniTile2D[19, 19].PlusCode.Code);
            Assert.AreEqual("8FX9WWV9+4R" , miniTile2D[17, 16].PlusCode.Code);
            Assert.AreEqual("8FX9WWV9+22", miniTile2D[19, 0].PlusCode.Code);
            Assert.AreEqual("8FX9WWV9+C5", miniTile2D[11, 3].PlusCode.Code);
            Assert.AreEqual("8FX9WWV9+P7", miniTile2D[5, 5].PlusCode.Code);


        }


        
    }
}
