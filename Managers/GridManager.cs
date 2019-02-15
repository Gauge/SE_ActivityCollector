using Sandbox.Game.Entities;
using global::ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities.Cube;
using System.Collections.Generic;
using System;

namespace ActivityCollectorPlugin.Managers
{

    public class GridManager : IManager
    {
        public bool IsInitialized { get; private set; } = true;

        private Dictionary<long, InventoryComponent> RegisteredBlockInventories = new Dictionary<long, InventoryComponent>();
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

            SQLQueryData.WriteToDatabase(new GridDescription()
            {
                GridId = grid.EntityId,
                Type = grid.GridSizeEnum.ToString(),
                Created = Tools.DateTime
            });

            SQLQueryData.WriteToDatabase(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = Tools.DateTime
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

            SQLQueryData.WriteToDatabase(new GridDescription()
            {
                GridId = grid.EntityId,
                Removed = Tools.DateTime
            });

            foreach (MySlimBlock block in grid.GetBlocks())
            {
                OnBlockRemoved(block);
            }
        }

        private void OnGridNameChange(MyCubeGrid grid)
        {
            SQLQueryData.WriteToDatabase(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = Tools.DateTime
            });
        }

        private void OnGridSplit(MyCubeGrid parent, MyCubeGrid child)
        {
            SQLQueryData.WriteToDatabase(new GridDescription()
            {
                GridId = child.EntityId,
                ParentId = parent.EntityId,
                SplitWithParent = Tools.DateTime
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

                if (!RegisteredBlockInventories.ContainsKey(tb.EntityId))
                {
                    RegisteredBlockInventories.Add(tb.EntityId, new InventoryComponent(tb));
                }

                tb.OwnershipChanged += OnBlockOwnershipChanged;
                blockEntityId = tb.EntityId;

                OnBlockOwnershipChanged(tb);
            }

            SQLQueryData.WriteToDatabase(new BlockDescription()
            {
                BlockEntityId = blockEntityId,
                GridId = slim.CubeGrid.EntityId,
                BuiltBy = slim.BuiltBy,
                TypeId = slim.BlockDefinition.Id.TypeId.ToString(),
                SubTypeId = slim.BlockDefinition.Id.SubtypeId.ToString(),
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Created = Tools.DateTime
            });
        }

        private void OnBlockCubeGridChanged(MySlimBlock slim, MyCubeGrid grid)
        {
            SQLQueryData.WriteToDatabase(new BlockDescription()
            {
                GridId = grid.EntityId,
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Removed = Tools.DateTime
            });

            SQLQueryData.WriteToDatabase(new BlockDescription()
            {
                GridId = slim.CubeGrid.EntityId,
                BuiltBy = slim.BuiltBy,
                TypeId = slim.BlockDefinition.Id.TypeId.ToString(),
                SubTypeId = slim.BlockDefinition.Id.SubtypeId.ToString(),
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Created = Tools.DateTime
            });
        }

        private void OnBlockRemoved(MySlimBlock slim)
        {
            slim.CubeGridChanged -= OnBlockCubeGridChanged;
            if (slim.FatBlock != null && slim.FatBlock is MyTerminalBlock)
            {
                if (RegisteredBlockInventories.ContainsKey(slim.FatBlock.EntityId))
                {
                    RegisteredBlockInventories[slim.FatBlock.EntityId].Close();
                    RegisteredBlockInventories.Remove(slim.FatBlock.EntityId);
                }
            }

            SQLQueryData.WriteToDatabase(new BlockDescription()
            {
                GridId = slim.CubeGrid.EntityId,
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z,
                Removed = Tools.DateTime
            });
        }

        private void OnBlockIntegrityChanged(MySlimBlock slim)
        {
            if (!LastBlockIntegrity.ContainsKey(slim.UniqueId))
            {
                LastBlockIntegrity.Add(slim.UniqueId, slim.Integrity);
            }

            float integrityDelta = LastBlockIntegrity[slim.UniqueId] - slim.Integrity;
            if (integrityDelta < 0)
            {
                SQLQueryData.WriteToDatabase(new CombatDescription()
                {
                    VictimGridBlockId = Tools.GetBlockId(slim.Position),
                    VictimGridId = slim.CubeGrid.EntityId,
                    Type = "Repair",
                    Damage = integrityDelta,
                    Integrity = slim.Integrity,
                    Timestamp = Tools.DateTime
                });
            }

            LastBlockIntegrity[slim.UniqueId] = slim.Integrity;
        }

        private void OnBlockOwnershipChanged(MyTerminalBlock block)
        {
            SQLQueryData.WriteToDatabase(new BlockOwnershipDescription()
            {
                GridId = block.CubeGrid.EntityId,
                BlockEntityId = block.EntityId,
                Owner = block.OwnerId,
                Timestamp = Tools.DateTime
            });
        }

        public void Run()
        {
        }
    }
}
