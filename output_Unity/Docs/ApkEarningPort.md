# APK 网赚流程移植记录

本文记录 2026-10-14 对指定 APK 的资源核对、Unity 接入范围与验证边界。目标是移植原 APK 的本地演示流程和界面；界面保留 `TEST MODE · NO CASH PAYMENT`，不接真实打款服务，不提交真实账户信息。

## 参考包与证据

| 项目 | 核对结果 |
| --- | --- |
| 用户指定文件 | `C:/Projects/com.nut.sort.relax/Builds/Android/ColorNutParty-1.0.1-20261013-app-icon-fix.apk` |
| 包名 | `com.color.nutparty` |
| versionName / versionCode | `1.0.1` / `1` |
| 文件大小 | `159251988` 字节 |
| SHA-256 | `C0C4E5DE0DACDF0F8E7376C8A270BF2BD11AB6D06B6989994F0858A73266B238` |
| 对应参考工程 | `C:/Projects/com.nut.sort.relax` |
| 构建记录 | 参考工程 `Docs/APKBuild/20261013-app-icon/result.json`，记录的 APK 哈希一致 |

解包与比对证据保存在 `E:/CodexCacheBackups/DJS_Apple-apk-reference-20261014`。11 份抽取配置与参考工程逐字节一致；现金、钻石、条件、顶部、过场等核心 Prefab 的布局、文字组件和 Sprite 引用已核对。源 TXPanel 存在两个同名 `main/Image`：背景为 983×1034，标题为 457×84；比对时分别识别，不能按重名路径覆盖后得出布局漂移结论。纹理对比中已区分 APK 的 ETC2 压缩与原图差异，未声称所有解码像素完全相同。

## 接入位置

- `Assets/Scripts/Assembly-CSharp/ApkEarning/`：本地账本、存档、流程控制、顶部展示和已配置组件绑定。
- `Assets/ApkEarningReference/`：隔离导入的原界面、美术、字体、英文文本与等级映射资料。
- `Assets/Resources/ApkEarningConfig.asset`：数值规则、显示等级映射、测试渠道、钻石预览参数。
- `Assets/res/local/withdrawal/ApkEarningHost.prefab`、`ApkEarningTop.prefab`：游戏内入口、页面挂载与永久测试标记。
- `Assets/Editor/ApkEarningUIAuthoring.cs`：从原静态 Prefab 生成和绑定移植页面；运行时不通过代码重建静态 UI。
- `Assets/Editor/ApkEarningValidation.cs`：Prefab 检查、多分辨率静态渲染与当前运行画面截图工具。

所有点击事件通过代码绑定 Unity 标准 Button。页面从配置好的 Prefab 加载；较大图像按 Resources 路径加载。主游戏输入在提现模态打开期间受阻，避免透过弹窗点击水果。

## 当前流程

1. 首次进入第 1 局展示原欢迎引导，确认后保存已读状态。
2. 前两局结算分别生成 $0.40、$0.60 的演示奖励。领取后经过奖励到账过场、达成提示、现金钱包、测试账户填写、确认与记录页面。未完成的奖励可在重启后继续；提交结果明确为 `Pending - not paid`。
3. 两笔早期记录归档后播放奖励评估、激活、加成、初始余额及记录说明。原规则的 $500 是教程的绝对余额设置，不是重复叠加奖励。
4. 后续水果三消与关卡胜利进入 APK 规则账本，领取行为先落盘再更新画面。视频奖励没有接可用广告服务，保留普通领取入口。
5. 达到钻石解锁局数后，播放配置的钻石奖励预览，再用原手指、提示气泡和钻石卡片进入钻石钱包。预览 30,000 钻石明确标为预览，不写入余额；引导确认状态单独保存。
6. 现金钱包、钻石四档选择、条件、后续关卡说明、测试账户、确认、历史记录之间有实际按钮导航。US 配置对应 Venmo、Zelle 和 PayPal；前两种显示电话输入，PayPal 显示邮箱输入，确认页使用所选渠道图标及对应测试字段。
7. 存档失败时停止发布未保存的新余额，展示独立恢复页面，可重试或返回游戏。顶部现金入口在发生实际存档错误时仍可打开恢复页。

## 数值与显示边界

原现金阶段使用实际局索引 22、51，对应原界面的显示等级 5、30。顶部与钱包共用导入的显示映射；条件列表保留实际已完成局数，并标注 `boards`，防止把累计局数与大关标题混用。

钻石按百分之一单位保存，100 个完整钻石对应演示 $1；四档目标为 50,000 / 100,000 / 150,000 / 200,000 钻石。源流程的登录天数、每日完成局数、账户等级等条件保留。源中缺失的账户等级更新和现金 `MARK` 目标不会凭空设置为达标；未知目标显示等待账户更新。测试模式不会伪造真实到账或向外部支付服务发起请求。

## 保存与恢复

新账本位于 `Application.persistentDataPath/fruit-apk-earning-v1.dat`。文件名保持稳定，当前载荷版本为 2，支持读取版本 1。每次修改先复制候选状态、写入校验和临时文件、原子替换，再通知界面；主文件损坏时尝试完整备份，两份均坏时保留错误而不重置余额。

迁移只读取旧 `fruit-withdrawal-simulation-v1.dat` 的现金和钻石，并结合当前实际游戏关卡；不改写旧文件。已保存的局 ID 与消除序号防止同一局、同一次三消和重复提交重复奖励。账户输入仅用于本次测试界面，不持久化或发送。

## 验证结果与复现

在仓库根目录运行：

