using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    class BlockOwnershipDescription : ISQLQueryData
    {
        public long GridId { get; set; }
        public long BlockEntityId { get; set; }
        public long Owner { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            return $@"INSERT INTO grid_block_ownership ([block_id], [grid_id], [session_id], [owner], [timestamp])
VALUES ('{BlockEntityId}', '{GridId}', {ActivityCollectorPlugin.CurrentSession}, '{Owner}', '{Helper.format(Timestamp)}');";
        }
    }
}
