using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Subjects;
using System.Text;
using DataModel.Common;

namespace DataModel.Server
{
    public class Client
    {

        public IPAddress Address { get; }

        public Client(IPAddress address) => (Address) = address;

    }
}
