using System;

namespace Wingsrv
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Server server = new Server();
            server.RunServer();
        }
    }
}
