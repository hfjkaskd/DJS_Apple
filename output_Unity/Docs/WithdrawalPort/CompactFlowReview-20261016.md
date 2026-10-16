# 苹果树提现流程整套界面检查（2026-10-16）

本次把已确认的紧凑现金钱包风格扩展到整套提现流程：奶油色主卡、橙色标题和倒角、苹果饰图、蓝色主按钮、浅色次级操作。现金、钻石、任务条件、账户校验、奖励领取、存档和历史记录继续使用已有流程；界面保留测试状态及无真实支付说明。

## 覆盖范围

当前活动流程包含 **18 个页面或提示 Prefab**。其中现金钱包 `TXPanel` 已在上一轮完成并验证，本轮新增主题配置保存另外 17 个活动页面，同时保存 3 个动态列表/奖励子项和 1 个奖励来源模板，共 **21 个 Prefab**。

| 分组 | 活动页面 / 提示 Prefab |
| --- | --- |
| 钱包 | `TXPanel`（上一轮）、`TXCoinPanel` |
| 条件与进度 | `LocalWithdrawalConditions`、`TXProgress0Panel` |
| 账户与确认 | `TXUserInfoPanel`、`TXUserInfoSurePanel`、`TXChannelPanel`、`LocalWithdrawalVerification` |
| 历史、恢复与提示 | `LocalCashHistory`、`LocalEarningRecovery`、`Tip` |
| 奖励与引导 | `LocalSuccess`、`LocalLuckyReward`、`TXRecordGuidePanel`、`TXGuideTargetCompletePanel`、`LocalDiamondUnlockGuide`、`TXGuideLoadingPanel`、`LocalEarningSequence` |

同时保存的子项为 `LocalCashHistoryRow`、`Item_Big`、`LocalLuckyItem`；来源模板为 `SuccessPanel`。其余保留模板纳入静态检查，但不应算作新增活动流程页面。

主要调整包括钻石钱包的 2×2 金额档位、七行提现条件、紧凑历史列表、支付渠道及账户输入、账户确认与提交验证、奖励和引导展示。账户页原本隐藏的白色覆盖图保持隐藏，避免挡住渠道图标；历史待处理状态补齐对象别名；钻石档位使用主题选中色，切换时不会恢复旧按钮素材；普通奖励领取保留清晰可见的文字按钮。

## 配置和应用

整套布局和样式保存于 `Assets/OrchardEarningArt/CompactEarningTheme.asset`。位置、尺寸、文本大小、素材、显隐保护和列表参数可在 Inspector 中调整。初始化配置由以下 Editor 文件提供：

- `Assets/Editor/CompactWalletLayouts.cs`
- `Assets/Editor/CompactAccountLayouts.cs`
- `Assets/Editor/CompactRewardLayouts.cs`
- 公共配置与应用器：`CompactEarningTheme.cs`、`CompactEarningLayout.cs`、`CompactEarningArtAuthoring.cs`

停止 Play 后执行 `Tools > Apk Earning > Apply Complete Compact Flow and Validate`，快捷键为 **Ctrl+Shift+Alt+B**。该操作读取已保存配置，写回 Prefab 并重新渲染检查图。

`Tools > Apk Earning > Restore Compact Layout Presets and Validate`（**Ctrl+Shift+Alt+N**）会用初始化配方重建配置中的页面列表，覆盖已手工调整的页面布局；常规修改使用 Apply 即可。现金钱包继续由 `Assets/OrchardEarningArt/CashWallet/CashWalletArtTheme.asset` 维护。

原画复用 `Assets/OrchardEarningArt/CashWallet/`。静态结构保存在 Prefab，使用标准 Button 和代码绑定事件；运行时只刷新数据、显隐、进度与已配置子项。没有新增反射、Editor 专用运行兜底、实时灯光、模糊或逐帧静态 UI 构建。

## 静态验证结果

检查快照来自 `prefab-fixtures-report.json`，生成时间为 **2026-10-16T08:31:35.5860588Z**：

- 32 个 Prefab 审计记录。
- 35 个页面/流程状态，覆盖 1080×1920、1080×2340、720×1600，共 **105 张 Unity Prefab 渲染图**。
- 报告 **0 项错误**。
- 保留 **15 项 legacy CorePlayUI 警告**，均为提现导入界面审计范围以外的旧游戏文字字体或材质引用提示；不能据此声称整个工程无警告。

示例覆盖现金/钻石条件、达标状态、历史待处理记录、电话与邮箱账户、提交验证、奖励领取及激活流程。重点页面和窄屏分支已经目视检查；八页总览中未见明显文字遮挡、越界或按钮重叠。静态预览使用隔离场景及明确构造的示例数据，不初始化账户服务、玩家存档或支付操作，因此不能替代运行交互验证。

## 图片来源和审阅包

本轮图库中的前后原图均为 **Unity Prefab 渲染 / 示例数据**，不是 APK 原界面，也不是玩家实际运行截图。修改前后示例金额、日期和状态可能不同，对比只用于检查布局与美术。

本机审阅目录为 `C:/Projects/DJS_Apple/orchard-full-flow-review/`：

- `output/index.html`：105 张静态预览，可按分辨率筛选、搜索并打开原图。
- `output/overview.png`：钻石、条件、历史、账户、确认、验证、进度、奖励八页总览。
- `output/comparison.png`：钻石、条件、账户、历史四页修改前后对比。
- `output/prefab-fixtures-report.json`：本次检查报告副本。
- `output/manifest.json`：源路径、生成时间、原始尺寸及 SHA-256。
- `build_gallery.py`：可重复打包脚本；只复制原图、等比例缩放和拼版，不裁切、改色、重绘或替换截图文字。

原始渲染输入为 `E:/CodexCacheBackups/DJS_Apple-apk-validation-20261014/`，修改前证据保存在审阅目录的 `before-fixtures/`。审阅包属于本机生成资料，以上绝对路径不表示文件已提交进 Git。

## 运行验证状态

上一轮紧凑现金钱包已有 Unity 实际运行图及采集 JSON，位于 `C:/Projects/DJS_Apple/orchard-wallet-redesign-review/actual/`；关闭、提现条件、历史记录和返回已验证。这些证据对应上一轮现金钱包，不能用于宣称其他新页面已完成运行验证。

本轮已使用当前存档完成 Unity Play Mode 实际点击检查：现金钱包 → 历史记录 → 关闭返回钱包 → 提现条件 → 继续游戏；钻石钱包 → 选择 $1,000 档位 → 对应说明与进度刷新 → 钻石提现条件。记录列表只有一条时已改为顶部排列，并重新运行采集确认。

已收录五张 720×1280 运行原图及同名 JSON：`cash-wallet`、`history`、`cash-conditions`、`diamond-wallet-tier-1000`、`diamond-conditions`。每张采集报告均为 0 errors，所有活动前景按钮的中心射线均可命中；被模态窗口遮挡的背景 HUD 按钮不计作故障。最后一张历史记录生成于 **2026-10-16T08:34:08.8711599Z**。账户提交、奖励激活等当前存档未到达的状态使用静态示例检查，不据此宣称完整流程均已实际操作。

原图位于 `orchard-full-flow-review/actual/`，可在图库独立的“游戏运行采集”区域查看。`output/actual-overview.png` 为四个主要运行页面的等比拼版。本轮尚未重新构建 Android APK。

## Git 中的审阅证据

同名目录 `CompactFlowReview-20261016/` 保存三张总览/对比拼版、五张运行原图及 JSON、静态检查报告与来源清单。完整 105 张原尺寸静态渲染仍保存在上述本机审阅图库中。实施提交为 `218de4f`。
