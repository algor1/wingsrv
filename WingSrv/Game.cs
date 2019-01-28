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



namespace Wingsrv
{
    public class Game : Plugin
    {
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;
        //public override Command[] Commands => new []
        //{
        //    new Command("groups", "Shows all chatgroups [groups username(optional]", "", GetChatGroupsCommand)
        //};

        // Tag
        private const byte GameTag = 4;
        private const ushort Shift = GameTag * Login.TagsPerPlugin;

        // Subjects

        private const ushort InitPlayer =(ushort)( 0 + Shift);
        private const ushort SetTarget = (ushort)(1 + Shift);
        private const ushort MoveToTarget = (ushort)(2 + Shift);
        public const ushort NearestSpaceObjects = (ushort)(3 + Shift);
        private const ushort MessageFailed = (ushort)(4 + Shift);
        //TODO add tags ------------------------------------------

        private const string ConfigPath = @"Plugins/Game.xml";
        private static readonly object InitializeLock = new object();
        private bool _debug = true;
        public Login _loginPlugin;
        //private ServerManager serverManager;
        
        // Servers
        public  Server server;
        public  ServerDB serverDB;
        public  InventoryServer inventoryServer;
        public  ItemDB itemDB;



        public Game(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            LoadConfig();
            RunServers();
            ClientManager.ClientConnected += OnPlayerConnected;
        }

        

        public  void RunServers ()
        {
            itemDB = new ItemDB(this);
            inventoryServer = new InventoryServer(this);
            serverDB = new ServerDB(this);
            server = new Server(this);
            server.RunServer();
            //Thread myThread = new Thread(new ThreadStart(server.RunServer));
            //myThread.Start();


        }
        public void Stop()
        {
            itemDB.OnApplicationQuit();
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
                        _loginPlugin.onLogout+= RemovePlayerFromServer;
                    }
                }
            }

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
                }
            }
        }

        private void RemovePlayerFromServer(string username)
        {
            IClient cl = _loginPlugin.Clients[username];
            server.RemovePlayer(username);
        }
        public void WriteToLog(string _event, LogType typeInfo)
        {
            WriteEvent(_event, typeInfo);
        }


        

    }
}