

public enum ItemType { ship, weapon, equipment, material, other, container, ore };


public class Item 
{
    public int Id { get; }
    public string Item { get; }
    public string SpritePath { get; }
    
    public ItemType ItemType { get; }
    public float Volume { get; }
    public string Prefab { get; }

    
}