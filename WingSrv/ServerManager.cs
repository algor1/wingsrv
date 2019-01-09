

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
            server.RunServer();

        }
        public void Stop()
        {
            itemDB.OnApplicationQuit();
        }

    }
}
