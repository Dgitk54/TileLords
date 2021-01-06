using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public class WaterBodyBiome : IBiome
    {
        public Dictionary<string, int> worldObjectWeight = new Dictionary<string, int>
        {
     
         {"Empty", 1 }
         


        };

        public Dictionary<string, int> tileTypeWeight = new Dictionary<string, int>
        {

         {"WaterBody_Tile", 1 },
        


        };

        Dictionary<string, int> IBiome.WorldObjectWeightDict { get => worldObjectWeight; }
        Dictionary<string, int> IBiome.TileTypeWeightDict { get => tileTypeWeight; }
    }

}
