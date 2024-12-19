CREATE FUNCTION [dbo].[SplitCsv]
(
    @Csv NVARCHAR(MAX), -- input CSV
    @Delimiter NVARCHAR(MAX) -- delimiter (supports multi-character)
)
RETURNS @Result TABLE (RowNum INT, VALUE NVARCHAR(MAX))
AS
BEGIN
    DECLARE @Start INT = 1, @End INT, @Index INT = 1;

    -- Replace multi-character delimiters with a single character placeholder
    SET @Csv = REPLACE(@Csv, @Delimiter, '|');

    WHILE @Start <= LEN(@Csv) + 1
    BEGIN
        SET @End = CHARINDEX('|', @Csv, @Start);

        -- If no delimiter found, get the rest of the string
        IF @End = 0 
            SET @End = LEN(@Csv) + 1;

        -- Extract the substring and handle empty fields
        DECLARE @Value NVARCHAR(MAX) = SUBSTRING(@Csv, @Start, @End - @Start);

        IF @Value = ''
            SET @Value = NULL;

        -- Insert into the result table
        INSERT INTO @Result (RowNum, VALUE)
        VALUES 
        (
            @Index,
            LTRIM(RTRIM(@Value))
        );

        -- Move to the next part of the string
        SET @Start = @End + 1;
        SET @Index += 1;
    END;

    RETURN;
END;