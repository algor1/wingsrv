using UnityEngine;
using DarkRift;

namespace SpaceObjects{


    public class InventoryItem : IDarkRiftSerializable
    {
        public int ItemId { get; set; }
        public int Tech { get; set; }
        public int Quantity { get; set; }





        public void Deserialize(DeserializeEvent e)
        {
            
            ItemId = e.Reader.ReadInt32();
            Tech = e.Reader.ReadInt32();
            Quantity = e.Reader.ReadInt32();


        }

        public void Serialize(SerializeEvent e)
        {
            
            e.Writer.Write(ItemId);
            e.Writer.Write(Tech);
            e.Writer.Write(Quantity);
        }
    }
}