using UnityEngine;

namespace Wingsrv
{
    public class SpaceObject
    {
        public int id;
        public string visibleName;
        public Server.typeSO type;
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
        public string prefab;
        public SO_ship ship;

    }
}