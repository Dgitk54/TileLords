using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace DataModel.Common
{
    public class TileGenerator
    {
        /// <summary>
        /// Function which generates a list of MiniTiles 
        /// </summary>
        /// <param name="tileCode">The Pluscode of the parent Tile</param>
        /// <returns>The MiniTile list</returns>
        public static List<MiniTile> GenerateMiniTiles(PlusCode tileCode, List<int> miniTileTypeList)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(tileCode.Code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), getRandomMiniTileType(miniTileTypeList), new List<ITileContent>());

            return tiles.ToList();

        }


        /// <summary>
        /// Function which generates a Tile and the MiniTile children of it
        /// </summary>
        /// <param name="code">The Pluscode of the Tile</param>
        /// <returns>The Tile</returns>
        public static Tile GenerateTile(PlusCode code, List<int> tileTypeList, List<int> miniTileTypeList)
        {
            return new Tile(code, getRandomTileType(tileTypeList), GenerateMiniTiles(code, miniTileTypeList));
        }

        /// <summary>
        /// Function which generates an area consisting of Tiles and Minitiles 
        /// </summary>
        /// <param name="code">The Pluscode of the Tile to generate from</param>
        /// <param name="radius">The radius to generate the Tiles</param>
        /// <returns>The generated area as a list of Tiles</returns>
        public static List<Tile> GenerateArea(PlusCode code, int radius, List<int> tileTypeList, List<int> miniTileTypeList)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(code.Code, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), tileTypeList, miniTileTypeList);
            return tiles.ToList();
        }

        public static List<Tile> GenerateArea(PlusCode code, int radius)
        {
            List<int> tileTypeList = new List<int>() { 0,1,2,3,4,5,6,7};
            List<int> miniTileTypeList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(code.Code, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8), tileTypeList, miniTileTypeList);
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

    }
}
