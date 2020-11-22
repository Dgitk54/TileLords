using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    //Algorithms from https://github.com/bocops/open-geotiling/blob/master/java/org/bocops/opengeotiling/OpenGeoTile.java
    public static class PlusCodeUtils
    {


        public static int GetManhattenDistance(PlusCode one, PlusCode two)
        {
            if (one.Precision != two.Precision)
                throw new Exception("Precision of Tiles must be equal");

            return GetLatitudinalTileDistance(one, two, true) +
                    GetLongitudinalTileDistance(one, two, true);

        }



        private static int CharacterDistance(char c1, char c2)
        {
            var alphabet = "23456789CFGHJMPQRVWX".ToCharArray();
            var charToIndex = new Dictionary<char, int>();
            int index = 0;
            foreach (char character in alphabet)
            {
                char lowerCaseCharacter = char.ToLower(character);

                charToIndex.Add(character, index);
                charToIndex.Add(lowerCaseCharacter, index);
                index++;
            }

            if (!(charToIndex.ContainsKey(c1) && charToIndex.ContainsKey(c2)))
                throw new Exception("Character does not exist in alphabet");

            int one;
            int two;
            charToIndex.TryGetValue(c1, out one);
            charToIndex.TryGetValue(c2, out two);
            return one - two;
        }


        private static int GetLatitudinalTileDistance(PlusCode one, PlusCode two, bool absolute)
        {
            if (one.Precision != two.Precision)
                throw new Exception("Precision of Tiles must be equal");

            int numIterations = one.Precision / 2; //1..5
            int tileDistance = 0;
            for (int i = 0; i < numIterations; i++)
            {
                tileDistance *= 20;
                char c1 = one.Code[i * 2];
                char c2 = two.Code[i * 2];
                tileDistance += CharacterDistance(c1, c2);
            }

            if (absolute)
            {
                return Math.Abs(tileDistance);
            }
            return tileDistance;
        }

        private static int GetLongitudinalTileDistance(PlusCode one, PlusCode two, bool absolute)
        {
            if (one.Precision != two.Precision)
                throw new Exception("Precision of Tiles must be equal");

            int numIterations = one.Precision / 2; //1..5
            int tileDistance = 0;
            for (int i = 0; i < numIterations; i++)
            {
                tileDistance *= 20;
                char c1 = one.Code[i * 2 + 1];
                char c2 = two.Code[i * 2 + 1];

                //for the first longitudinal value, we need to take care of wrapping - basically,
                //if it's shorter to go the other way around, do so
                if (i == 0)
                {
                    var firstDiff = CharacterDistance(c1, c2);
                    var charused = 18;// 360degree/20degree
                    if (Math.Abs(firstDiff) > 9)
                    {
                        if (firstDiff > 0)
                        {
                            firstDiff -= charused;
                        }
                        else
                        {
                            firstDiff += charused;
                        }
                    }
                    tileDistance += firstDiff;
                }
                else
                {
                    tileDistance += CharacterDistance(c1, c2);
                }
            }
            if (absolute)
            {
                return Math.Abs(tileDistance)
            }
            return tileDistance;
        }








    }
}
