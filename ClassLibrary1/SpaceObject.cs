using UnityEngine;

namespace Wingsrv
{
    public class SpaceObject
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
    }
    
}