using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ObservableTileContent : ITileContent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IObservable<PlusCode> Location { get; set; }

    }
}
