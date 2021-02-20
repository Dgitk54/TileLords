using DataModel.Common.Messages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace DataModel.Common.Tests
{
    [TestFixture]
    class MessageSerializationTests
    {

        [Test]
        public void SerializeAndDeserializeTests()
        {
            //Data:
            IMsgPackMsg playerMessage = new ContentMessage()
            {
                Type = ContentType.PLAYER,
                ResourceType = Messages.ResourceType.NONE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                Location = "testlocation",
                Name = "testname"
            };
            IMsgPackMsg resourceMessage = new ContentMessage()
            {
                Type = ContentType.RESSOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = Messages.ResourceType.APPLE,
                Location = "testlocation",
                Name = "appleTests"
            };
            IMsgPackMsg resourceMessage2 = new ContentMessage()
            {
                Type = ContentType.RESSOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = Messages.ResourceType.BANANA,
                Location = "testlocation",
                Name = "bananaTests"
            };
            IMsgPackMsg resourceMessage3 = new ContentMessage()
            {
                Type = ContentType.RESSOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = Messages.ResourceType.SHEEP,
                Location = "testlocation",
                Name = "sheep"
            };
            IMsgPackMsg batchContent = new BatchContentMessage() { ContentList = new List<IMsgPackMsg>() { resourceMessage, resourceMessage2, resourceMessage3 } };

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);



            var bin = MessagePackSerializer.Serialize(playerMessage, lz4Options);
            var value = MessagePackSerializer.Deserialize<IMsgPackMsg>(bin, lz4Options);

            var batch = MessagePackSerializer.Serialize(batchContent, lz4Options);
            var batchDeserialized = MessagePackSerializer.Deserialize<IMsgPackMsg>(batch, lz4Options);

            Assert.IsTrue(value is ContentMessage);
            var asContent = value as ContentMessage;
            Assert.IsTrue(asContent.Type == ContentType.PLAYER);
            Assert.IsTrue(batchDeserialized is BatchContentMessage);
            var asBatch = batchDeserialized as BatchContentMessage;
            Assert.IsTrue(asBatch.ContentList.Count == 3);
            
        }
    }

}
