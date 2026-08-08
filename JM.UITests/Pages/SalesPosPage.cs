using Microsoft.Playwright;

namespace JM.UITests.Pages;

/// <summary>
/// Page-object wrapping the elements/actions used by the SalesPOS UI tests.
/// Selectors mirror the actual component markup in SalesPOS.razor / AddCustomerDialog.razor.
/// </summary>
public class SalesPosPage
{
    private readonly IPage _page;
    public SalesPosPage(IPage page) => _page = page;

    public ILocator TabHeader(string name) => _page.Locator($".tab-header button:has-text(\"{name}\")");
    public ILocator AddCustomerButton => _page.Locator("button:has-text(\"+ New Customer\")");
    public ILocator MembershipDropdown => _page.Locator("div[tabindex]").Where(p => p.Locator("div.rz-dropdowncontainer").CountAsync().Result > 0);
    public ILocator CustomerDropdownTrigger => _page.Locator(".rz-dropdowncontainer").First;

    public ILocator CartRow(int index) => _page.Locator(".cart-grid .rz-datagrid-row").Nth(index);

    /// <summary>Price column editor on each cart row (RadzenNumeric renders an <input type="number"> with min=BaseUnitPrice).</summary>
    public ILocator CartPriceInputs => _page.Locator(".cart-grid .rz-datagrid-cell input[type=\"number\"][min]");
    public ILocator CartPriceInput(int row) => CartPriceInputs.Nth(row);

    /// <summary>The cart grid's discount column cell value (rendered as plain text).</summary>
    public ILocator CartRowByProductName(string productName) =>
        _page.Locator($".cart-grid .rz-datagrid-row").Where(r => r.InnerTextAsync().Result.Contains(productName));

    public ILocator InvoiceSearchBox => _page.Locator("input[placeholder=\"Search by invoice number...\"]");
    public ILocator InvoiceSearchButton => _page.Locator("button:has-text(\"Search\")");
    public ILocator InvoiceGridRows => _page.Locator(".rz-datagrid-row");

    public ILocator MembershipDropdownItems => _page.Locator(".rz-dropdown-item");

    public async Task ClickTabAsync(string name)
    {
        await TabHeader(name).ClickAsync();
        await _page.WaitForLoadStateAsync(WaitUntilState.NetworkIdle);
    }
}
