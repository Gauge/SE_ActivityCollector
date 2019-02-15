using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class BlockDescription : SQLQueryData
    {
        public long BlockEntityId { get; set; }
        public long GridId { get; set; }
        public long BuiltBy { get; set; }
        public string TypeId { get; set; }
        public string SubTypeId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }

        public override string GetQuery()
        {

            if (Removed != DateTime.MinValue)
            {
                return $@"UPDATE grid_blocks
SET [removed] = '{Tools.format(Removed)}'
WHERE [grid_id] = '{GridId}' AND [x] = '{X}' AND [y] = '{Y}' AND [z] = '{Z}' AND [removed] IS NULL";
            }
            else
            {
                return $@"IF NOT EXISTS (SELECT [grid_id] FROM grid_blocks WHERE [grid_id] = '{GridId}' AND [x] = '{X}' AND [y] = '{Y}' AND [z] = '{Z}' AND [removed] IS NULL)
BEGIN
INSERT INTO grid_blocks ([entity_id], [grid_id], [built_by], [type_id], [subtype_id], [x], [y], [z], [created])
VALUES ('{((BlockEntityId == 0) ? "null" : BlockEntityId.ToString())}', '{GridId}', '{BuiltBy}', '{TypeId}', '{SubTypeId}', '{X}', '{Y}', '{Z}', '{Tools.format(Created)}')
END;";
            }
        }
    }
}
