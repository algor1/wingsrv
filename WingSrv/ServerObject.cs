namespace WingSrv{
    interface SpaceObject
    {
        int id;
        string visibleName;
        Server.typeSO type;
        Vector3 position;
        public Quaternion rotation;
        public float speed;
        public string prefab;
        public SO_ship ship;

    }
}