using UnityEngine;
using DarkRift;


public class InventoryItem : IDarkRiftSerializable
{
    public int PlayerId { get; set; }
    public int ItemId { get; set; }
    public int Tech { get; set; }
    public int Quantity { get; set; }





    public void Deserialize(DeserializeEvent e)
    {
        PlayerId =e.Reader.ReadInt32();
        ItemId   =e.Reader.ReadInt32();
        Tech     =e.Reader.ReadInt32();
        Quantity = e.Reader.ReadInt32();


    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(PlayerId);
        e.Writer.Write(ItemId  );
        e.Writer.Write(Tech    );
        e.Writer.Write(Quantity);
    }
}
