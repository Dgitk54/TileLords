using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    interface ITileHandler
    {
        IObservable<Tile> PlusCodeTileMapper(IObservable<PlusCode> pluscode);
     

    }
}
