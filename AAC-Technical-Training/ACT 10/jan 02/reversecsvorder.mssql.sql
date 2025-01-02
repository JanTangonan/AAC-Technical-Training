USE [VENTURA_DEV]
GO
/****** Object:  StoredProcedure [dbo].[ReverseCsvOrder]    Script Date: 1/2/2025 1:54:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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

    -- Log input for debugging
    INSERT INTO dbo.DebugLog (Message) 
    VALUES ('Input: ' + @CsvInput);

    -- Parse the input into rows using the SplitCsv function
    DECLARE @ParsedData TABLE
    (
        RowNum INT IDENTITY(1,1),
        CriminalRecord NVARCHAR(MAX)
    );

    INSERT INTO @ParsedData (CriminalRecord)
    SELECT Value
    FROM dbo.SplitCsv(@CsvInput, CHAR(13) + CHAR(10)); -- Using normalized delimiter

    -- Log parsed rows for debugging
    INSERT INTO dbo.DebugLog (Message)
    SELECT 'Row ' + CAST(RowNum AS VARCHAR) + ': ' + CriminalRecord
    FROM @ParsedData;

    -- Initialize output
    SET @CsvOutput = '';

    -- Concatenate reversed rows
    SELECT @CsvOutput = @CsvOutput + 
        CASE 
            WHEN @CsvOutput = '' THEN CriminalRecord
            ELSE CHAR(13) + CHAR(10) + CriminalRecord 
        END
    FROM @ParsedData
    ORDER BY RowNum DESC;

    -- Log the final output for debugging
    INSERT INTO dbo.DebugLog (Message)
    VALUES ('Output: ' + @CsvOutput);
END;
