﻿namespace ActivityCollectorPlugin
{
    public class ActivityCollectorConfig
    {
        public const string Default_DB_Connection_String = @"Data Source=PATH_TO_DATABASE;Initial Catalog=SE_ActivityCollector;User ID=se;Password=se";

        public string DB_Connection_String { get; set; }
    }
}