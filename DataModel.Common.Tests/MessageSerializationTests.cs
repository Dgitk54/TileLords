using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using MessagePack;
using NUnit.Framework;
using System.Collections.Generic;

namespace DataModel.Common.Tests
{
    [TestFixture]
    class MessageSerializationTests
    {

        [Test]
        public void SerializeAndDeserializeTests()
        {
            var playerMessage = new ContentMessage()
            {
                Type = ContentType.PLAYER,
                ResourceType = ResourceType.NONE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                Location = "testlocation",
                Name = "testname"
            };
            var resourceMessage = new ContentMessage()
            {
                Type = ContentType.RESOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = ResourceType.APPLE,
                Location = "testlocation",
                Name = "appleTests"
            };
            var resourceMessage2 = new ContentMessage()
            {
                Type = ContentType.RESOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = ResourceType.BANANA,
                Location = "testlocation",
                Name = "bananaTests"
            };
            var resourceMessage3 = new ContentMessage()
            {
                Type = ContentType.RESOURCE,
                Id = new byte[6] { 5, 56, 87, 23, 64, 21 },
                ResourceType = ResourceType.SAND,
                Location = "testlocation",
                Name = "sand"
            };
            IMessage batchContent = new BatchContentMessage() { ContentList = new List<ContentMessage>() { resourceMessage, resourceMessage2, resourceMessage3 } };

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);



            
            var batch = MessagePackSerializer.Serialize(batchContent, lz4Options);
            var batchDeserialized = MessagePackSerializer.Deserialize<IMessage>(batch, lz4Options);
 
            Assert.IsTrue(batchDeserialized is BatchContentMessage);
            var asBatch = batchDeserialized as BatchContentMessage;
            Assert.IsTrue(asBatch.ContentList.Count == 3);

        }
    }

}
