using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class LoginPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator UsernameInput => _page.Locator("[data-test=\"username\"]");
    public ILocator PasswordInput => _page.Locator("[data-test=\"password\"]");
    public ILocator LoginButton => _page.Locator("[data-test=\"login-button\"]");
    public ILocator ErrorMessage => _page.Locator("[data-test=\"error\"]");

    public Task GotoAsync() => _page.GotoAsync("/");

    public async Task LoginAsync(string user, string password)
    {
        await UsernameInput.FillAsync(user);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
