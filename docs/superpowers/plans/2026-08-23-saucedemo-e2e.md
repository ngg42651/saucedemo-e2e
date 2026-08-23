# SauceDemo E2E 자동화 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** SauceDemo를 대상으로 Page Object Model 구조의 Playwright .NET E2E 테스트 20개와 GitHub Actions CI를 갖춘 포트폴리오 저장소를 완성한다.

**Architecture:** `Microsoft.Playwright.Xunit`의 `PageTest`를 상속한 테스트 클래스가 `Pages/`의 Page Object를 호출한다. Page는 조작·조회만 하고 검증은 테스트가 한다. 모든 셀렉터는 Page 클래스 내부에만 존재한다. `PageTest`가 테스트마다 새 `BrowserContext`를 주므로 장바구니 상태는 자동 격리되고 병렬 실행이 안전하다.

**Tech Stack:** .NET 10 (`net10.0`), xUnit 2.9.3, Microsoft.Playwright.Xunit, Chromium, GitHub Actions

**Spec:** `docs/superpowers/specs/2026-08-23-saucedemo-e2e-design.md`

## Global Constraints

- 타깃 프레임워크는 `net10.0`. 작업 머신 SDK가 10.0.302 단일이다.
- xUnit은 v2 계열(2.9.3). `dotnet new xunit` 템플릿 기본값이며 `Microsoft.Playwright.Xunit` 패키지(v3 아님)와 짝이다.
- `Thread.Sleep`을 쓰지 않는다. Playwright 자동 대기와 `Expect()`만 쓴다.
- Page 클래스는 assert하지 않는다. 조작과 조회만 한다.
- 셀렉터 문자열은 Page 클래스 안에만 존재한다. 테스트 파일에 CSS·XPath를 두지 않는다.
- 테스트를 통과시키려고 assert를 약화시키지 않는다. 통과 못 하면 통과 못 했다고 보고한다.
- 재시도 설정을 넣지 않는다.
- 모든 Playwright 호출은 `await`한다.

## 확인된 사실 (2026-08-23 실제 사이트 확인)

이 계획의 셀렉터와 문자열은 실제 https://www.saucedemo.com 을 브라우저로 열어 DOM에서 추출한 것이다. 추측이 아니다.

**계정** — 비밀번호는 전부 `secret_sauce`. `standard_user`, `locked_out_user`, `problem_user`, `performance_glitch_user`, `error_user`, `visual_user`.

**로그인 페이지** (`/`)

| 요소 | 셀렉터 |
|---|---|
| 아이디 입력 | `[data-test="username"]` |
| 비밀번호 입력 | `[data-test="password"]` |
| 로그인 버튼 | `[data-test="login-button"]` |
| 오류 메시지 | `[data-test="error"]` |

**오류 메시지 원문**

| 상황 | 문자열 |
|---|---|
| 잘못된 자격증명 | `Epic sadface: Username and password do not match any user in this service` |
| 잠긴 계정 | `Epic sadface: Sorry, this user has been locked out.` |
| 아이디 빈칸 | `Epic sadface: Username is required` |
| 비밀번호 빈칸 (아이디는 입력됨) | `Epic sadface: Password is required` |
| 미로그인 직접 접근 | `Epic sadface: You can only access '/inventory.html' when you are logged in.` |

**상품 목록** (`/inventory.html`)

| 요소 | 셀렉터 |
|---|---|
| 정렬 드롭다운 | `[data-test="product-sort-container"]` (옵션 값 `az`, `za`, `lohi`, `hilo`) |
| 상품 카드 | `[data-test="inventory-item"]` |
| 상품명 | `[data-test="inventory-item-name"]` |
| 상품가격 | `[data-test="inventory-item-price"]` (예: `$29.99`) |
| 담기 버튼 | `[data-test="add-to-cart-sauce-labs-backpack"]` 형식 (상품명 슬러그) |
| 빼기 버튼 | `[data-test="remove-sauce-labs-backpack"]` 형식 |
| 장바구니 배지 | `[data-test="shopping-cart-badge"]` |
| 장바구니 링크 | `[data-test="shopping-cart-link"]` |
| 햄버거 메뉴 | `[data-test="open-menu"]` |
| 로그아웃 링크 | `[data-test="logout-sidebar-link"]` |
| Reset App State | `[data-test="reset-sidebar-link"]` |
| 상품 이미지 | CSS `.inventory_item_img img` |

상품 6종과 정가: Sauce Labs Backpack `$29.99`, Sauce Labs Bike Light `$9.99`, Sauce Labs Bolt T-Shirt `$15.99`, Sauce Labs Fleece Jacket `$49.99`, Sauce Labs Onesie `$7.99`, Test.allTheThings() T-Shirt (Red) `$15.99`.

**이미지 결함**: `standard_user`로 로그인하면 상품 이미지 `src`가 6개 모두 다르다. `problem_user`로 로그인하면 6개 모두 `/assets/sl-404-<해시>.jpg`가 된다. 해시는 빌드마다 바뀌므로 `sl-404` 부분 문자열로 판정한다.

**장바구니** (`/cart.html`): 체크아웃 버튼 `[data-test="checkout"]`, 쇼핑 계속 `[data-test="continue-shopping"]`, 품목명은 목록과 같은 `[data-test="inventory-item-name"]`.

**체크아웃 1단계** (`/checkout-step-one.html`): `[data-test="firstName"]`, `[data-test="lastName"]`, `[data-test="postalCode"]`, 계속 `[data-test="continue"]`. 빈칸 제출 시 `[data-test="error"]`에 `Error: First Name is required`.

**체크아웃 2단계** (`/checkout-step-two.html`): `[data-test="subtotal-label"]` (`Item total: $39.98`), `[data-test="tax-label"]` (`Tax: $3.20`), `[data-test="total-label"]` (`Total: $43.18`), 완료 `[data-test="finish"]`.

