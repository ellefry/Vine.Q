# .NET 版本升级

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: feature/optimization-stuff
- **Working Branch**: feature/optimization-stuff（当前分支，直接修改）
- **Pending Changes**: None
- **Commit Strategy**: After Each Task
- **Branch Sync**: Disabled

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported API Handling: Fix Inline (5 source-incompatible API findings)

## Strategy
**Selected**: All-at-Once
**Rationale**: 3 个项目均为 SDK-style modern .NET 项目，当前目标为 net8.0/net6.0，依赖图较浅且升级范围集中。

### Execution Constraints
- 所有项目在一次原子升级操作中同步更新。
- 先验证 .NET 10 SDK 与 global.json 兼容性，再执行项目和包更新。
- 升级项目目标框架、包引用并修复源码兼容性问题后，统一恢复和构建解决方案。
- 构建成功后再运行完整测试套件，并将 CPM 作为迁移后的建议记录。
