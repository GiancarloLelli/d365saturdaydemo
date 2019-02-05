using D365.Saturday.DataProvider.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D365.Saturday.DataProvider
{
    public class NorthwindTradersDataProvider : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            var qe = context.InputParameters["Query"] as QueryExpression;

            if (qe != null)
            {
                var visitor = new NorthwindTradersVisitor();
                qe.Accept(visitor);

                if (visitor.SQLCriteria.Count > 0)
                {
                    var repo = new NorthwindRepository("", "");
                    var task = Task.Run(() => repo.Search(qe.EntityName, visitor.SQLCriteria, visitor.Columns));
                    context.OutputParameters["BusinessEntityCollection"] = task.Result;
                }
            }
        }
    }

    public class NorthwindTradersVisitor : QueryExpressionVisitorBase
    {
        public Dictionary<string, string> SQLCriteria { get; private set; }

        public IList<string> Columns { get; private set; }

        public override QueryExpression Visit(QueryExpression query)
        {
            var filter = query.Criteria;
            var columns = query.ColumnSet;

            if (columns.AllColumns)
            {
                Columns.Add("*");
            }
            else
            {
                Columns = columns.Columns;
            }

            if (filter.Conditions.Count > 0)
            {
                foreach (ConditionExpression condition in filter.Conditions)
                {
                    if (condition.Operator == ConditionOperator.Equal && condition.Values.Count > 0)
                    {
                        var conditionValue = condition.Values.FirstOrDefault() as string;

                        if (!string.IsNullOrEmpty(conditionValue))
                        {
                            SQLCriteria.Add(condition.AttributeName, conditionValue);
                        }
                    }
                }
            }

            return query;
        }
    }
}
