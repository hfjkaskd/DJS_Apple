"""Full-frame APK / previous Unity / current Unity screenshot comparison.

Run with --ready only after the final Unity render has completed. Reads the nine
1080x1920 originals without retouching/cropping and writes a proportional board
plus a final provenance manifest. This does not run Unity or touch game files.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


HERE = Path(__file__).resolve().parent
REFERENCE = Path("C:/Projects/DJS_Apple/apk-comparison/reference")
PREVIOUS = Path("E:/CodexCacheBackups/DJS_Apple-reference-layout-20261020/after")
CURRENT = Path("E:/CodexCacheBackups/DJS_Apple-layout-geometry-20261021/after")
NATIVE, FRAME = (1080, 1920), (540, 960)
PAD, GAP = 24, 20
TOP, COLUMN_HEADER = 124, 80
ROW_TITLE, ROW_NOTE, ROW_GAP, FOOTER = 48, 46, 28, 112
COLUMNS = [
    ("reference", "参考 APK 原界面", "原 APK 已保存截图", "#d8e8f8"),
    ("previous", "上轮 Unity 预览", "Prefab 测试数据 · 非手机实拍", "#e8e0ef"),
    ("current", "本轮 Unity 预览", "Prefab 测试数据 · 非手机实拍", "#dceedd"),
]
ROWS = [
    {
        "id": "cash-wallet", "title": "现金钱包", "reference": "APK-cash-wallet.png",
        "fixture": "fixture-TXPanel-1080x1920.png",
        "note": "比较标题、金额区、进度、主按钮、统计和历史的位置；余额及统计示例值不同。",
    },
    {
        "id": "diamond-conditions", "title": "钻石提现条件（4 项任务）", "reference": "APK-diamond-requirements.png",
        "fixture": "fixture-LocalWithdrawalConditions--diamond-1080x1920.png",
        "note": "比较固定主卡、标题、4 行任务和主按钮；不同存档的任务数值不代表布局差异。",
    },
    {
        "id": "account", "title": "收款账户", "reference": "06-account.png",
        "fixture": "fixture-TXUserInfoPanel-1080x1920.png",
        "note": "比较渠道选择、说明、输入区和确认入口；字段示例值可能不同。",
    },
]


def font(size, bold=False):
    return ImageFont.truetype("C:/Windows/Fonts/msyhbd.ttc" if bold else "C:/Windows/Fonts/msyh.ttc", size)


def label(draw, value, xy, face, fill="#26394a", width=None, centered=False):
    box = draw.textbbox((0, 0), value, font=face)
    length = box[2] - box[0]
    if width is not None and length > width:
        raise ValueError("Label overflows: " + value)
    x, y = xy
    if centered:
        x += (width - length) / 2
    draw.text((x, y), value, font=face, fill=fill)


def read_image(path):
    before = path.stat()
    raw = path.read_bytes()
    with Image.open(path) as source:
        if source.size != NATIVE:
            raise ValueError(f"Expected a complete 1080x1920 PNG, got {source.size}: {path}")
        picture = source.convert("RGB")
        picture.load()
    after = path.stat()
    if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns):
        raise RuntimeError("Source changed while reading; wait for final Unity completion: " + str(path))
    provenance = {
        "path": str(path), "size": list(NATIVE), "sha256": hashlib.sha256(raw).hexdigest(),
        "bytes": len(raw), "modifiedUtc": datetime.fromtimestamp(after.st_mtime, timezone.utc).isoformat(),
    }
    return picture.resize(FRAME, Image.Resampling.LANCZOS), provenance


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--ready", action="store_true", help="Explicitly confirm the final Unity render is ready")
    parser.add_argument("--current-dir", type=Path, default=CURRENT)
    parser.add_argument("--output-dir", type=Path, default=HERE)
    args = parser.parse_args()
    if not args.ready:
        parser.error("Wait for the final Unity run, then pass --ready. No output has been generated.")

    prepared = []
    for row in ROWS:
        sources = {
            "reference": REFERENCE / row["reference"],
            "previous": PREVIOUS / row["fixture"],
            "current": args.current_dir / row["fixture"],
        }
        prepared.append((row, {column: read_image(path) for column, path in sources.items()}))

    width = PAD * 2 + FRAME[0] * len(COLUMNS) + GAP * (len(COLUMNS) - 1)
    row_height = ROW_TITLE + FRAME[1] + ROW_NOTE
    height = TOP + COLUMN_HEADER + row_height * len(ROWS) + ROW_GAP * (len(ROWS) - 1) + FOOTER
    board = Image.new("RGB", (width, height), "#edf1f5")
    draw = ImageDraw.Draw(board)
    label(draw, "界面布局对比 · 现金 / 条件 / 账户", (PAD, 18), font(36, True))
    label(draw, "9 张原图完整等比排列，不重绘、不裁剪；Unity 为工程 Prefab 测试数据预览。",
          (PAD, 72), font(23), width=width - PAD * 2)
    for i, (_, title, subtitle, fill) in enumerate(COLUMNS):
        x = PAD + i * (FRAME[0] + GAP)
        draw.rounded_rectangle((x, TOP, x + FRAME[0], TOP + COLUMN_HEADER - 10), radius=8, fill=fill)
        label(draw, title, (x, TOP + 6), font(25, True), width=FRAME[0], centered=True)
        label(draw, subtitle, (x, TOP + 42), font(20), width=FRAME[0], centered=True)

    manifest_rows = []
    for row_index, (row, entries) in enumerate(prepared):
        y = TOP + COLUMN_HEADER + row_index * (row_height + ROW_GAP)
        label(draw, row["title"], (PAD, y + 7), font(27, True), width=width - PAD * 2)
        row_record = {"id": row["id"], "title": row["title"], "state_note": row["note"], "sources": []}
        for column_index, (column, _, _, _) in enumerate(COLUMNS):
            picture, metadata = entries[column]
            x = PAD + column_index * (FRAME[0] + GAP)
            board.paste(picture, (x, y + ROW_TITLE))
            row_record["sources"].append({"column": column, **metadata})
        label(draw, row["note"], (PAD, y + ROW_TITLE + FRAME[1] + 9), font(21), "#526576", width=width - PAD * 2)
        manifest_rows.append(row_record)

    footer_y = height - FOOTER + 12
    label(draw, "每列均为完整 1080×1920 原图等比缩至 540×960；不把不同分辨率的像素直接对比。",
          (PAD, footer_y), font(22), "#526576", width=width - PAD * 2)
    label(draw, "该图展示布局与美术差异，不作为已安装手机状态、支付结果或逐像素一致性的证明。",
          (PAD, footer_y + 36), font(22), "#526576", width=width - PAD * 2)

    args.output_dir.mkdir(parents=True, exist_ok=True)
    path = args.output_dir / "cash-conditions-account-screenshot-comparison.png"
    board.save(path)
    manifest = {
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "scope": "Saved original APK screenshots compared with previous and final-current Unity Prefab previews using synthetic test data. Unity previews are not installed-phone captures. No pixel-parity or workflow-parity assertion is made.",
        "processing": "Full-frame 1080x1920 PNGs converted to RGB and proportionally resized to 540x960 with LANCZOS. No crop, redraw, retouch, stretching or source modification.",
        "sourceCount": sum(len(row["sources"]) for row in manifest_rows), "rows": manifest_rows,
        "output": {"path": str(path), "size": [width, height], "sha256": hashlib.sha256(path.read_bytes()).hexdigest()},
    }
    manifest_path = args.output_dir / "screenshot-comparison-manifest.json"
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"figure": str(path), "manifest": str(manifest_path), "sourceCount": manifest["sourceCount"]}, ensure_ascii=False))


if __name__ == "__main__":
    main()
