using UnityEngine;

namespace Wingsrv
{
    public class SpaceObject
    {
        public int id { get; set; }
        public string visibleName { get; set; }
        public typeSO type { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public float speed { get; set; }
        public string prefab { get; set; }
        //public SO_ship ship { get; set; }

    }
}