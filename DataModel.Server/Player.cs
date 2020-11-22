using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Server
{
    public class Player : Client
    {
        BehaviorSubject<GPS> playerLocation { get; }
        BehaviorSubject<bool> isConnected { get; }

        
        public string GUID { get; }
    }
}
