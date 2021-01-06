using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class JungleBiome : IBiome
    {
          

        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"Bush", 5 },
         {"CopperRock", 12 },
         {"Empty", 200 },
         {"DesertTree", 10 },
         {"GoldRock", 15 },
         {"OakTree", 15 },
         {"OvergrownRuin", 15 },
         {"PalmTree", 15 },
         {"Pond", 8 },
         {"Rock", 20 },
         {"RoseBush", 10 },
         {"SmallPalmTree", 5 },
         {"Tree1", 15 },
         {"Tree2", 5 },
         {"Volcano", 20 },
         {"WaterFall", 5 },
         {"WheatField", 15 },
         {"Flamingo", 15 },
         {"TreeTrunk", 15 },
         {"FanTree", 10 },
         {"CowWhite", 15 },
         {"AppleTree", 15 },
         {"OrangeTree", 5 },
    





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Grass_Tile", 70 },
         {"Grass_River", 20 },
         {"Grass_River2", 20 },


        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }
}

