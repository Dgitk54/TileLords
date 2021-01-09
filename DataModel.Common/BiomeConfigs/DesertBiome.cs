using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
   public class DesertBiome : IBiome
    {
          

        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"Cactus", 30 },
         {"Cactus2", 30 },
         {"Empty", 200 },
         {"Camel", 15 },
         {"CopperRock", 20 },
         {"GoldRock", 20 },
         {"PalmTree", 5 },
         {"Rock", 20 },
         {"Skeleton", 8 },
         {"SmallPalmTree", 5 },
         {"Tree1", 5 },
         {"Tree2", 5 },
         {"Volcano", 2 },
         {"WheatField", 5 },
         {"FanTree", 4 },
         {"Pyramid", 5 },
        





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Sand_Tile", 30 },
         {"Sand_Tile2", 30 },
         {"Sand_River", 5 },
         {"Sand_Stone", 15 },



        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }
}

