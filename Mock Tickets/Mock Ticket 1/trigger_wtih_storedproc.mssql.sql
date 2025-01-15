CREATE TRIGGER trg_create_subitem
ON TV_LABITEM
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert sub-item into TV_LABITEM if AUTO_CREATE_SUBITEM is 'T'
    INSERT INTO TV_LABITEM (PARENT_ECN, ITEM_DESCRIPTION, ITEM_TYPE)
    SELECT i.Evidence_Control_Number, 'Default Subitem Details', i.ITEM_TYPE
    FROM inserted i
    JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
    WHERE t.AUTO_CREATE_SUBITEM = 'T';

    -- Call AUDITLOG stored procedure
    DECLARE @userId VARCHAR(15) = SYSTEM_USER;
    DECLARE @program VARCHAR(8) = 'trg_create_subitem';
    DECLARE @caseKey INT = NULL; -- Set appropriate value if available
    DECLARE @ecn INT;
    DECLARE @code INT = 1; -- Example code
    DECLARE @subcode INT = 1; -- Example subcode
    DECLARE @details VARCHAR(MAX);
    DECLARE @OSComputerName VARCHAR(50) = HOST_NAME();
    DECLARE @OSUserName VARCHAR(50) = SYSTEM_USER;
    DECLARE @OSAddress VARCHAR(50) = NULL; -- Set appropriate value if available
    DECLARE @BuildNumber VARCHAR(50) = NULL; -- Set appropriate value if available
    DECLARE @labCase VARCHAR(15) = NULL; -- Set appropriate value if available
    DECLARE @urn VARCHAR(30) = NULL; -- Set appropriate value if available

    SELECT @ecn = i.Evidence_Control_Number,
           @details = 'Subitem created for Evidence_Control_Number: ' + CAST(i.Evidence_Control_Number AS NVARCHAR(50))
    FROM inserted i
    JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
    WHERE t.AUTO_CREATE_SUBITEM = 'T';

    EXEC [dbo].[AUDITLOG]
        @userId,
        @program,
        @caseKey,
        @ecn,
        @code,
        @subcode,
        @details,
        @OSComputerName,
        @OSUserName,
        @OSAddress,
        @BuildNumber,
        @labCase,
        @urn;
END;