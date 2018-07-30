using ActivityCollectorPlugin.Managers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Server;

namespace ActivityCollectorPlugin
{
    public class ActivityCollector : TorchPluginBase, ITorchPlugin
    {
        public const string ServerSteamId = "00000000000000000";

        public static readonly Logger Log = LogManager.GetLogger("ActivityCollector");
        public static Persistent<Settings> Config = null;

        private TorchServer _torchServer = null;
        private ServerState _currentServerState;
        private EntityManager manager;

        public override void Init(ITorchBase torchBase)
        {
            base.Init(torchBase);

            _torchServer = (TorchServer)Torch;
            _currentServerState = _torchServer.State;
            manager = new EntityManager();

            string configPath = Path.Combine(StoragePath, "ActivityCollector.cfg");
            Log.Info($"Config location: {configPath}");
            if (!File.Exists(configPath))
            {
                Config = Persistent<Settings>.Load(configPath);
                Config.Data.DB_Connection_String = Settings.Default_DB_Connection_String;
                Config.Data.ProductionInventoryInterval = 500;
                Config.Data.GeneralInventoryInterval = 5;
                Config.Data.DebugMode = false;
                Config.Data.LogChat = true;
                Config.Data.LogDefinitions = true;
                Config.Data.LogEntities = true;
                Config.Data.LogFactions = true;
                Config.Data.LogGrids = true;
                Config.Data.LogInventory = true;
                Config.Data.LogPlayers = true;
                Config.Save();
            }
            else
            {
                Config = Persistent<Settings>.Load(configPath);
            }
        }

        /// <summary>
        /// activates every game interval
        /// </summary>
        public override void Update()
        {
            manager.Run();
        }
    }
}
