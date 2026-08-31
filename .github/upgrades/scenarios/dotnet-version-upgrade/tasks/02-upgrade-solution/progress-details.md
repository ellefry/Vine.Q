# 02-upgrade-solution Progress

## Changes
- `src/Sample.App/Sample.App.csproj`: `net8.0` → `net10.0`。
- `src/Vine.Q/Vine.Q.csproj`: `net8.0;net6.0` → `net10.0`，并将 Microsoft.Extensions.DependencyInjection 与 Abstractions 统一更新到 10.0.11。
- `tests/Vine.Q.Tests/Vine.Q.Tests.csproj`: `net8.0;net6.0` → `net10.0`，xunit 2.9.2 → 2.9.3。
- 评估列出的 `TimeSpan.FromSeconds(double)` 5 处源码均无需修改；.NET 10 编译已确认 API 有效。

## Validation
- 完整解决方案构建：通过，3 个项目均输出 `net10.0`，0 errors、0 warnings（使用 `NuGetAudit=false`；默认恢复时外部 Artifactory 源不可访问会产生 NU1900 网络警告，见 Issues）。
- `Vine.Q.Tests`：4 tests passed, 0 failed, 0 skipped。
- 包恢复：通过；xunit 2.9.3 提供预期 Xunit 编译类型。

## Issues
- 首次 IDE 构建使用旧的恢复资产，暂时报出 Xunit 类型缺失；显式 restore 后重新构建解决。
- NuGet 漏洞审计源 `https://artifactory.boschdevcloud.com/...` 当前不可访问；未修改仓库配置或抑制项目警告，验证命令临时使用 `NuGetAudit=false`。
