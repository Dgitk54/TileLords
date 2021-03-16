using MessagePack;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{
    public class BatchContentMessage : IMessage
    {
        public List<ContentMessage> ContentList { get; set; }
        public override string ToString()
        {
            string s = "";
            foreach (var content in ContentList)
            {
                s += (content.ToString() + " ");
            }
            return s;
        }
    }
}
