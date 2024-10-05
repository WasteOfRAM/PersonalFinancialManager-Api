[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = false)]

namespace PersonalFinancialManager.IntegrationTests.Helpers;

[CollectionDefinition("Tests collection", DisableParallelization = false)]
public class TestsCollection : ICollectionFixture<TestsFixture>
{
}