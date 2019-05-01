# Dapper.Azure.WebJobs.Extensions
Extension for Azure functions input/output bindings to natively use Dapper sql calls.  
[Azure Functions and Bindings Reference](https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings)

Azure functions can be triggered from various sources, most relevantly here Http requests.  As part of an Http request trigger, there can be a need to retrieve additional data from a database (additional input), and then also simply a need to update or create data in a database (output data).  By doing this declaratively in the bindings, the overhead of using Dapper is hidden into a simple set of statements.

This extension allows for a native binding to retrieve or set data automatically via Dapper to a SQL source.  This effectively allows the Azure function to be a remotely Http triggerable function with a declarative and simpler means of handling data.

## Installation
Dapper.Azure.WebJobs.Extensions is available via NuGet:

```
Install-Package Dapper.Azure.WebJobs.Extensions.SqlServer
```
or
```
dotnet add package Dapper.Azure.WebJobs.Extensions.SqlServer
```


## Using the binding
### C#

#### Configure the binding
```csharp
[assembly: WebJobsStartup(typeof(CustomerSamples.Startup))]
namespace CustomerSamples
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDapperSqlServer();
            builder.AddServiceBus();
        }
    }
}
```

#### Input binding sample with ServiceBus

##### select2.sql
```sql
UPDATE [Customers] SET Processed = @Processed where CustomerNumber in 
       (select top 5 CustomerNumber from [Customers] 
         WHERE Processed is null ORDER BY Updated ASC);
SELECT CustomerNumber, FirstName, LastName FROM [Customers] WHERE Processed = @Processed;
```

```csharp
[FunctionName("SelectCustomerSample5")]
public static void SelectCustomerSample5 ([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
                                    [Dapper(Sql = "select2.sql",
                                            SqlConnection = "SqlConnection",
                                            Parameters = "Processed:{datetime:yyyy-MM-dd HH:mm:ss}")] List<Customer> customers,
                                    [ServiceBus("myqueue", Connection = "myconnection")] ICollector<Customer> outputSbQueue,
                                    ILogger log)
{
    customers.ForEach(x=> outputSbQueue.Add(x));
}
```

#### Output binding samples with Http trigger

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
                                    [Dapper(Sql = "EXEC SpInsertCustomer @FirstName, @LastName", SqlConnection = "SqlConnection",
                                    CommandTimeout = 60,
                                    CommandType = CommandType.Text)] out IList<Customer> customers,
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
public static Customer SelectCustomerSample2([HttpTrigger] HttpRequestMessage req,
                                    [Dapper(Sql = "SELECT * FROM [dbo].[Customers] WHERE CustomerNumber = @CustomerNumber",
                                            SqlConnection = "SqlConnection", 
                                            Parameters = "CustomerNumber:{Query.CustomerNumber}",
                                            CommandTimeout = 60)
                                            ] Customer customer,
                                    ILogger log)
{
    return customer;
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
