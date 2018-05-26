using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
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

        DefinitionManager definitionManager = new DefinitionManager();
        GridManager gridManager = new GridManager();
        CombatManager combatManager = new CombatManager();
        PlayerManager playerManager = new PlayerManager();
        FactionManager factionManager = new FactionManager();

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
                Created = Helper.DateTime
            };

            if (entity is MyCubeGrid)
            {
                gridManager.AddGrid(entity as MyCubeGrid);
            }
            else if (entity is MyCharacter) 
            {
            }

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(description);
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
                Removed = Helper.DateTime
            };

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(description);

            if (entity is MyCubeGrid)
            {
                gridManager.RemoveGrid(entity as MyCubeGrid);
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

            //gridManager.Run(); // dont need this
            definitionManager.Run();
            factionManager.Run();
            combatManager.Run();
            playerManager.Run();


        }
    }
}
