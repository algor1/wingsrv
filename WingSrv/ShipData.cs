
using System.Collections;
using System.Collections.Generic;
using DarkRift;
using UnityEngine;

namespace SpaceObjects
{
    // Properties of spaceship that will be stored to DB
    public class ShipData: SpaceObject,IDarkRiftSerializable
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
        public WeaponData[] Weapons { get; set; }
        public EquipmentData[] Equipments{get; set;}


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
            
            Weapons = new WeaponData[value.Weapons.Length];
            for (int i = 0; i < value.Weapons.Length; i++)
			{
                Weapons[i] = new WeaponData (value.Weapons[i]);
			}

            Equipments = new EquipmentData[value.Equipments.Length];
            for (int i = 0; i < value.Equipments.Length; i++)
            {
                Equipments[i] = new EquipmentData(value.Equipments[i]);
            }
        }
        public new void Deserialize(DeserializeEvent e)
        {
            Id = e.Reader.ReadInt32();
            VisibleName = e.Reader.ReadString();
            Type = (TypeSO)e.Reader.ReadInt32();
            Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            Rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
            Speed = e.Reader.ReadSingle();
            Prefab = e.Reader.ReadString();

            SpeedMax = e.Reader.ReadSingle();
            RotationSpeed = e.Reader.ReadSingle();
            AccelerationMax = e.Reader.ReadSingle();
            SpeedNew = e.Reader.ReadSingle();

            Hull_full = e.Reader.ReadSingle();
            Armor_full = e.Reader.ReadSingle();
            Shield_full = e.Reader.ReadSingle();
            Capasitor_full = e.Reader.ReadSingle();

            Hull = e.Reader.ReadSingle();
            Armor = e.Reader.ReadSingle();
            Shield = e.Reader.ReadSingle(); ;
            Capasitor = e.Reader.ReadSingle();

            Hull_restore = e.Reader.ReadSingle();
            Armor_restore = e.Reader.ReadSingle();
            Shield_restore = e.Reader.ReadSingle();
            Capasitor_restore = e.Reader.ReadSingle();


            AgrDistance = e.Reader.ReadSingle();
            VisionDistance = e.Reader.ReadSingle();

            Destroyed = e.Reader.ReadBoolean();
            Hidden = e.Reader.ReadBoolean();
            Mob = e.Reader.ReadBoolean();

            WarpDriveStartTime = e.Reader.ReadSingle();
            WarpSpeed = e.Reader.ReadSingle();
            Weapons = e.Reader.ReadSerializables<WeaponData>();
            Equipments = e.Reader.ReadSerializables<EquipmentData>();
        }

        public new void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Id);
            e.Writer.Write(VisibleName);
            e.Writer.Write((int)Type);
            e.Writer.Write(Position.x);
            e.Writer.Write(Position.y);
            e.Writer.Write(Position.z);
            e.Writer.Write(Rotation.x);
            e.Writer.Write(Rotation.y);
            e.Writer.Write(Rotation.z);
            e.Writer.Write(Rotation.w);
            e.Writer.Write(Speed);
            e.Writer.Write(Prefab);

            e.Writer.Write(SpeedMax);
            e.Writer.Write(RotationSpeed);
            e.Writer.Write(AccelerationMax);
            e.Writer.Write(SpeedNew);

            e.Writer.Write(Hull_full);
            e.Writer.Write(Armor_full);
            e.Writer.Write(Shield_full);
            e.Writer.Write(Capasitor_full);

            e.Writer.Write(Hull);
            e.Writer.Write(Armor);
            e.Writer.Write(Shield);
            e.Writer.Write(Capasitor);

            e.Writer.Write(Hull_restore);
            e.Writer.Write(Armor_restore);
            e.Writer.Write(Shield_restore);
            e.Writer.Write(Capasitor_restore);


            e.Writer.Write(AgrDistance);
            e.Writer.Write(VisionDistance);

            e.Writer.Write(Destroyed);
            e.Writer.Write(Hidden);
            e.Writer.Write(Mob);

            e.Writer.Write(WarpDriveStartTime);
            e.Writer.Write(WarpSpeed);
            e.Writer.Write(Weapons);
            e.Writer.Write(Equipments);

        }


    }

}