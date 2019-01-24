using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using LoginPlugin;

namespace Wingsrv
{
    public enum Command { MoveTo, WarpTo, Atack, SetTarget, LandTo, Equipment, Open, TakeOff };
    public enum TypeSO { asteroid, ship, station, waypoint, container };

    public class Server
    {

        public bool started;
        private ConcurrentDictionary<int, Ship> ships;
        private Dictionary<IClient, int> playerShip;


        public delegate void TickHandler();
        public event TickHandler onTick;
        private ServerDB serverDB;
        public InventoryServer inventoryServer;
        private ServerManager serverManager;
        public int TickDeltaTime = 20;
        //private Login _loginPlugin;



        public Server(ServerManager manager)
        {
            serverManager = manager;
        }
        public void RunServer()
        {
            if (!started)
            {
                this.onTick += new TickHandler(Tick);
                ships = new ConcurrentDictionary<int, Ship>();
                Console.WriteLine("Starting DB server...");
                serverDB = serverManager.serverDB;
                Console.WriteLine("Starting inventory server...");
                inventoryServer = serverManager.inventoryServer;
                Console.WriteLine("loading ships...");
                LoadShips();
                Console.WriteLine("Starting server...");
                playerShip = new Dictionary<IClient, int>();

                Run();
            }

        }

        private void Run()
        {
            Console.WriteLine(" Server started.");
            started = true;
            SendNearest();
            while (started)
            {
                onTick();

                Task.Delay(TickDeltaTime); //await?????????????
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
        public bool LoadPlayer(string player)
        {
            playerShip.Add(player, 0);
            return true;
        }
        public bool RemovePlayer(IClient player)
        {
            Task.Delay(1000);
            return playerShip.Remove(player);
        }
        private void SendNearest()
        {
            while (started)
            {
               //serverManager.WriteEvent("Send Nearest.", LogType.Info);

                foreach (KeyValuePair<IClient, int> entry in playerShip)
                {
                    using (var writer = DarkRiftWriter.Create())
                    {

                        foreach (Ship ship in Nearest(entry.Value))
                        {
                            writer.Write(ship.p);
                        }
                        using (var msg = Message.Create(Game.NearestSpaceObjects, writer))
                        {
                            entry.Key.SendMessage(msg, SendMode.Reliable);
                        }
                    }
                }
                Task.Delay(1000);
            }
        } 




        #endregion


        #region ShipList

        private List<Ship> Nearest(int ship_id)
                
        {
            Ship _playerShip = GetShip(ship_id);
            List<Ship> resultShipList = new List<Ship>();
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i].p.Id == ship_id || ships[i].p.Hidden) continue;//remove player from list
                if (ships[i].p.Id == ship_id || ships[i].p.Destroyed) continue;//remove player from list

                float dist = Vector3.Distance(_playerShip.p.Position, ships[i].p.Position);
                //          print (dist);
                if (dist < _playerShip.p.VisionDistance)
                {
                    resultShipList.Add(ships[i]);
                }

            }
            return resultShipList;
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
        public void PlayerControlSetTarget(int player_id, Command player_command, int target_id)
        {
            Ship player = GetShip(player_id);
            Ship target = GetShip(target_id);
            switch (player_command)
            {
                case Command.SetTarget:
                    player.SetTarget(target.p);
                    break;
                case Command.WarpTo:
                    player.WarpToTarget();
                    break;
                case Command.MoveTo:
                    player.GoToTarget();
                    break;
            }

        }
        public Ship GetShip(int player_id)
        {
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i].p.Id == player_id) return ships[i];
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
            
        }
        private void ShipLand(object sender, LandEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  landed ", e.ship_id);
        }
        private void ShipDestroy(object sender, DestroyEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  landed ", e.ship_id);
        }
        #endregion
    }
    
}
