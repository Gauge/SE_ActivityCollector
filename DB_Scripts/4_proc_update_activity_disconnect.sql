CREATE PROCEDURE [dbo].update_activity_disconnect
	@steam_id [varchar](25), 
	@username [nvarchar](35), 
	@disconnected [datetime]
AS
BEGIN
	DECLARE @session_id int;
	SET @session_id = [dbo].get_session_id();

	SELECT * FROM users
	WHERE steam_id = @steam_id

	IF @@ROWCOUNT = 0
		INSERT INTO activity (steam_id, disconnected, [state], [session_id], [blocked_id])
		VALUES ('00000000000000000', @disconnected, 'Blocked', @session_id, @steam_id)
	ELSE
		UPDATE [dbo].[activity]
		SET disconnected = @disconnected, [state] = 'Disconnected'
		WHERE steam_id = @steam_id AND [state] = 'Active';

		IF @@ROWCOUNT = 0
			INSERT INTO [dbo].[activity] (steam_id, disconnected, [state], [session_id]) 
			VALUES (@steam_id, @disconnected, 'Failed', @session_id);
END