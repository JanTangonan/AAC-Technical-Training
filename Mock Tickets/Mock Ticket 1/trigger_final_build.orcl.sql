create or replace TRIGGER LABITEM_CREATE_SUBITEM
FOR INSERT ON LABITEM
COMPOUND TRIGGER

    -- Define record and table type to hold subitem and labstat data
    TYPE SubitemRecord IS RECORD (
        nextValueEvidenceControlNumber NUMBER,
        nextValueStatusKey NUMBER,
        nextValueBatchSequence NUMBER,
        newLabItemNumber VARCHAR2(50),
        parentECN NUMBER,
        itemDescription VARCHAR2(255),
        itemType VARCHAR2(50),
        caseKey NUMBER,
        packagingCode VARCHAR2(50),
        innerPack VARCHAR2(50),
        quantity NUMBER,
        labCaseSubmission VARCHAR2(50),
        custodyOf VARCHAR2(50),
        location VARCHAR2(50),
        itemCategory VARCHAR2(50),
        collectedBy VARCHAR2(50),
        dateCollected DATE,
        timeCollected DATE,
        bookedBy VARCHAR2(50),
        bookingDate DATE,
        bookingTime DATE,
        recoveryLocation VARCHAR2(255),
        recoveryAddressKey NUMBER,
        processReviewDate DATE,
        processDate DATE,
        process VARCHAR2(50),
        seizedForBiology VARCHAR2(50),
        eTrackInventoryID VARCHAR2(50),
        containerKey NUMBER,
        entryAnalyst VARCHAR2(50),
        entryTimeStamp DATE,
        departmentCode VARCHAR2(50)
    );

    TYPE SubitemTable IS TABLE OF SubitemRecord;
    subitems SubitemTable := SubitemTable();

    -- BEFORE STATEMENT: Initialize collection
    BEFORE STATEMENT IS
    BEGIN
        -- Clear collection to ensure it’s empty for this statement
        subitems.DELETE;
    END BEFORE STATEMENT;

    -- AFTER EACH ROW: Check the Auto Create Subitem flag and collect subitem details
    AFTER EACH ROW IS
        subitem SubitemRecord;
        autoCreateSubitemFlag CHAR(1);
    BEGIN
        -- Only proceed if item is not a subitem (Parent ECN is NULL)
        IF :NEW."Parent ECN" IS NULL THEN
            -- get AUTO_CREATE_SUBITEM_FLAG from TV_ITEMTYPE
            SELECT "Auto Create Subitem"
            INTO autoCreateSubitemFlag
            FROM ITEMTYPE
            WHERE "Item Type" = :NEW."Item Type";

            -- Check if the flag is set to T
            IF autoCreateSubitemFlag = 'T' THEN
                BEGIN
                    -- Get the next sequences value for the subitem
                    subitem.nextValueEvidenceControlNumber := LABITEM_SEQ.NEXTVAL;
                    subitem.nextValueStatusKey := LABSTAT_SEQ.NEXTVAL;
                    subitem.nextValueBatchSequence := BATCH_SEQ.NEXTVAL;
                    
                    -- Generate the new LAB_ITEM_NUMBER for the subitem by appending the existing number by 0.1 
                    subitem.newLabItemNumber := :NEW."Lab Item Number" || '.1';
                    
                    subitem.parentECN := :NEW."Evidence Control Number";
                    subitem.itemDescription := 'Subitem Details: ' || :NEW."Item Description";
                    subitem.itemType := :NEW."Item Type";
                    subitem.caseKey := :NEW."Case Key";
                    subitem.packagingCode := :NEW."Packaging Code";
                    subitem.innerPack := :NEW."Inner Pack";
                    subitem.quantity := :NEW."Quantity";
                    subitem.labCaseSubmission := :NEW."Lab Case Submission";
                    subitem.custodyOf := :NEW."Custody Of";
                    subitem.location := :NEW."Location";
                    subitem.itemCategory := :NEW."Item Category";
                    subitem.collectedBy := :NEW."Collected By";
                    subitem.dateCollected := :NEW."Date Collected";
                    subitem.timeCollected := :NEW."Time Collected";
                    subitem.bookedBy := :NEW."Booked By";
                    subitem.bookingDate := :NEW."Booking Date";
                    subitem.bookingTime := :NEW."Booking Time";
                    subitem.recoveryLocation := :NEW."Recovery Location";
                    subitem.recoveryAddressKey := :NEW."Recovery Address Key";
                    subitem.processReviewDate := :NEW."Process Review Date";
                    subitem.processDate := :NEW."Process Date";
                    subitem.process := :NEW."Process";
                    subitem.seizedForBiology := :NEW."Seized For Biology";
                    subitem.eTrackInventoryID := :NEW."ETrack Inventory ID";
                    subitem.containerKey := :NEW."Container Key";
                    subitem.entryAnalyst := :NEW."Entry Analyst";
                    subitem.entryTimeStamp := :NEW."Entry Time Stamp";
                    subitem.departmentCode := :NEW."Department Code";

                    -- Add the subitem and labstat details to the collection
                    subitems.EXTEND;
                    subitems(subitems.COUNT) := subitem;
                END;
            END IF;
        END IF;
    END AFTER EACH ROW;

    -- AFTER STATEMENT: Insert subitem and labstat records into LABITEM and LABSTAT
    AFTER STATEMENT IS
    BEGIN
        FOR idx IN 1 .. subitems.COUNT LOOP
            -- Insert the subitem into LABITEM
            INSERT INTO LABITEM (
                "Evidence Control Number", 
                "Parent ECN", 
                "Item Description", 
                "Item Type", 
                "Case Key",
                "Packaging Code",
                "Inner Pack",
                "Quantity",
                "Lab Case Submission",
                "Custody Of",
                "Location",
                "Lab Item Number",
                "Item Category",
                "Collected By",
                "Date Collected",
                "Time Collected",
                "Booked By",
                "Booking Date",
                "Booking Time",
                "Recovery Location",
                "Recovery Address Key",
                "Process Review Date",
                "Process Date",
                "Process",
                "Seized For Biology",
                "ETrack Inventory ID"
            )
            VALUES (
                subitems(idx).nextValueEvidenceControlNumber, 
                subitems(idx).parentECN, 
                subitems(idx).itemDescription, 
                subitems(idx).itemType, 
                subitems(idx).caseKey,
                subitems(idx).packagingCode,
                subitems(idx).innerPack,
                subitems(idx).quantity,
                subitems(idx).labCaseSubmission,
                subitems(idx).custodyOf,
                subitems(idx).location,
                subitems(idx).newLabItemNumber,
                subitems(idx).itemCategory,
                subitems(idx).collectedBy,
                subitems(idx).dateCollected,
                subitems(idx).timeCollected,
                subitems(idx).bookedBy,
                subitems(idx).bookingDate,
                subitems(idx).bookingTime,
                subitems(idx).recoveryLocation,
                subitems(idx).recoveryAddressKey,
                subitems(idx).processReviewDate,
                subitems(idx).processDate,
                subitems(idx).process,
                subitems(idx).seizedForBiology,
                subitems(idx).eTrackInventoryID
            );

            -- Insert the subitem custody into TV_LABSTAT
            INSERT INTO LABSTAT (
                "Status Key",
                "Case Key",
                "Evidence Control Number",
                "Status Date",
                "Status Time",
                "Entry Time",
                "Entered By",
                "Status Code",
                "Locker",
                "Batch Sequence",
                "Container Key",
                "Entry Analyst",
                "Entry Time Stamp",
                "Parent ECN",
                "Department Code"
            )
            VALUES (
                subitems(idx).nextValueStatusKey,
                subitems(idx).caseKey,
                subitems(idx).nextValueEvidenceControlNumber,
                TRUNC(SYSDATE),  
                SYSDATE, 
                TO_DATE(subitems(idx).entryTimeStamp, 'YYYY-MM-DD HH24:MI:SS'), 
                subitems(idx).entryAnalyst,
                subitems(idx).custodyOf,
                subitems(idx).location,
                subitems(idx).nextValueBatchSequence,
                subitems(idx).containerKey,
                subitems(idx).entryAnalyst,
                TO_DATE(subitems(idx).entryTimeStamp, 'YYYY-MM-DD HH24:MI:SS'),  
                subitems(idx).parentECN,
                subitems(idx).departmentCode
            );
            
            -- Audit log
            AUDIT_LOG(
                userId => 'ARVIN',
                program => 'TRGGR',
                caseKey => subitems(idx).caseKey,
                ecn => subitems(idx).nextValueEvidenceControlNumber,
                code => 1,
                subcode => 1,
                details => 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' || subitems(idx).nextValueEvidenceControlNumber,
                OSComputerName => '192.168.0.127',
                OSUserName => '',
                OSAddress => '192.168.0.127',
                BuildNumber => 'Master Build 1.1-16-2025-NET4.0.30319',
                labCase => NULL,
                urn => NULL
            );
        END LOOP;
    END AFTER STATEMENT;
END LABITEM_CREATE_SUBITEM;