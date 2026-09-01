using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Selenium.Pages;

/// <summary>
/// 주력 스위트의 <c>src/Pages/LoginPage.cs</c>와 같은 규칙을 따른다 —
/// 셀렉터는 이 클래스 안에만 있고, Page 클래스는 assert하지 않는다.
/// </summary>
public class LoginPage(IWebDriver driver, WebDriverWait wait)
{
    private readonly IWebDriver _driver = driver;
    private readonly WebDriverWait _wait = wait;

    private static readonly By Username = By.CssSelector("[data-test=\"username\"]");
    private static readonly By Password = By.CssSelector("[data-test=\"password\"]");
    private static readonly By LoginButton = By.CssSelector("[data-test=\"login-button\"]");
    private static readonly By Error = By.CssSelector("[data-test=\"error\"]");

    public void Goto() => _driver.Navigate().GoToUrl("https://www.saucedemo.com/");

    public void Login(string user, string password)
    {
        _wait.Until(d => d.FindElement(Username)).SendKeys(user);
        _driver.FindElement(Password).SendKeys(password);
        _driver.FindElement(LoginButton).Click();
    }

    /// <summary>오류 배너가 나타날 때까지 기다린 뒤 그 문구를 돌려준다.</summary>
    public string WaitForErrorText() => _wait.Until(d => d.FindElement(Error)).Text;

    /// <summary>URL이 지정한 조각을 포함할 때까지 기다린다. 대기만 하고 판정은 테스트가 한다.</summary>
    public void WaitForUrlContains(string fragment) => _wait.Until(d => d.Url.Contains(fragment));
}
