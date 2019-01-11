using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wingsrv
{
    public class SO_weapon
    {
        public SO_weaponData p; //weapon properties

        private SO_ship currentTarget;
        private SpaceObject mineTarget;
        private SO_ship host;
        private GameObject weaponPoint;
        public bool fire;
        public bool mine;
        public Coroutine atack_co;
        public bool activated;

        public SO_weapon(SO_weaponData _data, SO_ship _host)
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

            if (target.type == typeSO.ship)
            {
                currentTarget = target.ship;
                fire = true;
            }
            if (target.type == typeSO.asteroid)
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
                if (!currentTarget.p.destroyed)
                {
                    if (host.p.capasitor >= p.capasitor_use)
                    {
                        host.p.capasitor += -p.capasitor_use;
                    }
                    else
                    {
                        stop();
                    }

                    float sqrDistance = (currentTarget.p.SO.position - host.p.SO.position).sqrMagnitude;
                    if (sqrDistance > p.sqrDistanse_max * 4)
                    {
                        stop();
                    }
                    else
                    {
                        yield return new WaitForSeconds(p.reload);

                        if (sqrDistance < p.sqrDistanse_max)
                        {
                        
                            yield return new WaitForSeconds(2);

                            if (p.type == SO_weaponData.WeaponType.laser)
                            {
                                yield return new WaitForSeconds(p.activeTime);
                            }
                            else
                            {
                                yield return new WaitForSeconds(Mathf.Sqrt(sqrDistance) / p.ammoSpeed);
                            }
                            if (host.host != null)
                            {
                            }
                            else
                            {
                            }
                          
                            if (currentTarget != null) currentTarget.Damage(p.damage);
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