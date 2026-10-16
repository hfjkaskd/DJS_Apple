# 苹果树提现界面美术

本次只调整已接入流程的静态美术，保留现金、钻石、提现条件、奖励领取、记录、存档及测试状态。

## 视觉方向

- 使用原水果游戏的奶油色面板、苹果橙边框、叶绿色按钮和圆润字体。
- 现金与钻石仍保留各自图标，苹果与叶片仅作为果园装饰。
- 替换螺母主题装饰。页面继续使用已配置的 Prefab、标准 Button 和代码事件绑定。
- 原画通过正确的九宫格边界适配；装饰保留比例，不把完整图片拉伸成界面。

## 新素材

`Assets/OrchardEarningArt/AppleCrest.png` 由内置 image_gen 生成，透明背景；参考为本工程实际水果关卡截图 `C:/Projects/DJS_Apple/apk-comparison/game/04-game-hud.png`。项目使用的文件已复制进入工程。

生成提示词：

> Use case: stylized-concept. Asset type: one production-ready transparent UI ornament sprite for a Unity mobile apple-orchard fruit puzzle game. Reference image 1 is ONLY a style reference for the game's soft polished 3D fruit and foliage, not an edit target. Create a compact, charming orchard crest: three shiny ripe red apples, the center apple slightly larger, with short brown stems and a restrained fan of lush fresh green leaves behind them. Front-facing slightly elevated view, rounded toy-like forms, vivid fruit colors, warm upper-left highlights, soft painted 3D shading that matches the fruit game. Broad low composition about 2.5:1, all objects grouped together, a clean readable silhouette with a shallow curved bottom so the ornament sits above an orange rounded title plaque. Apples only, no other fruits. Single isolated ornament, no panel, no lettering, no numbers, no currency symbol, no watermark, no screws, no nuts, no scenery, no ground plane, no rectangle/background or baked checkerboard. Transparent pixels outside the ornament. Keep all leaves and stems within the image with roughly 7% empty transparent margin. Final sprite should still read at 180px width.

## 美术配置与维护

主题配置：`Assets/OrchardEarningArt/OrchardEarningTheme.asset`。在 Inspector 调整资源路径、九宫格像素倍率、字体颜色和装饰参数，然后在停止 Play 时执行 `Tools > Apk Earning > Apply Orchard Art and Validate`（Ctrl+Alt+O）。改动保存到 Prefab；运行时不动态构建美术结构。原有 `Build` 作者工具会在生成页面后重新应用已启用的主题。

复用原游戏 `res/local/common/sprite` 中的 `PopBg`、`Btn_Normal`、`Btn_Orange`、`PopClose_top`、进度条和里程碑素材，标题使用 `802-CAI978 SDF`。苹果饰图按比例显示，透明纹理最大尺寸为 1024，禁用 mipmap；主体面板使用九宫格。钻石选档的两个 Resources 图片也同步更换，避免点击切档时恢复原版图片；两种底色上的金额统一深棕。

现金、钻石、支付渠道图标、奖励数值、提现条件与代码事件绑定保留。钻石说明中的强调色读取 PageView 的 `EmphasisColor`，以保持奶油底上的可读性。主题作者工具位于 Editor 目录，运行时不依赖 Editor 分支。

## 验证与实际对比（2026-10-14）

- Unity 2022.3.62f3 成功编译并运行。32 个 Prefab 审计及 33 张静态多分辨率渲染完成，报告错误为 0。覆盖 1080×1920、1080×2340、720×1600；报告另保留原 CorePlayUI 字体、静态隐藏序列和输入占位文本的提示，不能据此声称所有旧界面均无警告。
- 在当前关卡实际点击并验证现金钱包、历史记录、钻石钱包、500/1,000 档位切换、条件页及返回；通过正常水果三消触发幸运奖励，普通领取后余额更新并自动返回游戏。未通过修改存档构造截图。
- 6 组修改前后对比保存于 `C:/Projects/DJS_Apple/orchard-art-review/output/index.html`，总览为同目录 `overview.png`。左右均为 Unity 实际运行截图；修改前来自此前保留的运行采集，修改后对应 `after/*.json` 的页面标识和时间。余额、奖励及日期存在正常运行产生的差异。
- 原始截图位于 `before/`、`after/`，配对配置为 `pairs.json`，采集与静态验证报告保存在该审阅目录。对比工具只进行等比排版，不裁切、不修饰原始游戏画面。静态 Prefab 渲染不冒充运行截图。
- 本轮提交为 Unity 工程美术更新，未重新构建 Android APK。测试状态与无真实打款说明继续保留。

