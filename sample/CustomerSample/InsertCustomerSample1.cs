using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dapper.Azure.WebJobs.Extensions.SqlServer;

namespace Samples
{
    public static class InsertCustomerSample1
    {
         [FunctionName("InsertCustomerSample1")]
         [return: Dapper(Sql = "insert.sql", SqlConnection = "SqlConnection")]
         public static Customer Run([HttpTrigger] Customer customer, ILogger log)
         {
             return customer;
         }
    }
}
