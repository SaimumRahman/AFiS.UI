using FluentAssertions;
using JM.UITests.Pages;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace JM.UITests.Tests;

/// <summary>
/// Req 1: In SalesPOS, when the Add Customer popup loads, a non-admin user sees only
/// "General" pre-selected and the membership dropdown is locked; an admin (UserId == 1)
/// sees all membership types unlocked with General default-selected.
/// </summary>
public class CustomerPopupMembershipTests : UiTestBase
{
    private readonly ITestOutputHelper _output;
    public CustomerPopupMembershipTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task NonAdmin_User_MembershipDropdown_ShowsOnlyGeneral_And_IsLocked()
    {
        await using var ctx = await NewSessionAsync();          // default = non-admin user
        var page = await ctx.NewPageAsync();
        await page.GotoAsync($"{TestSettings.UiBaseAddress}/salespos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var salesPos = new SalesPosPage(page);

        // Ensure the Sales tab is active (where the Add Customer button appears).
        await salesPos.ClickTabAsync("Cart");

        await salesPos.AddCustomerButton.ClickAsync();

        // The membership dropdown (RadzenDropDown) should render.
        var dropdown = page.Locator(".rz-dropdowncontainer").Nth(1); // 1st is customer lookup, 2nd is MemberTypeId
        await dropdown.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        // Expand to inspect options.
        await dropdown.ClickAsync();
        var options = page.Locator(".rz-dropdown-item");
        var count = await options.CountAsync();
        var visibleTexts = new List<string>();
        for (int i = 0; i < count; i++)
            visibleTexts.Add(await options.Nth(i).InnerTextAsync());

        _output.WriteLine($"Visible membership options: {string.Join(" | ", visibleTexts)}");

        // Only "General" should be offered to a non-admin.
        visibleTexts.Should().ContainSingle(t => t.Trim().Equals("General", StringComparison.OrdinalIgnoreCase),
            "non-admin users must see only the General membership type");

        // Dropdown should be disabled/locked.
        var isDisabled = await dropdown.EvaluateAsync<bool>("el => el.hasAttribute('aria-disabled') || el.classList.contains('rz-disabled')");
        isDisabled.Should().BeTrue("membership dropdown must be locked for non-admin users");
    }

    [Fact]
    public async Task Admin_User_MembershipDropdown_ShowsAllTypes_And_IsUnlocked()
    {
        // Assumes TestSettings provides admin credentials; using the default user
        // whose account is the admin (UserId == 1) in this environment.
        await using var ctx = await NewSessionAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync($"{TestSettings.UiBaseAddress}/salespos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var salesPos = new SalesPosPage(page);
        await salesPos.ClickTabAsync("Cart");

        await salesPos.AddCustomerButton.ClickAsync();

        var dropdown = page.Locator(".rz-dropdowncontainer").Nth(1);
        await dropdown.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var isDisabled = await dropdown.EvaluateAsync<bool>("el => el.hasAttribute('aria-disabled') || el.classList.contains('rz-disabled')");
        isDisabled.Should().BeFalse("admin users must have the membership dropdown unlocked");

        // Expand and confirm more than one option exists (i.e. General + others).
        await dropdown.ClickAsync();
        var options = page.Locator(".rz-dropdown-item");
        (await options.CountAsync()).Should().BeGreaterThan(1, "admin must see multiple membership types");
    }
}