```powershell
./Tests/ApkEarning/Run-Tests.ps1
./Tests/ApkEarning/Run-ServiceTests.ps1
./Tests/ApkEarning/Run-FieldLayoutTests.ps1
```

2026-10-14 最近一次结果：领域规则 45 项通过；存档、迁移、桥接、重复操作、失败回滚、渠道校验、显示映射及引导状态 37 项通过。水果区域布局检查遍历 4,336 关与 4 种可用区域，共 17,344 个场景、2,093,304 次 Sprite 边界检查通过；同步实际宽度缩放模型后，最小区域适配缩放为 0.605。运行时代码也已用本机 Unity 2022.3.62f3 的程序集独立编译通过。测试输出在临时目录，不将二进制结果写入仓库。

Unity `BuildAndValidate` 菜单快捷键为 Ctrl+Alt+J；当前运行画面截图为 Ctrl+Alt+K。静态检查报告位于 `E:/CodexCacheBackups/DJS_Apple-apk-validation-20261014/prefab-fixtures-report.json`。2026-10-14 10:11:25 UTC 的检查覆盖 32 个 Prefab、3 种分辨率、18 张静态图，错误数为 0，包含新增的钻石指南和恢复页面。

实际运行已点击验证“钻石指南 → 钻石钱包 → 条件 → 继续游戏”和“现金钱包 → 历史记录”，以及已缓存 Host 的关闭与再次打开。水果三消正常触发幸运奖励，普通 `Claim` 领取 $1.27 后，现金从 $0.20 变为 $1.47，并展示 $1.27 到账过场；撤销道具也已实际操作，水果回到原位置，剩余次数从 2 变为 1。截图保存在 `C:/Projects/DJS_Apple/apk-comparison/game/`；其中 `07-lucky-reward.png` 与对应 JSON 记录 2026-10-14 10:08:42 UTC 的实际幸运奖励页面。其余 JSON 记录各次实际页面状态。这里仅记录已操作的路径，不把代码检查或几何测试当作全部流程的实际操作验证。

原 APK 实际画面保存在 `C:/Projects/DJS_Apple/apk-comparison/reference/`。其中 `05-account.png` 实际是加载页面，`07-confirm.png` 实际是无效姓名提示，不可按文件名误标为账户页或确认页；实际账户页为 `06-account.png`。对比脚本 `Tools/BuildApkComparison.py` 仅使用 JSON 明确配对的真实截图，完整画面等比放入共同的 1080×1920 区域，不裁剪或修图，并输出原始尺寸与 SHA-256 清单。

静态图不是游戏运行截图，也不是 APK 实机截图。`C:/Projects/com.nut.sort.relax/Docs/Localization/20261013/validation/` 内的图是参考工程渲染图；与 APK 的资源核对不能替代真机交互验证。两侧背景玩法、独立存档余额和测试状态说明存在有意保留的差异；本文不把静态检查通过写成全部界面像素级一致。

## 实际截图对比清单

最终配对配置为 `C:/Projects/DJS_Apple/apk-comparison/pairs.json`，输出位于 `C:/Projects/DJS_Apple/apk-comparison/output/`。每组左侧为指定 APK 的真实设备截图，右侧为水果游戏当前存档的 Unity 实际运行截图。APK 原图为 1080×1920，水果游戏原图为 720×1280；仅等比排版，不裁剪界面内容或重绘截图。单页尺寸为 2256×2262。

| 页面 | 原 APK 截图 | 水果游戏实际截图 | 对比图 |
| --- | --- | --- | --- |
| 现金钱包 | `APK-cash-wallet.png` | `05-cash-wallet.png` | [现金钱包](C:/Projects/DJS_Apple/apk-comparison/output/01-cash-wallet.png) |
| 钻石钱包 | `APK-diamond-wallet.png` | `02-diamond-wallet.png` | [钻石钱包](C:/Projects/DJS_Apple/apk-comparison/output/02-diamond-wallet.png) |
| 钻石提现条件 | `APK-diamond-requirements.png` | `03-diamond-requirements.png` | [提现条件](C:/Projects/DJS_Apple/apk-comparison/output/03-withdrawal-requirements.png) |
| 提现记录 | `APK-cash-history.png` | `06-cash-history.png` | [提现记录](C:/Projects/DJS_Apple/apk-comparison/output/04-cash-history.png) |
| 幸运奖励 | `APK-lucky-reward.png` | `07-lucky-reward.png` | [幸运奖励](C:/Projects/DJS_Apple/apk-comparison/output/05-lucky-reward.png) |
| 双货币与关卡入口 | `APK-level4-hud.png` | `04-game-hud.png` | [顶部与玩法](C:/Projects/DJS_Apple/apk-comparison/output/06-game-hud.png) |

[六组对比总览](C:/Projects/DJS_Apple/apk-comparison/output/overview.png) 与 [来源清单](C:/Projects/DJS_Apple/apk-comparison/output/manifest.json) 一并保留。清单记录每幅原图路径、原始尺寸、SHA-256 和实际排版区域。最终钻石钱包与历史页面分别使用 2026-10-14 10:23:45 UTC、10:24:44 UTC 的运行截图，包含档位整数格式、原三行说明与历史余额格式修复；现金钱包包含统计标题绑定修复。

记录数量、余额、条件进度来自两侧各自实际存档，截图之间也存在正常游玩造成的数值变化。没有为了让数字相同而修改存档、补造历史记录或把静态预览图标为实际运行。最终对比不使用 `preview/` 中的修复前图片。
