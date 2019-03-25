# Dapper sql binding for Azure functions
sql input/output binding for azure functions

## Using the binding

### C#
Output binding samples

```csharp [FunctionName("InsertCustomerSample")]
  [return: Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")]
  public static async Task<Customer> Run([HttpTrigger] Customer customer, ILogger log)
  {
    return customer
  }
  ```

```csharp [FunctionName("InsertCustomerSample2")]
  [return: Dapper(Sql = "INSERT INTO [Customers] ([FirstName], [LastName]) VALUES (@FirstName, @LastName) ", 
                  SqlConnection = "SqlConnection")]
  public static async Task<Customer> Run([HttpTrigger] Customer customer, ILogger log)
  {
    return customer
  }
  ```
