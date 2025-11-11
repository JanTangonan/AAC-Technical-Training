INSERT INTO YOUR_TABLE_NAME (
    REPORT_FORMAT,
    CUST_REPORT_KEY,
    CUST_REPORT_GROUP,
    CUST_REPORT_ORDER,
    REPORT_DESCRIPTION,
    REPORT_SOURCE,
    REPORT_DATA,
    PANEL_NAME,
    ACCESS_GROUPS,
    SELECTION_CRITERIA,
    ALLOW_XLS_EXPORT,
    ALLOW_PDF_EXPORT,
    UPLOADED_BY,
    DATE_UPLOADED,
    REPORT_HASH,
    ALLOW_XLS_DATA_EXPORT,
    ALLOW_CRW_VIEWER
)
VALUES (
    'SAMPLEREPORT.rpt',             -- REPORT_FORMAT
    636,                            -- CUST_REPORT_KEY
    'ADM',                          -- CUST_REPORT_GROUP
    0,                              -- CUST_REPORT_ORDER
    'Test CustomReport Page 2',     -- REPORT_DESCRIPTION
    'MANAGEMENT',                   -- REPORT_SOURCE
    NULL,                           -- REPORT_DATA
    '',                             -- PANEL_NAME
    'ADMIN',                        -- ACCESS_GROUPS
    'WORKLIST ID=NUMBER|{TV_WORKLIST.WORKLIST_ID}||T',  -- SELECTION_CRITERIA
    'T',                            -- ALLOW_XLS_EXPORT
    'T',                            -- ALLOW_PDF_EXPORT
    NULL,                           -- UPLOADED_BY
    SYSDATE,                        -- DATE_UPLOADED
    NULL,                           -- REPORT_HASH
    NULL,                           -- ALLOW_XLS_DATA_EXPORT
    NULL                            -- ALLOW_CRW_VIEWER
);
