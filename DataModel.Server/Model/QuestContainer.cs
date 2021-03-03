using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class QuestContainer
    {
        public byte[] OwnerId { get; set; }
        public byte[] QuestId { get; set; }
        public Quest Quest { get; set; }
    }
}
