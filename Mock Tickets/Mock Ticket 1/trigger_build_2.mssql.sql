ALTER TRIGGER [dbo].[LABITEM_CREATE_SUBITEM] 
ON [dbo].[TV_LABITEM] 
AFTER INSERT 
AS
BEGIN
    SET NOCOUNT ON;

    -- Get the next sequence value for the subitem
    DECLARE @next_value INT;
    EXEC dbo.NEXTVAL 'LABITEM_SEQ', @next_value OUTPUT;

    -- Create subitem into TV_LABITEM if AUTO_CREATE_SUBITEM flag is set to 'T'
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T'
    )
    BEGIN
        INSERT INTO TV_LABITEM (
            EVIDENCE_CONTROL_NUMBER, 
            PARENT_ECN, 
            ITEM_DESCRIPTION, 
            ITEM_TYPE, 
            CASE_KEY,
            PACKAGING_CODE,
            INNER_PACK,
            QUANTITY,
            LAB_CASE_SUBMISSION,
            CUSTODY_OF,
            LOCATION,
            LAB_ITEM_NUMBER,
            ITEM_CATEGORY,
            COLLECTED_BY,
            DATE_COLLECTED,
            TIME_COLLECTED,
            BOOKED_BY,
            BOOKING_DATE,
            BOOKING_TIME,
            RECOVERY_LOCATION,
            RECOVERY_ADDRESS_KEY,
            PROCESS_REVIEW_DATE,
            PROCESS_DATE,
            PROCESS,
            SEIZED_FOR_BIOLOGY,
            ETRACK_INVENTORY_ID
        )
        SELECT 
            @next_value, 
            i.EVIDENCE_CONTROL_NUMBER, 
            'Subitem Details: ' + i.ITEM_DESCRIPTION, 
            i.ITEM_TYPE, 
            i.CASE_KEY,
            i.PACKAGING_CODE,
            i.INNER_PACK,
            i.QUANTITY,
            i.LAB_CASE_SUBMISSION,
            i.CUSTODY_OF,
            i.LOCATION,
            i.LAB_ITEM_NUMBER,
            i.ITEM_CATEGORY,
            i.COLLECTED_BY,
            i.DATE_COLLECTED,
            i.TIME_COLLECTED,
            i.BOOKED_BY,
            i.BOOKING_DATE,
            i.BOOKING_TIME,
            i.RECOVERY_LOCATION,
            i.RECOVERY_ADDRESS_KEY,
            i.PROCESS_REVIEW_DATE,
            i.PROCESS_DATE,
            i.PROCESS,
            i.SEIZED_FOR_BIOLOGY,
            i.ETRACK_INVENTORY_ID
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T';

        -- Call AUDITLOG stored procedure with selected parameters
        DECLARE 
            @userId VARCHAR(15) = 'ARVIN',
            @program VARCHAR(8) = 'TRGGR',
            @caseKey INT,
            @ecn INT,
            @details VARCHAR(MAX);

        -- Gather details for the audit log
        SELECT 
            @ecn = i.EVIDENCE_CONTROL_NUMBER,
            @caseKey = i.CASE_KEY,
            @details = 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + 
                    CAST(i.EVIDENCE_CONTROL_NUMBER AS NVARCHAR(50))
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T';

        -- Insert into audit log
        EXEC [dbo].[AUDITLOG]
            @userId,                               -- [USER_ID]
            @program,                              -- [PROGRAM]
            @caseKey,                              -- [CASE_KEY]
            @ecn,                                  -- [EVIDENCE_CONTROL_NUMBER]
            1,                                     -- [CODE]
            1,                                     -- [SUB_CODE]
            @details,                              -- [ADDITIONAL_INFORMATION]
            '192.168.0.127',                       -- [OS_COMPUTER_NAME]
            '',                                    -- [OS_USER_NAME]
            '192.168.0.127',                       -- [OS_ADDRESS]
            'Master Build 1.1-16-2025-NET4.0.30319', -- [BUILD_NUMBER]
            NULL,                                  -- [LAB_CASE]
            NULL;                                  -- [DEPARTMENT_CASE_NUMBER]
    END;
END;