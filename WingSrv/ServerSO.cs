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

namespace Wingsrv
{


    public class ServerSO
    {

        public bool started;
        private Dictionary<int, SpaceObject> spaceObjects;


        private Server server;
        

        private InventoryServer inventoryServer;
        //private ServerManager serverManager;
        public int TickDeltaTime = 10000;
        private Game gamePlugin;
        public Login _loginPlugin;
        public  DatabaseProxy _database{get;set;}



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
                inventoryServer = gamePlugin.inventoryServer;
                Console.WriteLine("loading Space objects...");
                LoadserverObjects();
                Console.WriteLine("Starting SpaceObjects server...");

                started = true;
                SendNearestTick();


            }

        }

        private void LoadserverObjects()
        {
            try
            {
                _database.DataLayer.GetAllSpaceObjects( soList =>
                {
                    for (int i = 0; i < soList.Count; i++)
                        {
                            
                            AddSpaceObject(soList[i]);
                        }
                        //if (_debug)
                        //   {
                              gamePlugin.WriteToLog("Ships loaded count:"+soList.Count, DarkRift.LogType.Info);
                        //   }
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading space objects" +ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null , 0 , ex);
            }
            
            

        }
        private void AddSpaceObject(SpaceObject so)
        {
            SpaceObject s = new SpaceObject(so);
            spaceObjects.Add(s.Id, s);
        }

        public void SendNearest(string player)
        {
            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(Nearest(server.GetPlayerShipId(player)));

                //Console.WriteLine("sending {0} bytes to player {1} ,tag {2}",writer.Length,player ,Game.NearestSpaceObjects);

                using (var msg = Message.Create(Game.NearestSpaceObjects, writer))
                {
                    //Console.WriteLine("sending message tag {0} of {1} bytes to player {2}", msg.Tag,msg.DataLength,player);
                    //Console.WriteLine(_loginPlugin);
                    //Console.WriteLine(_loginPlugin.Clients.Count);
                    //Console.WriteLine(_loginPlugin.UsersLoggedIn.Count);

                    IClient cl = _loginPlugin.Clients[player];
                    //Console.WriteLine("client id {0}",cl.ID);

                    cl.SendMessage(msg, SendMode.Unreliable);
                }
                //Console.WriteLine("sended {0} nearest so to player {1}", player, player);

            }
        }



        private async Task SendNearestTick()
        {
            while (started)
            {
                //serverManager.WriteEvent("Send Nearest.", LogType.Info);
                Console.WriteLine("trying to send nearest");
                foreach (string entry in server.GetAllPlayers())
                {
                    SendNearest(entry);
                }
                await Task.Delay(TickDeltaTime);
            }
        }






        private SpaceObject[] Nearest(int ship_id)

        {
            Ship _playerShip = server.GetShip(ship_id);
            List<SpaceObject> resultSOList = new List<SpaceObject>();
            foreach (KeyValuePair<int, SpaceObject> entry in spaceObjects)
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

        public SpaceObject GetSpaceObject(int id)
        {
            return spaceObjects[id];
        }

    }




}
