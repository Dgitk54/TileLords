using DataModel.Common;
using DataModel.Common.Messages;
using System.Collections.Generic;

namespace DataModel.Server.Model
{
    public static class WorldSpawnWeights
    {

        public static Dictionary<ResourceType, int> SwampSpawnValues = new Dictionary<ResourceType, int>
        {
            //example: does not spawn appels, coconuts, grapes, milk, orange, potato, salad

           
            //common very available resources (10+)
            {ResourceType.WOOD, 30 },
            {ResourceType.PUMPKIN, 20 },
            {ResourceType.BERRY, 20 },
            {ResourceType.CARROT, 15 },
            {ResourceType.EGG, 15 },
            {ResourceType.MEAT, 15 },
            {ResourceType.STONE, 30 },
            {ResourceType.SAND, 12 },
          //{ResourceType.POTATO, 10 },
           



            //medium rare resources (from 4-10)
           //{ResourceType.APPLE, 4 },
            {ResourceType.WOOL, 4 },
            {ResourceType.TEA, 5 },
            {ResourceType.COPPER, 5 },
            {ResourceType.SILVER, 4 },
            {ResourceType.COAL, 8 },
            {ResourceType.WHEAT, 5 },
            {ResourceType.CHICKEN, 9 },
            {ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 10 },
          //{ResourceType.COCONUT, 4 },
          //{ResourceType.GRAPE, 4 },
          //{ResourceType.MILK, 4 },
          //{ResourceType.ORANGE, 4 },
          //{ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            {ResourceType.COFFEE, 3 },
            {ResourceType.PELT, 2 },
            {ResourceType.SPICE, 2 },
            {ResourceType.AMETHYST, 2 },
            {ResourceType.EMERALD, 2 },
            {ResourceType.RUBY, 2 },
            {ResourceType.GOLD, 1 },
            {ResourceType.DIAMOND, 1 },



        };



        public static Dictionary<ResourceType, int> DesertSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 10 },
            //{ResourceType.PUMPKIN, 20 },
            {ResourceType.BERRY, 10 },
            //{ResourceType.CARROT, 15 },
            {ResourceType.EGG, 10 },
            {ResourceType.MEAT, 10 },
            {ResourceType.STONE, 30 },
            {ResourceType.SAND, 30 },
          //{ResourceType.POTATO, 10 },
           



