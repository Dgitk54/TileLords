using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserGpsEvent : IEvent
    {
        public readonly string EventType = "UserGps";
        public GPS GpsData { get; set; }
        public UserGpsEvent(GPS gps) => GpsData = gps;
            
    }
}
