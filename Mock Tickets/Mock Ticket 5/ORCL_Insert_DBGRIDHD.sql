-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT INTO DBGRIDHD (
    "Grid Name",
    "SQL String",
    "Description",
    "Grid Width",
    "Grid Height",
    "Records per Page"
) VALUES (
    'ANALSEARCH',
    'SELECT * FROM TV_ANALYST A ORDER BY A.NAME',
    'Analyst Search Result',
    1000,
    300,
    10
);

DELETE FROM DBGRIDHD
WHERE "Grid Name" = 'ANALSEARCH';

COMMIT;