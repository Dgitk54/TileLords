using System;
using DataModel.Client;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {

            Client.RunClientAsync().Wait();
        }
    }
}
