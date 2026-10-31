using JM.UITests.Fixtures;
using Xunit;

namespace JM.UITests;

/// <summary>
/// XUnit collection that holds the shared <see cref="ApiFixture"/> for API
/// integration tests in this project.
/// </summary>
[CollectionDefinition("API_INTEGRATION")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}
