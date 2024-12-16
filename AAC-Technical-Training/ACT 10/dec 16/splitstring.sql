USE [VENTURA_DEV]
GO
/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 12/16/2024 9:16:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[SplitString]
(
	-- Add the parameters for the function here
	@myString	VARCHAR(4000),
	@delimiter	VARCHAR(10)
)
RETURNS 
@ReturnTable TABLE 
(
	-- Add the column definitions for the TABLE variable here
	[part] [VARCHAR](50) NULL
)
AS
BEGIN
		DECLARE @iSpaces INT
		DECLARE @part VARCHAR(50)

		--initialize spaces
		SELECT @iSpaces = CHARINDEX(@delimiter,@myString,0)
		WHILE @iSpaces > 0

		BEGIN
			SELECT @part = SUBSTRING(@myString,0,CHARINDEX(@delimiter,@myString,0))

			INSERT INTO @ReturnTable(part)
			SELECT @part

			SELECT @myString = SUBSTRING(@mystring,CHARINDEX(@delimiter,@myString,0)+ LEN(@delimiter), LEN(@myString) - CHARINDEX(' ', @myString,0))
			SELECT @iSpaces = CHARINDEX(@delimiter,@myString,0)
		END

		IF LEN(@myString) > 0
			INSERT INTO @ReturnTable
			SELECT @myString
	
	RETURN 
END


