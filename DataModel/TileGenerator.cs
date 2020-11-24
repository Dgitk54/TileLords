using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace DataModel.Common
{
    public class TileGenerator
    {

        public static List<MiniTile> GenerateMiniTiles (PlusCode tileCode)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(tileCode.Code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), MiniTileType.Desert, new List<ITileContent>());

            return tiles.ToList();   

        }

        public static Tile FromPlusCode(PlusCode code)
        {
            return new Tile(code, TileType.Grassland, GenerateMiniTiles(code));
        }
        public static List<Tile> GenerateArea(PlusCode code, int radius)
        {
            var tiles = from miniTileCodeString in LocationCodeTileUtility.FindRenderRange(code.Code, radius, 8)
                        select FromPlusCode(new PlusCode(miniTileCodeString, 8));
            return tiles.ToList();
        }


        /*
        public static MiniTile[,] GenerateMiniTiles(string tileCode)
        {
            
            List<MiniTile> miniTileList = new List<MiniTile>();
            int miniTileListCounter = 0;
           

            //create miniTiles
            List<String> miniTileCodes = LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(tileCode);
            for (int j = 0; j < miniTileCodes.Count; j++)
            {
                List<ITileContent> tileContent = new List<ITileContent>();
                MiniTile miniTile = new MiniTile(new PlusCode(miniTileCodes[j], 10), MiniTileType.Desert, tileContent);
                miniTileList.Add(miniTile);

            }


            MiniTile[,] miniTileDArray = new MiniTile[20, 20];
            //create miniTileListList
            for (int k = 0; k < 20; k++)
            {
                for (int l = 0; l < 20; l++)
                {
                    miniTileDArray[k,l] = miniTileList[miniTileListCounter];
                   // Debug.WriteLine(miniTileList[miniTileListCounter].code.Code);
                    miniTileListCounter++;
                 
                }
            }

            return miniTileDArray;

        } */



     //  public Tile[,] GenerateTiles(PlusCode code)
     //  {
     //      String c = code.Code;
     //      c = c.Substring(0, 8);
     //
     //      List<String> tileChunk = LocationCodeTileUtility.FindRenderRange(c, 1, 8);
     //
     //      Tile[,] tileDArray = new Tile[3, 3];
     //      int tileChunkCounter = 0;
     //
     //
     //      for (int i = 0; i < 3; i++)
     //      {
     //
     //          for (int j = 0; j < 3; j++)
     //          {
     //
     //              Tile tile = new Tile(new PlusCode(tileChunk[tileChunkCounter], 8), TileType.Grassland, GenerateMiniTiles(tileChunk[tileChunkCounter]));
     //              tileDArray[i, j] = tile;
     //              tileChunkCounter++;
     //          }
     //      }
     //
     //      return tileDArray;
     //
     //
     //  }
    }
}
