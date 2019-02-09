using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using LoginPlugin;
using SpaceObjects;

namespace Wingsrv
{


    public class ServerSO
    {

        public bool started;
        private Dictionary<int, SpaceObject> spaceObjects;


        private Server server;
        private ServerDB serverDB;

        private InventoryServer inventoryServer;
        //private ServerManager serverManager;
        public int TickDeltaTime = 2000;
        private Game gamePlugin;
        public Login _loginPlugin;



        public ServerSO(Game game)
        {
            gamePlugin = game;

            //_loginPlugin = game.PluginManager.GetPluginByType<Login>();

        }
        public void RunServer()
        {
            if (!started)
            {
                spaceObjects = new Dictionary<int, SpaceObject>();
                server = gamePlugin.server;
                serverDB = gamePlugin.serverDB;
                inventoryServer = gamePlugin.inventoryServer;
                Console.WriteLine("loading Space objects...");
                LoadserverObjects();
                Console.WriteLine("Starting server...");
                //Thread myThread = new Thread(new ThreadStart(Run));
                //myThread.Start();
                //Console.WriteLine(myThread.IsBackground);

                
                SendNearest();

            }

        }

        private void LoadserverObjects()
        {
            List<SpaceObject> SOList = serverDB.GetAllSO();
            Debug.Log("all SO count" + SOList.Count);
            for (int i = 0; i < SOList.Count; i++)
            {
                if (SOList[i].Type != TypeSO.ship)
                {
                    Debug.Log("SO " + i);

                    SpaceObject s = new SpaceObject(SOList[i]);
                    spaceObjects.Add(s.Id,s);
                }
            }
        }




        #region PlayerList
        public void LoadPlayer(string player)
        {
            int ship_id = 0;// 0 - ship id temporary TODO
            playerShip.Add(player, ship_id);
            SendPlayerShipData(player);

            //return true;
        }
        public void RemovePlayer(string player)
        {
            if (playerShip.ContainsKey(player))
            {
                playerShip.Remove(player);
                gamePlugin.WriteToLog("player " + player + " removed", DarkRift.LogType.Info);
            }
        }
        private async Task SendNearest()
        {
            while (started)
            {
                //serverManager.WriteEvent("Send Nearest.", LogType.Info);
                //Console.WriteLine("trying to send nearest");
                foreach (KeyValuePair<string, int> entry in server.playerShip)
                {
                    using (var writer = DarkRiftWriter.Create())
                    {
                        writer.Write(Nearest(entry.Value));

                        //Console.WriteLine("sending {0} bytes to player {1}",writer.Length,entry.Key);

                        using (var msg = Message.Create(Game.NearestSpaceObjects, writer))
                        {
                            //Console.WriteLine("sending message tag {0} of {1} bytes to player {2}", msg.Tag,msg.DataLength,entry.Key);
                            //Console.WriteLine(_loginPlugin);
                            //Console.WriteLine(_loginPlugin.Clients.Count);
                            //Console.WriteLine(_loginPlugin.UsersLoggedIn.Count);

                            IClient cl = _loginPlugin.Clients[entry.Key];
                            //Console.WriteLine("client id {0}",cl.ID);

                            cl.SendMessage(msg, SendMode.Unreliable);
                        }
                        //Console.WriteLine("sended {0} nearest ships to player {1}", Nearest(entry.Value).Length, entry.Key);

                    }
                }
                await Task.Delay(3000);
            }
        }
        private void SendPlayerShipData(string _player)
        {

            Ship ship = ships[playerShip[_player]];
            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(ship.p);


                using (var msg = Message.Create(Game.PlayerShipData, writer))
                {
                    Console.WriteLine("sending message tag {0} of {1} bytes to player {2}", msg.Tag,msg.DataLength,_player);
                    Console.WriteLine(_loginPlugin);
                    Console.WriteLine(_loginPlugin.Clients.Count);
                    Console.WriteLine(_loginPlugin.UsersLoggedIn.Count);

                    IClient cl = _loginPlugin.Clients[_player];
                    //Console.WriteLine("client id {0}",cl.ID);

                    cl.SendMessage(msg, SendMode.Unreliable);
                }
                //Console.WriteLine("sended {0} nearest ships to player {1}", Nearest(entry.Value).Length, entry.Key);

            }

        }



        #endregion


        #region ShipList

        private SpaceObject[] Nearest(int ship_id)
                
        {
            Ship _playerShip = server.GetShip(ship_id);
            List<SpaceObject> resultSOList = new List<SpaceObject>();
            foreach (KeyValuePair<int,SpaceObject> entry in spaceObjects)
            {

                float dist = Vector3.Distance(_playerShip.p.Position, entry.Value.Position);
                //          print (dist);
                if (dist < _playerShip.p.VisionDistance)
                {
                    resultSOList.Add(entry.Value);
                }

            }
            return resultSOList.ToArray();
        }


        private void AddShip(ShipData ship)
        {
            if (!ships.ContainsKey(ship.Id)){
                Ship s = new Ship(ship);
                Console.WriteLine("added {0} id {1} position {2} , rotation {3}", s.p.Type, s.p.Id, s.p.Position, s.p.Rotation);

                ships.TryAdd(s.p.Id,s);
                onTick += s.Tick;
                
            }
            else
            {
                //TODO Log wrong ship adding
            }
        }
        private void DeleteShip(ShipData ship) { }

        private void LoadShips()
        {
            List<ShipData> shipList = serverDB.GetAllShips();
            
            for (int i = 0; i < shipList.Count; i++)
            {
                AddShip(shipList[i]);
            }
        }

        #endregion

        #region user commands
        public void GetPlayerShipCommand(string player, Command player_command, int target_id,int point_id)
        {
            Ship _playerShip = ships [playerShip[player]];
            Ship _target = ships [target_id];
            switch (player_command)
            {
                case Command.SetTarget:
                    _playerShip.SetTarget(_target.p);
                    break;
                case Command.WarpTo:
                    _playerShip.WarpToTarget();
                    break;
                case Command.MoveTo:
                    _playerShip.GoToTarget();
                    break;
            }

        }
        public Ship GetShip(int ship_id)
        {
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i].p.Id == ship_id) return ships[i];
            }
            return ships[0];

        }

        #endregion

        #region events



        private void EventSigner(Ship ship)
        {
            this.onTick += new TickHandler(ship.Tick);//избавится от делегата
            ship.ShipLanded += ShipLand;
            ship.ShipDestroyed += ShipDestroy;
            ship.ShipSpawn += ShipSpawn;
            
        }
        private void ShipLand(object sender, LandEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  landed ", e.ship_id);
        }
        private void ShipDestroy(object sender, DestroyEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  landed ", e.ship_id);
        }
         private void ShipSpawn(object sender, SpawnEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  spawn ", e.ship_id);
        }
        #endregion
    }
    
}
