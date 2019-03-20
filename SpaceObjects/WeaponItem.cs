using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

namespace SpaceObjects
{
    public enum WeaponTypes { laser, missile, projective };


    public class WeaponItem : Item, IDarkRiftSerializable
    {


        public WeaponTypes WeaponType { get; set; }
        public float Damage { get; set; }
        public float Reload { get; set; }
        public float AmmoSpeed { get; set; }
        public float ActiveTime { get; set; }//for laser
        public float SqrDistanse_max { get; set; }
        public float Capasitor_use { get; set; }

        public WeaponItem() : base()
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

            WeaponType = (WeaponTypes)e.Reader.ReadInt32();
            Damage = e.Reader.ReadSingle();
            Reload = e.Reader.ReadSingle();
            AmmoSpeed = e.Reader.ReadSingle();
            ActiveTime = e.Reader.ReadSingle();
            SqrDistanse_max = e.Reader.ReadSingle();
            Capasitor_use = e.Reader.ReadSingle();
        }

        public new void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Id);
            e.Writer.Write(ItemName);
            e.Writer.Write(SpritePath);
            e.Writer.Write((int)ItemType);
            e.Writer.Write(Volume);
            e.Writer.Write(Prefab);

            e.Writer.Write((int)WeaponType);
            e.Writer.Write(Damage);
            e.Writer.Write(Reload);
            e.Writer.Write(AmmoSpeed);
            e.Writer.Write(ActiveTime);
            e.Writer.Write(SqrDistanse_max);
            e.Writer.Write(Capasitor_use);


        }

    }
}