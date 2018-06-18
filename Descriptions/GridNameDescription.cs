using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class GridNameDescription : ISQLQueryData
    {
        public long GridId { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            return $@"IF (SELECT TOP 1 [name] FROM grid_names WHERE [grid_id] = '{GridId}' ORDER BY [timestamp] DESC) IS NULL OR (SELECT TOP 1 [name] FROM grid_names WHERE [grid_id] = '{GridId}' ORDER BY [timestamp] DESC) <> '{Name}'
BEGIN
INSERT INTO grid_names ([grid_id], [session_id], [name], [timestamp])
VALUES ('{GridId}', '{ActivityCollectorPlugin.CurrentSession}', '{Helper.prepString(Name)}', '{Helper.format(Timestamp)}')
END;";
        }
    }
}
