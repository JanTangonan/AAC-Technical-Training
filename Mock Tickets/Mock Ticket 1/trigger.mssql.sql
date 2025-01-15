CREATE TRIGGER trg_create_subitem
ON TV_LABITEM
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert sub-item into TV_LABITEM
    INSERT INTO TV_LABITEM (PARENT_ITEM_ID, ITEM_DETAILS, AUTO_CREATE_SUBITEM)
    SELECT ITEM_ID, 'Default Subitem Details', 'F'
    FROM inserted
    WHERE AUTO_CREATE_SUBITEM = 'T';

    -- Insert audit log
    INSERT INTO TV_AUDITLOG (LogTimestamp, LogMessage)
    SELECT CURRENT_TIMESTAMP, 'Subitem created for ITEM_ID: ' + CAST(ITEM_ID AS VARCHAR(50))
    FROM inserted
    WHERE AUTO_CREATE_SUBITEM = 'T';
END;