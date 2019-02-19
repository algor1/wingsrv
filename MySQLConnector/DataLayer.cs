using System;
using System.Collections.Generic;
using Database;
using MySql.Data.MySqlClient;
using SpaceObjects;

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
                new QueryParameter("@id", MySqlDbType.Int,32 , "id", _id));

            SpaceObject returnSO = new SpaceObject();
            int _type = 0;
            returnSO.Id = rows[0]["id"];
            returnSO.Type = (TypeSO) rows[0]["type"];
            returnSO.VisibleName = rows[0]["visibleName"];
            returnSO.Position = new Vector3(rows[0]["position_x"],rows[0]["position_y"],rows[0]["position_z"]);
            returnSO.Rotation = new MyQuaternion(rows[0]["rotation_x"],rows[0]["rotation_y"],rows[0]["rotation_z"],rows[0]["rotation_w"]);
            returnSO.Speed = rows[0]["speed"];
            returnSO.Prefab = rows[0]["prefab_path"];
            callback(returnSO);
        }
    }
}