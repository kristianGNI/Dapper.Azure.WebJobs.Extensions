UPDATE [Customers] SET Processed = @Processed where CustomerNumber in (select top 5 CustomerNumber from [Customers] WHERE Processed is null ORDER BY Updated ASC)
SELECT CustomerNumber, FirstName, LastName FROM [Customers] WHERE Processed = @Processed;
