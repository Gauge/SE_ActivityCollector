using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class UserPilotControlChangedDescription : SQLQueryData
    {
        public long GridId { get; set; }
        public long PlayerId { get; set; }
        public int SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string GetQuery()
        {
            if (EndTime == DateTime.MinValue)
            {
                return string.Format(@"
                    INSERT INTO user_piloting ([grid_id], [player_id], [start_time], [is_piloting])
                    VALUES ('{0}','{1}','{2}', 1);",
                    GridId, PlayerId, Tools.format(StartTime));
            }
            else
            {
                return string.Format(@"
                    UPDATE user_piloting
                    SET [end_time] = '{0}', [is_piloting] = 0
                    WHERE [grid_id] = '{1}' AND [player_id] = '{2}' AND [is_piloting] = 1;",
                    Tools.format(EndTime), GridId, PlayerId);
            }
        }
    }
}
