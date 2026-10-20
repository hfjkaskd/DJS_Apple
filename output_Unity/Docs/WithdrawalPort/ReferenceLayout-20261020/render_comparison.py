"""Full-frame layout comparisons; run only after the final Unity preview is ready.

Only reads source images and writes files next to this script. Every source must
be a complete 1080x1920 frame. No source retouching, crop or aspect-ratio changes.
"""
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parent
FRAME = (540, 960)
PAD, GAP = 24, 18
HEADER, COLUMN_HEADER = 112, 84
ROW_TITLE, ROW_FOOTER, ROW_GAP = 72, 34, 24
ROW_HEIGHT = ROW_TITLE + FRAME[1] + ROW_FOOTER
FOOTER = 100


def font(size, bold=False):
    path = "C:/Windows/Fonts/msyhbd.ttc" if bold else "C:/Windows/Fonts/msyh.ttc"
    return ImageFont.truetype(path, size)


def line(draw, text, x, y, width, face, color, center=False):
    bounds = draw.textbbox((0, 0), text, font=face)
    length = bounds[2] - bounds[0]
    if length > width:
        raise ValueError("Label would overflow: " + text)
    if center:
        x += (width - length) / 2
    draw.text((x, y), text, font=face, fill=color)


def load_inputs(plan):
    prepared = []
    for figure in plan["figures"]:
        rows = []
        for row in figure["rows"]:
            entries = []
            for column in figure["columns"]:
                path = Path(row["sources"][column["id"]])
                with Image.open(path) as original:
                    native_size = original.size
                    picture = ImageOps.exif_transpose(original).convert("RGB")
                    picture.load()
                if picture.size != (1080, 1920):
                    raise ValueError(f"Expected full 1080x1920 source, got {picture.size}: {path}")
                entries.append((column, path, native_size, picture))
            rows.append((row, entries))
        prepared.append((figure, rows))
    return prepared


def render(figure, rows):
    columns = figure["columns"]
    width = PAD * 2 + len(columns) * FRAME[0] + (len(columns) - 1) * GAP
    height = HEADER + COLUMN_HEADER + len(rows) * ROW_HEIGHT + (len(rows) - 1) * ROW_GAP + FOOTER
    image = Image.new("RGB", (width, height), "#e9edf2")
    draw = ImageDraw.Draw(image)
    line(draw, figure["title"], PAD, 20, width - PAD * 2, font(38, True), "#233447")
    line(draw, "完整原图等比排列；仅比较信息分区、顺序和视觉层级。", PAD, 76,
         width - PAD * 2, font(23), "#526174")
    for i, column in enumerate(columns):
        x = PAD + i * (FRAME[0] + GAP)
        draw.rounded_rectangle((x, HEADER, x + FRAME[0], HEADER + COLUMN_HEADER - 10),
                               radius=10, fill=column["header_color"])
        line(draw, column["label"], x, HEADER + 8, FRAME[0], font(25, True), "#233447", True)
        line(draw, column["subtitle"], x, HEADER + 43, FRAME[0], font(20), "#526174", True)

    record = {
        "title": figure["title"],
        "output": figure["output"],
        "image_size": [width, height],
        "columns": [{k: v for k, v in column.items() if k != "header_color"} for column in columns],
        "notes": figure["notes"],
        "rows": [],
    }
    for row_index, (row, entries) in enumerate(rows):
        top = HEADER + COLUMN_HEADER + row_index * (ROW_HEIGHT + ROW_GAP)
        line(draw, row["label"], PAD + 2, top + 3, width - PAD * 2, font(28, True), "#233447")
        line(draw, row["note"], PAD + 2, top + 41, width - PAD * 2, font(20), "#526174")
        row_record = {"id": row["id"], "label": row["label"], "note": row["note"], "images": []}
        for i, (column, path, native_size, picture) in enumerate(entries):
            x, y = PAD + i * (FRAME[0] + GAP), top + ROW_TITLE
            image.paste(picture.resize(FRAME, Image.Resampling.LANCZOS), (x, y))
            line(draw, "1080 × 1920 原图 · 完整显示", x, y + FRAME[1] + 8,
                 FRAME[0], font(17), "#526174", True)
            row_record["images"].append({
                "column": column["id"],
                "source": str(path),
                "native_size": list(native_size),
                "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                "fitted_rectangle": [x, y, FRAME[0], FRAME[1]],
            })
            picture.close()
        record["rows"].append(row_record)
    y = height - FOOTER + 8
    for index, note in enumerate(figure["notes"]):
        line(draw, note, PAD, y + index * 30, width - PAD * 2,
             font(22 if index == 0 else 20, index == 0),
             "#784810" if index == 0 else "#526174")
    image.save(ROOT / figure["output"])
    image.close()
    return record


def main():
    plan = json.loads((ROOT / "comparison-plan.json").read_text(encoding="utf-8"))
    # Validate every input before producing either table.
    prepared = load_inputs(plan)
    manifest = {
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "status": "rendered",
        "scope": plan["scope"],
        "processing": "Full-frame EXIF-oriented RGB images; strict 1080x1920 input; "
                      "aspect-preserving LANCZOS resize to 540x960; no crop, retouch, "
                      "source modification or stretching.",
        "frame_size": list(FRAME),
        "figures": [render(figure, rows) for figure, rows in prepared],
    }
    (ROOT / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    for figure in manifest["figures"]:
        print(ROOT / figure["output"])
    print(ROOT / "manifest.json")


if __name__ == "__main__":
    main()
