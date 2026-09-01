# 02-upgrade-solution: Upgrade all projects and resolve compatibility issues

同步更新 `src/Sample.App/Sample.App.csproj`、`src/Vine.Q/Vine.Q.csproj` 和 `tests/Vine.Q.Tests/Vine.Q.Tests.csproj` 的目标框架到 `net10.0`，保留现有项目结构并清理旧的多目标框架设置。按照评估结果更新建议的 Microsoft.Extensions.DependencyInjection 包及其 Abstractions 包，处理测试项目中已弃用的 xunit 包，并保留每项目包管理方式。

评估识别出 5 个源代码兼容性问题，主要涉及 `TimeSpan.FromSeconds(double)`；需要在本任务中直接修复并通过重新编译确认 API 调整正确。研究起点包括三个项目文件、Vine.Q 中受影响的源码文件、测试项目的测试框架引用及项目间引用关系。

**Done when**: 三个项目均仅目标为 `net10.0`，包引用恢复成功，所有源兼容性问题已直接修复，解决方案构建无错误且无警告。

## Research Findings

- 作用域包含 `src/Sample.App/Sample.App.csproj`、`src/Vine.Q/Vine.Q.csproj`、`tests/Vine.Q.Tests/Vine.Q.Tests.csproj`；三者均为 SDK-style 项目，Sample.App 和测试项目引用 Vine.Q。
- `Sample.App` 当前为单目标 `net8.0`，仅有项目引用和隐式依赖，无显式包版本需要更新。
- `Vine.Q` 当前为 `net8.0;net6.0`，显式使用 `System.Reactive` 6.0.0，以及按 TFM 条件引用的 Microsoft.Extensions.DependencyInjection 8.0.0/6.0.1 和 Abstractions 8.0.0/6.0.0。评估建议 .NET 10 版本为 10.0.11。
- `Vine.Q.Tests` 当前为 `net8.0;net6.0`，使用 Microsoft.NET.Test.Sdk 17.11.1、xunit 2.9.2 和 xunit.runner.visualstudio 2.8.2；评估将 xunit 标为弃用并建议升级到 2.9.3。
- 未发现 `Directory.Packages.props` 或 CPM；按已确认选项保留各项目 PackageReference。
- 评估列出 5 个 `TimeSpan.FromSeconds(double)` 源兼容性事件：`src/Vine.Q/VineWorkQueue.cs` 1 处、`tests/Vine.Q.Tests/VineWorkQueueTests.cs` 4 处。该 API 在现有代码中仍为标准 double 重载，先通过 net10 编译验证，只有出现实际编译错误时才改变实现。
- 未发现需要 SDK-style 转换、多目标兼容条件、System.Web 或其他迁移技能的信号；无 stub 标记需要解析。

