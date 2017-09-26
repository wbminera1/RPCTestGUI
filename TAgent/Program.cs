using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            Client client = new Client();
            client.Listen(9999);
            Thread.Sleep(100000);
            Console.WriteLine("Stopped");
        }
    }
}
