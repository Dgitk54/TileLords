using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    readonly public struct PlusCode
    {
        public string Code { get; }
        public int Precision { get; }

        public PlusCode(string code, int precision) => (Code, Precision) = (code, precision);
    }
}
