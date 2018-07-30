using System.Text;
using VRage;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum InvAction { Add, Remove, Current };
    public enum InvType { Input, Output };

    public class InventoryDescription : SQLQueryData
    {

        //public long BlockId { get; set; }
        //public bool IsItemAdded { get; set; }
        //public DateTime Timestamp { get; set; }

        private StringBuilder query = new StringBuilder();

        public InventoryDescription()
        {
            query.Append($@"INSERT INTO [entity_inventories] ([entity_id], [block_id], [item_id], [type], [action], [amount], [type_id], [subtype_id], [timestamp])
            VALUES");
        }

        public override string GetQuery()
        {
            return query.ToString().TrimEnd(',');
        }

        public void Clear()
        {
            query.Clear();
        }


        public void Add(long EntityId, long? BlockId, uint ItemId, InvType type, InvAction action, MyFixedPoint Amount, string TypeId, string SubtypeId)
        {
            query.Append($@"
('{EntityId}', '{((BlockId.HasValue) ? BlockId.Value.ToString() : "NULL")}', {ItemId}, '{type.ToString()}', '{action.ToString()}', {Amount}, '{TypeId}', '{SubtypeId}', '{Tools.DateTimeFormated}'),");
        }

    }
}
