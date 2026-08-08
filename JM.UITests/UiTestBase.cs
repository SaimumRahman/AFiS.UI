using JM.UITests.Fixtures;
using Microsoft.Playwright;
using Xunit;

namespace JM.UITests;

/// <summary>
/// Base class for UI (Playwright) tests. Creates a fresh authenticated browser
/// context per test via <see cref="PlaywrightSessionFactory"/> so that role-based
/// scenarios (admin vs. non-admin) can be tested independently.
///
/// Tests inherit this and call <c>await NewSessionAsync()</c> at the start to get
/// an <see cref="IBrowserContext"/> with login cookies applied, then
/// <c>await context.NewPageAsync()</c> for the working page.
/// </summary>
public abstract class UiTestBase : IAsyncLifetime
{
    protected readonly PlaywrightSessionFactory Factory = new();

    protected virtual async Task<IBrowserContext> NewSessionAsync(string? username = null, string? password = null)
        => await Factory.NewAuthenticatedContextAsync(username, password);

    public virtual async Task InitializeAsync() => await Factory.InitializeAsync();

    public virtual async Task DisposeAsync() => await Factory.DisposeAsync();
}
