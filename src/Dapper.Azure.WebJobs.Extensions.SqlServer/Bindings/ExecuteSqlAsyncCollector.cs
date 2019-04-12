using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer.Bindings
{
    internal class ExecuteSqlAsyncCollector : IAsyncCollector<SqlInput>
    {
        private DapperAttribute _dapperAttribute;
        public ExecuteSqlAsyncCollector(DapperAttribute attribute)
        {
            _dapperAttribute = attribute ?? throw new System.ArgumentNullException(nameof(attribute));
        }
        public async Task AddAsync(SqlInput input, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (input == null) throw new System.ArgumentNullException(nameof(input));

            string sql = _dapperAttribute.Sql;
            if (Utility.IsSqlScript(sql))
                sql = Utility.GetTextFromFile(sql);

            await GenericSqlStore.Execute(input, _dapperAttribute.SqlConnection, sql, _dapperAttribute.CommandTimeout);
        }
        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
        
    }
}
