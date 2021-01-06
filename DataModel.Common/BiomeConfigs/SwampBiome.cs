using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class SwampBiome : IBiome
    {


        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"Bush", 5 },
         {"CopperRock", 12 },
         {"Empty", 170 },
         {"GoldRock", 10 },
         {"OvergrownRuin", 10 },
         {"Rock", 15 },
         {"Pond", 5 },
         {"RoseBush", 5 },
         {"SheepBrown", 5 },
         {"SheepBlack", 5 },
         {"SheepWhite", 5 },
         {"Skeleton", 5 },
         {"Volcano", 1 },
         {"WaterFall", 5 },
         {"WheatField", 4 },
         {"WinterTree", 5 },
         {"WinterTree2", 5 },
         {"TreeRoots", 15 },
         {"TreeRoots2", 15 },
         {"SwampTree", 30 },
         {"Mangrove", 30 },
         {"TreeTrunk", 15 },
         {"FanTree", 20},
         {"PalmTree", 10 },
         {"SmallPalmTree", 10 },
        





        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"Mud_Grass", 70 },
         {"Mud_Marsh", 20 },
         {"Grass_Tile", 20 },
         {"Mud_Tile", 20 },


        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }

}
