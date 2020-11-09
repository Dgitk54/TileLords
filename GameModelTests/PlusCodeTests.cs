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

namespace DataModelTests
{

    [TestFixture]
    class PlusCodeTests : ReactiveTest
    {
        IObservable<int> precisionStraem;
        IObservable<GPS> gpsStream;
        DataModelFunctions func;

        //Bingen Kreisel
        //49.944365, 7.919616
        //8FX9WWV9+PR

        //Mainz HBF
        //49.993244, 8.277976
        //8FXCX7VH+75


        //Berlin Rathaus
        //52.519126, 13.406101
        //9F4MGC94+MC

        [SetUp]
        public void SetUp()
        {
            func = new DataModelFunctions();
        }

        [Test]
        public void TestGetPlusCodeReturnsCorrectString()
        {
            precisionStraem = Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; });
            gpsStream = Observable.Create<GPS>(v => { v.OnNext(new GPS(49.944365, 7.919616)); return v.OnCompleted; });

            var sub = func.GetPlusCode(gpsStream, precisionStraem)
                .Do(v => { Assert.IsTrue(v.Code.Equals("8FX9WWV9+")); })
                .Subscribe();
            sub.Dispose();
        }

        [Test]
        public void GetPlusCodeReturnsRightLength()
        {
            precisionStraem = Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; });
            gpsStream = Observable.Create<GPS>(v => { v.OnNext(new GPS(49.944365, 7.919616)); return v.OnCompleted; });

            var sub = func.GetPlusCode(gpsStream, precisionStraem)
                .Do(v => { Assert.IsTrue(v.Code.Length == 9 && v.Precision == 8); })
                .Subscribe();
            sub.Dispose();
        }
        [Test]
        public void GetPlusCodeCapableOfMultipleGpsInputs()
        {
            precisionStraem = Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; });
            gpsStream = Observable.Create<GPS>(v => 
            {
                v.OnNext(new GPS(49.944365, 7.919616));
                v.OnNext(new GPS(52.519126, 13.406101));
                return v.OnCompleted;
            });
            var sub = func.GetPlusCode(gpsStream, precisionStraem)
                .Do(v => 
                {
                    Assert.IsTrue(v.Code.Length == 9 && v.Precision == 8);
                    Assert.IsTrue(v.Code.Equals("8FX9WWV9+"));
                })
                .Do(v =>
                {
                    Assert.IsTrue(v.Code.Length == 9 && v.Precision == 8);
                    Assert.IsTrue(v.Code.Equals("9F4MGC94+"));
                })
                .Subscribe();
            sub.Dispose();
        }
    }
}