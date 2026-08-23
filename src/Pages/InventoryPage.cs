using System.Globalization;
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class InventoryPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Title => _page.Locator("[data-test=\"title\"]");
    public ILocator Items => _page.Locator("[data-test=\"inventory-item\"]");

    private ILocator SortDropdown => _page.Locator("[data-test=\"product-sort-container\"]");
    private ILocator Names => _page.Locator("[data-test=\"inventory-item-name\"]");
    private ILocator Prices => _page.Locator("[data-test=\"inventory-item-price\"]");
    private ILocator Images => _page.Locator(".inventory_item_img img");

    /// <param name="optionValue">az, za, lohi, hilo 중 하나</param>
    public Task SortByAsync(string optionValue) =>
        SortDropdown.SelectOptionAsync(optionValue);

    public async Task<IReadOnlyList<string>> ProductNamesAsync() =>
        await Names.AllInnerTextsAsync();

    public async Task<IReadOnlyList<decimal>> ProductPricesAsync()
    {
        var texts = await Prices.AllInnerTextsAsync();
        return texts
            .Select(t => decimal.Parse(t.TrimStart('$'), CultureInfo.InvariantCulture))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> ImageSourcesAsync()
    {
        var count = await Images.CountAsync();
        var sources = new List<string>(count);
        for (var i = 0; i < count; i++)
            sources.Add(await Images.Nth(i).GetAttributeAsync("src") ?? "");
        return sources;
    }

    public Task AddToCartAsync(string productName) =>
        _page.Locator($"[data-test=\"add-to-cart-{Slug(productName)}\"]").ClickAsync();

    public Task RemoveFromCartAsync(string productName) =>
        _page.Locator($"[data-test=\"remove-{Slug(productName)}\"]").ClickAsync();

    public Task OpenProductAsync(string productName) =>
        Names.GetByText(productName, new() { Exact = true }).ClickAsync();

    /// <summary>"Sauce Labs Backpack"을 "sauce-labs-backpack"으로 바꾼다. 사이트의 data-test 명명 규칙이다.</summary>
    private static string Slug(string productName) =>
        productName.ToLowerInvariant().Replace(' ', '-');
}
