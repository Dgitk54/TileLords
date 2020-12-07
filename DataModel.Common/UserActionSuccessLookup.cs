using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public static class UserActionSuccessLookup
    {
        public static Dictionary<int, string> IntToMessageLookup = new Dictionary<int, string>
        {
            { 1, "User successfully registered." },
            { 2, "User successfully logged in." }
        };
    }
}
