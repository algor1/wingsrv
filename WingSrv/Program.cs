using System;

namespace Wingsrv
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ServerManager serverManager = new ServerManager();
            serverManager.Run();
        }
    }
}
