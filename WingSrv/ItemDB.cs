using UnityEngine;
using System.Collections;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace Wingsrv {

    public class ItemDB
    {
        public bool started;
        private IDbConnection dbSkillCon;
        private IDataReader reader;
        private IDbCommand dbcmd;
        private Game gamePlugin;

        public ItemDB(Game game) {
            started = true;
            gamePlugin = game;
        }

        public void OnApplicationQuit()
        {
            if (dbSkillCon != null) dbSkillCon.Close();
        }
        private void InitDB()
        {
            string p = "inventory.db";
            string filepath = "./Data/Game/DB/" + p; 
            string connectionString = "URI=file:" + filepath;
            gamePlugin.WriteToLog(connectionString,DarkRift.LogType.Info);
            dbSkillCon = (IDbConnection)new SqliteConnection(connectionString);
            dbSkillCon.Open();
        }
        private void GetReader(string dbselect)
        {
            if (dbSkillCon == null)
            {
                InitDB();
            }

            dbcmd = dbSkillCon.CreateCommand();

            dbcmd.CommandText = dbselect;
            // ��������� ������
            reader = dbcmd.ExecuteReader();

        }


        public Item GetItem(int item_id)
        {
            Item returnItem = new Item();
            string qwery = "SELECT items.id,item,item_type.type, prefab FROM items inner join item_type  where items.id=" + item_id.ToString() + " and items.type_id=item_type.id";

            GetReader(qwery);
            while (reader.Read())
            {
                if (!reader.IsDBNull(0)) returnItem.id = reader.GetInt32(0);
                if (!reader.IsDBNull(1)) returnItem.item = reader.GetString(1);
                if (!reader.IsDBNull(2)) returnItem.itemType = (Item.Type_of_item)System.Enum.Parse(typeof(Item.Type_of_item), reader.GetString(2));
                if (!reader.IsDBNull(3)) returnItem.prefab = reader.GetString(3);
            }
            return returnItem;
        }
        public ShipItem GetShipItem(int item_id)
        {
            ShipItem returnShipItem = new ShipItem();
            string qwery = "SELECT item_id,max_speed,rotation_speed, acceleration_max,hull_full,armor_full,shield_full,capasitor_full,hull_restore,armor_restore,shield_restore,capasitor_restore,agr_distance,vision_distance,mob,warpDriveStartTime,warpSpeed  FROM ship_item  where item_id=" + item_id.ToString();

            GetReader(qwery);
            while (reader.Read())
            {
                if (!reader.IsDBNull(0)) returnShipItem.item_id = reader.GetInt32(0);
                if (!reader.IsDBNull(1)) returnShipItem.max_speed = reader.GetFloat(1);
                if (!reader.IsDBNull(2)) returnShipItem.rotation_speed = reader.GetFloat(2);
                if (!reader.IsDBNull(3)) returnShipItem.acceleration_max = reader.GetFloat(3);
                if (!reader.IsDBNull(4)) returnShipItem.hull_full = reader.GetFloat(4);
                if (!reader.IsDBNull(5)) returnShipItem.armor_full = reader.GetFloat(5);
                if (!reader.IsDBNull(6)) returnShipItem.shield_full = reader.GetFloat(6);
                if (!reader.IsDBNull(7)) returnShipItem.capasitor_full = reader.GetFloat(7);
                if (!reader.IsDBNull(8)) returnShipItem.hull_restore = reader.GetFloat(8);
                if (!reader.IsDBNull(9)) returnShipItem.armor_restore = reader.GetFloat(9);
                if (!reader.IsDBNull(10)) returnShipItem.shield_restore = reader.GetFloat(10);
                if (!reader.IsDBNull(11)) returnShipItem.capasitor_restore = reader.GetFloat(11);
                if (!reader.IsDBNull(12)) returnShipItem.agr_distance = reader.GetFloat(12);
                if (!reader.IsDBNull(13)) returnShipItem.vision_distance = reader.GetFloat(13);
                if (!reader.IsDBNull(14)) returnShipItem.mob = (reader.GetInt32(14) == 1);
                if (!reader.IsDBNull(15)) returnShipItem.warpDriveStartTime = reader.GetFloat(15);
                if (!reader.IsDBNull(16)) returnShipItem.warpSpeed = reader.GetFloat(16);
            }
            return returnShipItem;
        }
        public List<InventoryItem> GetInventory(int player_id, int holder_id)
        {
            List<InventoryItem> returnInventoryItemList = new List<InventoryItem>();
            string qwery = "SELECT item_id,tech,quantity FROM inventory where player_id=" + player_id.ToString() + " and inventory_holder_id = " + holder_id.ToString();
            GetReader(qwery);

            while (reader.Read())
            {
                InventoryItem _inventoryItem = new InventoryItem();
                if (!reader.IsDBNull(0)) _inventoryItem.item_id = reader.GetInt32(0);
                if (!reader.IsDBNull(1)) _inventoryItem.tech = reader.GetInt32(1);
                if (!reader.IsDBNull(2)) _inventoryItem.quantity = reader.GetInt32(2);
                returnInventoryItemList.Add(_inventoryItem);
            }
            return returnInventoryItemList;
        }

        public List<InventoryItem> GetObjectInventory(int holder_id)
        {
            List<InventoryItem> returnInventoryItemList = new List<InventoryItem>();
            string qwery = "SELECT  item_id,tech,quantity, player_id FROM inventory where inventory_holder_id = " + holder_id.ToString();
            GetReader(qwery);

            while (reader.Read())
            {
                InventoryItem _inventoryItem = new InventoryItem();
                if (!reader.IsDBNull(0)) _inventoryItem.item_id = reader.GetInt32(0);
                if (!reader.IsDBNull(1)) _inventoryItem.tech = reader.GetInt32(1);
                if (!reader.IsDBNull(2)) _inventoryItem.quantity = reader.GetInt32(2);
                if (!reader.IsDBNull(3)) _inventoryItem.player_id = reader.GetInt32(3);
                returnInventoryItemList.Add(_inventoryItem);
            }
            return returnInventoryItemList;
        }

        public void InventoryAdd(int player_id, int holder_id, int item_id, int tech, int quantity)
        {
            string qwery;
            if (Quantity(player_id, holder_id, item_id, tech) == 0)
            {
                qwery = "insert into inventory (player_id,inventory_holder_id,item_id,tech,quantity) values (" + player_id.ToString() + "," + holder_id.ToString() + "," + item_id.ToString() + "," + tech.ToString() + "," + quantity.ToString() + ")";


            }
            else
            {

                qwery = "update inventory set quantity = quantity +" + quantity.ToString() + " where player_id=" + player_id.ToString() + " and item_id=" + item_id.ToString() + " and inventory_holder_id=" + holder_id.ToString() + "  and tech=" + tech.ToString();

            }
            GetReader(qwery);
        }

        public bool InventoryDelete(int player_id, int holder_id, int item_id, int tech, int quantity)
        {
            bool deleted = false;
            string qwery;
            int quantityInInv = Quantity(player_id, holder_id, item_id, tech);
            if (quantityInInv == quantity)
            {
                qwery = "delete from inventory where (player_id=" + player_id.ToString() + " and item_id=" + item_id.ToString() + " and inventory_holder_id=" + holder_id.ToString() + "  and tech=" + tech.ToString() + " )";
                deleted = true;
                GetReader(qwery);

            } else if (quantityInInv > quantity) {
                qwery = "update inventory set quantity = quantity -" + quantity.ToString() + " where player_id=" + player_id.ToString() + " and item_id=" + item_id.ToString() + " and inventory_holder_id=" + holder_id.ToString() + "  and tech=" + tech.ToString() + " )";
                deleted = true;
                GetReader(qwery);
            }
            return deleted;
        }
        public void DeleteHolderInventory(int holder_id)
        {
            string qwery;
            qwery = "delete from inventory where (inventory_holder_id=" + holder_id.ToString() + " )";
            GetReader(qwery);
        }
        public int Quantity(int player_id, int holder_id, int item_id, int tech) {
            int retquantity = 0;
            string qwery = "SELECT quantity FROM inventory where player_id=" + player_id.ToString() + " and inventory_holder_id = " + holder_id.ToString() + " and item_id=" + item_id.ToString() + "  and tech=" + tech.ToString();
            GetReader(qwery);
            while (reader.Read())
            {
                if (!reader.IsDBNull(0)) retquantity = reader.GetInt32(0);
            }
            return retquantity;
        }

    }
}