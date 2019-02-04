using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

namespace SpaceObjects
{
    public class WeaponData : IDarkRiftSerializable
    {
        public enum WeaponType { laser, missile, projective };
        public WeaponType Type { get; set; }
        public float Damage { get; set; }
        public float Reload { get; set; }
        public float AmmoSpeed { get; set; }
        public float ActiveTime { get; set; }//for laser
        public float SqrDistanse_max { get; set; }
        public float Capasitor_use { get; set; }

        public WeaponData()
        {
        }
        public WeaponData(WeaponData value)
        {
            Type = value.Type;
            Damage = value.Damage;
            Reload = value.Reload;
            AmmoSpeed = value.AmmoSpeed;
            ActiveTime = value.ActiveTime;
            SqrDistanse_max = value.SqrDistanse_max;
            Capasitor_use = value.Capasitor_use;
        }


        public void Deserialize(DeserializeEvent e)
        {
            Type = (WeaponType)e.Reader.ReadInt32();
            Damage = e.Reader.ReadSingle();
            Reload = e.Reader.ReadSingle();
            AmmoSpeed = e.Reader.ReadSingle();
            ActiveTime = e.Reader.ReadSingle();
            SqrDistanse_max = e.Reader.ReadSingle();
            Capasitor_use = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write((int)Type); 
            e.Writer.Write(Damage);
            e.Writer.Write(Reload);
            e.Writer.Write(AmmoSpeed);
            e.Writer.Write(ActiveTime);
            e.Writer.Write(SqrDistanse_max);
            e.Writer.Write(Capasitor_use);

        }
    }
}