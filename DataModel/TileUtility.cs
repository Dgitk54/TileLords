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



        public List<MiniTile> GetTileSection(PlusCode locationCode,  List<Tile> tileList, int precision)
        {
            List<MiniTile> miniTileList = new List<MiniTile>();
            var minitile =
             from tile in tileList
             from miniTile in tile.MiniTiles
             where PlusCodeUtils.GetManhattenDistance(miniTile.Code, locationCode) < precision
             select miniTile;

            return minitile.ToList();


        } 

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


        /// <summary>
        /// Function which creates a 2d array to represent the Tiles
        /// </summary>
        /// <param name="miniTiles">The list of Tikes for the 2d array.</param>
        /// <param name="squareSize">The size of the 2d array.</param>
        /// <returns>The Tile 2D array</returns>

        public static Tile[,] GetTile2DArray(List<Tile> tile, int squareSize)
        {

            Tile[,] tile2DArray = new Tile[squareSize, squareSize];
            int tileCounter = 0;
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    tile2DArray[i, j] = tile[tileCounter];
                    tileCounter++;
                }
            }
            return tile2DArray;

        }


        /// <summary>
        /// Function which creates a textfile with a representation of the miniTile 2D array
        /// </summary>
        /// <param name="miniTileArray">The miniTile 2D array to be printed on the textfile.</param>
        /// <param name="fileLocation">The Location where the textfile will be stored.</param>

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

                        file.Write(miniTileArray[i, j].Code.Code + " (" + miniTileArray[i, j].TileType + ") | ");

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