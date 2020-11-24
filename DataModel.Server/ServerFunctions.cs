using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;

namespace DataModel.Server
{
    public class ServerFunctions
    {
        public IObservable<NetworkJsonMessage> TransformPacket(IObservable<byte[]> dataStream) => from jsonCode in (from bytePacket in dataStream
                                                                                                                    select JsonConvert.ToString(bytePacket))
                                                                                                  select new NetworkJsonMessage(jsonCode);

        


    }
}
