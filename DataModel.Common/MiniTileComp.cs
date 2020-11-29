using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DataModel.Common
{
    public class MiniTileComp : IComparer<MiniTile>
    {

        public int Compare(MiniTile x, MiniTile y)
        {
            
            if (IsTileCodeBigger(x.PlusCode, y.PlusCode))
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }

        public static bool IsTileCodeBigger(PlusCode plusCode, PlusCode plusCode2)
        {
            Dictionary<String, int> codeToInt;
            codeToInt = LocationCodeTileUtility.CreateDictionary();

            String code = plusCode.Code;
            //remove the plus
            code = code.Remove(8, 1);
            int[] array = new int[plusCode.Code.Length];

            LocationCodeTileUtility.CodeToIntegerValues(code, codeToInt, array);


            String code2 = plusCode2.Code;
            //remove the plus
            code2 = code2.Remove(8, 1);
            int[] array2 = new int[plusCode2.Code.Length];
            LocationCodeTileUtility.CodeToIntegerValues(code2, codeToInt, array2);

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

                if (i % 2 == 0)
                {
                    if (one < two)
                    {
                        return true;

                    }
                    else if(two > one)
                    {
                        return false;
                    }
                    else
                    {

                    }
                }
            }

            for (int i = 0; i < intCode.Length; i++)
            {
                int one = intCode[i];
                int two = intCode2[i];

                if (i % 2 != 0)
                {
                    if (one < two)
                    {
                        return false;

                    }
                    else if (two > one)
                    {
                        return true;
                    }
                    else
                    {

                    }
                }
            }
            Debug.WriteLine("!");
            return false;
           /* if (one < two)
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
            return false; */


        }
    }
}
