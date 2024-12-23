-- CSV SPLITTER FUNCTION
ALTER FUNCTION [dbo].[SplitCsv]
(
    @Csv NVARCHAR(MAX),      
    @Delimiter NVARCHAR(MAX)   
)
RETURNS @Result TABLE (RowNum INT, VALUE NVARCHAR(MAX))
AS
BEGIN
    DECLARE @Start INT = 1, @End INT, @Index INT = 1;

    -- multi-character delimiters to a unique single-character placeholder
    DECLARE @Placeholder CHAR(1) = CHAR(1); 
    SET @Csv = REPLACE(@Csv, @Delimiter, @Placeholder);

    -- Parse through the CSV
    WHILE @Start <= LEN(@Csv) + 1
    BEGIN
        SET @End = CHARINDEX(@Placeholder, @Csv, @Start);

        -- If no delimiter found, get the rest of the string
        IF @End = 0 
            SET @End = LEN(@Csv) + 1;

        -- get the value
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

        SET @Start = @End + 1;
        SET @Index += 1;
    END;

    RETURN;
END;

