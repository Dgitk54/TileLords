﻿using DataModel.Common.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataModel.Common
{
    public static class WorldGenerator
    {
        public static Tile GenerateTile(PlusCode code)
        {
            var md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(code.Code));
            var asInt = BitConverter.ToInt32(hashed, 0); // more collisions!
            var rnd = new Random(asInt);
            var type = getRandomTileType(rnd);

            var biomevalues = WorldWeights.biomes[type];
            var list = MiniTilesForTile(code, biomevalues.Item1, biomevalues.Item2, rnd, type);
            return new Tile(code, type, list);
        }
        public static TileType GetTileTypeForArea(PlusCode code, string addition = "")
        {
            var lowerPrecision = code;
            if (code.Precision == 10)
                lowerPrecision = code.ToLowerResolution(8);

            var md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(lowerPrecision.Code));
            var asInt = BitConverter.ToInt32(hashed, 0); // more collisions!
            var rnd = new Random(asInt);
            return getRandomTileType(rnd);
        }
        public static Tile GenerateTile(PlusCode code, string addition)
        {
            string newCode = code.Code + addition;

            var md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(newCode));
            var asInt = BitConverter.ToInt32(hashed, 0); // more collisions!
            var rnd = new Random(asInt);
            var type = getRandomTileType(rnd);

            var biomevalues = WorldWeights.biomes[type];
            var list = MiniTilesForTile(code, biomevalues.Item1, biomevalues.Item2, rnd, type);
            return new Tile(code, type, list);
        }

        public static List<MiniTile> MiniTilesForTile(PlusCode tileCode, Dictionary<string, int> worldObjects, Dictionary<string, int> floorValues, Random seed, TileType parentsType)
        {
            string code = tileCode.Code;
            if (code.Length > 9)
            {
                code = code.Substring(0, 9);
            }
            if (!code.Contains("+"))
            {
                code += "+";
            }
            var tiles = from miniTileCodeString in LocationCodeTileUtility.GetAndCombineWithAllAfterPlus(code)
                        select new MiniTile(new PlusCode(miniTileCodeString, 10), floorValues.RandomElementByWeight(e => e.Value, seed).Key.ConvertEnumString<MiniTileType>(), new List<ITileContent>() { new WorldObject(worldObjects.RandomElementByWeight(e => e.Value, seed).Key.ConvertEnumString<WorldObjectType>()) }, parentsType);
            return tiles.ToList();
        }

        public static TileType getRandomTileType(Random r)
        {
            int length = Enum.GetNames(typeof(TileType)).Length;
            int i = r.Next(0, length);
            return (TileType)i;
        }

    }
}
