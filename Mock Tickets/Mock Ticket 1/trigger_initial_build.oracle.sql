create or replace TRIGGER "NJSPDEV"."LABITEM_CREATE_SUBITEM"
AFTER INSERT ON LABITEM
FOR EACH ROW
DECLARE
    PRAGMA AUTONOMOUS_TRANSACTION;    -- to handle mutation
    next_value NUMBER;
BEGIN
  -- check if item is a subitem to handle recursion of trigger
  IF :NEW."Parent ECN" IS NULL THEN
    -- Get next LABITEM_SEQ value
    SELECT LABITEM_SEQ.NEXTVAL INTO next_value FROM DUAL;

    -- Insert Subitem into LABITEM if AUTO_CREATE_SUBITEM flag is 'T'
    INSERT INTO LABITEM (
        "Evidence Control Number", 
        "Parent ECN", 
        "Item Description", 
        "Item Type", 
        "Case Key"
    )
    SELECT 
        next_value, 
        :NEW."Evidence Control Number", 
        'Subitem Details: ' || :NEW."Item Description", 
        :NEW."Item Type", 
        :NEW."Case Key"
    FROM ITEMTYPE t
    WHERE t."Item Type" = :NEW."Item Type"
      AND t."Auto Create Subitem" = 'T';

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
END;