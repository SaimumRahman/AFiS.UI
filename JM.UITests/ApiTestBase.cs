using Xunit;
using Xunit.Abstractions;

namespace JM.UITests;

/// <summary>
/// Base class for API integration tests. Injects the shared <see cref="Fixtures.ApiFixture"/>
/// via the "API_INTEGRATION" collection and exposes an <see cref="ApiClient"/>.
/// </summary>
[Collection("API_INTEGRATION")]
public abstract class ApiTestBase
{
    protected readonly ApiClient Client;
    protected readonly ITestOutputHelper Output;

    protected ApiTestBase(Fixtures.ApiFixture fixture, ITestOutputHelper output)
    {
        Client = new ApiClient(fixture.Client!);
        Output = output;
    }
}
