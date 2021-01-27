using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class DataSinkEvent : IMessage
    {
        public string SerializedData { get; }

        public DataSinkEvent(string data) => SerializedData = data;
            
    }
}
