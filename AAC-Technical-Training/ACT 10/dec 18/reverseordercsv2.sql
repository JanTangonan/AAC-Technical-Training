USE [VADFS]
GO

CREATE PROCEDURE dbo.ReverseCsvOrder
(
    @CsvInput NVARCHAR(MAX),       
    @CsvOutput NVARCHAR(MAX) OUT   
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ParsedData TABLE
    (
        RowNum INT IDENTITY(1,1),
        CriminalRecord NVARCHAR(MAX)
    );

    INSERT INTO @ParsedData (CriminalRecord)
    SELECT VALUE
    FROM dbo.SplitCsv(@CsvInput, '\n');

    SET @CsvOutput = ''

    SELECT @CsvOutput = @CsvOutput + 
        CASE 
            WHEN @CsvOutput = '' THEN CriminalRecord
            ELSE '\n' + CriminalRecord 
        END
    FROM @ParsedData
    ORDER BY RowNum DESC;
END;
GO