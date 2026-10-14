"""Lay out paired real runtime screenshots without changing their content.

Example (PowerShell)::

    python Tools/BuildApkComparison.py --reference-dir C:/captures/apk `
        --game-dir C:/captures/game --pairs C:/captures/pairs.json `
        --output-dir E:/captures/comparison

The JSON file is an object with a nonempty ``pairs`` array. Each entry has
``id``, ``title``, ``reference`` and ``game``; ``note`` is optional. Image paths
are relative to their corresponding input directory. Example entry::

    {"id": "cash-wallet", "title": "现金钱包 / Cash wallet",
     "reference": "04-cash-wallet.png", "game": "cash-wallet.png"}

Optional top-level ``left_label``, ``right_label`` and ``footer`` strings
customize the comparison captions. Omit them to retain the original APK/game
report labels. For an art revision, use e.g. ``left_label: 改前 / Before`` and
``right_label: 苹果树风格 / Orchard style``; point ``--reference-dir`` at the
before captures and ``--game-dir`` at the after captures. An empty footer hides
the footer text. Labels do not alter capture pixels or source provenance.

Only supplied captures are used. Images are fitted, with their original aspect
ratio, inside a common 1080 x 1920 frame. No content is cropped or retouched.
The manifest records native sizes, the fitted rectangles and source hashes.
"""
from __future__ import annotations

import argparse
from dataclasses import dataclass
import hashlib
import json
from pathlib import Path
import re
import sys

from PIL import Image, ImageDraw, ImageFont, ImageOps


FRAME = (1080, 1920)
MARGIN = 32
GAP = 32
TITLE_HEIGHT = 116
COLUMN_HEIGHT = 62
CAPTION_HEIGHT = 74
FOOTER_HEIGHT = 90
PAGE_WIDTH = 2 * FRAME[0] + 2 * MARGIN + GAP
PAGE_HEIGHT = TITLE_HEIGHT + COLUMN_HEIGHT + FRAME[1] + CAPTION_HEIGHT + FOOTER_HEIGHT
BACKGROUND = "#edf1f5"
LETTERBOX = "#d5dce5"
INK = "#243345"
MUTED = "#596a7d"


@dataclass(frozen=True)
class Pair:
    identifier: str
    title: str
    reference: Path
    game: Path
    note: str


@dataclass(frozen=True)
class Labels:
    left_label: str = "原 APK / Original APK"
    right_label: str = "游戏实际运行 / Game runtime"
    footer: str = "完整截图等比排版，保留原始画面 / Full captures; aspect ratio preserved"


def load_labels(data: dict) -> Labels:
    defaults = Labels()
    values: dict[str, str] = {}
    for key in ("left_label", "right_label", "footer"):
        value = data.get(key, getattr(defaults, key))
        if not isinstance(value, str) or (key != "footer" and not value.strip()):
            raise ValueError(f"'{key}' must be a string; left and right labels cannot be empty.")
        values[key] = value
    return Labels(**values)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--reference-dir", type=Path, required=True, help="Left-side captures: original APK or before revision")
    parser.add_argument("--game-dir", type=Path, required=True, help="Right-side captures: game runtime or after revision")
    parser.add_argument("--pairs", type=Path, required=True, help="JSON containing the actual capture pairings")
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--font", type=Path, help="A CJK-capable TrueType/OpenType font; defaults to Microsoft YaHei")
    parser.add_argument("--overview-columns", type=int, default=2, choices=range(1, 5))
    parser.add_argument("--overview-cell-width", type=int, default=900)
    parser.add_argument("--validate-only", action="store_true", help="Validate pairings/images without creating output")
    return parser.parse_args()


def font_path(requested: Path | None) -> Path:
    candidates = [requested] if requested else [
        Path("C:/Windows/Fonts/msyh.ttc"),
        Path("C:/Windows/Fonts/simhei.ttf"),
        Path("/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc"),
    ]
    for candidate in candidates:
        if candidate and candidate.is_file():
            return candidate
    raise ValueError("A CJK font is required for the Chinese labels. Supply --font.")


def input_path(directory: Path, relative: str) -> Path:
    if not isinstance(relative, str) or not relative.strip():
        raise ValueError("Every image path must be a nonempty relative filename.")
    root = directory.resolve(strict=True)
    raw = Path(relative)
    if raw.is_absolute():
        raise ValueError(f"Image path must be relative to its input directory: {relative}")
    result = (root / raw).resolve(strict=True)
    if not result.is_relative_to(root) or not result.is_file():
        raise ValueError(f"Image is outside its input directory or not a file: {relative}")
    return result


