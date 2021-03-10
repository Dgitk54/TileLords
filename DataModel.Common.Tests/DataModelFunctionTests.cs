using DataModel.Common.GameModel;
using NUnit.Framework;
using System.Collections.Generic;

namespace DataModel.Common.Tests
{
    [TestFixture]
    class DataModelFunctionTests
    {

        [Test]
        public void DictionarySubtractionFindsKeys()
        {
            var key = new InventoryType() { ContentType = ContentType.RESOURCE, ResourceType = ResourceType.AMETHYST };

            Dictionary<InventoryType, int> one = new Dictionary<InventoryType, int>()
            {
                { key, 5 }
            };
            Dictionary<InventoryType, int> two = new Dictionary<InventoryType, int>()
            {
                { key, 3 }
            };

            var subtractionResult = one.SubtractDictionaries(two);
            Assert.IsTrue(subtractionResult.Keys.Count == 1);
            Assert.IsTrue(subtractionResult[key] == 2);
        }
    }
}
