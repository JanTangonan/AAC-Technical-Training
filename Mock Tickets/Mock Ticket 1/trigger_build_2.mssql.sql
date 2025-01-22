ALTER TRIGGER [dbo].[LABITEM_CREATE_SUBITEM] 
ON [dbo].[TV_LABITEM] 
AFTER INSERT 
AS
BEGIN
    SET NOCOUNT ON;

    -- Get the next sequence value for the subitem
    DECLARE @nextValue INT;
    EXEC dbo.NEXTVAL 'LABITEM_SEQ', @nextValue OUTPUT;

    -- Create subitem into TV_LABITEM if AUTO_CREATE_SUBITEM is 'T'
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T'
    )
    BEGIN
        DECLARE @labItemNumberPrefix NVARCHAR(50);
        DECLARE @newLabItemNumber NVARCHAR(50);
        DECLARE @evidenceControlNumber INT;
        DECLARE @itemDescription NVARCHAR(255);
        DECLARE @itemType NVARCHAR(50);
        DECLARE @caseKey INT;
        DECLARE @packagingCode NVARCHAR(50);
        DECLARE @innerPack NVARCHAR(50);
        DECLARE @quantity INT;
        DECLARE @labCaseSubmission NVARCHAR(50);
        DECLARE @custodyOf NVARCHAR(50);
        DECLARE @location NVARCHAR(50);
        DECLARE @itemCategory NVARCHAR(50);
        DECLARE @collectedBy NVARCHAR(50);
        DECLARE @dateCollected DATE;
        DECLARE @timeCollected TIME;
        DECLARE @bookedBy NVARCHAR(50);
        DECLARE @bookingDate DATE;
        DECLARE @bookingTime TIME;
        DECLARE @recoveryLocation NVARCHAR(50);
        DECLARE @recoveryAddressKey NVARCHAR(50);
        DECLARE @processReviewDate DATE;
        DECLARE @processDate DATE;
        DECLARE @process NVARCHAR(50);
        DECLARE @seizedForBiology NVARCHAR(50);
        DECLARE @etrackInventoryId NVARCHAR(50);

        -- Get the values from the inserted row
        SELECT 
            @labItemNumberPrefix = i.LAB_ITEM_NUMBER,
            @evidenceControlNumber = i.EVIDENCE_CONTROL_NUMBER,
            @itemDescription = i.ITEM_DESCRIPTION,
            @itemType = i.ITEM_TYPE,
            @caseKey = i.CASE_KEY,
            @packagingCode = i.PACKAGING_CODE,
            @innerPack = i.INNER_PACK,
            @quantity = i.QUANTITY,
            @labCaseSubmission = i.LAB_CASE_SUBMISSION,
            @custodyOf = i.CUSTODY_OF,
            @location = i.LOCATION,
            @itemCategory = i.ITEM_CATEGORY,
            @collectedBy = i.COLLECTED_BY,
            @dateCollected = i.DATE_COLLECTED,
            @timeCollected = i.TIME_COLLECTED,
            @bookedBy = i.BOOKED_BY,
            @bookingDate = i.BOOKING_DATE,
            @bookingTime = i.BOOKING_TIME,
            @recoveryLocation = i.RECOVERY_LOCATION,
            @recoveryAddressKey = i.RECOVERY_ADDRESS_KEY,
            @processReviewDate = i.PROCESS_REVIEW_DATE,
            @processDate = i.PROCESS_DATE,
            @process = i.PROCESS,
            @seizedForBiology = i.SEIZED_FOR_BIOLOGY,
            @etrackInventoryId = i.ETRACK_INVENTORY_ID
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T';

        -- Generate the new LAB_ITEM_NUMBER by incrementing the existing number by 0.1
        SET @newLabItemNumber = CAST(CAST(@labItemNumberPrefix AS DECIMAL(10, 1)) + 0.1 AS NVARCHAR(50));

        -- Insert the sub-item
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
        VALUES (
            @nextValue, 
            @evidenceControlNumber, 
            'Subitem Details: ' + @itemDescription, 
            @itemType, 
            @caseKey,
            @packagingCode,
            @innerPack,
            @quantity,
            @labCaseSubmission,
            @custodyOf,
            @location,
            @newLabItemNumber,
            @itemCategory,
            @collectedBy,
            @dateCollected,
            @timeCollected,
            @bookedBy,
            @bookingDate,
            @bookingTime,
            @recoveryLocation,
            @recoveryAddressKey,
            @processReviewDate,
            @processDate,
            @process,
            @seizedForBiology,
            @etrackInventoryId
        );

        -- Call AUDITLOG stored procedure with selected parameters
        DECLARE 
            @userId VARCHAR(15) = 'ARVIN',
            @program VARCHAR(8) = 'TRGGR',
            @ecn INT,
            @details VARCHAR(MAX);

        -- Gather details for the audit log
        SELECT 
            @ecn = @evidenceControlNumber,
            @caseKey = @caseKey,
            @details = 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + 
                       CAST(@evidenceControlNumber AS NVARCHAR(50));

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
            NULL;                                  -- [DEPARTMENT_CASE_NUMBER];
    END
END;