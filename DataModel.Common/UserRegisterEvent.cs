﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserRegisterEvent : IMessage
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
