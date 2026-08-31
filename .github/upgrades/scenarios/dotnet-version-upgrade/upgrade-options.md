# Upgrade Options — Vine.Q

Assessment: 3 个 SDK-style 项目，当前为 net8.0/net6.0，目标为 net10.0；包含 5 个源码兼容性问题、3 个 NuGet 更新建议及 1 个已弃用包。

## Strategy

### Upgrade Strategy
所有项目均已使用现代 .NET，项目数量少且依赖图较浅，适合一次性完成升级。

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | 在一次原子操作中同时升级所有项目，速度最快，但期间解决方案可能暂时无法构建。 |
| Top-Down | 先升级入口应用，并让共享库临时多目标构建，以保持增量可构建性。 |

## Project Structure

### Package Management
解决方案包含 3 个项目且未使用集中式包管理；当前升级规模较小，不引入 CPM 以避免额外迁移范围。

| Value | Description |
|-------|-------------|
| **Per-Project (defer CPM to post-migration)** (selected) | 保留各项目的包版本；升级稳定后再评估集中式包管理。 |
| Central Package Management (CPM) | 创建 Directory.Packages.props，将包版本集中维护。 |

## Compatibility

### Unsupported API Handling
评估发现 5 个源代码兼容性问题，均为少量现代 .NET API 编译兼容性调整。

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | 在当前升级任务中直接修复所有 API 兼容性问题，不留下临时桩代码。 |
| Defer Complex Changes | 直接修复简单变更，对复杂变更创建可编译桩代码并延后处理。 |
