using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class GridDescription : SQLQueryData
    {

        public long GridId { get; set; }
        public long ParentId { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }
        public DateTime SplitWithParent { get; set; }

        public override string GetQuery()
        {
            if (SplitWithParent != DateTime.MinValue)
            {
                return $@"UPDATE grids
SET [parent_id] = '{ParentId}', [split_with_parent] = '{Tools.format(SplitWithParent)}'
WHERE [id] = '{GridId}' AND [removed] IS NULL;";

            }
            else if (Removed == DateTime.MinValue)
            {
                return $@"IF NOT EXISTS (SELECT * FROM grids WHERE [id] = '{GridId}' AND [removed] IS NULL)
BEGIN
INSERT INTO grids ([id], [type], [created])
VALUES ('{GridId}', '{Type}', '{Tools.format(Created)}')
END;";
            }
            else
            {
                return $@"UPDATE grids
SET [removed] = '{Tools.format(Removed)}'
WHERE [id] = '{GridId}' AND [removed] IS NULL;";

            }
        }
    }
}
