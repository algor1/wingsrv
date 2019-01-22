using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DarkRift;
using DarkRift.Server;
using LoginPlugin;


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
        private const byte GameTag = 1;
        private const ushort Shift = GameTag * Login.TagsPerPlugin;

        // Subjects

        private const ushort InitPlayer = 0 + Shift;
        private const ushort SetTarget = 1 + Shift;
        private const ushort MoveToTarget = 2 + Shift;
        public const ushort NearestSpaceObjects = 3 + Shift;
        private const ushort MessageFailed = 4 + Shift;
        //TODO add tags ------------------------------------------

        private const string ConfigPath = @"Plugins/Game.xml";
        private static readonly object InitializeLock = new object();
        private bool _debug = true;
        private Login _loginPlugin;
        private ServerManager serverManager;
        

        public Game(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            LoadConfig();
            ClientManager.ClientConnected += OnPlayerConnected;
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
                        serverManager = new ServerManager();
                        //ChatGroups["General"] = new ChatGroup("General");
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
                            if (!serverManager.server.LoadPlayer(client)) _loginPlugin.PlayerLoggedIn(client, MessageFailed, "Init player on server failed");
                            break;
                        }
                }
            }
        }

        private void RemovePlayerFromServer(string username)
        {
            IClient cl = _loginPlugin.Clients[username];
            serverManager.server.RemovePlayer(cl);
        }

 
    }
}