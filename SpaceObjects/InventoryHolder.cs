
using DarkRift;

namespace SpaceObjects{


    public class InventoryHolder : IDarkRiftSerializable
    {
        public int   PlayerId   { get; set; }
        public int   HolderId   { get; set; }
        public float Volume     { get; set; }
        public float VolumeLeft { get; set; }





        public void Deserialize(DeserializeEvent e)
        {
            
            PlayerId   = e.Reader.ReadInt32();
            HolderId   = e.Reader.ReadInt32();
            Volume     = e.Reader.ReadSingle();
            VolumeLeft = e.Reader.ReadSingle();

        }

        public void Serialize(SerializeEvent e)
        {
            
            e.Writer.Write(PlayerId  );
            e.Writer.Write(HolderId  );
            e.Writer.Write(Volume    );
            e.Writer.Write(VolumeLeft);
    }
}