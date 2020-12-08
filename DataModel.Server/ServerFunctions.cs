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
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public static class ServerFunctions
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

        public static IObservable<T> ParseOnlyValidUsingErrorHandler<T>(IObservable<DataSourceEvent> observable, EventHandler<ErrorEventArgs> eventHandler) where T : IEvent

        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = eventHandler,
                NullValueHandling = NullValueHandling.Ignore
            };
            if (eventHandler == null)
                throw new Exception("Eventhandler is null!");

            var rawData = from e in observable
                          select e.Data;
            var parseDataIgnoringErrors = from e in rawData
                                          select JsonConvert.DeserializeObject<T>(e, settings);

            return from e in parseDataIgnoringErrors
                   where e != null
                   select e;

        }
        public static void PrintConsoleErrorHandler(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            //Console.WriteLine(currentError);
            errorArgs.ErrorContext.Handled = true;
        }

       

        public static byte[] Hash(string value, byte[] salt) => Hash(Encoding.UTF8.GetBytes(value), salt);


        public static byte[] Hash(byte[] value, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 10000);
            return pbkdf2.GetBytes(20);
        }

        public static bool PasswordMatches(byte[] password, byte[] originalPassword, byte[] originalSalt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, originalSalt, 10000);
            var result = pbkdf2.GetBytes(20);
            return result.SequenceEqual(originalPassword);
        }

    }
}
