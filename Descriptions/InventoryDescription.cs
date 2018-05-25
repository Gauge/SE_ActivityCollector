using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Entity;

namespace ActivityCollectorPlugin.Descriptions
{
    class InventoryDescription : ISQLQueryData
    {

        public long BlockId { get; set; }
        public bool IsItemAdded { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            throw new NotImplementedException();
        }

        public async static void Queue(MyPhysicalInventoryItem[] old, MyPhysicalInventoryItem[] current, long BlockId, DateTime timestamp)
        {
            IEnumerable<MyPhysicalInventoryItem> change = current.Except(old);

            foreach (MyPhysicalInventoryItem item in change)
            {
                ActivityCollectorPlugin.log.Info($"[{item.ItemId}] Name: {item.Content.TypeId}/{item.Content.SubtypeId} | Quanity: {item.Amount}");
            }
        }
    }
}
