using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class ProductDetailPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Name => _page.Locator("[data-test=\"inventory-item-name\"]");
    public ILocator Price => _page.Locator("[data-test=\"inventory-item-price\"]");

    public Task BackToProductsAsync() =>
        _page.Locator("[data-test=\"back-to-products\"]").ClickAsync();
}
