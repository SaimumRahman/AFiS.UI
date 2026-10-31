using Xunit;

namespace JM.UITests.Fixtures;

/// <summary>
/// Provides a fresh <see cref="HttpClient"/> (pointed at the API) for each test
/// class. Shared here so API tests and UI tests can coexist in one project.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    public HttpClient? Client { get; private set; }

    public Task InitializeAsync()
    {
        var handler = new HttpClientHandler();
        Client = new HttpClient(handler)
        {
            BaseAddress = new Uri(TestSettings.ApiBaseAddress)
        };
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }
}
