using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using System;
using VRageMath;
using Sandbox.ModAPI.Weapons;
using ActivityCollectorPlugin.Descriptions;

namespace ActivityCollectorPlugin.Managers
{
    public class CombatManager : IManager
    {
        public const string AlreadyLogged = "Logged";
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes event handlers and newly joined players
        /// </summary>
        public void Run()
        {
            if (!IsInitialized)
            {
                ActivityCollectorPlugin.log.Info($"Initializing Combat Manager");
                MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, CombatDamageHandler);
                MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, CauseKeenHacks);
                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                IsInitialized = true;
            }
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            // THIS IS A HACK because keen... 
            // sets cubeblock or grid id to missiles on load. to be used in the damage system
            if (entity.GetType().Name == "MyMissile")
            {
                Analytics.Start("addMissile");
                Vector3D from = entity.WorldMatrix.Translation + entity.WorldMatrix.Backward;
                Vector3D to = entity.WorldMatrix.Translation + entity.WorldMatrix.Backward * 50;

                IMyEntity rayEntity = Helper.RaycastEntity(from, to);

                if (rayEntity != null)
                {
                    entity.Name = rayEntity.EntityId.ToString();
                }
                Analytics.Stop("addMissile");
            }
        }

        private void CombatDamageHandler(object target, MyDamageInformation info)
        {

            if (info.Amount == 0) return;

            CombatDescription log = new CombatDescription
            {
                Damage = info.Amount,
                Type = info.Type.String,
                SessionId = ActivityCollectorPlugin.CurrentSession,
                Timestamp = Helper.DateTime
            };

            if (target is IMySlimBlock)
            {
                IMySlimBlock slim = target as IMySlimBlock;
                log.Integrity = slim.Integrity;
                log.VictimGridId = slim.CubeGrid.EntityId;
                log.VictimGridBlockId = Helper.getBlockId(slim.Position);
            }
            else if (target is IMyCharacter)
            {
                IMyCharacter character = target as IMyCharacter;

                // characters keep getting hit after death we dont want to log that
                if (character.Name == AlreadyLogged) return;

                if (character.Integrity <= 0)
                {
                    character.Name = AlreadyLogged;
                }
                log.VictimEntityId = character.EntityId;
                log.Integrity = character.Integrity;
            }
            else if (target is IMyFloatingObject)
            {
                IMyFloatingObject obj = (target as IMyFloatingObject);
                log.VictimEntityId = obj.EntityId;
                log.Integrity = obj.Integrity;
            }
            else
            {
                ActivityCollectorPlugin.log.Error($"Unrecognised Victim {target.GetType()}");
            }


            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(info.AttackerId);

            if (entity == null)
            {

            }
            else if (entity is IMyCubeBlock)
            {
                IMyCubeBlock cube = entity as IMyCubeBlock;

                log.AttackerGridId = cube.CubeGrid.EntityId;
                log.AttackerEntityId = cube.EntityId;
            }
            else if (entity is IMyCharacter)
            {
                try
                {
                    IMyCharacter character = entity as IMyCharacter;

                    if (character.Name != null) // hacks continued
                    {
                        long missileId = -1;
                        long.TryParse(entity.Name, out missileId);
                        if (missileId != -1)
                        {
                            IMyEntity missileEntity = MyAPIGateway.Entities.GetEntityById(missileId);

                            if (missileEntity != null)
                            {
                                if (missileEntity is IMyCubeGrid)
                                {
                                    IMyCubeGrid grid = missileEntity as IMyCubeGrid;

                                    log.AttackerGridId = grid.EntityId;
                                }
                                else
                                {
                                    IMyCubeBlock cube = missileEntity as IMyCubeBlock;

                                    log.AttackerGridId = cube.CubeGrid.EntityId;
                                    log.AttackerGridBlockId = Helper.getBlockId(cube.Position);
                                }
                            }
                            else
                            {
                                ActivityCollectorPlugin.log.Error($"missiles parent grid was not found!");
                            }
                        }
                        else
                        {
                            ActivityCollectorPlugin.log.Error($"Entity of type {entity.GetType()} failed to parse id {entity.Name}");
                        }
                    }
                    else
                    {
                        log.AttackerEntityId = character.EntityId;
                    }
                }
                catch (Exception e)
                {
                    ActivityCollectorPlugin.log.Error(e);
                }
            }
            else if (entity is IMyGunBaseUser) // player tools
            {
                IMyGunBaseUser gun = entity as IMyGunBaseUser;

                log.AttackerEntityId = gun.Weapon.EntityId;
            }
            else if (entity is IMyEngineerToolBase)
            {
                IMyEngineerToolBase toolbase = entity as IMyEngineerToolBase;

                log.AttackerEntityId = toolbase.EntityId;
            }
            else if (entity is IMySlimBlock)
            {
                IMySlimBlock slim = entity as IMySlimBlock;

                log.AttackerGridId = slim.CubeGrid.EntityId;

                if (slim.FatBlock != null)
                {
                    log.AttackerEntityId = slim.FatBlock.EntityId;
                }
                else
                {
                }
            }
            else if (entity is IMyCubeGrid)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;

                log.AttackerGridId = grid.EntityId;
            }
            else if (entity is MyVoxelBase)
            {
                MyVoxelBase voxel = entity as MyVoxelBase;
                log.AttackerEntityId = entity.EntityId;
            }
            else if (entity.GetType().Name == "MyMissile")
            {
                long missileId = -1;
                long.TryParse(entity.Name, out missileId);
                if (missileId != -1)
                {
                    IMyEntity missileEntity = MyAPIGateway.Entities.GetEntityById(missileId);

                    if (missileEntity != null)
                    {
                        if (missileEntity is IMyCubeGrid)
                        {
                            IMyCubeGrid grid = missileEntity as IMyCubeGrid;
                            log.AttackerGridId = grid.EntityId;
                        }
                        else
                        {
                            IMyCubeBlock cube = missileEntity as IMyCubeBlock;
                            log.AttackerGridId = cube.CubeGrid.EntityId;
                            log.AttackerEntityId = cube.EntityId;
                        }
                    }
                    else
                    {
                        ActivityCollectorPlugin.log.Error($"Missles parent grid was not found!");
                    }
                }
                else
                {
                    ActivityCollectorPlugin.log.Error($"Entity of type {entity.GetType()} failed to parse id {entity.Name}");
                }
            }
            else
            {
                ActivityCollectorPlugin.log.Error($"Unknown attacker entity of type: {entity.GetType()}");
            }

            ActivityCollectorPlugin.SessionLogQueue.Enqueue(log);
        }

        private void CauseKeenHacks(object target, MyDamageInformation info)
        {
            // Missles apply damage by blowing up. it is considered a damage by player...
            if (target.GetType().Name == "MyMissile")
            {
                IMyEntity entity = MyAPIGateway.Entities.GetEntityById(info.AttackerId);
                if (entity != null)
                {
                    entity.Name = (target as IMyEntity).Name; // more hacks
                }
            }
        }
    }
}
