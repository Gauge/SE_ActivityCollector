using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class BlockDescription : ISQLQueryData
    {
        public long BlockId { get; set; }
        public long GridId { get; set; }
        public long BuiltBy { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public float MaxIntegrity { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }

        public string GetQuery()
        {
            return string.Format(@"
    INSERT INTO blocks ([id], [grid_id], [iteration_id], [built_by], [name], [type], [max_integrity], [x], [y], [z])
    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}');", 
    BlockId, GridId, ActivityCollectorPlugin.CurrentIteration, BuiltBy, Name, Type, MaxIntegrity, X, Y, Z);
        }
    }
}
