using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wingsrv
{
    [System.Serializable]
    public class SO_weaponData
    {
        public enum WeaponType { laser, missile, projective };
        public WeaponType type;
        public float damage;
        public float reload;
        public float ammoSpeed;
        public float activeTime;//for laser
        public float sqrDistanse_max;
        public float capasitor_use;

        public SO_weaponData()
        {
        }
        public SO_weaponData(SO_weaponData val)
        {
            type = val.type;
            //		active = val.active;
            damage = val.damage;
            reload = val.reload;
            ammoSpeed = val.ammoSpeed;
            activeTime = val.activeTime;
            sqrDistanse_max = val.sqrDistanse_max;
            capasitor_use = val.capasitor_use;
        }

    }
}