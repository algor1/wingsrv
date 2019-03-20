
using System.Collections;
using System.Collections.Generic;
using DarkRift;
using UnityEngine;

namespace SpaceObjects
{


    // Properties of spaceship that will be stored to DB
    public class ShipItem : Item, IDarkRiftSerializable
    {
        public float SpeedMax { get; set; }
        public float RotationSpeed { get; set; }
        public float AccelerationMax { get; set; }
        public float SpeedNew { get; set; }

        public float Hull_full { get; set; }
        public float Armor_full { get; set; }
        public float Shield_full { get; set; }
        public float Capasitor_full { get; set; }

        public float Hull_restore { get; set; }
        public float Armor_restore { get; set; }
        public float Shield_restore { get; set; }
        public float Capasitor_restore { get; set; }

        public float AgrDistance { get; set; }
        public float VisionDistance { get; set; }

        public float WarpDriveStartTime { get; set; }
        public float WarpSpeed { get; set; }
        public int WeaponsCount { get; set; }
        public int EquipmentsCount { get; set; }


        public ShipItem() : base()
        {
        }

        public new void Deserialize(DeserializeEvent e)
        {
            Id = e.Reader.ReadInt32();
            ItemName = e.Reader.ReadString();
            SpritePath = e.Reader.ReadString();
            ItemType = (ItemTypes)e.Reader.ReadInt32();
            Volume = e.Reader.ReadInt32();
            Prefab = e.Reader.ReadString();

            SpeedMax = e.Reader.ReadSingle();
            RotationSpeed = e.Reader.ReadSingle();
            AccelerationMax = e.Reader.ReadSingle();
            SpeedNew = e.Reader.ReadSingle();

            Hull_full = e.Reader.ReadSingle();
            Armor_full = e.Reader.ReadSingle();
            Shield_full = e.Reader.ReadSingle();
            Capasitor_full = e.Reader.ReadSingle();

            Hull_restore = e.Reader.ReadSingle();
            Armor_restore = e.Reader.ReadSingle();
            Shield_restore = e.Reader.ReadSingle();
            Capasitor_restore = e.Reader.ReadSingle();

            AgrDistance = e.Reader.ReadSingle();
            VisionDistance = e.Reader.ReadSingle();

            WarpDriveStartTime = e.Reader.ReadSingle();
            WarpSpeed = e.Reader.ReadSingle();
            WeaponsCount = e.Reader.ReadInt32();
            EquipmentsCount = e.Reader.ReadInt32();
        }

        public new void Serialize(SerializeEvent e)
        {

            e.Writer.Write(Id);
            e.Writer.Write(ItemName);
            e.Writer.Write(SpritePath);
            e.Writer.Write((int)ItemType);
            e.Writer.Write(Volume);
            e.Writer.Write(Prefab);

            e.Writer.Write(SpeedMax);
            e.Writer.Write(RotationSpeed);
            e.Writer.Write(AccelerationMax);
            e.Writer.Write(SpeedNew);

            e.Writer.Write(Hull_full);
            e.Writer.Write(Armor_full);
            e.Writer.Write(Shield_full);
            e.Writer.Write(Capasitor_full);

            e.Writer.Write(Hull_restore);
            e.Writer.Write(Armor_restore);
            e.Writer.Write(Shield_restore);
            e.Writer.Write(Capasitor_restore);


            e.Writer.Write(AgrDistance);
            e.Writer.Write(VisionDistance);

            e.Writer.Write(WarpDriveStartTime);
            e.Writer.Write(WarpSpeed);
            e.Writer.Write(WeaponsCount);
            e.Writer.Write(EquipmentsCount);

        }


    }

}