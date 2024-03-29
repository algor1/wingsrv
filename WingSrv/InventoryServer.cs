﻿using UnityEngine;
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

        //private Server server;
        private Game gamePlugin;
        public DatabaseProxy _database { get; set; }
        System.Random autoRand = new System.Random();
        const float ContainerPersentage = 0.2f;

        private Dictionary<int, Item> items;


        public InventoryServer(Game game)
        {
            gamePlugin = game;
            items = new Dictionary<int, Item>();

        }
        public void RunServer()
        {
            if (!started)
            {
                LoadItems();

                started = true;
            }
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

        public List<Item> GetItems(List<int> itemIds)
        {
            List<Item> retItems = new List<Item>();
            foreach (var itemId in itemIds)
            {
                retItems.Add(items[itemId]);
            }
            return retItems;
        }


        #endregion 

        #region Inventory

        public List<InventoryItem> PlayerInventory(int playerId, int holderId)
        {
            List<InventoryItem> retList = new List<InventoryItem>();

            try
            {
                _database.DataLayer.GetPlayerInventory(playerId, holderId, playerInventory =>
                {
                    retList = playerInventory;
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on getting items of player inventory" + ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null, 0, ex);
            }
            return retList;
        }
        public List<InventoryItem> HolderInventory(int holderId)
        {
            Console.WriteLine("trying to get inventory of holder id: {0}", holderId);
            List<InventoryItem> retList = new List<InventoryItem>();
            try
            {
                _database.DataLayer.GetHolderInventory(holderId, holderInventory =>
               {
               Console.WriteLine("holder id {0} has {1} items" , holderId, holderInventory.Count);
                    retList= holderInventory;
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on getting items of holder inventory" + ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null, 0, ex);
            }
            return retList;
        }


        private void InventoryItemAddition(int player, int holder, int itemId, int quantity)
        {
            try
            {
                _database.DataLayer.InventoryItemAdd(player,holder,itemId,quantity, quantityAdded =>
                {
                    Console.WriteLine("added {0} items id:{1} to player {2} holder {3} ", quantity, itemId, player,holder);
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on adding items to inventory" + ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null, 0, ex);
            }
        }

        private void InventoryItemMove(int senderId, int senderHolder, int receiverId, int receiverHolder, int itemID, int quantity)
        {
            //TODO CheckVolume
            _database.DataLayer.InventoryItemMove( senderId, senderHolder, receiverId, receiverHolder, itemID, quantity, complited  =>
                {
                    if (complited) 
                    {
                        Console.WriteLine("moved {0} items id:{1} from player {2} to {3} from holder {4} to {5}",quantity,itemID,senderId,senderHolder,receiverId,receiverHolder);
                    }
                    else
                    {
                        Console.WriteLine("Failed to move {0} items id:{1} from player {2} to {3} from holder {4} to {5}",quantity,itemID,senderId,senderHolder,receiverId,receiverHolder);
                    }
             });
        }

        public void ContainerInventoryFromShip(SpaceObject cont, SpaceObject so)
        {
            List<InventoryItem> holderInventory = HolderInventory(so.Id);
            for (int i = 0; i < holderInventory.Count; i++)
            {

                if (autoRand.NextDouble() < ContainerPersentage )
                {
                    //				float tmp_quantity = (float)objectInventory [i].quantity;
                    InventoryItemMove(so.PlayerId, so.Id, cont.PlayerId, cont.Id, holderInventory[i].ItemId, (int)Math.Ceiling(holderInventory[i].Quantity * autoRand.NextDouble() * 0.5f));
                }

            }
            int metalScrapsId = 38; //TODO 
            InventoryItemAddition(cont.PlayerId, cont.Id, metalScrapsId,(int) Math.Ceiling(10 * autoRand.NextDouble()));
            
            DestroyInventory(so.Id);
        }
        private void DestroyInventory(int holderId)
        {
            try
            {
                _database.DataLayer.DeleteHolderInventory(holderId , () =>
                {
                    Console.WriteLine("Inventory destroyed holderId {0} ", holderId);
                });
            }
            catch (Exception ex)
            {
                gamePlugin.WriteToLog("Database error on destroing inventory holder" + ex, DarkRift.LogType.Error);

                //Return Error 2 for Database error
                _database.DatabaseError(null, 0, ex);
            }
        }


   

        
        //private InventoryItem GetInventoryItem(int player,int holder,int itemId , int tech)
        //{
        //    return inventories[holder][player][itemId];
        //}
        
  

        //private void UpdateItemInInventory(int playerId, int holder, InventoryItem itemToUpdate)
        //{
        //    if (!inventories.ContainsKey(holder))
        //    {
        //        inventories.Add(holder, new Dictionary<int, Dictionary<int, InventoryItem>>());
        //    }
        //    if (!inventories[holder].ContainsKey(playerId))
        //    {
        //        inventories[holder].Add(playerId, new Dictionary<int, InventoryItem>());
        //    }
        //    inventories[holder][playerId][itemToUpdate.ItemId]= itemToUpdate;
        //}
        

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