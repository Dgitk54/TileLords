using DataModel.Common.Messages;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Services
{
    public class MapContent
    {
        public ObjectId MapContentId { get; set; }

        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] Id { get; set; }
        
        public string Name { get; set; }
        
        public string Location { get; set; }
    }
}
