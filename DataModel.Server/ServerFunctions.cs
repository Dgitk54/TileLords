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
using DataModel.Common.Messages;
using MessagePack;
using DataModel.Server.Services;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public static class ServerFunctions
    {

        public static string BiomeConfigs
        {
            get
            {
                return System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + @"\DataModel.Common\BiomeConfigs\";
            }
        }
        public readonly static int CLIENTVISIBILITY = 10;
        public readonly static int CLIENTLOCATIONPRECISION = 10;
        
        public static IDisposable DebugEventToConsoleSink<T>(IObservable<T> events) where T : IMessage
            => events.Subscribe(v => Console.WriteLine("Event occured:" + v.ToString()));

        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : IMsgPackMsg
            => objStream.Subscribe(v =>
            {
                var data = MessagePackSerializer.Serialize(v);
                Console.WriteLine("PUSHING: DATA" + data.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(data));
            },
             e => Console.WriteLine("Error occured writing" + objStream),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));

        public static MapContent AsMapContent(this IUser user)
        {
            return new MapContent() { Id = user.UserId, Name = user.UserName, ResourceType = Common.Messages.ResourceType.NONE, Type = ContentType.PLAYER, Location = null, MapContentId = null };
        }

        public static IMsgPackMsg AsMessage(this MapContent content)
        => new ContentMessage() { Id = content.Id, Location = content.Location, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };

        public static IObservable<PlusCode> ExtractPlusCodeLocationStream(IMessageBus clientBus, int precision)
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

        public static IObservable<T> ParseOnlyValidUsingErrorHandler<T>(IObservable<DataSourceEvent> observable, EventHandler<ErrorEventArgs> eventHandler) where T : IMessage

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
