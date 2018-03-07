using ActivityCollectorPlugin.Descriptions;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace ActivityCollectorPlugin.Managers
{
    public class FactionManager
    {
        public bool IsInitialized { get; private set; } = false;

        public void Run()
        {
            if (!IsInitialized)
            {
                ActivityCollectorPlugin.log.Info($"Initializing Faction Manager");
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
                ActivityCollectorPlugin.log.Info($"New Faction Created: {faction.Tag} | {faction.Name}");
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new FactionDescription()
                {
                    FactionId = id,
                    Tag = faction.Tag,
                    Name = faction.Name,
                    Description = faction.Description,
                    CreationDate = DateTime.Now,
                    State = FactionState.Create
                });
            }
        }

        private void OnFactionEdited(long id)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(id);

            if (faction != null)
            {
                ActivityCollectorPlugin.log.Info($"Faction Edited: {faction.Tag} | {faction.Name}");
                ActivityCollectorPlugin.SessionLogQueue.Enqueue(new FactionDescription()
                {
                    FactionId = id,
                    Tag = faction.Tag,
                    Name = faction.Name,
                    Description = faction.Description,
                    State = FactionState.Edit
                });
            }
        }

        private void OnFactionStateChanged(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
        {
            ActivityCollectorPlugin.SessionLogQueue.Enqueue(new FactionActivityDescription()
            {
                Action = action,
                FromFaction = MyAPIGateway.Session.Factions.TryGetFactionById(fromFaction),
                ToFaction = MyAPIGateway.Session.Factions.TryGetFactionById(toFaction),
                FromFactionId = fromFaction,
                ToFactionId = toFaction,
                PlayerId = playerId,
                SenderId = senderId,
                Timestamp = DateTime.Now
            });

            if (action == MyFactionStateChange.RemoveFaction)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(fromFaction);

                if (faction != null)
                {
                    ActivityCollectorPlugin.log.Info($"Faction Removed: {faction.Tag} | {faction.Name}");
                    ActivityCollectorPlugin.SessionLogQueue.Enqueue(new FactionDescription()
                    {
                        FactionId = fromFaction,
                        TerminationDate = DateTime.Now,
                        State = FactionState.Remove
                    });
                }
            }
        }
    }
}
