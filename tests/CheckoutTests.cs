using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class CheckoutTests : BaseTest
{
    private async Task GoToCheckoutInfoAsync(params string[] products)
    {
        var inventory = await LoginAndOpenInventoryAsync();
        foreach (var name in products)
            await inventory.AddToCartAsync(name);

        await new CartBadge(Page).OpenCartAsync();
        await new CartPage(Page).GotoCheckoutAsync();
    }

    [Fact]
    public async Task 배송정보를_비우고_계속하면_이름_필수_오류가_표시된다()
    {
        await GoToCheckoutInfoAsync(TestData.Backpack);

        var info = new CheckoutInfoPage(Page);
        await info.ContinueAsync();

        await Expect(info.ErrorMessage).ToHaveTextAsync("Error: First Name is required");
        await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/checkout-step-one.html");
    }

    [Fact]
    public async Task 합계는_상품_정가_합산에_세율_8퍼센트를_적용한_값과_일치한다()
    {
        // 기대값은 화면에서 읽지 않고 테스트가 독립적으로 계산한다. 정가는 TestData 상수.
        var expectedSubtotal = TestData.BackpackPrice + TestData.BikeLightPrice;
        var expectedTax = Math.Round(
            expectedSubtotal * TestData.TaxRate, 2, MidpointRounding.AwayFromZero);
        var expectedTotal = expectedSubtotal + expectedTax;

        await GoToCheckoutInfoAsync(TestData.Backpack, TestData.BikeLight);

        var info = new CheckoutInfoPage(Page);
        await info.FillAsync(TestData.FirstName, TestData.LastName, TestData.PostalCode);
        await info.ContinueAsync();

        var overview = new CheckoutOverviewPage(Page);
        Assert.Equal(expectedSubtotal, await overview.SubtotalAsync());
        Assert.Equal(expectedTax, await overview.TaxAsync());
        Assert.Equal(expectedTotal, await overview.TotalAsync());
    }

    [Fact]
    public async Task 주문을_확정하면_완료_메시지가_표시된다()
    {
        await GoToCheckoutInfoAsync(TestData.Backpack);

        var info = new CheckoutInfoPage(Page);
        await info.FillAsync(TestData.FirstName, TestData.LastName, TestData.PostalCode);
        await info.ContinueAsync();
        await new CheckoutOverviewPage(Page).FinishAsync();

        await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/checkout-complete.html");
        await Expect(new CheckoutCompletePage(Page).Header)
            .ToHaveTextAsync("Thank you for your order!");
    }
}
