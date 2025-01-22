ALTER TRIGGER [dbo].[LABITEM_CREATE_SUBITEM]
ON [dbo].[TV_LABITEM]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare variables
    DECLARE @AUTO_CREATE_SUBITEM_FLAG CHAR(1);
    DECLARE @nextValue INT;
    DECLARE @labItemNumberPrefix NVARCHAR(50);
    DECLARE @newLabItemNumber NVARCHAR(50);

    -- get AUTO_CREATE_SUBITEM_FLAG from TV_ITEMTYPE
    SELECT TOP 1
        @AUTO_CREATE_SUBITEM_FLAG = t.AUTO_CREATE_SUBITEM,
        @labItemNumberPrefix = i.LAB_ITEM_NUMBER
    FROM inserted i
    JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE;

    -- check if flag is set to T
    IF @AUTO_CREATE_SUBITEM_FLAG = 'T'
    BEGIN
        -- Get the next sequence value for the subitem
        EXEC dbo.NEXTVAL 'LABITEM_SEQ', @nextValue OUTPUT;

        -- Generate the new LAB_ITEM_NUMBER for the subitem by incrementing the existing number by 0.1 
        SET @newLabItemNumber = CAST(CAST(@labItemNumberPrefix AS DECIMAL(10, 1)) + 0.1 AS NVARCHAR(50));

        -- Insert the sub-item into TV_LABITEM
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
            @nextValue,
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
            @newLabItemNumber,
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
        FROM inserted i;

        -- Audit log
        DECLARE @auditDetails NVARCHAR(MAX) = 
            'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + CAST(@nextValue AS NVARCHAR(50));

        EXEC [dbo].[AUDITLOG]
            'ARVIN',                      -- [USER_ID]
            'TRGGR',                      -- [PROGRAM]
            NULL,                         -- [CASE_KEY]
            @nextValue,                   -- [EVIDENCE_CONTROL_NUMBER]
            1,                            -- [CODE]
            1,                            -- [SUB_CODE]
            @auditDetails,                -- [ADDITIONAL_INFORMATION]
            '192.168.0.127',              -- [OS_COMPUTER_NAME]
            '',                           -- [OS_USER_NAME]
            '192.168.0.127',              -- [OS_ADDRESS]
            'Master Build 1.1-16-2025-NET4.0.30319', -- [BUILD_NUMBER]
            NULL,                         -- [LAB_CASE]
            NULL;                         -- [DEPARTMENT_CASE_NUMBER]
    END
END;
