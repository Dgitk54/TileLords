﻿using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class DataSinkEvent : IEvent
    {
        public string SerializedData { get; }

        public DataSinkEvent(string data) => SerializedData = data;
            
    }
}
