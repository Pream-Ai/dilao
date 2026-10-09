---
status: accepted
---

# 资源组由目录结构决定（目录即组）

dilao 的 Addressables 分组不手工维护，而由 `Assets/Project/AddressableRes/Bundles` 下的目录结构唯一决定：以 `m_`/`s_` 开头（或含 `.bundle.settings` 且 `isBundle=true`）的叶子目录即一个资源组，组名=目录名，资源地址=相对组父目录的路径。构建时先清空所有非内置组，再按目录重建，并生成 `AAConst.cs`。

手工在 Addressables Groups 窗口维护分组，在多轮迭代下极易漏配、产生重复地址、并在合并时漂移。把「分组」变成「目录即配置」后，增删资源只需移动文件，构建机一次性推导出分组、地址、标签、schema 与常量表，结果可 diff、可由 CI 复现。

## Considered Options

- **手工在 Addressables Groups 窗口维护**：灵活，但不可复现，重复地址与组漂移靠人眼排查。
- **每个资源单独建组**：组数量爆炸，bundle 粒度不可控。
- **目录即组（采用）**：分组事实唯一，移动文件即改分组。

## Consequences

- 资源目录的移动/重命名会改变地址与 `AAConst` 常量，必须同步修改调用点。
- 组名必须全局唯一（重名会在构建时报错）。
- 命名前缀 `m_/s_/_dl/_r/__i` 成为语义的一部分，不能随意起名。
