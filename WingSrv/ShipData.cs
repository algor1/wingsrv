
using System.Collections;
using System.Collections.Generic;
namespace Wingsrv
{
    // Properties of spaceship that will be stored to DB
    public class ShipData: SpaceObject
    {
        public Ship ShipLink { get; }
        public float SpeedMax { get; set; }
        public float RotationSpeed { get; set; }
        public float AccelerationMax{get; set;}
        public float SpeedNew { get; set; }

        public float Hull_full { get; set; }
        public float Armor_full { get; set; }
        public float Shield_full { get; set; }
        public float Capasitor_full { get; set; }

        public float Hull{get; set;}
        public float Armor{get; set;}
        public float Shield{get; set;}
        public float Capasitor { get; set; }

        public float Hull_restore { get; set; }
        public float Armor_restore { get; set; }
        public float Shield_restore { get; set; }
        public float Capasitor_restore { get; set; }


        public float AgrDistance { get; set; }
        public float VisionDistance { get; set; }

        public bool Destroyed { get; set; }
        public bool Hidden { get; set; }
        public bool Mob { get; set; }

        public float WarpDriveStartTime { get; set; }
        public float WarpSpeed { get; set; }
        public SO_weaponData[] Weapons { get; set; }
        public SO_equipmentData[] Equipments{get; set;}


        public ShipData():base()
        {

        }

        public ShipData(ShipData value,Ship ship): base(value)
        {
            ShipLink = ship;
            SpeedMax = value.SpeedMax;
            RotationSpeed = value.RotationSpeed;
            AccelerationMax = value.AccelerationMax;
            SpeedNew = value.SpeedNew;

            Hull_full = value.Hull_full;
            Armor_full = value.Armor_full;
            Shield_full = value.Shield_full;
            Capasitor_full = value.Capasitor_full;

            Hull = value.Hull;
            Armor = value.Armor;
            Shield = value.Shield;
            Capasitor = value.Capasitor;

            Hull_restore = value.Hull_restore;
            Armor_restore = value.Armor_restore;
            Shield_restore = value.Shield_restore;
            Capasitor_restore = value.Capasitor_restore;


            AgrDistance = value.AgrDistance;
            VisionDistance = value.VisionDistance;

            Destroyed = value.Destroyed;
            Hidden = value.Hidden;
            Mob = value.Mob;

            WarpDriveStartTime = value.WarpDriveStartTime;
            WarpSpeed = value.WarpSpeed;
            
            Weapons = new SO_weaponData[value.Weapons.Length];
            for (int i = 0; i < value.Weapons.Length; i++)
			{
                Weapons[i] = new SO_weaponData (value.Weapons[i]);
			}

            Equipments = new SO_equipmentData[value.Equipments.Length];
            for (int i = 0; i < value.Equipments.Length; i++)
            {
                Equipments[i] = new SO_equipmentData(value.Equipments[i]);
            }
        }
        
    }

}