using System;
using System.Collections.Generic;
using SpaceObjects;


namespace Database
{
    public interface IDataLayer
    {
        string Name { get; }

        #region Login

        void GetUser(string username, Action<IUser> callback);
        void UsernameAvailable(string username, Action<bool> callback);
        void AddNewUser(string username, string password, Action callback);
        void DeleteUser(string username, Action callback);

        #endregion

        #region Friends

        void AddRequest(string sender, string receiver, Action callback);
        void RemoveRequest(string sender, string receiver, Action callback);
        void AddFriend(string sender, string receiver, Action callback);
        void RemoveFriend(string sender, string receiver, Action callback);
        void GetFriends(string username, Action<IFriendList> callback);

        #endregion

        #region Space

        void GetAllMOBShips( Action<List<ShipData>> callback);
        void GetShip(int ship_id, Action<ShipData> callback);
        void AddNewShip (ShipData shipData, Action<int> callback);
        void SetShip(ShipData shipData, Action callback);
        void AddNewPlayerAndShip(string player, Action callback);
        void GetPlayerActiveShip(string player, Action<int> callback);
        void AddNewContainer(SpaceObject so, Action<SpaceObject> callback);
        void DeleteContainer(int id, Action callback);
        void GetSpaceObject(int _id, Action<SpaceObject> callback);
        void GetAllSpaceObjects ( Action<List<SpaceObject>> callback);

        #endregion

        #region Inventory
        void GetAllItems(Action<List<Item>> callback);
        void GetPlayerInventory(int playerId, int holderId, Action<List<InventoryItem>> callback);
        void GetHolderInventory(int holderId, Action<List<InventoryItem>> callback);
        void DeleteHolderInventory(int holderId, Action callback);
        void GetPlayerId(string player, Action<int> callback);
        void InventoryItemMove(int senderId, int senderHolder, int receiverId, int receiverHolder, int itemID, int quantity, Action<bool> callback);
        void InventoryItemAdd(int receiverId, int receiverHolder, int itemId, int quantity, Action<int> callback);
        void InventoryItemSubtract(int playerId, int HolderId, int itemId, int quantity, Action<int> callback);
        
        #endregion
    }
}