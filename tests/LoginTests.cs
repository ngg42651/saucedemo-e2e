using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class LoginTests : BaseTest
{
    [Fact]
    public async Task 정상_계정으로_로그인하면_상품_목록으로_이동한다()
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);

        await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory\.html$"));
        await Expect(new InventoryPage(Page).Title).ToHaveTextAsync("Products");
    }
}
