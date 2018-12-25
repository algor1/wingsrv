using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Wingsrv
{

    public class SO_equipment
    {
        public SO_equipmentData p; //equipment properties

        private SO_ship currentTarget;
        private SO_ship host;
        private GameObject weaponPoint;
        public bool coroutineStarted;
        public bool activate;
        public Coroutine use_co;


        public SO_equipment(SO_equipmentData _data, SO_ship _host)
        {
            p = new SO_equipmentData(_data);
            host = _host;
            activate = false;
            coroutineStarted = false;
        }
        public void BeforeDestroy()
        {
        }

        public void Use()
        {

            activate = true;
        }
        public void Stop()
        {

            activate = false;
        }

        public IEnumerator UseEq()
        {
            coroutineStarted = true;
            while (activate)
            {

                //check capasitor

                if (host.p.capasitor > p.capasitor_use)
                {
                    host.p.capasitor -= p.capasitor_use;
                }
                else
                {
                    Stop();
                }
                yield return new WaitForSeconds(p.reload);

                host.p.shield += p.shieldpoints;
                host.p.armor += p.armorpoints;
                host.p.hull += p.hullpoints;

                //if (host.host != null)
                //{
                //    //							Debug.Log (host.host.name + "  " + host.p.SO.visibleName + "----pew----  to " + currentTarget.p.SO.visibleName); 
                //}
                //else
                //{
                //    //							Debug.Log ("server  " + host.p.SO.visibleName + "----pew----  to " + currentTarget.p.SO.visibleName); 
                //}
                ////							Debug.Log (weaponPoint.GetComponent<WeaponPoint> ());


            }
            coroutineStarted = false;
        }
    }
}