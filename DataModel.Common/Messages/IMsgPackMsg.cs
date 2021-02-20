using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
   
    [MessagePack.Union(0, typeof(UserGpsMessage))]
    [MessagePack.Union(1, typeof(UserActionMessage))]
    [MessagePack.Union(2, typeof(ContentMessage))]
    [MessagePack.Union(3, typeof(BatchContentMessage))]
    [MessagePack.Union(4, typeof(AccountMessage))]
    public interface IMsgPackMsg 
    {

    }
}
