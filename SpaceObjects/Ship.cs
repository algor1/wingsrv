using System;
//using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace SpaceObjects
{
    public enum ComandType { warpTo, goTo, landTo, none, open };
    public enum ShipEvenentsType { spawn, warp, warmwarp, move, stop, land, hide, reveal, destroyed, open };
    public enum ShipCommand { MoveTo, WarpTo, Atack, SetTarget, SetTargetShip, LandTo, Equipment, Open, TakeOff };


    public class Ship 
    {
        public ShipData p; //ship properties	
        //public GameObject host;

        //public bool atack;
        private float zBeforeRotation;

        //private SpaceObject target;
        public SpaceObject TargetToMove {get;private set;}
        public SpaceObject NewTargetToMove {get;private set;}
        //	private Quaternion oldRotation; //for roll calc

        public SpaceObject TargetToAtack {get;private set;}
        public SpaceObject NewTargetToAtack {get;private set;}
        public enum MoveType { move, warp, stop };
        private MoveType moveCommand;
        
        private ComandType complexCommand;
    
        private bool warpActivated;
        private bool warpStarted;

        //public bool landed;
        //private bool atackAI;
        public Weapon[] Weapons;
        public Equipment[] Equipments;
        
        public int TickDeltaTime=20; //{get;set;}
        public int RestoreTickDeltaTime=3000; //TODO store it from shipdata



        public Ship(ShipData shipData)
        {
            p = new ShipData(shipData,this); ;
            if  (p.Rotation == new MyQuaternion(0f, 0f, 0f, 0f)) p.Rotation = new MyQuaternion(0f, 0f, 0f, 1f);


            //rotationToTarget = p.SO.rotation;
            moveCommand = MoveType.stop;
            //SendEvent(ShipEvenentsType.move);
            //host = _host;
            //newtargetToMove = null;
            Weapons = new Weapon[shipData.Weapons.Length];
            for (int i = 0; i < shipData.Weapons.Length; i++)
            {
                Weapons[i] = new Weapon(shipData.Weapons[i], this);
                
            }
            Equipments = new Equipment[shipData.Equipments.Length];
            for (int i = 0; i < shipData.Equipments.Length; i++)
            {
                Equipments[i] = new Equipment(shipData.Equipments[i], this);
            }
            RestoreTick();
        }

        public void BeforeDestroy()
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                Weapons[i].BeforeDestroy();
                //Weapons.RemoveAt(i);
            }
            for (int i = 0; i < Equipments.Length; i++)
            {
                Equipments[i].BeforeDestroy();
                //Equipments.RemoveAt(i);

            }
        }
#region events
        public event EventHandler<LandEventArgs> ShipLanded;

        protected virtual void OnLand(LandEventArgs e)
        {
            EventHandler<LandEventArgs> handler = ShipLanded;
        }
        private void OnLandCall(int ship_id)
        {
            LandEventArgs args = new LandEventArgs();
            args.ship_id=ship_id;
            OnLand(args);
        }

        public event EventHandler<DestroyEventArgs> ShipDestroyed;

        protected virtual void OnDestroy(DestroyEventArgs e)
        {
            EventHandler<DestroyEventArgs> handler = ShipDestroyed;
            Console.WriteLine("handler null? {0}",handler==null);//
            handler?.Invoke(this, e);

        }
        private void OnDestroyCall(int ship_id)
        {
            DestroyEventArgs args = new DestroyEventArgs();
            Console.WriteLine("Ship destroyed, id: {0}", ship_id);
            args.ship_id=ship_id;
            OnDestroy(args);
        }
        public event EventHandler<SpawnEventArgs> ShipSpawn;
        protected virtual void OnSpawn(SpawnEventArgs e)
        {
            EventHandler<SpawnEventArgs> handler = ShipSpawn;
            handler?.Invoke(this, e);

        }
        private void OnSpawnCall(int ship_id)
        {
            SpawnEventArgs args = new SpawnEventArgs();
            args.ship_id=ship_id;
            OnSpawn(args);
        }
        // Event for animation in client
        public event EventHandler<ChangeStateArgs> ChangeState;
        protected virtual void OnChangeState(ChangeStateArgs e)
        {
            EventHandler<ChangeStateArgs> handler = ChangeState;
            handler?.Invoke(this, e);

        }
        private void OnChangeStateCall(ShipEvenentsType _eventType)
        {
            ChangeStateArgs args = new ChangeStateArgs();
            args.ChangeState=_eventType;
            OnChangeState(args);
        }

        

