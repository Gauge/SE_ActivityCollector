using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class PilotControlChangedDescription : ISQLQueryData
    {
        public long GridId { get; set; }
        public long PlayerId { get; set; }
        public int SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string GetQuery()
        {
            if (EndTime == DateTime.MinValue)
            {
                return string.Format(@"
                    INSERT INTO user_piloting ([grid_id], [player_id], [session_id], [start_time], [is_piloting])
                    VALUES ('{0}','{1}','{2}','{3}', 1);", 
                    GridId, PlayerId, SessionId, Helper.format(StartTime));
            }
            else
            {
                return string.Format(@"
                    UPDATE user_piloting
                    SET [end_time] = '{0}', [is_piloting] = 0
                    WHERE [grid_id] = '{1}' AND [player_id] = '{2}' AND [session_id] = '{3}' AND [is_piloting] = 1;",
                    Helper.format(EndTime), GridId, PlayerId, SessionId);
            }
        }
    }
}
