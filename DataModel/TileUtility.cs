﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DataModel.Common
{
    public class TileUtility
    {


        public List<MiniTile> getRenderTiles(PlusCode locationCode, List<List<Tile>> tileList, int precision)
        {
            List<MiniTile> miniTileList = new List<MiniTile>();
            var minitile =
             from tileRows in tileList
             from tile in tileRows
             from miniTile in tile.MiniTiles
             where PlusCodeUtils.GetManhattenDistance(miniTile.Code, locationCode) < precision
             select miniTile;

            return minitile.ToList();


        }


      

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


        public static void ReadableMini2DArrayFile(MiniTile[,] miniTileArray, String fileLocation)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fileLocation))
                for (int i = 0; i < miniTileArray.GetLength(0); i++)
                {

                    file.WriteLine("");
                    file.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------" +
                        "----------------------------------------------------------------------------------------------------------------------------------------------");
                    for (int j = 0; j < miniTileArray.GetLength(1); j++)
                    {

                        file.Write(miniTileArray[i, j].Code.Code + " | ");

                    }
                }

        }

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
              from miniTile in tile.MiniTiles
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
