CREATE PROCEDURE [dbo].add_session
	@status [varchar](8),
	@timestamp [datetime]
AS
BEGIN
	DECLARE @iteration int;
	SET @iteration = [dbo].get_current_iteration();

	INSERT INTO [dbo].[sessions] ([iteration], [status], [timestamp])
	Values (@iteration, @status, @timestamp);
END