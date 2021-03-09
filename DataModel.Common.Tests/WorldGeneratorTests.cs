using NUnit.Framework;
namespace DataModel.Common.Tests
{
    public class WorldGeneratorTests
    {

        [Test]
        public void WorldGeneratorCreatesTiles()
        {
            var code = new PlusCode("8FX9WWV9+", 8);
            var tile = WorldGenerator.GenerateTile(code);
            Assert.IsTrue(tile.MiniTiles.Count == 400);
        }

    }
}
