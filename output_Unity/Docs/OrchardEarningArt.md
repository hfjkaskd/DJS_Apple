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
