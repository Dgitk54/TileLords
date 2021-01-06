using DataModel.Common;
using DataModel.Common.WorldObjectTypes;
using System;
using System.Collections.Generic;
using System.Text;



namespace DataModel.Common.BiomeConfigs 
{
    public class GrasslandBiome : IBiome
    {

        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {

         {"AppleTree", 5 },
         {"Bush", 12 },
         {"Empty", 200 },
         {"CopperRock", 10 },
         {"CowBlack", 15 },
         {"CowBrown", 15 },
         {"CowWhite", 15 },
         {"DesertTree", 15 },
         {"GoldRock", 8 },
         {"OakTree", 20 },
         {"OrangeTree", 10 },
         {"OvergrownRuin", 5 },
         {"PineTree", 15 },
         {"Pond", 5 },
         {"Rock", 20 },
         {"RoseBush", 5 },
         {"SheepBlack", 15 },
         {"SheepBrown", 15 },
         {"SheepWhite", 15 },
         {"Stork", 10 },
         {"Tree1", 15 },
         {"Tree2", 15 },
         {"TreeTrunk", 5 },
         {"Volcano", 1 },
         {"WaterFall", 5 },
         {"WheatField", 10 },


         


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