세율은 8%다. 실측 검증: 소계 39.98 × 0.08 = 3.1984, 소수 둘째 자리 반올림하면 3.20. 합계 39.98 + 3.20 = 43.18. 화면 값과 일치한다.

**주문 완료** (`/checkout-complete.html`): `[data-test="complete-header"]`에 `Thank you for your order!`, 목록 복귀 `[data-test="back-to-products"]`.

## File Structure

| 파일 | 책임 |
|---|---|
| `SauceDemo.E2E.sln` | 솔루션 |
| `SauceDemo.E2E.csproj` | 단일 프로젝트. Page와 테스트를 한 프로젝트에 둔다 (둘로 나눌 만큼 크지 않다) |
| `src/Support/TestData.cs` | 계정 상수, 상품 상수, 체크아웃 입력값, 세율 |
| `src/Support/BaseTest.cs` | `PageTest` 상속, `BaseURL` 설정 |
| `src/Pages/LoginPage.cs` | 로그인 폼 조작, 오류 메시지 조회 |
| `src/Pages/InventoryPage.cs` | 정렬, 담기/빼기, 상품명·가격·이미지 조회 |
| `src/Pages/ProductDetailPage.cs` | 상품 상세 조회, 목록 복귀 |
| `src/Pages/CartPage.cs` | 장바구니 품목 조회, 체크아웃 진입 |
| `src/Pages/CheckoutInfoPage.cs` | 체크아웃 1단계 입력 |
| `src/Pages/CheckoutOverviewPage.cs` | 소계·세금·합계 조회, 주문 확정 |
| `src/Pages/CheckoutCompletePage.cs` | 완료 메시지 조회 |
| `src/Components/HeaderMenu.cs` | 햄버거 메뉴, 로그아웃, Reset App State |
| `src/Components/CartBadge.cs` | 배지 개수 조회, 장바구니 이동 |
| `tests/LoginTests.cs` | 로그인 6케이스 |
| `tests/InventoryTests.cs` | 목록 6케이스 |
| `tests/CartTests.cs` | 장바구니 4케이스 |
| `tests/CheckoutTests.cs` | 체크아웃 3케이스 |
| `tests/SessionTests.cs` | 로그아웃 1케이스 |
| `.runsettings` | Playwright 실행 설정 (headless, chromium) |
| `.github/workflows/ci.yml` | CI |
| `CLAUDE.md` | 프로젝트 지침 |
| `README.md` | 테스트 전략 |

`src/`와 `tests/`는 같은 csproj에 속한다. csproj는 리포지토리 루트에 두고 두 폴더를 모두 컴파일한다.

---

## Task 1: 스캐폴드와 스모크 테스트 (M0)

**Files:**
- Create: `SauceDemo.E2E.csproj`, `SauceDemo.E2E.sln`, `.gitignore`, `CLAUDE.md`
- Create: `src/Support/BaseTest.cs`, `src/Support/TestData.cs`
- Test: `tests/SmokeTests.cs`

**Interfaces:**
- Consumes: 없음 (첫 태스크)
- Produces: `BaseTest` (추상 클래스, `PageTest` 상속, `ContextOptions()`에서 `BaseURL = "https://www.saucedemo.com"` 설정). `TestData` 정적 클래스: `StandardUser`, `LockedOutUser`, `ProblemUser`, `Password`, `FirstName`, `LastName`, `PostalCode`, `Backpack`, `BikeLight`, `FleeceJacket` (모두 `const string`), `TaxRate` (`const decimal`).

- [ ] **Step 1: 프로젝트 생성**

```bash
cd /d/Projects/saucedemo-playwright-dotnet
dotnet new xunit -n SauceDemo.E2E -o . --force
rm -f UnitTest1.cs
dotnet new sln -n SauceDemo.E2E --force
dotnet sln add SauceDemo.E2E.csproj
dotnet new gitignore
```

- [ ] **Step 2: Playwright 패키지 추가**

```bash
dotnet add package Microsoft.Playwright.Xunit
dotnet build
```

`dotnet build`가 성공해야 다음 단계의 `playwright.ps1`이 생성된다.

- [ ] **Step 3: 브라우저 설치**

```bash
pwsh bin/Debug/net10.0/playwright.ps1 install chromium --with-deps
```

`pwsh`가 없으면 `dotnet tool install --global PowerShell` 후 재시도한다. Chromium만 설치한다 — 스펙 8장에서 브라우저 매트릭스를 만들지 않기로 했다.

- [ ] **Step 4: `src/Support/TestData.cs` 작성**

```csharp
namespace SauceDemo.E2E.Support;

public static class TestData
{
    public const string StandardUser = "standard_user";
    public const string LockedOutUser = "locked_out_user";
    public const string ProblemUser = "problem_user";
    public const string Password = "secret_sauce";

    public const string FirstName = "Hong";
    public const string LastName = "GilDong";
    public const string PostalCode = "12345";

    public const string Backpack = "Sauce Labs Backpack";
    public const string BikeLight = "Sauce Labs Bike Light";
    public const string FleeceJacket = "Sauce Labs Fleece Jacket";

    /// <summary>SauceDemo 체크아웃 세율. 소계에 곱한 뒤 소수 둘째 자리에서 반올림한다.</summary>
    public const decimal TaxRate = 0.08m;
}
```

- [ ] **Step 5: `src/Support/BaseTest.cs` 작성**

```csharp
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SauceDemo.E2E.Support;

public abstract class BaseTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = "https://www.saucedemo.com",
        ViewportSize = new() { Width = 1280, Height = 900 },
    };
}
```

- [ ] **Step 6: 스모크 테스트 작성**

`tests/SmokeTests.cs`:

```csharp
using Microsoft.Playwright;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class SmokeTests : BaseTest
{
    [Fact]
    public async Task 로그인_페이지가_열린다()
    {
        await Page.GotoAsync("/");
        await Expect(Page.Locator("[data-test=\"login-button\"]")).ToBeVisibleAsync();
    }
}
```

- [ ] **Step 7: 테스트 실행**