#endregion

#region user commands

        public void Command(ShipCommand command, SpaceObject target=null,int point_id=0)
        {
            if (target != null)
                {
                    SetTarget(target);
                }

            switch (command)
            {
                case ShipCommand.MoveTo:
                    GoToTarget();
                    break;
                //case ShipCommand.SetTarget:
                //    SetTarget(target);
                //    break;
                //case ShipCommand.SetTargetShip:
                //    SetTarget(target);
                //    break;

                case ShipCommand.Atack:
                    Atack_target(point_id);
                    break;

                case ShipCommand.WarpTo:
                    WarpToTarget();
                    break;
                
            }
        }


        private void SetTarget(SpaceObject newTarget)
        {
            NewTargetToMove = newTarget;
            NewTargetToAtack = newTarget.Type== TypeSO.ship ? newTarget : null;
            Console.WriteLine("{0} id {1} target to move {2} , target to atack {3}",  newTarget.Type, p.Id, NewTargetToMove?.Id, NewTargetToAtack?.Id);
        }
        private void GoToTarget()
        {
            if (NewTargetToMove != null)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;
                complexCommand = ComandType.goTo;
                TargetToMove = NewTargetToMove;
                Console.WriteLine("{0} id {1} moving to {2} id{3}", p.Type, p.Id, NewTargetToMove.Type, NewTargetToMove.Id);
                //			oldRotation = p.SO.rotation;
            }
        }
        private void WarpToTarget()
        {
            if (NewTargetToMove != null && Vector3.Distance(p.Position, NewTargetToMove.Position) > 100000)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;

                moveCommand = MoveType.warp;
                complexCommand = ComandType.warpTo;
                TargetToMove = NewTargetToMove;
                //			oldRotation = p.SO.rotation;
                warpActivated = false;
                Console.WriteLine("{0} id {1} warping to {2} id{3}", p.Type, p.Id, NewTargetToMove.Type, NewTargetToMove.Id);

            }
            else
            {
                Console.WriteLine("{0} id {1} TO CLOSE TO WARP to {2} id{3}",p.Type,p.Id,NewTargetToMove.Type,NewTargetToMove.Id);
            }

        }
        private void StartEquipment()
        {
            for (int i = 0; i < Equipments.Length; i++)
            {
                if (!Equipments[i].activate)
                {
                    Equipments[i].Use();
                }
                else
                {
                    Equipments[i].Stop();
                }
            }

        }
        private void StopEquipment()
        {
            for (int i = 0; i < Equipments.Length; i++)
            {
                Equipments[i].Stop();

            }

        }
        private void LandToTarget()
        {
            if (NewTargetToMove != null)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;

                complexCommand = ComandType.landTo;
                TargetToMove = NewTargetToMove;
            }
        }
       private void OpenTarget()
        {
            if (NewTargetToMove != null)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;

                complexCommand = ComandType.open;
                TargetToMove = NewTargetToMove;
                //            oldRotation = p.SO.rotation;
            }
        }
        private void Atack_target(int weaponnum)
        {
            if (NewTargetToAtack != null)
            {
                TargetToAtack = NewTargetToAtack;

                //atack = true;
                Weapons[weaponnum].Atack_target(TargetToAtack);

            }
            else
            {
                //atack = false;
            }

        }
        private void StopFire(int weaponnum)
        {
            //atack = false;
            Weapons[weaponnum].Stop();
        }
        private void CommandManager()
        {
            if (complexCommand == ComandType.goTo)
            {
                if (Vector3.Distance(p.Position, TargetToMove.Position) > 10 * p.Speed / p.AccelerationMax)
                {
                    moveCommand = MoveType.move;
                    Console.WriteLine(p.Position);
                }
                else
                {
                    Console.WriteLine("{0} id {1} pos {2} stopped near {3} id {4} pos {5}", p.Type, p.Id,p.Position, TargetToMove.Type, TargetToMove.Id ,TargetToMove.Position);

                    moveCommand = MoveType.stop;
                    complexCommand = ComandType.none;
                }
            }
            if (complexCommand == ComandType.warpTo)
            {
                moveCommand = MoveType.warp;
            }
            if (complexCommand == ComandType.landTo)
            {
                if (Vector3.Distance(p.Position, TargetToMove.Position) > 10 * p.SpeedMax / p.AccelerationMax)
                {
                    moveCommand = MoveType.move;

                }
                else
                {

                    moveCommand = MoveType.stop;


                    OnLandCall(p.Id);
                    complexCommand = ComandType.none;

                }

            }
            if (complexCommand == ComandType.open)
            {
                if (Vector3.Distance(p.Position, TargetToMove.Position) > 10 * p.SpeedMax / p.AccelerationMax)
                {
                    moveCommand = MoveType.move;
                }
                else
                {
                    moveCommand = MoveType.stop;
                    complexCommand = ComandType.none;
                }

            }


        }
        #endregion

