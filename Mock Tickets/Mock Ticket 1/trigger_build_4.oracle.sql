create or replace TRIGGER LABITEM_CREATE_SUBITEM
AFTER INSERT ON LABITEM
FOR EACH ROW
DECLARE
    PRAGMA AUTONOMOUS_TRANSACTION;    -- to handle mutation
    
    -- Declare variables
    nextValueEvidenceControlNumber NUMBER;
    nextValueStatusKey NUMBER;
    nextValueBatchSequence NUMBER;
    
    newLabItemNumber VARCHAR2(50);
    autoCreateSubitemFlag CHAR(1);
BEGIN
    -- Check if Parent ECN is NULL then this means this is a subitem, to prevent recursion
    IF :NEW."Parent ECN" IS NULL THEN
        BEGIN
            -- get AUTO_CREATE_SUBITEM_FLAG from TV_ITEMTYPE
            SELECT t."Auto Create Subitem"
            INTO autoCreateSubitemFlag
            FROM ITEMTYPE t
            WHERE t."Item Type" = :NEW."Item Type"
              AND t."Auto Create Subitem" = 'T';
              
            -- Check if AUTO_CREATE_SUBITEM flag is set to T
            IF autoCreateSubitemFlag = 'T' THEN
                -- Get the next sequence value for the subitem
                SELECT LABITEM_SEQ.NEXTVAL INTO nextValueEvidenceControlNumber FROM DUAL;
                SELECT LABSTAT_SEQ.NEXTVAL INTO nextValueStatusKey FROM DUAL;
                SELECT BATCH_SEQ.NEXTVAL INTO nextValueBatchSequence FROM DUAL;

                -- Generate the new LAB_ITEM_NUMBER for the subitem by appending the existing number by 0.1
                newLabItemNumber := :NEW."Lab Item Number" || '.1';

                -- Insert Subitem into LABITEM
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
                    nextValueEvidenceControlNumber, 
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
                    newLabItemNumber,
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
                    nextValueStatusKey,
                    :NEW."Case Key",
                    nextValueEvidenceControlNumber,
                    TRUNC(SYSDATE),
                    TO_CHAR(SYSDATE, 'HH24:MI:SS'),
                    :NEW."Entry Time Stamp",
                    :NEW."Entry Analyst",
                    :NEW."Custody Of",
                    :NEW."Location",
                    nextValueBatchSequence,
                    :NEW."Container Key",
                    :NEW."Entry Analyst",
                    :NEW."Entry Time Stamp",
                    :NEW."Parent ECN",
                    :NEW."Department Code"
                );
          
                -- Audit log
                AUDIT_LOG(
                    userId => 'ARVIN',
                    program => 'TRGGR',
                    caseKey => :NEW."Case Key",
                    ecn => :NEW."Evidence Control Number",
                    code => 1,
                    subcode => 1,
                    details => 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' || :NEW."Evidence Control Number",
                    OSComputerName => '192.168.0.127',
                    OSUserName => '',
                    OSAddress => '192.168.0.127',
                    BuildNumber => 'Master Build 1.1-16-2025-NET4.0.30319',
                    labCase => NULL,
                    urn => NULL
                );

                COMMIT;
            END IF;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                -- Do nothing if no matching ITEMTYPE is found
                NULL;
        END;
    END IF;
END;