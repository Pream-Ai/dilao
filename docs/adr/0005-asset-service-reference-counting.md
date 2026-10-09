---
status: accepted
---

# AssetService 以引用计数管理加载句柄

`AssetService` 的注释声称「句柄缓存 + 引用计数」，但实现只有一个按地址索引的句柄字典：重复加载不计数，`Release(key)` 直接丢弃并释放句柄，而调用方（如 `buildSystem`）用 `Object.Instantiate` 绕过 `InstantiateAsync` 后从不释放，导致句柄与资源常驻。我们决定让 `AssetService` 真正按地址维护引用计数：`LoadAssetAsync` 递增、`Release` 递减，归零才执行 `Addressables.Release`；实例化统一走 `InstantiateAsync`，由 `ReleaseInstance` 释放。

「及时卸载、控制内存占用」是这套架构的目标之一。没有计数就无法区分「暂时没用到」与「还有人持有」，要么过早释放导致资源被重载，要么永不释放导致内存增长；两者都会在长会话里暴露。

## Considered Options

- **简单缓存（现状）**：最简单，但语义错误，句柄生命周期失控。
- **由每个调用方自行持有并释放句柄**：职责清晰，但调用点分散、迟早漏释放。
- **中心化引用计数（采用）**：释放职责收敛到 `AssetService`，调用方只认地址。

## Consequences

- 业务代码必须成对调用加载与释放；需要明确「场景级」`ReleaseAll` 的边界（切场景/切关卡时集中释放）。
- 需要为 `InstantiateAsync` 返回的实例记录归属，`ReleaseInstance` 才能正确卸载。
- 若将来引入「常驻资源」概念，需要与计数机制区分，避免被场景级释放误伤。
