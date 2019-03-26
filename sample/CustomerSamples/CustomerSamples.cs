using Dapper.Azure.WebJobs.Extensions.SqlServer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

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
                        SqlConnection = "SqlConnection")]
        public static Customer InsertCustomerSample2([HttpTrigger] Customer customer, ILogger log)
        {
           return customer;
        }

        [FunctionName("InsertCustomerSample3")]
        public static  IList<Customer> InsertCustomerSample3([HttpTrigger] HttpRequestMessage req,
                                            [Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")] out IList<Customer> customers,
                                            ILogger log)
        {
            customers = JsonConvert.DeserializeObject<IList<Customer>>(req.Content.ReadAsStringAsync().Result);
            return customers;
        }

        [FunctionName("SelectCustomerSample1")]
        public static IList<Customer> SelectCustomerSample1([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "select.sql", SqlConnection = "SqlConnection", 
                                                  parameters = "FirstName:{Query.FirstName}")] IList<Customer> customers,
                                          ILogger log)
        {     
            return customers;
        }

        [FunctionName("SelectCustomerSample2")]
        public static IList<Customer> SelectCustomerSample2([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "SELECT * FROM [dbo].[Customers] WHERE FirstName = @FirstName",
                                                  SqlConnection = "SqlConnection", 
                                                  parameters = "FirstName:{Query.FirstName}")] IList<Customer> customers,
                                          ILogger log)
        {
            return customers;
        }

        [FunctionName("SelectCustomerSample3")]
        public static IList<Customer> SelectCustomerSample3([HttpTrigger] HttpRequestMessage req,
                                          [Dapper(Sql = "SELECT * FROM [dbo].[Customers]",
                                                  SqlConnection = "SqlConnection")] IList<Customer> customers,
                                          ILogger log)
        {
            return customers;
        }
    }
}
