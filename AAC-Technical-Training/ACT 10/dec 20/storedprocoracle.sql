CREATE OR REPLACE PROCEDURE ReverseCsvOrder (
    CsvInput IN CLOB,
    CsvOutput OUT CLOB
)
AS
    ParsedData SYS.ODCIVARCHAR2LIST;
    ReversedOutput CLOB := EMPTY_CLOB();
BEGIN
    -- Load the SplitCsv results into the collection
    SELECT COLUMN_VALUE
    BULK COLLECT INTO ParsedData
    FROM TABLE(SplitCsv(CsvInput, CHR(10))); -- Use Unix-style newline as delimiter

    -- Concatenate rows in reverse order
    FOR i IN REVERSE ParsedData.FIRST .. ParsedData.LAST LOOP
        IF i = ParsedData.FIRST THEN
            ReversedOutput := ParsedData(i);
        ELSE
            ReversedOutput := ReversedOutput || CHR(10) || ParsedData(i);
        END IF;
    END LOOP;

    -- Assign the reversed output
    CsvOutput := ReversedOutput;
END;
/
