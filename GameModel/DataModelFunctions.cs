using Google.OpenLocationCode;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using DataModel;
using System.Linq;
using System.Reactive.Subjects;

namespace DataModel
{
    public class DataModelFunctions
    {





        //TODO: CombineLatest into one stream
        public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);
        
            
        //https://stackoverflow.com/questions/24781799/combining-latest-with-previous-value-in-an-observable-stream
        //https://stackoverflow.com/questions/32130668/determine-and-operate-on-the-latest-updated-sequence-in-a-combinelatest


        //public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
        //    => from j in precision
        //       from i in gps
        //       select Observable.CombineLatest<GPS, int, PlusCode>(gps, precision, (v1, v2) => new
        //          {
        //              if (v2 == 0)
        //                  throw new Exception("precision was set to 0");
        //              new PlusCode(new OpenLocationCode(v1.Lat, v1.Lon, v2).Code, j);
        //          });



        // public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
        //     => from j in precision
        //        from i in gps
        //        let combination = gps.CombineLatest(precision, (_gps, _prec) => (_gps: gps, _prec: precision))
        //        select new PlusCode(new OpenLocationCode(combination.las, i.Lon, j).Code, j);




    }
}
