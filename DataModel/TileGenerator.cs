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
        public static List<MiniTile> GenerateMiniTiles(PlusCode tileCode)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(tileCode.Code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), MiniTileType.Desert, new List<ITileContent>());

            return tiles.ToList();

        }


        /// <summary>
        /// Function which generates a Tile 
        /// </summary>
        /// <param name="code">The Pluscode of the Tile</param>
        /// <returns>The Tile</returns>
        public static Tile GenerateTile(PlusCode code)
        {
            return new Tile(code, TileType.Grassland, GenerateMiniTiles(code));
        }

        /// <summary>
        /// Function which generates an area consisting of Tiles and Minitiles 
        /// </summary>
        /// <param name="code">The Pluscode of the Tile to generate from</param>
        /// <param name="radius">The radius to generate the Tiles</param>
        /// <returns>The generated area as a list of Tiles</returns>
        public static List<Tile> GenerateArea(PlusCode code, int radius)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetTileSection(code.Code, radius, 8)
                        select GenerateTile(new PlusCode(miniTileCodeString, 8));
            return tiles.ToList();
        }



    }
}
