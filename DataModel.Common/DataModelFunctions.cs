using Google.OpenLocationCode;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace DataModel.Common
{
    public class DataModelFunctions
    {

        public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);


        /// <summary>
        /// Transfers a given GPS to a Pluscode in a non reactive way.
        /// </summary>
        /// <param name="gps"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static PlusCode GetPlusCode(GPS gps, int precision)
             => new PlusCode(new OpenLocationCode(gps.Lat, gps.Lon, precision).Code, precision);



        /// <summary>
        /// Helper function for obtaining a new gps node with added lattitude from a given GPS node.
        /// </summary>
        /// <param name="gps">GPS start point</param>
        /// <param name="amount">added lat amount</param>
        /// <returns>a new GPS node</returns>
        public static GPS AddLat(GPS gps, double amount)
            => new GPS(gps.Lat + amount, gps.Lon);


        /// <summary>
        /// Helper function for obtaining a new gps node with added longitute from a given GPS node.
        /// </summary>
        /// <param name="gps"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static GPS AddLon(GPS gps, double amount)
            => new GPS(gps.Lat, gps.Lon + amount);


        public static GPS AddOFfset(GPS gps, double latOffset, double lonOffset)
            => new GPS(gps.Lat + latOffset, gps.Lon + lonOffset);



        public static List<GPS> GPSNodesWithOffsets(GPS startPos, double latNodeOffset, double lonNodeOffset, int nodeAmount)
        {
            var changeList = new List<GPS>();
            for (int i = 0; i < nodeAmount; i++)
                changeList.Add(DataModelFunctions.AddOFfset(startPos, latNodeOffset * i, lonNodeOffset * i));
            return changeList;
        }

        public static PlusCode ToLowerResolution(PlusCode code, int target)
            => new PlusCode(string.Concat(code.Code.Take(target+1)), target);


    }
}
