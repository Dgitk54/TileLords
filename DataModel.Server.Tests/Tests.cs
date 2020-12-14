using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;

namespace ClientIntegration
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

            Assert.IsTrue(outcome.Count == 9);
            Assert.IsTrue(outcome.Contains(code));
        }
    }
}