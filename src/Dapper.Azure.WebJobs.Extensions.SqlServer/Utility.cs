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
        public static bool IsEnumerable(object type)
        {
            return type is IEnumerable;
        }
        public static DynamicParameters GetParameters(dynamic dynParameters)
        {
            if(dynParameters == null) return null;
            DynamicParameters parameters = new DynamicParameters();

            if (Utility.IsEnumerable(dynParameters))
                ((IEnumerable)dynParameters).Cast<object>().ToList().ForEach(x=> parameters.AddDynamicParams(x));            
            else
                parameters.AddDynamicParams(dynParameters);
            return parameters;
        }
        public static DynamicParameters GetParameters(string strParameters, string sql)
        {
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            if (string.IsNullOrEmpty(strParameters)) return null;

            DynamicParameters parameters;
            if (Utility.IsJson(strParameters))
                parameters = GetParametersFromJSON(strParameters);
            else if(Utility.IsParameterizeSql(sql))
                parameters =  GetMatchedParametersFromString(strParameters, sql);
            else
                parameters = GetParametersFromString(strParameters);                
            return parameters;
        }
        public static DynamicParameters GetMatchedParametersFromString(string strParameters, string sql)
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
        public static DynamicParameters GetParametersFromString(string strParameters)
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
        public static DynamicParameters GetParametersFromJSON(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            DynamicParameters parameters = new DynamicParameters();
            var objects = JsonConvert.DeserializeObject(json);
            if(IsEnumerable(parameters)){
                ((IEnumerable)parameters).Cast<Object>().ToList().ForEach(x=> 
                    parameters.AddDynamicParams(x)
                );
            }
            else{
                parameters.AddDynamicParams(objects);
            }
            return parameters;
        }
        public static bool IsParameterizeSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) throw new System.ArgumentNullException(nameof(sql));
            var sqlParameters = Utility.GetWords(sql);
            return sqlParameters.Count() > 0;
        }
    }
}
