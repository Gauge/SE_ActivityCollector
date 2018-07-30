using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class UserSpawnDescription : SQLQueryData
    {
        public ulong SteamId { get; set; }
        public long PlayerId { get; set; }
        public long CharacterId { get; set; }
        public int SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string GetQuery()
        {
            if (EndTime == DateTime.MinValue)
            {
                return string.Format(@"
                    INSERT INTO user_spawns ([steam_id], [player_id], [character_id], [start_time])
                    VALUES ('{0}', '{1}', '{2}', '{3}');",
                    SteamId, PlayerId, CharacterId, Tools.format(StartTime));
            }
            else
            {
                return string.Format(@"
                    UPDATE user_spawns
                    SET [end_time] = '{0}'
                    WHERE [character_id] = '{1}' AND [end_time] is NULL;",
                    Tools.format(EndTime), CharacterId);
            }
        }
    }
}
