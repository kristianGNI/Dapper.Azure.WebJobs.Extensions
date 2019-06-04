using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer.Bindings
{
    internal class ExecuteSqlAsyncCollector : IAsyncCollector<SqlInput>
    {
        private DapperAttribute _dapperAttribute;
        private string _path;
        public ExecuteSqlAsyncCollector(DapperAttribute attribute, string path)
        {
            _dapperAttribute = attribute ?? throw new System.ArgumentNullException(nameof(attribute));
            _path = path;
        }
        public async Task AddAsync(SqlInput input, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (input == null) throw new System.ArgumentNullException(nameof(input));

            string sql = _dapperAttribute.Sql;
            if (Utility.IsSqlScript(sql))
                sql = Utility.GetTextFromFile(_path, sql);
            var parameters = GetParameters(input.Parameters);
            await GenericSqlStore.Execute(parameters , _dapperAttribute.SqlConnection, sql, _dapperAttribute.CommandTimeout, _dapperAttribute.IsolationLevel, _dapperAttribute.CommandType);
        }
        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
        private IEnumerable<DynamicParameters> GetParameters(dynamic dynParameters)
        {
            if(dynParameters == null) return null;
            IEnumerable<DynamicParameters> parameters;
            
            if (Utility.IsEnumerable(dynParameters)){
                var list = ((IEnumerable)dynParameters).Cast<object>().ToList();
                parameters = from item in list select new DynamicParameters(item);
            }         
            else
                parameters  = new List<DynamicParameters>(){ new DynamicParameters(dynParameters)};
            return parameters;
        }        
    }
}
