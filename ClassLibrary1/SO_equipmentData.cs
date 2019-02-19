using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Wingsrv
{
    [System.Serializable]
    public class SO_equipmentData
    {
        public enum type { laser, missile, projective };

        public bool passive;
        public float shieldpoints;
        public float armorpoints;
        public float hullpoints;
        public float reload;
        public float capasitor_use;

        public SO_equipmentData()
        {
        }
        public SO_equipmentData(SO_equipmentData val)
        {
            passive = val.passive;
            shieldpoints = val.shieldpoints;
            armorpoints = val.armorpoints;
            hullpoints = val.hullpoints;
            reload = val.reload;
            capasitor_use = val.capasitor_use;

        }

    }
}