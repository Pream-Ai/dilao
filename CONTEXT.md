# 资源上下文（Resource Context）

本上下文命名 dilao 的 Addressables 资源管理与热更术语。它只定义「语言」，不含实现细节——实现与取舍见 `docs/adr/`。

## Language

**资源组（Group）**：
Addressables 中的一个分组，组名等于 `Assets/Project/AddressableRes/Bundles` 下的叶子目录名。
_Avoid_: Bundle、包、文件夹

**目录即组（Directory-as-Group）**：
分组由目录结构唯一决定，构建前清空并按目录重建，从不在编辑器里手工维护分组。
_Avoid_: 手工分组、临时组

**地址（Address）**：
运行时加载资源所用的字符串，等于资源相对「组父目录」的路径，因此总以组名开头。
_Avoid_: AssetReference、GUID、路径

**动态资源（Dynamic Resource）**：
组名带 `_dl` 的资源，其地址会被登记进 `AAConst` 常量；业务代码通过常量而非手写字符串取地址。
_Avoid_: 手写地址、魔法字符串

**远端资源（Remote Resource）**：
组名带 `_r` 的资源，打进远端 bundle，经 CDN 下载而非随包分发。
_Avoid_: 热更代码、可更新资源

**本地资源（Local Resource）**：
组名不带 `_r` 的资源，随安装包（StreamingAssets）分发。
_Avoid_: 内置资源、静态资源

**打包模式（Packing Mode）**：
`m_` 前缀 = PackTogether（整组一个 bundle）；`s_` 前缀 = PackSeparately（每资源一个 bundle）。
_Avoid_: 组类型、粒度

**忽略标记（Ignore Tag）**：
路径中含 `__i` 的目录或文件不参与分组与打包。
_Avoid_: 排除、黑名单

**AAConst**：
构建时生成的「键 → 地址」常量表。键由动态资源在组内的相对路径去扩展名得到。
_Avoid_: 地址表、字典

**资源服务（AssetService）**：
运行时唯一的资源加载入口，负责初始化、加载、缓存、引用计数与释放。
_Avoid_: ResourceManager（那是残留的 AssetBundle 加载器）

**资源句柄（Handle）**：
Addressables 加载/实例化返回的操作句柄；获取与释放必须成对。
_Avoid_: 引用、指针

**内容目录（Catalog）**：
Addressables 的地址索引；远端场景下为远程 catalog，是内容可被寻址与更新的前提。
_Avoid_: 清单、manifest

**内容更新（Content Update）**：
在不重新安装客户端的前提下替换远端资源（bundle + catalog）。
_Avoid_: 热更代码、版本更新

**基线（Baseline）**：
`addressables_content_state.bin`，是判断「哪些资源发生了变化、需进入内容更新」的事实来源。
_Avoid_: 存档、快照

**原生版本（Native Version）**：
与安装包绑定的版本标识，用于定位该包对应的远端内容目录。
_Avoid_: 游戏版本、构建号
