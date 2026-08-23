using Microsoft.Playwright;

namespace SauceDemo.E2E.Components;

public class CartBadge(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Badge => _page.Locator("[data-test=\"shopping-cart-badge\"]");
    private ILocator CartLink => _page.Locator("[data-test=\"shopping-cart-link\"]");

    /// <summary>장바구니가 비면 배지 요소 자체가 사라지므로 0을 반환한다.</summary>
    public async Task<int> CountAsync()
    {
        if (await Badge.CountAsync() == 0) return 0;
        return int.Parse(await Badge.InnerTextAsync());
    }

    public Task OpenCartAsync() => CartLink.ClickAsync();
}
