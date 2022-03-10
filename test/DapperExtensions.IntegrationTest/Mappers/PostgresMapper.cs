using System.Linq;
using DapperExtensions.Mapper;

namespace DapperExtensions.IntegrationTest.Mappers;

/// <summary>
/// Converts column names to snake case
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class PostgresMapper<T> : AutoClassMapper<T> where T : class
{
    protected override void AutoMap()
    {
        base.AutoMap();

        foreach (var property in Properties)
        {
            if (property is not MemberMap propertyMap)
            {
                continue;
            }

            var columnName = property.ColumnName;
            var snakeColumnName = string.Concat(columnName.Select((x, i) =>
                i > 0 && char.IsUpper(x) && !char.IsUpper(columnName[i - 1]) ? $"_{x}" : x.ToString())).ToLower();

            propertyMap.Column(snakeColumnName);
        }
    }
}