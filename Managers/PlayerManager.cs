using ActivityCollectorPlugin.Descriptions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
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
                //session.Players.NewPlayerRequestSucceeded += OnNewPlayerSuccess;
                //session.Players.NewPlayerRequestFailed += OnNewPlayerFailed;
                //session.Players.PlayerRemoved += OnPlayerRemoved;
                //Sync.Players.PlayersChanged += OnPlayersChanged;
                session.Players.PlayersChanged += OnPlayersChanged;
                //Sync.Players.IdentitiesChanged += OnIdentitiesChanged;
                session.Players.IdentitiesChanged += OnIdentitiesChanged;
                //session.Players.PlayerCharacterDied += OnPlayerCharacterDied;
                IsInitialized = true;
            }
        }

        private void OnIdentitiesChanged()
        {
            ActivityCollector.Log.Info("Identities have changed!");
        }

        private void OnPlayersChanged(bool connected, MyPlayer.PlayerId pid)
        {
            MyIdentity identity = session.Players.TryGetPlayerIdentity(pid);

            ActivityCollector.Log.Info($"{identity.DisplayName} has {((connected) ? "Connected" : "Disconnected")}");
            if (connected)
            {
                MyPlayer p;
                session.Players.TryGetPlayerById(pid, out p);
                p.Controller.ControlledEntityChanged += OnControlledEntityChanged;

                SQLQueryData.WriteToDatabase(new UserDescription()
                {
                    SteamId = pid.SteamId,
                    PlayerId = identity.IdentityId,
                    Name = identity.DisplayName,
                    Connected = Tools.DateTime,
                    State = LoginState.Active
                });
            }
            else
            {
                SQLQueryData.WriteToDatabase(new UserDescription()
                {
                    SteamId = pid.SteamId,
                    PlayerId = identity.IdentityId,
                    Name = identity.DisplayName,
                    Disconnected = Tools.DateTime,
                    State = LoginState.Disconnected
                });
            }
        }

        public void OnControlledEntityChanged(IMyControllableEntity oldEntity, IMyControllableEntity newEntity)
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
                    ActivityCollector.Log.Warn($"Unrecognized controlled object on player control release {oldEntity.Entity.GetType()}");
                    return;
                }

                SQLQueryData.WriteToDatabase(new UserSpawnDescription()
                {
                    CharacterId = entityId,
                    EndTime = Tools.DateTime
                });
            }
            else if (oldEntity == null)
            {
                if (newEntity.Entity is MyCharacter)
                {
                    SQLQueryData.WriteToDatabase(new UserSpawnDescription()
                    {
                        PlayerId = Tools.GetPlayerIdentityId(newEntity.Entity as MyCharacter),
                        CharacterId = newEntity.Entity.EntityId,
                        SteamId = Tools.GetPlayerSteamId(newEntity.Entity as MyCharacter),
                        StartTime = Tools.DateTime
                    });
                }
                else if (newEntity.Entity is MyShipController)
                {
                    IMyCharacter character = (newEntity.Entity as IMyShipController).LastPilot;
                    SQLQueryData.WriteToDatabase(new UserSpawnDescription()
                    {
                        PlayerId = Tools.GetPlayerIdentityId(character),
                        CharacterId = character.EntityId,
                        SteamId = Tools.GetPlayerSteamId(character),
                        StartTime = Tools.DateTime
                    });
                }
            }
            else if (newEntity.Entity is MyCharacter && oldEntity.Entity is IMyShipController)
            {
                SQLQueryData.WriteToDatabase(new UserPilotControlChangedDescription()
                {
                    PlayerId = Tools.GetPlayerIdentityId(newEntity.Entity as MyCharacter),
                    GridId = (oldEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    EndTime = Tools.DateTime
                });
            }
            else if (oldEntity.Entity is MyCharacter && newEntity.Entity is IMyShipController)
            {
                SQLQueryData.WriteToDatabase(new UserPilotControlChangedDescription()
                {
                    PlayerId = Tools.GetPlayerIdentityId(oldEntity.Entity as MyCharacter),
                    GridId = (newEntity.Entity as IMyShipController).CubeGrid.EntityId,
                    StartTime = Tools.DateTime
                });
            }
        }
    }
}
