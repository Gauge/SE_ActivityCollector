using ActivityCollectorPlugin.Descriptions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Torch.API;
using Torch.API.Managers;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;

namespace ActivityCollectorPlugin.Managers
{
    public class PlayerManager
    {
        private List<IMyPlayer> updatedPlayerList;
        private List<IMyPlayer> connectedPlayers;
        private List<IMyPlayer> tempPlayers;
        private Task listener;
        private int stateChanges = 0;
        private bool isUpdated = false;

        public PlayerManager(IMultiplayerManagerBase multiplayerManager)
        {
            ActivityCollectorPlugin.log.Info($"Initializing Player Manager");
            multiplayerManager.PlayerJoined += OnClientChange;
            multiplayerManager.PlayerLeft += OnClientChange;
            updatedPlayerList = new List<IMyPlayer>();
            connectedPlayers = new List<IMyPlayer>();
            tempPlayers = new List<IMyPlayer>();

            listener = new Task(Update);
        }

        public void Run()
        {
            if (!isUpdated && MyAPIGateway.Session != null)
            {
                lock (tempPlayers)
                {
                    tempPlayers.Clear();
                    MyAPIGateway.Players.GetPlayers(tempPlayers);
                    isUpdated = true;
                }
            }
        }

        private void OnControlledEntityChanged(IMyControllableEntity oldEntity, IMyControllableEntity newEntity)
        {
            if (newEntity == null)
            {
                long entityId;
                if (oldEntity.Entity is IMyCharacter)
                {
                    entityId = oldEntity.Entity.EntityId;
                }
                else if (oldEntity.Entity is IMyShipController)
                {
                    entityId = (oldEntity.Entity as IMyShipController).LastPilot.EntityId;
                }
                else
                {
                    ActivityCollectorPlugin.log.Warn($"Unrecognized controlled object on player control release {oldEntity.Entity.GetType()}");
                    return;
                }

                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new SpawnDescription()
                {
                    CharacterId = entityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    EndTime = DateTime.Now
                });
            }
            else if (oldEntity == null)
            {
                if (newEntity.Entity is IMyCharacter)
                {
                    ActivityCollectorPlugin.SessionLogQueue.Enqueue(new SpawnDescription()
                    {
                        PlayerId = Helper.GetPlayerIdentityId(newEntity.Entity as IMyCharacter),
                        CharacterId = newEntity.Entity.EntityId,
                        SessionId = ActivityCollectorPlugin.CurrentSession,
                        SteamId = Helper.GetPlayerSteamId(newEntity.Entity as IMyCharacter),
                        StartTime = DateTime.Now
                    });
                }
                else if (newEntity.Entity is IMyShipController)
                {
                    IMyCharacter character = (newEntity.Entity as IMyShipController).LastPilot;
                    ActivityCollectorPlugin.SessionLogQueue.Enqueue(new SpawnDescription()
                    {
                        PlayerId = Helper.GetPlayerIdentityId(character),
                        CharacterId = character.EntityId,
                        SessionId = ActivityCollectorPlugin.CurrentSession,
                        SteamId = Helper.GetPlayerSteamId(character),
                        StartTime = DateTime.Now
                    });
                }
            }
            else if (newEntity.Entity is IMyCharacter && oldEntity.Entity is IMyShipController)
            {
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new PilotControlChangedDescription()
                {
                    PlayerId = Helper.GetPlayerIdentityId(newEntity.Entity as IMyCharacter),
                    GridId = (oldEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    EndTime = DateTime.Now
                });
            }
            else if (oldEntity.Entity is IMyCharacter && newEntity.Entity is IMyShipController)
            {
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new PilotControlChangedDescription()
                {
                    PlayerId = Helper.GetPlayerIdentityId(oldEntity.Entity as IMyCharacter),
                    GridId = (newEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    StartTime = DateTime.Now
                });
            }
        }

