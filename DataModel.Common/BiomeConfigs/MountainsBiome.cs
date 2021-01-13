using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class MountainsBiome : IBiome
    {
        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"Bush", 5 },
         {"CopperRock", 20 },
         {"Empty", 200 },
         {"Glacier", 15 },
         {"GoldRock", 20 },
         {"OvergrownRuin", 4 },
         {"PineTree", 5 },
         {"Pond", 3 },
         {"Rock", 30 },
         {"RoseBush", 8 },
         {"SheepBrown", 5 },
         {"SheepWhite", 5 },
         {"SheepBlack", 5 },
         {"Tree1", 5 },
         {"Tree2", 5 },
         {"Volcano", 5 },
         {"WaterFall", 10 },
         {"WheatField", 2 },
         {"WinterTree", 15 },
         {"WinterTree2", 15 },
         {"TreeTrunk", 10 },
     





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Rock_Tile", 7 },
         {"Rock_Ice", 2 },
         {"Rock_Moss", 2 },
        

        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }
}

