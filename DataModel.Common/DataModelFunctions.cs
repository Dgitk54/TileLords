using Google.OpenLocationCode;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using DataModel.Common.Messages;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace DataModel.Common
{
    public static class DataModelFunctions
    {

        public static byte[] ToJsonPayload(this IMessage msg)
        {
            var serialized = JsonConvert.SerializeObject(msg, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            return Encoding.UTF8.GetBytes(serialized);
        }

        public static IMessage FromJsonPayload(this byte[] payload)
        {
            return JsonConvert.DeserializeObject<IMessage>(Encoding.UTF8.GetString(payload));
        }

        public static IMessage FromString(this string payload)
        {
            return JsonConvert.DeserializeObject<IMessage>(payload, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        
        // see https://stackoverflow.com/questions/56692/random-weighted-choice
        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector, Random seed)
        {
            float totalWeight = sequence.Sum(weightSelector);
            // The weight we are after...
            float itemWeightIndex = (float)seed.NextDouble() * totalWeight;
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;
            }

            return default;
        }

        public static T ConvertEnumString<T>(this string name)
            => (T)Enum.Parse(typeof(T), name);

        

        public static PlusCode GetNearbyLocationWithinMinMaxBounds(string startPluscode, int maxbound, int minbound)
        {
            Debug.Assert(maxbound > minbound);
            var maxboundList = LocationCodeTileUtility.GetTileSection(startPluscode, maxbound, 10);
            //Pick location:
            var minboundList = LocationCodeTileUtility.GetTileSection(startPluscode, minbound, 10);
            var result = maxboundList.Except(minboundList).ToList();
            var random = new Random().Next(result.Count);
            return new PlusCode(result[random], 10);
        }

        public static PlusCode GetNearbyRandomSpawn(string vicinity, int radius)
        {
            var list = LocationCodeTileUtility.GetTileSection(vicinity, radius, 10);
            var randomSpot = new Random().Next(list.Count);
            return new PlusCode(list[randomSpot], 10);
        }

        public static PlusCode From10String(this string location)
        {
            return new PlusCode(location, 10);
        }

        public static IObservable<PlusCode> GetPlusCode(this IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);


        /// <summary>
        /// Transfers a given GPS to a Pluscode in a non reactive way.
        /// </summary>
        /// <param name="gps"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static PlusCode GetPlusCode(this GPS gps, int precision)
             => new PlusCode(new OpenLocationCode(gps.Lat, gps.Lon, precision).Code, precision);



        public static GPS AddOFfset(this GPS gps, double latOffset, double lonOffset)
            => new GPS(gps.Lat + latOffset, gps.Lon + lonOffset);



        public static List<GPS> GPSNodesWithOffsets(GPS startPos, double latNodeOffset, double lonNodeOffset, int nodeAmount)
        {
            var changeList = new List<GPS>();
            for (int i = 0; i < nodeAmount; i++)
                changeList.Add(DataModelFunctions.AddOFfset(startPos, latNodeOffset * i, lonNodeOffset * i));
            return changeList;
        }


        public static List<GPS> GPSNodesInCircle(GPS startPos, int points, double radius)
        {
            double angleStep = 360d / points;
            Math.Floor(angleStep);
            var nodes = new List<GPS>();
            for (int i = 1; i < points + 1; i++)
            {
                double lat = startPos.Lat + (radius * Math.Cos(angleStep * i));
                double lon = startPos.Lon + (radius * Math.Sin(angleStep * i));
                nodes.Add(new GPS(lat, lon));
            }
            return nodes;
        }

        public static PlusCode ToLowerResolution(this PlusCode code, int target)
            => new PlusCode(string.Concat(code.Code.Take(target + 1)), target);


    }
}
