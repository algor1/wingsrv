using UnityEngine;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using SpaceObjects;
using Database;
using System;

namespace Wingsrv
{

    public class InventoryServer
    {
        public bool started;

        private Server server;
        private Game gamePlugin;
        public DatabaseProxy _database { get; set; }

        private Dictionary<int, Item> items;
        private Dictionary<int,Dictionary<int,Dictionary<int, InventoryItem>>> inventories; //<holder,<player<itemID,InvetoryItem>>>




        public InventoryServer(Game game)
        {
            started = true;
            gamePlugin = game;
            items = new Dictionary<int, Item>();
            inventories = new Dictionary<int, Dictionary<int, Dictionary<int, InventoryItem>>>();
        }

        #region Items

        private void LoadItems ()
        {
        try
            {
                _database.DataLayer.GetAllItems( itemList =>
                {
                    for (int i = 0; i < itemList.Count; i++)
                        {
                            items.Add(itemList[i].Id,itemList[i]);
                        }
                        //if (_debug)
                        //   {
                              gamePlugin.WriteToLog("Items loaded count:"+itemList.Count, DarkRift.LogType.Info);
                        //   }
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading items" +ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null , 0 , ex);
            }
        }

        public Item GetItem(int itemId)
        {
            return items[itemId];
        }

        #endregion 

        #region Inventory

        //public List<InventoryItem> PlayerInventory(int player_id, int holderId)
        //{

        //}

        //public List<InventoryItem> ObjectInventory(int holderId)
        //{

        //}

        public void LoadPlayerInventory(int playerId)
        {

            try
            {
                _database.DataLayer.GetPlayerInventory( playerId, inventoryDict =>
                {
                    foreach (KeyValuePair<int, List<InventoryItem>> entry in inventoryDict)
                    {
                        if (!inventories.ContainsKey(entry.Key))
                        {
                            inventories.Add(entry.Key, new Dictionary<int, Dictionary<int, InventoryItem>>());
                        }
                        if (!inventories[entry.Key].ContainsKey(playerId))
                        {
                            inventories[entry.Key].Add(playerId, new Dictionary<int, InventoryItem>());
                        }
                        foreach (InventoryItem inventoryItem in entry.Value)
                        {

                            inventories[entry.Key][playerId].Add(inventoryItem.ItemId, inventoryItem);
                        }
                    }
                    //if (_debug)
                    //   {
                    gamePlugin.WriteToLog("inventory holders loaded count:"+inventoryDict.Count, DarkRift.LogType.Info);
                    //   }
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on loading items" +ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null , 0 , ex);
            }
        
        }
        private void InventoryItemAddition(int player,int holder,InventoryItem itemToAdd)
        {
            InventoryItem item= GetInventoryItem(player,holder,itemToAdd.ItemId,itemToAdd.Tech);
            if (item==null)
                AddItemToInventory(player,holder,itemToAdd);
            else 
                UpdateItemInInventory(player,holder,itemToAdd);
        }
        
        private InventoryItem GetInventoryItem(int player,int holder,int itemId , int tech)
        {
            return inventories[holder][player][itemId];
        }
        
        private void AddItemToInventory (int playerId, int holder, InventoryItem itemToAdd)
        {
            
            if (!inventories.ContainsKey(holder))
            {
                inventories.Add(holder, new Dictionary<int, Dictionary<int, InventoryItem>>());
            }
            if (!inventories[holder].ContainsKey(playerId))
            {
                inventories[holder].Add(playerId, new Dictionary<int, InventoryItem>());
            }
            inventories[holder][playerId].Add(itemToAdd.ItemId, itemToAdd);
        }

        private void UpdateItemInInventory(int playerId, int holder, InventoryItem itemToUpdate)
        {
            if (!inventories.ContainsKey(holder))
            {
                inventories.Add(holder, new Dictionary<int, Dictionary<int, InventoryItem>>());
            }
            if (!inventories[holder].ContainsKey(playerId))
            {
                inventories[holder].Add(playerId, new Dictionary<int, InventoryItem>());
            }
            inventories[holder][playerId][itemToUpdate.ItemId]= itemToUpdate;
        }
        

        #endregion





      
   
        //public ShipItem GetShipItem(int item_id)
        //{
        //    return itemDB.GetShipItem(item_id);
        //}


        //public void AddToInventory(int player_id, int holder_id, int item_id, int tech, int quantity)
        //{
        //    itemDB.InventoryAdd(player_id, holder_id, item_id, tech, quantity);
        //}

        //public bool DeleteFromInventory(int player_id, int holder_id, int item_id, int tech, int quantity)
        //{
        //    return itemDB.InventoryDelete(player_id, holder_id, item_id, tech, quantity);
        //}

        //public void DestroyInventory(int holder_id)
        //{
        //    itemDB.DeleteHolderInventory(holder_id);
        //}

        //public void MoveItem(int fromPlayer_id, int fromHolder_id, int toPlayer_id, int toHolder_id, int item_id, int tech, int quantity)
        //{
        //    if (DeleteFromInventory(fromPlayer_id, fromHolder_id, item_id, tech, quantity))
        //    {
        //        AddToInventory(toPlayer_id, toHolder_id, item_id, tech, quantity);
        //    }
        //}
        //public void ContainerFromShip(SpaceObject cont, SpaceObject ship)
        //{
        //    List<InventoryItem> objectInventory = ObjectInventory(ship.Id);
        //    for (int i = 0; i < objectInventory.Count; i++)
        //    {

        //        if (Random.value > 0.8)
        //        {
        //            //				float tmp_quantity = (float)objectInventory [i].quantity;
        //            MoveItem(objectInventory[i].player_id, ship.Id, 0, cont.Id, objectInventory[i].item_id, objectInventory[i].tech, Mathf.CeilToInt(objectInventory[i].quantity * Random.value * 0.3f));
        //        }

        //    }
        //    AddToInventory(0, cont.Id, 38, 0, Mathf.CeilToInt(10 * Random.value));
        //    DestroyInventory(ship.Id);
        //}

    }
}