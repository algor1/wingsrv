using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wingsrv
{
    public class SO_weaponData
    {
        public enum WeaponType { laser, missile, projective };
        public WeaponType Type { get; set; }
        public float Damage { get; set; }
        public float Reload { get; set; }
        public float AmmoSpeed { get; set; }
        public float ActiveTime { get; set; }//for laser
        public float SqrDistanse_max { get; set; }
        public float Capasitor_use { get; set; }

        public SO_weaponData()
        {
        }
        public SO_weaponData(SO_weaponData value)
        {
            Type = value.Type;
            Damage = value.Damage;
            Reload = value.Reload;
            AmmoSpeed = value.AmmoSpeed;
            ActiveTime = value.ActiveTime;
            SqrDistanse_max = value.SqrDistanse_max;
            Capasitor_use = value.Capasitor_use;
        }

    }
}