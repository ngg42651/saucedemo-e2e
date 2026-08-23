using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class CartTests : BaseTest
{
    [Fact]
    public async Task 상품을_담으면_배지_수가_담은_개수만큼_증가한다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        var badge = new CartBadge(Page);
        Assert.Equal(0, await badge.CountAsync());

        await inventory.AddToCartAsync(TestData.Backpack);
        await Expect(badge.Badge).ToHaveTextAsync("1");

        await inventory.AddToCartAsync(TestData.BikeLight);
        await Expect(badge.Badge).ToHaveTextAsync("2");
    }

    [Fact]
    public async Task 상품을_빼면_배지_수가_감소하고_뺀_상품만_사라진다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        var badge = new CartBadge(Page);
        await inventory.AddToCartAsync(TestData.Backpack);
        await inventory.AddToCartAsync(TestData.BikeLight);
        await Expect(badge.Badge).ToHaveTextAsync("2");

        await inventory.RemoveFromCartAsync(TestData.Backpack);
        await Expect(badge.Badge).ToHaveTextAsync("1");

        await badge.OpenCartAsync();
        var names = await new CartPage(Page).ItemNamesAsync();
        Assert.Equal(new[] { TestData.BikeLight }, names);
    }

    [Fact]
    public async Task 장바구니_페이지의_품목이_담은_상품과_정확히_일치한다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        var expected = new[] { TestData.Backpack, TestData.FleeceJacket };
        foreach (var name in expected)
            await inventory.AddToCartAsync(name);

        await new CartBadge(Page).OpenCartAsync();
        var actual = await new CartPage(Page).ItemNamesAsync();

        Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
    }

    [Fact]
    public async Task Reset_App_State로_장바구니가_비워진다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        var badge = new CartBadge(Page);
        await inventory.AddToCartAsync(TestData.Backpack);
        await Expect(badge.Badge).ToHaveTextAsync("1");

        await new HeaderMenu(Page).ResetAppStateAsync();

        await Expect(badge.Badge).ToHaveCountAsync(0);
        await badge.OpenCartAsync();
        await Expect(new CartPage(Page).Items).ToHaveCountAsync(0);
    }
}
