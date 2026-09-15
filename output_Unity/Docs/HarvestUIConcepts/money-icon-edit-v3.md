## v3 钞票图标调整

用户要求：图标换成钱。

余额、六个档位、独立领奖和申请金额的奖励图标改成统一绿色卡通钞票。棋盘水果、三消任务水果和普通金币保留原样。图中金额、文案与页面布局保持不变。

落地时新增独立钞票 Sprite，仅用于提现额度；不得全局替换 Fruit_4。确认、最终等待、完成及记录等未在四屏效果图中展开的页面，也应统一使用钞票表示提现额度。

生成方式：内置 image_gen 编辑；尚未改动 Unity 界面。

最终编辑提示词：

Use case: precise-object-edit.
Edit the supplied full four-screen mobile game UI concept board. Change only the reward currency icons into MONEY icons. Preserve the exact composition, four screens, all UI layout, all text, numerical values, colors of panels and buttons, background, and dimensions.

Replace each papaya icon that represents the withdrawal/reward currency with a friendly bright GREEN STACK OF PAPER BANKNOTES: 2-3 rounded cartoon notes, a simple embossed dollar sign on the top note, light mint center, emerald border, soft glossy 2.5D highlights and warm soft shading. Use one visually consistent icon family, matching the existing casual fruit game's toy-like art. This should read immediately as money, not a fruit, not a circular coin. Make the small icons clean and legible at their existing sizes. Money is a fictional game UI symbol; preserve all existing 'SIMULATED' and 'NO CASH VALUE' text exactly.

Exact replacement locations:
1. Leftmost IN-GAME screen: the papaya next to balance 247.61, inside the upper reward panel, becomes a small green banknote stack.
2. WALLET screen: the large papaya next to 247.61 becomes a banknote stack; all SIX papaya icons on the 1,000 / 1,500 / 3,000 / 5,000 / 8,000 / 10,000 target cards become matching small banknote stacks.
3. REWARD screen: the three large papayas surrounded by golden rays above +10.00 become a satisfying composition of three green paper-money stacks, filling the same visual area; also replace the small reward papaya visible in the dimmed HUD behind the popup with the same money symbol.
4. PROGRESS screen: ONLY the papaya next to 'Request: 1,000.00' becomes a small banknote stack.

Critical exceptions: The three papayas DIRECTLY ABOVE 'Make 260 fruit triples' on the fourth screen represent a FRUIT-MATCHING TASK, so KEEP THOSE THREE FRUIT ICONS unchanged. All actual gameplay fruit tiles on the board must remain fruit unchanged. The ordinary gold coin next to the top-right 100 counters on all screens is a different game currency: KEEP those gold coin icons unchanged. Do not change clock, stage nodes, stars, leaves, backgrounds, buttons, or power-up icons. Do not add new text or claims. Keep 33% on the fourth screen and 25% on the second. Preserve the full board with all outer edges visible.

