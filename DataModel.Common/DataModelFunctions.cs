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

namespace DataModel.Common
{
    public static class DataModelFunctions
    {

        //TODO: Temporary cast  imessage to concrete message
        public static byte[] ToJsonPayload(this IMessage msg)
        {
            switch (msg)
            {
                case BatchContentMessage concreteType:
                    var serialized = JsonConvert.SerializeObject(concreteType, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
                    return Encoding.UTF8.GetBytes(serialized);
                default:
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    }));
            }
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
            for (int i = 1; i < points+1; i++)
            {
                double lat = startPos.Lat + (radius * Math.Cos(angleStep * i));
                double lon = startPos.Lon + (radius * Math.Sin(angleStep * i));
                nodes.Add(new GPS(lat, lon));
            }
            return nodes;
        }

        public static PlusCode ToLowerResolution(this PlusCode code, int target)
            => new PlusCode(string.Concat(code.Code.Take(target+1)), target);

        
    }
}
