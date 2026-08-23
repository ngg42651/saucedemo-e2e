using System.Globalization;
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CheckoutOverviewPage(IPage page)
{
    private readonly IPage _page = page;

    public Task<decimal> SubtotalAsync() => AmountAsync("subtotal-label");
    public Task<decimal> TaxAsync() => AmountAsync("tax-label");
    public Task<decimal> TotalAsync() => AmountAsync("total-label");

    public Task FinishAsync() =>
        _page.Locator("[data-test=\"finish\"]").ClickAsync();

    /// <summary>"Item total: $39.98" 같은 라벨에서 금액만 뽑는다.</summary>
    private async Task<decimal> AmountAsync(string dataTest)
    {
        var text = await _page.Locator($"[data-test=\"{dataTest}\"]").InnerTextAsync();
        var amount = text[(text.IndexOf('$') + 1)..].Trim();
        return decimal.Parse(amount, CultureInfo.InvariantCulture);
    }
}
