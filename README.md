# 다듬 (Dadeum)

**PC를 무리하게 깎지 않고 조심스럽게 다듬는, 무료 오픈소스 윈도우 최적화 도구.**

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](./LICENSE)
![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4)

> ⚠ **개발 초기 단계입니다.** 아직 정식 릴리스가 없습니다.

## 다듬은 무엇인가요

다듬은 [optimizerDuck](https://github.com/itsfatduck/optimizerDuck)(GPL-3.0)을 바탕으로 만든 윈도우 10/11 최적화 앱입니다.
원작의 장점인 **바꾸기 전에 원래 값을 기록하고 언제든 되돌리는 구조**를 그대로 이어받고, 그 위에 다음을 더하는 것이 목표입니다.

- **근거가 있는 항목만:** 모든 항목에 안전 / 주의 / 위험 표시, 바꾸면 무엇이 달라지는지, 근거 링크를 답니다. 효과가 확인되지 않았거나 해로운 "FPS 트윅"은 넣지 않습니다.
- **정확한 되돌리기:** "이전 상태로"와 "윈도우 기본값으로"를 따로 제공하고, 윈도우 업데이트로 바뀐 값도 알려 줍니다.
- **한국 사용자에 맞춤:** 한국어가 기본 언어이고, 넥슨·스토브·라이엇·배틀넷 등 국내에서 많이 쓰는 게임 런처의 캐시도 정리합니다.
- **수집하지 않음:** 앱이 사용자 정보를 어디에도 보내지 않습니다. 네트워크는 새 버전 확인에만 씁니다.

## 빌드

필요한 것: Windows 10/11, [.NET SDK 10](https://dotnet.microsoft.com/download)

```powershell
dotnet build optimizerDuck.slnx --configuration Release
dotnet test optimizerDuck.slnx --no-build -c Release
```

실행은 관리자 권한이 필요합니다(`dotnet run --project optimizerDuck/optimizerDuck.csproj`를 관리자 터미널에서 실행).
내부 프로젝트와 네임스페이스 이름이 `optimizerDuck`인 것은 원작의 업데이트를 계속 받아오기 쉽게 하려고 일부러 그대로 둔 것입니다.

## 라이선스와 출처

- 다듬은 **GNU GPL v3.0**으로 배포됩니다. 자유롭게 쓰고, 고치고, 다시 배포할 수 있으며, 배포할 때는 같은 라이선스로 소스를 함께 공개해야 합니다.
- 원작: optimizerDuck, Copyright (C) 2025–2026 fatDuck (itsfatduck). 다듬은 원작 프로젝트와 관계가 없고 원작의 보증을 받지 않았습니다.
- 원작에서 바꾼 내용: [NOTICE.md](./NOTICE.md)
- 함께 쓰는 오픈소스: [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md)
- 원작 README: [UPSTREAM-README.md](./UPSTREAM-README.md)

## 주의

시스템 설정을 바꾸는 프로그램입니다. 적용 전에 복원 지점이 만들어지지만, 중요한 데이터는 따로 백업하세요.
자세한 내용은 [DISCLAIMER.md](./DISCLAIMER.md)를 보세요.
