# Nut Sort 提现流程移植

本模块为明确标注的本地测试流程。没有真实支付、真实到账、账户采集或外部网络请求。新余额从零开始，与原游戏金币分开。

## 已接入流程

游戏内现金／钻石入口 → 钱包选择目标 → 查看全部规则 → 选择测试渠道 → 确认申请 → 条件进度 → 模拟审核 → 模拟完成／测试拒绝 → 记录查询。三消和过关产生待领取奖励；奖励中心一次领取入账。取消／拒绝会退回冻结的奖励单位。

实际页面使用 Nut Sort 导出资料的 Prefab 层级、贴图、字体和按钮资源，替换原业务组件并接入当前游戏的 BaseUI。静态结构和绑定保存在 Prefab；运行时仅加载已配置页面、绑定事件和刷新状态。

## 配置与适配

- `Assets/Resources/WithdrawalConfig.asset`：奖励数值、现金／钻石档位、所有条件与模拟审核时长。
- 原始两条余额线分别适配为测试现金和测试钻石。
- 保留源 0.40／0.60 示例档和钻石 500／1000／1500／2000 档。
- 源隐藏关／二阶段关卡适配为已通过的水果关卡序号；用户等级适配为奖励等级。
- 主现金目标 500、三消／通关奖励和 60 秒审核是明确的本地测试配置，不能声称是已验证的原业务规则。
- 当日通关和签到使用 UTC 日期；每个档位在申请前公开全部条件，申请快照冻结该档位的资格目标与审核时长。
- 签到日所需胜场 `DailyWinsForCheckIn` 与奖励等级换算 `WinsPerUserLevel` 是全局配置，不属于档位申请快照；配置更新后，后续签到判定和等级换算按当前全局配置执行，不能宣称这些全局规则永久固定。
- 没有移植之前已撤销的六阶段提现逻辑。

## 数据与资源

存档：`Application.persistentDataPath/fruit-withdrawal-simulation-v1.dat`，独立版本、原子替换和备份恢复；使用局次 ID 与三消序号处理重复回调。申请提交后金额冻结；终态不能重复打款或重复扣款。

来源：`C:/Projects/Nut Sort Relax/DeviceExports/com.nut.sort.relax_20260907_200801/Recovered/delivery`。该导出不是完整原始 C# 工程；源 GAPS.md 明确说明服务端和真实支付未恢复。

导入资源在 `Assets/WithdrawalImported/Art`，按需加载的页面在 `Assets/Resources/WithdrawalImported/Panels`。`Tools/NutWithdrawalImportManifest.json` 记录来源 GUID、依赖和移除的旧组件；`Tools/NutWithdrawalBindings.json` 记录原节点路径。

重建：先运行 `Tools/ImportNutWithdrawal.py`，再执行 Unity 菜单 `Tools/Withdrawal/Build Imported Flow`。导入会覆盖对应页面；保留人工绑定时使用 `--preserve-panels`。运行时不依赖源导出目录。

UI 构建完成后，在仓库根目录运行 `python Tools/MinimizeWithdrawalHudPatch.py`。该工具以 `9ab6eb0` 的原文为基线，保留 `CorePlayUI.prefab` 原有 YAML 文本、文档顺序和布局参数，仅追加当前提现 HUD 的 PrefabInstance、stripped RectTransform 及根节点子引用，消除 Unity 保存产生的全文件重排。可重复执行；追加 `--check` 只检查是否已最小化。

## 验证

`Tests/Withdrawal/Run-CoreTests.ps1`（Git 根目录下）验证奖励幂等、批量领取、单活动申请、冻结退款、公开条件、签到、审核、条款快照、时钟回拨、重启和坏存档恢复。旧存档三消序号迁移也已覆盖：首次接入时以已有消除组数建立新局次基线，已有 17 组时第 18 组仅发新增 1 组奖励；重启后传入更大或更小的基线均不覆盖已有提现计数，重复事件不补发奖励，负基线不能创建局次。Unity 的编译、页面渲染与交互验证另行执行，核心测试不等于真机或真实支付验收。

原工程已有 `FreeCoinUI` 空脚本警告与本模块无关。本次不接原工程尚未实现的广告接口，也不把广告取消或失败当作奖励完成。

2026-09-16 验证结果：10 项核心测试通过；主工程 Unity 批处理编译通过，最后的 UI 代码也使用主工程实际程序集引用编译通过。11 个页面在 1080×1920 和 1080×2340 下渲染，包含记录列表首尾共 24 张图，使用明确的合成样例覆盖长条件、大金额和终态文案，缺失引用、字体、绑定和文本溢出均为 0。钱包、规则和返回入口已在主工程 Play 中点验；完整状态转换由核心测试验证，未进行 Android/iOS 真机验收。

入口：仅保留关卡内左下角带 TEST 标签的钱图标；首页现金／钻石测试栏与 ADS 按钮已移除。关卡入口与水果、篮子、道具区域的静态坐标已核对。修改后退出并重新进入 Play，加载最终 Prefab 布局。
