using DataModel.Common.GameModel;
using System.Collections.Generic;

namespace DataModel.Common
{
    public static class WorldWeights
    {
        public static Dictionary<string, int> desertObjectWeights = new Dictionary<string, int>
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
         {"DryDesertTree", 15 },
         {"Bamboo", 3 },
        };

        public static Dictionary<string, int> desertTypeWeight = new Dictionary<string, int>
        {
         {"Sand_Tile", 2 },
         {"Sand_Tile2", 2 },
         {"Sand_Tile3", 2 },
         {"Sand_Stone", 1 },
        };

        public static Dictionary<string, int> grassObjectWeight = new Dictionary<string, int>
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
         {"Flowers", 10 },
         {"CherryTree", 5 },
        };

        public static Dictionary<string, int> grassTypeWeight = new Dictionary<string, int>
        {
         {"Grass_Tile", 7 },
         {"Grass_Dirt", 2 },
         {"Grass_Mushrooms", 2 },
         {"Grass_Flowers", 2 },
        };

        public static Dictionary<string, int> jungleObjectWeight = new Dictionary<string, int>
        {
         {"Bush", 5 },
         {"CopperRock", 12 },
         {"Empty", 200 },
         {"DesertTree", 10 },
         {"GoldRock", 15 },
         {"OakTree", 15 },
         {"OvergrownRuin", 3 },
         {"PalmTree", 40 },
         {"Pond", 8 },
         {"Rock", 20 },
         {"RoseBush", 10 },
         {"SmallPalmTree", 15 },
         {"Tree1", 20 },
         {"Tree2", 20 },
         {"Volcano", 3 },
         {"WaterFall", 5 },
         {"WheatField", 15 },
         {"Flamingo", 15 },
         {"TreeTrunk", 15 },
         {"FanTree", 10 },
         {"CowWhite", 15 },
         {"AppleTree", 15 },
         {"OrangeTree", 5 },
         {"Flowers", 10 },
         {"Bamboo", 15 },
         {"CrocodilePit", 5 },
        };

        public static Dictionary<string, int> jungleTypeWeight = new Dictionary<string, int>
        {
         {"Jungle_Tile", 1 },
         {"Jungle_Roots", 1 },
        };

        public static Dictionary<string, int> mountainsObjectWeight = new Dictionary<string, int>
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
         {"SnowBells", 10 },
         {"Flowers", 3 },
        };

        public static Dictionary<string, int> mountainsTypeWeight = new Dictionary<string, int>
        {
         {"Rock_Tile", 7 },
         {"Rock_Ice", 2 },
         {"Rock_Moss", 2 },
        };
        public static Dictionary<string, int> savannaObjectWeight = new Dictionary<string, int>
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
         {"CrocodilePit", 5 },
         {"DryDesertTree", 10 },
        };

        public static Dictionary<string, int> savannaTypeWeight = new Dictionary<string, int>
        {
         {"Grass_Tile", 1 },
         {"Savanna_Tile", 2 },
        };

        public static Dictionary<string, int> snowObjectWeight = new Dictionary<string, int>
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
         {"SnowBells", 8 },
        };

        public static Dictionary<string, int> snowTypeWeight = new Dictionary<string, int>
        {
         {"Snow_Tile", 10 },
         {"Snow_Tile2", 2 },
         {"Snow_Tile3", 2 },
         {"Snow_PatchyGrass", 7 }
        };
        public static Dictionary<string, int> swampObjectWeight = new Dictionary<string, int>
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
         {"CrocodilePit", 5 },
         {"Flowers", 10 },
         {"CherryTree", 10 },
         {"Bamboo", 10 }
        };

        public static Dictionary<string, int> swampTypeWeight = new Dictionary<string, int>
        {
         {"Mud_Grass", 7 },
         {"Mud_Marsh", 2 },
         {"Grass_Dirt", 2 },
         {"Mud_Tile", 2 },
         {"Mud_Moss", 2 },
        };

        public static Dictionary<TileType, (Dictionary<string, int>, Dictionary<string, int>)> biomes = new Dictionary<TileType, (Dictionary<string, int>, Dictionary<string, int>)>
        {
            { TileType.Desert, (desertObjectWeights, desertTypeWeight) },
            { TileType.Grassland, (grassObjectWeight, grassTypeWeight) },
            { TileType.Jungle, (jungleObjectWeight, jungleTypeWeight)},
            { TileType.Mountains, (mountainsObjectWeight , mountainsTypeWeight) },
            { TileType.Savanna, (savannaObjectWeight , savannaTypeWeight )},
            { TileType.Snow, (snowObjectWeight , snowTypeWeight )},
            { TileType.Swamp, (swampObjectWeight , swampTypeWeight)}
        };
    }
}
