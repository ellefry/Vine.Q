# 01-prerequisites: Verify .NET 10 prerequisites

确认 .NET 10 SDK 已安装，并检查仓库中的 global.json（如存在）是否允许目标 SDK。该任务为后续原子升级提供可验证的工具链前提，不修改业务代码。

**Done when**: .NET 10 SDK 和 global.json 兼容性验证完成，并记录验证结果。

## Research Findings

- 作用域：后续升级涉及 `src/Sample.App/Sample.App.csproj`、`src/Vine.Q/Vine.Q.csproj` 和 `tests/Vine.Q.Tests/Vine.Q.Tests.csproj`，均为 SDK-style 项目。
- 已通过 `validate_dotnet_sdk_installation` 验证可用的 .NET 10 SDK。
- 在仓库中未发现 `global.json`，因此没有 SDK 版本锁定需要调整。
- 本任务不涉及包、源码或项目文件修改；后续现代 SDK 项目使用 `dotnet build` 进行验证。