```bash
dotnet test --filter "FullyQualifiedName~SmokeTests"
```

Expected: PASS 1개. 이 테스트는 TDD의 red 단계가 아니라 환경 검증용이므로 처음부터 통과해야 정상이다. 실패하면 브라우저 설치(Step 3)나 네트워크를 먼저 확인한다.

- [ ] **Step 8: `CLAUDE.md` 작성**

```markdown
# saucedemo-playwright-dotnet

SauceDemo(https://www.saucedemo.com) 대상 E2E 자동화 포트폴리오.

## 명령어
- 전체 테스트: `dotnet test`
- 단일 클래스: `dotnet test --filter "FullyQualifiedName~LoginTests"`
- 브라우저 설치: `pwsh bin/Debug/net10.0/playwright.ps1 install chromium`

## 불변 규칙
1. `Thread.Sleep`을 쓰지 않는다. Playwright 자동 대기와 `Expect()`만 쓴다
2. Page 클래스는 assert하지 않는다. 조작과 조회만 한다
3. 셀렉터는 Page 클래스 안에만 존재한다. 테스트 파일에 CSS·XPath 문자열 금지
4. 테스트를 통과시키려고 assert를 약화시키지 않는다. 통과 못 하면 통과 못 했다고 보고한다
5. 모든 Playwright 호출은 `await`한다

## 규칙 추가 방침
같은 지적이 두 번 나오면 여기에 규칙으로 올린다. 미리 상상해서 쓰지 않는다.
```

- [ ] **Step 9: 커밋**

```bash
git add -A
git commit -m "chore: 프로젝트 스캐폴드와 스모크 테스트"
```

---

## Task 2: LoginPage와 로그인 성공 (M1)

**Files:**
- Create: `src/Pages/LoginPage.cs`, `src/Pages/InventoryPage.cs`
- Create: `tests/LoginTests.cs`
- Delete: `tests/SmokeTests.cs`

**Interfaces:**
- Consumes: `BaseTest`, `TestData` (Task 1)
- Produces:
  - `LoginPage(IPage page)` — `Task GotoAsync()`, `Task LoginAsync(string user, string password)`, `ILocator UsernameInput/PasswordInput/LoginButton/ErrorMessage { get; }`
  - `InventoryPage(IPage page)` — `ILocator Title { get; }` (이 태스크에서는 제목만. 나머지는 Task 6에서 추가)

- [ ] **Step 1: 실패하는 테스트 작성**

`tests/LoginTests.cs`:

```csharp
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class LoginTests : BaseTest
{
    [Fact]
    public async Task 정상_계정으로_로그인하면_상품_목록으로_이동한다()
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);

        await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory\.html$"));
        await Expect(new InventoryPage(Page).Title).ToHaveTextAsync("Products");
    }
}
```

- [ ] **Step 2: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~LoginTests"
```

Expected: 컴파일 실패. `LoginPage`, `InventoryPage`가 존재하지 않는다.

- [ ] **Step 3: `src/Pages/LoginPage.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class LoginPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator UsernameInput => _page.Locator("[data-test=\"username\"]");
    public ILocator PasswordInput => _page.Locator("[data-test=\"password\"]");
    public ILocator LoginButton => _page.Locator("[data-test=\"login-button\"]");
    public ILocator ErrorMessage => _page.Locator("[data-test=\"error\"]");

    public Task GotoAsync() => _page.GotoAsync("/");

    public async Task LoginAsync(string user, string password)
    {
        await UsernameInput.FillAsync(user);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
```

`FillAsync`에 빈 문자열을 넣어도 정상 동작하므로 빈칸 케이스도 같은 메서드로 처리한다.

- [ ] **Step 4: `src/Pages/InventoryPage.cs` 최소 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class InventoryPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Title => _page.Locator("[data-test=\"title\"]");
}
```

- [ ] **Step 5: 스모크 테스트 삭제**

```bash
rm tests/SmokeTests.cs
```

역할이 로그인 성공 테스트로 대체됐다. 남겨두면 같은 것을 두 번 검증한다.

- [ ] **Step 6: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~LoginTests"
```

Expected: PASS 1개.

- [ ] **Step 7: 커밋**

```bash
git add -A
git commit -m "feat: LoginPage와 로그인 성공 테스트"
```

---

## Task 3: 로그인 실패 5케이스 (M1 완료)

**Files:**
- Modify: `tests/LoginTests.cs`

**Interfaces:**
- Consumes: `LoginPage`, `TestData` (Task 2)
- Produces: 없음 (테스트만 추가)

- [ ] **Step 1: 테스트 5개 추가**

`tests/LoginTests.cs`의 클래스 안에 아래를 추가한다.

```csharp
    [Theory]
    [InlineData(TestData.LockedOutUser, TestData.Password,
        "Epic sadface: Sorry, this user has been locked out.")]
    [InlineData(TestData.StandardUser, "wrong_password",
        "Epic sadface: Username and password do not match any user in this service")]
    [InlineData("", TestData.Password,
        "Epic sadface: Username is required")]
    [InlineData(TestData.StandardUser, "",
        "Epic sadface: Password is required")]
    public async Task 로그인_실패시_지정된_오류_메시지가_표시된다(
        string user, string password, string expectedMessage)
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(user, password);

        await Expect(login.ErrorMessage).ToHaveTextAsync(expectedMessage);
        await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/");
    }

    [Fact]
    public async Task 로그인하지_않고_상품_목록에_직접_접근하면_차단된다()
    {
        var login = new LoginPage(Page);
        await Page.GotoAsync("/inventory.html");

        await Expect(login.ErrorMessage).ToHaveTextAsync(
            "Epic sadface: You can only access '/inventory.html' when you are logged in.");
        await Expect(login.LoginButton).ToBeVisibleAsync();
    }
