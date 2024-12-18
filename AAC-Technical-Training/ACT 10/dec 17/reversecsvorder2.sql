CREATE PROCEDURE dbo.ReverseCsvOrder
(
    @CsvInput NVARCHAR(MAX),       -- Input CSV string
    @CsvOutput NVARCHAR(MAX) OUT   -- Output reversed CSV string
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Parse the CSV into rows and reverse the order using ROW_NUMBER
    WITH ParsedData AS
    (
        SELECT 
            VALUE AS CriminalRecord,
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum
        FROM dbo.SplitCsv(@CsvInput, ',')
    )
    -- Combine the rows in reverse order into a single CSV string
    SELECT @CsvOutput = STRING_AGG(CriminalRecord, ',') 
    FROM ParsedData
    ORDER BY RowNum DESC;

    -- Optional: Return the reversed rows as a result set
    SELECT 
        CriminalRecord
    FROM ParsedData
    ORDER BY RowNum DESC;
END;
GO