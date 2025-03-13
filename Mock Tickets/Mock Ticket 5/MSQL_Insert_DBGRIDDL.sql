-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT INTO TV_DBGRIDDL (
    GRID_NAME,
    FIELD_NAME,
    FIELD_HEADER,
    FIELD_WIDTH,
    HIDE_FIELD,
    DISPLAY_ORDER
) VALUES 
    ('ANALSEARCH', 'ANALYST', 'Analyst', 100, 'F', 1),
    ('ANALSEARCH', 'NAME', 'Name', 100, 'F', 2),
    ('ANALSEARCH', 'LAB_CODE', 'Lab Code', 100, 'F', 3),
    ('ANALSEARCH', 'CUSTODY_OF', 'Custody Of', 100, 'F', 4),
    ('ANALSEARCH', 'GROUP_CODE', 'Group Code', 100, 'F', 5);

DELETE FROM TV_DBGRIDDL
WHERE GRID_NAME = 'ANALSEARCH';