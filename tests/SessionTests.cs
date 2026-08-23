using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class SessionTests : BaseTest
{
    [Fact]
    public async Task 로그아웃하면_세션이_무효화되어_직접_접근이_차단된다()
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);
        await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory\.html$"));

        await new HeaderMenu(Page).LogoutAsync();
        await Expect(login.LoginButton).ToBeVisibleAsync();

        await Page.GotoAsync("/inventory.html");
        await Expect(login.ErrorMessage).ToHaveTextAsync(
            "Epic sadface: You can only access '/inventory.html' when you are logged in.");
    }
}
