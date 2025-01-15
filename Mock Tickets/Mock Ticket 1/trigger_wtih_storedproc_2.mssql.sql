CREATE TRIGGER [dbo].[LABITEM_CREATE_SUBITEM] ON [dbo].[TV_LABITEM] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    -- Insert sub-item into TV_LABITEM if AUTO_CREATE_SUBITEM is 'T'
    INSERT INTO TV_LABITEM (PARENT_ECN, ITEM_DESCRIPTION, ITEM_TYPE)
    SELECT i.EVIDENCE_CONTROL_NUMBER, 'Subitem Details: ' + i.ITEM_DESCRIPTION, i.ITEM_TYPE
    FROM inserted i
    JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
    WHERE t.AUTO_CREATE_SUBITEM = 'T';

    -- Call AUDITLOG stored procedure with selected parameters
    DECLARE @userId VARCHAR(15) = SYSTEM_USER;
    DECLARE @program VARCHAR(8) = 'LABITEM_CREATE_SUBITEM';
    DECLARE @ecn INT;
    DECLARE @details VARCHAR(MAX);

    SELECT @ecn = i.EVIDENCE_CONTROL_NUMBER,
           @details = 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + CAST(i.EVIDENCE_CONTROL_NUMBER AS NVARCHAR(50))
    FROM inserted i
    JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
    WHERE t.AUTO_CREATE_SUBITEM = 'T';

    EXEC [dbo].[AUDITLOG]
        @userId,
        @program,
        NULL, -- @caseKey
        @ecn,
        1, -- @code
        1, -- @subcode
        @details,
        NULL, -- @OSComputerName
        NULL, -- @OSUserName
        NULL, -- @OSAddress
        NULL, -- @BuildNumber
        NULL, -- @labCase
        NULL; -- @urn
END;