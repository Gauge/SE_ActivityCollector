using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.Entity;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace ActivityCollectorPlugin.Managers
{
    public class InventoryComponent
    {
        public MyEntity Entity { get; private set; }
        public long EntityId => (Entity is MyTerminalBlock) ? (Entity as MyTerminalBlock).CubeGrid.EntityId : Entity.EntityId;
        public long BlockId => (Entity is MyTerminalBlock) ? Entity.EntityId : 0;

        public MyInventoryBase Output => Entity.GetInventoryBase(0);
        public MyInventoryBase Input => Entity.GetInventoryBase(1);

        private List<InventoryItem> ItemDeltas = new List<InventoryItem>();

        private MyPhysicalInventoryItem[] LastOutput = new MyPhysicalInventoryItem[0];
        private MyPhysicalInventoryItem[] LastInput = new MyPhysicalInventoryItem[0];

        private int ProductionInventory = 0;
        private int GeneralInventory = 0;

        public InventoryComponent(MyEntity entity)
        {
            Entity = entity;

            if (entity.HasInventory)
            {
                Entity.OnClose += OnClose;

                InventoryDescription description = new InventoryDescription();

                if (Output != null)
                {
                    Output.BeforeContentsChanged += OnBeforeOutputChange;
                    Output.ContentsChanged += OnOutputChange;

                    if (!(entity is MyCharacter))
                    {
                        foreach (var item in Output.GetItems())
                        {
                            description.Add(EntityId, BlockId, item.ItemId, InvType.Output, InvAction.Current, item.Amount, item.Content.TypeId.ToString(), item.Content.SubtypeId.ToString());
                        }
                    }
                }

                if (Input != null)
                {
                    Input.BeforeContentsChanged += OnBeforeInputChange;
                    Input.ContentsChanged += OnInputChange;

                    foreach (var item in Input.GetItems())
                    {
                        description.Add(EntityId, BlockId, item.ItemId, InvType.Input, InvAction.Current, item.Amount, item.Content.TypeId.ToString(), item.Content.SubtypeId.ToString());
                    }
                }

                SQLQueryData.WriteToDatabase(description);
            }
        }

        public void LogData()
        {
            InventoryDescription description = new InventoryDescription();

            foreach (InventoryItem item in ItemDeltas)
            {
                if (item.Added > 0)
                {
                    description.Add(EntityId, BlockId, item.Index, item.InventoryType, InvAction.Add, item.Added, item.TypeId.ToString(), item.SubtypeId.ToString());
                }

                if (item.Removed > 0)
                {
                    description.Add(EntityId, BlockId, item.Index, item.InventoryType, InvAction.Remove, item.Removed, item.TypeId.ToString(), item.SubtypeId.ToString());
                }
            }

            SQLQueryData.WriteToDatabase(description);
            ItemDeltas.Clear();
        }

        private void OnBeforeOutputChange(MyInventoryBase output)
        {
            LastOutput = output.GetItems().ToArray();
        }

        private void OnOutputChange(MyInventoryBase output)
        {
            MyPhysicalInventoryItem[] currentOutput = output.GetItems().ToArray();

            IEnumerable<MyPhysicalInventoryItem> added = currentOutput.Except(LastOutput);
            IEnumerable<MyPhysicalInventoryItem> removed = LastOutput.Except(currentOutput);

            if (removed.Count() == 1 && added.Count() == 1)
            {
                MyPhysicalInventoryItem old = removed.First();
                MyPhysicalInventoryItem cur = added.First();

                if (old.Amount > cur.Amount)
                {
                    old.Amount -= cur.Amount;
                    UpdateAmounts(InvType.Output, InvAction.Remove, old);
                }
                else
                {
                    cur.Amount -= old.Amount;
                    UpdateAmounts(InvType.Output, InvAction.Add, cur);
                }
            }
            else if (removed.Count() >= 1)
            {
                foreach (MyPhysicalInventoryItem item in removed)
                {
                    UpdateAmounts(InvType.Output, InvAction.Remove, item);
                }
            }
            else if (added.Count() >= 1)
            {
                foreach (MyPhysicalInventoryItem item in added)
                {
                    UpdateAmounts(InvType.Output, InvAction.Add, item);
                }
            }

            Increment();
        }

        private void OnBeforeInputChange(MyInventoryBase input)
        {
            LastInput = input.GetItems().ToArray();
        }

        private void OnInputChange(MyInventoryBase input)
        {
            MyPhysicalInventoryItem[] currentInput = input.GetItems().ToArray();

            IEnumerable<MyPhysicalInventoryItem> added = currentInput.Except(LastInput);
            IEnumerable<MyPhysicalInventoryItem> removed = LastInput.Except(currentInput);

            if (added.Count() > 1 || removed.Count() > 1)
            {
                ActivityCollector.Log.Error("More than one component being added or removed!!");
            }

            if (removed.Count() == 1 && added.Count() == 1)
            {
                MyPhysicalInventoryItem old = removed.First();
                MyPhysicalInventoryItem cur = added.First();

                if (old.Amount > cur.Amount)
                {
                    old.Amount -= cur.Amount;
                    UpdateAmounts(InvType.Input, InvAction.Remove, old);
                }
                else
                {
                    cur.Amount -= old.Amount;
                    UpdateAmounts(InvType.Input, InvAction.Add, cur);
                }
            }
            else if (removed.Count() == 1)
            {
                UpdateAmounts(InvType.Input, InvAction.Remove, removed.First());
            }
            else if (added.Count() == 1)
            {
                UpdateAmounts(InvType.Input, InvAction.Add, added.First());
            }

            Increment();
        }

        private void Increment()
        {
            if (Input != null || Entity is MyShipDrill || Entity is MyGasGenerator)
            {
                if (ProductionInventory == ActivityCollector.Config.Data.ProductionInventoryInterval)
                {
                    LogData();
                    ProductionInventory = 0;
                }
                ProductionInventory++;
            }
            else
            {
                if (GeneralInventory == ActivityCollector.Config.Data.GeneralInventoryInterval)
                {
                    LogData();
                    GeneralInventory = 0;
                }
                GeneralInventory++;
            }
        }

        private void UpdateAmounts(InvType type, InvAction action, MyPhysicalInventoryItem item)
        {
            InventoryItem storage = ItemDeltas.Find(x => x.Index == item.ItemId && x.TypeId == item.Content.TypeId && x.SubtypeId == item.Content.SubtypeId);

            if (storage == null)
            {
                ItemDeltas.Add(new InventoryItem()
                {
                    TypeId = item.Content.TypeId,
                    SubtypeId = item.Content.SubtypeId,
                    Index = item.ItemId,
                    InventoryType = type,
                    Removed = (action == InvAction.Remove) ? item.Amount : 0,
                    Added = (action == InvAction.Add) ? item.Amount : 0
                });
            }
            else if (action == InvAction.Add)
            {
                storage.Added += item.Amount;
            }
            else if (action == InvAction.Remove)
            {
                storage.Removed += item.Amount;
            }
        }

        private void OnClose(MyEntity e)
        {
            Entity.OnClose -= OnClose;
            Close();
        }

        public void Close()
        {
            LogData();
            if (Output != null)
            {
                Output.BeforeContentsChanged -= OnBeforeOutputChange;
                Output.ContentsChanged -= OnOutputChange;
            }

            if (Input != null)
            {
                Input.BeforeContentsChanged -= OnBeforeInputChange;
                Input.ContentsChanged -= OnInputChange;
            }
        }
    }

    public class InventoryItem
    {
        public MyObjectBuilderType TypeId { get; set; }
        public MyStringHash SubtypeId { get; set; }
        public InvType InventoryType { get; set; }
        public uint Index { get; set; }
        public MyFixedPoint Added { get; set; } = new MyFixedPoint();
        public MyFixedPoint Removed { get; set; } = new MyFixedPoint();
    }
}
