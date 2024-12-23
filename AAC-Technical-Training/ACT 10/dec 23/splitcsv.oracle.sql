-- CSV SPLITTER FUNCTION
create or replace FUNCTION SplitCsv(
    CsvInput   IN CLOB,
    Delimiter  IN VARCHAR2
) RETURN SYS.ODCIVARCHAR2LIST PIPELINED
AS
    CursorPosition    PLS_INTEGER := 1; 
    DelimiterPosition PLS_INTEGER;      
    SubString         VARCHAR2(4000);  
BEGIN
    -- Ensure the delimiter is valid
    IF Delimiter IS NULL OR LENGTH(Delimiter) = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Delimiter must not be NULL or empty');
    END IF;

    -- Loop through the input, splitting by the delimiter
    LOOP
        DelimiterPosition := INSTR(CsvInput, Delimiter, CursorPosition);

        -- If no delimiter is found, get the remaining string
        IF DelimiterPosition = 0 THEN
            SubString := SUBSTR(CsvInput, CursorPosition);  
            PIPE ROW (TRIM(SubString));                    
            EXIT;
        END IF;

        -- Get substring and trim it
        SubString := SUBSTR(CsvInput, CursorPosition, DelimiterPosition - CursorPosition);
        PIPE ROW (TRIM(SubString));  

        -- Move the cursor past the delimiter
        CursorPosition := DelimiterPosition + LENGTH(Delimiter);
    END LOOP;

    RETURN;
END;