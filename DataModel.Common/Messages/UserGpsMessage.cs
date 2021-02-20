using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class UserGpsMessage : IMsgPackMsg
    {
        [Key(0)]
        public double Lat { get; set; }

        [Key(1)]
        public double Lon { get; set; }
    }
}
