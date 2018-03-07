using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum LoginState { Active, Disconnected }

    public class UserDescription : ISQLQueryData
    {
        public ulong SteamId { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; }
        public int SessionId { get; set; }
        public DateTime Connected { get; set; }
        public DateTime Disconnected { get; set; }
        public LoginState State { get; set; }

        public string GetQuery()
        {
            if (State == LoginState.Active)
            {
                return string.Format(@"
                    SELECT * FROM [dbo].[users] 
		                WHERE [steam_id] = '{0}'

                    IF @@ROWCOUNT = 0
                        INSERT INTO [dbo].[users] ([steam_id])
                        VALUES ('{0}')

	                SELECT * FROM [dbo].[usernames]
		                WHERE [steam_id] = '{0}' AND [username] = '{1}'

	                IF @@ROWCOUNT = 0
		                INSERT INTO [dbo].[usernames] ([steam_id], [username], [timestamp])
		                VALUES ('{0}', '{1}', '{2}')

	                UPDATE [dbo].[activity]
                    SET [state] = 'Unresolved'
                    WHERE [steam_id] = '{0}' AND [state] = 'Active';

                    INSERT INTO [dbo].[activity] ([steam_id], [player_id], [connected], [state], [session_id]) 
	                VALUES('{0}', '{4}', '{2}', 'Active', '{3}')", 
                    SteamId, Name, Connected, SessionId, PlayerId);
            }
            else
            {
                return string.Format(@"
                    SELECT * FROM users
	                WHERE steam_id = '{0}'

	                IF @@ROWCOUNT = 0
		                INSERT INTO activity ([steam_id], [player_id], [disconnected], [state], [session_id], [blocked_id])
		                VALUES ('00000000000000000', '00000000000000000000', '{1}', 'Blocked', '{2}', '{0}')
	                ELSE
		                UPDATE [dbo].[activity]
		                SET [disconnected] = '{1}', [state] = 'Disconnected'
		                WHERE [steam_id] = '{0}' AND [state] = 'Active';

		                IF @@ROWCOUNT = 0
			                INSERT INTO [dbo].[activity] ([steam_id], [player_id], [disconnected], [state], [session_id]) 
			                VALUES ('{0}', '{3}', '{1}', 'Failed', '{2}');",
                    SteamId, Disconnected, SessionId, PlayerId);
            }
        }
    }
}
