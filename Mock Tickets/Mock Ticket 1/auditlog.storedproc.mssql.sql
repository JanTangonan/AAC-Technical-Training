USE [VENTURA_DEV]
GO
/****** Object:  StoredProcedure [dbo].[AUDITLOG]    Script Date: 1/15/2025 3:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[AUDITLOG]
	@userId VARCHAR(15),
	@program VARCHAR(8),
	@caseKey INT,
	@ecn INT,
	@code INT,
	@subcode INT,
	@details VARCHAR(MAX),
	@OSComputerName VARCHAR(50),
	@OSUserName VARCHAR(50),
	@OSAddress VARCHAR(50),
	@BuildNumber VARCHAR(50),
	@labCase VARCHAR(15),
	@urn VARCHAR(30)  
AS
	DECLARE @logKey INT
BEGIN
	BEGIN TRANSACTION

	BEGIN TRY
		EXEC [dbo].[NEXTVAL] 'AUDITLOG_SEQ', @logKey OUT
		
		INSERT INTO [dbo].[TV_AUDITLOG]
			   ([LOG_STAMP]
			   ,[TIME_STAMP]
			   ,[USER_ID]
			   ,[PROGRAM]
			   ,[CASE_KEY]
			   ,[EVIDENCE_CONTROL_NUMBER]
			   ,[CODE]
			   ,[SUB_CODE]
			   ,[ERROR_LEVEL]
			   ,[ADDITIONAL_INFORMATION]
			   ,[OS_COMPUTER_NAME]
			   ,[OS_USER_NAME]
			   ,[OS_ADDRESS]
			   ,[BUILD_NUMBER]
			   ,[LAB_CASE]
			   ,[DEPARTMENT_CASE_NUMBER])
		 VALUES
			   (@logKey
			   ,GETDATE()
			   ,@userId
			   ,@program
			   ,@caseKey
			   ,@ecn
			   ,@code
			   ,@subcode
			   ,0
			   ,@details
			   ,@OSComputerName
			   ,@OSUserName
			   ,@OSAddress
			   ,@BuildNumber
			   ,@labCase
			   ,@urn)
	    
		COMMIT TRANSACTION
		RETURN ''
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION
		
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT @ErrorMessage = ERROR_MESSAGE(),
			   @ErrorSeverity = ERROR_SEVERITY(),
			   @ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
		RETURN
	END CATCH
END


