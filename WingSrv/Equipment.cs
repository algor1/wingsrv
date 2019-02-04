using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceObjects
{

    public class Equipment
    {
        public EquipmentData p; //equipment properties

        private Ship currentTarget;
        private Ship host;
        public bool coroutineStarted;
        public bool activate;
        public Coroutine use_co;


        public Equipment(EquipmentData _data, Ship _host)
        {
            p = new EquipmentData(_data);
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

                if (host.p.Capasitor > p.capasitor_use)
                {
                    host.p.Capasitor -= p.capasitor_use;
                }
                else
                {
                    Stop();
                }
                yield return new WaitForSeconds(p.reload);

                host.p.Shield += p.shieldpoints;
                host.p.Armor += p.armorpoints;
                host.p.Hull += p.hullpoints;



            }
            coroutineStarted = false;
        }
    }
}