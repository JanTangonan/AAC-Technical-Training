-- #117446 - Please change the title of "Departments" tab to "Agencies".

INSERT INTO TV_TEXTRESOURCES (RESOURCE_ID, TEXT_DATA, RESOURCE_TYPE)
VALUES ('Config_Tabs_Departments', 'AGENCIES', 'Config Tab Name - DEPARTMENTS');


INSERT INTO TV_TEXTRESOURCES (RESOURCE_ID, TEXT_DATA, RESOURCE_TYPE)
VALUES 
('Config_Tab3Depts_btnDeptInfo', 'Agency Information', 'Tab Departments - Dept Info Button'),
('Config_Tab3Depts_DepartmentRolesButton', 'Agency Roles', 'Tab Departments - Department Roles Button'),
('Config_Tab3Depts_Label1', 'Find Agencies', 'Tab Departments - Label1'),
('Config_Tab3Depts_UploadToDepartmentButton', 'Upload to Agencies', 'Tab Departments - Upload to Department Button');

EXEC sp_help TV_TEXTRESOURCES;


update TV_TEXTRESOURCES
set TEXT_DATA = 'Upload to Agencies'
where RESOURCE_ID = 'Config_Tab3Depts_UploadToDepartmentButton'


-- #117446 - Please change the title of "Departments" tab to "Agencies".

INSERT INTO TV_TEXTRESOURCES (RESOURCE_ID, TEXT_DATA, RESOURCE_TYPE)
VALUES 
('ConfigTAB3Dept.lblActive', 'Find Agencies', 'Tab Departments - Label1'),
('ConfigTab3Dept.btnDeptInfo', 'Agency Information', 'Tab Departments - Dept Info Button'),
('ButtonPanel.DepartmentRoles', 'Agency Roles', 'Tab Departments - Department Roles Button'),
('ButtonPanel.UploadToDepartment', 'Upload to Agencies', 'Tab Departments - Upload to Department Button');

update TV_TEXTRESOURCES
set RESOURCE_ID = 'ConfigTab3Depts.btnDeptInfo1'
where RESOURCE_ID = 'ConfigTAB3Dept.btnDeptInfo'

select * from TV_TEXTRESOURCES
where RESOURCE_ID in ('ConfigTAB3Depts.lblActive1', 'ConfigTab3Depts.btnDeptInfo1', 'ButtonPanel.DepartmentRoles1', 'ButtonPanel.UploadToDepartment1')

select * from TV_TEXTRESOURCES
where RESOURCE_ID = 'ConfigTab3Depts.btnDeptInfo'