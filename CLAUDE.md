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
