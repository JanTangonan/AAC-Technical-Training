-- #117446 - Please change the title of "Departments" tab to "Agencies".

INSERT ALL
    INTO TEXTRESOURCES ("Resource ID", "Text Data", "Resource Type")
        VALUES ('ConfigTAB3Depts.lblActive', 'Find Agency', 'Tab Departments - Label')
    INTO TEXTRESOURCES ("Resource ID", "Text Data", "Resource Type")
        VALUES ('ConfigTab3Depts.btnDeptInfo', 'Agency Information', 'Tab Departments - Dept Info Button')
    INTO TEXTRESOURCES ("Resource ID", "Text Data", "Resource Type")
        VALUES ('ButtonPanel.Department_Roles', 'Agency Roles', 'Tab Departments - Department Roles Button')
    INTO TEXTRESOURCES ("Resource ID", "Text Data", "Resource Type")
        VALUES ('ButtonPanel.Upload_To_Departments', 'Upload to Agencies', 'Tab Departments - Upload to Department Button')
    INTO TEXTRESOURCES ("Resource ID", "Text Data", "Resource Type")
        VALUES ('ButtonPanel.Dept_Picture_Mask', 'Agency Picture Mask', 'Tab Departments - Dept Picture Mask')
SELECT 1 FROM DUAL;
