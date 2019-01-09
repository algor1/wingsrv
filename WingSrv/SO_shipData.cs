
using System.Collections;
using System.Collections.Generic;
namespace Wingsrv
{

    public class SO_shipData
    {
        public SpaceObject SO;

        public float max_speed;
        public float rotation_speed;
        public float acceleration_max;
        public float newSpeed;

        public float hull_full;
        public float armor_full;
        public float shield_full;
        public float capasitor_full;

        public float hull;
        public float armor;
        public float shield;
        public float capasitor;

        public float hull_restore;
        public float armor_restore;
        public float shield_restore;
        public float capasitor_restore;


        public float agr_distance;
        public float vision_distance;

        public bool destroyed;
        public bool hidden;
        public bool mob;

        public float warpDriveStartTime;
        public float warpSpeed;
        public List<SO_weaponData> weapons;
        public List<SO_equipmentData> equipments;

        public SO_shipData()
        {
            SO = new SpaceObject();
            weapons = new List<SO_weaponData>();
            equipments = new List<SO_equipmentData>();
        }


    }

}