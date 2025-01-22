create or replace TRIGGER "NJSPDEV"."LABITEM_CREATE_SUBITEM"
AFTER INSERT ON LABITEM
FOR EACH ROW
DECLARE
    PRAGMA AUTONOMOUS_TRANSACTION;    -- to handle mutation
    nextValue NUMBER;
    newLabItemNumber VARCHAR2(50);
BEGIN
    -- Check if Parent ECN is NULL to prevent recursion
    IF :NEW."Parent ECN" IS NULL THEN
        -- Check if AUTO_CREATE_SUBITEM flag is 'T'
        IF EXISTS (
            SELECT 1
            FROM ITEMTYPE t
            WHERE t."Item Type" = :NEW."Item Type"
              AND t."Auto Create Subitem" = 'T'
        ) THEN
            -- Get next LABITEM_SEQ value
            SELECT LABITEM_SEQ.NEXTVAL INTO nextValue FROM DUAL;

            -- Generate the new LAB_ITEM_NUMBER by incrementing the existing number by 0.1
            SELECT TO_CHAR(TO_NUMBER(:NEW."Lab Item Number") + 0.1, '9999999999D9') INTO newLabItemNumber FROM DUAL;

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
                nextValue, 
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

            -- Call AUDIT_LOG stored proc with selected parameters
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
    END IF;
END;