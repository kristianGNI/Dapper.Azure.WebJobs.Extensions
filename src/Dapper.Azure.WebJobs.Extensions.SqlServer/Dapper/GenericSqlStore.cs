using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer.Dapper
{
    internal class GenericSqlStore
    {
        public static async Task Execute(SqlInput input, string connectionString, string sql, int? commandTimeout, IsolationLevel isolationLevel, 
                                        CommandType commandType)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new System.ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            
            var isParameterizeSql = Utility.IsParameterizeSql(sql);
            if (isParameterizeSql)
            {
                if (input == null || input.Parameters == null)
                    throw new System.ArgumentNullException(nameof(input), "The sql statement is parameterized therefore input can't be null or empty");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        DynamicParameters parameters = Utility.GetParameters(input.Parameters);
                        await connection.ExecuteAsync(sql, parameters, 
                                                        transaction: transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        public static async Task<T> ExecuteQuery<T>(string connectionString, string sql, string parameters, int? commandTimeout, IsolationLevel isolationLevel, 
                                                    CommandType commandType)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new System.ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));

            var isParameterizeSql = Utility.IsParameterizeSql(sql);
            if (isParameterizeSql)
            {
                if (string.IsNullOrEmpty(parameters))
                    throw new System.ArgumentNullException(nameof(parameters), "The sql statement is parameterized therefore parameters can't be null or empty");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                IEnumerable<dynamic> result;
                await connection.OpenAsync().ConfigureAwait(false);
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        DynamicParameters queryParameters = Utility.GetParameters(parameters, sql);                        
                        result = await connection.QueryAsync(sql, queryParameters, 
                                                                transaction: transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
                        transaction.Commit();
                    }
                    catch(Exception ex){
                        transaction.Rollback();
                        throw ex;
                    }
                }
                return HandleQueryResult<T>(result);
            }
        }
        
        private static T HandleQueryResult<T>(IEnumerable<dynamic> result)
        {
            T resultValue = default(T);
            if (result != null && result.Count() > 0)
            {
                var resultType = typeof(T);
                string json;

                if (Utility.IsEnumerable(resultType))
                    json = JsonConvert.SerializeObject(result);
                else
                    json = JsonConvert.SerializeObject(result.FirstOrDefault());
                resultValue = JsonConvert.DeserializeObject<T>(json);
            }
            return resultValue;
        }
    }
}
