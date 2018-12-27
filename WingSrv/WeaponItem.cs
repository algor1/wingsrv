using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponItem{
    public int item_id;
	public enum WeaponType {laser,missile,projective};
    public WeaponType type;
	public bool active;	
	public float damage;
    public float reload;
	public float ammoSpeed;
    public float activeTime;//for laser
	public float sqrDistanse_max;
	public float capasitor_use;
}
