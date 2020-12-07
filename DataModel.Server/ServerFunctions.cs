using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using Google.OpenLocationCode;
using DotNetty.Transport.Channels;
using System.Diagnostics;
using DotNetty.Buffers;
using LiteDB;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public class ServerFunctions
    {



        public static IDisposable DebugEventToConsoleSink<T>(IObservable<T> events) where T : IEvent
            => events.Subscribe(v => Console.WriteLine("Event occured:" + v.ToString()));

        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : DataSinkEvent
            => objStream.Subscribe(v =>
            {
                var asByteMessage = Encoding.UTF8.GetBytes(v.SerializedData);
                Console.WriteLine("PUSHING: DATA" + asByteMessage.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(asByteMessage));
            },
             e => Console.WriteLine("Error occured writing" + objStream),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));


        //TODO: proper reactive stream?
        public static Tile LookUp(PlusCode code, ILiteDatabase db)
        {
            var largeCode = code;
            if (largeCode.Precision == 10)
                DataModelFunctions.ToLowerResolution(code, 8);

            var col = db.GetCollection<Tile>("tiles");
            col.EnsureIndex(v => v.MiniTiles);
            col.EnsureIndex(v => v.PlusCode);
            col.EnsureIndex(v => v.Ttype);
            var results = col.Find(v => v.PlusCode.Code == code.Code);
            if (results.Count() == 0)
            {
                ;
                var created = TileGenerator.GenerateArea(largeCode, 0);
                var tile = created[0];

                var dbVal = col.Insert(tile);
                tile.Id = dbVal.AsInt32;
                return tile;
            }
            if (results.Count() > 1)
                throw new Exception("More than one object for same index!");
            return results.First();
        }

        public static List<PlusCode> NeighborsIn8(PlusCode code)
        {
            var strings = LocationCodeTileUtility.GetTileSection(code.Code, 1, code.Precision);
            return strings.Select(v => new PlusCode(v, code.Precision)).ToList();
        }

    }
}
