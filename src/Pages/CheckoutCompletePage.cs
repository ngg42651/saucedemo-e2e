using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CheckoutCompletePage(IPage page)
{
    public ILocator Header => page.Locator("[data-test=\"complete-header\"]");
}
