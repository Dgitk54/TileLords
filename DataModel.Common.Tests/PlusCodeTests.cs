using System;
using System.Collections.Generic;
using System.Text;
using DataModel.Common;
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
using System.IO;

namespace DataModel.Common.Tests
{

    [TestFixture]
    class PlusCodeTests : ReactiveTest
    {
        IObservable<int> precisionStraem;
        IObservable<GPS> gpsStream;

        //Bingen Kreisel
        //49.944365, 7.919616
        //8FX9WWV9+PR

        //Mainz HBF
        //49.993244, 8.277976
        //8FXCX7VH+75


        //Berlin Rathaus
        //52.519126, 13.406101
        //9F4MGC94+MC


        //TH Bingen 
        //49.953111, 7.923055


        //Berg Norden von TH bingen
        //49.962096, 7.923703



        [SetUp]
        public void SetUp()
        {
        }


        [Test]
        public void GetGpsAlongCircleReturnsProperAmount()
        {
            var start = new GPS(49.000000, 7.900000);


            //radius: 0.001  => 6-8 minitiles
            var nodesAmount = 10;
            var list = DataModelFunctions.GPSNodesInCircle(start, nodesAmount, 0.001);

            Assert.IsTrue(list.Count == nodesAmount);
            list.ForEach(v =>
            {
                var dist = start.GetPlusCode(10).GetChebyshevDistance(v.GetPlusCode(10));
                //Debug.WriteLine("Distance: from" + start.GetPlusCode(10).Code + " to" + v.GetPlusCode(10).Code + "   is:  " + dist);
                Assert.IsTrue(dist > 0);
            });
        }

        [Test]
        public void GetChebyshevDistanceReturnsRightLength()
        {

            var givenOne = new PlusCode("8FX9XW2F+9X", 10);
            var givenTwo = new PlusCode("8FX9XW2F+8X", 10);
            var givenThree = new PlusCode("8FX9XW2F+7X", 10);
            var givenFour = new PlusCode("8FX9XW2F+2X", 10);


            Assert.IsTrue(givenOne.GetChebyshevDistance(givenTwo) == 1);
            Assert.IsTrue(givenOne.GetChebyshevDistance(givenThree) == 2);
            Assert.IsTrue(givenOne.GetChebyshevDistance(givenFour) == 7);

            var bug1 = new PlusCode("8FX92W22+22", 10);
            var bug2 = new PlusCode("8FW9XVXX+WJ", 10);
            var dist = bug1.GetChebyshevDistance(bug2);
            Assert.IsTrue(dist > 0);


        }

        [Test]
        public void TestGetPlusCodeReturnsCorrectString()
        {
            precisionStraem = Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; });
            gpsStream = Observable.Create<GPS>(v => { v.OnNext(new GPS(49.944365, 7.919616)); return v.OnCompleted; });

