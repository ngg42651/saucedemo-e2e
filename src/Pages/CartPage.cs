using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CartPage(IPage page)
{
    private readonly IPage _page = page;

    // inventory-item / inventory-item-name data-test는 상품 목록 페이지와 공유되는 값이다.
    // 사이트가 장바구니 항목도 같은 마크업 구조로 렌더링하기 때문이다.
    public ILocator Items => _page.Locator("[data-test=\"inventory-item\"]");
    private ILocator Names => _page.Locator("[data-test=\"inventory-item-name\"]");
    private ILocator CheckoutButton => _page.Locator("[data-test=\"checkout\"]");

    public async Task<IReadOnlyList<string>> ItemNamesAsync() =>
        await Names.AllInnerTextsAsync();

    public Task GotoCheckoutAsync() => CheckoutButton.ClickAsync();
}
