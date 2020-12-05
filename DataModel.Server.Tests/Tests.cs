using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void GivenPlusCodeNeighborsIn8Works()
        {
            var code = new PlusCode("8FX9WWV9+", 8);
            var outcome = ServerFunctions.NeighborsIn8(code);

            Assert.IsTrue(outcome.Count == 8);
            Assert.IsFalse(outcome.Contains(code));
        }
    }
}