using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer
{
    internal class Utility
    {
        public static bool IsJson(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            input = input.Trim();
            return (input.StartsWith("{", StringComparison.OrdinalIgnoreCase) && input.EndsWith("}", StringComparison.OrdinalIgnoreCase))
                || (input.StartsWith("[", StringComparison.OrdinalIgnoreCase) && input.EndsWith("]", StringComparison.OrdinalIgnoreCase));
        }
        public static bool IsSqlScript(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            return input.ToLower().EndsWith(".sql");
        }
        public static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\B@\w+");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value.Trim());

            return words.ToArray();
        }
        public static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
        public static string GetTextFromFile(string fileName)
        {
            string path = Path.Combine(Environment.CurrentDirectory, fileName);
            return System.IO.File.ReadAllText(path);
        }
        public static Dictionary<string, string> StringToDict(string input)
        {
            return input
                    .Split(',')
                    .Select(part => part.Split(':'))
                    .Where(part => part.Length == 2)
                    .ToDictionary(sp => sp[0].Trim(), sp => sp[1].Trim());
        }
        public static bool IsEnumerable(Type type)
        {
            return (type.GetInterface(nameof(IEnumerable)) != null);
        }
        public static bool IsEnumerable(dynamic type)
        {
            return type is IEnumerable;
        }
        private static object GetParameters(dynamic dynParameters)
        {
            if(dynParameters == null) throw new System.ArgumentNullException(nameof(dynParameters));

            if (Utility.IsEnumerable(dynParameters))
                return ((IEnumerable)dynParameters).Cast<object>().ToArray();            
            else
                return new[] { dynParameters };
        }
        public static object GetParameters(string strParameters, string sql, CommandType commandType)
        {
            if (string.IsNullOrEmpty(strParameters)) throw new System.ArgumentNullException(nameof(strParameters));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));

            object parameters = null;
            if (Utility.IsJson(strParameters))
            {
                parameters = JsonConvert.DeserializeObject<ExpandoObject>(strParameters, new ExpandoObjectConverter());
            }
            else if(commandType == CommandType.StoredProcedure){
                parameters = GetParameters(strParameters);
            }
            else
            {
                parameters =  GetParameters(strParameters, sql);
            }
            return parameters;
        }
        public static object GetParameters(string strParameters, string sql)
        {
            if (string.IsNullOrEmpty(strParameters)) throw new System.ArgumentNullException(nameof(strParameters));
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            
            var sqlParameter = Utility.GetWords(sql);
            var values = Utility.StringToDict(strParameters);
            object parameters = new DynamicParameters();
            for (int i = 0; i < sqlParameter.Count(); i++)
            {
                ((DynamicParameters)parameters).Add(sqlParameter[i], values[sqlParameter[i].Remove(0, 1)], null, ParameterDirection.Input);
            }
            return parameters;
        }
        public static object GetParameters(string strParameters)
        {
            if (string.IsNullOrEmpty(strParameters)) throw new System.ArgumentNullException(nameof(strParameters));
                        
            var values = Utility.StringToDict(strParameters);
            object parameters = new DynamicParameters();
            foreach (var item in values)
            {
                ((DynamicParameters)parameters).Add("@" + item.Key, item.Value, null, ParameterDirection.Input);
            }
            return parameters;
        }

        public static bool IsParameterizeSql(string sql, CommandType commandType, SqlInput input)
        {
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));

            if(commandType == CommandType.StoredProcedure && input != null && input.Parameters != null) 
                return true;
            return IsParameterizeSql(sql);;
        }
        public static bool IsParameterizeSql(string sql, CommandType commandType, string strParameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));

            if(commandType == CommandType.StoredProcedure && !string.IsNullOrEmpty(strParameters)) 
                return true;
            return IsParameterizeSql(sql);
        }
        public static bool IsParameterizeSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            var sqlParameters = Utility.GetWords(sql);
            return sqlParameters.Count() > 0;
        }
    }
}
