using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.BiomeConfigs
{
    public interface IBiome
    {

        Dictionary<string, int> WorldObjectWeightDict { get; }
        Dictionary<string, int> TileTypeWeightDict { get; }
    }
}

