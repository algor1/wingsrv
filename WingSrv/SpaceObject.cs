using UnityEngine;

namespace Wingsrv
{
    public class SpaceObject
    {
        public int Id { get; set; }
        public string VisibleName { get; set; }
        public typeSO Type { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
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