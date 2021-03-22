using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class DatabaseMapStorage
    {

        public string Location { get; set; }
        
        public byte[] MapContentObject { get; set; }

        /// <summary>
        /// Returns a DatabaseMapStorage given a redis value string.
        /// </summary>
        /// <param name="redisStoreValue"></param>
        /// <returns></returns>
        public static DatabaseMapStorage FromRedisValue(string redisStoreValue)
        {
            var index = redisStoreValue.IndexOf('\n');
            if (index == -1)
                return null;
            var location = redisStoreValue.Substring(0, index);
            var encodedMapString = redisStoreValue.Substring(index);
            var mapContentRaw = Convert.FromBase64String(encodedMapString);

            return new DatabaseMapStorage() { Location = location, MapContentObject = mapContentRaw };
        }

        /// <summary>
        /// Encodes a location and a serialized mapcontent object into a base64 string.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="mapObject"></param>
        /// <returns></returns>
        public static string ToDatabaseString(string location, byte[] mapObject)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(location).Append('\n').Append(Convert.ToBase64String(mapObject));
            return stringBuilder.ToString();
        }

    }
}
