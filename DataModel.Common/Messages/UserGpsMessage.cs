
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
  
    public class UserGpsMessage : IMsgPackMsg
    {
     
        public double Lat { get; set; }

      
        public double Lon { get; set; }
    }
}
