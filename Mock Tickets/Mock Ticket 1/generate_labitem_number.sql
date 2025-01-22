CREATE FUNCTION [dbo].[GenerateLabItemNumber]
(
    @labItemNumberPrefix NVARCHAR(50)
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @maxSuffix DECIMAL(10, 1);
    DECLARE @newSuffix DECIMAL(10, 1);
    DECLARE @newLabItemNumber NVARCHAR(50);

    -- Determine the maximum suffix for existing sub-items
    SELECT @maxSuffix = MAX(CAST(SUBSTRING(LAB_ITEM_NUMBER, LEN(@labItemNumberPrefix) + 2, LEN(LAB_ITEM_NUMBER)) AS DECIMAL(10, 1)))
    FROM TV_LABITEM
    WHERE LAB_ITEM_NUMBER LIKE @labItemNumberPrefix + '.%';

    -- Generate the new suffix
    IF @maxSuffix IS NULL
        SET @newSuffix = 1.0;
    ELSE
        SET @newSuffix = @maxSuffix + 0.1;

    -- Generate the new LAB_ITEM_NUMBER
    SET @newLabItemNumber = @labItemNumberPrefix + '.' + CAST(@newSuffix AS NVARCHAR(50));

    RETURN @newLabItemNumber;
END;
GO


