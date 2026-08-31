# .NET Version Upgrade Plan

## Overview

**Target**: 将 Vine.Q 解决方案中的 3 个 SDK-style 项目升级到 .NET 10 (`net10.0`)，同时更新兼容的包并修复源码兼容性问题。
**Scope**: 3 个项目、约 827 行代码；包含类库、应用和测试项目，当前目标框架为 net8.0/net6.0。

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 3 个项目均为现代 .NET SDK-style 项目，依赖图较浅，评估显示升级范围集中且没有高风险迁移。

## Tasks

### 01-prerequisites: Verify .NET 10 prerequisites

确认 .NET 10 SDK 已安装，并检查仓库中的 global.json（如存在）是否允许目标 SDK。该任务为后续原子升级提供可验证的工具链前提，不修改业务代码。

**Done when**: .NET 10 SDK 和 global.json 兼容性验证完成，并记录验证结果。

---

### 02-upgrade-solution: Upgrade all projects and resolve compatibility issues

同步更新 `src/Sample.App/Sample.App.csproj`、`src/Vine.Q/Vine.Q.csproj` 和 `tests/Vine.Q.Tests/Vine.Q.Tests.csproj` 的目标框架到 `net10.0`，保留现有项目结构并清理旧的多目标框架设置。按照评估结果更新建议的 Microsoft.Extensions.DependencyInjection 包及其 Abstractions 包，处理测试项目中已弃用的 xunit 包，并保留每项目包管理方式。

评估识别出 5 个源代码兼容性问题，主要涉及 `TimeSpan.FromSeconds(double)`；需要在本任务中直接修复并通过重新编译确认 API 调整正确。研究起点包括三个项目文件、Vine.Q 中受影响的源码文件、测试项目的测试框架引用及项目间引用关系。

**Done when**: 三个项目均仅目标为 `net10.0`，包引用恢复成功，所有源兼容性问题已直接修复，解决方案构建无错误且无警告。

---

### 03-solution-validation: Validate build and tests after upgrade

执行完整解决方案恢复、构建和测试，确认应用、类库及测试项目在 .NET 10 下协同工作。检查升级后的包依赖不存在冲突，并记录 CPM 作为后续稳定化阶段的可选建议，而不是在本次原子升级中引入集中式包管理。

**Done when**: 解决方案构建无错误和警告，所有测试通过，且不存在已知依赖冲突或未记录的升级问题。
