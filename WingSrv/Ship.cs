using System;
//using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace SpaceObjects
{
    public enum ComandType { warpTo, goTo, landTo, none, open };
    public enum ShipEvenentsType { spawn, warp, warmwarp, move, stop, land, hide, reveal, destroyed, open };
    public enum Command { MoveTo, WarpTo, Atack, SetTarget, LandTo, Equipment, Open, TakeOff };


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
        public bool warpCoroutineStarted;
        //public bool landed;
        private bool atackAI;
        private Weapon[] weapons;
        private Equipment[] equipments;
        
        public int TickDeltaTime=20; //{get;set;}




        public Ship(ShipData shipData)
        {
            p = new ShipData(shipData,this); ;
            if  (p.Rotation == new MyQuaternion(0f, 0f, 0f, 0f)) p.Rotation = new MyQuaternion(0f, 0f, 0f, 1f);


            //rotationToTarget = p.SO.rotation;
            moveCommand = MoveType.stop;
            //SendEvent(ShipEvenentsType.move);
            //host = _host;
            //newtargetToMove = null;
            weapons = new Weapon[shipData.Weapons.Length];
            for (int i = 0; i < shipData.Weapons.Length; i++)
            {
                weapons[i] = new Weapon(shipData.Weapons[i], this);
                
            }
            equipments = new Equipment[shipData.Equipments.Length];
            for (int i = 0; i < shipData.Equipments.Length; i++)
            {
                equipments[i] = new Equipment(shipData.Equipments[i], this);
            }
        }

        public void BeforeDestroy()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].BeforeDestroy();
                //weapons.RemoveAt(i);
            }
            for (int i = 0; i < equipments.Length; i++)
            {
                equipments[i].BeforeDestroy();
                //equipments.RemoveAt(i);

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
            OnLand(args);
        }

        public event EventHandler<DestroyEventArgs> ShipDestroyed;

        protected virtual void OnDestroy(DestroyEventArgs e)
        {
            EventHandler<DestroyEventArgs> handler = ShipDestroyed;
        }
        private void OnDestroyCall(int ship_id)
        {
            DestroyEventArgs args = new DestroyEventArgs();
            OnDestroy(args);
        }
        public event EventHandler<SpawnEventArgs> ShipSpawn;
        protected virtual void OnSpawn(SpawnEventArgs e)
        {
            EventHandler<SpawnEventArgs> handler = ShipSpawn;
        }
        private void OnSpawnCall(int ship_id)
        {
            SpawnEventArgs args = new SpawnEventArgs();
            OnSpawn(args);
        }
#endregion

#region user commands
        public void SetTarget(SpaceObject newTarget)
        {
            NewTargetToMove = newTarget;
            NewTargetToAtack = newTarget.Type== TypeSO.ship ? newTarget : null;
            Console.WriteLine("{0} id {1} target to move {2} , target to atack {3}",  newTarget.Type, p.Id, NewTargetToMove.Id, NewTargetToAtack.Id);
        }
        public void GoToTarget()
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
        public void WarpToTarget()
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
        public void StartEquipment()
        {
            for (int i = 0; i < equipments.Length; i++)
            {
                if (!equipments[i].activate)
                {
                    equipments[i].Use();
                }
                else
                {
                    equipments[i].Stop();
                }
            }

        }
        public void StopEquipment()
        {
            for (int i = 0; i < equipments.Length; i++)
            {
                equipments[i].Stop();

            }

        }
        public void LandToTarget()
        {
            if (NewTargetToMove != null)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;

                complexCommand = ComandType.landTo;
                TargetToMove = NewTargetToMove;
            }
        }
        public void OpenTarget()
        {
            if (NewTargetToMove != null)
            {
                zBeforeRotation = p.Rotation.eulerAngles.z;

                complexCommand = ComandType.open;
                TargetToMove = NewTargetToMove;
                //            oldRotation = p.SO.rotation;
            }
        }
        public void Atack_target(int weaponnum)
        {
            if (NewTargetToAtack != null)
            {
                TargetToAtack = NewTargetToAtack;

                //atack = true;
                weapons[weaponnum].Atack_target(TargetToAtack);

            }
            else
            {
                //atack = false;
            }

        }
        public void StopFire(int weaponnum)
        {
            //atack = false;
            weapons[weaponnum].stop();
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
                        //Console.WriteLine("Ship {0} , position {1}", p.Id,p.Position);

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
                //p.Speed = Mathf.Lerp(p.Speed, p.SpeedMax, TickDeltaTime/1000f * p.AccelerationMax);
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
                p.Shield += -damage;
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
        private void RestoreTick()
        {
            if (p.Hull < p.Hull_full)
            {
                p.Hull += p.Hull_restore * TickDeltaTime/1000f;
            }
            else { p.Hull = p.Hull_full; }
            if (p.Shield < p.Shield_full)
            { p.Shield += p.Shield_restore * TickDeltaTime/1000f; }
            else { p.Shield = p.Shield_full; }

            if (p.Armor < p.Armor_full) { p.Armor += p.Armor_restore * TickDeltaTime/1000f; }
            else { p.Armor = p.Armor_full; }
            if (p.Capasitor < p.Capasitor_full) { p.Capasitor += p.Capasitor_restore * TickDeltaTime/1000f; }
            else { p.Capasitor = p.Capasitor_full; }

        }
        private void Destroyed()
        {
            p.Destroyed = true;
            for (int i = 0; i < weapons.Length; i++)
            {
                StopFire(i);
            }
            StopEquipment();
            OnDestroyCall(p.Id);
            
        }
        private void Agr()
        {
            if (p.Mob && !p.Destroyed)
            {
                if (NewTargetToAtack != null && !atackAI)
                {
                    if (Vector3.Distance(p.Position, NewTargetToAtack.Position) < p.AgrDistance)
                    {
                        SetTarget(NewTargetToAtack);
                        GoToTarget();
                        for (int i = 0; i < weapons.Length; i++)
                        {
                            Atack_target(i);
                        }
                    }
                    else
                    {
                        atackAI = false;
                    }
                }
            }
        }
        public async Task Warpdrive()
        {
            warpCoroutineStarted = true;
            await Task.Delay((int)(1000f*p.WarpDriveStartTime));
            float warpDistance = Vector3.Distance(p.Position, TargetToMove.Position);
            float warpTime = warpDistance / p.WarpSpeed;
            Hide(); //TODO damage=0
            warpActivated = true;


            await Task.Delay((int)warpTime*1000);
            Spawn(TargetToMove.Position - Vector3.forward * 10);

            Reveal();

            warpActivated = false;
            warpCoroutineStarted = false;
            p.Speed = p.SpeedMax * 0.1f;
            moveCommand = MoveType.stop;
            complexCommand = ComandType.none;
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

            Agr(); //TODO too heavy fo tick may be it must hav diffeent time of activation
            RestoreTick();
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

#endregion
}