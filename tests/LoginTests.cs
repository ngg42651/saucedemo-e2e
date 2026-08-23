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

    [Theory]
    [InlineData(TestData.LockedOutUser, TestData.Password,
        "Epic sadface: Sorry, this user has been locked out.")]
    [InlineData(TestData.StandardUser, "wrong_password",
        "Epic sadface: Username and password do not match any user in this service")]
    [InlineData("", TestData.Password,
        "Epic sadface: Username is required")]
    [InlineData(TestData.StandardUser, "",
        "Epic sadface: Password is required")]
    public async Task 로그인_실패시_지정된_오류_메시지가_표시된다(
        string user, string password, string expectedMessage)
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(user, password);

        await Expect(login.ErrorMessage).ToHaveTextAsync(expectedMessage);
        await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/");
    }

    [Fact]
    public async Task 로그인하지_않고_상품_목록에_직접_접근하면_차단된다()
    {
        var login = new LoginPage(Page);
        await Page.GotoAsync("/inventory.html");

        await Expect(login.ErrorMessage).ToHaveTextAsync(
            "Epic sadface: You can only access '/inventory.html' when you are logged in.");
        await Expect(login.LoginButton).ToBeVisibleAsync();
    }
}
