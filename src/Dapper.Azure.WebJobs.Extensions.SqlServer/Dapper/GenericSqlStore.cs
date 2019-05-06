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
        public static async Task Execute(IEnumerable<DynamicParameters> parameters, string connectionString, string sql, int? commandTimeout, IsolationLevel isolationLevel, 
                                        CommandType commandType)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new System.ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {
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
        public static async Task<IEnumerable<dynamic>> ExecuteQuery(DynamicParameters parameters, string connectionString, string sql, int? commandTimeout, IsolationLevel isolationLevel, 
                                                    CommandType commandType)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new System.ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            
            using (var connection = new SqlConnection(connectionString))
            {
                IEnumerable<dynamic> result;
                await connection.OpenAsync().ConfigureAwait(false);
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {                      
                        result = await connection.QueryAsync(sql, parameters, 
                                                                transaction: transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
                        transaction.Commit();
                    }
                    catch(Exception ex){
                        transaction.Rollback();
                        throw ex;
                    }
                }
                return result;
            }
        }
        
        
    }
}
