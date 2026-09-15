# Unity 还原与验证记录

2026-09-15，Unity 2022.3.62f3。

[打开设计与实现并排对照](review.html)。四张右侧图片由工程实际 Prefab 渲染，分辨率 1119×2268；示例状态为余额 247.61、目标 1,000、任务 86/260。背景使用项目中的 Map1008 与第四关水果。它们是固定状态的 Unity 参考渲染，另有编辑器 Play Mode 点击验证，不是 Android/iOS 实机截图。

| 页面 | Unity 渲染 |
| --- | --- |
| 游戏内入口 | [hud-reference.png](hud-reference.png) |
| 钱包 | [wallet-reference.png](wallet-reference.png) |
| 独立领奖 | [popup-reference.png](popup-reference.png) |
| 阶段任务 | [progress-reference.png](progress-reference.png) |

本轮重做专用圆角切片、进度条、选中框、时钟、节点、光芒与静态圆润字体，按效果图坐标配置两个弹窗。钱包与详情页使用各自底栏；规则从顶部阅读；遮罩覆盖整个屏幕。HUD 为适配真实顶部按钮与所有已提供关卡，压缩了内部空隙，并将顶部按钮栏上移 28 个逻辑单位。棋盘、收集篮与玩法坐标保持本轮修改前状态。不同屏幕比例按可用空间等比适配，不宣称各设备逐像素相同。

验证结果：

- `HarvestFontAuthoring.BuildAll`：两个静态字体图集烘焙通过，无缺失必需字符。
- `Tests/HarvestRewards/Run-Tests.ps1`：21 项通过，0 失败。
- `HarvestRewardValidation.ValidateAndRender`：28 个长短屏和流程状态通过；金额、已完成档、规则及各类奖励文本无溢出。
- 本地 1668 份关卡配置、208929 个水果矩形检查通过；HUD 最小间距 20.9、篮子间距 47.9 个逻辑单位。未提供本地配置的关卡不在此审计范围。
- `HarvestRewardValidation.RenderReferenceComparison`：四张参考渲染通过，标题、金额、任务文字与全屏遮罩检查通过。
- Play Mode：进入第四关、打开钱包、切换到 1,500 档显示差额 1,252.39 / 17%、规则与记录页打开返回、余额不足时继续当前关卡均正常。返回规则后保留选档。参考渲染使用固定余额；Play Mode 现场截图按当前存档显示。

本轮日志位于工作区根目录：`unity-harvest-reference-fonts.log`、`unity-harvest-reference-audit3.log`、`unity-harvest-reference-render2.log`、`unity-harvest-reference-interactive.log`。仍为模拟奖励，核心经济与阶段规则未改动。
