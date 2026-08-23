using Microsoft.Playwright;

namespace SauceDemo.E2E.Components;

public class HeaderMenu(IPage page)
{
    private readonly IPage _page = page;

    // data-test="open-menu"는 장식용 <img>에 붙어 있고, 같은 .bm-burger-button 안의
    // 형제 <button>이 그 위를 덮는다. 클릭 대상은 아이콘이 아니라 버튼이다.
    private ILocator OpenButton => _page.Locator("#react-burger-menu-btn");
    private ILocator LogoutLink => _page.Locator("[data-test=\"logout-sidebar-link\"]");
    private ILocator ResetLink => _page.Locator("[data-test=\"reset-sidebar-link\"]");

    public async Task LogoutAsync()
    {
        await OpenButton.ClickAsync();
        await LogoutLink.ClickAsync();
    }

    public async Task ResetAppStateAsync()
    {
        await OpenButton.ClickAsync();
        await ResetLink.ClickAsync();
    }
}
