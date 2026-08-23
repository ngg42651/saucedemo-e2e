using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class InventoryPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Title => _page.Locator("[data-test=\"title\"]");
}
