using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CheckoutInfoPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator ErrorMessage => _page.Locator("[data-test=\"error\"]");

    private ILocator FirstName => _page.Locator("[data-test=\"firstName\"]");
    private ILocator LastName => _page.Locator("[data-test=\"lastName\"]");
    private ILocator PostalCode => _page.Locator("[data-test=\"postalCode\"]");

    public async Task FillAsync(string first, string last, string postal)
    {
        await FirstName.FillAsync(first);
        await LastName.FillAsync(last);
        await PostalCode.FillAsync(postal);
    }

    public Task ContinueAsync() =>
        _page.Locator("[data-test=\"continue\"]").ClickAsync();
}
