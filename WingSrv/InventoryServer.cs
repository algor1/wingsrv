using UnityEngine;
using System.Collections.Generic;

namespace Wingsrv
{

    class InventoryServer : MonoBehaviour
    {
        public bool started;
        void Start()
        {
            started = true;
        }

        public List<InventoryItem> PlayerInventory(int player_id, int holder_id)
        {
            return GetComponent<ItemDB>().GetInventory(player_id, holder_id);
        }
        public List<InventoryItem> ObjectInventory(int holder_id)
        {
            return GetComponent<ItemDB>().GetObjectInventory(holder_id);
        }

        public Item GetItem(int item_id)
        {
            return GetComponent<ItemDB>().GetItem(item_id);
        }
        public ShipItem GetShipItem(int item_id)
        {
            return GetComponent<ItemDB>().GetShipItem(item_id);
        }


        public void AddToInventory(int player_id, int holder_id, int item_id, int tech, int quantity)
        {
            GetComponent<ItemDB>().InventoryAdd(player_id, holder_id, item_id, tech, quantity);
        }

        public bool DeleteFromInventory(int player_id, int holder_id, int item_id, int tech, int quantity)
        {
            return GetComponent<ItemDB>().InventoryDelete(player_id, holder_id, item_id, tech, quantity);
        }

        public void DestroyInventory(int holder_id)
        {
            GetComponent<ItemDB>().DeleteHolderInventory(holder_id);
        }

        public void MoveItem(int fromPlayer_id, int fromHolder_id, int toPlayer_id, int toHolder_id, int item_id, int tech, int quantity)
        {
            if (DeleteFromInventory(fromPlayer_id, fromHolder_id, item_id, tech, quantity))
            {
                AddToInventory(toPlayer_id, toHolder_id, item_id, tech, quantity);
            }
        }
        public void ContainerFromShip(SpaceObject cont, SpaceObject ship)
        {
            List<InventoryItem> objectInventory = ObjectInventory(ship.id);
            for (int i = 0; i < objectInventory.Count; i++)
            {

                if (Random.value > 0.8)
                {
                    //				float tmp_quantity = (float)objectInventory [i].quantity;
                    MoveItem(objectInventory[i].player_id, ship.id, 0, cont.id, objectInventory[i].item_id, objectInventory[i].tech, Mathf.CeilToInt(objectInventory[i].quantity * Random.value * 0.3f));
                }

            }
            AddToInventory(0, cont.id, 38, 0, Mathf.CeilToInt(10 * Random.value));
            DestroyInventory(ship.id);
        }

    }
}