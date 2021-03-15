using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataModel.Common
{
    public static class TileUtility
    {


        /// <summary>
        /// Given the List of possible tiles, this code returns all tiles within a chebyshev distance.
        /// </summary>
        /// <param name="locationCode">The center tile</param>
        /// <param name="tileList">The surrounding tiles</param>
        /// <param name="distance">The distance from the center tile</param>
        /// <returns>A list with tiles within the chebyshev distance</returns>
        public static List<MiniTile> GetTileSectionWithinChebyshevDistance(PlusCode locationCode, List<Tile> tileList, int distance)
        {
            var minitile =
             from tile in tileList
             from miniTile in tile.MiniTiles
             where PlusCodeUtils.GetChebyshevDistance(locationCode, miniTile.MiniTileId) <= distance
             select miniTile;

            return minitile.ToList();
        }

        /// <summary>
        /// Given the List of possible miniTiles, this code returns all miniTiles within a chebyshev distance.
        /// </summary>
        /// <param name="locationCode">The center miniTile</param>
        /// <param name="tileList">The surrounding miniTiles</param>
        /// <param name="distance">The distance from the center miniTile</param>
        /// <returns>A list with miniTiles within the chebyshev distance</returns>
        public static List<MiniTile> GetMiniTileSectionWithinChebyshevDistance(PlusCode locationCode, IList<MiniTile> tileList, int distance)
        {
            var minitile = from miniTile in tileList
                           where PlusCodeUtils.GetChebyshevDistance(locationCode, miniTile.MiniTileId) <= distance
                           select miniTile;
            return minitile.ToList();
        }


        public static string TileDebugContentString(this Tile t)
        {

            int contentCount = 0;
            string tileContentString = "";
            WorldObject wo;
            t.MiniTiles.ForEach(v =>
            {
                contentCount += v.Content.Count;
                wo = (WorldObject)v.Content[0];
                v.Content.ToList().ForEach(v2 => tileContentString += " " + v2.ToString() + " " + wo.Type);
                wo = (WorldObject)v.Content[0];
            });
            return "Tile" + t.PlusCode.Code + "   Count:" + contentCount + "   ContentString:" + tileContentString;

        }

        public static bool EqualsBasedOnPlusCode(this MiniTile t1, MiniTile t2)
        {
            return t1.MiniTileId.Code.Equals(t2.MiniTileId.Code);
        }


        /// <summary>
        /// Function which creates a 2d array to represent the MiniTiles
        /// </summary>
        /// <param name="miniTiles">The list of MiniTiles for the 2d array.</param>
        /// <param name="squareSize">The size of the 2d array.</param>
        /// <returns>The MiniTile 2D array</returns>
        public static MiniTile[,] GetMiniTile2DArray(List<MiniTile> miniTiles, int squareSize)
        {

            MiniTile[,] miniTile2DArray = new MiniTile[squareSize, squareSize];
            int miniTileCounter = 0;
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    miniTile2DArray[i, j] = miniTiles[miniTileCounter];
                    miniTileCounter++;
                }
            }
            return miniTile2DArray;

        }


        public static MiniTile GetMiniTile(PlusCode locationCode, List<Tile> tileList)
        {
            var minitile =
              from tile in tileList
              from miniTile in tile.MiniTiles
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            return minitile.First();
        }

        public static MiniTile GetMiniTile(this PlusCode locationCode, IList<MiniTile> miniTileList)
        {
            var minitile =
              from miniTile in miniTileList
              where miniTile.MiniTileId.Code == locationCode.Code
              select miniTile;

            if (minitile.Count() == 0)
                return null;
            return minitile.First();
        }

        public static MiniTile GetMiniTile(this PlusCode locationCode, Dictionary<PlusCode, MiniTile> miniTileDict)
        {
            if (miniTileDict.TryGetValue(locationCode, out MiniTile miniTile))
            {
                return miniTile;
            }
            return null;
        }

        /// <summary>
        /// Function which returns a random miniTile code that belong to the parent 
        /// </summary>
        /// <returns>The pluscode of a random miniTile</returns>
        public static PlusCode GetRandomMiniTileByTileCode(Tile parentTile)
        {
            String parentCode = parentTile.PlusCode.Code;
            String afterPlus = "23456789CFGHJMPQRVWX";
            parentCode += "+";
            Random r = new Random();
            int i = r.Next(0, afterPlus.Length);
            parentCode += (afterPlus[i] + "");


            i = r.Next(0, afterPlus.Length);
            parentCode += (afterPlus[i] + "");

            return new PlusCode(parentCode, 10);
        }

        /// <summary>
        /// Function which returns a random miniTile chosen from the list of miniTiles
        /// </summary>
        /// <returns>The pluscode of a random miniTile</returns>
        public static MiniTile GetRandomMiniTileByList(List<MiniTile> miniTileList)
        {

            Random r = new Random();
            int i = r.Next(0, miniTileList.Count);


            return miniTileList[i];
        }



        public static String PlusCodeToTileName(PlusCode code)
        {
            string plusCode = code.Code;
            if (plusCode.Length > 8)
            {
               plusCode = plusCode.Substring(0, 8);
            }
            var md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(plusCode));
            var asInt = BitConverter.ToInt32(hashed, 0);
            var rnd = new Random(asInt);
            string newName = townNames[rnd.Next(0, townNames.Count)] + townNames[rnd.Next(0, townNames.Count)].ToLower();

            return newName;
        }


        public static List<string> townNames = new List<string>()
        {
"Witch",
"Bread",
"Sandwich",
"Sand",
"Green",
"Lore",
"Choir",
"Zorf",
"Man",
"Mill",
"Mule",
"Frighter",
"Twitch",
"Merk",
"Wich",
"Left",
"Right",
"Morf",
"Sky",
"Water",
"Mar",
"Morning",
"Evening",
"Like",
"Worm",
"Wood",
"Little",
"Lire",
"Freight",
"Fon",
"Fan",
"Like",
"Sip",
"Norf",
"Esler",
"Ear",
"Forest",
"Mountain",
"River",
"Bank",
"Rite",
"Norse",
"Fole",
"Maiden",
"King",
"Reach",
"Camp",
"Ore",
"Kor",
"Ton",
"Touple",
"Gecko",
"Big",
"Tall",
"Rel",
"Kanth",
"Zul",
"For",
"Extra",
"Super",
"Tin",
"Weed",
"Pork",
"Beef",
"Wind",
"Lake",
"Stop",
"Step",
"Fork",
"Spoon",
"Knife",
"Water",
"Mist",
"Fog",
"Lonely",
"Lively",
"Shrimp",
"Frog",
"Light",
"Dark",
"Hidden",
"Bread",
"Paper",
"Scary",
"Forgotten",
"Oler",
"Haro",
"Kela",
"Up",
"Down",
"Church",
"Medical",
"Speaker",
"Ha",
"Adventure",
"Hill",
"Dead",
"Ruin",
"Pop",
"Size",
"Margin",
"Mod",
"Lop",
"Ru",
"Far",
"Hor",
"Mun",
"Mouth",
"Crossing",
"Soil",
"Fun",
"Two",
"One",
"Four",
"Dragon",
"Long",
"Onward",
"Char",
"Gond",
"Xa",
"Cola",
"Undesire",
"Moon",
"Sun",
"Star",
"Dragonfly",
"Berry",
"Banana",
"Blue",
"Red",
"Yellow",
"Green",
"Upside",
"Downside",
"Earth",
"Quake",
"Sunken",
"Cake",
"Sing",
"Honest",
"Alpha",
"Beta",
"Grassy",
"Bare",
"Graph",
"Limit",
"Unreal",
"Sprout",
"Fall",
"Clay",
"Granite",
"Cow",
"Catcus",
"Crem",
"Soup",
"Pine",
"Beer",
"Cough",
"Laugh",
"Ana",
"Merry",
"Lorry",
"Furor",
"Orle",
"Hank",
"Brightly",
"Brith",
"Bright",
"Chocolate",
"Ghost",
"Ghostly",
"Forever",
"Infinite",
"Finite",
"Spring",
"Winter",
"Autmn",
"Summer",
"Glow",
"Desert",
"Jungle",
"Valley",
"Dirty",
"Dirt",
"Skeleton",
"Jonas",
"Glass",
"Pretty",
"Pre",
"Cursor",
"Curse",
"Cur",
"Ing",
"Angler",
"Fish",
"Fishing",
"Somewhere",
"Nowhere",
"Bird",
"Stork",
"Blood",
"Money",
"Tissue",
"Face",
"Hand",
"Leaf",
"Lightless",
"Hope",
"Strength",
"Desire",
"Numb",
"Table",
"Cat",
"Dog",
"Foot",
"Nose",
"Norse",
"Barbaric",
"Banana",
"Apple",
"Ketchup",
"Chip",
"Melody",
"System",
"Underwater",
"North",
"South",
"East",
"West",
"Over",
"Under",
"Tuka",
"Nor",
"Hol",
"Ma",
"Enz",
"Yoba",
"Aba",
"So",
"Baba",
"Ga",
"Daga",
"Ex",
"Loop",
"Excite",
"Sad",
"Turn",
"Flower",
"Sun",
"Sunflower",
"Cucumber",
"Reflection",
"Wrapping",
"Snake",
"Milk",
"Sunny",
"Cloudy",
"Snowy",
"Windy",
"Wind",
"Snow",
"Cloud",
"Sun",
"Ball",
"Brawl",
"Feet",
"Whooping",
"Cannon",
"Rocket",
"League",
"Hotel",
"Path",
"Finder",
"Bottle",
"Botch",
"Swamp",
"Tiger",
"Volcano",
"Balloney",
"Eye",
"Swallow",
"Come",
"Ice",
"Wallow",
"Fire",
"Why",
"Shake",
"Nation",
"Chips",
"Kiwi",
"Poker",
"Gambler",
"Addict",
"City",
"Bulb",
"Onion",
"Squirt",
"Pouf",
"Spooky",
"Sunless",
"Crash",
"Short",
"Bow",
"Hammer",
"Sword",
"King",
"Queen",
"Lord",
"Sire",
"Princess",
"Rythm",
"Dancer",
"Need",
"Many",
"Forsaken",
"Imagination",
"Way",
"Egg",
"Key",
"Lock",
"Elephant",
"Draw",
"Victory",
"Defeat",
"Civilization",
"Out",
"In",
"Get",
"Getting",
"Reverse",
"Bunch",
"Boring",
"Idea",
"Shiny",
"Metal",
"Smith",
"Anchor",
"Holy",
"Coal",
"Say",
"Knowledge",
"Move",
"Leather",
"Hard",
"Soft",
"Drunk",
"Unknown",
"Close",
"Far",
"Plains",
"Gem",
"Brave",
"Coward",
"Dark",
"Crystal",
"Detail",
"Term",
"Quick",
"Slow",
"Sleepy",
"Round",
"Ground",
"Hidden",
"Time",
"Space",
"Cubic",
"Wizard",
"Eggplant",
"Bear",
"Ant",
"Creep",
"Destroy",
"Bush",
"Nul",
"Old",
"New",
"Rust",
"Zoo",
"Hen",
"Choke",
"Rough",
"Sly",
"Nomad",
"Settler",
"Care",
"Fox",
"Twilight",
"Dawn",
"Dusk",
"Free",
"Sugar",
"Astral",
"Syrup",
"Ancient",
"Nightmare",
"Wood",
"Fool",
"Wool",
"Vo",
"Nal",
"Re",
"Jar",
"Und",
"Rosa",
"Cosy",
"Trap",
"Hike",
"Empty",
"Full",
"Task",
"Cemetery",
"Side",
"Simmer",
"Whisper",
"Glimmer",
"Strong",
"Weak",
"Pure",
"Sock",
"Chief",
"Chef",
"Cook",
"Thrill",
"Bridge",
"Owl",
"Cliff",
"Fissure",
"Mushroom",
"Room",
"Day",
"Night",
"Early",
"Late",
"Discovery",
"No",
"In",
"Rad",
"Rof",
"Roof",
"Loa",
"Snake",
"Cas",
"Rour",
"Hedge",
"Hike",
"Por",
"Joke",
"Fly",
"Bug",
"Diamond",
"Acidic",
"Nap",
"Cert",
"Leap",
"Heartache",
"Fetch",
"Patience",
"Roof",
"Plate",
"Purpose",
"Verse",
"Boat",
"Men",
"Twist",
"Meal",
"Apparatus",
"Waves",
"Porter",
"Cave",
"Class",
"Governor",
"Rifle",
"Adjustment",
"Ink",
"Collar",
"Design",
"Quiet",
"Slope",
"Flavor",
"Oil",
"Ladybug",
"Neck",
"Act",
"Fire",
"Crayon",
"Love",
"Stranger",
"Form",
"Teeth",
"Start",
"Believe",
"Death",
"Development",
"Twig",
"Hour",
"Plantation",
"Cent",
"Mind",
"Door",
"Daughter",
"Magic",
"Dog",
"Trade",
"Things",
"Star",
"Plot",
"Question",
"Station",
"Cakes",
"Receipt",
"Corn",
"Arch",
"Knowledge",
"Pail",
"Curtain",
"Tray",
"Zinc",
"Chicken",
"Skirt",
"Profit",
"Thumb",
"Yak",
"Temper",
"Snail",
"Food",
"Zebra",
"Pocket",
"Dime",
"Toes",
"Space",
"Copper",
"Page",
"Plant",
"Pump",
"Instrument",
"Army",
"Hook",
"Point",
"Liquid",
"Afternoon",
"Scarf",
"Throat",
"Middle",
"Cherry",
"Burst",
"Low",
"Voyage",
"Drain",
"Plot",
"Vein",
"Duck",
"Cow",
"Bit",
"Cap",
"Wind",
"Sense",
"Rose",
"Rabbit",
"Memory",
"Room",
"Clover",
"Shame",
"Governor",
"Shape",
"Vacation",
"Move",
"Stamp",
"Temper",
"Pest",
"Force",
"Place",
"Sail",
"Work",
"Slope",
"Slip",
"Spy",
"Wash",
"Horn",
"Tendency",
"Cannon",
"Tax",
"Coat",
"Top",
"Jellyfish",
"Protest",
"Picture",
"Rice",
"Trip",
"Insect",
"Soap",
"Meeting",
"Art",
"Temper",
"Level",
"Touch",
"Show",
"Reaction",
"Corn",
"Respect",
"Throne",
"Grip",
"Frame",
"Seat",
"Wealth",
"Brick",
"Salt",
"Shop",
"Swim",
"Circle",
"Mass",
"Lumber",
"Moon",
"Measure",
"Cave",
"Plantation",
"Property",
"Creator",
"Value",
"Mice",
"Limit",
"Crown",
"Morning",
"Coast",
"Popcorn",
"Scent",
"Sinister",
"Fire",
"Rail",
"Pencil",
"Quiver",
"Home",
"Machine",
"Fold",
"Duck",
"Mind",
"Nerve",
"Bird",
"Pig",



        };
    }
}
