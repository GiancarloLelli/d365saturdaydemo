using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace D365.Saturday.DataProvider.Data
{
    public class NorthwindRepository
    {
        private readonly string m_connection;
        private readonly string m_publisher;

        public NorthwindRepository(string connection, string publisher)
        {
            m_connection = connection;
            m_publisher = publisher;
        }

        public async Task<EntityCollection> Search(string entityName, Dictionary<string, string> criteria, IList<string> columns)
        {
            var collection = new EntityCollection();
            var columnsList = "*"; // TODO: Aggregate
            var criteriaText = "";

            using (var connection = new SqlConnection(m_connection))
            {
                using (var cmd = new SqlCommand($"SELECT {columnsList} FROM {Map(entityName)} WITH (NOLOCK) WHERE {criteriaText}"))
                {
                    cmd.Connection = connection;
                    await cmd.Connection.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.NextResult())
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

        private string Map(string entityName)
        {
            var result = string.Empty;

            switch (entityName)
            {
                default:
                    break;
            }

            return result;
        }
    }
}
