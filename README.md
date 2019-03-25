# Dapper sql binding for Azure functions
sql input/output binding for azure functions

## Using the binding

### C#
```csharp [FunctionName("InsertCustomerSample")]
  [return: Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")]
  public static async Task<Customer> Run([HttpTrigger] Customer customer, ILogger log)
  {
    return customer
  }
  ```
