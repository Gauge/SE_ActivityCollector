using System.Text;
using VRage;


namespace ActivityCollectorPlugin.Descriptions
{
    public class InventoryDescription : ISQLQueryData
    {

        //public long BlockId { get; set; }
        //public bool IsItemAdded { get; set; }
        //public DateTime Timestamp { get; set; }

        private StringBuilder query = new StringBuilder();

        public InventoryDescription()
        {
            query.Append($@"INSERT INTO [entity_inventories] ([entity_id], [block_id], [item_id], [amount], [durability], [type_id], [subtype_id], [session_id], [timestamp])
            VALUES");
        }

        public string GetQuery()
        {
            return query.ToString().TrimEnd(',');
        }


        public void Add(long EntityId, long? BlockId, uint ItemId, MyFixedPoint Amount, float? durability, string TypeId, string SubtypeId)
        {
            query.Append($@"
('{EntityId}', '{((BlockId.HasValue) ? BlockId.Value.ToString() : "NULL")}', {ItemId}, {Amount}, {((durability.HasValue) ? durability.Value.ToString() : "NULL")}, '{TypeId}', '{SubtypeId}', {ActivityCollectorPlugin.CurrentSession}, '{Helper.DateTimeFormated}'),");
        }

    }
}
