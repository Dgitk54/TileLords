using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{

    public class Resource : ITileContent
    {
        public readonly ResourceType rtype;

        public Resource(ResourceType r) => (rtype) = (r);

        public double ResourceType { get; }

    }
}