#region actions
        private void Rotate()
        {
            if (TargetToMove != null)
            {
                MyQuaternion rotationToTarget = MyQuaternion.LookRotation(TargetToMove.Position-p.Position);
                //removeing z axis  
                rotationToTarget = MyQuaternion.Euler(rotationToTarget.eulerAngles.x , rotationToTarget.eulerAngles.y, zBeforeRotation);
                
                if (p.Rotation!=rotationToTarget)
                {
                    p.Rotation = MyQuaternion.RotateTowards(p.Rotation, rotationToTarget, p.RotationSpeed * TickDeltaTime/1000f);
                    Console.WriteLine("Ship {0} , rotation {1} target {2} rotation to target {3}" , p.Id, p.Rotation.eulerAngles, TargetToMove.Id,rotationToTarget.eulerAngles);
                }
            }
        }
        void Accelerate()
        {
            if (p.SpeedNew != p.Speed)
            {
                if (p.SpeedNew > p.Speed)
                {
                    p.Speed += TickDeltaTime/1000f * p.AccelerationMax;
                    if (p.SpeedNew < p.Speed)
                    {
                        p.Speed = p.SpeedNew;
                        Console.WriteLine("Ship {0} , position {1}", p.Id,p.Position);
                    }
                }
                else
                {
                    p.Speed += -TickDeltaTime/1000f * p.AccelerationMax;
                    if (p.SpeedNew > p.Speed)
                    {
                        p.Speed = p.SpeedNew;
                    }
                }
            }
        }

        private void Move()
        {
            if (moveCommand == MoveType.move)
            {
                p.SpeedNew = p.SpeedMax;
                Rotate();
                Accelerate();
                p.Position += p.Rotation * Vector3.forward * TickDeltaTime/1000f * p.Speed;
            }
        }

        private void Stop()
        {
            if (moveCommand == MoveType.stop)
            {
                //Console.WriteLine(p.Position);
                p.SpeedNew = 0;
            }
        }

        public void Damage(float damage)
        {
            if (p.Shield - damage > 0)
            {
                p.Shield += damage;
            }
            else
            {
                if (p.Shield + p.Armor - damage > 0)
                {
                    p.Armor += p.Shield - damage;
                    p.Shield = 0;
                }
                else
                {
                    if (p.Shield + p.Armor + p.Hull - damage > 0)
                    {
                        p.Hull += p.Armor + p.Shield - damage;
                        p.Shield = 0;
                        p.Armor = 0;
                    }
                    else
                    {
                        Destroyed();
                    }
                }
            }
        }
        private async Task  RestoreTick()
        {
            while (true)
            {
                if (p.Hull < p.Hull_full)
                {
                    p.Hull += p.Hull_restore * RestoreTickDeltaTime / 1000f;
                }
                else { p.Hull = p.Hull_full; }
                if (p.Shield < p.Shield_full)
                { p.Shield += p.Shield_restore * RestoreTickDeltaTime / 1000f; }
                else { p.Shield = p.Shield_full; }

                if (p.Armor < p.Armor_full) { p.Armor += p.Armor_restore * RestoreTickDeltaTime / 1000f; }
                else { p.Armor = p.Armor_full; }
                if (p.Capasitor < p.Capasitor_full) { p.Capasitor += p.Capasitor_restore * RestoreTickDeltaTime / 1000f; }
                else { p.Capasitor = p.Capasitor_full; }
                await Task.Delay(RestoreTickDeltaTime);
            }
        }
        private void Destroyed()
        {
            p.Destroyed = true;
            for (int i = 0; i < Weapons.Length; i++)
            {
                StopFire(i);
            }
            StopEquipment();
            OnDestroyCall(p.Id);
            
        }
        //private void Agr()
        //{
        //    if (p.Mob && !p.Destroyed)
        //    {
        //        if (NewTargetToAtack != null && !atackAI)
        //        {
        //            if (Vector3.Distance(p.Position, NewTargetToAtack.Position) < p.AgrDistance)
        //            {
        //                SetTarget(NewTargetToAtack);
        //                GoToTarget();
        //                for (int i = 0; i < Weapons.Length; i++)
        //                {
        //                    Atack_target(i);
        //                }
        //            }
        //            else
        //            {
        //                atackAI = false;
        //            }
        //        }
        //    }
        //}
        public async Task Warpdrive()
        {
            warpStarted = true;
            OnChangeStateCall(ShipEvenentsType.warmwarp);
            await Task.Delay((int)(1000f*p.WarpDriveStartTime));
            float warpDistance = Vector3.Distance(p.Position, TargetToMove.Position);
            float warpTime = warpDistance / p.WarpSpeed;
            OnChangeStateCall(ShipEvenentsType.warp);
            Hide(); //TODO damage=0
            warpActivated = true;

            await Task.Delay((int)(warpTime*1000f));
            Spawn(TargetToMove.Position - Vector3.forward * 10);//10 metres before target
            OnChangeStateCall(ShipEvenentsType.spawn);
            Reveal();

            warpActivated = false;
            warpStarted = false;
            p.Speed = p.SpeedMax * 0.1f;
            moveCommand = MoveType.stop;
            complexCommand = ComandType.none;
            OnChangeStateCall(ShipEvenentsType.stop);
        }
        private void Spawn(Vector3 _position){
            OnSpawnCall(p.Id);
        }
      
        private void Hide()
        {
            p.Hidden = true;
        }
        private void Reveal()
        {
            p.Hidden = false;
        }
        public void Tick()
        {
            //Console.WriteLine($"Tick {p.VisibleName}");

            //Agr(); //TODO too heavy fo tick may be it must have diffeent time of activation
            //RestoreTick();
            CommandManager();
            Move();
            Stop();
            //Console.WriteLine("Ship {0} , position {1}", p.SO.id, p.SO.position);

        }
#endregion
    }

#region  Events Args Classes

    public class LandEventArgs: EventArgs
    {
        public int ship_id {get; set;}
    }
    public class DestroyEventArgs: EventArgs
    {
        public int ship_id {get; set;}
    }
    public class SpawnEventArgs: EventArgs
    {
        public int ship_id {get; set;}
    }
    
    public class ChangeStateArgs: EventArgs
    {
        public ShipEvenentsType ChangeState {get;set;}
    }
#endregion
}