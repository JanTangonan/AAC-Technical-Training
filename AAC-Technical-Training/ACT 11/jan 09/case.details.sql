SELECT 
 "LABITEM"."Lab Case Submission", 
 "LABITEM"."Lab Item Number", 
 "LABITEM"."Item Sort", 
 "LABITEM"."Item Description", 
 "LABITEM"."Item Type", 
 "LABITEM"."Packaging Code", 
 "DEPTNAME"."Department Name", 
 "LABCASE"."Department Case Number", 
 "OFFENSE"."Offense Description", 
 "LABCASE"."Lab Case", 
 "LABCASE"."Offense Date", "LABCASE"."Case Key"
FROM   
("NJSPDEV"."LABITEM" "LABITEM" 
INNER JOIN ("NJSPDEV"."DEPTNAME" "DEPTNAME" 
INNER JOIN "NJSPDEV"."LABCASE" "LABCASE" ON "DEPTNAME"."Department Code"="LABCASE"."Department Code") ON "LABITEM"."Case Key"="LABCASE"."Case Key") 
INNER JOIN "NJSPDEV"."OFFENSE" "OFFENSE" ON "LABCASE"."Offense Code"="OFFENSE"."Offense Code"
WHERE "LABITEM"."Case Key" = '1054332'