```

URL을 함께 검증하는 이유: 오류 메시지만 확인하면 메시지가 뜬 채로 페이지가 넘어가는 경우를 놓친다.

- [ ] **Step 2: 실행**

```bash
dotnet test --filter "FullyQualifiedName~LoginTests" -v n
```

Expected: PASS 6개 (성공 1 + Theory 4 + 직접 접근 1).

Page 객체가 이미 존재하므로 red 단계 없이 통과하는 것이 정상이다. 실패하면 오류 메시지 문자열이 사이트에서 바뀐 것이므로 실제 사이트를 다시 확인해 문자열을 고친다 — assert를 지우지 않는다.

- [ ] **Step 3: 커밋**

```bash
git add -A
git commit -m "test: 로그인 실패 5케이스"
```

- [ ] **Step 4: 1단계 회고 — CLAUDE.md 갱신**

M1 구간에서 실제로 반복된 지적을 `CLAUDE.md`의 불변 규칙에 추가한다. 없으면 추가하지 않는다. 상상해서 쓰지 않는 것이 이 단계의 목적이다.

지적이 있었다면 커밋한다.

```bash
git add CLAUDE.md
git commit -m "docs: M1에서 관찰된 규칙 추가"
```

---

## Task 4: 리뷰 에이전트 도입 (2단계 시작)

**Files:**
- Create: `.claude/agents/reviewer.md`

**Interfaces:**
- Consumes: `CLAUDE.md`의 불변 규칙 (Task 1, Task 3)
- Produces: 없음 (도구 설정)

- [ ] **Step 1: 리뷰 에이전트 정의 작성**

`.claude/agents/reviewer.md`:

```markdown
---
name: reviewer
description: 구현이 끝난 diff를 리뷰한다. 버그와 규칙 위반만 지적한다
tools: Read, Grep, Bash
model: sonnet
---

`CLAUDE.md`의 불변 규칙을 기준으로 diff를 검토한다.
`git diff HEAD~1`로 변경분을 확인하고, 필요하면 주변 파일을 읽는다.

## 체크 순서 (곧 우선순위)

1. **assert 약화** — 이전 커밋 대비 검증이 느슨해졌는가. 삭제된 assert,
   범위가 넓어진 비교, 조건이 사라진 검증을 찾는다. 이것이 최우선이다.
2. **불변 규칙 위반** — `CLAUDE.md`에 적힌 규칙 위반
3. **책임 경계** — Page 클래스가 assert하는가, 테스트 파일에 셀렉터가 있는가
4. **중복** — 같은 셀렉터나 같은 대기 로직이 두 곳 이상에 있는가

## 출력

`path:line: [심각도] 문제. 수정안.` 한 줄씩.
심각도는 critical / major / minor.

발견한 것이 없으면 "지적 없음" 한 줄만 출력한다.

## 금지

- 칭찬하지 않는다
- 요청 범위 밖의 개선을 제안하지 않는다
- 확신이 없으면 단정하지 말고 "확인 필요"로 표시한다
```

- [ ] **Step 2: 직전 커밋에 대해 시험 실행**

Task 3의 커밋을 대상으로 리뷰 에이전트를 한 번 돌린다. 지적이 타당한지 사람이 판단한다. 지적이 전부 무의미하면 체크 항목을 조정한 뒤 다시 돌린다.

- [ ] **Step 3: 커밋**

```bash
git add .claude/agents/reviewer.md
git commit -m "chore: 리뷰 에이전트 정의 추가"
```

---

## Task 5: HeaderMenu, CartBadge, 로그아웃 (M2)

**Files:**
- Create: `src/Components/HeaderMenu.cs`, `src/Components/CartBadge.cs`
- Create: `tests/SessionTests.cs`

**Interfaces:**
- Consumes: `LoginPage`, `BaseTest`, `TestData`
- Produces:
  - `HeaderMenu(IPage page)` — `Task LogoutAsync()`, `Task ResetAppStateAsync()`
  - `CartBadge(IPage page)` — `Task<int> CountAsync()` (배지가 없으면 `0`), `Task OpenCartAsync()`, `ILocator Badge { get; }`

- [ ] **Step 1: 실패하는 테스트 작성**

`tests/SessionTests.cs`:

```csharp
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class SessionTests : BaseTest
{
    [Fact]
    public async Task 로그아웃하면_세션이_무효화되어_직접_접근이_차단된다()
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);
        await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory\.html$"));

        await new HeaderMenu(Page).LogoutAsync();
        await Expect(login.LoginButton).ToBeVisibleAsync();

        await Page.GotoAsync("/inventory.html");
        await Expect(login.ErrorMessage).ToHaveTextAsync(
            "Epic sadface: You can only access '/inventory.html' when you are logged in.");
    }
}
```

로그아웃 후 로그인 화면으로 돌아온 것만 확인하면 부족하다. 세션이 실제로 무효화됐는지는 직접 접근을 다시 시도해야 알 수 있다.

- [ ] **Step 2: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~SessionTests"
```

Expected: 컴파일 실패. `HeaderMenu`가 없다.

- [ ] **Step 3: `src/Components/HeaderMenu.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Components;

public class HeaderMenu(IPage page)
{
    private readonly IPage _page = page;

    private ILocator OpenButton => _page.Locator("[data-test=\"open-menu\"]");
    private ILocator LogoutLink => _page.Locator("[data-test=\"logout-sidebar-link\"]");
    private ILocator ResetLink => _page.Locator("[data-test=\"reset-sidebar-link\"]");

    public async Task LogoutAsync()
    {
        await OpenButton.ClickAsync();
        await LogoutLink.ClickAsync();
    }

    public async Task ResetAppStateAsync()
    {
        await OpenButton.ClickAsync();
        await ResetLink.ClickAsync();
    }
}
```

- [ ] **Step 4: `src/Components/CartBadge.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Components;

public class CartBadge(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Badge => _page.Locator("[data-test=\"shopping-cart-badge\"]");
    private ILocator CartLink => _page.Locator("[data-test=\"shopping-cart-link\"]");

    /// <summary>장바구니가 비면 배지 요소 자체가 사라지므로 0을 반환한다.</summary>
    public async Task<int> CountAsync()
    {
        if (await Badge.CountAsync() == 0) return 0;
        return int.Parse(await Badge.InnerTextAsync());
    }

    public Task OpenCartAsync() => CartLink.ClickAsync();
}
```

