﻿using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class GridNameDescription : SQLQueryData
    {
        public long GridId { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public override string GetQuery()
        {
            return $@"IF (SELECT TOP 1 [name] FROM grid_names WHERE [grid_id] = '{GridId}' ORDER BY [timestamp] DESC) IS NULL OR (SELECT TOP 1 [name] FROM grid_names WHERE [grid_id] = '{GridId}' ORDER BY [timestamp] DESC) <> '{Name}'
BEGIN
INSERT INTO grid_names ([grid_id], [name], [timestamp])
VALUES ('{GridId}', '{Tools.PrepString(Name)}', '{Tools.format(Timestamp)}')
END;";
        }
    }
}