        private void OnClientChange(IPlayer p)
        {
            stateChanges++;

            if (listener.Status == TaskStatus.Created || listener.Status == TaskStatus.Canceled || listener.Status == TaskStatus.Faulted || listener.Status == TaskStatus.RanToCompletion)
            {
                if (listener.Status == TaskStatus.RanToCompletion)
                {
                    listener.Dispose();
                }

                listener = new Task(Update);
                listener.Start();
            }
        }

        private void Update()
        {
            while (stateChanges > 0)
            {
                if (!isUpdated) continue;

                lock (tempPlayers)
                {
                    int tpCount = tempPlayers.Count;
                    for (int i = 0; i < tpCount; i++)
                    {
                        if (i < connectedPlayers.Count)
                        {
                            if (connectedPlayers[i] != tempPlayers[i])
                            {
                                if (!connectedPlayers.Contains(tempPlayers[i]))
                                {
                                    ActivityCollectorPlugin.log.Info($"(i) Adding Player {tempPlayers[i].DisplayName}");
                                    ActivityCollectorPlugin.SessionLogQueue.Enqueue(new UserDescription()
                                    {
                                        SteamId = tempPlayers[i].SteamUserId,
                                        PlayerId = tempPlayers[i].IdentityId,
                                        SessionId = ActivityCollectorPlugin.CurrentSession,
                                        Name = tempPlayers[i].DisplayName,
                                        Connected = DateTime.Now,
                                        State = LoginState.Active
                                    });
                                    stateChanges--;

                                    tempPlayers[i].Controller.ControlledEntityChanged += OnControlledEntityChanged;
                                    connectedPlayers.Insert(i, tempPlayers[i]);
                                }
                                else if (!tempPlayers.Contains(connectedPlayers[i]))
                                {
                                    ActivityCollectorPlugin.log.Info($"(ra) Removing Player {connectedPlayers[i].DisplayName}");
                                    ActivityCollectorPlugin.SessionLogQueue.Enqueue(new UserDescription()
                                    {
                                        SteamId = connectedPlayers[i].SteamUserId,
                                        PlayerId = connectedPlayers[i].IdentityId,
                                        SessionId = ActivityCollectorPlugin.CurrentSession,
                                        Disconnected = DateTime.Now,
                                        State = LoginState.Disconnected
                                    });
                                    stateChanges--;

                                    connectedPlayers.RemoveAt(i);
                                }
                            }
                        }
                        else
                        {
                            ActivityCollectorPlugin.log.Info($"(a) Adding Player {tempPlayers[i].DisplayName}");
                            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new UserDescription()
                            {
                                SteamId = tempPlayers[i].SteamUserId,
                                PlayerId = tempPlayers[i].IdentityId,
                                SessionId = ActivityCollectorPlugin.CurrentSession,
                                Name = tempPlayers[i].DisplayName,
                                Connected = DateTime.Now,
                                State = LoginState.Active
                            });
                            stateChanges--;

                            tempPlayers[i].Controller.ControlledEntityChanged += OnControlledEntityChanged;
                            connectedPlayers.Add(tempPlayers[i]);
                        }
                    }

                    int cpCount = connectedPlayers.Count;
                    if (cpCount > tpCount)
                    {
                        for (int i = tpCount; i < cpCount; i++)
                        {
                            ActivityCollectorPlugin.log.Info($"(rr) Removing Player {connectedPlayers[i].DisplayName}");
                            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new UserDescription()
                            {
                                SteamId = connectedPlayers[i].SteamUserId,
                                PlayerId = connectedPlayers[i].IdentityId,
                                SessionId = ActivityCollectorPlugin.CurrentSession,
                                Disconnected = DateTime.Now,
                                State = LoginState.Disconnected
                            });
                            stateChanges--;
                            connectedPlayers.RemoveAt(i);
                        }
                    }
                    
                    tempPlayers.Clear();
                }

                isUpdated = false;
                Thread.Sleep(2000);
            }
        }
    }
}
