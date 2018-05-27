namespace ActivityCollectorPlugin.Managers
{
    using Sandbox.Game.Entities;
    using global::ActivityCollectorPlugin.Descriptions;
    using Sandbox.Game.Entities.Cube;
    using System.Collections.Generic;

    public class GridManager : IManager
    {

        public bool IsInitialized { get; private set; } = true;

        private InventoryManager inventoryManager;

        public GridManager(InventoryManager im)
        {
            inventoryManager = im;
        }

        private Dictionary<long, float> LastBlockIntegrity = new Dictionary<long, float>();

        public void AddGrid(MyCubeGrid grid)
        {
            grid.OnNameChanged += OnGridNameChange;
            grid.OnGridSplit += OnGridSplit;
            //grid.OnStaticChanged += OnStaticStateChanged;

            grid.OnBlockAdded += OnBlockAdded;
            grid.OnBlockRemoved += OnBlockRemoved;
            grid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
            //grid.OnBlockOwnershipChanged += OnOwnershipChanged;

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
            {
                GridId = grid.EntityId,
                Type = grid.GridSizeEnum.ToString(),
                Created = Helper.DateTime
            });

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = Helper.DateTime
            });

            AddGridBlocks(grid);
        }

        public void RemoveGrid(MyCubeGrid grid)
        {
            grid.OnNameChanged -= OnGridNameChange;
            grid.OnGridSplit -= OnGridSplit;
            //grid.OnStaticChanged -= OnStaticStateChanged;

            grid.OnBlockAdded -= OnBlockAdded;
            grid.OnBlockRemoved -= OnBlockRemoved;

            //grid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;
            //grid.OnBlockOwnershipChanged -= OnOwnershipChanged;

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
            {
                GridId = grid.EntityId,
                Removed = Helper.DateTime
            });

            foreach (MySlimBlock block in grid.GetBlocks())
            {
                OnBlockRemoved(block);
            }
        }

        private void OnGridNameChange(MyCubeGrid grid)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = Helper.DateTime
            });
        }

        private void OnGridSplit(MyCubeGrid parent, MyCubeGrid child)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
            {
                GridId = child.EntityId,
                ParentId = parent.EntityId,
                SplitWithParent = Helper.DateTime
            });
        }

        private void AddGridBlocks(MyCubeGrid grid)
        {
            foreach (MySlimBlock slim in grid.GetBlocks())
            {
                OnBlockAdded(slim);
            }
        }

        private void OnBlockAdded(MySlimBlock slim)
        {
            long blockEntityId = 0;
            slim.CubeGridChanged += OnBlockCubeGridChanged;

            if (!LastBlockIntegrity.ContainsKey(slim.UniqueId))
            {
                LastBlockIntegrity.Add(slim.UniqueId, slim.Integrity);
            }

            if (slim.FatBlock != null && slim.FatBlock is MyTerminalBlock)
            {
                MyTerminalBlock tb = slim.FatBlock as MyTerminalBlock;

                tb.OwnershipChanged += OnBlockOwnershipChanged;
                blockEntityId = tb.EntityId;

                OnBlockOwnershipChanged(tb);

                if (tb.HasInventory)
                {
                    //MyInventoryBase inventory = tb.GetInventoryBase();
                    //inventory.BeforeContentsChanged += inventoryManager.OnBeforeInventoryChanged;
                    //inventory.ContentsChanged += inventoryManager.OnInventoryChanged;
                }

                //    ((MyTerminalBlock)slim.FatBlock).PropertiesChanged += OnBlockPropertyChange;
                //    ((MyTerminalBlock)slim.FatBlock).SyncPropertyChanged += OnBlockPropertyChange;
                //    ((MyTerminalBlock)slim.FatBlock).
                //    ((MyTerminalBlock)slim.FatBlock).GetInventoryBase().ContentsChanged += OnBlockInventoryChange;
            }

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockDescription()
            {
                BlockEntityId = blockEntityId,
                GridId = slim.CubeGrid.EntityId,
                BuiltBy = slim.BuiltBy,
                TypeId = slim.BlockDefinition.Id.TypeId.ToString(),
                SubTypeId = slim.BlockDefinition.Id.SubtypeId.ToString(),
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Created = Helper.DateTime
            });
        }

        private void OnBlockCubeGridChanged(MySlimBlock slim, MyCubeGrid grid)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockDescription()
            {
                GridId = grid.EntityId,
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Removed = Helper.DateTime
            });

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockDescription()
            {
                GridId = slim.CubeGrid.EntityId,
                BuiltBy = slim.BuiltBy,
                TypeId = slim.BlockDefinition.Id.TypeId.ToString(),
                SubTypeId = slim.BlockDefinition.Id.SubtypeId.ToString(),
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Created = Helper.DateTime
            });
        }

        private void OnBlockRemoved(MySlimBlock slim)
        {
            slim.CubeGridChanged -= OnBlockCubeGridChanged;
            if (slim.FatBlock != null && slim.FatBlock is MyTerminalBlock)
            {
                //((MyTerminalBlock)slim.FatBlock).PropertiesChanged -= OnBlockPropertyChange;
            }

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockDescription()
            {
                GridId = slim.CubeGrid.EntityId,
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Removed = Helper.DateTime
            });
        }

        private void OnBlockIntegrityChanged(MySlimBlock slim)
        {
            float integrityDelta = LastBlockIntegrity[slim.UniqueId] - slim.Integrity;
            if (integrityDelta < 0)
            {
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new CombatDescription()
                {
                    VictimGridBlockId = Helper.getBlockId(slim.Position),
                    VictimGridId = slim.CubeGrid.EntityId,
                    Type = "Repair",
                    Damage = integrityDelta,
                    Integrity = slim.Integrity,
                    Timestamp = Helper.DateTime
                });
            }

            LastBlockIntegrity[slim.UniqueId] = slim.Integrity;
        }

        private void OnBlockOwnershipChanged(MyTerminalBlock block)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockOwnershipDescription() {
                GridId = block.CubeGrid.EntityId,
                BlockEntityId = block.EntityId,
                Owner = block.OwnerId,
                Timestamp = Helper.DateTime
            });
        }

        //private void OnBlockPropertyChange(MyTerminalBlock block)
        //{
        //    List<ITerminalProperty> props = new List<ITerminalProperty>();
        //    block.GetProperties(props);

        //    StringBuilder b = new StringBuilder();
        //    foreach (ITerminalProperty prop in props)
        //    {
        //        b.Append($"[{prop.Id}] {prop.TypeName}");

        //        if (prop.TypeName == typeof(bool).Name)
        //        {
        //            b.Append($" {prop.AsBool().GetValue(block)}\n");
        //        }
        //        else if (prop.TypeName == typeof(Color).Name)
        //        {
        //            b.Append($" {prop.AsColor().GetValue(block)}\n");
        //        }
        //        else if (prop.TypeName == typeof(float).Name)
        //        {
        //            b.Append($" {prop.AsFloat().GetValue(block)}\n");
        //        }
        //        else if (prop.TypeName == typeof(StringBuilder).Name)
        //        {
        //            b.Append($" {prop.As<StringBuilder>().GetValue(block)}\n");
        //        }
        //    }
        //    ActivityCollectorPlugin.log.Info(b.ToString());
        //}

        //private void OnBlockPropertyChange(SyncBase s)
        //{
        //    ActivityCollectorPlugin.log.Info($"[{s.Id}] {s.ValueType} {s as Sync<object, SyncDirection.BothWays> == null} {s as Sync<object, SyncDirection.FromServer> == null}");

        //    ActivityCollectorPlugin.log.Info((s as Sync<object, SyncDirection.FromServer>).ToString());
        //}



        public void Run()
        {
        }
    }
}
