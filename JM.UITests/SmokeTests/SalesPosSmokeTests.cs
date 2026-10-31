using FluentAssertions;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace JM.UITests.SmokeTests;

/// <summary>
/// Smoke test: logs in via the AuthServer and navigates to the Blazor SalesPOS UI,
/// confirming the page loads for an authenticated user.
/// </summary>
public class SalesPosSmokeTests : UiTestBase
{
    private readonly ITestOutputHelper _output;
    public SalesPosSmokeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task SalesPOS_Page_Should_Load_For_Authenticated_User()
    {
        await using var ctx = await NewSessionAsync();
        var page = await ctx.NewPageAsync();
        var response = await page.GotoAsync($"{TestSettings.UiBaseAddress}/salespos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var title = await page.TitleAsync();
        _output.WriteLine($"Page title: {title}, final URL: {page.Url}, status: {response?.Status}");

        title.Should().NotBeNullOrWhiteSpace();
        (response?.Status).Should().Be(200, "an authenticated user must reach the SalesPOS UI");

        var posRoot = await page.Locator(".pos-body").First.IsVisibleAsync();
        posRoot.Should().BeTrue("the SalesPOS .pos-body container should be rendered");
    }
}
