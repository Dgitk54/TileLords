using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Services;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
namespace DataModel.Server.Tests
{
    [TestFixture]
    public class ResourceSpawnTests
    {
        [SetUp]
        public void Setup()
        {
            if (File.Exists(@"MyData.db"))
                File.Delete(@"MyData.db");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(@"MyData.db"))
            {
                File.Delete(@"MyData.db");
            }
        }

        [Test]
        public void SpawnsResource()
        {

            var a = new byte[] { 1, 2, 3, 4, 5 };
            var b = new byte[] { 1, 2, 3, 4, 5 };
            var c = new byte[] { 1, 2, 3, 4, 6 };
            var equals = a == b;
            var equalsnot = b == c;
            ;
        }
    }
}
