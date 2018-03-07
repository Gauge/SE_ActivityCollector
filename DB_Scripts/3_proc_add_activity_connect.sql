CREATE PROCEDURE [dbo].add_activity_connect
	@steam_id [varchar](17), 
	@username [nvarchar](35), 
	@connected [datetime]
AS
BEGIN
	DECLARE @session_id int;
	SET @session_id = [dbo].get_session_id();

    SELECT * FROM [dbo].[users] 
		WHERE [steam_id] = @steam_id

    IF @@ROWCOUNT = 0
        INSERT INTO [dbo].[users] ([steam_id])
        VALUES (@steam_id)

	SELECT * FROM [dbo].[usernames]
		WHERE [steam_id] = @steam_id AND [username] = @username

	IF @@ROWCOUNT = 0
		INSERT INTO [dbo].[usernames] ([steam_id], [username], [timestamp])
		VALUES (@steam_id, @username, @connected)

	UPDATE [dbo].[activity]
    SET [state] = 'Unresolved'
    WHERE [steam_id] = @steam_id AND [state] = 'Active';

    INSERT INTO [dbo].[activity] ([steam_id], [connected], [state], [session_id]) 
	VALUES(@steam_id, @connected, 'Active', @session_id)
END