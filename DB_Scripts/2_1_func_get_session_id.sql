CREATE FUNCTION [dbo].get_session_id()
RETURNS int
BEGIN
	DECLARE @session_id int;
	SET @session_id = (SELECT TOP 1 [id] FROM [dbo].[sessions] 
						ORDER BY [timestamp] DESC);
	
	IF @session_id IS NULL
		RETURN -1;

	RETURN @session_id;
END