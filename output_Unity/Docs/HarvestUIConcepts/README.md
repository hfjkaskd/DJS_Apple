# Harvest Rewards 界面效果图

日期：2026-09-15

生成方式：内置 image_gen；设计预览，不是 Unity 实机截图。

最终效果图：[harvest-rewards-ui-v2.png](harvest-rewards-ui-v2.png)

## 视觉方向

- 参考当前游戏截图的蓝天、紫粉花丛、奶油色面板、橙色描边和绿色果冻按钮。
- 游戏入口：余额、提现入口和目标进度整合在一个紧凑组件内。
- 钱包选档：独立余额卡，六个档位采用两列三行，绿色选中态，规则和记录作为次级操作。
- 独立领奖：突出水果奖励与领取按钮，按实际奖励类型展示。
- 阶段进度：六个阶段节点，当前任务、数量进度和等待时间分块展示。
- 金额、档位和任务条件保持现有配置，继续明确标记模拟奖励。

## 实现约束

- 效果图内的棋盘、篮子和道具仅表达背景语境；本次设计范围为提现相关 UI，不据此改动玩法或关卡排列。
- 图中的第一屏和钱包表示未申请状态；第四屏为另一个已申请示例状态。
- Unity 使用现有可切片 Sprite、Prefab 和标准 Button；静态层级与样式配置在 Prefab，事件通过代码绑定。
- 优先复用 PopBg、Btn_Normal、Btn_Orange、Fruit_4 以及当前游戏背景。
- 钱包的示例比例 247.61 / 1000 约为 25%；阶段的 86 / 260 约为 33%。运行时均按实际数据刷新。

## 原始生成提示词

Use case: ui-mockup.
Create a polished high-resolution landscape design presentation showing FOUR distinct portrait mobile game screens side by side, evenly spaced, fully visible, straight-on with NO phone hardware. Large 3840x2160 composition. This is a coherent redesign of an existing Unity casual fruit-matching game's simulated reward / withdrawal flow.
Input image 1 is the authoritative visual style reference: bright blue sky, soft violet and pink flowers, warm cream rounded panels, thin orange bevels, glossy lime green buttons, papaya reward icon and chunky playful brown lettering. Match this exact friendly toy-like casual mobile art style. Image 1 is a reference, not an edit target. Improve hierarchy and spacing, not the game economy.
Overall art direction: high quality 2.5D glossy fruit game UI, restrained soft bevels, warm ivory surfaces with golden orange rims, cocoa brown type, green primary actions, purple floral atmosphere. Large rounded corners, tidy readable typography, generous spacing, selected cards clearly distinguished. Keep decoration tasteful and implementable from sliced sprites. No realistic banknotes, dollar symbols, bank logos, unrelated characters, wood-grain boards, photorealism or flat corporate dashboard styling. Do not put all workflow instructions on one screen.
Presentation background: very pale warm lavender. Above screens a small elegant heading 'HARVEST REWARDS' and subtitle 'UI VISUAL CONCEPT'. Four separate small labels over the screens: '01  IN-GAME', '02  WALLET', '03  REWARD', '04  PROGRESS'.
SCREEN 1 — game screen: blue sky top and purple flowers below. Existing orange gear upper left, centered 'Level 4', gold coin counter upper right. Below it one compact integrated warm cream reward capsule: papaya medallion on left, large '247.61', small clearly legible 'SIMULATED REWARDS', glossy green 'WITHDRAW' button on right. Below this balance row but inside the SAME compact component show '752.39 to next target', a thin green progress bar at 25%, tiny '247.61 / 1,000'. Do not create a second oversized separate banner. Below the HUD leave plentiful space for a central cluster of colorful rounded fruit tiles arranged as the actual matching game, with a basket tray at bottom and three rounded power-up buttons. Treat background gameplay as context; reward HUD is the design focus.
SCREEN 2 — wallet popup: softly dimmed blue/floral game behind a large floating cream orange-rimmed rounded panel with a bright orange title tab 'HARVEST WALLET', small orange X close button at top right. Under the title a small visible line 'SIMULATED · NO CASH VALUE'. Center a recessed pale peach balance card with papaya icon and big '247.61', label 'Available rewards', and small 'In challenge: 0.00'. Heading 'Choose your target'. A tidy 2-column by 3-row grid of SIX compact cream/orange reward target cards, with values exactly '1,000', '1,500', '3,000', '5,000', '8,000', '10,000'. Each has a small papaya icon. The 1,000 card is selected with pale green fill, thick green border and a check badge; the remaining cards are quiet pale ivory with fine orange borders. No ribbons obscuring values. Beneath: '752.39 more to apply' and a 25% green progress bar. Small line '6 stages + final wait after applying'. Large glossy green 'KEEP HARVESTING' button. Quiet small secondary buttons 'Rules' and 'Records' at bottom. Balanced panel height, no giant dead whitespace.
SCREEN 3 — reward popup: the same dimmed floral game backdrop, smaller compact cream orange-rimmed card, with orange title tab 'LEVEL COMPLETE' and X. Small top line 'SIMULATED · NO CASH VALUE'. Cheerful restrained golden rays and 3 papaya icons in center, large '+10.00' and label 'Harvest reward'. Small inset 'Balance after claim: 257.61'. One big glossy green 'COLLECT' button, small gray secondary row 'Ad bonus unavailable'. Compact, satisfying, legible, no real cash imagery. This is a separate interaction, not embedded into wallet.
SCREEN 4 — stage progress popup: same panel and title tab 'HARVEST JOURNEY', X and visible small 'SIMULATED · NO CASH VALUE'. A small top summary 'Request: 1,000.00', 'In challenge'. Horizontal six round nodes connected with a narrow line: checkmark on node1, orange/current node2, quiet neutral nodes3,4,5,6. Below title 'Stage 2 of 6'. A pale peach task inset with three matching papayas, 'Make 260 fruit triples', large '86 / 260', green bar at 33%. Separate compact clock inset with tiny clock icon, label 'Stage wait', value '18:42:10'. Small clear line 'Complete both the task and timer'. Large green 'CONTINUE PLAYING' button. Three quiet footer actions 'Back', 'Rules', 'Records'. This state is a separate illustrative later step; it need not share the pre-application balance of screen1.
All text must be spelled exactly as specified. Screen mockups should have sharp consistent UI styling and readable large text. Render the full outer edges of all four screens without clipping. No arrows between the screens, no added mechanics, no floating fake tap indicators.

## 最终修正提示词

Use case: precise-object-edit. This image is the edit target. Preserve the entire four-screen mockup board exactly: layout, all art, background, headers, typography, every other number, all other UI. Make ONLY ONE local text correction: on the far-right fourth screen, in the task card immediately under '86 / 260', change the small percent label to the RIGHT of that card's green progress bar from '25%' to '33%'. The bar already looks about one third full, preserve its appearance. The wallet screen's separate '25%' MUST stay 25% because 247.61 / 1000 rounds to 25%. Do not change gameplay fruit tiles or any other text. Output the same entire full board, all edges visible. Sharp high-quality UI mockup.

