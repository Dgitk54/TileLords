using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;

namespace DataModel.Common
{
    public static class TileUtils
    {


        /// <summary>
        /// Function which finds a Minitile inside a given tileList
        /// </summary>
        /// <param name="locationCode">PlusCode to search for.</param>
        /// <param name="tileList">A 2 dimensional map of the world represented in tiles.</param>
        /// <returns>The MiniTile searched for</returns>
        public static MiniTile GetMiniTile(PlusCode locationCode, List<List<Tile>> tileList)
        {
            var minitile =
              from tileRows in tileList
              from tile in tileRows
              from miniTileRows in tile.MiniTileListList
              from miniTile in miniTileRows
              where miniTile.Code.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }

        public static string SerializeTile(Tile tile)
        {
            return JsonConvert.SerializeObject(tile);
        }

        
        public static Tile DeserializeTile(string text)
        {
            return JsonConvert.DeserializeObject<Tile>(text);
        }







    }
}
