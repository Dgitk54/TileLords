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

            List<GPS> gpsList = new List<GPS>();
            gpsList.Add(new GPS(49.944365, 7.919616));
            gpsList.Add(new GPS(52.519126, 13.406101));

            List<int> intList = new List<int>();
            intList.Add(8);


            var sub = func.GetPlusCode(gpsList.ToObservable(), intList.ToObservable());

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


        //TH Bingen 
        //49.953111, 7.923055


        //Berg Norden von TH bingen
        //49.962096, 7.923703

        [Test]
        public void PlusCodePrintingTest()
        {

            List<GPS> gpsList = new List<GPS>();
            gpsList.Add(new GPS(49.953111, 7.923055));
            gpsList.Add(new GPS(49.962096, 7.923703));

            List<int> intList = new List<int>();
            intList.Add(8);


            var sub = func.GetPlusCode(gpsList.ToObservable(), intList.ToObservable());

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
    }
}