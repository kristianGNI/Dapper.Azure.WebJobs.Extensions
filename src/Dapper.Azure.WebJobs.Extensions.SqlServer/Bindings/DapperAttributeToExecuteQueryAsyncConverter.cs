using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
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

            DynamicParameters parameters;
            if (Utility.IsJson(attr.Parameters))
                parameters = GetParametersFromJSON(attr.Parameters);
            else
                parameters = GetParametersFromString(attr.Parameters); 
            var result = await GenericSqlStore.ExecuteQuery(parameters, attr.SqlConnection, sql, attr.CommandTimeout, attr.IsolationLevel, attr.CommandType);
            return HandleQueryResult(result);
        }
        private DynamicParameters GetParametersFromString(string strParameters)
        {
            if (string.IsNullOrEmpty(strParameters)) return null;
                        
            var values = Utility.StringToDict(strParameters);
            DynamicParameters parameters = new DynamicParameters();
            foreach (var item in values)
            {
                parameters.Add("@" + item.Key, item.Value);
            }
            return parameters;
        }
        private DynamicParameters GetParametersFromJSON(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            DynamicParameters parameters = new DynamicParameters();
            var objects = JsonConvert.DeserializeObject(json);
            parameters.AddDynamicParams(objects);
            
            return parameters;
        }
        private static T HandleQueryResult(IEnumerable<dynamic> result)
        {
            T resultValue = default(T);
            if (result != null && result.Count() > 0)
            {
                string json;
                if(Utility.IsEnumerable(typeof(T)))
                    json = JsonConvert.SerializeObject(result);
                else
                    json = JsonConvert.SerializeObject(result.FirstOrDefault());
                resultValue = JsonConvert.DeserializeObject<T>(json);
            }
            return resultValue;
        }
     }
}
