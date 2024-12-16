CREATE FUNCTION dbo.SplitCsv
(
    @Csv NVARCHAR(MAX),
    @Delimiter CHAR(1)
)
RETURNS @Result TABLE (RowNum INT, VALUE NVARCHAR(MAX))
AS
BEGIN
    DECLARE @Start INT = 1, @End INT, @Index INT = 1;

    WHILE @Start <= LEN(@Csv) + 1
    BEGIN
        SET @End = CHARINDEX(@Delimiter, @Csv, @Start);
        
        -- If no delimiter found, get the rest of the string
        IF @End = 0 
            SET @End = LEN(@Csv) + 1;
        
        -- Insert the substring into the result table
        INSERT INTO @Result (RowNum, VALUE)
        VALUES 
        (
            @Index,
            SUBSTRING(@Csv, @Start, @End - @Start)
        );

        -- Move to the next part of the string
        SET @Start = @End + 1;
        SET @Index += 1;
    END;

    RETURN;
END;
GO
