using UnityEngine;
using DarkRift;

namespace SpaceObjects
{
    public enum TypeSO { asteroid, ship, station, waypoint, container };

    public class SpaceObject :IDarkRiftSerializable
    {
        public int Id { get; set; }
        public string VisibleName { get; set; }
        public TypeSO Type { get; set; }
        public Vector3 Position { get; set; }
        public MyQuaternion Rotation { get; set; } = new MyQuaternion(0f, 0f, 0f, 1f);
        public float Speed { get; set; }
        public string Prefab { get; set; }
        //public SO_ship ship { get; set; }


        public SpaceObject() { }

        public SpaceObject(SpaceObject value)
        {
            Id = value.Id;
            VisibleName = value.VisibleName;
            Type = value.Type;
            Position = value.Position;
            Rotation = value.Rotation;
            Speed = value.Speed;
            Prefab = value.Prefab;
        }

    
public void  Deserialize(DeserializeEvent e)
{
 	        Id= e.Reader.ReadInt32();
            VisibleName = e.Reader.ReadString();
            Type = (TypeSO) e.Reader.ReadInt32();
            Position = new Vector3(e.Reader.ReadSingle(),e.Reader.ReadSingle(),e.Reader.ReadSingle());
            Rotation = new MyQuaternion(e.Reader.ReadSingle(),e.Reader.ReadSingle(),e.Reader.ReadSingle(),e.Reader.ReadSingle());
            Speed = e.Reader.ReadSingle();
            Prefab = e.Reader.ReadString();
}

public void  Serialize(SerializeEvent e)
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
}
}
    
}