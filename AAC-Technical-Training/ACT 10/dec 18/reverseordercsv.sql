ALTER PROCEDURE dbo.ReverseCsvOrder
(
    @CsvInput NVARCHAR(MAX),       -- Input CSV string
    @CsvOutput NVARCHAR(MAX) OUT   -- Output reversed CSV string
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare a table variable to store parsed data
    DECLARE @ParsedData TABLE
    (
        RowNum INT IDENTITY(1,1), -- Automatically increments row number
        CriminalRecord NVARCHAR(MAX)
    );

    -- Step 1: Insert parsed rows into the table variable
    INSERT INTO @ParsedData (CriminalRecord)
    SELECT VALUE
    FROM dbo.SplitCsv(@CsvInput, '\n'); -- Split by newline to preserve rows

    -- Step 2: Reverse the rows and concatenate into a single CSV
    SELECT @CsvOutput = STRING_AGG(CriminalRecord, '\n') WITHIN GROUP (ORDER BY RowNum DESC);

    -- Optional: Return the reversed rows as a result set
    SELECT CriminalRecord
    FROM @ParsedData
    ORDER BY RowNum DESC;
END;