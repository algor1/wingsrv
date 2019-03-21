using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

namespace SpaceObjects
{

    public class EquipmentItem:Item, IDarkRiftSerializable
    {
        public bool passive;
        public float shieldpoints;
        public float armorpoints;
        public float hullpoints;
        public float reload;
        public float capasitor_use;

        public EquipmentItem():base()
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

            passive = e.Reader.ReadBoolean();
            shieldpoints = e.Reader.ReadSingle();
            armorpoints = e.Reader.ReadSingle();
            hullpoints = e.Reader.ReadSingle();
            reload = e.Reader.ReadSingle();
            capasitor_use = e.Reader.ReadSingle();
        }

        public new void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Id);
            e.Writer.Write(ItemName);
            e.Writer.Write(SpritePath);
            e.Writer.Write((int)ItemType);
            e.Writer.Write(Volume);
            e.Writer.Write(Prefab);

            e.Writer.Write(passive);
            e.Writer.Write(shieldpoints);
            e.Writer.Write(armorpoints); 
            e.Writer.Write(hullpoints);
            e.Writer.Write(reload);
            e.Writer.Write(capasitor_use);
        }
    }
}