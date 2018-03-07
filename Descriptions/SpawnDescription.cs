using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class SpawnDescription : ISQLQueryData
    {
        public ulong SteamId { get; set; }
        public long PlayerId { get; set; }
        public long CharacterId { get; set; }
        public int SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string GetQuery()
        {
            if (EndTime == DateTime.MinValue)
            {
                return string.Format(@"
                    INSERT INTO players ([steam_id], [player_id], [character_id], [session_id], [start_time])
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
                    SteamId, PlayerId, CharacterId, SessionId, StartTime);
            }
            else
            {
                return string.Format(@"
                    UPDATE players
                    SET [end_time] = '{0}'
                    WHERE [character_id] = '{1}' AND [session_id] = '{2}' AND [end_time] is NULL", 
                    EndTime, CharacterId, SessionId);
            }

        }
    }
}
