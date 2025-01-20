CREATE OR REPLACE TRIGGER LABITEM_CREATE_SUBITEM
AFTER INSERT ON LABITEM
FOR EACH ROW
DECLARE
    next_value NUMBER;
BEGIN
    -- Get the next sequence value for LABITEM_SEQ
    SELECT LABITEM_SEQ.NEXTVAL INTO next_value FROM DUAL;

    -- Insert sub-item into LABITEM if AUTO_CREATE_SUBITEM is 'T'
    INSERT INTO LABITEM (
        EVIDENCE_CONTROL_NUMBER, 
        PARENT_ECN, 
        ITEM_DESCRIPTION, 
        ITEM_TYPE, 
        CASE_KEY
    )
    SELECT 
        next_value, 
        :NEW.EVIDENCE_CONTROL_NUMBER, 
        'Subitem Details: ' || :NEW.ITEM_DESCRIPTION, 
        :NEW.ITEM_TYPE, 
        :NEW.CASE_KEY
    FROM ITEMTYPE t
    WHERE t.ITEM_TYPE = :NEW.ITEM_TYPE
      AND t.AUTO_CREATE_SUBITEM = 'T';

    -- Call AUDIT_LOG stored procedure with selected parameters
    AUDIT_LOG(
        userId => 'ARVIN',
        program => 'TRGGR',
        caseKey => :NEW.CASE_KEY,
        ecn => :NEW.EVIDENCE_CONTROL_NUMBER,
        code => 1,
        subcode => 1,
        details => 'Subitem created for EVIDENCE_CONTROL_NUMBER: ' || :NEW.EVIDENCE_CONTROL_NUMBER,
        OSComputerName => '192.168.0.127',
        OSUserName => '',
        OSAddress => '192.168.0.127',
        BuildNumber => 'Master Build 1.1-16-2025-NET4.0.30319',
        labCase => NULL,
        urn => NULL
    );
END;