- [ ] **Step 5: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~SessionTests"
```

Expected: PASS 1개.

- [ ] **Step 6: 리뷰 에이전트 실행**

`git diff HEAD~1`을 대상으로 리뷰를 돌린다. critical·major 지적은 고치고 다시 테스트한다.

- [ ] **Step 7: 커밋**

```bash
git add -A
git commit -m "feat: HeaderMenu·CartBadge 컴포넌트와 로그아웃 세션 검증"
```

---

## Task 6: 정렬 4케이스 (M2)

**Files:**
- Modify: `src/Pages/InventoryPage.cs`
- Create: `tests/InventoryTests.cs`

**Interfaces:**
- Consumes: `InventoryPage` (Task 2의 최소 버전), `LoginPage`, `TestData`
- Produces: `InventoryPage`에 추가 —
  `ILocator Items { get; }`, `Task SortByAsync(string optionValue)`,
  `Task<IReadOnlyList<string>> ProductNamesAsync()`, `Task<IReadOnlyList<decimal>> ProductPricesAsync()`,
  `Task<IReadOnlyList<string>> ImageSourcesAsync()`, `Task AddToCartAsync(string productName)`,
  `Task RemoveFromCartAsync(string productName)`, `Task OpenProductAsync(string productName)`

- [ ] **Step 1: 실패하는 테스트 작성**

`tests/InventoryTests.cs`:

```csharp
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
```

기대값을 하드코딩하지 않고 정렬 여부를 계산으로 확인한다. 상품 목록이 바뀌어도 테스트가 살아남고, 무엇을 검증하는지가 코드에 그대로 드러난다.

`StringComparer.Ordinal`을 쓰는 이유: `Test.allTheThings() T-Shirt (Red)`처럼 특수문자로 시작하는 이름이 섞여 있어 문화권 기반 비교와 사이트의 정렬 결과가 어긋날 수 있다. 이 케이스가 실패하면 사이트의 실제 정렬 기준을 확인하고 비교자를 맞춘다 — assert를 지우지 않는다.

- [ ] **Step 2: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~InventoryTests"
```

Expected: 컴파일 실패. `SortByAsync`, `Items` 등이 없다.

- [ ] **Step 3: `src/Pages/InventoryPage.cs` 전체 교체**

```csharp
using System.Globalization;
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class InventoryPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Title => _page.Locator("[data-test=\"title\"]");
    public ILocator Items => _page.Locator("[data-test=\"inventory-item\"]");

    private ILocator SortDropdown => _page.Locator("[data-test=\"product-sort-container\"]");
    private ILocator Names => _page.Locator("[data-test=\"inventory-item-name\"]");
    private ILocator Prices => _page.Locator("[data-test=\"inventory-item-price\"]");
    private ILocator Images => _page.Locator(".inventory_item_img img");

    /// <param name="optionValue">az, za, lohi, hilo 중 하나</param>
    public Task SortByAsync(string optionValue) =>
        SortDropdown.SelectOptionAsync(optionValue);

    public async Task<IReadOnlyList<string>> ProductNamesAsync() =>
        await Names.AllInnerTextsAsync();

    public async Task<IReadOnlyList<decimal>> ProductPricesAsync()
    {
        var texts = await Prices.AllInnerTextsAsync();
        return texts
            .Select(t => decimal.Parse(t.TrimStart('$'), CultureInfo.InvariantCulture))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> ImageSourcesAsync()
    {
        var count = await Images.CountAsync();
        var sources = new List<string>(count);
        for (var i = 0; i < count; i++)
            sources.Add(await Images.Nth(i).GetAttributeAsync("src") ?? "");
        return sources;
    }

    public Task AddToCartAsync(string productName) =>
        _page.Locator($"[data-test=\"add-to-cart-{Slug(productName)}\"]").ClickAsync();

    public Task RemoveFromCartAsync(string productName) =>
        _page.Locator($"[data-test=\"remove-{Slug(productName)}\"]").ClickAsync();

    public Task OpenProductAsync(string productName) =>
        Names.GetByText(productName, new() { Exact = true }).ClickAsync();

    /// <summary>"Sauce Labs Backpack"을 "sauce-labs-backpack"으로 바꾼다. 사이트의 data-test 명명 규칙이다.</summary>
    private static string Slug(string productName) =>
        productName.ToLowerInvariant().Replace(' ', '-');
}
```

`Slug`가 사이트 규칙과 맞는지는 Task 8의 담기 테스트가 실제로 검증한다.

- [ ] **Step 4: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~InventoryTests"
```

Expected: PASS 4개.

- [ ] **Step 5: 리뷰 에이전트 실행 후 커밋**

```bash
git add -A
git commit -m "feat: InventoryPage 정렬·조회와 정렬 4케이스"
```

---

## Task 7: 상품 상세 진입과 problem_user 이미지 결함 (M2 완료)

**Files:**
- Create: `src/Pages/ProductDetailPage.cs`
- Modify: `tests/InventoryTests.cs`

**Interfaces:**
- Consumes: `InventoryPage.OpenProductAsync`, `InventoryPage.ImageSourcesAsync` (Task 6), `HeaderMenu.LogoutAsync` (Task 5)
- Produces: `ProductDetailPage(IPage page)` — `ILocator Name { get; }`, `ILocator Price { get; }`, `Task BackToProductsAsync()`

- [ ] **Step 1: 테스트 2개 추가**

`InventoryTests` 클래스 안에 추가한다.

```csharp
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
        Assert.All(broken, src => Assert.Contains("sl-404", src));
    }
