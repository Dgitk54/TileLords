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


       


        public static IObservable<PlusCode> ExtractPlusCodeLocationStream(IEventBus clientBus, int precision)
        {
            var onlyValid = ParseOnlyValidUsingErrorHandler<UserGpsEvent>(clientBus.GetEventStream<DataSourceEvent>(), PrintConsoleErrorHandler);

            var onlyNonDefault = from e in onlyValid
                                 where !e.GpsData.Equals(default)
                                 select e;

            var gpsExtracted = from e in onlyNonDefault
                               select e.GpsData;

            return from e in gpsExtracted
                   select e.GetPlusCode(precision);
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

        public static MiniTile LookUpMiniTile(PlusCode code, ILiteDatabase db)
        {
            var tile = DataBaseFunctions.LookUpWithGenerateTile(code);
            var miniTile = from e in tile.MiniTiles
                           where e.MiniTileId.Code == code.Code
                           select e;
            return miniTile.First();
        }

        public static bool AddContent<T>(this MiniTile tile, T newtileContent, ILiteDatabase database) where T : ITileContent
        {
            if (newtileContent == null)
                return false;
            if (database == null)
                return false;

            var updated = tile.AddTileContent(newtileContent);
            return UpdateMiniTile(updated, database);
        }

        public static bool RemoveContent<T>(this MiniTile tile, T contentToRemove, ILiteDatabase database) where T : ITileContent
        {
            if (contentToRemove == null)
                return false;
            if (database == null)
                return false;

            var updated = tile.RemoveTileContent(contentToRemove);
            return UpdateMiniTile(updated, database);

        }


        static bool UpdateTile(Tile newTile, ILiteDatabase database)
        {
            var col = database.GetCollection<Tile>("tiles");
            col.EnsureIndex(v => v.MiniTiles);
            col.EnsureIndex(v => v.PlusCode);
            col.EnsureIndex(v => v.Ttype);
            var results = col.Find(v => v.PlusCode.Code == newTile.PlusCode.Code);
            if (results.Count() > 1)
                throw new Exception("More than one object for same index!");

            return col.Update(newTile);
        }

        static bool UpdateMiniTile(MiniTile tileWithNewValues, ILiteDatabase database)
        {
            var old = LookUpMiniTile(tileWithNewValues.MiniTileId, database);
            Debug.Assert(old.Id == tileWithNewValues.Id);
            Debug.Assert(old.MiniTileId.Code == tileWithNewValues.MiniTileId.Code);

            Tile t = DataBaseFunctions.LookUpWithGenerateTile(tileWithNewValues.MiniTileId);

            var removed = t.MiniTiles.Remove(old);
            Debug.Assert(removed);
            t.MiniTiles.Add(tileWithNewValues);

            return UpdateTile(t, database);
        }



        static T DeepClone<T>(this T obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }


        /// <summary>
        /// Creates a deep copy of the minitile with the added content.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="content"></param>
        /// <returns>Updated Minitile</returns>
        static MiniTile AddTileContent(this MiniTile tile, ITileContent content)
        {
            List<ITileContent> collection;
            if (tile.Content != null)
            {
                collection = tile.Content.Concat(new[] { content }).ToList();
            }
            else
            {
                collection = new List<ITileContent>() { content };
            }
            var copy = tile.DeepClone();
            copy.Content = collection;
            return copy;
        }

        static MiniTile RemoveTileContent(this MiniTile tile, ITileContent content)
        {
            List<ITileContent> collection;
            if (tile.Content != null)
            {
                collection = tile.Content.Except(new[] { content }).ToList();
            }
            else
            {
                collection = new List<ITileContent>();
            }
            var copy = tile.DeepClone();
            copy.Content = collection;
            return copy;

        }



    }
}
