using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class ObservableResource
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public ITileContent Content { get; set; }
        public IObservable<PlusCode> Location { get; set; }

        public IObservable<bool> IsActive { get; set; }
    }
}