```

두 번째 테스트가 `standard_user`를 먼저 확인하는 이유: `problem_user`의 이미지가 전부 `sl-404`라는 것만으로는 그것이 결함인지 원래 그런지 알 수 없다. 정상 계정과 대조해야 결함을 검출했다고 말할 수 있다. `Distinct().Count() == 6`은 정상 계정의 이미지가 상품마다 다르다는 확인이다.

- [ ] **Step 2: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~InventoryTests"
```

Expected: 컴파일 실패. `ProductDetailPage`가 없다.

- [ ] **Step 3: `src/Pages/ProductDetailPage.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class ProductDetailPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Name => _page.Locator("[data-test=\"inventory-item-name\"]");
    public ILocator Price => _page.Locator("[data-test=\"inventory-item-price\"]");

    public Task BackToProductsAsync() =>
        _page.Locator("[data-test=\"back-to-products\"]").ClickAsync();
}
```

- [ ] **Step 4: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~InventoryTests"
```

Expected: PASS 6개.

`back-to-products` 셀렉터는 주문 완료 페이지에서 확인된 것이다. 상품 상세 페이지가 다른 이름을 쓴다면 실패한다. 실패하면 실제 DOM을 열어 확인하고 셀렉터를 고친다 — 테스트를 지우지 않는다.

- [ ] **Step 5: 전체 테스트 실행**

```bash
dotnet test
```

Expected: PASS 13개 (로그인 6 + 목록 6 + 로그아웃 1).

- [ ] **Step 6: 리뷰 에이전트 실행 후 커밋**

```bash
git add -A
git commit -m "feat: 상품 상세 페이지와 problem_user 이미지 결함 검출"
```

- [ ] **Step 7: 2단계 회고**

리뷰 에이전트가 M2 구간에서 실제로 잡은 문제를 기록한다. 반복된 지적은 `CLAUDE.md` 규칙으로 승격한다. 잡은 것이 없으면 체크 항목이 현실과 안 맞는 것이므로 조정한다.

---

## Task 8: 장바구니 4케이스 (M3, 계획 게이트 적용)

**Files:**
- Create: `src/Pages/CartPage.cs`
- Create: `tests/CartTests.cs`

**Interfaces:**
- Consumes: `InventoryPage.AddToCartAsync`, `InventoryPage.RemoveFromCartAsync` (Task 6), `CartBadge`, `HeaderMenu.ResetAppStateAsync` (Task 5)
- Produces: `CartPage(IPage page)` — `ILocator Items { get; }`, `Task<IReadOnlyList<string>> ItemNamesAsync()`, `Task GotoCheckoutAsync()`

**이 태스크부터 계획과 구현을 분리한다.** 구현 전에 "무엇을 검증할 것인가"를 문장으로 먼저 적고 승인받은 뒤 코드를 쓴다. Step 1이 그 계획에 해당한다.

- [ ] **Step 1: 검증 대상 확정 (계획 게이트)**

네 케이스가 각각 무엇을 확인하는지 확정한다.

1. 담기 — 배지 수가 0에서 2로 증가한다. 담은 개수와 배지 숫자가 일치한다.
2. 빼기 — 배지 수가 2에서 1로 감소한다. 남은 상품이 뺀 것이 아닌 쪽이다.
3. 장바구니 품목 — 담은 상품명 집합과 장바구니 품목명 집합이 정확히 일치한다. 개수만 세지 않는다.
4. Reset App State — 배지가 사라지고(개수 0) 장바구니 페이지가 빈다.

3번에서 개수만 세면 다른 상품이 담겨도 통과한다. 집합 비교여야 한다.

- [ ] **Step 2: 실패하는 테스트 작성**

`tests/CartTests.cs`:

```csharp
using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class CartTests : BaseTest
{
    private async Task<InventoryPage> LoginAsync()
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);
        var inventory = new InventoryPage(Page);
        await Expect(inventory.Items).ToHaveCountAsync(6);
        return inventory;
    }

    [Fact]
    public async Task 상품을_담으면_배지_수가_담은_개수만큼_증가한다()
    {
        var inventory = await LoginAsync();
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
        var inventory = await LoginAsync();
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
        var inventory = await LoginAsync();
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
        var inventory = await LoginAsync();
        var badge = new CartBadge(Page);
        await inventory.AddToCartAsync(TestData.Backpack);
        await Expect(badge.Badge).ToHaveTextAsync("1");

        await new HeaderMenu(Page).ResetAppStateAsync();

        await Expect(badge.Badge).ToHaveCountAsync(0);
        await badge.OpenCartAsync();
        await Expect(new CartPage(Page).Items).ToHaveCountAsync(0);
    }
}
```

- [ ] **Step 3: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~CartTests"
```

Expected: 컴파일 실패. `CartPage`가 없다.

- [ ] **Step 4: `src/Pages/CartPage.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CartPage(IPage page)
{
    private readonly IPage _page = page;

    public ILocator Items => _page.Locator("[data-test=\"inventory-item\"]");
    private ILocator Names => _page.Locator("[data-test=\"inventory-item-name\"]");

    public async Task<IReadOnlyList<string>> ItemNamesAsync() =>
        await Names.AllInnerTextsAsync();

    public Task GotoCheckoutAsync() =>
        _page.Locator("[data-test=\"checkout\"]").ClickAsync();
}
```

