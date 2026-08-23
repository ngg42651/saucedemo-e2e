using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using SauceDemo.E2E.Pages;

namespace SauceDemo.E2E.Support;

public abstract class BaseTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = "https://www.saucedemo.com",
        ViewportSize = new() { Width = 1280, Height = 900 },
    };

    protected async Task<InventoryPage> LoginAndOpenInventoryAsync(string user = TestData.StandardUser)
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(user, TestData.Password);
        var inventory = new InventoryPage(Page);
        await Expect(inventory.Items).ToHaveCountAsync(6);
        return inventory;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
    }

    public override async Task DisposeAsync()
    {
        var path = Path.Combine("playwright-traces", $"{GetType().Name}-{Guid.NewGuid():N}.zip");
        Directory.CreateDirectory("playwright-traces");
        await Context.Tracing.StopAsync(new() { Path = path });
        await base.DisposeAsync();
    }
}
