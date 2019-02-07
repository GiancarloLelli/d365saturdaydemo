using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365.Saturday.DataProvider.Data
{
    public class WideWorldImportersRepository
    {
        private readonly string m_connection;
        private readonly string m_publisher;

        public WideWorldImportersRepository(string connection, string publisher)
        {
            m_connection = connection;
            m_publisher = publisher;
        }

        public async Task<EntityCollection> Search(string entityName, Dictionary<string, string> criteria, IList<string> columns, int count)
        {
            var collection = new EntityCollection();
            var columnsList = FormatColumns(columns, entityName);
            var criteriaText = string.Empty;

            if (criteria.Count > 0)
                criteriaText = $"WHERE {FormatCriteria(criteria)}";

            using (var connection = new SqlConnection(m_connection))
            {
                using (var cmd = new SqlCommand($"SELECT TOP {count} {columnsList} FROM {Map(entityName)} WITH (NOLOCK) {criteriaText}"))
                {
                    cmd.Connection = connection;
                    await cmd.Connection.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var item = new Entity(entityName);

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? null : reader[i];

                            if (value == null)
                                continue;

                            var fieldName = reader.GetName(i);
                            var crmAttribute = $"{m_publisher}_{fieldName.ToLower()}";
                            item[crmAttribute] = value;
                        }

                        collection.Entities.Add(item);
                    }
                }
            }

            return collection;
        }

        public async Task<Entity> GetById(string entity, Guid id)
        {
            Entity result = null;
            var table = Map(entity);

            using (var connection = new SqlConnection(m_connection))
            {
                using (var cmd = new SqlCommand($"SELECT TOP 1 * FROM {table} WITH (NOLOCK) WHERE [WideWorldImportersId] = '{id.ToString()}'"))
                {
                    cmd.Connection = connection;
                    await cmd.Connection.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        result = new Entity(entity);

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? null : reader[i];

                            if (value == null)
                                continue;

                            var fieldName = reader.GetName(i);
                            var crmAttribute = $"{m_publisher}_{fieldName.ToLower()}";
                            result[crmAttribute] = value;
                        }
                    }
                }
            }

            return result;
        }

        private string FormatColumns(IList<string> cols, string entity)
        {
            var sqlColumns = new List<string>();

            // Primary Key
            sqlColumns.Add($"{entity.Replace($"{m_publisher}_", string.Empty)}id");

            foreach (var item in cols)
            {
                var sqlColumn = item.Replace($"{m_publisher}_", string.Empty);
                sqlColumns.Add(sqlColumn);
            }

            return sqlColumns.Aggregate((x, y) => string.Concat(x, ",", y));
        }

        private string FormatCriteria(Dictionary<string, string> criteria)
        {
            var builder = new StringBuilder();
            var stop = criteria.Keys.Count - 1;

            for (int i = 0; i < criteria.Keys.Count; i++)
            {
                string column = criteria.Keys.ElementAt(i).Replace($"{m_publisher}_", string.Empty);
                builder.Append($"{column} = '{criteria.Values.ElementAt(i)}'");

                if (!(i == stop))
                {
                    builder.Append(" AND ");
                }
            }

            return builder.ToString();
        }

        private string Map(string entityName)
        {
            var result = string.Empty;

            switch (entityName)
            {
                case "sat_wideworldimporters":
                    result = "Application.Demo";
                    break;
                default:
                    throw new Exception("Unknown entity");
            }

            return result;
        }
    }
}