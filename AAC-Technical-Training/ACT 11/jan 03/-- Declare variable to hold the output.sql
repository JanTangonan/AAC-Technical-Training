-- Declare variable to hold the output
DECLARE @ReversedCsv NVARCHAR(MAX);

-- Execute the stored procedure
EXEC dbo.ReverseCsvOrder 
    @CsvInput = 'jumbs,jum,jumba,Male,1996-12-12,28\njan ,arvin,tan,Male,2000-06-01,24',
    @CsvOutput = @ReversedCsv OUT;

-- View the results
SELECT @ReversedCsv AS ReversedOutput;