using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace Wingsrv
{

    public class SO_ship
    {
        public ShipData p; //ship properties	
        //public GameObject host;

        //public bool atack;
        //private Quaternion rotationToTarget;

        //private SpaceObject target;
        public SpaceObject targetToMove;
        public SpaceObject newtargetToMove;
        //	private Quaternion oldRotation; //for roll calc

        private SpaceObject targetToAtack;
        private SpaceObject newtargetToAtack;
        public enum MoveType { move, warp, stop };
        public MoveType moveCommand;
        public enum ComandType { warpTo, goTo, landTo, none, open };
        public ComandType complexCommand;
        public enum ShipEvenentsType { spawn, warp, warmwarp, move, stop, land, hide, reveal, destroyed, open };
        public bool warpActivated;
        //public bool warpCoroutineStarted;
        //public bool landed;

        private SO_weapon[] weapons;
        private SO_equipment[] equipments;
        
        public int TickDeltaTime=20; //{get;set;}




        public SO_ship(ShipData shipData)
        {
            p = new ShipData(shipData);
            //rotationToTarget = p.SO.rotation;
            //moveCommand = MoveType.move;
            //SendEvent(ShipEvenentsType.move);
            //host = _host;
            //newtargetToMove = null;
            weapons = new SO_weapon[shipData.Weapons.Length];
            for (int i = 0; i < shipData.Weapons.Length; i++)
            {
                weapons[i] = new SO_weapon(shipData.Weapons[i], this);
                
            }
            equipments = new SO_equipment[shipData.Equipments.Length];
            for (int i = 0; i < shipData.Equipments.Length; i++)
            {
                equipments[i] = new SO_equipment(shipData.Equipments[i], this);
            }
        }

        public void BeforeDestroy()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].BeforeDestroy();
                weapons.RemoveAt(i);
            }
            for (int i = 0; i < equipments.Length; i++)
            {
                equipments[i].BeforeDestroy();
                equipments.RemoveAt(i);

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

#endregion
        //public void SendEvent(ShipEvenentsType evnt)
        //{
        //    onland?.Invoke(p.SO.id);
        //    Console.WriteLine("onland");

        //}


#region user commands
        public void SetTarget(SpaceObject newtarget)
        {
            newtargetToMove = newtarget;
            newtargetToAtack = newtarget;
        }
        public void GoToTarget()
        {
            if (newtargetToMove != null)
            {

                complexCommand = ComandType.goTo;
                targetToMove = newtargetToMove;
                //			oldRotation = p.SO.rotation;


            }

        }
        public void WarpToTarget()
        {
            if (newtargetToMove != null && Vector3.Distance(p.SO.position, newtargetToMove.position) > 100000)
            {
                moveCommand = MoveType.warp;
                complexCommand = ComandType.warpTo;
                targetToMove = newtargetToMove;
                //			oldRotation = p.SO.rotation;
                warpActivated = false;
            }
            else
            {
                //Debug.Log("TO CLOSE TO WARP");
            }

        }
        public void StartEquipment()
        {
            for (int i = 0; i < equipments.Count; i++)
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
            for (int i = 0; i < equipments.Count; i++)
            {
                equipments[i].Stop();

            }

        }
        public void LandToTarget()
        {
            if (newtargetToMove != null)
            {
                complexCommand = ComandType.landTo;
                targetToMove = newtargetToMove;
            }
        }
        public void OpenTarget()
        {
            if (newtargetToMove != null)
            {
                complexCommand = ComandType.open;
                targetToMove = newtargetToMove;
                //            oldRotation = p.SO.rotation;
            }
        }
        public void Atack_target(int weaponnum)
        {
            if (newtargetToAtack != null)
            {
                targetToAtack = newtargetToAtack;

                //atack = true;
                weapons[weaponnum].Atack_target(targetToAtack);

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

                if (Vector3.Distance(p.SO.position, targetToMove.position) > 10 * p.SO.speed / p.acceleration_max)
                {
                    moveCommand = MoveType.move;

                }
                else
                {

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
                if (Vector3.Distance(p.SO.position, targetToMove.position) > 10 * p.max_speed / p.acceleration_max)
                {
                    moveCommand = MoveType.move;

                }
                else
                {

                    moveCommand = MoveType.stop;


                    OnLandCall(p.SO.id);
                    complexCommand = ComandType.none;

                }

            }
            if (complexCommand == ComandType.open)
            {
                if (Vector3.Distance(p.SO.position, targetToMove.position) > 10 * p.max_speed / p.acceleration_max)
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
        public bool Rotate()
        {
            if (targetToMove != null)
            {
                rotationToTarget = Quaternion.LookRotation(targetToMove.position - p.SO.position);
                Vector3 rt = new Vector3(rotationToTarget.eulerAngles.x, rotationToTarget.eulerAngles.y, p.SO.rotation.eulerAngles.z);
                rotationToTarget = Quaternion.Euler(rt);

                if (Mathf.Abs(p.SO.rotation.eulerAngles.x - rotationToTarget.eulerAngles.x) > 1 || Mathf.Abs(p.SO.rotation.eulerAngles.y - rotationToTarget.eulerAngles.y) > 1)
                {
                    p.SO.rotation = Quaternion.RotateTowards(p.SO.rotation, rotationToTarget, p.rotation_speed * TickDeltaTime/1000);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        void Accelerate()
        {
            if (p.newSpeed != p.SO.speed)
            {
                if (p.newSpeed > p.SO.speed)
                {
                    p.SO.speed += TickDeltaTime/1000 * p.acceleration_max;
                    if (p.newSpeed < p.SO.speed)
                    {
                        p.SO.speed = p.newSpeed;
                        Console.WriteLine("Ship {0} , position {1}", p.SO.id,p.SO.position);

                    }
                }
                else
                {
                    p.SO.speed += -TickDeltaTime/1000 * p.acceleration_max;
                    if (p.newSpeed > p.SO.speed)
                    {
                        p.SO.speed = p.newSpeed;
                    }
                }
            }
        }
        private void Move()
        {
            if (moveCommand == MoveType.move)
            {
                Rotate();
                Accelerate();
                p.SO.speed = Mathf.Lerp(p.SO.speed, p.max_speed, TickDeltaTime/1000 * p.acceleration_max);
                p.SO.position += p.SO.rotation * Vector3.forward * TickDeltaTime/1000 * p.SO.speed;
            }
        }
        private void Stop()
        {
            if (moveCommand == MoveType.stop)
            {
                p.newSpeed = 0;
            }
        }
        public void Damage(float damage)
        {
            if (p.shield - damage > 0)
            {
                p.shield += -damage;
            }
            else
            {
                if (p.shield + p.armor - damage > 0)
                {
                    p.armor += p.shield - damage;
                    p.shield = 0;
                }
                else
                {
                    if (p.shield + p.armor + p.hull - damage > 0)
                    {
                        p.hull += p.armor + p.shield - damage;
                        p.shield = 0;
                        p.armor = 0;
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
            if (p.hull < p.hull_full)
            {
                p.hull += p.hull_restore * TickDeltaTime/1000;
            }
            else { p.hull = p.hull_full; }
            if (p.shield < p.shield_full)
            { p.shield += p.shield_restore * TickDeltaTime/1000; }
            else { p.shield = p.shield_full; }

            if (p.armor < p.armor_full) { p.armor += p.armor_restore * TickDeltaTime/1000; }
            else { p.armor = p.armor_full; }
            if (p.capasitor < p.capasitor_full) { p.capasitor += p.capasitor_restore * TickDeltaTime/1000; }
            else { p.capasitor = p.capasitor_full; }

        }
        private void Destroyed()
        {
            p.destroyed = true;
            for (int i = 0; i < weapons.Count; i++)
            {
                StopFire(i);
            }
            StopEquipment();
            OnDestroyCall(p.SO.id);
            
        }
        private void Agr()
        {
            if (p.Mob && !p.Destroyed)
            {
                if (newtargetToAtack != null && !atack)
                {
                    if (Vector3.Distance(p.position, newtargetToAtack.position) < p.AgrDistance)
                    {
                        SetTarget(newtargetToAtack);
                        GoToTarget();
                        for (int i = 0; i < weapons.Count; i++)
                        {
                            Atack_target(i);
                        }
                    }
                    else
                    {
                        atack = false;
                    }
                }
            }
        }
        public async Task Warpdrive()
        {
            warpCoroutineStarted = true;
            await Task.Delay((int)(1000*p.warpDriveStartTime));
            float warpDistance = Vector3.Distance(p.SO.position, targetToMove.position);
            float warpTime = warpDistance / p.warpSpeed;
            Hide(); //TODO damage=0
            warpActivated = true;


            await Task.Delay((int)warpTime*1000);
            Spawn(targetToMove.position - Vector3.forward * 10);

            Reveal();

            warpActivated = false;
            warpCoroutineStarted = false;
            p.SO.speed = p.max_speed * 0.1f;
            moveCommand = MoveType.stop;
            complexCommand = ComandType.none;
        }
        private void Spawn(Vector3 _position){}
      
        private void Hide()
        {
            p.hidden = true;
        }
        private void Reveal()
        {
            p.hidden = false;
        }
        public void Tick()
        {
            Agr(); //TODO too heavy fo tick may be it must hav diffeent time of activation
            RestoreTick();
            //CommandManager();
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


#endregion
}