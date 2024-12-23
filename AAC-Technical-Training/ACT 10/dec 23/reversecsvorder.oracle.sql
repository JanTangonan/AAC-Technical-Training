-- REVERSE CSV ORDER STORED PROCEDURE
create or replace PROCEDURE ReverseCsvOrder (
    CsvInput IN CLOB,
    CsvOutput OUT CLOB
)
AS
    ParsedData SYS.ODCIVARCHAR2LIST;
    ReversedOutput CLOB;
    TempRow VARCHAR2(4000);
BEGIN
    DBMS_LOB.CREATETEMPORARY(ReversedOutput, TRUE);

    -- Split CsvInput
    SELECT COLUMN_VALUE
    BULK COLLECT INTO ParsedData
    FROM TABLE(SplitCsv(CsvInput, CHR(10)));  

    -- Debug
    DBMS_OUTPUT.PUT_LINE('Parsed Data:');
    FOR i IN ParsedData.FIRST .. ParsedData.LAST LOOP
        DBMS_OUTPUT.PUT_LINE(ParsedData(i));
    END LOOP;

    -- Concatenate rows in reverse order
    FOR i IN REVERSE ParsedData.FIRST .. ParsedData.LAST LOOP
        TempRow := ParsedData(i);
        DBMS_OUTPUT.PUT_LINE('Processing row: ' || TempRow);

        IF DBMS_LOB.GETLENGTH(ReversedOutput) = 0 THEN
            DBMS_LOB.WRITEAPPEND(ReversedOutput, LENGTH(TempRow), TempRow);
        ELSE
            DBMS_LOB.WRITEAPPEND(ReversedOutput, 1, CHR(10)); 
            DBMS_LOB.WRITEAPPEND(ReversedOutput, LENGTH(TempRow), TempRow);
        END IF;
    END LOOP;

    CsvOutput := ReversedOutput;

    DBMS_OUTPUT.PUT_LINE('Reversed Output:');
    DBMS_OUTPUT.PUT_LINE(CsvOutput);

    INSERT INTO CsvOutputTable (OutputCLOB) VALUES (CsvOutput);

    DBMS_LOB.FREETEMPORARY(ReversedOutput);
END;