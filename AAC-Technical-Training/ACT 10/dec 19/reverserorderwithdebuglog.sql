ALTER PROCEDURE [dbo].[ReverseCsvOrder]
(
    @CsvInput NVARCHAR(MAX),       
    @CsvOutput NVARCHAR(MAX) OUT   
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Debug point 1: Log input
    INSERT INTO dbo.DebugLog(Message) 
    VALUES('Input: ' + @CsvInput);

    DECLARE @ParsedData TABLE
    (
        RowNum INT IDENTITY(1,1),
        CriminalRecord NVARCHAR(MAX)
    );

    INSERT INTO @ParsedData (CriminalRecord)
    SELECT VALUE
    FROM dbo.SplitCsv(@CsvInput, CHAR(13) + CHAR(10));

    -- Debug point 2: Log parsed rows
    INSERT INTO dbo.DebugLog(Message)
    SELECT 'Row ' + CAST(RowNum AS VARCHAR) + ': ' + CriminalRecord
    FROM @ParsedData;

    SET @CsvOutput = ''

    SELECT @CsvOutput = @CsvOutput + 
        CASE 
            WHEN @CsvOutput = '' THEN CriminalRecord
            ELSE CHAR(13) + CHAR(10) + CriminalRecord 
        END
    FROM @ParsedData
    ORDER BY RowNum DESC;

    -- Debug point 3: Log output
    INSERT INTO dbo.DebugLog(Message)
    VALUES('Output: ' + @CsvOutput);
END;