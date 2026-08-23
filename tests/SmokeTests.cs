using Microsoft.Playwright;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class SmokeTests : BaseTest
{
    [Fact]
    public async Task 로그인_페이지가_열린다()
    {
        await Page.GotoAsync("/");
        await Expect(Page.Locator("[data-test=\"login-button\"]")).ToBeVisibleAsync();
    }
}
