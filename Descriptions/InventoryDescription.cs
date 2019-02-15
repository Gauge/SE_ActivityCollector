using System.Text;
using VRage;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum InvAction { Add, Remove, Current };
    public enum InvType { Input, Output };

    public class InventoryDescription : SQLQueryData
    {
        private StringBuilder query = new StringBuilder();
        private bool hasData = false;

        public InventoryDescription()
        {
            query.Append($@"INSERT INTO [entities_inventory] ([entity_id], [block_id], [item_id], [type], [action], [amount], [type_id], [subtype_id], [timestamp])
            VALUES ");
        }

        public override string GetQuery()
        {
            if (hasData)
                return query.ToString().TrimEnd(',') + ";";

            return string.Empty;
        }

        public void Clear()
        {
            query.Clear();
        }


        public void Add(long EntityId, long? BlockId, uint ItemId, InvType type, InvAction action, MyFixedPoint Amount, string TypeId, string SubtypeId)
        {
            hasData = true;
            query.Append($@"('{EntityId}', '{((BlockId.HasValue) ? BlockId.Value.ToString() : "NULL")}', {ItemId}, '{type.ToString()}', '{action.ToString()}', {Amount}, '{TypeId}', '{SubtypeId}', '{Tools.DateTimeFormated}'),");
        }

    }
}
