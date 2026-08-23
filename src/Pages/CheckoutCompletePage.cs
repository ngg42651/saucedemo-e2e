using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CheckoutCompletePage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Header => _page.Locator("[data-test=\"complete-header\"]");
}
