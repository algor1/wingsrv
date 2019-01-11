using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Wingsrv
{
    public enum Command { MoveTo, WarpTo, Atack, SetTarget, LandTo, Equipment, Open, TakeOff };
    public enum ShipEvenentsType { spawn, warp, warmwarp, move, stop, land, hide, reveal };
    public enum typeSO { asteroid, ship, station, waypoint, container };

    public class Server
    {

        public bool started;
        private ConcurrentDictionary<int, SO_ship> ships;


        public delegate void TickHandler();
        public event TickHandler onTick;
        private ServerDB serverDB;
        public InventoryServer inventoryServer;
        private ServerManager serverManager;
        public int TickDeltaTime = 20;



        public Server(ServerManager manager)
        {
            serverManager = manager;
        }
        public void RunServer()
        {
            if (!started)
            {
                this.onTick += new TickHandler(Tick);
                ships = new ConcurrentDictionary<int, SO_ship>();
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

        private void AddShip(SO_shipData ship)
        {
            if (!ships.ContainsKey(ship.SO.id)){
                SO_ship s = new SO_ship(ship);
                ships.TryAdd(s.p.SO.id,s);
                onTick += s.Tick;
                
            }
            else
            {
                //TODO Log wrong ship adding
            }
        }
        private void DeleteShip(SO_shipData ship) { }

        private void LoadShips()
        {
            List<SO_shipData> shipList = serverDB.GetAllShips();
            
            for (int i = 0; i < shipList.Count; i++)
            {
                AddShip(shipList[i]);
            }
        }

        #endregion

        #region user commands
        #endregion

        #region events



        private void EventSigner(SO_ship ship)
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
