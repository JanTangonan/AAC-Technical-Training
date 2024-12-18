CREATE PROCEDURE dbo.ReverseCsvOrder
(
    @CsvInput NVARCHAR(MAX),       -- Input CSV string
    @CsvOutput NVARCHAR(MAX) OUT   -- Output reversed CSV string
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Temporary table to hold parsed rows
    CREATE TABLE #ParsedData
    (
        RowNum INT IDENTITY(1,1),
        CriminalRecord NVARCHAR(MAX)
    );

    -- Split the input CSV into rows and insert into temporary table
    INSERT INTO #ParsedData (CriminalRecord)
    SELECT VALUE
    FROM dbo.SplitCsv(@CsvInput, ','); -- Your custom SplitCsv function

    -- Combine the rows in reverse order into a single CSV string
    DECLARE @ReversedCsv NVARCHAR(MAX);

    SELECT @ReversedCsv = STRING_AGG(CriminalRecord, ',')
    FROM #ParsedData
    ORDER BY RowNum DESC;

    -- Set the OUT parameter
    SET @CsvOutput = @ReversedCsv;

    -- Return the rows in reverse order as a result set (optional)
    SELECT CriminalRecord
    FROM #ParsedData
    ORDER BY RowNum DESC;

    -- Clean up
    DROP TABLE #ParsedData;
END;
GO
