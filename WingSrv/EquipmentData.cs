using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

namespace Wingsrv
{

    public class EquipmentData:IDarkRiftSerializable
    {
        public bool passive;
        public float shieldpoints;
        public float armorpoints;
        public float hullpoints;
        public float reload;
        public float capasitor_use;

        public EquipmentData()
        {
        }
        public EquipmentData(EquipmentData val)
        {
            passive = val.passive;
            shieldpoints = val.shieldpoints;
            armorpoints = val.armorpoints;
            hullpoints = val.hullpoints;
            reload = val.reload;
            capasitor_use = val.capasitor_use;

        }


        public void Deserialize(DeserializeEvent e)
        {
            passive = e.Reader.ReadBoolean();
            shieldpoints = e.Reader.ReadSingle();
            armorpoints = e.Reader.ReadSingle();
            hullpoints = e.Reader.ReadSingle();
            reload = e.Reader.ReadSingle();
            capasitor_use = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(passive);
            e.Writer.Write(shieldpoints);
            e.Writer.Write(armorpoints); 
            e.Writer.Write(hullpoints);
            e.Writer.Write(reload);
            e.Writer.Write(capasitor_use);
        }
    }
}