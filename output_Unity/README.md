# Triple Harvest Unity 资产库

## 丰收奖励计划

适配水果入篮三消玩法的奖励与模拟提现方案见 [丰收奖励计划](Docs/HarvestRewards.md)，浏览版见 [方案 HTML](Docs/HarvestRewards.html)。文档包含六阶段任务、奖励与广告规则、页面文案及开发接入点；这是产品方案交付，尚未接入游戏运行逻辑，不代表真实现金兑付。

本目录是可直接作为 Unity 项目打开的开发资产库，目标编辑器版本为 `2022.3.62f3`。启动场景、核心 Prefab、AnimationClip、AnimatorController、Material、Texture、AudioClip、配置、脚本类型和依赖程序集均按 Unity 项目结构组织，并保留项目内对象引用。

## 首次使用

1. 使用 Unity `2022.3.62f3` 打开本目录，不要先用更低版本保存项目。
2. 等待首次资源导入完成，确认 Console 中没有 Missing Script、Missing GUID 或 Shader 编译错误。
3. 打开 `Assets/Game.unity` 检查常驻入口，再从 `Assets/res/local` 接入页面、玩法和特效 Prefab，从 `Assets/res_server` 读取关卡、地图和版本配置。
4. 特效优先整 Prefab 实例化；默认沿用粒子模块、Renderer、Material、AnimationClip 和层级参数，只在接入文档声明的灵活项内调整。
5. 完成目标平台材质与 Shader 检查后，再执行 Android ARMv7 构建验证。

## 目录职责

- `Assets/res/local`：页面、玩法、特效、字体、音频、动画及其直接依赖。
- `Assets/res_server`：关卡配置、版本配置、地图和关卡图片。
- `Assets/Game.unity`、`Assets/Game`：项目入口场景及其 LightingData。
- `Assets/MonoBehaviour`：入口场景直接使用的音频扩展与触觉设置。
- `Assets/Material`、`Assets/Sprite`、`Assets/Texture2D`：无独立容器路径但仍属于版本资产的支持对象。
- `Assets/Scripts`：业务类型、字段、枚举和接口承载；用于恢复序列化组件，不作为完整产品逻辑实现。
- `Assets/Plugins`：脚本类型解析所需的程序集依赖。
- `Assets/RuntimeSerialized/Shaders`：17 个 Shader 的完整序列化快照，只读保存，用于参数和运行时数据核对。
- `Assets/Editor/SerializedShaderSupport`：保护序列化 Shader 快照、注册快照对象并恢复 AudioMixer 参数显示名。
- `ProjectAssetLibraryManifest.json`：工程资产数量、生成 Meta 和完整性摘要。

## 资产规模

- 1 个项目入口 Scene
- 86 个 Prefab
- 96 个 AnimationClip
- 2 个 AnimatorController
- 83 个 Material
- 409 张 PNG 纹理
- 55 条 OGG 音频
- 18 个可编辑兼容 Shader
- 17 个完整序列化 Shader 快照
- 437 个脚本/编辑器支持文件
- 55 个依赖程序集
- 3 个入口场景支持数据资产

## 接入边界

- 项目内 `.meta` GUID 是本交付资产库的稳定 GUID，全部引用已经重映射闭合；它们不代表内容制作期源工程 GUID。
- Prefab、粒子模块、动画曲线、材质属性、纹理像素、音频内容和配置数据以当前项目值为默认实现依据。
- `Assets/Scripts` 中的方法体仅提供类型与接口边界。玩法、流程和持久化逻辑应按项目需求文档实现，不应把空方法体当作功能实现。
- 可编辑 Shader 用于项目接入与跨平台适配；完整序列化快照和相邻目录 `../assets/runtime_bundles` 用于保持运行时 Shader 数据基线。内容制作期 Shader 源码未纳入版本资产，因此不得把兼容 Shader 标记为原作者源码。
- 字体以 TextMeshPro FontAsset、Material 和 SDF Atlas 交付；版本资产不含 TTF/OTF 源字体。

## 完整性状态

- 1915 个项目资产文件均有 Meta。
- 2129 个文件/目录 GUID 全部唯一。
- 5066 条 Unity PPtr GUID 引用全部可解析。
- 缺失 Meta、孤立 Meta、重复 GUID、悬空 GUID 均为 0。
- 本机精确版本编辑器不可用；较低版本编辑器的许可证服务在项目导入前退出，因此未把编辑器导入测试标记为通过。首次在 `2022.3.62f3` 打开时必须执行上方检查。
