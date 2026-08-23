using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CartPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Items => _page.Locator("[data-test=\"inventory-item\"]");
    private ILocator Names => _page.Locator("[data-test=\"inventory-item-name\"]");

    public async Task<IReadOnlyList<string>> ItemNamesAsync() =>
        await Names.AllInnerTextsAsync();

    public Task GotoCheckoutAsync() =>
        _page.Locator("[data-test=\"checkout\"]").ClickAsync();
}
