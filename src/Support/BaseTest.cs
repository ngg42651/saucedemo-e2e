using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SauceDemo.E2E.Support;

public abstract class BaseTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = "https://www.saucedemo.com",
        ViewportSize = new() { Width = 1280, Height = 900 },
    };
}
