USE [VENTURA_DEV]
GO
/****** Object:  Trigger [dbo].[LABITEM_CREATE_SUBITEM]    Script Date: 1/22/2025 10:20:39 AM ******/
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

    -- Get the next sequence value for the subitem
    DECLARE @next_value INT;
    EXEC dbo.NEXTVAL 'LABITEM_SEQ', @next_value OUTPUT;

    -- Create subitem into TV_LABITEM if AUTO_CREATE_SUBITEM is 'T'
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T'
    )
    BEGIN
        DECLARE @lab_item_number_prefix NVARCHAR(50);
        DECLARE @new_lab_item_number NVARCHAR(50);
        DECLARE @evidence_control_number INT;
        DECLARE @item_description NVARCHAR(255);
        DECLARE @item_type NVARCHAR(50);
        DECLARE @case_key INT;
        DECLARE @packaging_code NVARCHAR(50);
        DECLARE @inner_pack NVARCHAR(50);
        DECLARE @quantity INT;
        DECLARE @lab_case_submission NVARCHAR(50);
        DECLARE @custody_of NVARCHAR(50);
        DECLARE @location NVARCHAR(50);
        DECLARE @item_category NVARCHAR(50);
        DECLARE @collected_by NVARCHAR(50);
        DECLARE @date_collected DATE;
        DECLARE @time_collected TIME;
        DECLARE @booked_by NVARCHAR(50);
        DECLARE @booking_date DATE;
        DECLARE @booking_time TIME;
        DECLARE @recovery_location NVARCHAR(50);
        DECLARE @recovery_address_key NVARCHAR(50);
        DECLARE @process_review_date DATE;
        DECLARE @process_date DATE;
        DECLARE @process NVARCHAR(50);
        DECLARE @seized_for_biology NVARCHAR(50);
        DECLARE @etrack_inventory_id NVARCHAR(50);

        -- Get the values from the inserted row
        SELECT 
            @lab_item_number_prefix = i.LAB_ITEM_NUMBER,
            @evidence_control_number = i.EVIDENCE_CONTROL_NUMBER,
            @item_description = i.ITEM_DESCRIPTION,
            @item_type = i.ITEM_TYPE,
            @case_key = i.CASE_KEY,
            @packaging_code = i.PACKAGING_CODE,
            @inner_pack = i.INNER_PACK,
            @quantity = i.QUANTITY,
            @lab_case_submission = i.LAB_CASE_SUBMISSION,
            @custody_of = i.CUSTODY_OF,
            @location = i.LOCATION,
            @item_category = i.ITEM_CATEGORY,
            @collected_by = i.COLLECTED_BY,
            @date_collected = i.DATE_COLLECTED,
            @time_collected = i.TIME_COLLECTED,
            @booked_by = i.BOOKED_BY,
            @booking_date = i.BOOKING_DATE,
            @booking_time = i.BOOKING_TIME,
            @recovery_location = i.RECOVERY_LOCATION,
            @recovery_address_key = i.RECOVERY_ADDRESS_KEY,
            @process_review_date = i.PROCESS_REVIEW_DATE,
            @process_date = i.PROCESS_DATE,
            @process = i.PROCESS,
            @seized_for_biology = i.SEIZED_FOR_BIOLOGY,
            @etrack_inventory_id = i.ETRACK_INVENTORY_ID
        FROM inserted i
        JOIN TV_ITEMTYPE t ON i.ITEM_TYPE = t.ITEM_TYPE
        WHERE t.AUTO_CREATE_SUBITEM = 'T';

        -- Generate the new LAB_ITEM_NUMBER using the function
        SET @new_lab_item_number = CAST(CAST(@lab_item_number_prefix AS DECIMAL(10, 1)) + 0.1 AS NVARCHAR(50));

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
            @next_value, 
            @evidence_control_number, 
            'Subitem Details: ' + @item_description, 
            @item_type, 
            @case_key,
            @packaging_code,
            @inner_pack,
            @quantity,
            @lab_case_submission,
            @custody_of,
            @location,
            @new_lab_item_number,
            @item_category,
            @collected_by,
            @date_collected,
            @time_collected,
            @booked_by,
            @booking_date,
            @booking_time,
            @recovery_location,
            @recovery_address_key,
            @process_review_date,
            @process_date,
            @process,
            @seized_for_biology,
            @etrack_inventory_id
        );

        -- Call AUDITLOG stored procedure with selected parameters
        DECLARE 
            @userId VARCHAR(15) = 'ARVIN',
            @program VARCHAR(8) = 'TRGGR',
            @caseKey INT,
            @ecn INT,
            @details VARCHAR(MAX);

        -- Gather details for the audit log
        SELECT 
            @ecn = @evidence_control_number,
            @caseKey = @case_key,
            @details = 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' + 
                       CAST(@evidence_control_number AS NVARCHAR(50));

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
