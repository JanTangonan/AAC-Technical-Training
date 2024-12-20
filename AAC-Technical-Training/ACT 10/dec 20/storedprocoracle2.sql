CREATE OR REPLACE PROCEDURE ReverseCsvOrder(
    InputCsv   IN CLOB,
    ReversedCsv OUT CLOB
) AS
    TempReversedCsv CLOB;
BEGIN
    -- Initialize TempReversedCsv
    DBMS_LOB.CREATETEMPORARY(TempReversedCsv, TRUE);

    -- Retrieve rows from SplitCsv and reverse order
    FOR r IN (SELECT REGEXP_SUBSTR(COLUMN_VALUE, ',(.*)$', 1, 1) AS CriminalRecord
              FROM TABLE(SplitCsv(InputCsv, CHR(10)))
              ORDER BY TO_NUMBER(REGEXP_SUBSTR(COLUMN_VALUE, '^\d+')) DESC)
    LOOP
        IF DBMS_LOB.GETLENGTH(TempReversedCsv) > 0 THEN
            DBMS_LOB.WRITEAPPEND(TempReversedCsv, LENGTH(CHR(10)), CHR(10));
        END IF;

        DBMS_LOB.WRITEAPPEND(TempReversedCsv, LENGTH(r.CriminalRecord), r.CriminalRecord);
    END LOOP;

    -- Assign reversed CSV
    ReversedCsv := TempReversedCsv;

    -- Free temporary CLOB
    DBMS_LOB.FREETEMPORARY(TempReversedCsv);
END;
/