            var sub = DataModelFunctions.GetPlusCode(gpsStream, precisionStraem)
                .Do(v => { Assert.IsTrue(v.Code.Equals("8FX9WWV9+")); })
                .Subscribe();
            sub.Dispose();
        }

        [Test]
        public void GetPlusCodeReturnsRightLength()
        {
            precisionStraem = Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; });
            gpsStream = Observable.Create<GPS>(v => { v.OnNext(new GPS(49.944365, 7.919616)); return v.OnCompleted; });

            var sub = DataModelFunctions.GetPlusCode(gpsStream, precisionStraem)
                .Do(v => { Assert.IsTrue(v.Code.Length == 9 && v.Precision == 8); })
                .Subscribe();
            sub.Dispose();
        }


        [Test]
        public void GivenPlusCodeTrimWorksCorrectly()
        {
            var given = new PlusCode("8FX9XW2F+XX", 10);
            var got = DataModelFunctions.ToLowerResolution(given, 8);
            Assert.IsTrue(got.Precision == 8);
            Assert.IsTrue(got.Code.Equals("8FX9XW2F+"));
            var fromCode = new PlusCode("8FX9XW2F+", 8);
            Assert.IsTrue(got.Equals(fromCode));
        }


        [Test]
        public void WithChangedLatValuesLatDistanceGrows()
        {

            var start = new GPS(49.000000, 7.900000);
            var deltaChange = 0.000150;

            var list = DataModelFunctions.GPSNodesWithOffsets(start, deltaChange, 0, 20);

            var plusCodes = new List<PlusCode>();
            list.ForEach(v => plusCodes.Add(DataModelFunctions.GetPlusCode(v, 10)));

            PlusCode tmp = default;
            plusCodes.ForEach(v =>
            {
                if (tmp.Equals(default(PlusCode)))
                    tmp = v;
                var deltaDistance = PlusCodeUtils.GetLatitudinalTileDistance(tmp, v, true);
                Assert.IsTrue(deltaDistance >= 0);
                Debug.WriteLine(v.Code + " distance delta from " + tmp.Code + " = " + deltaDistance);
                tmp = v;
            });
        }

        [Test]
        public void WithChangedLonValuesLonDistanceGrows()
        {
            var start = new GPS(49.000000, 7.900000);
            var deltaChange = 0.000150;

            var list = DataModelFunctions.GPSNodesWithOffsets(start, 0, deltaChange, 20);
            var plusCodes = new List<PlusCode>();
            list.ForEach(v => plusCodes.Add(DataModelFunctions.GetPlusCode(v, 10)));

            PlusCode tmp = default;
            plusCodes.ForEach(v =>
            {
                if (tmp.Equals(default(PlusCode)))
                    tmp = v;
                var deltaDistance = PlusCodeUtils.GetLongitudinalTileDistance(tmp, v, true);
                Assert.IsTrue(deltaDistance >= 0);
                Debug.WriteLine(v.Code + " distance delta from " + tmp.Code + " = " + deltaDistance);
                tmp = v;
            });
        }

        [Test]
        public void WithChangedLonValuesTheLatDistanceStaysSame()
        {
            var start = new GPS(49.000000, 7.900000);
            var deltaChange = 0.000150;

            var list = DataModelFunctions.GPSNodesWithOffsets(start, 0, deltaChange, 20);
            var plusCodes = new List<PlusCode>();
            list.ForEach(v => plusCodes.Add(DataModelFunctions.GetPlusCode(v, 10)));

            PlusCode tmp = default;
            plusCodes.ForEach(v =>
            {
                if (tmp.Equals(default(PlusCode)))
                    tmp = v;
                var deltaDistance = PlusCodeUtils.GetLatitudinalTileDistance(tmp, v, true);
                Assert.IsTrue(deltaDistance == 0);
                Debug.WriteLine(v.Code + " distance delta from " + tmp.Code + " = " + deltaDistance);
                tmp = v;
            });
        }

        [Test]
        public void WithChangedLatValuesTheLonDistanceStaysSame()
        {
            var start = new GPS(49.000000, 7.900000);
            var deltaChange = 0.000150;

            var list = DataModelFunctions.GPSNodesWithOffsets(start, deltaChange, 0, 20);
            var plusCodes = new List<PlusCode>();
            list.ForEach(v => plusCodes.Add(DataModelFunctions.GetPlusCode(v, 10)));

            PlusCode tmp = default;
            plusCodes.ForEach(v =>
            {
                if (tmp.Equals(default(PlusCode)))
                    tmp = v;
                var deltaDistance = PlusCodeUtils.GetLongitudinalTileDistance(tmp, v, true);
                Assert.IsTrue(deltaDistance == 0);
                Debug.WriteLine(v.Code + " distance delta from " + tmp.Code + " = " + deltaDistance);
                tmp = v;
            });
        }


        [Test]
        public void GetPlusCodeCapableOfMultipleGpsInputs()
        {

            List<GPS> gpsList = new List<GPS>();
            gpsList.Add(new GPS(49.944365, 7.919616));
            gpsList.Add(new GPS(52.519126, 13.406101));

            List<int> intList = new List<int>();
            intList.Add(8);


            var sub = DataModelFunctions.GetPlusCode(gpsList.ToObservable(), intList.ToObservable());

            int count = 0;
            List<PlusCode> codes = new List<PlusCode>();


            sub.Subscribe(
                v =>
                {

                    count = count + 1;
                    codes.Add(v);



                },
                () =>
                {

                    //   Debug.WriteLine("GetPlusCodeCapableOfMultipleGpsInputs: " + codes[0].Code);
                    //   Debug.WriteLine("GetPlusCodeCapableOfMultipleGpsInputs: " + codes[1].Code);
                    Assert.IsTrue(count == 2);
                    Assert.IsTrue(codes[0].Code.Equals("8FX9WWV9+"));
                    Assert.IsTrue(codes[1].Code.Equals("9F4MGC94+"));

                })

                .Dispose();
        }




        [Test]
        public void PlusCodePrintingTest()
        {
            String all = "23456789CFGHJMPQRVWX";
            String rev = "XWVRQPMJHGFC98765432";

            var sum = from c1 in all
                      from c2 in rev
                      select new string(c1 + "" + c2 + "");

            List<GPS> gpsList = new List<GPS>();
            gpsList.Add(new GPS(49.953111, 7.923055));
            gpsList.Add(new GPS(49.962096, 7.923703));

            List<int> intList = new List<int>();
            intList.Add(8);


            var sub = DataModelFunctions.GetPlusCode(gpsList.ToObservable(), intList.ToObservable());

            int count = 0;
            List<PlusCode> codes = new List<PlusCode>();


            sub.Subscribe(
                v =>
                {

                    count = count + 1;
                    codes.Add(v);



                },
                () =>
                {

                    Debug.WriteLine("PlusCodePrintingTest: " + codes[0].Code);
                    Debug.WriteLine("PlusCodePrintingTest: " + codes[1].Code);


                })

                .Dispose();
        }


        [Test]
        public void PlusCodeToTownNameTest()
        {

            string townName = TileUtility.PlusCodeToTileName(new PlusCode("8FX9XW2F+XX", 10));
            Assert.IsTrue(townName != null);
            Debug.WriteLine(townName);

      
        }
    }
}