using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    interface ITileHandler
    {
        IObservable<Tile> LookUpTiles(IObservable<PlusCode> pluscode, IObservable<int> playerID);
        IObservable<Tile> RequestTiles(IObservable<PlusCode> pluscode);
        IObservable<Tile> UpdateTiles(IObservable<PlusCode> pluscode, IObservable<Tile> tile);
        IObservable<Tile> RequestTileUIUpdate();



    }
}
