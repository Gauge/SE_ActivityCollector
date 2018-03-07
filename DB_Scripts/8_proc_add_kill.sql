CREATE PROCEDURE [dbo].add_kill
	@killer_id [char](17), 
	@weapon [nvarchar](128), 
	@killer_was_piloting [bit],
	@killer_grid_id [varchar](50),
	@killer_grid_name [nvarchar](128),
	@victim_id [char](17),
	@victim_was_piloting [bit],
	@victim_grid_id [varchar](50),
	@victim_grid_name [nvarchar](128),
	@timestamp [datetime]
AS
BEGIN
	DECLARE @session_id int;
	SET @session_id = [dbo].get_session_id();

	INSERT INTO [dbo].[kills] ([session_id], [killer_id], [weapon], [killer_was_piloting], [killer_grid_id], [killer_grid_name], [victim_id], [victim_was_piloting], [victim_grid_id], [victim_grid_name], [timestamp])
	VALUES (@session_id, @killer_id, @weapon, @killer_was_piloting, @killer_grid_id, @killer_grid_name, @victim_id, @victim_was_piloting, @victim_grid_id, @victim_grid_name, @timestamp)
END