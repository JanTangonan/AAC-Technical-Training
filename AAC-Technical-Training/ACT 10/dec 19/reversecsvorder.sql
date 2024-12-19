USE [VENTURA_DEV]
GO
/****** Object:  UserDefinedFunction [dbo].[SplitCsv]    Script Date: 12/19/2024 2:09:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[SplitCsv]
(
    @Csv NVARCHAR(MAX),       -- Input CSV
    @Delimiter NVARCHAR(MAX)  -- Multi-character delimiter
)
RETURNS @Result TABLE (RowNum INT, VALUE NVARCHAR(MAX))
AS
BEGIN
    DECLARE @Start INT = 1, @End INT, @Index INT = 1;

    -- Normalize multi-character delimiters to a unique single-character placeholder
    DECLARE @Placeholder CHAR(1) = CHAR(1); -- Use a control character unlikely to appear in data
    SET @Csv = REPLACE(@Csv, @Delimiter, @Placeholder);

    -- Parse through the CSV
    WHILE @Start <= LEN(@Csv) + 1
    BEGIN
        SET @End = CHARINDEX(@Placeholder, @Csv, @Start);

        -- If no delimiter found, get the rest of the string
        IF @End = 0 
            SET @End = LEN(@Csv) + 1;

        -- Extract the value
        DECLARE @Value NVARCHAR(MAX) = SUBSTRING(@Csv, @Start, @End - @Start);

        -- Trim and handle empty values
        INSERT INTO @Result (RowNum, VALUE)
        VALUES 
        (
            @Index,
            CASE 
                WHEN LTRIM(RTRIM(@Value)) = '' THEN NULL
                ELSE LTRIM(RTRIM(@Value))
            END
        );

        -- Move to the next substring	
        SET @Start = @End + 1;
        SET @Index += 1;
    END;

    RETURN;
END;

