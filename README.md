# SauceDemo E2E 자동화

[![CI](https://github.com/ngg42651/saucedemo-e2e/actions/workflows/ci.yml/badge.svg)](https://github.com/ngg42651/saucedemo-e2e/actions/workflows/ci.yml)

Playwright(.NET)와 xUnit으로 작성한 E2E 테스트 20개.
대상은 공개 데모 사이트 [SauceDemo](https://www.saucedemo.com)다.
같은 로그인 시나리오를 Selenium C#으로 재구현한 테스트 2개가 `selenium/`에 따로 있다
(→ [왜 주력은 Playwright인가](#왜-주력은-playwright인가)).

## 실행

```bash
# 브라우저 설치 (최초 1회)
powershell bin/Debug/net10.0/playwright.ps1 install chromium

# 전체 테스트
dotnet test

# 단일 클래스만
dotnet test --filter "FullyQualifiedName~LoginTests"

# 주력 스위트(Playwright)만
dotnet test SauceDemo.E2E.csproj

# Selenium 보조 스위트만 (로컬에 Chrome 필요, 드라이버는 Selenium Manager가 받는다)
dotnet test selenium/SauceDemo.Selenium.csproj
```

`dotnet test`를 인자 없이 돌리면 두 프로젝트가 모두 실행된다(20 + 2 = 22개).

## 테스트 전략

### 커버리지 판단 기준

사용자가 실제로 거치는 경로(로그인 → 상품 목록 → 장바구니 → 결제)를 축으로
잡고, 각 단계에서 정상 경로 1개와 실패 경로 1개 이상을 확보했다. 세션 관리는
별도 축으로 두어 로그아웃 후 직접 접근 차단을 추가했다.

| 영역 | 케이스 수 | 내용 |
|---|---|---|
| 로그인 | 6 | 성공 1, 실패 4(잠긴 계정/비밀번호 오류/아이디 누락/비밀번호 누락), 미로그인 직접 접근 차단 1 |
| 상품 목록 | 6 | 정렬 4(이름 오름/내림차순, 가격 오름/내림차순), 상세 진입 후 복귀 1, problem_user 이미지 결함 1 |
| 장바구니 | 4 | 담기, 빼기, 장바구니 페이지 품목 일치, Reset App State로 초기화 |
| 결제 | 3 | 배송정보 필수 오류, 총액 계산 검증, 주문 확정 완료 메시지 |
| 세션 | 1 | 로그아웃 후 직접 접근 차단 |
| **합계** | **20** | |

### 포함한 케이스와 이유

- **총액 계산 검증** (`CheckoutTests.합계는_상품_정가_합산에_세율_8퍼센트를_적용한_값과_일치한다`):
  화면에 표시된 소계·세금·합계 세 값끼리 서로 비교하지 않는다. 대신 정가
  29.99(Backpack) + 9.99(Bike Light)를 테스트 코드가 하드코딩해 소계
  39.98을 만들고, 세율 8%(`TestData.TaxRate`)를 `MidpointRounding.AwayFromZero`로
  반올림해 세금 3.20, 합계 43.18을 독립적으로 계산한다. 사이트가 세 값을
  똑같이 틀리게 계산해도(예: 세율을 다르게 적용) 화면 값끼리의 비교로는
  잡히지 않지만, 독립 계산과의 비교는 잡아낸다.
- **problem_user 이미지 결함** (`InventoryTests.problem_user는_모든_상품_이미지가_404_플레이스홀더로_깨진다`):
  `standard_user`로 6개 상품 이미지의 `src`를 수집하면 `/assets/sauce-backpack-1200x1500-CjRW-Djj.jpg`
  등 6개가 모두 서로 다르다. 같은 절차를 `problem_user`로 반복하면 6개
  모두 `/assets/sl-404-Cq1a9k9X.jpg`로 동일하다. `standard_user`라는 대조군이
  있어야 "이미지가 깨져 있다"가 관찰이 아니라 결함이라고 말할 수 있다 —
  대조군 없이 problem_user만 봤다면 그 사이트의 정상 이미지가 원래 그런지
  구분할 수 없다.
- **미로그인 직접 접근 차단** (`LoginTests.로그인하지_않고_상품_목록에_직접_접근하면_차단된다`):
  오류 메시지(`Epic sadface: You can only access '/inventory.html' when you are logged in.`)뿐
  아니라 URL도 함께 검증한다. 메시지만 확인하면 리다이렉트 없이 같은
  페이지에 오류 배너만 띄우는 구현도 통과해 버려, 실제로 보호되지 않는
  라우팅을 놓칠 수 있다.

### 제외한 케이스와 이유

- `performance_glitch_user`: 의도적으로 응답을 지연시키는 계정이다. 응답
  시간에 대한 assert를 넣으면 CI 환경의 부하에 따라 흔들리는 flaky 테스트가
  된다. 응답 시간 검증은 E2E가 아니라 성능 테스트의 영역이라 판단해 제외했다.
- 브라우저 매트릭스(Firefox/WebKit): SauceDemo는 브라우저별로 달리 동작하는
  요소가 없는 정적에 가까운 데모 사이트다. 매트릭스를 추가하면 실질적 커버리지
  증가 없이 CI 실행 시간만 3배가 된다.
- 픽셀 단위 이미지 비교: problem_user 결함은 `src` 속성이 `sl-404`로
  바뀌는 것만으로 충분히 검출된다. 스크린샷 비교는 여기서 얻는 것 없이
  브라우저·폰트 렌더링 차이로 인한 오탐 유지보수 비용만 늘린다.

## 구조

```
/src
  Pages/          LoginPage, InventoryPage, CartPage, CheckoutInfoPage,
                  CheckoutOverviewPage, CheckoutCompletePage, ProductDetailPage
  Components/     HeaderMenu, CartBadge
  Support/        BaseTest, TestData
/tests
  LoginTests.cs  InventoryTests.cs  CartTests.cs  CheckoutTests.cs  SessionTests.cs

/selenium         보조 스위트 (별도 프로젝트)
  Pages/          LoginPage
  Support/        BaseTest
  Tests/          LoginTests.cs
```

`Pages/`와 `Components/`를 분리했다. 헤더 메뉴(로그아웃 등)와 장바구니
배지는 로그인 이후 거의 모든 페이지에 나타난다. 이를 각 Page 클래스에
중복 정의하는 것이 Page Object Model을 처음 쓸 때 가장 흔히 저지르는
실수다. 공유 요소를 `Components/`로 분리해 `HeaderMenu`, `CartBadge`
하나씩만 두고 필요한 Page에서 조합해 쓰도록 했다.

### 왜 주력은 Playwright인가

`selenium/`에는 주력 스위트의 로그인 케이스 두 개(정상 로그인, 잠긴 계정)를
Selenium C#으로 그대로 옮긴 테스트가 있다. 커버리지를 늘리려는 것이 아니라
**같은 시나리오를 두 도구로 써 본 뒤 주력을 고른 근거를 남기려는 것**이다.
20개를 양쪽에 중복으로 두면 UI가 바뀔 때마다 같은 수정을 두 번 해야 하므로
비교에 필요한 최소한만 남겼다.

Playwright를 주력으로 고른 이유는 셋이다.

1. **대기 처리.** Playwright는 `Expect()`와 액션 API에 자동 대기가 들어 있어
   대기 코드를 테스트가 들고 있지 않아도 된다. Selenium은 `WebDriverWait`을
   명시적으로 만들어 Page 클래스에 넘겨야 하고(`selenium/Pages/LoginPage.cs`
   생성자 참고), 그 대기를 빠뜨린 자리가 그대로 flaky가 된다.
2. **트레이스.** 실패 시 스냅샷·네트워크·콘솔이 담긴 트레이스를 표준으로
   남길 수 있다. 이 저장소의 실패 분석 절차가 여기 기대고 있다.
3. **브라우저 설치.** `playwright.ps1 install`이 브라우저까지 고정 버전으로
   받아 CI와 로컬이 같은 환경이 된다. Selenium은 러너에 깔린 Chrome을 쓰므로
   러너 이미지가 바뀌면 브라우저 버전도 같이 바뀐다.

두 스위트는 같은 규칙(셀렉터는 Page 클래스 안에만, Page는 assert하지 않음,
`Thread.Sleep` 금지)을 따르고, 계정·비밀번호 상수는 `src/Support/TestData.cs`
하나를 `selenium` 프로젝트가 링크해 공유한다. 복사본을 만들면 한쪽만 고쳐진다.

### 설계 규칙

1. Page 클래스는 assert하지 않는다 — 조작과 조회만. 검증은 테스트에 있다.
   이 경계가 무너지면 테스트가 무엇을 검증하는지 테스트 파일만 읽어서는
   알 수 없게 된다.
2. 셀렉터는 Page 클래스 안에만 존재한다 — UI 변경 시 수정 지점이 한 곳이다.
   테스트 파일에 CSS·XPath 문자열이 등장하면 수정 지점이 흩어진다.
3. `Thread.Sleep`을 쓰지 않는다 — Playwright 자동 대기와 `Expect()`만 쓴다.
4. 테스트를 통과시키려고 assert를 약화시키지 않는다 — 통과 못 하면 통과 못
   했다고 보고한다. assert를 느슨하게 고쳐 초록불을 만드는 순간 테스트는
   진실을 보고하는 도구가 아니게 된다.
5. 모든 Playwright 호출은 `await`한다.

## 발견한 결함

`problem_user` 계정으로 로그인하면 상품 목록의 이미지 6개가 모두 깨진다.

- `standard_user`: 6개 이미지가 모두 서로 다른 URL을 가진다
  (`/assets/sauce-backpack-1200x1500-CjRW-Djj.jpg`,
  `/assets/bike-light-1200x1500-DxcZRFOA.jpg`,
  `/assets/bolt-shirt-1200x1500-mR0ldpVS.jpg`,
  `/assets/sauce-pullover-1200x1500-BfbI-PSd.jpg`,
  `/assets/red-onesie-1200x1500-BrSuq0ic.jpg`,
  `/assets/red-tatt-1200x1500-E-qp6aYf.jpg`).
- `problem_user`: 같은 6개 위치의 이미지가 전부 동일한 404 플레이스홀더
  `/assets/sl-404-Cq1a9k9X.jpg`를 가리킨다.

`InventoryTests.problem_user는_모든_상품_이미지가_404_플레이스홀더로_깨진다`
테스트가 두 계정을 한 테스트 안에서 비교해 이를 검출한다.

## CI 결과 확인

README 상단 배지가 최신 상태를 나타낸다. 초록 `passing`이면 통과, 빨강
`failing`이면 실패, 회색 `no status`면 워크플로가 아직 한 번도 돌지 않은
상태다. 배지를 클릭하면 Actions 실행 목록으로 이동한다. 배지 이미지는
GitHub가 몇 분 캐싱하므로 방금 푸시한 결과가 안 보이면 강력 새로고침
(`Ctrl+Shift+R`)한다.

명령줄에서는 배지 SVG의 제목만 뽑아 확인한다.

```bash
curl -s https://github.com/ngg42651/saucedemo-e2e/actions/workflows/ci.yml/badge.svg | grep -o "CI - [a-z]*"
```

**실행 건수는 사람이 확인하지 않는다.** 잡이 초록이어도 실행 건수가 0이면
검증된 것이 없기 때문에, `Summarize test counts` 단계가 두 프로젝트의
`results.trx`에서 건수를 읽어 실행 요약(Actions 실행 페이지 상단 Summary)에
표로 남기고 **합계가 0이면 잡을 실패시킨다.**

| 결과 파일 | 전체 | 통과 | 실패 |
|---|---:|---:|---:|
| `TestResults/results.trx` | 20 | 20 | 0 |
| `selenium/TestResults/results.trx` | 2 | 2 | 0 |
| **합계** | **22** | **22** | **0** |

이 단계는 `if: always()`라 테스트가 실패한 실행에서도 건수를 남긴다.
원본 로그로 직접 확인하려면 **Actions → CI**에서 실행을 고르고 `test` 잡의
**Test** 단계 로그 마지막 요약 줄을 본다.

```
Passed!  - Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 8 s
```

`trx` 파일 자체는 `test-results` 아티팩트로 올라간다.

## 실패 분석 방법

CI 실패 시 Actions 실행 페이지의 `playwright-traces` 아티팩트를 내려받아
아래 명령으로 연다.

```bash
powershell bin/Debug/net10.0/playwright.ps1 show-trace trace.zip
```

CI는 러너에 PowerShell 7이 있어 `pwsh`를 쓰고, 로컬에서는 위 예시처럼
`powershell`을 쓴다(`CLAUDE.md` 명령어 기준). 트레이스에는 각 단계의
스냅샷·네트워크 요청·콘솔 로그가 남는다.

CI 재시도 횟수는 0이다. flaky를 재시도로 가리면 자동화가 존재하는
이유가 사라지기 때문이다.

## 알려진 제약

- 대상이 외부 공개 데모 사이트이므로 사이트 장애나 DOM 변경 시 테스트가
  실패할 수 있다. 이 때문에 스케줄 실행을 붙이지 않고 `push`·`pull_request`
  에서만 돌린다.
- `selenium/` 스위트는 브라우저를 고정 버전으로 받지 않고 실행 환경에 깔린
  Chrome을 쓴다. CI에서는 러너 이미지의 Chrome, 로컬에서는 설치된 Chrome이며
  드라이버만 Selenium Manager가 맞춰 받는다. 주력 스위트와 달리 브라우저
  버전이 환경에 따라 달라진다.
