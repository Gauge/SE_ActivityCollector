using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ActivityCollectorPlugin.Managers
{
    public class PlayerManager : IManager
    {
        private MySession session;

        public bool IsInitialized { get; set; } = false;

        public void Run()
        {
            if (!IsInitialized)
            {
                session = (MySession)MyAPIGateway.Session;
                session.Players.NewPlayerRequestSucceeded += OnNewPlayerSuccess;
                session.Players.NewPlayerRequestFailed += OnNewPlayerFailed;
                session.Players.PlayerRemoved += OnPlayerRemoved;
                session.Players.PlayersChanged += OnPlayersChanged;
                session.Players.IdentitiesChanged += OnIdentitiesChanged;
                //session.Players.PlayerCharacterDied += OnPlayerCharacterDied;
                IsInitialized = true;
            }
        }

        private void OnIdentitiesChanged()
        {
            ActivityCollectorPlugin.log.Info("Identities have changed!");
        }

        private void OnPlayersChanged(bool connected, MyPlayer.PlayerId pid)
        {
            MyIdentity identity = session.Players.TryGetPlayerIdentity(pid);

            ActivityCollectorPlugin.log.Info($"{identity.DisplayName} has {((connected) ? "Connected" : "Disconnected")}");
            if (connected)
            {
                MyPlayer p;
                session.Players.TryGetPlayerById(pid, out p);
                p.Controller.ControlledEntityChanged += OnControlledEntityChanged;

                ActivityCollectorPlugin.Enqueue(new UserDescription()
                {
                    SteamId = pid.SteamId,
                    PlayerId = identity.IdentityId,
                    Name = identity.DisplayName,
                    Connected = Helper.DateTime,
                    State = LoginState.Active
                });
            }
            else
            {
                ActivityCollectorPlugin.Enqueue(new UserDescription()
                {
                    SteamId = pid.SteamId,
                    PlayerId = identity.IdentityId,
                    Name = identity.DisplayName,
                    Disconnected = Helper.DateTime,
                    State = LoginState.Disconnected
                });
            }
        }

        private void OnNewPlayerSuccess(MyPlayer.PlayerId pid)
        {
            ActivityCollectorPlugin.log.Info($"{pid.SteamId} New Player Success");
        }

        private void OnPlayerRemoved(MyPlayer.PlayerId pid)
        {
            ActivityCollectorPlugin.log.Info($"{pid.SteamId} Has been removed");
        }

        private void OnNewPlayerFailed(int something)
        {
            ActivityCollectorPlugin.log.Info($"{something} New Player");
        }

        public void OnControlledEntityChanged(Sandbox.Game.Entities.IMyControllableEntity oldEntity, Sandbox.Game.Entities.IMyControllableEntity newEntity)
        {
            if (newEntity == null)
            {
                long entityId;
                if (oldEntity.Entity is MyCharacter)
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

                ActivityCollectorPlugin.Enqueue(new SpawnDescription()
                {
                    CharacterId = entityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    EndTime = Helper.DateTime
                });
            }
            else if (oldEntity == null)
            {
                if (newEntity.Entity is MyCharacter)
                {
                    ActivityCollectorPlugin.Enqueue(new SpawnDescription()
                    {
                        PlayerId = Helper.GetPlayerIdentityId(newEntity.Entity as MyCharacter),
                        CharacterId = newEntity.Entity.EntityId,
                        SessionId = ActivityCollectorPlugin.CurrentSession,
                        SteamId = Helper.GetPlayerSteamId(newEntity.Entity as MyCharacter),
                        StartTime = Helper.DateTime
                    });
                }
                else if (newEntity.Entity is MyShipController)
                {
                    IMyCharacter character = (newEntity.Entity as IMyShipController).LastPilot;
                    ActivityCollectorPlugin.Enqueue(new SpawnDescription()
                    {
                        PlayerId = Helper.GetPlayerIdentityId(character),
                        CharacterId = character.EntityId,
                        SessionId = ActivityCollectorPlugin.CurrentSession,
                        SteamId = Helper.GetPlayerSteamId(character),
                        StartTime = Helper.DateTime
                    });
                }
            }
            else if (newEntity.Entity is MyCharacter && oldEntity.Entity is IMyShipController)
            {
                ActivityCollectorPlugin.Enqueue(new PilotControlChangedDescription()
                {
                    PlayerId = Helper.GetPlayerIdentityId(newEntity.Entity as MyCharacter),
                    GridId = (oldEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    EndTime = Helper.DateTime
                });
            }
            else if (oldEntity.Entity is MyCharacter && newEntity.Entity is IMyShipController)
            {
                ActivityCollectorPlugin.Enqueue(new PilotControlChangedDescription()
                {
                    PlayerId = Helper.GetPlayerIdentityId(oldEntity.Entity as MyCharacter),
                    GridId = (newEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    SessionId = ActivityCollectorPlugin.CurrentSession,
                    StartTime = Helper.DateTime
                });
            }
        }
    }
}
