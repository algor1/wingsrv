﻿using System;
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


        private int AddNewSpaceObject(SpaceObject so)
        {
            long _id =_database.ExecuteInsert(
                @"INSERT INTO server_objects (
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
                            prefab_path) 
                            VALUES(

                            @type,
                            @visibleName,
                            @position_x,
                            @position_y,
                            @position_z,
                            @rotation_x,
                            @rotation_y,
                            @rotation_z,
                            @rotation_w,
                            @speed,
                            @prefab_path)",
                            new QueryParameter("@type",        MySqlDbType.Int32, 32, "type",(int)so.Type       ),
                            new QueryParameter("@visibleName", MySqlDbType.VarChar, 250, "visibleName",so.VisibleName ),
                            new QueryParameter("@position_x",  MySqlDbType.Float, 32, "position_x", so.Position.x),
                            new QueryParameter("@position_y", MySqlDbType.Float, 32, "position_y", so.Position.y),
                            new QueryParameter("@position_z", MySqlDbType.Float, 32, "position_z", so.Position.z),
                            new QueryParameter("@rotation_x", MySqlDbType.Float, 32, "rotation_x", so.Rotation.x),
                            new QueryParameter("@rotation_y", MySqlDbType.Float, 32, "rotation_y", so.Rotation.y),
                            new QueryParameter("@rotation_z", MySqlDbType.Float, 32, "rotation_z", so.Rotation.z),
                            new QueryParameter("@rotation_w", MySqlDbType.Float, 32, "rotation_w", so.Rotation.w),
                            new QueryParameter("@speed", MySqlDbType.Float, 32, "speed", so.Speed),
                            new QueryParameter("@prefab_path", MySqlDbType.VarChar, 250, "prefab_path",so.Prefab ));
         
         //var id = _database.ExecuteScalar("SELECT LAST_INSERT_ID()");
         return (int)_id ;                
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
            returnSO.Id = (int)data["id"];
            returnSO.Type = (TypeSO)data["type"];
            returnSO.VisibleName = (String)data["visibleName"];
            returnSO.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
            returnSO.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
            returnSO.Speed = (float)data["speed"];
            returnSO.Prefab = (string)data["prefab_path"];

            callback(returnSO);
        }

        public void GetAllSpaceObjects( Action<List<SpaceObject>> callback)
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
                    where type <> 1 and id>=100");//not a ship and id<100 only for copying TODO find another way
            
            List<SpaceObject> returnList=new List<SpaceObject>();
            foreach (var row in rows)
            {
                SpaceObject returnSO = new SpaceObject();
                var data = row.GetRow();
                returnSO.Id = (int)data["id"];
                returnSO.Type = (TypeSO)data["type"];
                returnSO.VisibleName = (String)data["visibleName"];
                returnSO.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
                returnSO.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
                returnSO.Speed = (float)data["speed"];
                returnSO.Prefab = (string)data["prefab_path"];
                returnList.Add(returnSO);
            }
            callback(returnList);
        }

        private void SetSpaceObject(SpaceObject so)
        {
            _database.ExecuteNonQuery(
            @"UPDATE server_objects SET
                        type       = @type,
                        visibleName= @visibleName,
                        position_x = @position_x,
                        position_y = @position_y,
                        position_z = @position_z,
                        rotation_x = @rotation_x,
                        rotation_y = @rotation_y,
                        rotation_z = @rotation_z,
                        rotation_w = @rotation_w,
                        speed      = @speed,
                        prefab_path= @prefab_path
                    WHERE id=@id" ,
                    new QueryParameter("@id", MySqlDbType.Int32, 11, "id", so.Id),
                    new QueryParameter("@type",        MySqlDbType.Int32, 32, "type",(int)so.Type       ),
                    new QueryParameter("@visibleName", MySqlDbType.VarChar, 250, "visibleName",so.VisibleName ),
                    new QueryParameter("@position_x",  MySqlDbType.Float, 32, "position_x", so.Position.x),
                    new QueryParameter("@position_y", MySqlDbType.Float, 32, "position_y", so.Position.y),
                    new QueryParameter("@position_z", MySqlDbType.Float, 32, "position_z", so.Position.z),
                    new QueryParameter("@rotation_x", MySqlDbType.Float, 32, "rotation_x", so.Rotation.x),
                    new QueryParameter("@rotation_y", MySqlDbType.Float, 32, "rotation_y", so.Rotation.y),
                    new QueryParameter("@rotation_z", MySqlDbType.Float, 32, "rotation_z", so.Rotation.z),
                    new QueryParameter("@rotation_w", MySqlDbType.Float, 32, "rotation_w", so.Rotation.w),
                    new QueryParameter("@speed", MySqlDbType.Float, 32, "speed", so.Speed),
                    new QueryParameter("@prefab_path", MySqlDbType.VarChar, 250, "prefab_path",so.Prefab ));
        }

        public void AddNewContainer(SpaceObject so, Action<SpaceObject> callback)
        {
            int id = AddNewSpaceObject(so);
            so.Id = id;
            callback(so);
        }

        public void DeleteContainer(int id, Action callback)
        {
            _database.ExecuteNonQuery(
                @"DELETE FROM server_objects 
                WHERE id=@id",
            new QueryParameter("@id", MySqlDbType.Int32, 11, "id", id));

            callback();

            
            
        }

        public void AddNewShip(ShipData shipData, Action<int> callback)
        {
            int newShip_id=AddNewSpaceObject(shipData);
            Console.WriteLine("new ship id {0} ", newShip_id);
            _database.ExecuteNonQuery(
                @"INSERT INTO SO_shipdata (SO_id,
                        max_speed,
                        rotation_speed,
                        acceleration_max,
                        newSpeed,
                        hull_full,
                        armor_full,
                        shield_full,
                        capasitor_full,
                        hull,
                        armor,
                        shield,
                        capasitor,
                        hull_restore,
                        armor_restore,
                        shield_restore,
                        capasitor_restore,
                        agr_distance,
                        vision_distance,
                        destroyed,
                        hidden,
                        mob,
                        warpDriveStartTime,
                        warpSpeed) 
                 VALUES(
                        @SO_id,
                        @max_speed,
                        @rotation_speed,
                        @acceleration_max,
                        @newSpeed,
                        @hull_full,
                        @armor_full,
                        @shield_full,
                        @capasitor_full,
                        @hull,
                        @armor,
                        @shield,
                        @capasitor,
                        @hull_restore,
                        @armor_restore,
                        @shield_restore,
                        @capasitor_restore,
                        @agr_distance,
                        @vision_distance,
                        @destroyed,
                        @hidden,
                        @mob,
                        @warpDriveStartTime,
                        @warpSpeed)",
                new QueryParameter("@SO_id",MySqlDbType.Int32, 11,"SO_id",newShip_id),
                new QueryParameter("@max_speed", MySqlDbType.Float, 32, "max_speed", shipData.SpeedMax),
                new QueryParameter("@rotation_speed", MySqlDbType.Float, 32, "rotation_speed", shipData.RotationSpeed),
                new QueryParameter("@acceleration_max", MySqlDbType.Float, 32, "acceleration_max", shipData.AccelerationMax),
                new QueryParameter("@newSpeed", MySqlDbType.Float, 32, "newSpeed", shipData.SpeedNew),
                new QueryParameter("@hull_full", MySqlDbType.Float, 32, "hull_full", shipData.Hull_full),
                new QueryParameter("@armor_full", MySqlDbType.Float, 32, "armor_full", shipData.Armor_full),
                new QueryParameter("@shield_full", MySqlDbType.Float, 32, "shield_full", shipData.Shield_full),
                new QueryParameter("@capasitor_full", MySqlDbType.Float, 32, "capasitor_full", shipData.Capasitor_full),
                new QueryParameter("@hull", MySqlDbType.Float, 32, "hull", shipData.Hull),
                new QueryParameter("@armor", MySqlDbType.Float, 32, "armor", shipData.Armor),
                new QueryParameter("@shield", MySqlDbType.Float, 32, "shield", shipData.Shield),
                new QueryParameter("@capasitor", MySqlDbType.Float, 32, "capasitor", shipData.Capasitor),
                new QueryParameter("@hull_restore", MySqlDbType.Float, 32, "hull_restore", shipData.Hull_restore),
                new QueryParameter("@armor_restore", MySqlDbType.Float, 32, "armor_restore", shipData.Armor_restore),
                new QueryParameter("@shield_restore", MySqlDbType.Float, 32, "shield_restore", shipData.Shield_restore),
                new QueryParameter("@capasitor_restore", MySqlDbType.Float, 32, "capasitor_restore", shipData.Capasitor_restore),
                new QueryParameter("@agr_distance", MySqlDbType.Float, 32, "agr_distance", shipData.AgrDistance),
                new QueryParameter("@vision_distance", MySqlDbType.Float, 32, "vision_distance", shipData.VisionDistance),
                new QueryParameter("@destroyed", MySqlDbType.Int16, 16, "destroyed", shipData.Destroyed),
                new QueryParameter("@hidden", MySqlDbType.Int16, 16, "hidden", shipData.Hidden),
                new QueryParameter("@mob", MySqlDbType.Int16, 16, "mob", shipData.Mob),
                new QueryParameter("@warpDriveStartTime", MySqlDbType.Float, 32, "warpDriveStartTime", shipData.WarpDriveStartTime),
                new QueryParameter("@warpSpeed", MySqlDbType.Float, 32, "warpSpeed", shipData.WarpSpeed));
            foreach (WeaponData weapon in shipData.Weapons)
            {
                AddNewWeapon(newShip_id, weapon);
            }
            foreach (EquipmentData eq in shipData.Equipments)
            {
                //AddNewEquipment(newShip_id, eq);
            }

            callback(newShip_id);
        }

        public void GetShip(int ship_id ,Action<ShipData> callback)
        {
        
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
                        ON SO_shipdata.SO_id=server_objects.id 
                        WHERE id=@id", 
                        new QueryParameter("@id",MySqlDbType.Int32, 32,"id",ship_id));

         ShipData retShipData = new ShipData();   
        foreach (var row in rows)
            {
               
                var data = row.GetRow();
                retShipData.Id = (int)data["id"];
                retShipData.Type = (TypeSO)(int)data["type"];
                retShipData.VisibleName = (string)data["visibleName"];
                retShipData.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
                retShipData.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
                retShipData.Speed = (float)data["speed"];
                retShipData.Prefab = (string)data["prefab_path"];
                retShipData.SpeedMax = (float)data["max_speed"];
                retShipData.RotationSpeed = (float)data["rotation_speed"];
                retShipData.AccelerationMax = (float)data["acceleration_max"];
                retShipData.SpeedNew = (float)data["newSpeed"];
                retShipData.Hull_full = (float)data["hull_full"];
                retShipData.Armor_full = (float)data["armor_full"];
                retShipData.Shield_full = (float)data["shield_full"];
                retShipData.Capasitor_full = (float)data["capasitor_full"];
                retShipData.Hull = (float)data["hull"];
                retShipData.Armor = (float)data["armor"];
                retShipData.Shield = (float)data["shield"];
                retShipData.Capasitor = (float)data["capasitor"];
                retShipData.Hull_restore = (float)data["hull_restore"];
                retShipData.Armor_restore = (float)data["armor_restore"];
                retShipData.Shield_restore = (float)data["shield_restore"];
                retShipData.Capasitor_restore = (float)data["capasitor_restore"];
                retShipData.AgrDistance = (float)data["agr_distance"];
                retShipData.VisionDistance = (float)data["vision_distance"];
                retShipData.Destroyed = (bool)data["destroyed"];
                retShipData.Hidden = (bool)data["hidden"];
                retShipData.Mob = (bool)data["mob"];
                retShipData.WarpDriveStartTime = (float)data["warpDriveStartTime"];
                retShipData.WarpSpeed = (float)data["warpSpeed"];
            }
            retShipData.Weapons = GetWeapons(ship_id);
            retShipData.Equipments = GetEquip(ship_id);

            callback(retShipData);

        }

        public void SetShip(ShipData shipData, Action callback)
        {
            SetSpaceObject(shipData);
                      
            _database.ExecuteNonQuery(
                @"UPDATE SO_shipdata SET
                    max_speed =@max_speed,
                    rotation_speed =@rotation_speed,
                    acceleration_max =@acceleration_max,
                    newSpeed =@newSpeed,
                    hull_full =@hull_full,
                    armor_full =@armor_full,
                    shield_full =@shield_full,
                    capasitor_full =@capasitor_full,
                    hull =@hull,
                    armor =@armor,
                    shield =@shield,
                    capasitor =@capasitor,
                    hull_restore =@hull_restore,
                    armor_restore =@armor_restore,
                    shield_restore =@shield_restore,
                    capasitor_restore=@capasitor_restore,
                    agr_distance =@agr_distance,
                    vision_distance =@vision_distance,
                    destroyed =@destroyed,
                    hidden =@hidden,
                    mob =@mob,
                    warpDriveStartTime=@warpDriveStartTime,
                    warpSpeed =@warpSpeed
                WHERE SO_id=@id",


                new QueryParameter("@id", MySqlDbType.Int32, 32, "id", shipData.Id),
                new QueryParameter("@max_speed", MySqlDbType.Float, 32, "max_speed", shipData.SpeedMax),
                new QueryParameter("@rotation_speed", MySqlDbType.Float, 32, "rotation_speed", shipData.RotationSpeed),
                new QueryParameter("@acceleration_max", MySqlDbType.Float, 32, "acceleration_max", shipData.AccelerationMax),
                new QueryParameter("@newSpeed", MySqlDbType.Float, 32, "newSpeed", shipData.SpeedNew),
                new QueryParameter("@hull_full", MySqlDbType.Float, 32, "hull_full", shipData.Hull_full),
                new QueryParameter("@armor_full", MySqlDbType.Float, 32, "armor_full", shipData.Armor_full),
                new QueryParameter("@shield_full", MySqlDbType.Float, 32, "shield_full", shipData.Shield_full),
                new QueryParameter("@capasitor_full", MySqlDbType.Float, 32, "capasitor_full", shipData.Capasitor_full),
                new QueryParameter("@hull", MySqlDbType.Float, 32, "hull", shipData.Hull),
                new QueryParameter("@armor", MySqlDbType.Float, 32, "armor", shipData.Armor),
                new QueryParameter("@shield", MySqlDbType.Float, 32, "shield", shipData.Shield),
                new QueryParameter("@capasitor", MySqlDbType.Float, 32, "capasitor", shipData.Capasitor),
                new QueryParameter("@hull_restore", MySqlDbType.Float, 32, "hull_restore", shipData.Hull_restore),
                new QueryParameter("@armor_restore", MySqlDbType.Float, 32, "armor_restore", shipData.Armor_restore),
                new QueryParameter("@shield_restore", MySqlDbType.Float, 32, "shield_restore", shipData.Shield_restore),
                new QueryParameter("@capasitor_restore", MySqlDbType.Float, 32, "capasitor_restore", shipData.Capasitor_restore),
                new QueryParameter("@agr_distance", MySqlDbType.Float, 32, "agr_distance", shipData.AgrDistance),
                new QueryParameter("@vision_distance", MySqlDbType.Float, 32, "vision_distance", shipData.VisionDistance),
                new QueryParameter("@destroyed", MySqlDbType.Int16, 16, "destroyed", shipData.Destroyed),
                new QueryParameter("@hidden", MySqlDbType.Int16, 16, "hidden", shipData.Hidden),
                new QueryParameter("@mob", MySqlDbType.Int16, 16, "mob", shipData.Mob),
                new QueryParameter("@warpDriveStartTime", MySqlDbType.Float, 32, "warpDriveStartTime", shipData.WarpDriveStartTime),
                new QueryParameter("@warpSpeed", MySqlDbType.Float, 32, "warpSpeed", shipData.WarpSpeed));

            foreach (WeaponData weapon in shipData.Weapons)
            {
                SetWeapon(shipData.Id, weapon);
            }
            foreach (EquipmentData eq in shipData.Equipments)
            {
                //AddNewEquipment(newShip_id, eq);
            }

            callback();
        }
        public void DeleteShip(int shipId, Action callback)
        {
            _database.ExecuteNonQuery(
                @"DELETE a,b FROM SO_shipdata a
                  INNER JOIN server_objects b
                  ON a.SO_id=b.id 
                WHERE b.id = @Id",
                new QueryParameter("@Id", MySqlDbType.Int32, 11, "Id", shipId));
            callback();
        }

        public void GetAllMOBShips(Action<List<ShipData>> callback)
        {
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
                        ON SO_shipdata.SO_id=server_objects.id 
                        WHERE SO_shipdata.mob = 1 and id>=100");

            foreach (var row in rows)
            {
                ShipData retShipData = new ShipData();
                var data = row.GetRow();
                retShipData.Id = (int)data["id"];
                retShipData.Type = (TypeSO)(int)data["type"];
                retShipData.VisibleName = (string)data["visibleName"];
                retShipData.Position = new Vector3((float)data["position_x"], (float)data["position_y"], (float)data["position_z"]);
                retShipData.Rotation = new MyQuaternion((float)data["rotation_x"], (float)data["rotation_y"], (float)data["rotation_z"], (float)data["rotation_w"]);
                retShipData.Speed = (float)data["speed"];
                retShipData.Prefab = (string)data["prefab_path"];
                retShipData.SpeedMax = (float)data["max_speed"];
                retShipData.RotationSpeed = (float)data["rotation_speed"];
                retShipData.AccelerationMax = (float)data["acceleration_max"];
                retShipData.SpeedNew = (float)data["newSpeed"];
                retShipData.Hull_full = (float)data["hull_full"];
                retShipData.Armor_full = (float)data["armor_full"];
                retShipData.Shield_full = (float)data["shield_full"];
                retShipData.Capasitor_full = (float)data["capasitor_full"];
                retShipData.Hull = (float)data["hull"];
                retShipData.Armor = (float)data["armor"];
                retShipData.Shield = (float)data["shield"];
                retShipData.Capasitor = (float)data["capasitor"];
                retShipData.Hull_restore = (float)data["hull_restore"];
                retShipData.Armor_restore = (float)data["armor_restore"];
                retShipData.Shield_restore = (float)data["shield_restore"];
                retShipData.Capasitor_restore = (float)data["capasitor_restore"];
                retShipData.AgrDistance = (float)data["agr_distance"];
                retShipData.VisionDistance = (float)data["vision_distance"];
                retShipData.Destroyed = (bool)data["destroyed"];
                retShipData.Hidden = (bool)data["hidden"];
                retShipData.Mob = (bool)data["mob"];
                retShipData.WarpDriveStartTime = (float)data["warpDriveStartTime"];
                retShipData.WarpSpeed = (float)data["warpSpeed"];
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

        private void AddNewWeapon(int ship_id, WeaponData weaponData)
        {
            _database.ExecuteNonQuery(
                    @"INSERT INTO SO_weapondata (
                        SO_id,
                        point,
                        WeaponType,
                        active,
                        damage,
                        reload ,
                        ammoSpeed ,
                        activeTime ,
                        sqrDistanse_max ,
                        capasitor_use) 
                    VALUES(
                        @SO_id,
                        @point,
                        @WeaponType,
                        @active,
                        @damage,
                        @reload ,
                        @ammoSpeed ,
                        @activeTime ,
                        @sqrDistanse_max ,
                        @capasitor_use)",
                    new QueryParameter("@SO_id", MySqlDbType.Int32, 32, "SO_id", ship_id),
                    new QueryParameter("@point", MySqlDbType.Int32, 32, "point", weaponData.Point),
                    new QueryParameter("@WeaponType", MySqlDbType.Int32, 32, "WeaponType", (int)weaponData.Type),
                    new QueryParameter("@active"         , MySqlDbType.Float, 32, "active"           ,0),
                    new QueryParameter("@damage"         , MySqlDbType.Float, 32, "damage"           ,weaponData.Damage),
                    new QueryParameter("@reload"         , MySqlDbType.Float, 32, "reload"           ,weaponData.Reload),
                    new QueryParameter("@ammoSpeed"      , MySqlDbType.Float, 32, "ammoSpeed"        ,weaponData.AmmoSpeed),
                    new QueryParameter("@activeTime"     , MySqlDbType.Float, 32, "activeTime"       ,weaponData.ActiveTime),
                    new QueryParameter("@sqrDistanse_max", MySqlDbType.Float, 32, "sqrDistanse_max"  ,weaponData.SqrDistanse_max),
                    new QueryParameter("@capasitor_use"  , MySqlDbType.Float, 32, "capasitor_use"    ,weaponData.Capasitor_use));

        }
        private void SetWeapon(int ship_id, WeaponData weaponData)
        {
            _database.ExecuteNonQuery(
                @"UPDATE SO_weapondata SET

                        SO_id            =@SO_id,
                        WeaponType       =@WeaponType,
                        active           =@active,
                        damage           =@damage,
                        reload           =@reload ,
                        ammoSpeed        =@ammoSpeed ,
                        activeTime       =@activeTime ,
                        sqrDistanse_max  =@sqrDistanse_max,
                        capasitor_use    =@capasitor_use
                        WHERE SO_id=@SO_id and point=@point",
                    new QueryParameter("@SO_id", MySqlDbType.Int32, 32, "SO_id", ship_id),
                    new QueryParameter("@point", MySqlDbType.Int32, 32, "point", weaponData.Point),
                    new QueryParameter("@WeaponType", MySqlDbType.Int32, 32, "WeaponType", (int)weaponData.Type),
                    new QueryParameter("@active", MySqlDbType.Float, 32, "active", 0),
                    new QueryParameter("@damage", MySqlDbType.Float, 32, "damage", weaponData.Damage),
                    new QueryParameter("@reload", MySqlDbType.Float, 32, "reload", weaponData.Reload),
                    new QueryParameter("@ammoSpeed", MySqlDbType.Float, 32, "ammoSpeed", weaponData.AmmoSpeed),
                    new QueryParameter("@activeTime", MySqlDbType.Float, 32, "activeTime", weaponData.ActiveTime),
                    new QueryParameter("@sqrDistanse_max", MySqlDbType.Float, 32, "sqrDistanse_max", weaponData.SqrDistanse_max),
                    new QueryParameter("@capasitor_use", MySqlDbType.Float, 32, "capasitor_use", weaponData.Capasitor_use));

        }

        private WeaponData[] GetWeapons(int ship_id)
        {
            List<WeaponData> retList = new List<WeaponData>();

            var rows = _database.ExecuteQuery(@"SELECT 
					WeaponType, 
                    point,
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
                weapon.Point = (int)data["point"];
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
       
        public void GetPlayerActiveShip(string player, Action<int> callback)
        {
            var row = _database.ExecuteScalar(
                            "SELECT active_ship_id FROM players WHERE player = @player",
                            new QueryParameter("@player", MySqlDbType.VarChar, 60, "player", _database.EscapeString(player)));
            callback((int)row);
        }

        public void AddNewPlayerAndShip(string player, Action callback)
        {
            GetShip(0, newShip =>
                {
                    AddNewShip(newShip, ship_id =>
                        {
                            AddNewPlayer(player, ship_id);
                        });
                });
            callback();
        }

        private void AddNewPlayer(string player,int ship_id)
        {
            _database.ExecuteNonQuery(
                @"INSERT INTO players (
                            player,
                            active_ship_id)
                            VALUES(
                            @player,
                            @active_ship_id)",
                            new QueryParameter("@player", MySqlDbType.VarChar, 200, "player",player ),
                            new QueryParameter("@active_ship_id", MySqlDbType.Int32, 11, "active_ship_id",ship_id ));
        }



        public void GetAllItems(Action<List<Item>> callback)
        {

            List<Item> retListItems = new List<Item>();
            var rows = _database.ExecuteQuery(@"SELECT
                            id,
                            item,
                            type_id,
                            base_value,
                            sprite  
                        FROM items");

            foreach (var row in rows)
            {
                Item retItem = new Item();
                var data = row.GetRow();

                retItem.Id         = (int)data["id"];
                retItem.ItemName   = (string)data["item"];
                retItem.ItemType = (ItemTypes)data["type_id"];
                retItem.Volume    = (float)data["base_value"];
                retItem.SpritePath = (string)data["sprite"]; 
                retListItems.Add(retItem);
            }
            callback(retListItems);
        }
    

        public void  GetPlayerInventory(int playerId, int holderId, Action<List< InventoryItem>> callback)
        {
            List<InventoryItem> retList = new List<InventoryItem>();
            var rows = _database.ExecuteQuery(
                        @"SELECT
                            inventory_holder_id,
                            item_id,
                            tech,
                            quantity 
                        FROM inventory 
                        WHERE player_id = @playerId and 
                                inventory_holder_id = @holderId ",
                        new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId",playerId ),
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId",holderId ));

            foreach (var row in rows)
            {
                InventoryItem retInventoryItem = new InventoryItem();
                var data = row.GetRow();
                
                retInventoryItem.ItemId   = (int)data["item_id"];
                retInventoryItem.Tech     = (int)data["tech"];
                retInventoryItem.Quantity = (int)data["quantity"];
                
                retList.Add(retInventoryItem);
            }
            callback(retList);
        }


        public void GetPlayerId(string player, Action<int> callback)
        {
            Console.WriteLine("DB {0}   {1}", _database , player);

            var row = _database.ExecuteScalar(
                "SELECT id FROM players WHERE player = @player",
                new QueryParameter("@player", MySqlDbType.VarChar, 60, "player", _database.EscapeString(player)));

            Console.WriteLine("row {0}", row);

            callback((int)row);
        }

        public void GetHolderInventory( int holderId, Action<List<InventoryItem>> callback)
        {
            List < InventoryItem > retList = new List<InventoryItem>();
            var rows = _database.ExecuteQuery(
                        @"SELECT 
                            inventory_holder_id,
                            item_id,
                            tech,
                            quantity 
                        FROM inventory 
                        WHERE inventory_holder_id = @holderId ",
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId", holderId));

            Console.WriteLine("rows selected in holder inventory {0}",rows!=null);
            foreach (var row in rows)
            {
                InventoryItem retInventoryItem = new InventoryItem();
                var data = row.GetRow();

                retInventoryItem.ItemId = (int)data["item_id"];
                retInventoryItem.Tech = (int)data["tech"];
                retInventoryItem.Quantity = (int)data["quantity"];

                retList.Add(retInventoryItem);
            }
            callback(retList);
        }

        public void InventoryItemMove(int senderId, int senderHolder, int receiverId, int receiverHolder, int itemId, int quantity, Action<bool> callback)
        {
            bool sucsess=false;
            //try
            //{
                InventoryItemSubtract ( senderId, senderHolder, itemId, quantity, realQuantity  =>
                {
                    if (realQuantity>0) 
                    {
                        InventoryItemAdd( receiverId, receiverHolder, itemId, quantity, addedQuantity =>
                            {
                                Console.WriteLine("moved {0} items id:{1} from player {2} to {3} from holder {4} to {5}",addedQuantity,itemId,senderId,senderHolder,receiverId,receiverHolder);
                            });
                        sucsess = true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to move {0} items id:{1} from player {2} to {3} from holder {4} to {5}",quantity,itemId,senderId,senderHolder,receiverId,receiverHolder);
                    }
                });
            //}
            //catch (Exception ex)
            //{
            //    //TODO exeption description
            //    //_database.WriteToLog("Database error on moving inventory items" + ex, DarkRift.LogType.Error);

            //    //Return Error 2 for Database error
            //    //_database.DatabaseError(null, 0, ex);
            //}
            callback(sucsess);
        }

        private int InventoryItemQuantity(int playerId, int holderId, int itemId)
        {
            int retQuantity=0;
            var row = _database.ExecuteScalar(@"SELECT
                                                        quantity
                                                FROM    inventory
                                                WHERE   player_id = @playerId and 
                                                        inventory_holder_id = @holderId and 
                                                        item_id = @itemId ",
                        new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId",playerId ),
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId",holderId ),
                        new QueryParameter("@itemId", MySqlDbType.Int32, 11, "itemId",itemId ));
            
            if (row!=null)
                retQuantity=(int)row;
            return retQuantity;
        }



        public void InventoryItemAdd(int receiverId, int receiverHolder, int itemId, int quantity, Action<int> callback)
        {
            int receiverQuantity = InventoryItemQuantity(receiverId, receiverHolder, itemId);

            if (receiverQuantity != 0)
            {
                _database.ExecuteNonQuery(
                                    @"UPDATE inventory SET
                                            quantity = quantity + @quantity 
                                    WHERE   player_id = @playerId and 
                                            inventory_holder_id = @holderId and 
                                            item_id = @itemId ",
                        new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId", receiverId),
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId", receiverHolder),
                        new QueryParameter("@itemId", MySqlDbType.Int32, 11, "itemId", itemId),
                        new QueryParameter("@quantity", MySqlDbType.Int32, 11, "quantity", quantity));

            }
            else
            {
                _database.ExecuteNonQuery(
                             @"INSERT INTO inventory 
                                  (player_id, inventory_holder_id,item_id,quantity)
                               VALUES 
                                  (@playerId,  @holderId , @itemId, @quantity) ",
                        new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId", receiverId),
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId", receiverHolder),
                        new QueryParameter("@itemId", MySqlDbType.Int32, 11, "itemId", itemId),
                        new QueryParameter("@quantity", MySqlDbType.Int32, 11, "quantity", quantity));

            }
        }

        public void InventoryItemSubtract(int playerId, int holderId, int itemId, int quantity, Action<int> callback)
        {
            int playerQuantity = InventoryItemQuantity(playerId, holderId, itemId);
            
            if (playerQuantity > quantity)
            {
                _database.ExecuteNonQuery(
                                    @"UPDATE inventory SET
                                            quantity = quantity - @quantity 
                                    WHERE   player_id = @playerId and 
                                            inventory_holder_id = @holderId and 
                                            item_id = @itemId ",
                        new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId",playerId ),
                        new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId",holderId ),
                        new QueryParameter("@itemId", MySqlDbType.Int32, 11, "itemId",itemId ),
                        new QueryParameter("@quantity", MySqlDbType.Int32, 11, "quantity",quantity ));
                callback(quantity);
            }
            else
            {
                //delete
                if (playerQuantity > 0)
                {
                    _database.ExecuteNonQuery(
                                 @"DELETE FROM inventory 
                                    WHERE   player_id = @playerId and 
                                            inventory_holder_id = @holderId and 
                                            item_id = @itemId ",
                            new QueryParameter("@playerId", MySqlDbType.Int32, 11, "playerId", playerId),
                            new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId", holderId),
                            new QueryParameter("@itemId", MySqlDbType.Int32, 11, "itemId", itemId));
                }
                callback(playerQuantity);
            
            }

        }


        public void DeleteHolderInventory(int holderId, Action callback)
        {
            _database.ExecuteNonQuery(
                @"DELETE FROM inventory 
                WHERE inventory_holder_id = @holderId",
            new QueryParameter("@holderId", MySqlDbType.Int32, 11, "holderId", holderId));

            callback();
        }





        
    }
}