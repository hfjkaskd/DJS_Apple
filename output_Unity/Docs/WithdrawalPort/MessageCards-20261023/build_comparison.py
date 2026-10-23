"""Compose existing screenshot evidence. Never modify or overwrite source artwork."""

from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parent
CONCEPT = Path(
    "C:/Users/pc/.codex/generated_images/01a0a207-4e93-7ed2-952e-9872c49f4ddc/"
    "exec-bfeb85b1-2a23-4351-98a1-7d3a076560b5.png"
)
STATES = (
    ("", "奖励说明", "Assessment"),
    ("--activating", "奖励激活中", "Activating"),
    ("--activated", "激活完成", "Activated"),
    ("--summary", "记录说明", "Summary"),
)
BG = "#f3f0e9"
INK = "#342719"
MUTED = "#71675a"
LINE = "#d9d0c1"
FONTS = {
    "normal": Path("C:/Windows/Fonts/msyh.ttc"),
    "bold": Path("C:/Windows/Fonts/msyhbd.ttc"),
}


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    path = FONTS["bold" if bold else "normal"]
    if not path.is_file():
        raise FileNotFoundError(f"Chinese caption font missing: {path}")
    return ImageFont.truetype(str(path), size)


def source(folder: str, suffix: str) -> Path:
    return ROOT / folder / f"fixture-LocalEarningSequence{suffix}-1080x1920.png"


def text(
    draw: ImageDraw.ImageDraw,
    xy: tuple[int, int],
    value: str,
    size: int,
    *,
    bold: bool = False,
    fill: str = INK,
    anchor: str = "la",
) -> None:
    draw.text(xy, value, font=font(size, bold), fill=fill, anchor=anchor)


def screenshot(canvas: Image.Image, path: Path, rect: tuple[int, int, int, int]) -> None:
    """Contain the entire screenshot in its box, without cropping or retouching."""
    left, top, width, height = rect
    with Image.open(path) as image:
        image = ImageOps.contain(image.convert("RGB"), (width, height), Image.Resampling.LANCZOS)
        x = left + (width - image.width) // 2
        y = top + (height - image.height) // 2
        canvas.paste(image, (x, y))
        draw = ImageDraw.Draw(canvas)
        draw.rectangle((x - 1, y - 1, x + image.width, y + image.height), outline=LINE, width=1)


def save(canvas: Image.Image, name: str) -> Path:
    target = ROOT / name
    canvas.save(target, optimize=True)
    return target


def four_after() -> Path:
    margin, gap, width, height = 32, 24, 420, 747
    canvas = Image.new("RGB", (margin * 2 + width * 4 + gap * 3, 1000), BG)
    draw = ImageDraw.Draw(canvas)
    text(draw, (margin, 24), "四个说明界面 · 游戏内实现预览", 34, bold=True)
    text(draw, (margin, 78), "沿用已批准的现金与苹果插画、厚木金边横牌；保留各步骤独立说明。", 21, fill=MUTED)
    for index, (suffix, caption, english) in enumerate(STATES):
        x = margin + index * (width + gap)
        text(draw, (x + width // 2, 129), f"{index + 1:02d}  {caption}", 25, bold=True, anchor="ma")
        text(draw, (x + width // 2, 167), english, 18, fill=MUTED, anchor="ma")
        screenshot(canvas, source("after", suffix), (x, 204, width, height))
    text(draw, (margin, 970), "Unity Prefab 真实相机渲染 · 原始分辨率 1080×1920 · 合成预览数据 · 非手机截图", 18, fill=MUTED)
    return save(canvas, "four-messages-after.png")


def before_after() -> Path:
    margin, label_width, gap, width, height = 30, 104, 20, 360, 640
    total_width = margin * 2 + label_width + width * 4 + gap * 3
    canvas = Image.new("RGB", (total_width, 1545), BG)
    draw = ImageDraw.Draw(canvas)
    text(draw, (margin, 25), "四个说明界面 · 修改前后对比", 32, bold=True)
    text(draw, (margin, 76), "同分辨率完整画面并列，无裁切；上行为原界面，下行为新版 Prefab。", 20, fill=MUTED)
    for index, (_, caption, _) in enumerate(STATES):
        x = margin + label_width + index * (width + gap)
        text(draw, (x + width // 2, 122), caption, 25, bold=True, anchor="ma")
    for row, (folder, label) in enumerate((("before", "修改前"), ("after", "修改后"))):
        top = 174 + row * 675
        text(draw, (margin + 42, top + 284), label, 23, bold=True, anchor="ma")
        for index, (suffix, _, _) in enumerate(STATES):
            x = margin + label_width + index * (width + gap)
            screenshot(canvas, source(folder, suffix), (x, top, width, height))
    text(draw, (margin, 1513), "Unity Prefab 真实相机渲染 · 合成预览数据 · 非手机截图；本图仅拼接已有截图。", 18, fill=MUTED)
    return save(canvas, "before-after.png")


def concept_comparison() -> Path:
    margin, gap, width, height = 32, 32, 530, 943
    canvas = Image.new("RGB", (margin * 2 + width * 2 + gap, 1150), BG)
    draw = ImageDraw.Draw(canvas)
    text(draw, (margin, 24), "奖励说明 · 已批准效果图与游戏实现", 31, bold=True)
    labels = ((CONCEPT, "已批准效果图", "美术概念参考"),
              (source("after", ""), "Unity 实际 Prefab", "相机渲染 · 合成预览数据 · 非手机截图"))
    for index, (path, title, detail) in enumerate(labels):
        x = margin + index * (width + gap)
        text(draw, (x + width // 2, 82), title, 27, bold=True, anchor="ma")
        text(draw, (x + width // 2, 124), detail, 18, fill=MUTED, anchor="ma")
        screenshot(canvas, path, (x, 162, width, height))
    text(draw, (margin, 1119), "两张完整竖屏按相同比例区域展示；无裁切、修饰或替换截图内容。", 18, fill=MUTED)
    return save(canvas, "assessment-concept-comparison.png")


def main() -> None:
    inputs = [CONCEPT] + [source(folder, suffix) for folder in ("before", "after") for suffix, _, _ in STATES]
    for path in inputs:
        if not path.is_file():
            raise FileNotFoundError(f"Required source is not ready: {path}")
        with Image.open(path) as image:
            image.verify()
    outputs = [four_after(), before_after(), concept_comparison()]
    manifest = {
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "scope": "Only existing images composited with labels; source files unchanged. No reference APK or phone screenshots synthesized.",
        "sources": [
            {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
            for path in inputs
        ],
        "outputs": [str(path) for path in outputs],
    }
    (ROOT / "comparison-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(manifest["outputs"], ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
