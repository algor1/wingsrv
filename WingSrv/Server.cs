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
    public enum Command { MoveTo, WarpTo, Atack, SetTarget, LandTo, Equipment, Open, TakeOff };

    public class Server
    {

        public bool started;
        private ConcurrentDictionary<int, Ship> ships;
        public Dictionary<string, int> playerShip;


        public delegate void TickHandler();
        public event TickHandler onTick;
        private ServerDB serverDB;
        private InventoryServer inventoryServer;
        //private ServerManager serverManager;
        public int TickDeltaTime = 20;
        private Game gamePlugin;
        public Login _loginPlugin;



        public Server(Game game)
        {
            gamePlugin = game;

            //_loginPlugin = game.PluginManager.GetPluginByType<Login>();

        }
        public void RunServer()
        {
            if (!started)
            {
                this.onTick += new TickHandler(Tick);
                ships = new ConcurrentDictionary<int, Ship>();
                Console.WriteLine("Starting DB server...");
                serverDB = gamePlugin.serverDB;
                Console.WriteLine("Starting inventory server...");
                inventoryServer = gamePlugin.inventoryServer;
                Console.WriteLine("loading ships...");
                LoadShips();
                Console.WriteLine("Starting server...");
                playerShip = new Dictionary<string, int>();
                //Thread myThread = new Thread(new ThreadStart(Run));
                //myThread.Start();
                //Console.WriteLine(myThread.IsBackground);

                RunTick();
                SendNearest();

            }

        }

        private async  Task RunTick()
        {
            gamePlugin.WriteToLog(" Server started.", DarkRift.LogType.Info);
            started = true;
            //SendNearest();
            while (started)
            {
                onTick();

                await Task.Delay(TickDeltaTime); 
                //Console.WriteLine("tick {0}", DateTime.Now);
            }
        }
        private void Tick()
        {
            for (int i = 0; i < ships.Count; i++)
            {
                //Console.WriteLine("Ship tick {0}",i);
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
                foreach (KeyValuePair<string, int> entry in playerShip)
                {
                    using (var writer = DarkRiftWriter.Create())
                    {
                        writer.Write(Nearest(entry.Value));

                        //Console.WriteLine("sending {0} bytes to player {1}",writer.Length,entry.Key);

                        using (var msg = Message.Create(Game.NearestShips, writer))
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

        private ShipData[] Nearest(int ship_id)
                
        {
            Ship _playerShip = GetShip(ship_id);
            List<ShipData> resultShipList = new List<ShipData>();
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i].p.Id == ship_id || ships[i].p.Hidden) continue;//remove player from list
                if (ships[i].p.Id == ship_id || ships[i].p.Destroyed) continue;//remove player from list

                float dist = Vector3.Distance(_playerShip.p.Position, ships[i].p.Position);
                //          print (dist);
                if (dist < _playerShip.p.VisionDistance)
                {
                    resultShipList.Add(ships[i].p);
                }

            }
            return resultShipList.ToArray();
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
            Ship _target = target_id!=-1? ships [target_id]: null;
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
