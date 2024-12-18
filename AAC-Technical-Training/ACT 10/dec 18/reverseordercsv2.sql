USE [VENTURA_DEV]
GO
/****** Object:  StoredProcedure [dbo].[ReverseCsvOrder]    Script Date: 12/18/2024 9:37:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE dbo.ReverseCsvOrder
(
    @CsvInput NVARCHAR(MAX),       -- Input CSV string
    @CsvOutput NVARCHAR(MAX) OUT   -- Output reversed CSV string
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Step 1: Create a temporary table to store parsed data
    DECLARE @ParsedData TABLE
	(
		RowNum INT IDENTITY(1,1),
		CriminalRecord NVARCHAR(MAX)
	);

	-- Store the rows
	INSERT INTO @ParsedData (CriminalRecord)
	SELECT VALUE
	FROM dbo.SplitCsv(@CsvInput, '\n');

	-- Initialize output
	SET @CsvOutput = ''

	-- Build output string by concatenating records in reverse order
	SELECT @CsvOutput = @CsvOutput + 
		CASE 
			WHEN @CsvOutput = '' THEN CriminalRecord
			ELSE '\n' + CriminalRecord 
		END
	FROM @ParsedData
	ORDER BY RowNum DESC

    -- Optional: Return the reversed rows as a result set
    SELECT CriminalRecord
    FROM @ParsedData
    ORDER BY RowNum DESC;
END;
GO
