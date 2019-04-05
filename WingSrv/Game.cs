using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DarkRift;
using DarkRift.Server;
using LoginPlugin;
using System.Threading;
using SpaceObjects;
using Database;

namespace Wingsrv
{
    public class Game : Plugin
    {
        public override Version Version => new Version(1, 0, 1);
        public override bool ThreadSafe => true;
        //public override Command[] Commands => new []
        //{
        //    new Command("groups", "Shows all chatgroups [groups username(optional]", "", GetChatGroupsCommand)
        //};

        // Tag
        private const byte GameTag = 4;
        private const ushort Shift = GameTag * Login.TagsPerPlugin;

        // Subjects

        public const ushort InitPlayer =(ushort)( 0 + Shift);
        public const ushort PlayerShipCommand = (ushort)(1 + Shift);
        public const ushort MoveToTarget = (ushort)(2 + Shift);
        public const ushort NearestShips = (ushort)(3 + Shift);
        public const ushort MessageFailed = (ushort)(4 + Shift);
        public const ushort PlayerShipData = (ushort)(5 + Shift);
        public const ushort NearestSpaceObjects = (ushort)(6 + Shift);
        public const ushort ShipCommand = (ushort)(7 + Shift);


        private const string ConfigPath = @"Plugins/Game.xml";
        private static readonly object InitializeLock = new object();
        private bool _debug = true;
        private Login _loginPlugin;
        //private ServerManager serverManager;
        
        // Servers
        public  Server server;
        //public  ServerDB serverDB;
        public  ServerSO serverSO;
        public  InventoryServer inventoryServer;
        //public  ItemDB itemDB;

        //private ConcurrentDictionary<string, int> PlayerOnShips;




        public Game(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            LoadConfig();
            RunServers();
            ClientManager.ClientConnected += OnPlayerConnected;

        }

        

        public  void RunServers ()
        {
            //itemDB = new ItemDB(this);
            inventoryServer = new InventoryServer(this);
            //serverDB = new ServerDB(this);
            server = new Server(this);
            serverSO = new ServerSO(this);

            //Thread myThread = new Thread(new ThreadStart(server.RunServer));
            //myThread.Start();
            //Console.WriteLine(myThread.IsBackground);


        }
        public void Stop()
        {
            //itemDB.OnApplicationQuit();
        }






        private void LoadConfig()
        {
            XDocument document;

            if (!File.Exists(ConfigPath))
            {
                document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("Settings for the Game Plugin"),
                    new XElement("Variables", new XAttribute("Debug", true))
                );
                try
                {
                    document.Save(ConfigPath);
                    WriteEvent("Created /Plugins/Game.xml!", LogType.Info);
                }
                catch (Exception ex)
                {
                    WriteEvent("Failed to create Game.xml: " + ex.Message + " - " + ex.StackTrace, LogType.Error);
                }
            }
            else
            {
                try
                {
                    document = XDocument.Load(ConfigPath);
                    _debug = document.Element("Variables").Attribute("Debug").Value == "true";
                }
                catch (Exception ex)
                {
                    WriteEvent("Failed to load Game.xml: " + ex.Message + " - " + ex.StackTrace, LogType.Error);
                }
            }
        }

        private void OnPlayerConnected(object sender, ClientConnectedEventArgs e)
        {
            // If you have DR2 Pro, use the Loaded() method instead and spare yourself the locks
            if (_loginPlugin == null)
            {
                lock (InitializeLock)
                {
                    if (_loginPlugin == null)
                    {
                        _loginPlugin = PluginManager.GetPluginByType<Login>();
                        server._loginPlugin = _loginPlugin;
                        serverSO._loginPlugin = _loginPlugin;
                        _loginPlugin.onLogout+= RemovePlayerFromServer;
                        _loginPlugin.onSignUp+= AddNewPlayer;
                    }
                }

            }
            if (server._database == null)
            {
                lock (InitializeLock)
                {
                    if (server._database == null ) server._database = PluginManager.GetPluginByType<DatabaseProxy>();
                }
            }
            if (serverSO._database == null)
            {
                lock (InitializeLock)
                {
                    if (serverSO._database == null ) serverSO._database = PluginManager.GetPluginByType<DatabaseProxy>();
                }
            }
            if (inventoryServer._database == null)
            {
                lock (InitializeLock)
                {
                    if (inventoryServer._database == null) inventoryServer._database = PluginManager.GetPluginByType<DatabaseProxy>();
                }
            }
            if (!server.started) server.RunServer();
            if (!serverSO.started) serverSO.RunServer();
            if (!inventoryServer.started) inventoryServer.RunServer();



            e.Client.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                // Check if message is meant for this plugin
                if (message.Tag < Login.TagsPerPlugin * GameTag || message.Tag >= Login.TagsPerPlugin * (GameTag + 1))
                {
                    return;
                }

                var client = e.Client;

                // Private Message
                switch (message.Tag)
                {
                    case InitPlayer:
                        {
                            // If player isn't logged in -> return error 1
                            if (!_loginPlugin.PlayerLoggedIn(client, MessageFailed, "Init player failed.")) return;

                            var senderName = _loginPlugin.UsersLoggedIn[client];
                            server.LoadPlayer(senderName);

                            break;
                        }
                    case PlayerShipCommand:
                        {
                            if (!_loginPlugin.PlayerLoggedIn(client, MessageFailed, "Init player failed.")) return;
                            ShipCommand command;
                            int target_id;
                            int point_id;
                            try
                            {
                                using (var reader = message.GetReader())
                                {
                                    command = (ShipCommand)reader.ReadUInt32();
                                    target_id = reader.ReadInt32();
                                    point_id = reader.ReadInt32();
                                }
                                Console.WriteLine("Command {0}  target {1}", command, target_id);
                            }
                            catch (Exception ex)
                            {
                                // Return Error 0 for Invalid Data Packages Recieved
                                _loginPlugin.InvalidData(client, MessageFailed, ex, "Send Message failed! ");
                                return;
                            }
                            var senderName = _loginPlugin.UsersLoggedIn[client];

                            server.GetPlayerShipCommand(senderName, command, target_id, point_id);
                            break;
                        }
                }
            }
        }

        private void RemovePlayerFromServer(string username)
        {
             server.RemovePlayer(username);
        }
        private void AddNewPlayer(string username)
        {
             server.AddNewPlayer(username);
        }
        public void WriteToLog(string _event, LogType typeInfo)
        {
            WriteEvent(_event, typeInfo);
        }


        

    }
}