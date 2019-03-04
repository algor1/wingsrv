using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceObjects
{
    public class Weapon
    {
        public WeaponData p; //weapon properties

        private Ship currentTarget;
        private SpaceObject mineTarget;
        private Ship host;
        //private GameObject weaponPoint;
        private bool fire;
        private bool mine;
        //public Coroutine atack_co;
        //private bool activated;
        private bool reloaded;

        public Weapon(WeaponData _data, Ship _host)
        {
            p = new WeaponData(_data);
            host = _host;
            //activated = false;
            fire = false;
        }
        public void BeforeDestroy()
        {
            fire = false;
            mine = false;
        }
        //public void SetWeaponPoint(GameObject _weaponPoint)
        //{
        //    weaponPoint = _weaponPoint;
        //}
       
        public void Atack_target(SpaceObject target)
        {
            if (target.Type == TypeSO.ship)
            {
                currentTarget = ((ShipData) target).ShipLink;
                fire = true;
                Atack();
            }
            if (target.Type == TypeSO.asteroid)
            {
                mineTarget = target;
                mine = true;
            }

        }

        public void Stop()
        {
            currentTarget = null;
            mineTarget = null;
            fire = false;
            mine = false;
        }

        private async Task Reload()
        {
            if (host.p.Capasitor >= p.Capasitor_use)
                    {
                        OnStartReload(EventArgs.Empty);
                        host.p.Capasitor += -p.Capasitor_use;
                        await Task.Delay(p.Reload*1000);
                        reloaded = true;
                        OnStopReload(EventArgs.Empty);
                    }
                    else
                    {
                        Stop();
                    }

        }
        private async Task Fire(float sqrDistance)
        {
            OnStartFireCall(currentTarget.p.Id);
            if (p.Type == WeaponData.WeaponType.laser)
            {
                await Task.Delay(p.ActiveTime*1000);
            }
            else
            {
                await Task.Delay(Mathf.Sqrt(sqrDistance) / p.AmmoSpeed*1000);
            }
            currentTarget?.Damage(p.Damage);
            OnStopFire(EventArgs.Empty);
        }

        

        private async Task Atack()
        {
            //activated = true;
            while (fire)
            {
                if (!currentTarget?.p.Destroyed)
                {
                    Stop();
                    return;
                }

                float sqrDistance = (currentTarget.p.Position - host.p.Position).sqrMagnitude;
                if (sqrDistance > p.SqrDistanse_max)
                {
                    Stop();
                    return;
                }

                if (reloaded)
                {
                    Fire(sqrDistance);
                    reloaded = false;
                    await Reload();
                }
            }
        }



#region events
        public event EventHandler<StartFireEventArgs> StartFire;
        protected virtual void OnStartFire(StartFireEventArgs e)
        {
            EventHandler<StartFireEventArgs> handler = StartFire;
        }
        private void OnStartFireCall(int ship_id)
        {
            StartFireEventArgs args = new StartFireEventArgs();
            args.ship_id=ship_id;
            OnStartFire(args);
        }

        public event EventHandler StopFire;
        protected virtual void OnStopFire(EventArgs e)
        {
            EventHandler handler = StopFire;
        }

        public event EventHandler StartReload;
        protected virtual void OnStartReload(EventArgs e)
        {
            EventHandler handler = StartReload;
        }

        public event EventHandler StopReload;
        protected virtual void OnStopReload(EventArgs e)
        {
            EventHandler handler = StopReload;
        }
        

#endregion

    }

#region  Events Args Classes

    public class StartFireEventArgs: EventArgs
    {
        public int ship_id {get; set;}
    }

#endregion
}