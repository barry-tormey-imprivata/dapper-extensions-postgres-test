using Xunit;

namespace DapperExtensions.IntegrationTest.Setup;

[CollectionDefinition("db")]
public class CollectionFixture : ICollectionFixture<TestContext>
{
}