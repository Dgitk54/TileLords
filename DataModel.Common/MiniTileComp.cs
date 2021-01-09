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
          

            if (IsTileCodeBigger(x.MiniTileId, y.MiniTileId))
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// Function to determine which plusCode is bigger than the other (order: top right (NW) to bottom left (SE))
        /// </summary>
        /// <param name="plusCode"></param>
        /// <param name="plusCode2"></param>
        /// <returns>True or False depending on plusCode < plusCode2 </returns>
        public static bool IsTileCodeBigger(PlusCode plusCode, PlusCode plusCode2)
        {
            var codeToInt = LocationCodeTileUtility.PlusCodeIntegerLookup;

            String code = plusCode.Code;
            //remove the plus
            code = code.Replace("+", "");
         
            int[] array = new int[code.Length];

            CodeToIntegerValues(code, codeToInt, array);


            String code2 = plusCode2.Code;
            //remove the plus
            code2 = code2.Replace("+", "");
       
            int[] array2 = new int[code.Length];
            CodeToIntegerValues(code2, codeToInt, array2);

     
            return IsBigger(array, array2);


        }

        static void CodeToIntegerValues(String code, Dictionary<String, int> codeToInt, int[] array)
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

        static bool IsBigger(int[] intCode, int[] intCode2) //is intCode bigger intCode2
        {

            //x values follow the logic 9 < 8 < 7 etc
            //y values follow normal logic 7 < 8 < 9
            //the intCode is xyxyxyxyxy 
            //compare x values fist, then y values
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
                    else if (two < one)
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
                    else if (two < one)
                    {
                        return true;
                    }
                    else
                    {

                    }
                }
            }

            return false;
            


        }
    }
}
