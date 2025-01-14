CREATE OR REPLACE TRIGGER trg_create_subitem
AFTER INSERT ON TV_LABITEM
FOR EACH ROW
BEGIN
    IF :NEW.AUTO_CREATE_SUBITEM = 'T' THEN
        INSERT INTO SUBITEM (PARENT_ITEM_ID, SUBITEM_DETAILS)
        VALUES (:NEW.ITEM_ID, 'Default Subitem Details');
    END IF;

    -- Insert audit log
    INSERT INTO AuditLog (LogTimestamp, LogMessage)
    VALUES (CURRENT_TIMESTAMP, 'Subitem created for ITEM_ID: ' || :NEW.ITEM_ID);
END;