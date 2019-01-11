using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wingsrv
{
    public class SO_weaponData
    {
        public enum WeaponType { laser, missile, projective };
        public WeaponType type { get; set; }
        public float damage { get; set; }
        public float reload { get; set; }
        public float ammoSpeed { get; set; }
        public float activeTime { get; set; }//for laser
        public float sqrDistanse_max { get; set; }
        public float capasitor_use { get; set; }

        public SO_weaponData()
        {
        }
        public SO_weaponData(SO_weaponData value)
        {
            type = value.type;
            damage = value.damage;
            reload = value.reload;
            ammoSpeed = value.ammoSpeed;
            activeTime = value.activeTime;
            sqrDistanse_max = value.sqrDistanse_max;
            capasitor_use = value.capasitor_use;
        }

    }
}