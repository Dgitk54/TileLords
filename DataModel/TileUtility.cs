using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace DataModel.Common
{
    class TileUtility
    {

        



        public MiniTile GetMiniTile(PlusCode locationCode, List<List<Tile>> tileList)
        {
            var minitile =
              from tileRows in tileList
              from tile in tileRows
              from miniTileRows in tile.MiniTileListList
              from miniTile in miniTileRows
              where miniTile.code.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }


       







    }
}
