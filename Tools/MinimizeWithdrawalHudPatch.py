"""Keep only the WithdrawalHud installation over the pre-install prefab baseline.

Unity's prefab authoring can reorder/reserialize every original YAML document.
This bounded tool restores the exact original documents and adds only the current
HUD PrefabInstance, its stripped root RectTransform and one root child reference.
Run after authoring is finished, before unrelated edits to these two prefabs.
It does not invoke Unity, change Git state, or regenerate any HUD/UI assets.
"""
from __future__ import annotations

import argparse
from dataclasses import dataclass
from pathlib import Path
import re
import subprocess


REPOSITORY = Path(__file__).resolve().parents[1]
PREFABS = (
    "output_Unity/Assets/res/local/coreplay/CorePlayUI.prefab",
)
HUD = "output_Unity/Assets/res/local/withdrawal/WithdrawalHud.prefab"
HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)( stripped)?[ \t]*\r?$", re.M)
CHILDREN = re.compile(r"^  m_Children:(?: \[\])?\n(?:  - \{fileID: -?\d+\}\n)*", re.M)


@dataclass(frozen=True)
class Document:
    kind: int
    file_id: str
    stripped: bool
    start: int
    end: int
    text: str


def documents(text: str) -> list[Document]:
    matches = list(HEADER.finditer(text))
    result = []
    seen = set()
    for index, match in enumerate(matches):
        end = matches[index + 1].start() if index + 1 < len(matches) else len(text)
        file_id = match[2]
        if file_id in seen:
            raise ValueError(f"Duplicate YAML document fileID {file_id}")
        seen.add(file_id)
        result.append(Document(int(match[1]), file_id, bool(match[3]), match.start(), end, text[match.start():end]))
    if not result:
        raise ValueError("No Unity YAML documents found")
    return result


def unique(items: list[Document], description: str) -> Document:
    if len(items) != 1:
        raise ValueError(f"Expected exactly one {description}, found {len(items)}")
    return items[0]


def root_rect(items: list[Document]) -> Document:
    return unique([doc for doc in items if doc.kind == 224 and not doc.stripped
                   and re.search(r"^  m_Father: \{fileID: 0\}$", doc.text, re.M)], "root RectTransform")


def git_baseline(base: str, relative: str) -> str:
    result = subprocess.run(["git", "show", f"{base}:{relative}"], cwd=REPOSITORY,
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True)
    text = result.stdout.decode("utf-8")
    if "\r" in text:
        raise ValueError("Expected the repository baseline to use LF; refusing an implicit newline rewrite")
    return text


def prepare(base: str, relative: str, hud_guid: str, hud_root_id: str) -> tuple[Path, bytes, bytes, int]:
    path = REPOSITORY / relative
    original_bytes = path.read_bytes()
    current = original_bytes.decode("utf-8").replace("\r\n", "\n")
    baseline = git_baseline(base, relative)
    current_docs, base_docs = documents(current), documents(baseline)
    root = root_rect(base_docs)
    source_pattern = re.compile(r"^  m_SourcePrefab: \{fileID: -?\d+, guid: " + re.escape(hud_guid) + r", type: 3\}$", re.M)
    instance = unique([doc for doc in current_docs if doc.kind == 1001 and source_pattern.search(doc.text)], "WithdrawalHud instance")
    stripped = unique([doc for doc in current_docs if doc.kind == 224 and doc.stripped
                       and f"  m_PrefabInstance: {{fileID: {instance.file_id}}}\n" in doc.text
                       and f"  m_CorrespondingSourceObject: {{fileID: {hud_root_id}, guid: {hud_guid}, type: 3}}\n" in doc.text],
                      "WithdrawalHud stripped root RectTransform")
    if instance.file_id in {doc.file_id for doc in base_docs} or stripped.file_id in {doc.file_id for doc in base_docs}:
        raise ValueError("HUD fileIDs already exist in the baseline; select the pre-install base revision")
    for key in ("m_RemovedComponents", "m_RemovedGameObjects", "m_AddedGameObjects", "m_AddedComponents"):
        if f"    {key}: []\n" not in instance.text:
            raise ValueError(f"HUD has nontrivial {key}; this two-document minimizer cannot preserve it safely")

    children = CHILDREN.search(root.text)
    if children is None:
        raise ValueError("Root child list has an unsupported shape")
    child_line = f"  - {{fileID: {stripped.file_id}}}\n"
    original_children = children[0]
    if child_line in original_children:
        raise ValueError("HUD child reference already exists in the baseline")
    updated_children = original_children.replace("  m_Children: []\n", "  m_Children:\n") + child_line
    updated_root = root.text[:children.start()] + updated_children + root.text[children.end():]
    instance_text, parent_count = re.subn(r"^    m_TransformParent: \{fileID: -?\d+\}$",
                                        f"    m_TransformParent: {{fileID: {root.file_id}}}", instance.text, flags=re.M)
    if parent_count != 1:
        raise ValueError("HUD instance is missing a unique TransformParent")
    result = baseline[:root.start] + updated_root + baseline[root.end:]
    if not result.endswith("\n"):
        raise ValueError("Baseline is missing its final newline")
    result += instance_text.rstrip("\n") + "\n" + stripped.text.rstrip("\n") + "\n"

    # Every original component, value and ordering is preserved, except one added root child.
    result_docs = documents(result)
    if len(result_docs) != len(base_docs) + 2:
        raise ValueError("Unexpected document count after minimization")
    for before, after in zip(base_docs, result_docs):
        expected = updated_root if before.file_id == root.file_id else before.text
        if before.file_id != after.file_id or after.text != expected:
            raise ValueError(f"Original YAML document changed unexpectedly: {before.file_id}")
    restored_root = updated_root.replace(updated_children, original_children, 1)
    if restored_root != root.text:
        raise ValueError("Original root values changed beyond the HUD child list")
    return path, original_bytes, result.encode("utf-8"), len(base_docs)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base", default="9ab6eb0", help="Pre-HUD Git revision; default: 9ab6eb0")
    parser.add_argument("--check", action="store_true", help="Validate only; return 1 if either prefab still needs minimization")
    args = parser.parse_args()
    hud_meta = (REPOSITORY / (HUD + ".meta")).read_text(encoding="utf-8")
    guid_match = re.search(r"^guid: ([0-9a-f]{32})$", hud_meta, re.M)
    if guid_match is None:
        raise ValueError("WithdrawalHud prefab GUID is missing")
    hud_root_id = root_rect(documents((REPOSITORY / HUD).read_text(encoding="utf-8"))).file_id
    plans = [prepare(args.base, relative, guid_match[1], hud_root_id) for relative in PREFABS]
    needs_changes = False
    for path, original, minimized, original_count in plans:
        if path.read_bytes() != original:
            raise RuntimeError("A prefab changed during analysis; wait for Unity authoring to finish and retry")
        changed = original != minimized
        needs_changes |= changed
        if changed and not args.check:
            path.write_bytes(minimized)
        status = "needs minimization" if changed and args.check else "minimized" if changed else "already minimal"
        print(f"{path.relative_to(REPOSITORY).as_posix()}: {status}; preserved {original_count} original documents, added one HUD instance")
    return int(args.check and needs_changes)


if __name__ == "__main__":
    raise SystemExit(main())
