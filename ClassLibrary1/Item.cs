using UnityEngine;
 
//[System.Serializable]
public class Item 
{
    public int id;
    public string item;
    public Sprite itemSprite;
    public enum Type_of_item { ship, weapon,equipment,material, other,container };
    public Type_of_item itemType;
    public float volume;
	public string prefab;

    
}