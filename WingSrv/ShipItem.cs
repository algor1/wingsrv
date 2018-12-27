using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipItem {
    public int item_id;

	public float max_speed;
	public float rotation_speed;
    public float acceleration_max;
	public float newSpeed;

	public float hull_full;
	public float armor_full;
	public float shield_full;
	public float capasitor_full;

	public float hull;
	public float armor;
	public float shield;
	public float capasitor;

	public float hull_restore;
	public float armor_restore;
	public float shield_restore;
	public float capasitor_restore;
	

	public float agr_distance;
	public float vision_distance;
	
	public bool mob;

	public float warpDriveStartTime;
    public float warpSpeed;
	public int weaponPoints ;
	public int equipmentPoints ;


}