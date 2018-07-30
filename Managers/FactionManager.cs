using ActivityCollectorPlugin.Descriptions;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ActivityCollectorPlugin.Managers
{
    public class FactionManager : IManager
    {
        public bool IsInitialized { get; private set; } = false;

        public void Run()
        {
            if (!IsInitialized)
            {
                ActivityCollector.Log.Info($"Initializing Faction Manager");
                MyAPIGateway.Session.Factions.FactionCreated += OnFactionCreated;
                MyAPIGateway.Session.Factions.FactionEdited += OnFactionEdited;
                MyAPIGateway.Session.Factions.FactionStateChanged += OnFactionStateChanged;
                IsInitialized = true;
            }
        }

        private void OnFactionCreated(long id)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(id);

            if (faction != null)
            {
                ActivityCollector.Log.Info($"New Faction Created: {faction.Tag} | {faction.Name}");

                SQLQueryData.WriteToDatabase(new FactionActivityDescription()
                {
                    FromFactionId = id,
                    ToFactionId = id,
                    PlayerId = faction.FounderId,
                    SenderId = faction.FounderId,
                    Action = "CreateFaction",
                    Timestamp = Tools.DateTime
                });

                SQLQueryData.WriteToDatabase(new FactionNameDescription()
                {
                    FactionId = id,
                    Tag = faction.Tag,
                    Name = faction.Name,
                    Description = faction.Description,
                    Timestamp = Tools.DateTime
                });
            }
        }

        private void OnFactionEdited(long id)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(id);

            if (faction != null)
            {
                ActivityCollector.Log.Info($"Faction Edited: {faction.Tag} | {faction.Name}");
                SQLQueryData.WriteToDatabase(new FactionNameDescription()
                {
                    FactionId = id,
                    Tag = faction.Tag,
                    Name = faction.Name,
                    Description = faction.Description,
                    Timestamp = Tools.DateTime
                });
            }
        }

        private void OnFactionStateChanged(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
        {
            SQLQueryData.WriteToDatabase(new FactionActivityDescription()
            {
                Action = action.ToString(),
                FromFactionId = fromFaction,
                ToFactionId = toFaction,
                PlayerId = playerId,
                SenderId = senderId,
                Timestamp = Tools.DateTime
            });
        }
    }
}
