CREATE OR REPLACE TRIGGER LABITEM_CREATE_SUBITEM
FOR INSERT ON LABITEM
COMPOUND TRIGGER

    -- Variables to hold data across the trigger phases
    TYPE SubitemData IS RECORD (
        nextValueEvidenceControlNumber NUMBER,
        nextValueStatusKey NUMBER,
        nextValueBatchSequence NUMBER,
        newLabItemNumber VARCHAR2(50),
        autoCreateSubitemFlag CHAR(1)
    );

    data SubitemData;

    -- BEFORE STATEMENT: Fetch sequence values and prepare subitem data
    BEFORE STATEMENT IS
    BEGIN
        -- Fetch sequence values to avoid mutation error
        SELECT LABITEM_SEQ.NEXTVAL, LABSTAT_SEQ.NEXTVAL, BATCH_SEQ.NEXTVAL
        INTO data.nextValueEvidenceControlNumber, data.nextValueStatusKey, data.nextValueBatchSequence
        FROM DUAL;
    END BEFORE STATEMENT;

    -- BEFORE EACH ROW: Validate and set up subitem data if applicable
    BEFORE EACH ROW IS
    BEGIN
        -- Only proceed for parent items (Parent ECN is NULL)
        IF :NEW."Parent ECN" IS NULL THEN
            BEGIN
                -- Fetch AUTO_CREATE_SUBITEM_FLAG for the Item Type
                SELECT t."Auto Create Subitem"
                INTO data.autoCreateSubitemFlag
                FROM ITEMTYPE t
                WHERE t."Item Type" = :NEW."Item Type";

                -- If AUTO_CREATE_SUBITEM_FLAG is 'T', prepare new Lab Item Number
                IF data.autoCreateSubitemFlag = 'T' THEN
                    data.newLabItemNumber := :NEW."Lab Item Number" || '.1';
                END IF;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    -- No action needed if the Item Type doesn't exist or AUTO_CREATE_SUBITEM_FLAG isn't 'T'
                    data.autoCreateSubitemFlag := NULL;
            END;
        END IF;
    END BEFORE EACH ROW;

    -- AFTER EACH ROW: Insert subitem and corresponding LABSTAT record
    AFTER EACH ROW IS
    BEGIN
        -- Only proceed if AUTO_CREATE_SUBITEM_FLAG was set to 'T'
        IF data.autoCreateSubitemFlag = 'T' THEN
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
                data.nextValueEvidenceControlNumber, 
                :NEW."Evidence Control Number", 
                'Subitem Details: ' || :NEW."Item Description", 
                :NEW."Item Type", 
                :NEW."Case Key",
                :NEW."Packaging Code",
                :NEW."Inner Pack",
                :NEW."Quantity",
                :NEW."Lab Case Submission",
                :NEW."Custody Of",
                :NEW."Location",
                data.newLabItemNumber,
                :NEW."Item Category",
                :NEW."Collected By",
                :NEW."Date Collected",
                :NEW."Time Collected",
                :NEW."Booked By",
                :NEW."Booking Date",
                :NEW."Booking Time",
                :NEW."Recovery Location",
                :NEW."Recovery Address Key",
                :NEW."Process Review Date",
                :NEW."Process Date",
                :NEW."Process",
                :NEW."Seized For Biology",
                :NEW."ETrack Inventory ID"
            );

            -- Insert a corresponding record into LABSTAT
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
                data.nextValueStatusKey,
                :NEW."Case Key",
                data.nextValueEvidenceControlNumber,
                TRUNC(SYSDATE),
                TO_CHAR(SYSDATE, 'HH24:MI:SS'),
                :NEW."Entry Time Stamp",
                :NEW."Entry Analyst",
                :NEW."Custody Of",
                :NEW."Location",
                data.nextValueBatchSequence,
                :NEW."Container Key",
                :NEW."Entry Analyst",
                :NEW."Entry Time Stamp",
                :NEW."Evidence Control Number",
                :NEW."Department Code"
            );
            
            -- Audit log
            AUDIT_LOG(
                userId => 'ARVIN',
                program => 'TRGGR',
                caseKey => : NEW."Case Key",
                ecn => : NEW."Evidence Control Number",
                code => 1,
                subcode => 1,
                details => 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' || : NEW."Evidence Control Number",
                OSComputerName => '192.168.0.127',
                OSUserName => '',
                OSAddress => '192.168.0.127',
                BuildNumber => 'Master Build 1.1-16-2025-NET4.0.30319',
                labCase => NULL,
                urn => NULL
            );
        END IF;
    END AFTER EACH ROW;
END LABITEM_CREATE_SUBITEM;
