using Dapper.Azure.WebJobs.Extensions.SqlServer.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

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
