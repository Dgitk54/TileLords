using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public PlusCode LastPostion { get; set; }

        public List<int> Inventory { get; set; }

        public DateTime LastOnline { get; set; }

        public DateTime AccountCreated { get; set; }

        public byte[] Salt { get; set; }

        public byte[] SaltedHash { get; set; }
    }
}
