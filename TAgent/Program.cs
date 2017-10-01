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
        public Program()
        {

        }

        public void Run()
        {
            Client client = new Client(OnReceived);
            client.Listen(9999);
            client.WaitForConnection();
            client.Send(Encoding.ASCII.GetBytes("Hello"));
            client.WaitForEnd();
        }
        public void OnReceived(byte[] data)
        {
            Console.WriteLine("OnReceived " + data.Length + " bytes");
        }
        static void Main(string[] args)
        {
            Console.WriteLine("TAgent started");
            var prog = new Program();
            prog.Run();
            Console.WriteLine("TAgent stopped");
        }
    }
}
