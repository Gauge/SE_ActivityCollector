using ActivityCollectorPlugin.Managers;
using NLog;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.Server;

namespace ActivityCollectorPlugin
{
    public class ActivityCollectorPlugin : TorchPluginBase, ITorchPlugin
    {
        public const string ServerSteamId = "00000000000000000";
        public const bool DebugMode = true;

        public static readonly Logger log = LogManager.GetLogger("ActivityCollector");
        public static ConcurrentQueue<object> SessionLogQueue = new ConcurrentQueue<object>();
        public static int CurrentSession { get; private set; }
        public static int CurrentIteration { get; private set; }

        private Persistent<ActivityCollectorConfig> _config;
        private event Action ServerStateChanged;
        private SqlConnection _sqldatabase;
        private TorchServer _torchServer = null;
        private ServerState _currentServerState;
        private CombatManager _combatManager;
        private PlayerManager _playerManager;
        private FactionManager _factionManager;
        private GridManager _gridManager;
        private DefinitionManager _definitionManager;
        private ITorchBase torch;

        public override void Init(ITorchBase torchBase)
        {
            base.Init(torchBase);
            torch = torchBase;

            ServerStateChanged += OnServerStateChanged;

            _torchServer = (TorchServer)Torch;
            _currentServerState = _torchServer.State;
            _combatManager = new CombatManager();
            _factionManager = new FactionManager();
            _gridManager = new GridManager();
            _definitionManager = new DefinitionManager();

            string configPath = Path.Combine(StoragePath, "ActivityCollector.cfg");
            log.Info($"Config location: {configPath}");
            if (!File.Exists(configPath))
            {
                _config = Persistent<ActivityCollectorConfig>.Load(configPath);
                _config.Data.DB_Connection_String = ActivityCollectorConfig.Default_DB_Connection_String;
                _config.Save();
            }
            else
            {
                _config = Persistent<ActivityCollectorConfig>.Load(configPath);
            }

            _sqldatabase = new SqlConnection(_config.Data.DB_Connection_String);
            Task.Run(() => WriteToDatabase());
        }

        /// <summary>
        /// activates every game interval
        /// </summary>
        public override void Update()
        {
            if (MyAPIGateway.Session == null) return;

            _definitionManager.Run();
            _gridManager.Run();
            _combatManager.Run();
            //_playerManager.Run();
            //_factionManager.Run();
        }

        /// <summary>
        /// Sends ingame chat messages to be writen to the database
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sendToOthers"></param>
        public void OnMessageReceived(TorchChatMessage message, ref bool sendToOthers)
        {
            SessionLogQueue.Enqueue(message);
        }

        /// <summary>
        /// Initializes data collection liseners
        /// ch
        /// </summary>
        private void OnServerStateChanged()
        {
            if (_torchServer.State == ServerState.Running)
            {
                _playerManager = new PlayerManager(torch.CurrentSession.Managers.GetManager<IMultiplayerManagerServer>());

                IChatManagerClient chatManager = torch.CurrentSession.Managers.GetManager<IChatManagerClient>();
                chatManager.MessageRecieved += OnMessageReceived;
                log.Info($"Added chat hook");
            }

            _currentServerState = _torchServer.State;
            SessionLogQueue.Enqueue(_currentServerState);
        }

        private void WriteToDatabase()
        {
            log.Info("Starting database logger");
            bool updateSessionNumber = false;
            SqlCommand sqlCommand = new SqlCommand("", _sqldatabase);

            try
            {
                _sqldatabase.Open();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return;
            }

            while (true)
            {
                try
                {
                    if (_torchServer != null && _torchServer.State != _currentServerState)
                    {
                        ServerStateChanged?.Invoke();
                    }

                    if (SessionLogQueue.Count > 0)
                    {
                        Analytics.Start("WriteToDatabase");


                        while (SessionLogQueue.Count > 0 /*&& command.Length < 500000*/)
                        {
                            StringBuilder command = new StringBuilder();
                            object element = null;
                            SessionLogQueue.TryDequeue(out element);

                            if (element is TorchChatMessage)
                            {
                                TorchChatMessage message = (TorchChatMessage)element;
                                command.Append(string.Format(@"EXEC [dbo].add_chatlog '{0}', '{1}', '{2}';", ((message.AuthorSteamId == null) ? ServerSteamId : message.AuthorSteamId.ToString()), message.Message?.Replace("'", "''"), Helper.format(message.Timestamp)));
                            }
                            else if (element is ServerState)
                            {
                                if (((ServerState)element) != ServerState.Running)
                                {
                                    // run cleanup
                                    command.Append(string.Format(@"
                                    UPDATE [dbo].[activity]
                                    SET [state] = 'Unresolved'
                                    WHERE [state] = 'Active';

                                    UPDATE [dbo].[piloting]
                                    SET [end_time] = '{0}', [is_piloting] = 0
                                    WHERE [is_piloting] = 1;", Helper.DateTimeFormated));
                                }

                                command.Append(string.Format(@"
                                INSERT INTO [dbo].[sessions] ([iteration_id], [status], [timestamp])
	                                Values ((SELECT TOP 1 id FROM iterations order by startdate desc), '{0}', '{1}');", ((ServerState)element).ToString(), Helper.DateTimeFormated));
                                updateSessionNumber = true;
                            }
                            else if (element is ISQLQueryData)
                            {
                                command.Append((element as ISQLQueryData).GetQuery());
                            }

                            sqlCommand.CommandText = command.ToString();
                            sqlCommand.ExecuteNonQuery();
                        }

                        //ActivityCollectorPlugin.log.Info($"Writing string {command.Length}");
                        //new SqlCommand(command.ToString(), _sqldatabase).BeginExecuteNonQuery();

                        if (updateSessionNumber)
                        {
                            SqlCommand sessionator = new SqlCommand("SELECT TOP 1 [id], [iteration_id] FROM sessions ORDER BY [id] DESC;", _sqldatabase);

                            SqlDataReader reader = sessionator.ExecuteReader();
                            reader.Read();
                            CurrentSession = reader.GetInt32(0);
                            CurrentIteration = reader.GetInt32(1);
                            reader.Close();
                            sessionator.Dispose();

                            updateSessionNumber = false;
                        }

                        Analytics.Stop("WriteToDatabase");
                    }

                    Thread.Sleep(3000);
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                    log.Error($"Current Query: {sqlCommand.CommandText}");
                }
            }
        }
    }
}
