-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT ALL
    INTO DBGRIDDL ("Grid Name", "Field Name", "Field Header", "Field Width", "Hide Field", "Display Order") VALUES ('ANALSEARCH', 'ANALYST', 'Analyst', 100, 'F', 1)
    INTO DBGRIDDL ("Grid Name", "Field Name", "Field Header", "Field Width", "Hide Field", "Display Order") VALUES ('ANALSEARCH', 'NAME', 'Name', 100, 'F', 2)
    INTO DBGRIDDL ("Grid Name", "Field Name", "Field Header", "Field Width", "Hide Field", "Display Order") VALUES ('ANALSEARCH', 'LAB_CODE', 'Lab Code', 100, 'F', 3)
    INTO DBGRIDDL ("Grid Name", "Field Name", "Field Header", "Field Width", "Hide Field", "Display Order") VALUES ('ANALSEARCH', 'CUSTODY_OF', 'Custody Of', 100, 'F', 4)
    INTO DBGRIDDL ("Grid Name", "Field Name", "Field Header", "Field Width", "Hide Field", "Display Order") VALUES ('ANALSEARCH', 'GROUP_CODE', 'Group Code', 100, 'F', 5)
SELECT 1 FROM DUAL;

DELETE FROM DBGRIDDL
WHERE "Grid Name" = 'ANALSEARCH';

COMMIT;