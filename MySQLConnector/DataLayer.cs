using System;
using System.Collections.Generic;
using Database;
using MySql.Data.MySqlClient;
using SpaceObjects;
using UnityEngine;

namespace MySQLConnector
{
    internal class DataLayer : IDataLayer
    {
        private readonly MySqlConnector _database;

        public DataLayer(string name, MySqlConnector database)
        {
            Name = name;
            _database = database;
        }

        public string Name { get; }

        public void GetUser(string username, Action<IUser> callback)
        {
            var row = _database.ExecuteQuery(
                "SELECT ID, username, password FROM Users WHERE username = @userName LIMIT 1;",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            callback(new User((string)row[0]["username"], (string)row[0]["password"]));
        }

        public void UsernameAvailable(string username, Action<bool> callback)
        {
            var row = _database.ExecuteScalar(
                "SELECT ID FROM Users WHERE username = @userName",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            callback(row == null);
        }

        public void AddNewUser(string username, string password, Action callback)
        {
            _database.ExecuteNonQuery(
                "INSERT INTO Users(username,password) VALUES(@userName,@pass)",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)),
                new QueryParameter("@pass", MySqlDbType.VarChar, 255, "password", _database.EscapeString(password)));
            callback();
        }

        public void DeleteUser(string username, Action callback)
        {
            _database.ExecuteNonQuery(
                "DELETE FROM Users WHERE username = @userName",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            callback();
        }

        public void AddRequest(string sender, string receiver, Action callback)
        {
            _database.ExecuteNonQuery(
                "INSERT INTO Friends(user, request) " +
                "VALUES(@user, @request)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(receiver)));
            callback();
        }

        public void RemoveRequest(string sender, string receiver, Action callback)
        {
            //Sender declined receivers request
            _database.ExecuteNonQuery(
                "DELETE FROM Friends " +
                "WHERE(user = @user AND request = @request)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(receiver)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(sender)));

            callback();
        }

        public void AddFriend(string sender, string receiver, Action callback)
        {
            //Sender accepted receivers request
            _database.ExecuteNonQuery(
                "UPDATE Friends " +
                "SET friend = @friend, request = NULL " +
                "WHERE(user = @user AND request = @request)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(receiver)),
                new QueryParameter("@friend", MySqlDbType.VarChar, 160, "friend", _database.EscapeString(sender)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(sender)));

            callback();
        }

        public void RemoveFriend(string sender, string receiver, Action callback)
        {
            _database.ExecuteNonQuery(
                "DELETE FROM Friends " +
                "WHERE(user = @user AND (friend = @friend OR request = @request)) " +
                "OR (user = @notFriend AND friend = @isFriend)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@friend", MySqlDbType.VarChar, 60, "friend", _database.EscapeString(receiver)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(receiver)),
                new QueryParameter("@notFriend", MySqlDbType.VarChar, 60, "user", _database.EscapeString(receiver)),
                new QueryParameter("@isFriend", MySqlDbType.VarChar, 60, "friend", _database.EscapeString(sender)));
            callback();
        }

        public void GetFriends(string username, Action<IFriendList> callback)
        {
            var rows = _database.ExecuteQuery(
                "SELECT user, friend, request FROM Friends WHERE user = @user OR friend = @friend OR request = @request;",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(username)),
                new QueryParameter("@friend", MySqlDbType.VarChar, 60, "friend", _database.EscapeString(username)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(username)));


            var friends = new List<string>();
            var outRequests = new List<string>();
            var inRequests = new List<string>();

            foreach (var row in rows)
            {
                var relation = row.GetRow();
                var user = (string)relation["user"];
                var friend = (string)relation["friend"];
                var request = (string)relation["request"];

                if (user == username)
                {
                    if (!string.IsNullOrEmpty(friend))
                    {
                        friends.Add(friend);
                    }
                    else
                    {
                        outRequests.Add(request);
                    }
                }
                else if (friend == username)
                {
                    if (!string.IsNullOrEmpty(user))
                    {
                        friends.Add(user);
                    }
                }
                else
                {
                    if (request == username)
                    {
                        inRequests.Add(user);
                    }
                }
            }

            callback(new FriendList(friends, inRequests, outRequests));
        }

        public void GetSpaceObject(int _id, Action<SpaceObject> callback)
        {
            var rows = _database.ExecuteQuery(
                @"SELECT id,
                    type,
                    visibleName,
                    position_x,
                    position_y,
                    position_z,
                    rotation_x,
                    rotation_y,
                    rotation_z,
                    rotation_w,
                    speed,
                    prefab_path
                    FROM server_objects
                    where id = @id",
                new QueryParameter("@id", MySqlDbType.Int32, 32, "id", _id));

            var data = rows[0].GetRow();
            SpaceObject returnSO = new SpaceObject();
            int _type = 0;
            returnSO.Id = (int)data["id"];
            returnSO.Type = (TypeSO)data["type"];
            returnSO.VisibleName = (String)data["visibleName"];
            returnSO.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
            returnSO.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
            returnSO.Speed = (float)data["speed"];
            returnSO.Prefab = (string)data["prefab_path"];

            callback(returnSO);
        }

        public void GetAllShips( Action<List<ShipData>> callback)
        {
            Console.WriteLine("01");
            List<ShipData> retListShip = new List<ShipData>();
            var rows = _database.ExecuteQuery(@"SELECT 
                            server_objects.id,
                            server_objects.type,
                            server_objects.visibleName,
                            server_objects.position_x,
                            server_objects.position_y,
                            server_objects.position_z,
                            server_objects.rotation_x,
                            server_objects.rotation_y,
                            server_objects.rotation_z,
                            server_objects.rotation_w,
                            server_objects.speed,
                            server_objects.prefab_path,
                            SO_shipdata.max_speed,
                            SO_shipdata.rotation_speed,
                            SO_shipdata.acceleration_max,
                            SO_shipdata.newSpeed,
                            SO_shipdata.hull_full,
                            SO_shipdata.armor_full,
                            SO_shipdata.shield_full,
                            SO_shipdata.capasitor_full,
                            SO_shipdata.hull,
                            SO_shipdata.armor,
                            SO_shipdata.shield,
                            SO_shipdata.capasitor,
                            SO_shipdata.hull_restore,
                            SO_shipdata.armor_restore,
                            SO_shipdata.shield_restore,
                            SO_shipdata.capasitor_restore,
                            SO_shipdata.agr_distance,
                            SO_shipdata.vision_distance,
                            SO_shipdata.destroyed,
                            SO_shipdata.hidden,
                            SO_shipdata.mob,
                            SO_shipdata.warpDriveStartTime,
                            SO_shipdata.warpSpeed
                        FROM SO_shipdata INNER JOIN server_objects
                        ON SO_shipdata.SO_id=server_objects.id");

            Console.WriteLine("02");

            foreach (var row in rows)
            {
                Console.WriteLine("03");

                ShipData retShipData = new ShipData();

                var data = row.GetRow();
                Console.WriteLine("04");
                int j = 0;
                retShipData.Id = (int)data["id"];
                Console.WriteLine("00"+j++);
                retShipData.Type = TypeSO.ship; //(TypeSO)(int)data["type"];
                Console.WriteLine("00" + j++);

                retShipData.VisibleName = (string)data["visibleName"];
                Console.WriteLine("00" + j++);
                retShipData.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
                Console.WriteLine("00" + j++);
                retShipData.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
                Console.WriteLine("00" + j++);
                retShipData.Speed = (float)data["speed"];
                Console.WriteLine("00" + j++);
                retShipData.Prefab = (string)data["prefab_path"];
                Console.WriteLine("00" + j++);
                retShipData.SpeedMax = (float)data["max_speed"];
                Console.WriteLine("00" + j++);
                retShipData.RotationSpeed = (float)data["rotation_speed"];
                Console.WriteLine("00" + j++);
                retShipData.AccelerationMax = (float)data["acceleration_max"];
                Console.WriteLine("00" + j++);
                retShipData.SpeedNew = (float)data["newSpeed"];
                Console.WriteLine("00" + j++);
                retShipData.Hull_full = (float)data["hull_full"];
                Console.WriteLine("00" + j++);
                retShipData.Armor_full = (float)data["armor_full"];
                Console.WriteLine("00" + j++);
                retShipData.Shield_full = (float)data["shield_full"];
                Console.WriteLine("00" + j++);
                retShipData.Capasitor_full = (float)data["capasitor_full"];
                Console.WriteLine("00" + j++);
                retShipData.Hull = (float)data["hull"];
                Console.WriteLine("00" + j++);
                retShipData.Armor = (float)data["armor"];
                Console.WriteLine("00" + j++);
                retShipData.Shield = (float)data["shield"];
                Console.WriteLine("00" + j++);
                retShipData.Capasitor = (float)data["capasitor"];
                Console.WriteLine("00" + j++);
                retShipData.Hull_restore = (float)data["hull_restore"];
                Console.WriteLine("00" + j++);
                retShipData.Armor_restore = (float)data["armor_restore"];
                Console.WriteLine("00" + j++);
                retShipData.Shield_restore = (float)data["shield_restore"];
                Console.WriteLine("00" + j++);
                retShipData.Capasitor_restore = (float)data["capasitor_restore"];
                Console.WriteLine("00" + j++);
                retShipData.AgrDistance = (float)data["agr_distance"];
                Console.WriteLine("00" + j++);
                retShipData.VisionDistance = (float)data["vision_distance"];
                Console.WriteLine("00" + j++);
                Console.WriteLine(data["destroyed"].GetType());
                retShipData.Destroyed = (bool)data["destroyed"];
                Console.WriteLine("00" + j++);


                retShipData.Hidden = (bool)data["hidden"];
                Console.WriteLine("00" + j++);
                retShipData.Mob = (bool)data["mob"];
                Console.WriteLine("00" + j++);
                retShipData.WarpDriveStartTime = (float)data["warpDriveStartTime"];
                Console.WriteLine("00" + j++);
                retShipData.WarpSpeed = (float)data["warpSpeed"];
                Console.WriteLine("00" + j++);

                retListShip.Add(retShipData);
            }
            for (int i = 0; i < retListShip.Count; i++)
            {
                retListShip[i].Weapons = GetWeapons(retListShip[i].Id);
                retListShip[i].Equipments = GetEquip(retListShip[i].Id);
            }
            
            callback(retListShip);

        }
        private EquipmentData[] GetEquip(int Ship_id)
        {
            EquipmentData[] retList = new EquipmentData[0];
            return retList;
        }

        private WeaponData[] GetWeapons(int ship_id)
        {
            List<WeaponData> retList = new List<WeaponData>();
 
            var rows = _database.ExecuteQuery(@"SELECT 
					WeaponType, 
					damage, 
					reload, 
					ammoSpeed, 
					activeTime, 
					sqrDistanse_max, 
					capasitor_use
					FROM SO_weapondata
					WHERE SO_id=@id",
                new QueryParameter("@id", MySqlDbType.Int32, 32, "id", ship_id));

            foreach (var row in rows)
            {
                WeaponData weapon = new WeaponData();
                var data = row.GetRow();

                weapon.Type = (WeaponData.WeaponType)data["WeaponType"];
                weapon.Damage = (float)data["damage"];
                weapon.Reload = (float)data["reload"];
                weapon.AmmoSpeed = (float)data["ammoSpeed"];
                weapon.ActiveTime = (float)data["activeTime"];
                weapon.SqrDistanse_max = (float)data["sqrDistanse_max"];
                weapon.Capasitor_use = (float)data["capasitor_use"];

                retList.Add(weapon);
            }
            
            return retList.ToArray();


        }
    }
}