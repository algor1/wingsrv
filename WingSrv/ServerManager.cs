

using System.Threading;
using System.Threading.Tasks;

namespace Wingsrv
{


    public class ServerManager
    {
        public  Server server;
        public  ServerDB serverDB;
        public  InventoryServer inventoryServer;
        public  ItemDB itemDB;

        public  void Run()
        {
            itemDB = new ItemDB(this);
            inventoryServer = new InventoryServer(this);
            serverDB = new ServerDB(this);
            server = new Server(this);
            Thread myThread = new Thread(new ThreadStart(server.RunServer));
            myThread.Start();
            Thread.Sleep(7000);
            server.PlayerControlSetTarget(0, Command.SetTarget, 1);
            //server.RunServer();

        }
        public void Stop()
        {
            itemDB.OnApplicationQuit();
        }

    }
}
