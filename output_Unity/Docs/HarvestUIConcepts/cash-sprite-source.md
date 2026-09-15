# HarvestCash 图标

文件：`Assets/res/local/harvest/sprite/HarvestCash.png`

生成方式：内置 image_gen，依据 v3 效果图生成独立透明 PNG。原始图 1254 × 1254、RGBA；Unity 按 Sprite 导入，最大 512 × 512，无 Mipmap、不可读，所有提现界面复用同一张贴图。

使用范围：提现余额、额度档位、申请金额、领奖、最终等待和完成页。水果任务与游戏水果仍使用原水果资源；普通金币图标不受影响。

提示词：

Use case: precise-object-edit / game UI asset extraction.
Input is an approved mobile game UI mockup board, used as reference for the money icon style ONLY.
Create ONE isolated production-ready 2D sprite of the same green cartoon stack of banknotes used beside '247.61' in its wallet panel. The user needs this asset for Unity, not another mockup.
Subject: a single compact neat stack of 3 green paper banknotes, rounded corners, tilted three-quarter view, light mint inset on top, embossed simple dollar glyph on top, emerald green side edges, friendly bright glossy 2.5D mobile casual-game style. Match the reference wallet money icon's silhouette, color, angle and material. Not three separate piles and no surrounding icons.
Composition: square canvas, icon centered, tightly fill approximately 84% of canvas with transparent padding on all sides; no clipped edges.
Background: genuinely transparent alpha. No background colors, no gradient, no checkerboard pattern. No floor plane or cast ground shadow; mild contact shading only between the three notes, and soft dimensional highlights on the icon itself. Sharp clean anti-aliased edges. No labels, numbers, extra text, UI panels or frames. Single reusable asset, clear at small sizes.

