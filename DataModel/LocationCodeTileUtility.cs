using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Common
{

    public class LocationCodeTileUtility
    {





        public static List<String> FindRenderRange(String code, int radius, int precision)
        {
            Dictionary<String, int> codeToInt = new Dictionary<string, int>();
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


        public static void DetermineLocationCodes(String code, List<String> locationCodes, Dictionary<String, int> codeToInt, int radius, int precision, int[] xArray, int[] yArray, int[] xSaveArray, int[] ySaveArray)
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
                    locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
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
                locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

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
                    locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
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
                locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

            }
            Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            Array.Copy(ResetY(ySaveArray), yArray, yArray.Length);


            //set middle
            locationCodes.Add(code);



            //set right
            for (int k = 1; k <= radius; k++)
            {



                newCode = "";

                GoRight(xArray);
                locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

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

                    locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
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
                locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));

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
                    locationCodes.Add(ConvertBackToString(newCode, codeToInt, precision, xArray, yArray));
                }
                Array.Copy(ResetX(xSaveArray), xArray, xArray.Length);
            }







      

        }



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

        public static int[] ResetX(int[] xSaveArray)
        {
            int[] xArray = xSaveArray;
            return xArray;

        }


        public static int[] ResetY(int[] ySaveArray)
        {
            int[] yArray = ySaveArray;
            return yArray;

        }

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


        /*public static List<String> GetAllAfterPlus()
        {
            String all = "23456789CFGHJMPQRVWX";
            List<String> allAfterPlus = new List<String>();
            for (int i = 0; i < all.Length; i++)
            {
                char c = all[i];
                for (int j = 0; j < all.Length; j++)
                {
                    char h = all[j];
                    String combine = c + "" + h + "";
                    allAfterPlus.Add(combine);

                }
            }
            return allAfterPlus;
        }*/

        public static List<String> GetAllAfterPlus()
        {
            String all = "23456789CFGHJMPQRVWX";
            String rev = "XWVRQPMJHGFC98765432";

            var sum = from c1 in rev
                      from c2 in all
                      select  c1 + "" + c2;
          

            return sum.ToList();
        }

        public static List<String> GetAndCombineWithAllAfterPlus(String locationCode)
        {
           
            List<string> allAfterPlus = GetAllAfterPlus();
            List<string> completeCodeList = new List<string>();
            for (int i = 0; i < allAfterPlus.Count; i++)
            {
                completeCodeList.Add(locationCode + allAfterPlus[i]);
            }
            return completeCodeList;
        }


        

        
    }

}


