using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    readonly public struct GPS
    {
        public GPS(double lat, double lon) => (Lat, Lon) = (lat, lon);
        public double Lat { get; }
        public double Lon { get; }
    }

   
}
