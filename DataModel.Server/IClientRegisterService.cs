using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public interface IClientRegisterService
    {
        IObservable<IClientRegisterMessage>RegisterUser(IObservable<IClientRegisterMessage> registerRequest);
    }
}
