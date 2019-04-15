# Dapper.Azure.WebJobs.Extensions
sql input/output binding for azure functions

## Using the binding
### C#

```csharp
[assembly: WebJobsStartup(typeof(CustomerSamples.Startup))]
namespace CustomerSamples
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDapperSqlServer();
        }
    }
}
```

#### Output binding samples

```csharp 
[FunctionName("InsertCustomerSample1")]
[return: Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")]
public static Customer Run([HttpTrigger] Customer customer, ILogger log)
{
   return customer;
}
 ```

```csharp 
[FunctionName("InsertCustomerSample2")]
[return: Dapper(Sql = "INSERT INTO [Customers] ([FirstName], [LastName]) VALUES (@FirstName, @LastName)",
                SqlConnection = "SqlConnection",
                IsolationLevel = IsolationLevel.ReadCommitted)]
public static Customer Run([HttpTrigger] Customer customer, ILogger log)
{
    return customer;
}
```
  
```csharp
[FunctionName("InsertCustomerSample3")]
public static  IList<Customer> Run([HttpTrigger] HttpRequestMessage req,
                                   [Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")] out IList<Customer> customers,
                                   ILogger log)
{
    customers = JsonConvert.DeserializeObject<IList<Customer>>(req.Content.ReadAsStringAsync().Result);
    return customers;
}
```

```csharp
[FunctionName("InsertCustomerSample4")]
public static  IList<Customer> InsertCustomerSample4([HttpTrigger] HttpRequestMessage req,
                                    [Dapper(Sql = "SpInsertCustomer", SqlConnection = "SqlConnection",
                                    CommandTimeout = 60,
                                    CommandType = CommandType.StoredProcedure)] out IList<Customer> customers,
                                    ILogger log)
{
    customers = JsonConvert.DeserializeObject<IList<Customer>>(req.Content.ReadAsStringAsync().Result);
    return customers;
}
```

#### Input binding samples

```csharp 
[FunctionName("SelectCustomerSample1")]
public static IList<Customer> Run([HttpTrigger] HttpRequestMessage req,
                                  [Dapper(Sql = "select.sql", SqlConnection = "SqlConnection", 
                                          Parameters = "FirstName:{Query.FirstName}",
                                          IsolationLevel = IsolationLevel.ReadCommitted)] IList<Customer> customers,
                                  ILogger log)
{
    return customers;
}
```
  
```csharp 
[FunctionName("SelectCustomerSample2")]
public static IList<Customer> Run([HttpTrigger] HttpRequestMessage req,
                                  [Dapper(Sql = "SELECT * FROM [dbo].[Customers] WHERE FirstName = @FirstName",
                                          SqlConnection = "SqlConnection", 
                                          Parameters = "FirstName:{Query.FirstName}")] IList<Customer> customers,
                                          ILogger log)
{
    return customers;
}
```
  
```csharp 
[FunctionName("SelectCustomerSample3")]
public static IList<Customer> Run([HttpTrigger] HttpRequestMessage req,
                                  [Dapper(Sql = "SELECT * FROM [dbo].[Customers]",
                                          SqlConnection = "SqlConnection")] IList<Customer> customers,
                                          ILogger log)
{
    return customers;
}
```

```csharp 
[FunctionName("SelectCustomerSample4")]
public static IList<Customer> SelectCustomerSample4dotnet ([HttpTrigger] HttpRequestMessage req,
                                    [Dapper(Sql = "SpGetCustomerByFirstname",
                                            SqlConnection = "SqlConnection",
                                            Parameters = "FirstName:{Query.FirstName}",
                                            CommandType = CommandType.StoredProcedure)] IList<Customer> customers,
                                    ILogger log)
{
    return customers;
}
```
