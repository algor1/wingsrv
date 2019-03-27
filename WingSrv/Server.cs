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
using Database;
using System.Linq;

namespace Wingsrv
{
    //public enum Command { MoveTo, WarpTo, Atack, SetTarget, SetTargetShip, LandTo, Equipment, Open, TakeOff };

    public class Server
    {

        public bool started;
        private ConcurrentDictionary<int, Ship> ships;
        //private Dictionary<int, SpaceObject> spaseObjects;
        private Dictionary<string, int> playerShip;
        private Dictionary<int,string> playerShipInverse;
        private Dictionary<string,int> playerIds;

        public delegate void TickHandler();
        public event TickHandler onTick;
        //private ServerDB serverDB;
        private InventoryServer inventoryServer;
        //private ServerManager serverManager;
        public int TickDeltaTime = 20;
        private Game gamePlugin;
        public Login _loginPlugin {get;set;}
        public  DatabaseProxy _database{get;set;}




        public Server(Game game)
        {
            gamePlugin = game;


        

        }
        public void RunServer()
        {
            if (!started)
            {
                this.onTick += new TickHandler(Tick);
                ships = new ConcurrentDictionary<int, Ship>();
                //serverDB = gamePlugin.serverDB;
                inventoryServer = gamePlugin.inventoryServer;
                Console.WriteLine("loading ships...");
                LoadShips();
                Console.WriteLine("Starting server...");
                playerShip = new Dictionary<string, int>();
                playerShipInverse = new Dictionary<int,string >();
                playerIds = new Dictionary<string, int>();
                //Thread myThread = new Thread(new ThreadStart(Run));
                //myThread.Start();
                //Console.WriteLine(myThread.IsBackground);
                started = true;

                RunTick();
                SendNearest();

            }

        }

