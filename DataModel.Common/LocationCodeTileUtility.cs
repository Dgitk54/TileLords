using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Common
{

    public static class LocationCodeTileUtility
    {


        public static Dictionary<int, string> IntegerPlusCodeLookup = new Dictionary<int, string>()
        {
            {1,"2" },
            {2,"3" },
            {3,"4" },
            {4,"5" },
            {5,"6" },
            {6,"7" },
            {7,"8" },
            {8,"9" },
            {9,"C" },
            {10,"F" },
            {11,"G" },
            {12,"H" },
            {13,"J" },
            {14,"M" },
            {15,"P" },
            {16,"Q" },
            {17,"R" },
            {18,"V" },
            {19,"W" },
            {20,"X" },

        };

        public static Dictionary<string, int> PlusCodeIntegerLookup = new Dictionary<string, int>()
        {
            {"2", 1},
            {"3", 2},
            {"4", 3},
            {"5", 4},
            {"6", 5},
            {"7", 6},
            {"8", 7},
            {"9", 8},
            {"C", 9},
            {"F", 10},
            {"G", 11},
            {"H", 12},
            {"J", 13},
            {"M", 14},
            {"P", 15},
            {"Q", 16},
            {"R", 17},
            {"V", 18},
            {"W", 19},
            {"X", 20}
        };

        /// <summary>
        /// Function which adds all PlusCodes signs after the plus to the given plusCode of a tile 
        /// </summary>
        public static List<String> GetAndCombineWithAllAfterPlus(String plusCode)
        {

            List<string> allAfterPlus = GetAllAfterPlus();
            List<string> completeCodeList = new List<string>();
            for (int i = 0; i < allAfterPlus.Count; i++)
            {
                completeCodeList.Add(plusCode + allAfterPlus[i]);
            }
            return completeCodeList;
        }




        /// <summary>
        /// Function which returns a section of PlusCode strings 
        /// </summary>
        /// <param name="code">PlusCode of the "Tile" in the middle of the desired section.</param>
        /// <param name="radius">The radius of the section to create.</param>
        /// <param name="precision">The PlusCode precision of this section.</param>
        /// <returns>The list containing all PlusCodes of this section</returns>
        public static List<string> GetTileSection(string code, int radius, int precision)
        {
            int arraySize = precision / 2;
            int[] xArray = new int[arraySize];
            int[] yArray = new int[arraySize];
            int[] xSaveArray = new int[arraySize];
            int[] ySaveArray = new int[arraySize];



            for (int i = 0; i < precision / 2; i++)
            {

                xArray[i] = 0;
                yArray[i] = 0;
                xSaveArray[i] = 0;
                ySaveArray[i] = 0;

            }

            code = code.Replace("+", "");

            //create the list to return later
            List<string> locationCodes = new List<string>();


            //set x1 to x5 and y1 to y5 to the int values accordingly
            CodeToIntegerValues(code, PlusCodeIntegerLookup, xArray, yArray);


            //save x and y int values of the given code
            Array.Copy(xArray, xSaveArray, xSaveArray.Length);
            Array.Copy(yArray, ySaveArray, ySaveArray.Length);


            //determine the codes and add them to the list
            //DetermineLocationCodes(code, locationCodes, PlusCodeIntegerLookup, radius, precision, xArray, yArray, xSaveArray, ySaveArray);
            DetermineLocationCodesSorted(locationCodes, radius, precision, xArray, yArray);

            return locationCodes;
        }

       


        /// <summary>
        /// Function which calculates the Location Codes for the desired section
        /// </summary>
        /// <param name="code">PlusCode of the "Tile" in the middle of the desired section.</param>
        /// <param name="plusCodes">The List which will hold the created PlusCode strings.</param>
        /// <param name="codeToInt">Dictionary to convert PlusCode signs into ints.</param>
        /// <param name="radius">radius of desired section.</param>
        /// <param name="precision">precision of given PlusCode.</param>
        /// <param name="yArray">Array holding each PlusCode part of x (left/right).</param>
        /// <param name="xArray">Array holding each PlusCode part of Y (north/south).</param>
        /// <param name="xSaveArray">Array with original "Tile" X PlusCode parts.</param>
        /// <param name="ySaveArray">Array with original "Tile" Y PlusCode parts.</param>
        static void DetermineLocationCodesSorted(List<string> plusCodes, int radius, int precision, int[] yArray, int[] xArray)
        {
            StringBuilder sb = new StringBuilder();

            //get to top left corner
            for (int k = 1; k <= radius; k++)
            {

                GoLeft(xArray);

                GoUp(yArray);

            }
            //copy area by value (not by reference!)
            int[] saveX = xArray.ToArray();
            //go from top left to bottom right
            for (int i = 1; i <= (radius * 2) + 1; i++)
            {

                for (int j = 1; j <= (radius * 2) + 1; j++)
                {

                    plusCodes.Add(ConvertBackToString(precision, yArray, xArray, sb));
                    GoRight(xArray);


                }

                //reset x value (first column again)
                xArray = saveX.ToArray();

                GoDown(yArray);

            }
        }


       

        /// <summary>
        /// Function which converts PlusCode into int values
        /// </summary>
        static void CodeToIntegerValues(String code, Dictionary<String, int> codeToInt, int[] xArray, int[] yArray)
        {
            int xCounter = 0;
            int yCounter = 0;
            for (int i = 0; i < code.Length; i++)
            {
                String c = code[i] + "";
                if (codeToInt.TryGetValue(c, out int j))
                {

                    if (i % 2 == 0)
                    {

                        xArray[xCounter] = j;
                        xCounter++;

                    }
                    else
                    {
                        yArray[yCounter] = j;
                        yCounter++;

                    }
                }

            }
        }






        /// <summary>
        /// Function which copies values of given array
        /// </summary>
        static int[] ResetX(int[] xSaveArray)
        {
            int[] xArray = xSaveArray;
            return xArray;

        }

        /// <summary>
        /// Function which copies values of given array
        /// </summary>
        static int[] ResetY(int[] ySaveArray)
        {
            int[] yArray = ySaveArray;
            return yArray;

        }

        /// <summary>
        /// Function which converts int to PlusCode back
        /// </summary>
        static string ConvertBackToString(int precision, int[] xArray, int[] yArray, StringBuilder sb)
        {
            sb.Clear();
            int xArrayCounter = 0;
            int yArrayCounter = 0;
            string newCode;
            for (int i = 0; i < precision; i++)
            {
                if (i % 2 == 0)
                {
                    sb.Append(IntegerPlusCodeLookup[xArray[xArrayCounter]]);
                    //string sign = IntegerPlusCodeLookup[xArray[xArrayCounter]];

                    xArrayCounter++;
                    //newCode += sign;
                }
                else
                {
                    sb.Append(IntegerPlusCodeLookup[yArray[yArrayCounter]]);
                    // string sign = IntegerPlusCodeLookup[yArray[yArrayCounter]];

                    yArrayCounter++;
                    // newCode += sign;
                }


            }
            sb.Insert(8, "+");
            newCode = sb.ToString();
            //newCode = newCode.Insert(8, "+");
            return newCode;
        }



        /// <summary>
        /// Function which determines the upper PlusCode
        /// </summary>
        static void GoUp(int[] xArray)
        {


            for (int i = xArray.Length - 1; i > 0; i--)
            {
                if (xArray[i] < 20)
                {
                    xArray[i] += 1;
                    break;
                }
                xArray[i] = 1;
            }

        }



        /// <summary>
        /// Function which determines the lower PlusCode
        /// </summary>
        static void GoDown(int[] xArray)
        {
            for (int i = xArray.Length - 1; i > 0; i--)
            {
                if (xArray[i] > 1)
                {
                    xArray[i] -= 1;
                    break;
                }
                xArray[i] = 20;
            }
        }

        /// <summary>
        /// Function which determines the right PlusCode
        /// </summary>

        static void GoRight(int[] yArray)
        {

            for (int i = yArray.Length - 1; i > 0; i--)
            {
                if (yArray[i] < 20)
                {
                    yArray[i] += 1;
                    break;
                }
                yArray[i] = 1;
            }
        }


        /// <summary>
        /// Function which determines the left PlusCode
        /// </summary>
        static void GoLeft(int[] yArray)
        {
            for (int i = yArray.Length - 1; i > 0; i--)
            {
                if (yArray[i] > 1)
                {
                    yArray[i] -= 1;
                    break;
                }
                yArray[i] = 20;
            }
        }



        /// <summary>
        /// Function which determines the two PlusCodes signs that come after the "+"
        /// </summary>
        ///  <returns>The list with all PlusCodes that come after the "+"</returns>
        static List<String> GetAllAfterPlus()
        {
            String all = "23456789CFGHJMPQRVWX";
            String rev = "XWVRQPMJHGFC98765432";

            var sum = from c1 in rev
                      from c2 in all
                      select c1 + "" + c2;


            return sum.ToList();
        }









    }

}


