using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wingsrv
{
    public class SO_weapon
    {
        public SO_weaponData p; //weapon properties

        private Ship currentTarget;
        private SpaceObject mineTarget;
        private Ship host;
        private GameObject weaponPoint;
        public bool fire;
        public bool mine;
        public Coroutine atack_co;
        public bool activated;

        public SO_weapon(SO_weaponData _data, Ship _host)
        {
            p = new SO_weaponData(_data);
            host = _host;
            activated = false;
            fire = false;
        }
        public void BeforeDestroy()
        {
            fire = false;
            mine = false;
        }
        public void SetWeaponPoint(GameObject _weaponPoint)
        {
            weaponPoint = _weaponPoint;
        }
        public void Atack_target(SpaceObject target)
        {

            if (target.Type == typeSO.ship)
            {
                currentTarget = ((ShipData) target).ShipLink;
                fire = true;
            }
            if (target.Type == typeSO.asteroid)
            {
                mineTarget = target;
                mine = true;
            }
        }

        public void stop()
        {
            currentTarget = null;
            mineTarget = null;
            fire = false;
            mine = false;
        }

        public IEnumerator Attack()
        {
            activated = true;
            while (fire)
            {
                if (!currentTarget.p.Destroyed)
                {
                    if (host.p.Capasitor >= p.Capasitor_use)
                    {
                        host.p.Capasitor += -p.Capasitor_use;
                    }
                    else
                    {
                        stop();
                    }

                    float sqrDistance = (currentTarget.p.Position - host.p.Position).sqrMagnitude;
                    if (sqrDistance > p.SqrDistanse_max * 4)
                    {
                        stop();
                    }
                    else
                    {
                        yield return new WaitForSeconds(p.Reload);

                        if (sqrDistance < p.SqrDistanse_max)
                        {
                        
                            yield return new WaitForSeconds(2);

                            if (p.Type == SO_weaponData.WeaponType.laser)
                            {
                                yield return new WaitForSeconds(p.ActiveTime);
                            }
                            else
                            {
                                yield return new WaitForSeconds(Mathf.Sqrt(sqrDistance) / p.AmmoSpeed);
                            }

                         
                            if (currentTarget != null) currentTarget.Damage(p.Damage);
                        }
                    }
                }
                else
                {
                    stop();
                }
            }
            activated = false;
        }


    }
}