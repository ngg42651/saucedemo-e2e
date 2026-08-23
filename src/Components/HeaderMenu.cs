using Microsoft.Playwright;

namespace SauceDemo.E2E.Components;

public class HeaderMenu(IPage page)
{
    private readonly IPage _page = page;

    private ILocator OpenButton => _page.Locator("[data-test=\"open-menu\"]");
    private ILocator LogoutLink => _page.Locator("[data-test=\"logout-sidebar-link\"]");
    private ILocator ResetLink => _page.Locator("[data-test=\"reset-sidebar-link\"]");

    public async Task LogoutAsync()
    {
        await OpenButton.ClickAsync(new LocatorClickOptions { Force = true });
        await LogoutLink.ClickAsync();
    }

    public async Task ResetAppStateAsync()
    {
        await OpenButton.ClickAsync(new LocatorClickOptions { Force = true });
        await ResetLink.ClickAsync();
    }
}
