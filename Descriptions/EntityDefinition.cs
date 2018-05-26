using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    public class EntityDescription : ISQLQueryData
    {

        public long EntityId { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string SubtypeId { get; set; }
        public string ObjectType { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }


        public string GetQuery()
        {
            if (Removed == DateTime.MinValue)
            {
                return $@"IF NOT EXISTS (SELECT * FROM entities WHERE [id] = '{EntityId}' AND [removed] IS NULL)
BEGIN
INSERT INTO entities ([id], [session_id], [name], [object_type], [type_id], [subtype_id], [created])
VALUES ('{EntityId}', '{ActivityCollectorPlugin.CurrentSession}', '{Name}', '{ObjectType}', '{TypeId}', '{SubtypeId}', '{Helper.format(Created)}')
END;";
            }
            else
            {
                return $@"UPDATE entities
SET [removed] = '{Helper.format(Removed)}'
WHERE [id] = '{EntityId}' AND [removed] IS NULL;";

            }
        }
    }
}
