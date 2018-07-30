using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.Sync;

namespace ActivityCollectorPlugin.Managers
{
    class EntityManager : IManager
    {
        public bool IsInitialized { get; private set; }

        private DefinitionManager definitionManager = new DefinitionManager();
        PlayerManager playerManager = new PlayerManager();
        FactionManager factionManager = new FactionManager();
        ChatManager chatManager = new ChatManager();
        private GridManager gridManager;
        CombatManager combatManager = new CombatManager();

        private Dictionary<long, InventoryComponent> RegisteredInventories = new Dictionary<long, InventoryComponent>();


        public EntityManager()
        {
             gridManager = new GridManager();
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

            if (entity is MyCubeGrid)
            {
                gridManager.AddGrid(entity as MyCubeGrid);
            }

            if (!RegisteredInventories.ContainsKey(entity.EntityId))
            {
                RegisteredInventories.Add(entity.EntityId, new InventoryComponent(entity));
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

            if (entity is MyCubeGrid)
            {
                gridManager.RemoveGrid(entity as MyCubeGrid);
            }

            if (RegisteredInventories.ContainsKey(entity.EntityId))
            {
                RegisteredInventories[entity.EntityId].Close();
                RegisteredInventories.Remove(entity.EntityId);
            }
        }

        public void Run()
        {
            if (!IsInitialized)
            {
                HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(entities);

                foreach (IMyEntity e in entities)
                {
                    OnEntityAdd(e);
                }

                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                MyAPIGateway.Entities.OnEntityRemove += OnEntityRemove;

                IsInitialized = true;
            }


            definitionManager.Run();
            playerManager.Run();
            factionManager.Run();
            chatManager.Run();
            combatManager.Run();
        }
    }
}
