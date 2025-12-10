

select * from TV_LABITEM
where CUSTODY_BY = 'MIKE'

select * from TV_LABCASE
where LAB_CASE = 'S22-00524'
-- 450989

select * from TV_LABCASE
where CASE_KEY = 450989
where CASE_KEY = 457704

select * from TV_LABASSIGN
where CASE_KEY = 457704

select * from TV_LABASSIGN
where EXAM_KEY in (614709,614715,614716,614837)
-- 457704

select * from TV_EXAMCODE
select * from TV_EXAMITEM

select * from TV_REVLIST

update TV_REVLIST
set SECTION = 'BIO'
where REVIEW_CODE = 'APPROVE'
and SECTION = 'TOX'