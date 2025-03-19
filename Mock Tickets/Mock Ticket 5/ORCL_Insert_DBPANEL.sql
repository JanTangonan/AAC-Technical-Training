-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT ALL
    INTO DBPANEL ("Table Name", "Field Name", "Prompt", "Length", "Code Table", "Sequence", "Panel Name") VALUES ('TV_ANALYST', 'ANALYST', 'Analyst', 30, 'TV_ANALYST', 1, 'ANALSEARCH')
    INTO DBPANEL ("Table Name", "Field Name", "Prompt", "Length", "Code Table", "Sequence", "Panel Name") VALUES ('TV_ANALYST', 'NAME', 'Name', 30, NULL, 2, 'ANALSEARCH')
    INTO DBPANEL ("Table Name", "Field Name", "Prompt", "Length", "Code Table", "Sequence", "Panel Name") VALUES ('TV_ANALYST', 'LAB_CODE', 'Lab Code', 30, 'TV_LABCTRL', 3, 'ANALSEARCH')
    INTO DBPANEL ("Table Name", "Field Name", "Prompt", "Length", "Code Table", "Sequence", "Panel Name") VALUES ('TV_ANALYST', 'CUSTODY_OF', 'Custody of', 30, NULL, 4, 'ANALSEARCH')
    INTO DBPANEL ("Table Name", "Field Name", "Prompt", "Length", "Code Table", "Sequence", "Panel Name") VALUES ('TV_ANALYST', 'GROUP_CODE', 'Group Code', 30, 'TV_GROUPS', 5, 'ANALSEARCH')
SELECT 1 FROM DUAL;

DELETE FROM DBPANEL
WHERE "Panel Name" = 'ANALSEARCH';

COMMIT;


