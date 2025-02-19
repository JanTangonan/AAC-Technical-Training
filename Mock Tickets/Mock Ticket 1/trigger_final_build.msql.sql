USE [VENTURA_DEV]
GO
/****** Object:  Trigger [dbo].[LABITEM_CREATE_SUBITEM]    Script Date: 2/19/2025 9:16:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER TRIGGER [dbo].[LABITEM_CREATE_SUBITEM]
ON [dbo].[TV_LABITEM]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare variables
    DECLARE @autoCreateSubitemFlag VARCHAR(1);

    DECLARE @nextValueEvidenceControlNumber INT;
    DECLARE @nextValueStatusKey INT;
	DECLARE @nextValueBatchSequence INT;

    DECLARE @newLabItemNumber VARCHAR(50);

    -- get AUTO_CREATE_SUBITEM_FLAG from TV_ITEMTYPE
    SELECT @autoCreateSubitemFlag = AUTO_CREATE_SUBITEM
    FROM TV_ITEMTYPE 
    WHERE ITEM_TYPE = (SELECT ITEM_TYPE FROM inserted);

    -- check if @AUTO_CREATE_SUBITEM_FLAG is set to T
    IF @autoCreateSubitemFlag = 'T'
    BEGIN
        -- Get the next sequences value for the subitem
        EXEC dbo.NEXTVAL 'LABITEM_SEQ', @nextValueEvidenceControlNumber OUTPUT;
		EXEC dbo.NEXTVAL 'LABSTAT_SEQ', @nextValueStatusKey OUTPUT;
		EXEC dbo.NEXTVAL 'BATCH_SEQ', @nextValueBatchSequence OUTPUT;

        -- Generate the new LAB_ITEM_NUMBER for the subitem by appending the existing number by 0.1 
         SET @newLabItemNumber = (SELECT LAB_ITEM_NUMBER FROM inserted) + '.1';

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
            ETRACK_INVENTORY_ID,
			DEPARTMENT_ITEM_NUMBER
        )
        SELECT 
            @nextValueEvidenceControlNumber,
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
            i.ETRACK_INVENTORY_ID,
			i.DEPARTMENT_ITEM_NUMBER
        FROM inserted i;

		-- Insert the sub-item custody into TV_LABSTAT
		INSERT INTO TV_LABSTAT (
			STATUS_KEY,
			CASE_KEY,
			EVIDENCE_CONTROL_NUMBER,
			STATUS_DATE,
			STATUS_TIME,
			ENTRY_TIME,
			ENTERED_BY,
			STATUS_CODE,
			LOCKER,
			BATCH_SEQUENCE,
			CONTAINER_KEY,
			ENTRY_ANALYST,
			ENTRY_TIME_STAMP,
			PARENT_ECN,
			DEPARTMENT_CODE
		)
		SELECT
			@nextValueStatusKey,
			i.CASE_KEY,
			@nextValueEvidenceControlNumber,
			CAST(GETDATE() AS DATE),
            CONVERT(VARCHAR(8), GETDATE(), 108),
			i.ENTRY_TIME_STAMP,
			i.ENTRY_ANALYST,
			i.CUSTODY_OF,
			i.LOCATION,
			@nextValueBatchSequence,
			i.CONTAINER_KEY,
			i.ENTRY_ANALYST,
			i.ENTRY_TIME_STAMP,
			i.PARENT_ECN,
			i.DEPARTMENT_CODE
		FROM inserted i;

        -- Audit log
        DECLARE @auditDetails VARCHAR(50) = 
            'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + CAST(@nextValueEvidenceControlNumber AS VARCHAR(20));

        EXEC [dbo].[AUDITLOG]
            'ARVIN',                      -- [USER_ID]
            'TRGGR',                      -- [PROGRAM]
            NULL,                         -- [CASE_KEY]
            @nextValueEvidenceControlNumber, -- [EVIDENCE_CONTROL_NUMBER]
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
