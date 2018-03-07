CREATE PROCEDURE [dbo].add_chatlog
	@steam_id [varchar](17), 
	@message [text],
	@timestamp [datetime]
AS
BEGIN
	DECLARE @session_id int;
	SET @session_id = [dbo].get_session_id();

	INSERT INTO [dbo].chatlog ([steam_id], [session_id], [message], [timestamp])
	Values (@steam_id, @session_id, @message, @timestamp)

END