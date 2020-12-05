using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DataModel.Common
{
    public static class TileUtility
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

        public static bool EqualsBasedOnPlusCode(this MiniTile t1, MiniTile t2) => t1.MiniTileId.Code.Equals(t2.MiniTileId.Code);

        
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


        public static MiniTile GetMiniTile(PlusCode locationCode, List<Tile> tileList)
        {
            var minitile =
              from tile in tileList
              from miniTile in tile.MiniTiles
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }

        public static MiniTile GetMiniTile(this PlusCode locationCode, IList<MiniTile> miniTileList)
        {
            var minitile =
              from miniTile in miniTileList
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            if (minitile.Count() == 0)
                return null;
            return minitile.First();
        }



    }
}
