using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Selenium.Support;

/// <summary>
/// Selenium 테스트의 드라이버 수명주기를 담당한다.
/// 주력 스위트(Playwright)와 같은 규칙을 따른다 — Thread.Sleep 대신 명시적 대기만 쓴다.
/// </summary>
public abstract class BaseTest : IDisposable
{
    protected const string BaseUrl = "https://www.saucedemo.com";

    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }

    protected BaseTest()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1280,900");

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
        GC.SuppressFinalize(this);
    }
}
