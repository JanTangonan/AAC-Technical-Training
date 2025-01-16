create or replace PROCEDURE AUDIT_LOG (
    userId IN VARCHAR2,
    program IN VARCHAR2,
    caseKey IN NUMBER,
    ecn IN NUMBER,
    code IN NUMBER,
    subcode IN NUMBER,
    details IN VARCHAR2,
    OSComputerName IN VARCHAR2,
    OSUserName IN VARCHAR2,
    OSAddress IN VARCHAR2,
    BuildNumber IN VARCHAR2,
    labCase IN VARCHAR2,
    urn IN VARCHAR2
) IS
BEGIN
    DECLARE
        logKey INT;
        currentDate DATE;
        vCaseKey INT;
        vEcn INT;
        vURN VARCHAR2(30);
        vLabCase VARCHAR2(15);
    BEGIN
        vURN := NVL(urn, '');
        vLabCase := NVL(labCase, '');
    
        vCaseKey := caseKey;
        IF (vCaseKey = 0) THEN
            vCaseKey := NULL;
        END IF;
        
        vEcn := ecn;
        IF (vEcn = 0) THEN
            vEcn := NULL;
        END IF;
        
        SELECT AUDITLOG_SEQ.NEXTVAL INTO logKey FROM DUAL;
        SELECT SYSDATE INTO currentDate FROM DUAL;
        
        INSERT INTO TV_AUDITLOG
            (LOG_STAMP, TIME_STAMP, USER_ID, PROGRAM, CASE_KEY, EVIDENCE_CONTROL_NUMBER, CODE, SUB_CODE, ERROR_LEVEL, ADDITIONAL_INFORMATION, OS_COMPUTER_NAME, OS_USER_NAME, OS_ADDRESS, BUILD_NUMBER, LAB_CASE, DEPARTMENT_CASE_NUMBER)
         Values
            (logKey, currentDate, userId, program, vCaseKey, vEcn, code, subcode, 0, details, OSComputerName, OSUserName, OSAddress, BuildNumber, vLabCase, vURN);
        
        EXCEPTION
        WHEN OTHERS THEN            
            ROLLBACK;
            RAISE;
    END;
END;
 