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
            var qe = context.InputParameters["Query"] as QueryExpression;
            var result = ExecuteLogic(qe);
            context.OutputParameters["BusinessEntityCollection"] = result;
        }

        public EntityCollection ExecuteWrapper(QueryExpression mockQuery) => ExecuteLogic(mockQuery);

        private EntityCollection ExecuteLogic(QueryExpression qe)
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
    }
}
