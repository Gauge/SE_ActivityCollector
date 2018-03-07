CREATE FUNCTION [dbo].get_current_iteration()
RETURNS int
BEGIN
	DECLARE @iteration_id int
	SET @iteration_id = (SELECT TOP 1 [id] FROM [dbo].[iterations]
							ORDER BY [startdate] DESC);

	RETURN @iteration_id;
END