        private async Task RunTick()
        {
            gamePlugin.WriteToLog(" Server started.", DarkRift.LogType.Info);
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
        
        public void AddNewPlayer(string player)
        {
            try
            {
                _database.DataLayer.AddNewPlayerAndShip(player,()  =>
               {
                   //LoadPlayer(player);
               });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on adding player" +ex, DarkRift.LogType.Error);
                _database.DatabaseError(null , 0 , ex);
            }
        }

        public void LoadPlayer(string player)
        {
            Console.WriteLine("Loading player {0}", player);

            try
            {
                _database.DataLayer.GetPlayerId( player, id =>
                    {
                        playerIds.Add(player,id);
                    });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading player " +player +"  "+ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null , 0 , ex);
            }

            try
            {
                _database.DataLayer.GetPlayerActiveShip(player, shipId =>
               {
                   _database.DataLayer.GetShip(shipId, ship =>
                    {
                        AddShip(ship);
                        playerShip.Add(player, shipId);
                        playerShipInverse.Add(shipId, player);
                        SendPlayerShipData(player);
                        //inventoryServer.LoadPlayerInventory(GetPlayerId(player));
                        gamePlugin.WriteToLog("player " + player + " loaded. Ship id: " + shipId, DarkRift.LogType.Info);
                    });
               });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading player" +ex, DarkRift.LogType.Error);
                _database.DatabaseError(null , 0 , ex);
            }
        }

        public void SavePlayer(string player)
        {
            _database.DataLayer.GetPlayerActiveShip(player, shipId =>
               {
                   _database.DataLayer.SetShip(ships[shipId].p, () =>
                     {
                         gamePlugin.WriteToLog("player " + player + " saved. Ship id: " + shipId, DarkRift.LogType.Info);
                     });
               });
        }

        public void RemovePlayer(string player)
        {
            if (playerShip.ContainsKey(player))
            {
                SavePlayer(player);
                int shipIdToRemove=playerShip[player];
                playerShip.Remove(player);
                playerShipInverse.Remove(shipIdToRemove);
                playerIds.Remove(player);
                DeleteShip(shipIdToRemove);
                gamePlugin.WriteToLog("player " + player + " removed", DarkRift.LogType.Info);
            }
        }

        public int GetPlayerId(string player)
        {
            return playerIds[player];
        }

        private async Task SendNearest()
        {
            while (started)
            {
                //serverManager.WriteEvent("Send Nearest.", LogType.Info);
                foreach (KeyValuePair<string, int> entry in playerShip)
                {
                    Console.WriteLine("trying to send nearest Ships");

                    using (var writer = DarkRiftWriter.Create())
                    {
                        writer.Write(Nearest(entry.Value));

                        Console.WriteLine("sending {0} bytes to player {1}",writer.Length,entry.Key);

                        using (var msg = Message.Create(Game.NearestShips, writer))
                        {
                            Console.WriteLine("sending message tag {0} of {1} bytes to player {2}", msg.Tag,msg.DataLength,entry.Key);
                            Console.WriteLine(_loginPlugin);
                            Console.WriteLine(_loginPlugin.Clients.Count);
                            Console.WriteLine(_loginPlugin.UsersLoggedIn.Count);

                            IClient cl = _loginPlugin.Clients[entry.Key];
                            Console.WriteLine("client id {0}",cl.ID);

                            cl.SendMessage(msg, SendMode.Unreliable);
                        }
                        Console.WriteLine("sended {0} nearest ships to player {1}", Nearest(entry.Value).Length, entry.Key);

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

        public List<string> GetAllPlayers()
        {
            return playerShipInverse.Values.ToList();
        }

        public int GetPlayerShipId(string player)
        {
            return playerShip[player];
        }

        #endregion


        #region ShipList

        private ShipData[] Nearest(int ship_id)
                
        {
            Ship _playerShip = GetShip(ship_id);
            //Console.WriteLine("nearets ship for id: {0}  count {1}", ship_id,ships.Count);

            List<ShipData> resultShipList = new List<ShipData>();
            foreach (KeyValuePair<int, Ship> entry in ships)
            {
                     
                //Console.WriteLine("nearets ship id: {0}", entry.Value.p.Id);

                if (entry.Value.p.Id == ship_id || entry.Value.p.Hidden || entry.Value.p.Destroyed) continue;//remove player from list


                float dist = Vector3.Distance(_playerShip.p.Position, entry.Value.p.Position);
                //          print (dist);
                if (dist < _playerShip.p.VisionDistance)
                {
                    //Console.WriteLine("nearets ship  add id: {0}", entry.Value.p.Id);

                    resultShipList.Add(entry.Value.p);
                }

            }
            return resultShipList.ToArray();
        }
         private int[] NearestShipIds(int ship_id)
                
        {
            Ship _playerShip = GetShip(ship_id);
            List<int> resultShipList = new List<int>();
            foreach (KeyValuePair<int, Ship> entry in ships)
            {
                if (entry.Value.p.Id == ship_id || entry.Value.p.Hidden || entry.Value.p.Destroyed) continue;//remove player from list
                float dist = Vector3.Distance(_playerShip.p.Position, entry.Value.p.Position);
                //          print (dist);
                if (dist < _playerShip.p.VisionDistance)
                {
                    resultShipList.Add(entry.Value.p.Id);
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
        private void DeleteShip(int shipId) 
        {
            ships[shipId].BeforeDestroy();
            ships.TryRemove(shipId, out _);
        }

        private void LoadShips()
        {
            try
            {
                _database.DataLayer.GetAllMOBShips( shipList =>
                {
                    for (int i = 0; i < shipList.Count; i++)
                        {
                            AddShip(shipList[i]);
                        }
                        //if (_debug)
                        //   {
                              gamePlugin.WriteToLog("Ships loaded count:"+shipList.Count, DarkRift.LogType.Info);
                        //   }
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading ships" +ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null , 0 , ex);
            }
        }

        #endregion

        #region user commands
        public void GetPlayerShipCommand(string player, ShipCommand player_command, int target_id,int point_id)
        {
            gamePlugin.WriteToLog(player + "  " + player_command + " " + target_id + " " + point_id,DarkRift.LogType.Info);
            
            SendShipCommandToNearest(playerShip[player], player_command, target_id, point_id);
            
            Ship _playerShip = ships [playerShip[player]];
            SpaceObject _target;

            switch (player_command)
            {
                case ShipCommand.SetTargetShip:
                    _target = target_id != -1 ? ships[target_id].p : null;
                    _playerShip.Command(player_command, _target);
                    break;
                case ShipCommand.SetTarget:
                    _target = target_id != -1 ? gamePlugin.serverSO.GetSpaceObject(target_id) : null;
                    _playerShip.Command(player_command, _target);
                    break;
                case ShipCommand.WarpTo:
                     _target = target_id != -1 ? gamePlugin.serverSO.GetSpaceObject(target_id) : null;
                    _playerShip.Command(player_command, _target);
                    break;
                case ShipCommand.MoveTo:
                     _target = target_id != -1 ? gamePlugin.serverSO.GetSpaceObject(target_id) : null;
                    _playerShip.Command(player_command, _target);
                    break;
                case ShipCommand.Atack:
                     _target = target_id != -1 ? gamePlugin.serverSO.GetSpaceObject(target_id) : null;
                    _playerShip.Command(player_command, _target,point_id);
                    break;
            }


        }
        private void SendShipCommandToNearest(int shipId, ShipCommand player_command, int target_id,int point_id)
        {
            int[] nearest= NearestShipIds (shipId);
            foreach (int entryId in nearest)
            {
                if (playerShipInverse.ContainsKey(entryId))
                {
                     using (var writer = DarkRiftWriter.Create())
                    {
                        writer.Write(shipId);
                        writer.Write((int) player_command);
                        writer.Write(target_id);
                        writer.Write(point_id);

                        //Console.WriteLine("sending {0} bytes to player {1}",writer.Length,entry.Key);

                        using (var msg = Message.Create(Game.ShipCommand, writer))
                        {
                            //Console.WriteLine("sending message tag {0} of {1} bytes to player {2}", msg.Tag,msg.DataLength,entry.Key);
                            //Console.WriteLine(_loginPlugin);
                            //Console.WriteLine(_loginPlugin.Clients.Count);
                            //Console.WriteLine(_loginPlugin.UsersLoggedIn.Count);

                            IClient cl = _loginPlugin.Clients[playerShipInverse[entryId]];
                            //Console.WriteLine("client id {0}",cl.ID);

                            cl.SendMessage(msg, SendMode.Unreliable);
                        }
                        //Console.WriteLine("sended {0} nearest ships to player {1}", Nearest(entry.Value).Length, entry.Key);

                    }
                }
            }


        }

        public Ship GetShip(int ship_id)
        {
            if (ships.ContainsKey(ship_id)) return ships[ship_id];
            //TODO somthing else when not found
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
            gamePlugin.serverSO.AddNewContainer(ships[e.ship_id].p);
            DeleteShip(e.ship_id);
            Console.WriteLine(" Ship id: {0}  Destroyed ", e.ship_id);
        }
         private void ShipSpawn(object sender, SpawnEventArgs e)
        {
            Console.WriteLine(" Ship id: {0}  spawn ", e.ship_id);
        }
        #endregion
    }
    
}
