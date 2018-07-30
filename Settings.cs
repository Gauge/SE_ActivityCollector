using Torch;

namespace ActivityCollectorPlugin
{
    public class Settings
    {
        public const string Default_DB_Connection_String = @"Data Source=PATH_TO_DATABASE;Initial Catalog=SE_ActivityCollector;User ID=se;Password=se;MultipleActiveResultSets=True";

        public string DB_Connection_String { get; set; }
        public bool DebugMode { get; set; }

        public int ProductionInventoryInterval { get; set; }
        public int GeneralInventoryInterval { get; set; }

        public bool LogDefinitions { get; set; }
        public bool LogPlayers { get; set; }
        public bool LogFactions { get; set; }
        public bool LogChat { get; set; }
        public bool LogEntities { get; set; }
        public bool LogGrids { get; set; }
        public bool LogInventory { get; set; }
    }
}
