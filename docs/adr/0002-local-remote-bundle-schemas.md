---
status: accepted
---

# 本地与远端资源使用两套固定 Bundle Schema

资源是否远端由目录名 `_r` 判定，并据此套用两套固定的 `BundledAssetGroupSchema`：

- 本地：`BundleNaming = NoHash`、`Compression = LZ4`、`ContentUpdateGroupSchema.StaticContent = true`、CRC off。
- 远端：`BundleNaming = AppendHash`、`Compression = LZMA`、`StaticContent = false`、`UseAssetBundleCrc = true`、`Timeout = 20`、`RetryCount = 3`。

本地内容随安装包分发，地址在一次安装内稳定，因此不需要文件名 hash，也不必强制内容校验；但需要被标记为静态内容，才能被复用而不触发内容更新约束。远端内容经 CDN 下载，需要 hash 命名以做缓存失效、需要 CRC 与超时重试以容忍网络，且必须允许变化（`StaticContent = false`）才能进入内容更新流程。

## Considered Options

- **本地/远端统一一套 schema**：配置更少，但本地被迫承担 hash 与校验成本，远端又失去必要的重试语义。
- **每组手配 schema**：最灵活，但无法保证一致，容易漏配。
- **按 `_r` 自动套用两套固定 schema（采用）**：约定驱动，一致且可复现。

## Consequences

- `_r` 是远端性的唯一开关（无论写在目录名还是 `.bundle.settings`）。
- 远端组默认落入内容更新范围；若某远端组不应被更新，需要 `s_`/`.bundle.settings` 或重新归类。
- 更改压缩/命名策略会改变已发布 bundle 的身份，需要重建并重新走发布流程。
