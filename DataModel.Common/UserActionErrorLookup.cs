using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public static class UserActionErrorLookup
    {
        public static Dictionary<int, string> IntToEnglishLookup = new Dictionary<int, string>
        {
            { 1, "Could not create Account, username taken?" },
            { 2, "Logging in failed, account does not exist or the password is wrong." }
        };
    }
}
