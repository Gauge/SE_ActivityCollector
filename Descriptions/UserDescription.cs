using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum LoginState { Active, Disconnected }

    public class UserDescription : SQLQueryData
    {
        public ulong SteamId { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; }
        public DateTime Connected { get; set; }
        public DateTime Disconnected { get; set; }
        public LoginState State { get; set; }

        public override string GetQuery()
        {
            if (State == LoginState.Active)
            {
                return $@"
IF NOT EXISTS (SELECT * FROM [user_names] WHERE [steam_id] = '{SteamId}' AND [username] = '{Name}')
    INSERT INTO [user_names] ([steam_id], [username], [timestamp]) VALUES ('{SteamId}', '{Name}', '{Tools.format(Connected)}');
	                
UPDATE [user_activity]
SET [state] = 'Unresolved'
WHERE [steam_id] = '{SteamId}' AND [state] = 'Active';

INSERT INTO [user_activity] ([steam_id], [player_id], [connected], [state]) 
    VALUES('{SteamId}', '{PlayerId}', '{Tools.format(Connected)}', 'Active');";
            }
            else
            {
                /*
IF NOT EXISTS (SELECT * FROM [users] WHERE [steam_id] = '{SteamId}')
    INSERT INTO [user_activity] ([steam_id], [player_id], [disconnected], [state], [blocked_id])
    VALUES ('00000000000000000', '00000000000000000000', '{Tools.format(Disconnected)}', 'Blocked', '{SteamId}');
ELSE
                 */

                return $@"
UPDATE [user_activity]
SET [disconnected] = '{Tools.format(Disconnected)}', [state] = 'Disconnected'
WHERE [steam_id] = '{SteamId}' AND [state] = 'Active';

IF @@ROWCOUNT = 0
	INSERT INTO [user_activity] ([steam_id], [player_id], [disconnected], [state]) 
	VALUES ('{SteamId}', '{PlayerId}', '{Tools.format(Disconnected)}', 'Failed');";
            }
        }
    }
}
