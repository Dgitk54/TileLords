using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.GameModel
{
    public class Town
    {
        public byte[] TownId { get; set; }
        public ContentType TownContentType { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int TownDecayState { get; set; }
    }
}
