using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Common
{

    public class LocationCodeTileUtility
    {




        /// <summary>
        /// Function which returns a section of PlusCode strings 
        /// </summary>
        /// <param name="code">PlusCode of the "Tile" in the middle of the desired section.</param>
        /// <param name="radius">The radius of the section to create.</param>
        /// <param name="precision">The PlusCode precision of this section.</param>
        /// <returns>The list containing all PlusCodes of this section</returns>
        public static List<String> GetTileSection(String code, int radius, int precision)
        {
            Dictionary<String, int> codeToInt;
            codeToInt = CreateDictionary();
            int[] xArray = new int[precision / 2];
            int[] yArray = new int[precision / 2];
            int[] xSaveArray = new int[precision / 2];
            int[] ySaveArray = new int[precision / 2];

            code = code.Substring(0, precision);

            for (int i = 0; i < precision / 2; i++)
            {

                xArray[i] = 0;
                yArray[i] = 0;
                xSaveArray[i] = 0;
                ySaveArray[i] = 0;

            }


            //remove the plus
            if (precision > 8)
            {
                code = code.Remove(8, 1);
            }

            //create the list to return later
            List<String> locationCodes = new List<String>();


            //set x1 to x5 and y1 to y5 to the int values accordingly
            CodeToIntegerValues(code, codeToInt, xArray, yArray);



            //save x and y int values of the given code
            Array.Copy(xArray, xSaveArray, xSaveArray.Length);
            Array.Copy(yArray, ySaveArray, ySaveArray.Length);


            //determine the codes and add them to the list
            DetermineLocationCodes(code, locationCodes, codeToInt, radius, precision, xArray, yArray, xSaveArray, ySaveArray);


            for (int i = 0; i < locationCodes.Count; i++)
            {
                //add the "+" back
                if (locationCodes[i].Length > 8)
                {
                    locationCodes[i] = locationCodes[i].Insert(8, "+");
                }
                else
                {
                    locationCodes[i] = locationCodes[i] + "+";
                }


            }



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
        /// <param name="xArray">Array holding each PlusCode part of x (left/right).</param>
        /// <param name="yArray">Array holding each PlusCode part of Y (north/south).</param>
        /// <param name="xSaveArray">Array with original "Tile" X PlusCode parts.</param>
        /// <param name="ySaveArray">Array with original "Tile" Y PlusCode parts.</param>

        public static void DetermineLocationCodes(String code, List<String> plusCodes, Dictionary<String, int> codeToInt, int radius, int precision, int[] xArray, int[] yArray, int[] xSaveArray, int[] ySaveArray)
        {
            String newCode = "";
            //set top left
            for (int k = 1; k <= radius; k++)
            {


                GoUp(yArray);
                for (int l = 1; l <= radius; l++)
                {
                    newCode = "";

                    GoLeft(xArray);
                    plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
                }
                Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);

            //set top
            for (int k = 1; k <= radius; k++)
            {



                newCode = "";

                GoUp(yArray);
                plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);


            //set top right
            for (int k = 1; k <= radius; k++)
            {


                GoUp(yArray);

                for (int l = 1; l <= radius; l++)
                {
                    newCode = "";

                    GoRight(xArray);
                    //convert the code back into a location code and add it to the list
                    plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
                }
                //resetting X values to original one
                Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);


            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);



            //set left
            for (int k = 1; k <= radius; k++)
            {



                newCode = "";

                GoLeft(xArray);
                plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);


            //set middle
            plusCodes.Add(code);



            //set right
            for (int k = 1; k <= radius; k++)
            {



                newCode = "";

                GoRight(xArray);
                plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);



            //set bottom left
            for (int k = 1; k <= radius; k++)
            {


                GoDown(yArray);
                for (int l = 1; l <= radius; l++)
                {
                    newCode = "";

                    GoLeft(xArray);

                    plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
                }
                Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);



            //set bottom
            for (int k = 1; k <= radius; k++)
            {

                newCode = "";

                GoDown(yArray);
                plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

            }

            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);



            //set bottom right
            for (int k = 1; k <= radius; k++)
            {


                GoDown(yArray);
                for (int l = 1; l <= radius; l++)
                {
                    newCode = "";

                    GoRight(xArray);
                    plusCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
                }
                Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            }









        }


        /// <summary>
        /// Function which converts PlusCode into int values
        /// </summary>
        public static void CodeToIntegerValues(String code, Dictionary<String, int> codeToInt, int[] xArray, int[] yArray)
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

        public static void CodeToIntegerValues(String code, Dictionary<String, int> codeToInt, int[] array)
        {

            for (int i = 0; i < code.Length; i++)
            {
                String c = code[i] + "";
                if (codeToInt.TryGetValue(c, out int j))
                {

                    array[i] = j;

                }

            }
        }




        /// <summary>
        /// Function which copies values of given array
        /// </summary>
        public static int[] ResetX(int[] xSaveArray)
        {
            int[] xArray = xSaveArray;
            return xArray;

        }

        /// <summary>
        /// Function which copies values of given array
        /// </summary>
        public static int[] ResetY(int[] ySaveArray)
        {
            int[] yArray = ySaveArray;
            return yArray;

        }

        /// <summary>
        /// Function which converts int to PlusCode back
        /// </summary>
        public static String ConvertBackToString(String newCode, Dictionary<String, int> codeToInt, int precision, int[] xArray, int[] yArray)
        {
            int xArrayCounter = 0;
            int yArrayCounter = 0;
            for (int i = 0; i < precision; i++)
            {
                if (i % 2 == 0)
                {
                    var myKey = codeToInt.FirstOrDefault(x => x.Value == xArray[xArrayCounter]).Key;
                    xArrayCounter++;
                    newCode += myKey;
                }
                else
                {
                    var myKey = codeToInt.FirstOrDefault(x => x.Value == yArray[yArrayCounter]).Key;
                    yArrayCounter++;
                    newCode += myKey;
                }


            }

            return newCode;
        }



        /// <summary>
        /// Function which creates the dictionary to convert PlusCodes to int and back
        /// </summary>
        public static Dictionary<String, int> CreateDictionary()
        {

            Dictionary<String, int> codeToInt = new Dictionary<String, int>();
            codeToInt.Add("2", 1);
            codeToInt.Add("3", 2);
            codeToInt.Add("4", 3);
            codeToInt.Add("5", 4);
            codeToInt.Add("6", 5);
            codeToInt.Add("7", 6);
            codeToInt.Add("8", 7);
            codeToInt.Add("9", 8);
            codeToInt.Add("C", 9);
            codeToInt.Add("F", 10);
            codeToInt.Add("G", 11);
            codeToInt.Add("H", 12);
            codeToInt.Add("J", 13);
            codeToInt.Add("M", 14);
            codeToInt.Add("P", 15);
            codeToInt.Add("Q", 16);
            codeToInt.Add("R", 17);
            codeToInt.Add("V", 18);
            codeToInt.Add("W", 19);
            codeToInt.Add("X", 20);

            return codeToInt;
        }

        /// <summary>
        /// Function which determines the right PlusCode
        /// </summary>
        public static void GoRight(int[] xArray)
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
        /// Function which determines the left PlusCode
        /// </summary>
        public static void GoLeft(int[] xArray)
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
        /// Function which determines the upper PlusCode
        /// </summary>

        public static void GoUp(int[] yArray)
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
        /// Function which determines the lower PlusCode
        /// </summary>
        public static void GoDown(int[] yArray)
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
        public static List<String> GetAllAfterPlus()
        {
            String all = "23456789CFGHJMPQRVWX";
            String rev = "XWVRQPMJHGFC98765432";

            var sum = from c1 in rev
                      from c2 in all
                      select c1 + "" + c2;


            return sum.ToList();
        }

        /// <summary>
        /// Function which determines the two PlusCodes signs that come after the "+" and combines it with a given parent PlusCode string
        /// </summary>
        ///  <returns>The List of complete PlusCodes (8 signs "+" 2 signs)</returns>
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



        public static List<MiniTile> SortList(List<MiniTile> miniTileList)
        {
            List<MiniTile> sortedList = miniTileList;

            for (int i = 0; i < sortedList.Count - 1; i++)
            {

                for (int j = 0; j < sortedList.Count - 1; j++)
                {
                    PlusCode code = sortedList[j].PlusCode;
                    PlusCode code2 = sortedList[j + 1].PlusCode;
                    if (IsTileCodeBigger(code, code2))
                    {
                        MiniTile tmp = sortedList[j];
                        sortedList[j] = sortedList[j + 1];
                        sortedList[j + 1] = tmp;
                    }

                }
            }


            return sortedList;

        }



        public static bool IsTileCodeBigger(PlusCode plusCode, PlusCode plusCode2)
        {
            Dictionary<String, int> codeToInt;
            codeToInt = CreateDictionary();

            String code = plusCode.Code;
            //remove the plus
            code = code.Remove(8, 1);
            int[] array = new int[plusCode.Code.Length];

            CodeToIntegerValues(code, codeToInt, array);


            String code2 = plusCode2.Code;
            //remove the plus
            code2 = code2.Remove(8, 1);
            int[] array2 = new int[plusCode2.Code.Length];
            CodeToIntegerValues(code2, codeToInt, array2);

            return IsBigger(array, array2);


        }


        public static bool IsBigger(int[] intCode, int[] intCode2) //is intCode bigger intCode2
        {

            //x values follow the logic 9 < 8 < 7 etc
            //y values follow normal logic 7 < 8 < 9
            //the intCode is xyxyxyxyxy 
            for (int i = 0; i < intCode.Length; i++)
            {
                int one = intCode[i];
                int two = intCode2[i];

                if (one < two)
                {
                    if (i % 2 == 0) //apply x value logic
                    {
                        //intCode is bigger intCode2
                        return true;

                    }
                    else //apply y value logic
                    {
                        //intCode is smaller intCode2
                        return false;

                    }

                }
                else if (one > two)
                {
                    if (i % 2 == 0) //apply x value logic
                    {
                        //intCode is smaller intCode2
                        return false;

                    }
                    else  //apply y value logic
                    {
                        //intCode is bigger intCode2
                        return true;

                    }

                }
                else //they are the same, continue the loop
                {

                }
            }
            return false;


        }





    }

}


