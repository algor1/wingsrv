﻿using System;
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
                //Debug.Log("add Ship " + i);
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



        //void Awake()
        //{
        //    DontDestroyOnLoad(transform.gameObject);
        //    ships = new List<SO_ship>();
        //    LoadShips();
        //}

        //void Start()
        //{
        //    ships[1].SetTarget(ships[0].p.SO);
        //    ships[1].GoToTarget();
        //    started = true;


        //}


        //void FixedUpdate()
        //{
        //    Tick();

        //}




        //public List<SO_ship> Nearest(int player_id)
        //{
        //    SO_ship pl = GetPlayer(player_id);
        //    List<SO_ship> resultShipList = new List<SO_ship>();
        //    for (int i = 0; i < ships.Count; i++)
        //    {
        //        if (ships[i].p.SO.id == player_id || ships[i].p.hidden) continue;//remove player from list
        //        if (ships[i].p.SO.id == player_id || ships[i].p.destroyed) continue;//remove player from list

        //        float dist = Vector3.Distance(pl.p.SO.position, ships[i].p.SO.position);
        //        //			print (dist);
        //        if (dist < pl.p.vision_distance)
        //        {
        //            resultShipList.Add(ships[i]);
        //        }
        //        if (dist < ships[i].p.vision_distance && !ships[i].atack)
        //        {
        //            ships[i].SetTarget(pl.p.SO);//playerid=0
        //        }
        //    }
        //    //		if (resultShipList.Count>0)
        //    //		Debug.Log ("nearest "+resultShipList.Count);

        //    return resultShipList;
        //}

        //public SO_ship GetPlayer(int player_id)
        //{
        //    for (int i = 0; i < ships.Count; i++)
        //    {
        //        if (ships[i].p.SO.id == player_id) return ships[i];
        //    }
        //    return ships[0];

        //}

        //private void LoadShips()
        //{
        //    List<SO_shipData> shipList = GetComponent<ServerDB>().GetAllShips();
        //    for (int i = 0; i < shipList.Count; i++)
        //    {
        //        Debug.Log("add Ship " + i);
        //        AddShip(shipList[i]);
        //    }

        //    //Debug.Log("all ship" + shipsDB.shipList.Count);
        //    //for (int i = 0; i < shipsDB.shipList.Count; i++)
        //    //{
        //    //   AddShip (shipsDB.shipList[i]);

        //    //    //            ships = shipsDB.shipList;
        //    //}
        //}



        //private void DeleteShip(int ship_id)
        //{
        //    GetComponent<ServerDB>().DeleteSOShipData(ship_id);
        //    for (int i = 0; i < ships.Count; i++)
        //    {
        //        if (ships[i].p.SO.id == ship_id)
        //        {
        //            ships[i].BeforeDestroy();
        //            ships[i] = null;
        //            ships.RemoveAt(i);

        //            break;
        //        }
        //    }

        //}
        //private void DestrtoyShip(int ship_id)
        //{
        //    GetComponent<ServerSO>().AddContainer(ship_id);
        //    DeleteShip(ship_id);
        //}
        //public int RegisterPlayer()
        //{
        //    return 0;
        //}

        //public void PlayerControl(int player_id, Command player_command, int weapon_equip_num)
        //{
        //    SO_ship player = GetPlayer(player_id);
        //    switch (player_command)
        //    {
        //        case Command.MoveTo:
        //            player.GoToTarget();
        //            break;
        //        case Command.WarpTo:
        //            player.WarpToTarget();
        //            break;
        //        case Command.Atack:
        //            player.Atack_target(weapon_equip_num);
        //            break;
        //        case Command.LandTo:
        //            player.LandToTarget();
        //            break;
        //        case Command.Equipment:
        //            player.StartEquipment();
        //            break;
        //        case Command.Open:
        //            player.OpenTarget();
        //            break;
        //        case Command.TakeOff:
        //            player.landed = false;
        //            break;
        //    }
        //}

        ////	public void PlayerControlSetTargetShip(int player_id,Command player_command ,SO_ship target) {
        ////		SO_ship player = GetPlayer(player_id);
        ////		SO_ship tg = GetPlayer(target.p.SO.id);
        ////		if (player_command==Command.SetTarget){
        ////			player.SetTarget(tg);
        ////		}
        ////	}
        //public void PlayerControlSetTarget(int player_id, Command player_command, ServerObject target)
        //{
        //    SO_ship player = GetPlayer(player_id);
        //    if (player_command == Command.SetTarget)
        //    {
        //        player.SetTarget(target);
        //    }

        //}


        //private void Warp(SO_ship ship)
        //{
        //    if (ship.moveCommand == SO_ship.MoveType.warp)
        //    {
        //        if (ship.Rotate())
        //        {
        //            if (!ship.warpCoroutineStarted)
        //            {
        //                StartCoroutine(ship.Warpdrive());
        //            }
        //        }
        //    }
        //}
        //public void Events(SO_ship.ShipEvenentsType shipEvent, SO_ship ship)
        //{
        //    switch (shipEvent)
        //    {
        //        case SO_ship.ShipEvenentsType.stop:
        //            //                Debug.Log("server" + ship.p.SO.visibleName + " ship stopped");

        //            break;
        //        case SO_ship.ShipEvenentsType.move:
        //            Debug.Log("server" + ship.p.SO.visibleName + " ship accelerated");
        //            break;
        //        case SO_ship.ShipEvenentsType.warmwarp:
        //            Debug.Log("server" + ship.p.SO.visibleName + " ship preparing to warp");
        //            break;
        //        case SO_ship.ShipEvenentsType.warp:
        //            Debug.Log("server" + ship.p.SO.visibleName + " ship warping....");

        //            break;
        //        case SO_ship.ShipEvenentsType.spawn:
        //            Debug.Log("server" + ship.p.SO.visibleName + " ship spawn");
        //            break;
        //        case SO_ship.ShipEvenentsType.land:
        //            //			Debug.Log ("server" + ship.p.SO.visibleName + " ship landing " + ship.targetToMove.id);
        //            //			GetComponent<SkillsDB> ().GetAllSkills ();
        //            GetComponent<LandingServer>().Landing(ship.targetToMove.id, ship.p.SO.id);
        //            ship.landed = true;
        //            break;
        //        case SO_ship.ShipEvenentsType.destroyed:
        //            StopAllCoroutines(ship);
        //            DestrtoyShip(ship.p.SO.id);
        //            break;
        //        case SO_ship.ShipEvenentsType.open:
        //            //			Debug.Log ("server" + ship.p.SO.visibleName + " ship landing " + ship.targetToMove.id);
        //            //			DO NOTHING

        //            break;

        //    }
        //}

        //private void Atack(SO_ship ship)
        //{
        //    if (ship.atack)
        //    {
        //        for (int i = 0; i < ship.weapons.Count; i++)
        //        {
        //            if (ship.weapons[i].fire && !ship.weapons[i].activated)

        //            {
        //                //					Debug.Log (ship.weapons [i].GetHashCode ());
        //                StartCoroutine(ship.weapons[i].Attack());
        //            }

        //        }
        //    }
        //}
        //private void Mine(SO_ship ship)
        //{
        //    if (ship.atack)
        //    {
        //        for (int i = 0; i < ship.weapons.Count; i++)
        //        {
        //            if (ship.weapons[i].fire && !ship.weapons[i].activated)

        //            {
        //                //					Debug.Log (ship.weapons [i].GetHashCode ());
        //                StartCoroutine(ship.weapons[i].Mine());
        //            }

        //        }
        //    }
        //}
        //private void Equipment(SO_ship ship)
        //{
        //    for (int i = 0; i < ship.equipments.Count; i++)
        //    {
        //        if (ship.equipments[i].activate && !ship.equipments[i].coroutineStarted)
        //        {
        //            StartCoroutine(ship.equipments[i].UseEq());
        //        }
        //    }
        //}
        //private void StopAllCoroutines(SO_ship ship)
        //{
        //    for (int i = 0; i < ship.weapons.Count; i++)
        //    {
        //        if (ship.weapons[i].activated)
        //        {
        //            StopCoroutine(ship.weapons[i].atack_co);
        //        }
        //    }
        //    for (int i = 0; i < ship.equipments.Count; i++)
        //    {
        //        if (ship.equipments[i].activate)
        //        {
        //            StopCoroutine(ship.equipments[i].use_co);
        //        }
        //    }

        //}

        //private void Tick()
        //{
        //    for (int i = 0; i < ships.Count; i++)
        //    {
        //        ships[i].Tick();
        //        Warp(ships[i]);
        //        Atack(ships[i]);
        //        Equipment(ships[i]);
        //        Mine(ships[i]);
        //    }
        //}

