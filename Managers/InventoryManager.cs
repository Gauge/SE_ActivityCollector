using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Entity;

namespace ActivityCollectorPlugin.Managers
{
    public class InventoryManager : IManager
    {
        public bool IsInitialized { get; set; } = false;

        private Dictionary<long, MyPhysicalInventoryItem[]> LastInventory = new Dictionary<long, MyPhysicalInventoryItem[]>();

        public void OnInventoryChanged(MyInventoryBase inventory)
        {
            //Analytics.Start("InventoryChanged");

            long inventoryId = inventory.Entity.EntityId;
            long entityId = inventoryId;
            long? blockId = null;

            if (inventory.Entity is MyCubeBlock)
            {
                blockId = inventoryId;
                entityId = (inventory.Entity as MyCubeBlock).CubeGrid.EntityId;
            }

            if (!LastInventory.ContainsKey(inventoryId))
            {
                LastInventory.Add(inventoryId, new MyPhysicalInventoryItem[0]);
            }

            MyPhysicalInventoryItem[] newInventory = inventory.GetItems().ToArray();
            MyPhysicalInventoryItem[] oldInventory = LastInventory[inventoryId];

            IEnumerable<MyPhysicalInventoryItem> stuffAdded = newInventory.Except(oldInventory);
            IEnumerable<MyPhysicalInventoryItem> stuffRemoved = oldInventory.Except(newInventory);

            InventoryDescription description = new InventoryDescription();

            foreach (MyPhysicalInventoryItem item in stuffAdded)
            {
                description.Add(entityId, blockId, item.ItemId, item.Amount, item.Content.DurabilityHP, item.Content.TypeId.ToString(), item.Content.SubtypeId.ToString());
            }

            foreach (MyPhysicalInventoryItem item in stuffRemoved)
            {
                description.Add(entityId, blockId, item.ItemId, item.Amount, item.Content.DurabilityHP, item.Content.TypeId.ToString(), item.Content.SubtypeId.ToString());
            }

            LastInventory[inventoryId] = newInventory;

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(description);
            //Analytics.Stop("InventoryChanged");
        }

        public void OnBeforeInventoryChanged(MyInventoryBase inventory)
        {
            if (!LastInventory.ContainsKey(inventory.Entity.EntityId))
            {
                LastInventory.Add(inventory.Entity.EntityId, inventory.GetItems().ToArray());
            }

            inventory.BeforeContentsChanged -= OnBeforeInventoryChanged;
        }

        public void Run()
        {
            return;
        }
    }
}
