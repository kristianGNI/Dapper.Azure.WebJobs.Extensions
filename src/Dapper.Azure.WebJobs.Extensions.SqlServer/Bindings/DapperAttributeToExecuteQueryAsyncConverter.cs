using Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Collections;
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
            else if(Utility.IsParameterizeSql(sql))
                parameters =  GetMatchedParametersFromString(attr.Parameters, sql);
            else
                parameters = GetParametersFromString(attr.Parameters); 

            return await GenericSqlStore.ExecuteQuery<T>(parameters, attr.SqlConnection, sql, attr.CommandTimeout, attr.IsolationLevel, attr.CommandType);
        }
        private DynamicParameters GetMatchedParametersFromString(string strParameters, string sql)
        {
            if (string.IsNullOrEmpty(strParameters)) return null;
            
            var sqlParameter = Utility.GetWords(sql);
            var values = Utility.StringToDict(strParameters);
            DynamicParameters parameters = new DynamicParameters();
            for (int i = 0; i < sqlParameter.Count(); i++)
            {
                ((DynamicParameters)parameters).Add(sqlParameter[i], values[sqlParameter[i].Remove(0, 1)], null, ParameterDirection.Input);
            }
            return parameters;
        }
        private DynamicParameters GetParametersFromString(string strParameters)
        {
            if (string.IsNullOrEmpty(strParameters)) return null;
                        
            var values = Utility.StringToDict(strParameters);
            DynamicParameters parameters = new DynamicParameters();
            foreach (var item in values)
            {
                ((DynamicParameters)parameters).Add("@" + item.Key, item.Value, null, ParameterDirection.Input);
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
     }
}