def load_pairs(args: argparse.Namespace) -> tuple[str, list[Pair], Labels]:
    data = json.loads(args.pairs.read_text(encoding="utf-8-sig"))
    if not isinstance(data, dict) or not isinstance(data.get("pairs"), list) or not data["pairs"]:
        raise ValueError("The JSON must contain a nonempty 'pairs' array; no images are fabricated for missing pairings.")
    title = str(data.get("title", "APK 与游戏实际运行对比 / Actual runtime comparison"))
    labels = load_labels(data)
    pairs: list[Pair] = []
    identifiers: set[str] = set()
    for index, entry in enumerate(data["pairs"], 1):
        if not isinstance(entry, dict):
            raise ValueError(f"Pair {index} must be an object.")
        identifier = entry.get("id", "")
        if not isinstance(identifier, str) or not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}", identifier):
            raise ValueError(f"Pair {index} needs an 'id' containing letters, digits, hyphens or underscores.")
        if identifier.casefold() in identifiers:
            raise ValueError(f"Duplicate pair id: {identifier}")
        if identifier.casefold() == "overview":
            raise ValueError("Pair id 'overview' is reserved for the overview output.")
        identifiers.add(identifier.casefold())
        pair_title = entry.get("title", "")
        if not isinstance(pair_title, str) or not pair_title.strip():
            raise ValueError(f"Pair {identifier} needs a descriptive title identifying the observed screen.")
        pairs.append(Pair(identifier, pair_title,
                          input_path(args.reference_dir, entry.get("reference")),
                          input_path(args.game_dir, entry.get("game")),
                          str(entry.get("note", ""))))
    return title, pairs, labels


def inspect_image(path: Path) -> dict:
    with Image.open(path) as source:
        source.verify()
    with Image.open(path) as source:
        width, height = source.size
        if width <= 0 or height <= 0:
            raise ValueError(f"Empty image: {path}")
        oriented = ImageOps.exif_transpose(source)
        display_size = list(oriented.size)
    with path.open("rb") as source_file:
        digest = hashlib.file_digest(source_file, "sha256").hexdigest()
    return {"path": str(path), "native_size": [width, height],
            "orientation_corrected_size": display_size, "sha256": digest}


def centered_text(draw: ImageDraw.ImageDraw, text: str, rectangle: tuple[int, int, int, int],
                  fonts: Path, size: int, color: str = INK, minimum: int = 18) -> None:
    left, top, right, bottom = rectangle
    font = ImageFont.truetype(str(fonts), size)
    while draw.textbbox((0, 0), text, font=font)[2] > right - left and size > minimum:
        size -= 1
        font = ImageFont.truetype(str(fonts), size)
    box = draw.textbbox((0, 0), text, font=font)
    if box[2] - box[0] > right - left:
        raise ValueError(f"Caption too long to fit without losing content: {text}")
    draw.text((left + (right - left - box[2] + box[0]) / 2 - box[0],
               top + (bottom - top - box[3] + box[1]) / 2 - box[1]),
              text, font=font, fill=color)


def paste_capture(page: Image.Image, path: Path, left: int, top: int) -> list[int]:
    with Image.open(path) as source:
        oriented = ImageOps.exif_transpose(source)
        scale = min(FRAME[0] / oriented.width, FRAME[1] / oriented.height)
        size = (max(1, round(oriented.width * scale)), max(1, round(oriented.height * scale)))
        fitted = oriented.convert("RGBA").resize(size, Image.Resampling.LANCZOS)
        x = left + (FRAME[0] - size[0]) // 2
        y = top + (FRAME[1] - size[1]) // 2
        page.paste(fitted, (x, y), fitted)
        return [x, y, size[0], size[1]]


