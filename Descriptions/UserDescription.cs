using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum LoginState { Active, Disconnected }

    public class UserDescription : ISQLQueryData
    {
        public ulong SteamId { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; }
        public int SessionId { get; set; } = ActivityCollectorPlugin.CurrentSession;
        public DateTime Connected { get; set; }
        public DateTime Disconnected { get; set; }
        public LoginState State { get; set; }

        public string GetQuery()
        {
            if (State == LoginState.Active)
            {
                return $@"
IF NOT EXISTS (SELECT * FROM [users] WHERE [steam_id] = '{SteamId}')
    INSERT INTO [users] (steam_id) VALUES ('{SteamId}');

IF NOT EXISTS (SELECT * FROM [user_names] WHERE [steam_id] = '{SteamId}' AND [username] = '{Name}')
    INSERT INTO [user_names] ([steam_id], [username], [timestamp]) VALUES ('{SteamId}', '{Name}', '{Helper.format(Connected)}');
	                
UPDATE [user_activity]
SET [state] = 'Unresolved'
WHERE [steam_id] = '{SteamId}' AND [state] = 'Active';

INSERT INTO [user_activity] ([steam_id], [player_id], [connected], [state], [session_id]) 
    VALUES('{SteamId}', '{PlayerId}', '{Helper.format(Connected)}', 'Active', '{SessionId}');";
            }
            else
            {
                return $@"
IF NOT EXISTS (SELECT * FROM [users] WHERE [steam_id] = '{SteamId}')
    INSERT INTO [user_activity] ([steam_id], [player_id], [disconnected], [state], [session_id], [blocked_id])
    VALUES ('00000000000000000', '00000000000000000000', '{Helper.format(Disconnected)}', 'Blocked', '{SessionId}', '{SteamId}');
ELSE
    UPDATE [user_activity]
    SET [disconnected] = '{Helper.format(Disconnected)}', [state] = 'Disconnected'
    WHERE [steam_id] = '{SteamId}' AND [state] = 'Active';

    IF @@ROWCOUNT = 0
	    INSERT INTO [user_activity] ([steam_id], [player_id], [disconnected], [state], [session_id]) 
	    VALUES ('{SteamId}', '{PlayerId}', '{Helper.format(Disconnected)}', 'Failed', '{SessionId}');";
            }
        }
    }
}
