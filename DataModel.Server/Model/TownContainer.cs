﻿using DataModel.Common.GameModel;

namespace DataModel.Server.Model
{
    public class TownContainer
    {
        public byte[] BuilderId { get; set; }

        public Town Town { get; set; }

    }
}