using Dapper.Azure.WebJobs.Extensions.SqlServer.Bindings;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer.Config
{
    [Extension("Dapper")]
    public class DapperExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<JObject, SqlInput>(input => {
                return ConvertFromJObject(input);
            });
            context.AddConverter<JArray, SqlInput>(input => {
                return ConvertFromJArray(input);                
            });

            context.AddOpenConverter<OpenType, SqlInput>(typeof(OpenTypeToSqlInputConverter<>));

            var rule = context.AddBindingRule<DapperAttribute>();
            rule.BindToCollector<SqlInput>(attr => new ExecuteSqlAsyncCollector(attr));
            rule.BindToInput<OpenType>(typeof(DapperAttributeToExecuteQueryAsyncConverter<>));
        }
        public SqlInput ConvertFromJArray(JArray arrary)
        {
            SqlInput input = new SqlInput();
            List<ExpandoObject> list = new List<ExpandoObject>();
            foreach (var item in arrary)
            {
                var expando = JsonConvert.DeserializeObject<ExpandoObject>(item.ToString(Newtonsoft.Json.Formatting.None));
                list.Add(expando);
            }
            input.Parameters = list;
            return input;
        }
        public SqlInput ConvertFromJObject(JObject jObject)
        {
            SqlInput input = new SqlInput();
            List<ExpandoObject> list = new List<ExpandoObject>();
            var expando = JsonConvert.DeserializeObject<ExpandoObject>(jObject.ToString(Newtonsoft.Json.Formatting.None), new ExpandoObjectConverter());
            list.Add(expando);
            input.Parameters = list;
            return input;
        }
    }
}
