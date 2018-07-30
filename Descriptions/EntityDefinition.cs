using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class EntityDescription : SQLQueryData
    {

        public long EntityId { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string SubtypeId { get; set; }
        public string ObjectType { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }

        public override string GetQuery()
        {

            if (Removed == DateTime.MinValue)
            {
                return $@"IF NOT EXISTS (SELECT * FROM entities WHERE [id] = '{EntityId}' AND [removed] IS NULL)
BEGIN
INSERT INTO entities ([id], [name], [object_type], [type_id], [subtype_id], [created])
VALUES ('{EntityId}', '{Name}', '{ObjectType}', '{TypeId}', '{SubtypeId}', '{Tools.format(Created)}')
END;";
            }
            else
            {
                return $@"UPDATE entities
SET [removed] = '{Tools.format(Removed)}'
WHERE [id] = '{EntityId}' AND [removed] IS NULL;";
            }
        }
    }
}
