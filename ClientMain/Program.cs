using System;
using DataModel.Client;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {

            ClientInstance.RunClientAsync().Wait();
        }
    }
}
