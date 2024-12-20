create or replace FUNCTION SplitCsv(
    CsvInput   IN CLOB,
    Delimiter  IN VARCHAR2
) RETURN SYS.ODCIVARCHAR2LIST PIPELINED
AS
    CursorPosition    PLS_INTEGER := 1; -- Tracks the current position in the input
    DelimiterPosition PLS_INTEGER;      -- Tracks the position of the next delimiter
    SubString         VARCHAR2(4000);  -- Holds the extracted substring
BEGIN
    -- Ensure the delimiter is valid
    IF Delimiter IS NULL OR LENGTH(Delimiter) = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Delimiter must not be NULL or empty');
    END IF;

    -- Loop through the input CLOB, splitting by the delimiter
    LOOP
        DelimiterPosition := INSTR(CsvInput, Delimiter, CursorPosition); -- Find next delimiter
        
        -- If no delimiter is found, process the remaining string
        IF DelimiterPosition = 0 THEN
            SubString := SUBSTR(CsvInput, CursorPosition); -- Extract remaining part
            PIPE ROW (TRIM(SubString));                   -- Return the row
            EXIT;                                         -- Exit the loop
        END IF;

        -- Extract substring and trim it
        SubString := SUBSTR(CsvInput, CursorPosition, DelimiterPosition - CursorPosition);
        PIPE ROW (TRIM(SubString)); -- Return the row

        -- Move the cursor past the delimiter
        CursorPosition := DelimiterPosition + LENGTH(Delimiter);
    END LOOP;

    RETURN;
END;