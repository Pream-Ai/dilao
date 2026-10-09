---
status: accepted
---

# AAConst 键使用组内相对路径，而非 merge_vertex 的文件名

`AAConst` 的键采用资源在组内的相对路径去掉扩展名（`/` 转 `_`）；若冲突，回退为含扩展名的完整路径。merge_vertex 的约定是用文件名（basename）作键，dilao 有意偏离。

dilao 的资源里存在大量跨目录重名：角色动画帧在不同角色目录下都是 `1.png`、`2.png`、`idle.png`、`walk.png`（17 个角色 × 多套动作），场景/家具的图标也反复使用 `part_01.png` 这类名字。若照搬 basename 方案，绝大多数键会冲突并落入回退路径，键名既不唯一也不可读，还会随文件集合变化而抖动。组内相对路径天然唯一，且与磁盘位置一一对应，便于定位。

## Considered Options

- **采用 merge_vertex 的 basename 键**（被否）：与参考项目一致，但在 dilao 的资源命名下冲突率过高。
- **组内相对路径作键（采用）**：唯一、可读、与目录结构一致。

## Consequences

- 键名较长（如 `furni_data_bingxiang`、`icons_interact_punchbag_1`）。
- 重命名/移动资源目录会连带改变键名，调用点需同步。
- 与 merge_vertex 的 `AAConst` 存在系统性差异，跨项目复制代码时不可直接类比。