- [ ] **Step 5: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~CartTests"
```

Expected: PASS 4개.

`Reset App State` 케이스가 실패하면 사이트가 배지만 지우고 장바구니 내용은 남기는 것이다. 그 경우 이는 실제 결함이므로 README에 기록하고 테스트를 실제 동작에 맞춰 조정하되, 무엇이 기대와 달랐는지 반드시 남긴다.

- [ ] **Step 6: 리뷰 에이전트 실행 후 커밋**

```bash
git add -A
git commit -m "feat: CartPage와 장바구니 4케이스"
```

---

## Task 9: 체크아웃 3케이스와 총액 계산 검증 (M3 완료)

**Files:**
- Create: `src/Pages/CheckoutInfoPage.cs`, `src/Pages/CheckoutOverviewPage.cs`, `src/Pages/CheckoutCompletePage.cs`
- Create: `tests/CheckoutTests.cs`

**Interfaces:**
- Consumes: `CartPage.GotoCheckoutAsync` (Task 8), `InventoryPage.AddToCartAsync`, `CartBadge.OpenCartAsync`, `TestData.TaxRate`
- Produces:
  - `CheckoutInfoPage(IPage page)` — `ILocator ErrorMessage { get; }`, `Task FillAsync(string first, string last, string postal)`, `Task ContinueAsync()`
  - `CheckoutOverviewPage(IPage page)` — `Task<decimal> SubtotalAsync()`, `Task<decimal> TaxAsync()`, `Task<decimal> TotalAsync()`, `Task FinishAsync()`
  - `CheckoutCompletePage(IPage page)` — `ILocator Header { get; }`

**계획 게이트.** 이 태스크의 핵심은 총액 검증이다. 화면에 표시된 합계를 화면에 표시된 소계·세금과만 비교하면 사이트가 세 값을 모두 틀리게 계산해도 통과한다. **상품 정가를 테스트가 독립적으로 합산해 기대값을 만들고** 그것을 화면 값과 비교한다.

- [ ] **Step 1: 실패하는 테스트 작성**

`tests/CheckoutTests.cs`:

```csharp
using Microsoft.Playwright;
using SauceDemo.E2E.Components;
using SauceDemo.E2E.Pages;
using SauceDemo.E2E.Support;

namespace SauceDemo.E2E.Tests;

