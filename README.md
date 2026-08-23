# SauceDemo E2E 자동화

[![CI](https://github.com/ngg42651/saucedemo-playwright-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/ngg42651/saucedemo-playwright-dotnet/actions/workflows/ci.yml)

Playwright(.NET)와 xUnit으로 작성한 E2E 테스트 20개.
대상은 공개 데모 사이트 [SauceDemo](https://www.saucedemo.com)다.

## 실행

```bash
# 브라우저 설치 (최초 1회)
powershell bin/Debug/net10.0/playwright.ps1 install chromium

# 전체 테스트
dotnet test

# 단일 클래스만
dotnet test --filter "FullyQualifiedName~LoginTests"
```

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
```

`Pages/`와 `Components/`를 분리했다. 헤더 메뉴(로그아웃 등)와 장바구니
배지는 로그인 이후 거의 모든 페이지에 나타난다. 이를 각 Page 클래스에
중복 정의하는 것이 Page Object Model을 처음 쓸 때 가장 흔히 저지르는
실수다. 공유 요소를 `Components/`로 분리해 `HeaderMenu`, `CartBadge`
하나씩만 두고 필요한 Page에서 조합해 쓰도록 했다.

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
- 이 저장소는 아직 GitHub에 올라가지 않았다. 위 CI 배지는 저장소가
  생성되고 워크플로가 최소 1회 실행된 뒤에 정상 표시된다.
