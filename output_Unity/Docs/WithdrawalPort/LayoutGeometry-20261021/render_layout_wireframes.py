"""Read post-render Unity measurements and draw geometry-only comparison boards.

Run only after the parent marks this run's JSONs ready. This reads source JSONs
and original screenshot metadata/hashes, writes only to --output-dir, and never
modifies game files, scenes, source PNGs or UI geometry.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


WIDTH, HEIGHT = 540, 960
SCALE = WIDTH / 1080
PAD, GAP = 24, 20
TITLE_HEIGHT, ROW_TITLE, ROW_GAP, FOOTER = 116, 48, 42, 162
BLUE, ORANGE = "#1672c4", "#cc5a11"
PAGES = [
    ("TXPanel", "", "现金钱包", ["mainCard", "titlePlate", "amountCard", "progressTrack", "action", "stats", "history"]),
    ("LocalWithdrawalConditions", "diamond", "钻石提现条件", ["mainCard", "titlePlate", "row[0]", "row[1]", "row[2]", "row[3]", "action"]),
    ("TXCoinPanel", "", "钻石钱包", ["balanceCard", "amountCard", "tierCard", "tier[0]", "tier[1]", "tier[2]", "tier[3]", "conditions", "action"]),
]
OLD_ROOT = Path("E:/CodexCacheBackups/DJS_Apple-reference-layout-20261020/after")
OLD = {
    "TXPanel": {
        "mainCard": (52, 210, 976, 1041), "titlePlate": (164, 120, 752, 155),
        "amountCard": (117, 336, 846, 308), "progressTrack": (122, 720, 836, 72),
        "action": (201, 1026, 677, 176), "stats": (59, 1281, 962, 297),
        "history": (46, 1611, 988, 121),
    },
    "LocalWithdrawalConditions": {
        "mainCard": (50, 118, 980, 1187), "titlePlate": (162, 198, 755, 140),
        "row[0]": (120, 633.2, 840, 104), "row[1]": (120, 745.2, 840, 104),
        "row[2]": (120, 857.2, 840, 104), "row[3]": (120, 969.2, 840, 104),
        "action": (112, 1108, 856, 139),
    },
    "TXCoinPanel": {
        "balanceCard": (18, 268, 1045, 108), "amountCard": (99, 422, 882, 241),
        "tierCard": (71, 801, 938, 576), "tier[0]": (126, 900, 368, 208),
        "tier[1]": (586, 900, 368, 208), "tier[2]": (126, 1140, 368, 208),
        "tier[3]": (586, 1140, 368, 208), "conditions": (71, 1396, 938, 277),
        "action": (178, 1706, 724, 145),
    },
}
LABELS = {
    "mainCard": "主卡", "titlePlate": "标题", "amountCard": "金额卡", "progressTrack": "进度",
    "action": "主按钮", "stats": "统计", "history": "历史", "balanceCard": "余额",
    "tierCard": "档位区", "tier[0]": "档位 1", "tier[1]": "档位 2", "tier[2]": "档位 3",
    "tier[3]": "档位 4", "conditions": "条件", "row[0]": "条件 1",
    "row[1]": "条件 2", "row[2]": "条件 3", "row[3]": "条件 4",
}


def font(size, bold=False):
    return ImageFont.truetype("C:/Windows/Fonts/msyhbd.ttc" if bold else "C:/Windows/Fonts/msyh.ttc", size)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def text(draw, value, xy, face, fill="#263646", width=None):
    if width is not None and draw.textbbox((0, 0), value, font=face)[2] > width:
        raise ValueError("Label does not fit: " + value)
    draw.text(xy, value, font=face, fill=fill)


def rect(draw, box, origin, color, label=None, thickness=2):
    x, y = origin
    x1, y1 = x + box["x"] * SCALE, y + box["y"] * SCALE
    x2, y2 = x1 + box["w"] * SCALE, y1 + box["h"] * SCALE
    draw.rectangle((x1, y1, x2, y2), outline=color, width=thickness)
    if label:
        face = font(16, True)
        bounds = draw.textbbox((0, 0), label, font=face)
        tw = bounds[2] - bounds[0]
        draw.rectangle((x1 + 4, y1 + 3, x1 + tw + 12, y1 + 27), fill="#ffffff")
        text(draw, label, (x1 + 7, y1 + 3), face, color)


def load_sources(root):
    rows, sources = [], []
    for page, state, title, keys in PAGES:
        suffix = "--" + state if state else ""
        path = root / ("measurements-" + page + suffix + "-1080x1920.json")
        document = json.loads(path.read_text(encoding="utf-8-sig"))
        if (document["width"], document["height"]) != (1080, 1920):
            raise ValueError("Measurement viewport must be 1080x1920: " + str(path))
        if document["page"] != page or document["state"] != (state or "representative"):
            raise ValueError("Wrong page/state: " + str(path))
        index = {item["key"]: item for item in document["entries"]}
        selected = []
        for key in keys:
            item = index[key]
            if not item["active"] or not item["enabled"]:
                raise ValueError("Representative module is inactive: " + page + " / " + key)
            if len(item["reference"]) != 1:
                raise ValueError("Expected one evidenced reference rectangle: " + key)
            selected.append(item)
        rows.append((page, state, title, selected))
        sources.append({"path": str(path), "sha256": sha256(path), "generatedUtc": document["generatedUtc"]})
    return rows, sources


def render(rows, destination, filename, title):
    total_width = PAD * 2 + WIDTH * 3 + GAP * 2
    row_height = ROW_TITLE + HEIGHT
    total_height = TITLE_HEIGHT + row_height * len(rows) + ROW_GAP * (len(rows) - 1) + FOOTER
    board = Image.new("RGB", (total_width, total_height), "#edf1f5")
    draw = ImageDraw.Draw(board)
    text(draw, title, (PAD, 18), font(36, True))
    text(draw, "同一 1080×1920 坐标系；只看模块位置与尺寸，不用装饰图案掩盖布局差异。", (PAD, 68), font(23))
    headers = ["APK 原图边界估计", "上轮 PNG / 旧布局估计", "本轮 Unity 实测控件框"]
    deltas, references = [], {}
    for row_index, (page, state, row_title, entries) in enumerate(rows):
        top = TITLE_HEIGHT + row_index * (row_height + ROW_GAP)
        for column in range(3):
            left = PAD + column * (WIDTH + GAP)
            text(draw, row_title + " · " + headers[column], (left, top + 4), font(20, True), width=WIDTH)
            origin = (left, top + ROW_TITLE)
            draw.rectangle((left, origin[1], left + WIDTH, origin[1] + HEIGHT), fill="#ffffff", outline="#bec8d1", width=2)
            for grid_y in range(240, 1920, 240):
                yy = origin[1] + grid_y * SCALE
                draw.line((left, yy, left + WIDTH, yy), fill="#e8edf1", width=1)
            for entry in entries:
                reference = entry["reference"][0]
                label = LABELS[entry["key"]]
                if column == 0:
                    rect(draw, reference, origin, BLUE, label)
                elif column == 1:
                    previous = dict(zip(("x", "y", "w", "h"), OLD[page][entry["key"]]))
                    rect(draw, previous, origin, "#7e688b", label)
                else:
                    rect(draw, entry, origin, ORANGE, label)
        previous_source = OLD_ROOT / ("fixture-" + page + ("--" + state if state else "") + "-1080x1920.png")
        with Image.open(previous_source) as original:
            if original.size != (1080, 1920):
                raise ValueError("Previous PNG is not 1080x1920: " + str(previous_source))
        references[str(previous_source)] = {"path": str(previous_source), "sha256": sha256(previous_source), "size": [1080, 1920], "role": "previous synthetic Unity preview; manually estimated boundaries, not current camera capture"}
        for entry in entries:
            reference = entry["reference"][0]
            source = Path(reference["source"])
            if str(source) not in references:
                with Image.open(source) as original:
                    if original.size != (1080, 1920):
                        raise ValueError("Reference PNG is not 1080x1920: " + str(source))
                references[str(source)] = {"path": str(source), "sha256": sha256(source), "size": [1080, 1920]}
            deltas.append({
                "page": page, "key": entry["key"], "label": LABELS[entry["key"]],
                "reference": reference,
                "actual": {axis: entry[axis] for axis in ("x", "y", "w", "h")},
                "actual_minus_reference": {axis: round(entry[axis] - reference[axis], 2) for axis in ("x", "y", "w", "h")},
                "previous_estimate": dict(zip(("x", "y", "w", "h"), OLD[page][entry["key"]])),
                "previous_provenance": "Manual previous-PNG visible boundary estimate, approx +/-5px. Conditions rows and diamond option controls use positions derived from authoring code at git c27e9a5 and checked against that PNG; they are not a previous real-camera measurement.",
                "note": "Different boundary types: visible reference artwork versus actual Unity control rectangle. No pass/fail threshold is applied.",
            })
    foot = total_height - FOOTER + 18
    notes = [
        "参考为 APK 可见边界估计（±3–5px）；上轮为 PNG / 旧布局估计；本轮为相机 RectTransform 实测。",
        "控件框包含透明留边，参考框为可见美术边界；小差值不能直接判定为复刻失败。",
        "条件页只比较 4 个可见任务行，不把 7 行容器的预留空位计为任务内容。Unity 两版均非手机实拍。",
        "同一 1080×1920 坐标等比缩为 540×960；线框仅为几何说明，没有新增到游戏界面。",
    ]
    for i, line in enumerate(notes):
        text(draw, line, (PAD, foot + i * 30), font(21), "#526273", width=total_width - PAD * 2)
    path = destination / filename
    board.save(path)
    return path, deltas, list(references.values())


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input-dir", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    args = parser.parse_args()
    rows, measurements = load_sources(args.input_dir)
    args.output_dir.mkdir(parents=True, exist_ok=True)
    main, deltas, references = render(rows[:2], args.output_dir, "cash-conditions-layout-wireframes.png", "现金钱包与条件页 · 三版布局线框")
    diamond, diamond_deltas, diamond_references = render(rows[2:], args.output_dir, "diamond-layout-wireframes.png", "钻石钱包 · 三版布局线框")
    deltas.extend(diamond_deltas)
    unique_sources = {item["path"]: item for item in references + diamond_references}
    manifest = {
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "scope": "Geometry-only evidence. Current coordinates are actual camera-rendered RectTransform bounds from this measurement run; references are evidenced manual visible-edge estimates. No UI modifications and no expected current coordinate tests.",
        "source_measurements": measurements, "source_pngs": list(unique_sources.values()),
        "outputs": [{"path": str(path), "sha256": sha256(path)} for path in (main, diamond)], "deltas": deltas,
    }
    target = args.output_dir / "layout-wireframe-manifest.json"
    target.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"figures": [str(main), str(diamond)], "manifest": str(target), "measuredModules": len(deltas)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
