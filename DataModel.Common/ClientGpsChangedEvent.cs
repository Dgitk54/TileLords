using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ClientGpsChangedEvent : IEvent
    {
        public GPS ClientGPSHasChanged { get; set; }
        public ClientGpsChangedEvent(GPS gps) => ClientGPSHasChanged = gps;
            
    }
}
