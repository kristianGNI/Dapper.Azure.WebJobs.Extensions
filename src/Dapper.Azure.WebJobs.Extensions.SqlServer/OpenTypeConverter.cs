using Microsoft.Azure.WebJobs;

namespace Dapper.Azure.WebJobs.Extensions.SqlServer
{
    internal class OpenTypeConverter<T> : IConverter<T, SqlInput>
    {
        public SqlInput Convert(T input)
        {
            return new SqlInput() { Parameters = input };
        }
    }
}
