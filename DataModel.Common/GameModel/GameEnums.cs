using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.GameModel
{

    public enum ContentType
    {
        RESOURCE,
        PLAYER,
        TOWNLEVEL1,
        TOWNLEVEL2,
        TOWNLEVEL3,
        QUESTLEVEL1,
        QUESTLEVEL2,
        QUESTLEVEL3,
        QUESTLEVEL4,
        QUESTLEVEL5,
        QUESTREWARDPOINTS
    }

    public enum ResourceType
    {
        NONE,
        APPLE,
        BANANA,
        WOOD,
        AMETHYST,
        BERRY,
        CARROT,
        CHICKEN,
        COAL,
        COCONUT,
        COFFEE,
        COPPER,
        DIAMOND,
        EGG,
        EMERALD,
        GOLD,
        GRAPE,
        IRON,
        LEATHER,
        MEAT,
        MILK,
        ORANGE,
        PELT,
        POTATO,
        PUMPKIN,
        RUBY,
        SALAD,
        SAND,
        SILVER,
        SPICE,
        STONE,
        TEA,
        TOMATO,
        WHEAT,
        WOOL,
    }
    public enum TileType
    {
        Grassland,
        Desert,
        Snow,
        Swamp,
        Jungle,
        Mountains,
        Savanna,
    }
    public enum MiniTileType
    {
        Grass_Tile,
        Grass_Dirt,
        Grass_Mushrooms,
        Grass_Flowers,
        Snow_Tile,
        Snow_Tile2,
        Snow_Tile3,
        Snow_PatchyGrass,
        Sand_Tile,
        Sand_Tile3,
        Sand_Stone,
        Sand_Tile2,
        Rock_Tile,
        Rock_Ice,
        Rock_Moss,
        WaterBody_Tile,
        Mud_Grass,
        Mud_Marsh,
        Mud_Moss,
        Mud_Tile,
        Jungle_Tile,
        Jungle_Roots,
        Savanna_Tile,
        Unknown_Tile

    }

    public enum WorldObjectType
    {
        Empty,
        Bush,
        Cactus,
        Cactus2,
        Camel,
        CopperRock,
        DesertTree,
        Glacier,
        GoldRock,
        OakTree,
        OvergrownRuin,
        PalmTree,
        Penguin,
        PineTree,
        Pond,
        Rock,
        RoseBush,
        SheepBrown,
        SheepBlack,
        SheepWhite,
        Skeleton,
        SmallPalmTree,
        SnowMan,
        SnowTree,
        Tree1,
        Tree2,
        Volcano,
        WaterFall,
        WheatField,
        WinterTree,
        WinterTree2,
        TreeRoots,
        TreeRoots2,
        SwampTree,
        Mangrove,
        Stork,
        Flamingo,
        TreeTrunk,
        FanTree,
        Zebra,
        CowBlack,
        CowBrown,
        CowWhite,
        AppleTree,
        OrangeTree,
        Pyramid,
        Bamboo,
        Flowers,
        DryDesertTree,
        CrocodilePit,
        SnowBells,
        CherryTree



    }
}
