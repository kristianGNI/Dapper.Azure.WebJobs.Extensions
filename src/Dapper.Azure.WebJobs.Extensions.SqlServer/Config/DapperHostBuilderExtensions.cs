using System;
using Dapper.Azure.WebJobs.Extensions.SqlServer.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting
{
    public static class DapperHostBuilderExtensions
    {
        public static IWebJobsBuilder AddDapperSqlServer(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<DapperExtensionConfigProvider>();

            return builder;
        }
    }
}
