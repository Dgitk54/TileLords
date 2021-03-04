using DataModel.Common;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public static class WorldSpawnWeights
    {

        public static Dictionary<ResourceType, int> SwampSpawnValues = new Dictionary<ResourceType, int> 
        {
            {ResourceType.WOOD, 10 },
            {ResourceType.PUMPKIN, 5 },
           
        };

        public static Dictionary<TileType, Dictionary<ResourceType, int>> TileTypeToResourceTypeWeights = new Dictionary<TileType, Dictionary<ResourceType, int>>
        {
            {TileType.Swamp, SwampSpawnValues }
        };
    }
}
