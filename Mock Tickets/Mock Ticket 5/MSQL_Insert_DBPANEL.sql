-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT INTO TV_DBPANEL (
    TABLE_NAME,
    FIELD_NAME,
    PROMPT,
    LENGTH,
    CODE_TABLE,
    SEQUENCE,
    PANEL_NAME
) VALUES 
    ('TV_ANALYST', 'ANALYST', 'Analyst', 30, 'TV_ANALYST', 1, 'ANALSEARCH'),
    ('TV_ANALYST', 'NAME', 'Name', 30, NULL, 2, 'ANALSEARCH'),
    ('TV_ANALYST', 'LAB_CODE', 'Lab Code', 30, 'TV_LABCTRL', 3, 'ANALSEARCH'),
    ('TV_ANALYST', 'CUSTODY_OF', 'Custody of', 30, NULL, 4, 'ANALSEARCH'),
    ('TV_ANALYST', 'GROUP_CODE', 'Group Code', 30, 'TV_GROUPS', 5, 'ANALSEARCH');

DELETE FROM TV_DBPANEL
WHERE PANEL_NAME = 'ANALSEARCH';