## 立体感微调

沿用同一套苹果树素材，适度降低九宫格的像素倍率，使原图已有的橙色倒角、奶油内高光和绿色按钮下缘重新显现。主面板、大内卡、小输入框及 HUD 分别配置倍率，避免小控件变成胶囊形。大内卡略微调暖，与主面板拉开层次。

轻投影通过标准 Unity UI `Shadow` 配置在原有 Image 对象上，颜色、偏移和开关保存在主题的 SpriteRule 中。作者工具只在 Editor 保存 Prefab；运行时复用已有结构。每个适用 Graphic 仅单层阴影，不引入实时灯光、模糊材质、额外大纹理或逐帧逻辑。保留原 Button 点击区域、货币标识和测试状态。

本次微调在 Unity 中重新编译、应用并验证：32 个 Prefab 审计、33 张三种分辨率渲染的错误为 0；保留上述既有提示。实际打开现金钱包、钻石钱包及游戏顶部栏，验证 500/1,000 档位往返切换，三次运行采集的错误均为 0。

3 组真实运行对比位于 `C:/Projects/DJS_Apple/orchard-depth-review/output/index.html`，原始图片及采集 JSON 位于该目录上级的 `before/`、`after/`；静态报告为 `prefab-fixtures-report.json`。左右分别为上一版苹果树美术和本次立体感微调，均为 Unity 运行图。本轮未重新构建 Android APK。

## 已确认的紧凑现金钱包（2026-10-16）

现金钱包按确认稿改为一张奶油色卡片，使用橙色标题、苹果白花饰图、苹果钞票图标、浅桃色进度区域和蓝色主按钮。今日统计及历史入口收进主卡，减少原有木框堆叠。现金、关卡进度及统计仍来自真实存档，提现条件和路由逻辑保留；测试说明显示在卡片顶部。

布局配置保存在 `Assets/OrchardEarningArt/CashWallet/CashWalletArtTheme.asset`，可调整位置、尺寸、字体、描边及资源裁切。停止 Play 后执行 `Tools > Apk Earning > Apply Approved Cash Wallet and Validate`（Ctrl+Alt+U）。新美术保存到原 `TXPanel.prefab`，保持原标准 Button 和别名绑定，运行时仅刷新数据与进度。整体主题的 Apply/Build 最后会重新应用本现金钱包配置。

9 张透明原画及生成记录保存在同目录。通过 SpriteRect 去除透明留白，不改写原 PNG；纹理限制为 256–1024，禁用 mipmap 和 Read/Write。高光及体积光影直接来自素材，没有新增实时模糊、灯光或逐帧静态 UI 创建。进度填充用固定最大宽度乘实际进度，保持圆头；旧页面仍使用 fillAmount。

本轮 Unity 编译成功，32 个 Prefab 审计和 33 张多分辨率静态渲染通过，错误为 0，保留既有 21 条旧资源提示。实际点击验证现金入口、历史及返回、提现条件及返回、关闭及重新打开；运行采集错误为 0，三个前景 Button 中心可命中。验证未修改存档。

对比包位于 `C:/Projects/DJS_Apple/orchard-wallet-redesign-review/output/index.html`，并排图为 `comparison.png`。左侧是用户确认的生成效果图，右侧为 `actual/cash-wallet.png` 的 Unity 运行截图；实际余额 $3.15、钻石 102、Level 4、进度 3/5。只等比排版，不修饰或替换截图。字体、进度条及背景 HUD 仍有细节差异，不宣称像素级一致。本轮未重新构建 Android APK。
