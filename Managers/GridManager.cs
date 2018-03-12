namespace ActivityCollectorPlugin.Managers
{
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using Sandbox.ModAPI;
    using Sandbox.Game.Entities;
    using global::ActivityCollectorPlugin.Descriptions;
    using System;
    using Sandbox.Game.Entities.Cube;
    using System.Collections.Generic;

    public class GridManager : IManager
    {

        public bool IsInitialized { get; private set; } = false;

        private void OnEntityAdd(IMyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                AddNewGrid(entity as MyCubeGrid);
            }
        }

        private void OnEntityRemove(IMyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
                {
                    GridId = entity.EntityId,
                    Removed = DateTime.Now
                });
            }
        }

        private void AddNewGrid(MyCubeGrid grid)
        {
            grid.OnNameChanged += OnGridNameChange;
            grid.OnGridSplit += OnGridSplit;
            //grid.OnStaticChanged += OnStaticStateChanged;

            grid.OnBlockAdded += OnBlockAdded;
            grid.OnBlockRemoved += OnBlockRemoved;
            grid.OnBlockIntegrityChanged += OnBlockChanged;
            //grid.OnBlockOwnershipChanged += OnOwnershipChanged;

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
            {
                GridId = grid.EntityId,
                Type = grid.GridSizeEnum.ToString(),
                Created = DateTime.Now
            });

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = DateTime.Now
            });

            AddGridBlocks(grid);
        }

        private void OnGridNameChange(MyCubeGrid grid)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridNameDescription()
            {
                GridId = grid.EntityId,
                Name = grid.DisplayName,
                Timestamp = DateTime.Now
            });
        }

        private void OnGridSplit(MyCubeGrid parent, MyCubeGrid child)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new GridDescription()
            {
                GridId = child.EntityId,
                ParentId = parent.EntityId,
                SplitWithParent = DateTime.Now
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
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new BlockDescription()
            {
                BlockId = slim.UniqueId,
                GridId = slim.CubeGrid.EntityId,
                BuiltBy = slim.BuiltBy,
                Name = slim.BlockDefinition.DisplayNameText,
                Type = (slim.FatBlock == null) ? "SLIM" : "FAT",
                MaxIntegrity = slim.BlockDefinition.MaxIntegrity,
                X = slim.Position.X,
                Y = slim.Position.Y,
                Z = slim.Position.Z
            });
        }

        private void OnBlockRemoved(MySlimBlock slim)
        {
            return;
        }

        private void OnBlockChanged(MySlimBlock slim)
        {
            return;
        }

        private void OnOwnershipChanged(MyCubeGrid grid)
        {
            return;
        }



        public void Run()
        {
            if (!IsInitialized)
            {
                HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(entities, x => x is IMyCubeGrid);

                foreach (MyCubeGrid grid in entities)
                {
                    AddNewGrid(grid);
                }

                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                MyAPIGateway.Entities.OnEntityRemove += OnEntityRemove;

                IsInitialized = true;
            }
        }
    }
}
