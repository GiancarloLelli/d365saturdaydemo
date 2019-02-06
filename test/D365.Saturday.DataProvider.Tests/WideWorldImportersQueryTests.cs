using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

namespace D365.Saturday.DataProvider.Tests
{
    [TestClass]
    public class WideWorldImportersQueryTests
    {
        [TestMethod]
        public void Query_Count_Should_Be_Respected()
        {
            var query = new QueryExpression("sat_account");
            query.Criteria.AddCondition("sat_preferredname", ConditionOperator.Equal, "Kayla");
            query.TopCount = 1;

            var provider = new WideWorldImportersDataProvider();
            var result = provider.ExecuteWrapper(query);

            Assert.AreEqual(result.Entities.Count, 1);
        }
    }
}
