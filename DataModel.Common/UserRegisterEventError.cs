using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserRegisterEventError : IEvent
    {

        public string ErrorMessage { get; set; }
        public override string ToString()
        {
            return base.ToString() + "       " + ErrorMessage;
        }
    }
}
