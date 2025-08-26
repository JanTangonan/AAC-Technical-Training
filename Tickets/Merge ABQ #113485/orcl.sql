SELECT DISTINCT
    A.SCHEDULE_KEY,
    A.CASE_KEY,
    TO_CHAR(A.DATE_RES, 'MM/DD/YYYY') AS DATE_RES,
    TO_CHAR(A.TIME, 'HH24:MI:SS') AS TIME,
    B.DESCRIPTION,
    C.NAME,
    A.COMMENTS,
    CASE 
        WHEN A.EVIDENCE_CONTROL_NUMBER IS NULL THEN 
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM TV_SCHDETL S
                    WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                ) THEN
                    COALESCE(
                        (
                            SELECT LISTAGG(LI2.LAB_ITEM_NUMBER, ', ') 
                                   WITHIN GROUP (ORDER BY LI2.LAB_ITEM_NUMBER)
                            FROM TV_SCHDETL SD
                            LEFT JOIN TV_LABITEM LI2 
                                   ON LI2.EVIDENCE_CONTROL_NUMBER = SD.EVIDENCE_CONTROL_NUMBER
                            WHERE SD.SCHEDULE_KEY = A.SCHEDULE_KEY
                        ),
                        'Deleted'
                    )
                ELSE 'Not Item Related'
            END
        ELSE 
            COALESCE(I.LAB_ITEM_NUMBER, 'Deleted')
    END AS ITEM_NUMBER,
    E.DESCRIPTION AS SECTION_DESCRIPTION
FROM TV_SCHEDULE A
    LEFT JOIN TV_SCHTYPE B ON B.TYPE_RES = A.TYPE_RES
    LEFT JOIN TV_ANALYST C ON C.ANALYST = A.ANALYST
    LEFT JOIN TV_LABITEM I ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER
    LEFT JOIN TV_EXAMCODE E ON E.EXAM_CODE = A.SECTION
    LEFT JOIN TV_SCHDETL D ON D.SCHEDULE_KEY = A.SCHEDULE_KEY
WHERE A.CASE_KEY = 1052447
ORDER BY DATE_RES DESC, TIME DESC;
