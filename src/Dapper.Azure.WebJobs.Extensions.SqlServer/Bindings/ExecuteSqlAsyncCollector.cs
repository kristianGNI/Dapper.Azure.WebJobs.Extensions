using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using System.Collections;
using System.Linq;
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
            var parameters = GetParameters(input.Parameters);
            await GenericSqlStore.Execute(parameters , _dapperAttribute.SqlConnection, sql, _dapperAttribute.CommandTimeout, _dapperAttribute.IsolationLevel, _dapperAttribute.CommandType);
        }
        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
        private DynamicParameters GetParameters(dynamic dynParameters)
        {
            if(dynParameters == null) return null;
            DynamicParameters parameters = new DynamicParameters();

            if (Utility.IsEnumerable(dynParameters))
                ((IEnumerable)dynParameters).Cast<object>().ToList().ForEach(x=> parameters.AddDynamicParams(x));            
            else
                parameters.AddDynamicParams(dynParameters);
            return parameters;
        }        
    }
}
