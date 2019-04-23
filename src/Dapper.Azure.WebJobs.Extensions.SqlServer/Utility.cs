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
            if (string.IsNullOrEmpty(input)) throw new System.ArgumentNullException(nameof(input));

            MatchCollection matches = Regex.Matches(input, @"\B@\w+");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value.Trim());

            return words.ToArray();
        }
        public static string TrimSuffix(string word)
        {
            if (string.IsNullOrEmpty(word)) throw new System.ArgumentNullException(nameof(word));

            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
        public static string GetTextFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new System.ArgumentNullException(nameof(fileName));

            string path = Path.Combine(Environment.CurrentDirectory, fileName);
            return System.IO.File.ReadAllText(path);
        }
        public static Dictionary<string, string> StringToDict(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new System.ArgumentNullException(nameof(input));
            return input
                    .Split(',')
                    .Select(part => part.Split(':'))
                    .Where(part => part.Length == 2)
                    .ToDictionary(sp => sp[0].Trim(), sp => sp[1].Trim());
        }
        public static bool IsEnumerable(object type)
        {
            return type is IEnumerable;
        }
        public static bool IsParameterizeSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return false;
            
            var sqlParameters = Utility.GetWords(sql);
            return sqlParameters.Count() > 0;
        }
    }
}
