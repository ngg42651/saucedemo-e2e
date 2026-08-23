using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class InventoryTests : BaseTest
{
    private async Task<InventoryPage> LoginAndOpenInventoryAsync(string user = TestData.StandardUser)
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(user, TestData.Password);
        var inventory = new InventoryPage(Page);
        await Expect(inventory.Items).ToHaveCountAsync(6);
        return inventory;
    }

    [Fact]
    public async Task 이름_오름차순_정렬시_상품명이_사전순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("az");

        var names = await inventory.ProductNamesAsync();
        Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task 이름_내림차순_정렬시_상품명이_역사전순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("za");

        var names = await inventory.ProductNamesAsync();
        Assert.Equal(names.OrderByDescending(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task 가격_낮은순_정렬시_가격이_오름차순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("lohi");

        var prices = await inventory.ProductPricesAsync();
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task 가격_높은순_정렬시_가격이_내림차순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("hilo");

        var prices = await inventory.ProductPricesAsync();
        Assert.Equal(prices.OrderByDescending(p => p).ToList(), prices);
    }
}
