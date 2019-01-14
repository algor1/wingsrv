using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Wingsrv
{
    public enum Command { MoveTo, WarpTo, Atack, SetTarget, LandTo, Equipment, Open, TakeOff };
    public enum typeSO { asteroid, ship, station, waypoint, container };

    public class Server
    {

        public bool started;
        private ConcurrentDictionary<int, Ship> ships;


        public delegate void TickHandler();
        public event TickHandler onTick;
        private ServerDB serverDB;
        public InventoryServer inventoryServer;
        private ServerManager serverManager;
        public int TickDeltaTime = 20;



        public  Server(ServerManager manager)
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
                Run();
            }

        }

        private void Run()
        {
            Console.WriteLine(" Server started.");
            started = true;
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

        #region ShipList

        private void AddShip(ShipData ship)
        {
            if (!ships.ContainsKey(ship.Id)){
                Ship s = new Ship(ship);
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
