using DataModel.Common;
using DataModel.Common.GameModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class TownContainer
    {
        public byte[] BuilderId { get; set; }

        public Town Town { get; set; }

    }
}