            //medium rare resources (from 4-10)
           //{ResourceType.APPLE, 4 },
            {ResourceType.WOOL, 4 },
            //{ResourceType.TEA, 5 },
            {ResourceType.COPPER, 10 },
            {ResourceType.SILVER, 10 },
            {ResourceType.COAL, 10 },
            {ResourceType.WHEAT, 4 },
            {ResourceType.CHICKEN, 4 },
            {ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 5 },
          //{ResourceType.COCONUT, 4 },
          //{ResourceType.GRAPE, 4 },
            {ResourceType.MILK, 5 },
          //{ResourceType.ORANGE, 4 },
          //{ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            //{ResourceType.COFFEE, 3 },
            {ResourceType.PELT, 2 },
            //{ResourceType.SPICE, 2 },
            {ResourceType.AMETHYST, 3 },
            {ResourceType.EMERALD, 3 },
            {ResourceType.RUBY, 3 },
            {ResourceType.GOLD, 2 },
            {ResourceType.DIAMOND, 2 },



        };



        public static Dictionary<ResourceType, int> MountainSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 15 },
            {ResourceType.PUMPKIN, 10 },
            {ResourceType.BERRY, 10 },
            {ResourceType.CARROT, 10 },
            {ResourceType.EGG, 10 },
            {ResourceType.MEAT, 10 },
            {ResourceType.STONE, 30 },
          //{ResourceType.SAND, 10 },
            {ResourceType.POTATO, 10 },
           



            //medium rare resources (from 4-10)
            {ResourceType.APPLE, 4 },
            {ResourceType.WOOL, 10 },
            //{ResourceType.TEA, 5 },
            {ResourceType.COPPER, 10 },
            {ResourceType.SILVER, 10 },
            {ResourceType.COAL, 10 },
            {ResourceType.WHEAT, 4 },
            {ResourceType.CHICKEN, 4 },
          //{ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 7 },
          //{ResourceType.COCONUT, 4 },
          //{ResourceType.GRAPE, 4 },
            {ResourceType.MILK, 10 },
          //{ResourceType.ORANGE, 4 },
            {ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            //{ResourceType.COFFEE, 3 },
            {ResourceType.PELT, 3 },
            //{ResourceType.SPICE, 2 },
            {ResourceType.AMETHYST, 3 },
            {ResourceType.EMERALD, 3 },
            {ResourceType.RUBY, 3 },
            {ResourceType.GOLD, 2 },
            {ResourceType.DIAMOND, 2 },



        };


        public static Dictionary<ResourceType, int> GrassSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 20 },
            {ResourceType.PUMPKIN, 10 },
            {ResourceType.BERRY, 20 },
            {ResourceType.CARROT, 20 },
            {ResourceType.EGG, 25 },
            {ResourceType.MEAT, 25 },
            {ResourceType.STONE, 15 },
            {ResourceType.SAND, 10 },
            {ResourceType.POTATO, 20 },
           



            //medium rare resources (from 4-10)
            {ResourceType.APPLE, 10 },
            {ResourceType.WOOL, 7 },
            //{ResourceType.TEA, 5 },
            {ResourceType.COPPER, 5 },
            {ResourceType.SILVER, 5 },
            {ResourceType.COAL, 5 },
            {ResourceType.WHEAT, 10 },
            {ResourceType.CHICKEN, 10 },
          //{ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 8 },
          //{ResourceType.COCONUT, 4 },
            {ResourceType.GRAPE, 10 },
            {ResourceType.MILK, 8 },
            {ResourceType.ORANGE, 6 },
            {ResourceType.SALAD, 10 },
           

            //rare higher tier resources (from 1-3)
          
            //{ResourceType.COFFEE, 3 },
            {ResourceType.PELT, 3 },
            {ResourceType.SPICE, 1 },
            {ResourceType.AMETHYST, 1 },
            {ResourceType.EMERALD, 1 },
            {ResourceType.RUBY, 1 },
            {ResourceType.GOLD, 1 },
            {ResourceType.DIAMOND, 1 },



        };


        public static Dictionary<ResourceType, int> SnowSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 20 },
            {ResourceType.PUMPKIN, 10 },
            {ResourceType.BERRY, 10 },
           //{ResourceType.CARROT, 20 },
            {ResourceType.EGG, 10 },
            {ResourceType.MEAT, 15 },
            {ResourceType.STONE, 15 },
            //{ResourceType.SAND, 10 },
            {ResourceType.POTATO, 10 },
           



            //medium rare resources (from 4-10)
            //{ResourceType.APPLE, 10 },
            {ResourceType.WOOL, 10 },
            //{ResourceType.TEA, 5 },
            {ResourceType.COPPER, 5 },
            {ResourceType.SILVER, 5 },
            {ResourceType.COAL, 5 },
            {ResourceType.WHEAT, 4 },
            {ResourceType.CHICKEN, 5 },
          //{ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 5 },
          //{ResourceType.COCONUT, 4 },
            //{ResourceType.GRAPE, 10 },
            {ResourceType.MILK, 4 },
           // {ResourceType.ORANGE, 6 },
            {ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            //{ResourceType.COFFEE, 3 },
            {ResourceType.PELT, 3 },
            //{ResourceType.SPICE, 1 },
            {ResourceType.AMETHYST, 1 },
            {ResourceType.EMERALD, 1 },
            {ResourceType.RUBY, 1 },
            {ResourceType.GOLD, 1 },
            {ResourceType.DIAMOND, 1 },



        };

        public static Dictionary<ResourceType, int> JungleSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 30 },
           // {ResourceType.PUMPKIN, 15 },
            {ResourceType.BERRY, 30 },
           //{ResourceType.CARROT, 20 },
            {ResourceType.EGG, 20 },
            {ResourceType.MEAT, 30 },
            {ResourceType.STONE, 20 },
            {ResourceType.SAND, 10 },
            //{ResourceType.POTATO, 20 },
           



            //medium rare resources (from 4-10)
            {ResourceType.APPLE, 10 },
            {ResourceType.WOOL, 4 },
            {ResourceType.TEA, 6 },
            {ResourceType.COPPER, 4 },
            {ResourceType.SILVER, 4 },
            {ResourceType.COAL, 4 },
            {ResourceType.WHEAT, 8 },
            {ResourceType.CHICKEN, 8 },
            {ResourceType.BANANA, 10 },
            {ResourceType.LEATHER, 5 },
            {ResourceType.COCONUT, 10 },
            //{ResourceType.GRAPE, 10 },
            {ResourceType.MILK, 4 },
            {ResourceType.ORANGE, 10 },
           //{ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            {ResourceType.COFFEE, 3 },
            //{ResourceType.PELT, 3 },
            {ResourceType.SPICE, 3 },
            {ResourceType.AMETHYST, 1 },
            {ResourceType.EMERALD, 1 },
            {ResourceType.RUBY, 1 },
            //{ResourceType.GOLD, 1 },
            //{ResourceType.DIAMOND, 1 },



        };



        public static Dictionary<ResourceType, int> SavannaSpawnValues = new Dictionary<ResourceType, int>
        {
           

              
            //common very available resources (10+)
            {ResourceType.WOOD, 15 },
            //{ResourceType.PUMPKIN, 15 },
            {ResourceType.BERRY, 10 },
           //{ResourceType.CARROT, 20 },
            {ResourceType.EGG, 10 },
            {ResourceType.MEAT, 25 },
            {ResourceType.STONE, 10 },
            {ResourceType.SAND, 10 },
            //{ResourceType.POTATO, 20 },
           



            //medium rare resources (from 4-10)
            {ResourceType.APPLE, 10 },
            {ResourceType.WOOL, 4 },
            {ResourceType.TEA, 4 },
            {ResourceType.COPPER, 4 },
            {ResourceType.SILVER, 4 },
            {ResourceType.COAL, 4 },
            {ResourceType.WHEAT, 8 },
            {ResourceType.CHICKEN, 8 },
            {ResourceType.BANANA, 4 },
            {ResourceType.LEATHER, 10 },
            {ResourceType.COCONUT, 4 },
            {ResourceType.GRAPE, 4 },
            {ResourceType.MILK, 4 },
            {ResourceType.ORANGE, 4 },
           {ResourceType.SALAD, 4 },
           

            //rare higher tier resources (from 1-3)
          
            {ResourceType.COFFEE, 1 },
            //{ResourceType.PELT, 3 },
            {ResourceType.SPICE, 1 },
            {ResourceType.AMETHYST, 2 },
            {ResourceType.EMERALD, 2 },
            {ResourceType.RUBY, 2 },
            {ResourceType.GOLD, 1 },
            {ResourceType.DIAMOND, 1 },



        };


        public static Dictionary<TileType, Dictionary<ResourceType, int>> TileTypeToResourceTypeWeights = new Dictionary<TileType, Dictionary<ResourceType, int>>
        {
            {TileType.Swamp, SwampSpawnValues },
            {TileType.Desert, DesertSpawnValues },
            {TileType.Grassland, GrassSpawnValues },
            {TileType.Jungle, JungleSpawnValues },
            {TileType.Mountains, MountainSpawnValues },
            {TileType.Savanna, SavannaSpawnValues },
            {TileType.Snow, SnowSpawnValues },
        };
    }
}
