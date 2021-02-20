using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class ContentMessage : IMsgPackMsg
    {
        [Key(0)]
        public ContentType Type { get; set; }
        [Key(1)]
        public ResourceType ResourceType { get; set; }
        [Key(2)]
        public byte[] Id { get; set; }
        [Key(3)]
        public string Name { get; set; }
        [Key(4)]
        public string Location { get; set; }
    }
}
