using Google.OpenLocationCode;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using System.Linq;
using System.Reactive.Subjects;

namespace DataModel.Common
{
    public class DataModelFunctions
    {

        public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);

        public static PlusCode GetPlusCode(GPS gps, int precision)
             => new PlusCode(new OpenLocationCode(gps.Lat, gps.Lon, precision).Code, precision);

    }
}
