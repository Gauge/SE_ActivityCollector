CREATE PROCEDURE [dbo].cleanup_activity
AS
BEGIN
	UPDATE [dbo].[activity]
	SET [state] = 'Unresolved'
	WHERE [state] = 'Active';

	UPDATE [dbo].[piloting]
	SET [ended] = GETDATE(), [is_piloting] = 0
	WHERE [is_piloting] = 1
END