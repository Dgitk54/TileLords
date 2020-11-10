using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{

    public enum ResourceType
    {
        Stone,
        Wood,
        Gold,
        Silver,
        Food,
        Iron,
        Copper,
        Water

    }
    public class Resource
    {


        public readonly ResourceType rtype;


        public Resource(ResourceType r) => (rtype) = (r);

        public double ResourceType { get; }



    }
}
