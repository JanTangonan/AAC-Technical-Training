-- Ticket#6779 - PLCButtonPanel enhancement (Analyst search screen)

INSERT INTO AUTHCODE (
    "Authority Code",
    "Description",
    "User",
    "Tree"
) VALUES (
    'ANALSEARCH',
    'Analyst Search', 
    '0',
    'Program Access/PRELIMS/Web'
);

COMMIT;
-- 
DELETE FROM AUTHCODE
WHERE "Authority Code" = 'ANALSEARCH'
  AND "Description" = 'Analyst Search'
  AND "User" = '0'
  AND "Tree" = 'Program Access/PRELIMS/Web';

COMMIT;