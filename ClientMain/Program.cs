using System;
using DataModel.Client;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {
            var instance = new ClientInstance();
            instance.RunClientAsync().Wait();
        }
    }
}
