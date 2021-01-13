using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class SnowBiome : IBiome
    {
        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"CopperRock", 10 },
         {"Bush", 5 },
         {"Empty", 200 },
         {"Glacier", 20 },
         {"Penguin", 15 },
         {"PineTree", 30 },
         {"Pond", 5 },
         {"Rock", 15 },
         {"SnowMan", 15 },
         {"SnowTree", 25 },
         {"WaterFall", 5 },
         {"WheatField", 3 },
         {"WinterTree", 15 },
         {"WinterTree2", 15 },
         {"TreeTrunk", 20 },
         {"CowBrown", 5 },
         {"SheepBlack", 5 },
         {"SheepBrown", 5 },
         {"SheepWhite", 5 },
         {"CowBlack", 5 },
         {"CowWhite", 5 },
       





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Snow_Tile", 10 },
         {"Snow_Tile2", 2 },
         {"Snow_Tile3", 2 },
         {"Snow_PatchyGrass", 7 },


        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }
}

