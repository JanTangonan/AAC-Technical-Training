CREATE OR REPLACE FUNCTION SplitCsv (
    CsvInput CLOB,
    Delimiter CHAR
) RETURN SYS.ODCIVARCHAR2LIST
IS
    Lines SYS.ODCIVARCHAR2LIST := SYS.ODCIVARCHAR2LIST(); -- Oracle collection type
    Pos PLS_INTEGER := 1;
    LineEnd PLS_INTEGER;
    Line VARCHAR2(4000);
    NormalizedInput CLOB;
BEGIN
    -- Normalize line endings to the specified delimiter
    NormalizedInput := REPLACE(CsvInput, CHR(13) || CHR(10), Delimiter);

    -- Split input into lines
    WHILE Pos <= LENGTH(NormalizedInput) LOOP
        LineEnd := INSTR(NormalizedInput, Delimiter, Pos);
        IF LineEnd = 0 THEN
            Line := SUBSTR(NormalizedInput, Pos);
            Pos := LENGTH(NormalizedInput) + 1;
        ELSE
            Line := SUBSTR(NormalizedInput, Pos, LineEnd - Pos);
            Pos := LineEnd + LENGTH(Delimiter);
        END IF;

        -- Add line to collection
        Lines.EXTEND;
        Lines(Lines.COUNT) := TRIM(Line);
    END LOOP;

    RETURN Lines;
END;
/
