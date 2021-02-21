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
        
        public static MapContent AsMapContent(this IUser user)
        {
            return new MapContent() { Id = user.UserId, Name = user.UserName, ResourceType = Common.Messages.ResourceType.NONE, Type = ContentType.PLAYER, Location = null, MapContentId = null };
        }

        public static IMsgPackMsg AsMessage(this MapContent content)
        => new ContentMessage() { Id = content.Id, Location = content.Location, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };

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
