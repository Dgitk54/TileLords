using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserGpsChangedEvent : IEvent
    {
        public string EventType = "UserGpsChanged";
        public GPS GpsData { get; set; }
        public UserGpsChangedEvent(GPS gps) => GpsData = gps;
            
    }
}
