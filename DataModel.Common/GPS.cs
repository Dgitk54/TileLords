using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public struct GPS
    {
        public GPS(double lat, double lon) => (Lat, Lon) = (lat, lon);
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

   
}
