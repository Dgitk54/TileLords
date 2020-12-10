using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class Player : ITileContent
    {
        public string Name { get; set; }
        
        public PlusCode Location { get; set; }
    }
}
