using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class FactionActivityDescription : SQLQueryData
    {
        public string Action { get; set; }
        public long FromFactionId { get; set; }
        public long ToFactionId { get; set; }
        public long PlayerId { get; set; }
        public long SenderId { get; set; }
        public DateTime Timestamp { get; set; }

        public override string GetQuery()
        {
            return string.Format(@"
INSERT INTO faction_activity ([action], [from_faction], [to_faction], [player_id], [sender_id], [timestamp])
VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');",
Action, FromFactionId, ToFactionId, PlayerId, SenderId, Tools.format(Timestamp));
        }
    }
}
