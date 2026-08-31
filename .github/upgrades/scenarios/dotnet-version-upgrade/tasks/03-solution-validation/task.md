# 03-solution-validation: Validate build and tests after upgrade

执行完整解决方案恢复、构建和测试，确认应用、类库及测试项目在 .NET 10 下协同工作。检查升级后的包依赖不存在冲突，并记录 CPM 作为后续稳定化阶段的可选建议，而不是在本次原子升级中引入集中式包管理。

**Done when**: 解决方案构建无错误和警告，所有测试通过，且不存在已知依赖冲突或未记录的升级问题。

