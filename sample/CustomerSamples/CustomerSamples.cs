using Dapper.Azure.WebJobs.Extensions.SqlServer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Data;
using System;
using System.Linq;

namespace Samples
{
    public static class CustomerSamples
    {
        [FunctionName("InsertCustomerSample1")]
        [return: Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")]
        public static Customer InsertCustomerSample1([HttpTrigger] Customer customer, ILogger log)
        {
            return customer;
        }

        [FunctionName("InsertCustomerSample2")]
        [return: Dapper(Sql = "INSERT INTO [Customers] ([FirstName], [LastName]) VALUES (@FirstName, @LastName)",
                        SqlConnection = "SqlConnection",
                        IsolationLevel = IsolationLevel.ReadCommitted)]
        public static Customer InsertCustomerSample2([HttpTrigger] Customer customer, ILogger log)
        {
           return customer;
        }

        [FunctionName("InsertCustomerSample3")]
        public static  IList<Customer> InsertCustomerSample3([HttpTrigger] HttpRequestMessage req,
                                            [Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection",
                                            CommandTimeout = 60)] out IList<Customer> customers,
                                            ILogger log)
        {
            customers = JsonConvert.DeserializeObject<IList<Customer>>(req.Content.ReadAsStringAsync().Result);
            return customers;
        } 

        [FunctionName("InsertCustomerSample4")]
        public static  IList<Customer> InsertCustomerSample4([HttpTrigger] HttpRequestMessage req,
                                            [Dapper(Sql = "SpInsertCustomer2", SqlConnection = "SqlConnection",
                                            CommandTimeout = 60,
                                            CommandType = CommandType.StoredProcedure)] out IList<Customer> customers,
                                            ILogger log)
        {
            customers = JsonConvert.DeserializeObject<IList<Customer>>(req.Content.ReadAsStringAsync().Result);
            return customers;
        }

        [FunctionName("SelectCustomerSample1")]
        public static IList<Customer> SelectCustomerSample1([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "select.sql", SqlConnection = "SqlConnection", 
                                                  Parameters = "FirstName:{Query.FirstName}",
                                                  IsolationLevel = IsolationLevel.ReadCommitted)] IList<Customer> customers,
                                          ILogger log)
        {     
            return customers;
        }

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

        [FunctionName("SelectCustomerSample3")]
        public static IList<Customer> SelectCustomerSample3([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "SELECT * FROM [dbo].[Customers]",
                                                  SqlConnection = "SqlConnection")] IList<Customer> customers,
                                          ILogger log)
        {
            return customers;
        }

        [FunctionName("SelectCustomerSample4")]
        public static IList<Customer> SelectCustomerSample4 ([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "SpGetCustomerByFirstname",
                                                  SqlConnection = "SqlConnection",
                                                  Parameters = "FirstName:{Query.FirstName}",
                                                  CommandType = CommandType.StoredProcedure)] IList<Customer> customers,
                                          ILogger log)
        {
            return customers;
        }

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
    }
}
