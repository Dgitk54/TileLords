using System;
using System.Collections.Generic;
using System.Text;
using DataModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.PlatformServices;
using NUnit.Framework;
using Google.OpenLocationCode;
using Microsoft.Reactive.Testing;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using DataModel.Common;
using Newtonsoft.Json;
using System.IO;
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
