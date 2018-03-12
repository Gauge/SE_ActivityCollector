using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using System;
using VRageMath;
using System.Linq;
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
                FactionManager faction = new FactionManager();
                IsInitialized = true;
            }
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            // THIS IS A HACK because keen... 
            // sets cubeblock or grid id to missiles on load. to be used in the damage system
            if (entity.GetType().ToString() == "Sandbox.Game.Weapons.MyMissile")
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
                DamageType = info.Type.String,
                SessionId = ActivityCollectorPlugin.CurrentSession,
                Timestamp = DateTime.Now
            };

            if (target is IMySlimBlock)
            {
                IMySlimBlock slim = target as IMySlimBlock;
                log.TargetEntityDestroyed = slim.Integrity <= log.Damage;
                log.TargetEntityFunctional = true; // assumes true until proven false
                log.VictimGridId = slim.CubeGrid.EntityId;
                log.VictimGridName = slim.CubeGrid.DisplayName;
                log.VictimGridOwner = slim.CubeGrid.BigOwners.FirstOrDefault();
                log.VictimEntityOwner = slim.OwnerId;

                if (slim.FatBlock != null)
                {
                    log.TargetEntityFunctional = slim.FatBlock.IsFunctional;
                    log.VictimEntityId = slim.FatBlock.EntityId;
                    log.VictimEntityName = slim.FatBlock.DisplayName;
                    log.VictimEntityType = slim.FatBlock.BlockDefinition.TypeIdString;
                    log.VictimEntitySubtype = slim.FatBlock.BlockDefinition.SubtypeId;
                    log.VictimEntityObjectType = slim.FatBlock.GetType().Name;
                }
                else
                {
                    log.VictimEntityName = slim.BlockDefinition.DisplayNameText;
                    log.VictimEntityObjectType = slim.GetType().Name;
                }
            }
            else if (target is IMyCharacter)
            {
                IMyCharacter character = target as IMyCharacter;

                // characters keep getting hit after death we dont want to log that
                if (character.Name == AlreadyLogged) return;
                log.TargetEntityDestroyed = log.TargetEntityFunctional = character.Integrity <= log.Damage;
                if (log.TargetEntityDestroyed)
                {
                    character.Name = AlreadyLogged;
                }

                log.VictimEntityId = character.EntityId;
                log.VictimEntityName = character.DisplayName;
                log.VictimEntityType = character.Definition.DisplayNameText;
                log.VictimEntitySubtype = character.Definition.DescriptionText;
                log.VictimEntityObjectType = character.GetType().Name;
                log.VictimEntityOwner = Helper.GetPlayerIdentityId(character);
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
                log.AttackerGridName = cube.CubeGrid.DisplayName;
                log.AttackerGridOwner = cube.CubeGrid.BigOwners.FirstOrDefault();

                log.AttackerEntityId = cube.EntityId;
                log.AttackerEntityName = cube.DisplayName;
                log.AttackerEntityType = cube.BlockDefinition.TypeIdString;
                log.AttackerEntitySubtype = cube.BlockDefinition.SubtypeName;
                log.AttackerEntityObjectType = cube.GetType().Name;
                log.AttackerEntityOwner = cube.OwnerId;
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
                                    log.AttackerGridName = grid.DisplayName;
                                    log.AttackerGridOwner = grid.BigOwners.FirstOrDefault();
                                }
                                else
                                {
                                    IMyCubeBlock cube = missileEntity as IMyCubeBlock;

                                    log.AttackerGridId = cube.CubeGrid.EntityId;
                                    log.AttackerGridName = cube.CubeGrid.DisplayName;
                                    log.AttackerGridOwner = cube.CubeGrid.BigOwners.FirstOrDefault();

                                    log.AttackerEntityId = cube.EntityId;
                                    log.AttackerEntityName = cube.DisplayName;
                                    log.AttackerEntityType = cube.BlockDefinition.TypeIdString;
                                    log.AttackerEntitySubtype = cube.BlockDefinition.SubtypeName;
                                    log.AttackerEntityObjectType = cube.GetType().Name;
                                    log.AttackerEntityOwner = cube.OwnerId;
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
                        log.AttackerEntityName = character.DisplayName;
                        log.AttackerEntityType = character.Definition.DisplayNameText;
                        log.AttackerEntitySubtype = character.Definition.DescriptionText;
                        log.AttackerEntityObjectType = character.GetType().Name;
                        log.AttackerEntityOwner = Helper.GetPlayerIdentityId(character);
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
                log.AttackerEntityName = gun.Weapon.DisplayName;
                log.AttackerEntityType = gun.Weapon.GetObjectBuilder().SubtypeId.String;
                log.AttackerEntitySubtype = gun.Weapon.GetObjectBuilder().SubtypeName;
                log.AttackerEntityObjectType = gun.Weapon.GetType().Name;
                log.AttackerEntityOwner = MyAPIGateway.Players.GetPlayerControllingEntity(gun.Owner).IdentityId;
            }
            else if (entity is IMyEngineerToolBase)
            {
                IMyEngineerToolBase toolbase = entity as IMyEngineerToolBase;

                log.AttackerEntityId = toolbase.EntityId;
                log.AttackerEntityName = toolbase.DisplayName;
                log.AttackerEntityType = toolbase.GetObjectBuilder().Name;
                log.AttackerEntitySubtype = toolbase.GetObjectBuilder().SubtypeName;
                log.AttackerEntityObjectType = toolbase.GetType().Name;
                log.AttackerEntityOwner = toolbase.OwnerIdentityId;
            }
            else if (entity is IMySlimBlock)
            {
                IMySlimBlock slim = entity as IMySlimBlock;

                log.AttackerGridId = slim.CubeGrid.EntityId;
                log.AttackerGridName = slim.CubeGrid.DisplayName;
                log.AttackerGridOwner = slim.CubeGrid.BigOwners.FirstOrDefault();
                log.AttackerEntityOwner = slim.OwnerId;

                if (slim.FatBlock != null)
                {
                    log.AttackerEntityId = slim.FatBlock.EntityId;
                    log.AttackerEntityName = slim.FatBlock.DisplayName;
                    log.AttackerEntityType = slim.FatBlock.BlockDefinition.TypeIdString;
                    log.AttackerEntitySubtype = slim.FatBlock.BlockDefinition.SubtypeName;
                    log.AttackerEntityObjectType = slim.FatBlock.GetType().Name;
                }
                else
                {
                    log.AttackerEntityName = slim.BlockDefinition.DisplayNameText;
                    log.AttackerEntityObjectType = slim.GetType().Name;
                }
            }
            else if (entity is IMyCubeGrid)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;

                log.AttackerGridId = grid.EntityId;
                log.AttackerGridName = grid.DisplayName;
                log.AttackerGridOwner = grid.BigOwners.FirstOrDefault();
            }
            else if (entity is MyVoxelBase)
            {
                MyVoxelBase voxel = entity as MyVoxelBase;

                log.AttackerEntityId = entity.EntityId;
                log.AttackerEntityName = voxel.RootVoxel.StorageName;
                log.AttackerEntityType = (voxel.RootVoxel.Name == null) ? "Asteroid" : "Planet";
                log.AttackerEntityObjectType = entity.GetType().Name;
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
                            log.AttackerGridName = grid.DisplayName;
                            log.AttackerGridOwner = grid.BigOwners.FirstOrDefault();
                        }
                        else
                        {
                            IMyCubeBlock cube = missileEntity as IMyCubeBlock;

                            log.AttackerGridId = cube.CubeGrid.EntityId;
                            log.AttackerGridName = cube.CubeGrid.DisplayName;
                            log.AttackerGridOwner = cube.CubeGrid.BigOwners.FirstOrDefault();

                            log.AttackerEntityId = cube.EntityId;
                            log.AttackerEntityName = cube.DisplayName;
                            log.AttackerEntityType = cube.BlockDefinition.TypeIdString;
                            log.AttackerEntitySubtype = cube.BlockDefinition.SubtypeName;
                            log.AttackerEntityObjectType = cube.GetType().Name;
                            log.AttackerEntityOwner = cube.OwnerId;
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
