using DataModel.Common.GameModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Tests
{
    [TestFixture]
    class ServerFunctionsTests
    {
        [Test]
        public void ToResourceDictionaryWorksWithQuests()
        {
            var questList = new List<QuestReward>()
            {
                new QuestReward() {  Amount = 5, ContentType = ContentType.QUESTREWARDPOINTS, ResourceType = ResourceType.NONE },
                new QuestReward() {  Amount = 10, ContentType = ContentType.QUESTREWARDPOINTS, ResourceType = ResourceType.NONE },
                new QuestReward() {  Amount = 3, ContentType = ContentType.QUESTREWARDPOINTS, ResourceType = ResourceType.NONE },
                new QuestReward() {  Amount = 5, ContentType = ContentType.RESOURCE, ResourceType = ResourceType.MEAT },
                new QuestReward() {  Amount = 2, ContentType = ContentType.RESOURCE, ResourceType = ResourceType.MEAT },
            };

            var questRewardItemTypeKey = new InventoryType() { ContentType = ContentType.QUESTREWARDPOINTS, ResourceType = ResourceType.NONE }; 
            var dict = questList.ToResourceDictionary();

            Assert.IsTrue(dict.Keys.Count == 2);
            Assert.IsTrue(dict[questRewardItemTypeKey] == 18);
            ;
        }
    }
}
