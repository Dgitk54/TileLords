using DataModel.Common.Messages;
using Google.OpenLocationCode;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Common
{
    public static class DataModelFunctions
    {
       public static string ToConsoleString(this byte[] bytes)
        {
            var stringBuilder = new StringBuilder("byte[] array: [ ");
            foreach (var @byte in bytes)
            {
                stringBuilder.Append(@byte + ", ");
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }
       public static IMessage FromMessagePackPayload(this byte[] payload)
        {
            var msg = MessagePackSerializer.Deserialize<IMessage>(payload);
            return msg;
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
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        /// <summary>
        /// Pure function from <see href="https://stackoverflow.com/a/57032216">Stackoverflow</see>
        /// Subtracts two dictionaries.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns>subtracted dictionary with negative values in case the key is missing in the first dictionary </returns>
        public static Dictionary<K, int> SubtractDictionaries<K>(this Dictionary<K, int> one, Dictionary<K, int> other) where K : IEquatable<K>
        {
            var distinctKeys = one.Concat(other).Distinct().ToList();
            ;
            return one.Concat(other)
                       .Select(x => x.Key)
                       .Distinct()
                       .Select(x => new
                       {
                           Key = x,
                           Value1 = one.TryGetValue(x, out int Value1) ? Value1 : 0,
                           Value2 = other.TryGetValue(x, out int Value2) ? Value2 : 0,
                       })
                       .ToDictionary(x => x.Key, x => x.Value1 - x.Value2);
        }

        /// <summary>
        /// Pure Function, similar to subtraction <see cref="SubtractDictionaries{K}(Dictionary{K, int}, Dictionary{K, int})"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns>added Dictionary</returns>
        public static Dictionary<K, int> AddDictionaries<K>(this Dictionary<K, int> one, Dictionary<K, int> other)
        {
            return one.Concat(other)
                       .Select(x => x.Key)
                       .Distinct()
                       .Select(x => new
                       {
                           Key = x,
                           Value1 = one.TryGetValue(x, out int Value1) ? Value1 : 0,
                           Value2 = other.TryGetValue(x, out int Value2) ? Value2 : 0,
                       })
                       .ToDictionary(x => x.Key, x => x.Value1 + x.Value2);
        }



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

        public static GPS PlusCodeToGPS(this PlusCode code)
        {
            CodeArea codeArea = OpenLocationCode.Decode(code.Code);
            GPS gpsCenter = new GPS(codeArea.Center.Latitude, codeArea.Center.Longitude);
            return gpsCenter;

        }

        public static PlusCode GetNearbyRandomSpawn(PlusCode vicinity, int radius)
        {
            var list = LocationCodeTileUtility.GetTileSection(vicinity.Code, radius, 10);
            var randomSpot = new Random().Next(list.Count);
            return new PlusCode(list[randomSpot], 10);
        }

        public static PlusCode From10String(this string location)
        {
            return new PlusCode(location, 10);
        }


        public static IObservable<PlusCode> GetPlusCode(this IObservable<GPS> gps, IObservable<int> precision)
        {
            return from i in gps
                   from j in precision
                   select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);
        }


        /// <summary>
        /// Transfers a given GPS to a Pluscode in a non reactive way.
        /// </summary>
        /// <param name="gps"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static PlusCode GetPlusCode(this GPS gps, int precision)
        {
            return new PlusCode(new OpenLocationCode(gps.Lat, gps.Lon, precision).Code, precision);
        }

        public static GPS AddOFfset(this GPS gps, double latOffset, double lonOffset)
        {
            return new GPS(gps.Lat + latOffset, gps.Lon + lonOffset);
        }

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
        {
            return new PlusCode(string.Concat(code.Code.Take(target + 1)), target);
        }
    }
}
