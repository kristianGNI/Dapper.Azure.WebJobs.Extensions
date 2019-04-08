using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                        select TrimSuffix(m.Value);

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
                    .ToDictionary(sp => sp[0], sp => sp[1]);
        }
        public static bool IsEnumerable(Type type)
        {
            return (type.GetInterface(nameof(IEnumerable)) != null);
        }
        public static bool IsEnumerable(dynamic type)
        {
            return type is IEnumerable;
        }
    }
}