def build_pair(pair: Pair, metadata: dict, target: Path, fonts: Path, labels: Labels) -> None:
    page = Image.new("RGB", (PAGE_WIDTH, PAGE_HEIGHT), BACKGROUND)
    draw = ImageDraw.Draw(page)
    centered_text(draw, pair.title, (MARGIN, 0, PAGE_WIDTH - MARGIN, TITLE_HEIGHT), fonts, 48)
    top = TITLE_HEIGHT + COLUMN_HEIGHT
    for column, label, path, key in (
        (0, labels.left_label, pair.reference, "reference"),
        (1, labels.right_label, pair.game, "game"),
    ):
        left = MARGIN + column * (FRAME[0] + GAP)
        centered_text(draw, label, (left, TITLE_HEIGHT, left + FRAME[0], top), fonts, 32)
        draw.rectangle((left, top, left + FRAME[0] - 1, top + FRAME[1] - 1), fill=LETTERBOX)
        metadata[key]["fitted_rectangle"] = paste_capture(page, path, left, top)
        native = metadata[key]["native_size"]
        caption = f"{path.name}  ·  {native[0]} × {native[1]}"
        centered_text(draw, caption, (left, top + FRAME[1], left + FRAME[0], top + FRAME[1] + CAPTION_HEIGHT),
                      fonts, 25, MUTED)
    foot_top = top + FRAME[1] + CAPTION_HEIGHT
    if pair.note:
        centered_text(draw, pair.note, (MARGIN, foot_top, PAGE_WIDTH - MARGIN, foot_top + 44), fonts, 27, MUTED)
    if labels.footer:
        centered_text(draw, labels.footer, (MARGIN, foot_top + 44, PAGE_WIDTH - MARGIN, PAGE_HEIGHT), fonts, 22, MUTED)
    page.save(target, optimize=True)


def build_overview(title: str, images: list[Path], target: Path, fonts: Path,
                   columns: int, cell_width: int) -> None:
    if not 300 <= cell_width <= 1800:
        raise ValueError("--overview-cell-width must be between 300 and 1800.")
    columns = min(columns, len(images))
    cell_height = round(PAGE_HEIGHT * cell_width / PAGE_WIDTH)
    gap, padding, heading = 24, 24, 100
    rows = (len(images) + columns - 1) // columns
    size = (columns * cell_width + (columns - 1) * gap + padding * 2,
            rows * cell_height + (rows - 1) * gap + padding * 2 + heading)
    overview = Image.new("RGB", size, "#dce3eb")
    centered_text(ImageDraw.Draw(overview), title, (padding, 0, size[0] - padding, heading), fonts, 38)
    for index, path in enumerate(images):
        with Image.open(path) as pair_image:
            thumb = pair_image.resize((cell_width, cell_height), Image.Resampling.LANCZOS)
            overview.paste(thumb, (padding + (index % columns) * (cell_width + gap),
                                  heading + padding + (index // columns) * (cell_height + gap)))
    overview.save(target, optimize=True)


def main() -> int:
    # Windows shells may otherwise select cp1252 for piped output.
    for stream in (sys.stdout, sys.stderr):
        if hasattr(stream, "reconfigure"):
            stream.reconfigure(encoding="utf-8")
    args = parse_args()
    try:
        title, pairs, labels = load_pairs(args)
        fonts = font_path(args.font)
        if not 300 <= args.overview_cell_width <= 1800:
            raise ValueError("--overview-cell-width must be between 300 and 1800.")
        manifest = {"title": title, "left_label": labels.left_label, "right_label": labels.right_label,
                    "footer": labels.footer, "frame_size": list(FRAME), "pair_image_size": [PAGE_WIDTH, PAGE_HEIGHT],
                    "processing": "EXIF orientation, aspect-preserving resize, letterbox; no crop or retouch",
                    "pairs": [{"id": pair.identifier, "title": pair.title, "note": pair.note,
                               "reference": inspect_image(pair.reference), "game": inspect_image(pair.game)}
                              for pair in pairs]}
        if args.validate_only:
            print(json.dumps({"valid_pairs": len(pairs), "output_written": False}, ensure_ascii=False))
            return 0
        output = args.output_dir.resolve()
        destinations = [output / f"{pair.identifier}.png" for pair in pairs]
        destinations += [output / "overview.png", output / "manifest.json"]
        source_paths = {path for pair in pairs for path in (pair.reference, pair.game)}
        if any(path in source_paths or path == args.pairs.resolve() for path in destinations):
            raise ValueError("Output must not overwrite a source capture or pairing configuration.")
        output.mkdir(parents=True, exist_ok=True)
        for pair, metadata, target in zip(pairs, manifest["pairs"], destinations):
            build_pair(pair, metadata, target, fonts, labels)
            metadata["output"] = target.name
        build_overview(title, destinations[:len(pairs)], output / "overview.png", fonts,
                       args.overview_columns, args.overview_cell_width)
        (output / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(json.dumps({"pairs": len(pairs), "output_dir": str(output), "overview": str(output / "overview.png")},
                         ensure_ascii=False))
        return 0
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Comparison generation failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
