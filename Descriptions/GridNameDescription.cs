using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    public class GridNameDescription : ISQLQueryData
    {
        public long GridId { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            return string.Format(@"
INSERT INTO grid_names ([grid_id], [iteration_id], [name], [timestamp])
VALUES ('{0}', '{1}', '{2}', '{3}');", GridId, ActivityCollectorPlugin.CurrentIteration, Name, Timestamp);
        }
    }
}
