using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer.Bindings
{
    internal class DapperAttributeToExecuteQueryAsyncConverter<T> : IAsyncConverter<DapperAttribute, T>
    {
        public async Task<T> ConvertAsync(DapperAttribute attr, CancellationToken cancellationToken)
        {
            if (attr == null) throw new System.ArgumentNullException(nameof(attr));

            string sql = attr.Sql;
            if (Utility.IsSqlScript(sql))
                sql = Utility.GetTextFromFile(attr.Sql);

            return await GenericSqlStore.ExecuteQuery<T>(attr.SqlConnection, sql, attr.parameters);
        }
    }
}
