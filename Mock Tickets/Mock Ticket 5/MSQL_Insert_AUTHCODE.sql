-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT INTO TV_AUTHCODE (
    AUTHORITY_CODE,
    DESCRIPTION,
    USER_RES,
    TREE
) VALUES (
    'ANALSEARCH', 
    'Analyst Search', 
    '0',
    'Program Access/PRELIMS/Web'
);

-- 
DELETE FROM TV_AUTHCODE
WHERE AUTHORITY_CODE = 'ANALSEARCH'
  AND DESCRIPTION = 'Analyst Search'
  AND USER_RES = '0'
  AND TREE = 'Program Access/PRELIMS/Web';