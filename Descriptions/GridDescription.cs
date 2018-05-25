using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class GridDescription : ISQLQueryData
    {

        public long GridId { get; set; }
        public long ParentId { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }
        public DateTime SplitWithParent { get; set; }

        public string GetQuery()
        {
            if (SplitWithParent != DateTime.MinValue)
            {
                return $@"UPDATE grids
SET [parent_id] = '{ParentId}', [split_with_parent] = '{Helper.format(SplitWithParent)}'
WHERE [id] = '{GridId}' AND [removed] IS NULL;";

            }
            else if (Removed == DateTime.MinValue)
            { //  AND [session_id] IN (SELECT [session_id] FROM sessions WHERE [iteration_id] = '{ActivityCollectorPlugin.CurrentIteration}')
                return $@"IF NOT EXISTS (SELECT * FROM grids WHERE [id] = '{GridId}' AND [removed] IS NULL)
BEGIN
INSERT INTO grids ([id], [session_id], [type], [created])
VALUES ('{GridId}', '{ActivityCollectorPlugin.CurrentSession}', '{Type}', '{Helper.format(Created)}')
END;";
            }
            else
            {
                return $@"UPDATE grids
SET [removed] = '{Helper.format(Removed)}'
WHERE [id] = '{GridId}' AND [removed] IS NULL;"; // AND [session_id] IN (SELECT [session_id] FROM sessions WHERE [iteration_id] = '{ActivityCollectorPlugin.CurrentIteration}')

            }
        }
    }
}