public class CheckoutTests : BaseTest
{
    private async Task GoToCheckoutInfoAsync(params string[] products)
    {
        var login = new LoginPage(Page);
        await login.GotoAsync();
        await login.LoginAsync(TestData.StandardUser, TestData.Password);

        var inventory = new InventoryPage(Page);
        await Expect(inventory.Items).ToHaveCountAsync(6);
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
        // 정가는 상품 목록에서 확인된 값. 테스트가 독립적으로 기대값을 계산한다.
        const decimal backpackPrice = 29.99m;   // Sauce Labs Backpack
        const decimal bikeLightPrice = 9.99m;   // Sauce Labs Bike Light
        var expectedSubtotal = backpackPrice + bikeLightPrice;
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
```

`MidpointRounding.AwayFromZero`를 쓰는 이유: .NET의 기본 반올림은 은행가 반올림(`ToEven`)이라 `2.345`를 `2.34`로 만든다. 웹 애플리케이션의 금액 계산은 보통 사사오입이다. 실측한 39.98 케이스는 두 방식이 같은 값(3.20)을 내지만 다른 상품 조합에서 갈릴 수 있으므로 명시한다.

- [ ] **Step 2: 실패 확인**

```bash
dotnet test --filter "FullyQualifiedName~CheckoutTests"
```

Expected: 컴파일 실패. 체크아웃 Page 3종이 없다.

- [ ] **Step 3: `src/Pages/CheckoutInfoPage.cs` 작성**

```csharp
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
```

- [ ] **Step 4: `src/Pages/CheckoutOverviewPage.cs` 작성**

```csharp
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
```

- [ ] **Step 5: `src/Pages/CheckoutCompletePage.cs` 작성**

```csharp
using Microsoft.Playwright;

namespace SauceDemo.E2E.Pages;

public class CheckoutCompletePage(IPage page)
{
    public ILocator Header => page.Locator("[data-test=\"complete-header\"]");
}
```

- [ ] **Step 6: 테스트 통과 확인**

```bash
dotnet test --filter "FullyQualifiedName~CheckoutTests"
```

Expected: PASS 3개.

- [ ] **Step 7: 전체 20케이스 실행**

```bash
dotnet test
```

Expected: PASS 20개 (로그인 6 + 목록 6 + 장바구니 4 + 체크아웃 3 + 세션 1).

개수가 20이 아니면 세어서 어느 영역이 빠졌는지 확인한다.

- [ ] **Step 8: 리뷰 에이전트 실행 후 커밋**

```bash
git add -A
git commit -m "feat: 체크아웃 3케이스와 총액 계산 독립 검증"
```

- [ ] **Step 9: 3단계 회고**

Step 1의 계획 게이트가 실제로 무엇을 미리 잡았는지 기록한다. 아무것도 안 잡았다면 계획 단계를 이 규모의 프로젝트에서 유지할 가치가 있는지 판단한다.

---

## Task 10: GitHub Actions CI (M4)

**Files:**
- Create: `.runsettings`, `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: Task 9까지의 전체 테스트 스위트
- Produces: 없음

- [ ] **Step 1: `.runsettings` 작성**

리포지토리 루트에 만든다. `BaseTest`는 건드리지 않는다 — 실행 설정이 코드에 섞이면 로컬에서 헤드풀로 디버깅할 수 없다.

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <Playwright>
    <BrowserName>chromium</BrowserName>
    <LaunchOptions>
      <Headless>true</Headless>
    </LaunchOptions>
  </Playwright>
</RunSettings>
```

- [ ] **Step 2: `.github/workflows/ci.yml` 작성**

```yaml
name: CI

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Install Playwright browsers
        run: pwsh bin/Release/net10.0/playwright.ps1 install chromium --with-deps

      - name: Test
        run: dotnet test --no-build --configuration Release --settings .runsettings --logger "trx;LogFileName=results.trx"

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: TestResults/
          if-no-files-found: ignore

      - name: Upload Playwright traces
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-traces
          path: |
            **/playwright-traces/
            **/trace.zip
          if-no-files-found: ignore
```

재시도 설정을 넣지 않는다. 스케줄 트리거를 넣지 않는다. 브라우저 매트릭스를 넣지 않는다. 셋 다 스펙 8장의 결정이다.

- [ ] **Step 3: 로컬에서 CI와 같은 명령으로 실행**

```bash
dotnet build --configuration Release
pwsh bin/Release/net10.0/playwright.ps1 install chromium
dotnet test --no-build --configuration Release --settings .runsettings
```

Expected: PASS 20개. Release 구성에서 `playwright.ps1` 경로가 달라지므로 여기서 먼저 확인해야 CI에서 헛돈다.

- [ ] **Step 4: 커밋**

```bash
git add -A
git commit -m "ci: GitHub Actions 워크플로와 실행 설정"
```

- [ ] **Step 5: 원격 저장소 생성과 푸시**

원격이 아직 없으므로 public 저장소를 만들고 연결한다. 저장소가 공개되는 시점이므로 진행 전에 사용자 확인을 받는다.

```bash
gh repo create saucedemo-playwright-dotnet --public --source=. --remote=origin --push
```

- [ ] **Step 6: CI 통과 확인**

```bash
gh run watch
```

Expected: 성공. 실패하면 로그를 읽고 고친다. 여기서 실패를 방치하면 README에 배지를 걸 수 없다.

---

## Task 11: README 테스트 전략 (M4 완료)

**Files:**
- Create: `README.md`

**Interfaces:**
- Consumes: Task 1~10의 모든 결과
- Produces: 없음

- [ ] **Step 1: README 작성**

아래 구조로 작성한다. 각 절은 실제로 구현한 내용과 실행 결과를 근거로 채운다. 계획서 문구를 그대로 옮기지 않는다.

```markdown
# SauceDemo E2E 자동화

[![CI](https://github.com/<계정>/saucedemo-playwright-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/<계정>/saucedemo-playwright-dotnet/actions/workflows/ci.yml)

Playwright(.NET)와 xUnit으로 작성한 E2E 테스트 20개.
대상은 공개 데모 사이트 [SauceDemo](https://www.saucedemo.com)다.

## 실행

(설치·실행 명령)

## 테스트 전략

### 커버리지 판단 기준
사용자가 실제로 거치는 경로(로그인 → 상품 선택 → 장바구니 → 결제)를 축으로
잡고, 각 단계에서 정상 경로 1개와 실패 경로 1개 이상을 확보했다.
(실제 케이스 표를 여기 넣는다)

### 포함한 케이스와 이유
- 총액 계산 검증: 화면 값끼리 비교하지 않고 상품 정가를 테스트가 독립적으로
  합산해 기대값을 만든다. 사이트가 세 값을 모두 틀리게 계산해도 잡힌다.
- problem_user 이미지 결함: 정상 계정과 대조해야 결함이라고 말할 수 있으므로
  두 계정을 한 테스트에서 비교한다.
- 미로그인 직접 접근 차단: 오류 메시지뿐 아니라 URL도 함께 검증한다.

### 제외한 케이스와 이유
- `performance_glitch_user`: 의도적 응답 지연 계정이라 flaky의 원인이 된다.
  응답 시간 검증은 E2E가 아니라 성능 테스트의 영역이다.
- 브라우저 매트릭스(Firefox/WebKit): SauceDemo에서 브라우저별 동작 차이가
  드러나지 않는데 CI 실행 시간만 3배가 된다.
- 픽셀 단위 이미지 비교: 유지보수 비용 대비 얻는 것이 없어 `src` 속성
  비교로 한정했다.

## 구조

(Pages / Components / Support 역할과 Components를 분리한 이유)

### 설계 규칙
1. Page 클래스는 assert하지 않는다 — 조작과 조회만. 검증은 테스트에 있다.
2. 셀렉터는 Page 클래스 안에만 존재한다 — UI 변경 시 수정 지점이 한 곳이다.
3. `Thread.Sleep`을 쓰지 않는다 — Playwright 자동 대기와 `Expect()`만 쓴다.
4. 재시도를 설정하지 않는다 — flaky를 재시도로 가리면 자동화의 의미가 없다.

## 발견한 결함

(problem_user 이미지 결함 등, 실제로 검출한 것)

## 실패 분석 방법

CI 실패 시 Actions 실행 페이지의 `playwright-traces` 아티팩트를 내려받아
`pwsh bin/Debug/net10.0/playwright.ps1 show-trace trace.zip`으로 연다.
각 단계의 스냅샷·네트워크·콘솔이 남는다.

## 알려진 제약

대상이 외부 공개 데모 사이트이므로 사이트 장애나 DOM 변경 시 실패한다.
이 때문에 스케줄 실행을 붙이지 않고 push·pull_request에서만 돌린다.
```

- [ ] **Step 2: 배지 URL의 계정명 치환**

`<계정>` 자리를 실제 GitHub 계정명으로 바꾼다. 치환하지 않으면 배지가 깨진 이미지로 뜬다.

- [ ] **Step 3: 커밋과 푸시**

```bash
git add README.md
git commit -m "docs: README 테스트 전략"
git push
```

- [ ] **Step 4: 최종 확인**

```bash
dotnet test
gh run watch
```

Expected: 로컬 PASS 20개, CI 성공, README 배지 정상 표시.

- [ ] **Step 5: 완료 판정**

스펙 11장의 두 조건을 확인한다.

- **포트폴리오**: 20케이스 통과, POM 구조, CI 배지, README 테스트 전략. 이력서에 링크를 제출할 수 있는 상태인가.
- **팀 구조**: `CLAUDE.md`가 관찰에 근거해 축적됐는가. 리뷰 에이전트가 실제 문제를 1건 이상 잡았는가. 계획 게이트가 설계 오류를 1건 이상 사전에 막았는가.

팀 구조 쪽 세 항목 중 충족하지 못한 것이 있으면 무엇이 왜 작동하지 않았는지 기록한다. 다음 프로젝트에서 그 단계를 조정할 근거가 된다.

---

## 케이스 수 대조표

| 영역 | 태스크 | 케이스 |
|---|---|---|
| 로그인 | Task 2, 3 | 6 |
| 상품 목록 | Task 6, 7 | 6 |
| 장바구니 | Task 8 | 4 |
| 체크아웃 | Task 9 | 3 |
| 세션 | Task 5 | 1 |
| **합계** | | **20** |
