using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class DataSourceEvent : IEvent
    {
        public string Data { get; }
        public DataSourceEvent(string data) => Data = data;
    }
}
