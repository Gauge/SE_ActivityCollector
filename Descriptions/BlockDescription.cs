using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class BlockDescription : ISQLQueryData
    {
        public string BlockId { get { return $"{X}|{Y}|{Z}"; } }
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

        public string GetQuery()
        {

            if (Removed != DateTime.MinValue)
            {
                return $@"UPDATE grid_blocks
SET [removed] = '{Helper.format(Removed)}'
WHERE [id] = '{BlockId}' AND [grid_id] = '{GridId}' AND [removed] IS NULL";
            }
            else
            {
                return $@"IF NOT EXISTS (SELECT [id] FROM grid_blocks WHERE [id] = '{BlockId}' AND [grid_id] = '{GridId}' AND [removed] IS NULL)
BEGIN
INSERT INTO grid_blocks ([id], [entity_id], [grid_id], [session_id], [built_by], [type_id], [subtype_id], [x], [y], [z], [created])
VALUES ('{BlockId}', '{((BlockEntityId == 0) ? "null" : BlockEntityId.ToString())}', '{GridId}', {ActivityCollectorPlugin.CurrentSession}, '{BuiltBy}', '{TypeId}', '{SubTypeId}', '{X}', '{Y}', '{Z}', '{Helper.format(Created)}')
END;";
            }
        }
    }
}
