

public enum ItemType { ship, weapon, equipment, material, other, container, ore };


public class Item 
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public string SpritePath { get; set; }
    
    public ItemType ItemType { get; set; }
    public float Volume { get; set; }
    public string Prefab { get; set; }

    
}