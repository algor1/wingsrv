

using DarkRift;

namespace Inventory
{
    public enum ItemTypes { ship, weapon, equipment, material, other, container, ore };


    public class Item : IDarkRiftSerializable
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string SpritePath { get; set; }

        public ItemTypes ItemType { get; set; }
        public float Volume { get; set; }
        public string Prefab { get; set; }


        public void Deserialize(DeserializeEvent e)
        {
            Id = e.Reader.ReadInt32();
            ItemName = e.Reader.ReadString();
            SpritePath = e.Reader.ReadString();
            ItemType = (ItemTypes)e.Reader.ReadInt32();
            Volume = e.Reader.ReadInt32();
            Prefab = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Id);
            e.Writer.Write(ItemName);
            e.Writer.Write(SpritePath);
            e.Writer.Write((int)ItemType);
            e.Writer.Write(Volume);
            e.Writer.Write(Prefab);
        }
    }
}