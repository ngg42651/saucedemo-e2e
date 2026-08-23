using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class InventoryTests : BaseTest
{
    [Fact]
    public async Task 이름_오름차순_정렬시_상품명이_사전순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("az");

        var names = await inventory.ProductNamesAsync();
        Assert.Equal(6, names.Count);
        Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task 이름_내림차순_정렬시_상품명이_역사전순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("za");

        var names = await inventory.ProductNamesAsync();
        Assert.Equal(6, names.Count);
        Assert.Equal(names.OrderByDescending(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task 가격_낮은순_정렬시_가격이_오름차순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("lohi");

        var prices = await inventory.ProductPricesAsync();
        Assert.Equal(6, prices.Count);
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task 가격_높은순_정렬시_가격이_내림차순으로_배열된다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.SortByAsync("hilo");

        var prices = await inventory.ProductPricesAsync();
        Assert.Equal(6, prices.Count);
        Assert.Equal(prices.OrderByDescending(p => p).ToList(), prices);
    }

    [Fact]
    public async Task 상품_상세로_진입했다_목록으로_복귀할_수_있다()
    {
        var inventory = await LoginAndOpenInventoryAsync();
        await inventory.OpenProductAsync(TestData.Backpack);

        var detail = new ProductDetailPage(Page);
        await Expect(detail.Name).ToHaveTextAsync(TestData.Backpack);
        await Expect(detail.Price).ToHaveTextAsync("$29.99");

        await detail.BackToProductsAsync();
        await Expect(inventory.Items).ToHaveCountAsync(6);
    }

    [Fact]
    public async Task problem_user는_모든_상품_이미지가_404_플레이스홀더로_깨진다()
    {
        var standard = await LoginAndOpenInventoryAsync(TestData.StandardUser);
        var healthy = await standard.ImageSourcesAsync();
        Assert.Equal(6, healthy.Distinct().Count());
        Assert.DoesNotContain(healthy, src => src.Contains("sl-404"));

        await new HeaderMenu(Page).LogoutAsync();

        var problem = await LoginAndOpenInventoryAsync(TestData.ProblemUser);
        var broken = await problem.ImageSourcesAsync();
        Assert.Equal(6, broken.Count);
        Assert.All(broken, src => Assert.Contains("sl-404", src));
    }
}
