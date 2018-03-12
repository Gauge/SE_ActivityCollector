using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return string.Format(@"
UPDATE grids
SET [parent_id] = '{0}', [split_with_parent] = '{1}'
WHERE [id] = '{2}';", ParentId, SplitWithParent, GridId);
            }
            else if (Removed == DateTime.MinValue)
            {
                return string.Format(@"
                SELECT * FROM grids 
                    WHERE id = '{0}' AND
                        iteration_id = '{1}';
                
                IF @@ROWCOUNT = 0
                    INSERT INTO grids ([id], [iteration_id], [type], [created])
                    VALUES ('{0}', '{1}', '{2}', '{3}');",
                    GridId, ActivityCollectorPlugin.CurrentIteration, Type, Created);

            }
            else
            {
                return string.Format(@"
                SELECT * FROM grids 
                    WHERE id = '{0}' AND
                        iteration_id = '{1}';
                
                IF @@ROWCOUNT <> 0
                    UPDATE grids
                    SET [removed] = '{2}'
                    WHERE [id] = '{0}' AND
                        iteration_id = '{1}';",
                    GridId, ActivityCollectorPlugin.CurrentIteration, Removed);
            }
        }
    }
}
