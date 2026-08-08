using Microsoft.Playwright;
using Xunit;

namespace JM.UITests.Fixtures;

/// <summary>
/// Factory that produces an authenticated browser context per test. Each test gets
/// a fresh context so role-switching (admin vs. non-admin) works reliably within a
/// single test run. Login is performed through the AuthServer.
/// </summary>
public class PlaywrightSessionFactory : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestSettings.Headless,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
        });
    }

    public async Task<IBrowserContext> NewAuthenticatedContextAsync(string? username = null, string? password = null)
    {
        var ctx = await _browser!.NewContextAsync(new BrowserNewContextOptions { BypassCSP = true });
        await using var page = await ctx.NewPageAsync();

        username ??= TestSettings.Username;
        password ??= TestSettings.Password;

        await page.GotoAsync(TestSettings.AuthBaseAddress);

        // Fill login form — tolerant of common AuthServer input shapes.
        await page.FillAsync("input[name=\"Email\" i], input[name=\"Username\" i], input[type=\"email\" i], input[type=\"text\" i]", username);
        await page.FillAsync("input[name=\"Password\" i], input[type=\"password\" i]", password);

        var loginSelectors = new[]
        {
            "button[type=\"submit\"]",
            "button:has-text(\"Log in\")",
            "button:has-text(\"Login\")",
            "input[type=\"submit\"][value=\"Log in\"]",
            "input[type=\"submit\"][value=\"Login\"]"
        };
        foreach (var sel in loginSelectors)
        {
            await page.ClickAsync(sel).ConfigureAwait(false);
            try { await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 5000 }); break; }
            catch { /* try next selector */ }
        }

        // Establish the Blazor UI session so any auth cookie is written to this context.
        await page.GotoAsync(TestSettings.UiBaseAddress, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        return ctx;
    }

    public async Task DisposeAsync()
    {
        if (_browser != null) await _browser.CloseAsync();
        if (_playwright != null) _playwright.Dispose();
    }
}
