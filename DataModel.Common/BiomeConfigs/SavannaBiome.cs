using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class SavannaBiome : IBiome
    {
        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"Bush", 15 },
         {"Cactus", 12 },
         {"Empty", 200 },
         {"Cactus2", 10 },
         {"Camel", 5 },
         {"CopperRock", 15 },
         {"DesertTree", 20 },
         {"GoldRock", 8 },
         {"PalmTree", 10 },
         {"Rock", 10 },
         {"Skeleton", 5 },
         {"SmallPalmTree", 10 },
         {"Tree1", 5 },
         {"Tree2", 5 },
         {"Volcano", 2 },
         {"WaterFall", 5 },
         {"WheatField", 5 },
         {"Stork", 5 },
         {"Zebra", 15 },
        





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Grass_Tile", 1 },
       


        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }
}

