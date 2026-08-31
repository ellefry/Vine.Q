# 02-upgrade-solution: Upgrade all projects and resolve compatibility issues

同步更新 `src/Sample.App/Sample.App.csproj`、`src/Vine.Q/Vine.Q.csproj` 和 `tests/Vine.Q.Tests/Vine.Q.Tests.csproj` 的目标框架到 `net10.0`，保留现有项目结构并清理旧的多目标框架设置。按照评估结果更新建议的 Microsoft.Extensions.DependencyInjection 包及其 Abstractions 包，处理测试项目中已弃用的 xunit 包，并保留每项目包管理方式。

评估识别出 5 个源代码兼容性问题，主要涉及 `TimeSpan.FromSeconds(double)`；需要在本任务中直接修复并通过重新编译确认 API 调整正确。研究起点包括三个项目文件、Vine.Q 中受影响的源码文件、测试项目的测试框架引用及项目间引用关系。

**Done when**: 三个项目均仅目标为 `net10.0`，包引用恢复成功，所有源兼容性问题已直接修复，解决方案构建无错误且无警告。

