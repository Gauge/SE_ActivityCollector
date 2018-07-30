using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    class BlockOwnershipDescription : SQLQueryData
    {
        public long GridId { get; set; }
        public long BlockEntityId { get; set; }
        public long Owner { get; set; }
        public DateTime Timestamp { get; set; }

        public override string GetQuery()
        {
            return $@"INSERT INTO grid_block_ownership ([block_id], [grid_id], [owner], [timestamp])
VALUES ('{BlockEntityId}', '{GridId}', '{Owner}', '{Tools.format(Timestamp)}');";
        }
    }
}
