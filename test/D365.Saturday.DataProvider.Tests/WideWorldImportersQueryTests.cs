using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace D365.Saturday.DataProvider.Tests
{
    [TestClass]
    public class WideWorldImportersQueryTests
    {
        [TestMethod]
        public void Retrieve_Multiple_Test()
        {
            var query = new QueryExpression("sat_wideworldimporters");
            query.ColumnSet = new ColumnSet("sat_logonname", "sat_fullname");
            query.Criteria.AddCondition("sat_preferredname", ConditionOperator.Equal, "Kayla");
            query.TopCount = 1;

            var provider = new WideWorldImportersDataProvider();
            var result = provider.RetrieveMultipleWrapper(query);

            Assert.AreEqual(result.Entities.Count, 1);
        }

        [TestMethod]
        public void Retrieve_Test()
        {
            var provider = new WideWorldImportersDataProvider();
            var result = provider.RetrieveWrapper("sat_wideworldimporters", Guid.Parse("bbba5a6d-e965-21ff-4b73-e891b4afe6e8"));
            Assert.IsNotNull(result);
        }
    }
}
