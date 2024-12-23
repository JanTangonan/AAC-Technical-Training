-- REVERSE CSV ORDER STORED PROCEDURE
ALTER PROCEDURE [dbo].[ReverseCsvOrder]
(
    @CsvInput NVARCHAR(MAX),
    @CsvOutput NVARCHAR(MAX) OUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize line endings to Windows-style
    SET @CsvInput = REPLACE(@CsvInput, CHAR(10), CHAR(13) + CHAR(10));

	INSERT INTO dbo.DebugLog(Message) 
    VALUES('Input: ' + @CsvInput);

    -- Parse the input into rows
    DECLARE @ParsedData TABLE
    (
        RowNum INT IDENTITY(1,1),
        CriminalRecord NVARCHAR(MAX)
    );

    INSERT INTO @ParsedData (CriminalRecord)
    SELECT part
    FROM dbo.SplitString(@CsvInput, CHAR(13) + CHAR(10)); 

	INSERT INTO dbo.DebugLog(Message)
    SELECT 'Row ' + CAST(RowNum AS VARCHAR) + ': ' + CriminalRecord
    FROM @ParsedData;

    SET @CsvOutput = '';

    -- Concatenate reversed rows
    SELECT @CsvOutput = @CsvOutput + 
        CASE 
            WHEN @CsvOutput = '' THEN CriminalRecord
            ELSE CHAR(13) + CHAR(10) + CriminalRecord 
        END
    FROM @ParsedData
    ORDER BY RowNum DESC;

	INSERT INTO dbo.DebugLog(Message)
    VALUES('Output: ' + @CsvOutput);
END;