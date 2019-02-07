using D365.Saturday.DataProvider.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Threading.Tasks;

namespace D365.Saturday.DataProvider
{
    public class WideWorldImportersDataProvider : IPlugin
    {
        private const string PUBLISHER = "sat";
        private const string SQL = "";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            var isRetrieveMultiple = context.MessageName.ToLower().Equals("retrievemultiple");

            if (isRetrieveMultiple)
            {
                var qe = context.InputParameters["Query"] as QueryExpression;
                var collectionResult = RetrieveMultipleLogic(qe);
                context.OutputParameters["BusinessEntityCollection"] = collectionResult;
            }
            else
            {
                var entity = context.InputParameters["Target"] as EntityReference;
                var singleResult = RetrieveLogic(entity.LogicalName, entity.Id);
                context.OutputParameters["BusinessEntity"] = singleResult;
            }
        }

        public EntityCollection RetrieveMultipleWrapper(QueryExpression mockQuery) => RetrieveMultipleLogic(mockQuery);

        public Entity RetrieveWrapper(string logicalName, Guid id) => RetrieveLogic(logicalName, id);

        private EntityCollection RetrieveMultipleLogic(QueryExpression qe)
        {
            var collection = new EntityCollection();

            if (qe != null)
            {
                var visitor = new WideWorlImportersQueryVisitor();
                qe.Accept(visitor);

                if (visitor.SQLCriteria.Count > 0)
                {
                    var repo = new WideWorldImportersRepository(SQL, PUBLISHER);
                    var task = Task.Run(() => repo.Search(qe.EntityName, visitor.SQLCriteria, visitor.Columns, visitor.Count));
                    collection = task.Result;
                }
            }

            return collection;
        }

        private Entity RetrieveLogic(string logicalName, Guid id)
        {
            var repo = new WideWorldImportersRepository(SQL, PUBLISHER);
            var task = Task.Run(() => repo.GetById(logicalName, id));
            return task.Result;
        }
    }
}
