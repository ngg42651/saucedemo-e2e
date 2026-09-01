using SauceDemo.E2E.Support;
using SauceDemo.Selenium.Pages;
using SauceDemo.Selenium.Support;

namespace SauceDemo.Selenium.Tests;

/// <summary>
/// 주력 스위트(Playwright)의 <c>LoginTests</c> 두 케이스를 Selenium C#으로 재구현한 것이다.
/// 커버리지를 늘리려는 목적이 아니라 같은 시나리오를 두 도구로 써 보고 주력을 고른 근거를 남기려는 것이다.
/// 판단 근거는 README "왜 주력은 Playwright인가" 참고.
/// </summary>
public class LoginTests : BaseTest
{
    [Fact]
    public void 정상_계정으로_로그인하면_상품_목록으로_이동한다()
    {
        var login = new LoginPage(Driver, Wait);
        login.Goto();
        login.Login(TestData.StandardUser, TestData.Password);

        login.WaitForUrlContains("/inventory.html");
        Assert.EndsWith("/inventory.html", Driver.Url);
    }

    [Fact]
    public void 잠긴_계정으로_로그인하면_지정된_오류_메시지가_표시된다()
    {
        var login = new LoginPage(Driver, Wait);
        login.Goto();
        login.Login(TestData.LockedOutUser, TestData.Password);

        Assert.Equal("Epic sadface: Sorry, this user has been locked out.", login.WaitForErrorText());
        Assert.Equal($"{BaseUrl}/", Driver.Url);
    }
}
