using System;
using System.Text;
using VRage.Game.ModAPI;

namespace ActivityCollectorPlugin.Descriptions
{
    public class FactionActivityDescription : ISQLQueryData
    {
        public MyFactionStateChange Action { get; set; }
        public IMyFaction FromFaction { get; set; }
        public IMyFaction ToFaction { get; set; }
        public long FromFactionId { get; set; }
        public long ToFactionId { get; set; }
        public long PlayerId { get; set; }
        public long SenderId { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            StringBuilder query = new StringBuilder();
            if (FromFaction != null)
            {
                query.Append(string.Format(@"
SELECT * FROM factions WHERE [faction_id] = '{0}'
IF @@ROWCOUNT = 0
    INSERT INTO factions ([faction_id], [iteration_id], [tag], [name], [description], [creation_date])
    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
", FromFaction.FactionId, ActivityCollectorPlugin.CurrentIteration, FromFaction.Tag, FromFaction.Name, FromFaction.Description, Helper.format(Timestamp)));
            }

            if (ToFaction != null)
            {
                query.Append(string.Format(@"
SELECT * FROM factions WHERE [faction_id] = '{0}'
IF @@ROWCOUNT = 0
    INSERT INTO factions ([faction_id], [iteration_id], [tag], [name], [description], [creation_date])
    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
", ToFaction.FactionId, ActivityCollectorPlugin.CurrentIteration, ToFaction.Tag, ToFaction.Name, ToFaction.Description, Helper.format(Timestamp)));
            }

            query.Append(string.Format(@"
INSERT INTO faction_activity ([action], [session_id], [from_faction], [to_faction], [player_id], [sender_id], [timestamp])
VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');", 
Action.ToString(), ActivityCollectorPlugin.CurrentSession, FromFactionId, ToFactionId, PlayerId, SenderId, Helper.format(Timestamp)));

            return query.ToString();
        }
    }
}
