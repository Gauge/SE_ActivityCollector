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
    public class ActivityCollectorPlugin : TorchPluginBase, ITorchPlugin
    {
        public const string ServerSteamId = "00000000000000000";
        public const bool DebugMode = false;

        public static readonly Logger log = LogManager.GetLogger("ActivityCollector");
        private static Queue<object> DataQueue = new Queue<object>();
        public static int CurrentSession { get; private set; }
        public static int CurrentIteration { get; private set; }

        private Persistent<ActivityCollectorConfig> _config;
        private event Action ServerStateChanged;
        private SqlConnection _sqldatabase;
        private TorchServer _torchServer = null;
        private ServerState _currentServerState;
        private EntityManager manager;
        private ITorchBase torch;

        public override void Init(ITorchBase torchBase)
        {
            base.Init(torchBase);
            torch = torchBase;

            ServerStateChanged += OnServerStateChanged;

            _torchServer = (TorchServer)Torch;
            _currentServerState = _torchServer.State;
            manager = new EntityManager();

            string configPath = Path.Combine(StoragePath, "ActivityCollector.cfg");
            log.Info($"Config location: {configPath}");
            if (!File.Exists(configPath))
            {
                _config = Persistent<ActivityCollectorConfig>.Load(configPath);
                _config.Data.DB_Connection_String = ActivityCollectorConfig.Default_DB_Connection_String;
                _config.Data.DebugMode = false;
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
            manager.Run();
        }

        /// <summary>
        /// Initializes data collection liseners
        /// ch
        /// </summary>
        private void OnServerStateChanged()
        {
            _currentServerState = _torchServer.State;
            DataQueue.Enqueue(_currentServerState);
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

                    if (DataQueue.Count > 0)
                    {
                        Analytics.Start("WriteToDatabase");

                        object element;
                        int count = DataQueue.Count;
                        for (int i = 0; i<count; i++)
                        {
                            StringBuilder command = new StringBuilder();

                            lock (DataQueue)
                            {
                                element = DataQueue.Dequeue();
                            }

                            if (element is ServerState)
                            {
                                if (((ServerState)element) != ServerState.Running)
                                {
                                    // run cleanup
                                    command.Append(string.Format(@"
                                    UPDATE [dbo].[user_activity]
                                    SET [state] = 'Unresolved'
                                    WHERE [state] = 'Active';

                                    UPDATE [dbo].[user_piloting]
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

        private static WaitCallback EnqueueCallback = new WaitCallback(PoolFunc);

        private static void PoolFunc(object data)
        {
            lock(DataQueue)
            {
                DataQueue.Enqueue(data);
            }
        }

        public static void Enqueue(object data)
        {
            ThreadPool.QueueUserWorkItem(EnqueueCallback, data);
        }
    }
}
