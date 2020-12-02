using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ClientGpsChangedEvent : IEvent
    {
        public GPS NewGPS { get; }
        public ClientGpsChangedEvent(GPS gps) => NewGPS = gps;
            
    }
}
