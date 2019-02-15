using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace ActivityCollectorPlugin.Managers
{
    class EntityManager : IManager
    {
        public bool IsInitialized { get; private set; }

        private Settings conf;

        private DefinitionManager definitionManager = new DefinitionManager();
        private PlayerManager playerManager = new PlayerManager();
        private FactionManager factionManager = new FactionManager();
        private ChatManager chatManager = new ChatManager();
        private GridManager gridManager = new GridManager();
        private CombatManager combatManager = new CombatManager();
        private MovementManager movementManager = new MovementManager();

        private Dictionary<long, InventoryComponent> RegisteredInventories = new Dictionary<long, InventoryComponent>();

        public EntityManager()
        {
        }

        private void OnEntityAdd(IMyEntity e)
        {
            MyEntity entity = e as MyEntity;

            EntityDescription description = new EntityDescription()
            {
                EntityId = entity.EntityId,
                Name = entity.DisplayNameText,
                ObjectType = entity.GetType().Name,
                TypeId = ((entity.DefinitionId.HasValue) ? entity.DefinitionId.Value.TypeId.ToString() : string.Empty),
                SubtypeId = ((entity.DefinitionId.HasValue) ? entity.DefinitionId.Value.SubtypeId.ToString() : string.Empty),
                Created = Tools.DateTime
            };

            SQLQueryData.WriteToDatabase(description);

            if (conf.LogGrids)
            {
                if (entity is MyCubeGrid)
                {
                    gridManager.AddGrid(entity as MyCubeGrid);
                }

                if (conf.LogInventory && !RegisteredInventories.ContainsKey(entity.EntityId))
                {
                    RegisteredInventories.Add(entity.EntityId, new InventoryComponent(entity));
                }
            }
        }

        private void OnEntityRemove(IMyEntity e)
        {
            if (!((MySession)MyAPIGateway.Session).Ready) return;
            MyEntity entity = e as MyEntity;

            EntityDescription description = new EntityDescription()
            {
                EntityId = entity.EntityId,
                Name = entity.DisplayNameText,
                ObjectType = entity.GetType().Name,
                TypeId = ((entity.DefinitionId.HasValue) ? entity.DefinitionId.Value.TypeId.ToString() : string.Empty),
                SubtypeId = ((entity.DefinitionId.HasValue) ? entity.DefinitionId.Value.SubtypeId.ToString() : string.Empty),
                Removed = Tools.DateTime
            };

            SQLQueryData.WriteToDatabase(description);
            if (conf.LogGrids)
            {
                if (entity is MyCubeGrid)
                {
                    gridManager.RemoveGrid(entity as MyCubeGrid);
                }

                if (conf.LogInventory && RegisteredInventories.ContainsKey(entity.EntityId))
                {
                    RegisteredInventories[entity.EntityId].Close();
                    RegisteredInventories.Remove(entity.EntityId);
                }
            }
        }

        public void Run()
        {
            if (!IsInitialized)
            {
                conf = ActivityCollector.Config.Data;

                HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(entities);

                foreach (IMyEntity e in entities)
                {
                    OnEntityAdd(e);
                }

                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                MyAPIGateway.Entities.OnEntityRemove += OnEntityRemove;

                if (conf.LogDefinitions)
                {
                    MyAPIGateway.Session.OnSessionReady += definitionManager.SessionReady;
                }

                if (conf.LogMovement)
                {
                    MyAPIGateway.Entities.OnEntityAdd += movementManager.AddEntity;
                    MyAPIGateway.Entities.OnEntityRemove += movementManager.RemoveEntity;
                }

                IsInitialized = true;
            }

            if (conf.LogPlayers)
                playerManager.Run();

            if (conf.LogFactions)
                factionManager.Run();

            //if (conf.LogDefinitions)
            //    definitionManager.Run();

            if (conf.LogChat)
                chatManager.Run();

            if (conf.LogCombat)
                combatManager.Run();

            //if (conf.LogMovement)
            //    movementManager.Run();
        }
    }
}
