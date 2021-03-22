using DataModel.Server.Model;
using DataModel.Server.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class UserServiceTests
    {

        [SetUp]
        public void Setup()
        {
            LiteDBDatabaseFunctions.WipeAllDatabases();
            LiteDBDatabaseFunctions.InitializeDataBases();
        }

        [TearDown]
        public void TearDown()
        {
            LiteDBDatabaseFunctions.WipeAllDatabases();
        }

        [Test]
        public void CanRegisterAndLogIn()
        {
            //Setup:
            var service = new UserAccountService(LiteDBDatabaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            var registerResults = new List<bool>();
            var userLoginResults = new List<IUser>();


            //Register same username, expect error
            service.RegisterUser("hans", "hans").Subscribe(v => registerResults.Add(v));
            service.RegisterUser("hans", "hans2")
                .Catch<bool, Exception>(tx =>
                 {
                     return Observable.Return(false);
                 })
                .Subscribe(v => registerResults.Add(v));
            Assert.IsTrue(registerResults.Count == 2);
            Assert.IsTrue(registerResults[0] == true);
            Assert.IsTrue(registerResults[1] == false);


            //Login user
            service.LoginUser("hans", "hans").Subscribe(v => userLoginResults.Add(v));
            Assert.IsTrue(userLoginResults.Count == 1);

            //Login call again, should fail because user is already online:
            service.LoginUser("hans", "hans")
                .Catch<IUser, Exception>(tx =>
                {
                    return Observable.Empty<IUser>();
                })
                .Subscribe(v => userLoginResults.Add(v));
            Assert.IsTrue(userLoginResults.Count == 1);


            //Register 2nd User and log in:
            service.RegisterUser("hans2", "hans2").Subscribe(v => registerResults.Add(v));
            service.LoginUser("hans2", "hans2").Subscribe(v => userLoginResults.Add(v));




            //Test Log out:
            bool couldLogOut = false;
            Assert.IsTrue(couldLogOut);


        }



    }
}
