using ProductManagement.IntegrationTests.Helpers;
using Xunit;

namespace ProductManagement.IntegrationTests;

/// <summary>
/// Collection definition for integration tests.
/// All tests in this collection share the same factory instance and run sequentially.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
