"""Import a dependency-closed, static UI subset of the supplied Nut Sort export.

No APK, device snapshots, app-data, server configuration or user saves are read.
Only ReferenceOriginal/Assets plus its already-normalized native UI counterpart
are used. Original local fileIDs/hierarchies are retained for the host's bindings;
asset GUIDs are namespaced to prevent collisions with the fruit game's assets.
Run before authoring host controller bindings, since reimport restores raw pages.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from collections import Counter
from pathlib import Path

SOURCE = Path(r"C:/Projects/Nut Sort Relax/DeviceExports/com.nut.sort.relax_20260907_200801/Recovered/delivery/UnityFramework")
PROJECT = Path(__file__).resolve().parents[1]
PAGES = (
    "TXPanel", "TXCoinPanel", "TXChannelPanel", "TXUserInfoSurePanel",
    "TXProgress0Panel", "TXProgress1Panel", "TXProgress2Panel", "TXProgress3Panel",
    "TXRecordPanel", "RewardGetPanel", "NewPeopleRewardGetPanel",
)
PANEL_DEST = Path("Assets/Resources/WithdrawalImported/Panels")
ART_DEST = Path("Assets/WithdrawalImported/Art")
HEADER = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
REF = re.compile(r"\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}")
SCRIPT = re.compile(r"m_Script: (\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\})")
BLOCK = re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\n.*?(?=^--- !u!|\Z)", re.M | re.S)
TMP_DLL = "67dfb1fdfb2b407222eda8e23ac8b724"
OFFICIAL_TMP = {
    "1453722849": "f4688fdb7df04437aeb418b961361dc5",  # TextMeshProUGUI
    "-667331979": "71c1514a6bd24e1e882cebbe1904ce04",  # TMP_FontAsset
    "-1620774994": "2da0c512f12947e489f739169773d7ca",  # TMP_InputField
}
TEXT_EXTENSIONS = {".prefab", ".asset", ".mat", ".controller", ".anim"}
BINARY_EXTENSIONS = {".png", ".jpg", ".jpeg", ".ttf", ".otf"}


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def guid_for(key: str) -> str:
    return hashlib.md5(("djs-apple/nut-withdrawal/" + key).encode()).hexdigest()


def index_guids(root: Path) -> dict[str, Path]:
    result = {}
    for meta in root.rglob("*.meta"):
        match = re.search(r"^guid: ([0-9a-f]{32})$", read(meta), re.M)
        if match:
            result[match[1]] = meta.with_suffix("")
    return result


def split_blocks(text: str) -> dict[str, tuple[int, str]]:
    return {match[2]: (int(match[1]), match[0]) for match in BLOCK.finditer(text)}


def package_script_map(original: Path, native: Path) -> dict[tuple[str, str], str]:
    mapping = {}
    # The exporter already identified standard UGUI scripts. Match by the
    # original component fileID, never by hierarchy position or a guessed type.
    for path in (original / "Resources/prefabs/panels").glob("*.prefab"):
        normalized = native / path.relative_to(original)
        if not normalized.exists():
            continue
        old, new = split_blocks(read(path)), split_blocks(read(normalized))
        for file_id, (_, body) in old.items():
            before = SCRIPT.search(body)
            after = SCRIPT.search(new.get(file_id, (0, ""))[1])
            if before and after and after[2] == "11500000":
                mapping[(before[3], before[2])] = after[1]
    for file_id, guid in OFFICIAL_TMP.items():
        mapping[(TMP_DLL, file_id)] = f"{{fileID: 11500000, guid: {guid}, type: 3}}"
    return mapping


def clean_persistent_events(body: str) -> str:
    # All user-facing Button handlers are rebound in host C#, never Inspector
    # UnityEvents referencing stripped original controllers.
    return re.sub(r"(^\s+m_PersistentCalls:\n)([ ]+)m_Calls:(?! \[\])\n.*?(?=^\S|^ {0,4}\S|\Z)",
                  lambda m: m[1] + m[2] + "m_Calls: []\n", body, flags=re.M | re.S)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=SOURCE)
    parser.add_argument("--preserve-panels", action="store_true", help="Refresh dependencies/manifest without overwriting host-authored panels.")
    args = parser.parse_args()
    original = args.source / "ReferenceOriginal/Assets"
    native = args.source / "Assets/Recovered"
    index = index_guids(original)
    inverse = {path: guid for guid, path in index.items()}
    scripts = package_script_map(original, native)
    official_guids = {REF.fullmatch(value)[2] for value in scripts.values()}

    target_shaders = {}
    for path in (PROJECT / "Assets").rglob("*.shader"):
        if "WithdrawalImported" in path.parts:
            continue
        name = re.search(r'^\s*Shader\s+"([^"]+)"', read(path), re.M)
        meta = path.with_name(path.name + ".meta")
        if name and meta.exists():
            target_shaders[name[1]] = re.search(r"^guid: (\w+)$", read(meta), re.M)[1]

    external_map = {}
    for guid, path in index.items():
        if path.suffix == ".shader":
            name = re.search(r'^\s*Shader\s+"([^"]+)"', read(path), re.M)
            if name and name[1] in target_shaders:
                external_map[guid] = target_shaders[name[1]]
    official_guids.update(external_map.values())
    destinations: dict[str, Path] = {}
    transformed: dict[str, str | bytes] = {}
    removed = []
    bindings = {}
    notes = []

    def material_supported(guid: str) -> bool:
        material = index.get(guid)
        if not material or material.suffix != ".mat":
            return True
        shader = re.search(r"m_Shader: \{fileID: -?\d+, guid: (\w+)", read(material))
        return not shader or shader[1] in external_map or shader[1].startswith("0000000000000000")

    def clean_yaml(path: Path, text: str) -> str:
        blocks = split_blocks(text)
        kept, dropped = {}, set()
        for file_id, (class_id, body) in blocks.items():
            if class_id == 114:
                script = SCRIPT.search(body)
                replacement = scripts.get((script[3], script[2])) if script else None
                if replacement is None:
                    dropped.add(file_id)
                    removed.append({"asset": str(path.relative_to(original)).replace("\\", "/"),
                                    "fileID": file_id,
                                    "script": str(index.get(script[3], "unknown")).replace("\\", "/") if script else "missing",
                                    "fields": body})
                    continue
                body = body.replace(script[1], replacement, 1)
                body = clean_persistent_events(body)
                if script[3] == TMP_DLL and script[2] == "-1620774994":
                    # Use the same populated source font as this input's child
                    # labels. Its exported LiberationSans dynamic fallback has
                    # no atlas pixels in the source delivery.
                    body = re.sub(r"m_GlobalFontAsset: \{[^}]+\}",
                                  "m_GlobalFontAsset: {fileID: 11400000, guid: 1381a6b2f7565e2439d9f2a2bf704720, type: 2}", body)
                for material in list(re.finditer(r"m_Material: (\{fileID: -?\d+, guid: (\w+), type: \d+\})", body)):
                    if not material_supported(material[2]):
                        body = body.replace(material[0], "m_Material: {fileID: 0}")
                        notes.append(f"{path.stem} component {file_id}: unsupported exported visual shader uses standard UI material; sprite retained.")
                if script[3] == TMP_DLL and script[2] == "-667331979":
                    # These exported atlases already contain glyph pixels. Keep
                    # their exact metrics and freeze them, avoiding unbounded
                    # atlas growth or reliance on missing original font files.
                    body = re.sub(r"^  m_AtlasPopulationMode: .*", "  m_AtlasPopulationMode: 0", body, flags=re.M)
            kept[file_id] = (class_id, body)
        for file_id, (class_id, body) in list(kept.items()):
            for drop in dropped:
                body = re.sub(rf"^  - component: \{{fileID: {drop}\}}\n", "", body, flags=re.M)
                body = body.replace(f"{{fileID: {drop}}}", "{fileID: 0}")
            kept[file_id] = (class_id, body)
        if path.suffix == ".prefab":
            names = {}
            object_to_transform, transform_to_parent = {}, {}
            for file_id, (class_id, body) in kept.items():
                if class_id == 1:
                    names[file_id] = re.search(r"^  m_Name: (.*)", body, re.M)[1]
                elif class_id in (4, 224):
                    go = re.search(r"m_GameObject: \{fileID: (\d+)\}", body)[1]
                    object_to_transform[go] = file_id
                    transform_to_parent[file_id] = re.search(r"m_Father: \{fileID: (\d+)\}", body)[1]
            transform_to_object = {value: key for key, value in object_to_transform.items()}
            def hierarchy(go):
                parent = transform_to_parent[object_to_transform[go]]
                return hierarchy(transform_to_object[parent]) + "/" + names[go] if parent != "0" else names[go]
            entries = []
            for file_id, (class_id, body) in kept.items():
                script = SCRIPT.search(body)
                if script:
                    go = re.search(r"m_GameObject: \{fileID: (\d+)\}", body)[1]
                    entries.append({"fileID": file_id, "gameObject": go, "path": hierarchy(go),
                                    "scriptGUID": script[3],
                                    "text": re.search(r"^  m_text: (.*)", body, re.M)[1] if "  m_text:" in body else None})
            bindings[path.stem] = entries
        return HEADER + "".join(body for _, body in kept.values())

    def enqueue(guid: str) -> None:
        if guid in transformed or guid in external_map or guid in official_guids or guid.startswith("0000000000000000"):
            return
        if guid not in index:
            raise RuntimeError(f"Missing source GUID {guid}")
        path = index[guid]
        if path.suffix not in TEXT_EXTENSIONS | BINARY_EXTENSIONS:
            raise RuntimeError(f"Unsupported dependency {path}")
        relative = path.relative_to(original)
        dest = PANEL_DEST / path.name if path.stem in PAGES and path.suffix == ".prefab" else ART_DEST / relative
        destinations[guid] = dest
        if path.suffix in BINARY_EXTENSIONS:
            transformed[guid] = path.read_bytes()
            return
        text = clean_yaml(path, read(path))
        transformed[guid] = text
        for _, dependency, _ in REF.findall(text):
            enqueue(dependency)

    for page in PAGES:
        enqueue(inverse[original / "Resources/prefabs/panels" / (page + ".prefab")])
    # The original GoldHelp/controller assigns these sprites by resource path
    # at runtime; therefore they are not in the serialized dependency closure.
    for relative in ("Resources/atlas/golds/US1.asset", "Resources/atlas/golds/US2.asset", "Resources/atlas/golds/US3.asset",
                     "Resources/atlas/items/Coin.asset", "Resources/atlas/items/Coin1.asset", "Resources/atlas/items/Coin2.asset",
                     "Resources/atlas/pay/paypal.asset", "Resources/atlas/pay/Other.asset", "Resources/atlas/paysimple/PayPal.asset"):
        enqueue(inverse[original / relative])
    # Preserve visual options referenced only by original controller fields
    # (channel logos/reward variants), without importing those controllers.
    for entry in removed:
        if "/UI/Panel/" not in entry["script"]:
            continue
        for _, guid, _ in REF.findall(entry["fields"]):
            path = index.get(guid)
            if path and material_supported(guid) and (path.suffix in BINARY_EXTENSIONS or path.parent.name in ("Sprite", "Material")):
                enqueue(guid)

    new_guids = {guid: guid_for(str(path).replace("\\", "/")) for guid, path in destinations.items()}
    remaps = external_map | new_guids
    def remap(text):
        return re.sub(r"guid: ([0-9a-f]{32})", lambda m: "guid: " + remaps.get(m[1], m[1]), text)
    written = []
    for guid, data in transformed.items():
        dest = PROJECT / destinations[guid]
        dest.parent.mkdir(parents=True, exist_ok=True)
        preserve = args.preserve_panels and dest.suffix == ".prefab" and dest.exists()
        if preserve:
            pass
        elif isinstance(data, bytes):
            dest.write_bytes(data)
        else:
            dest.write_text(remap(data), encoding="utf-8", newline="\n")
        meta = remap(read(index[guid].with_name(index[guid].name + ".meta")))
        meta = re.sub(r"^(\s*assetBundleName:).*", r"\1", meta, flags=re.M)
        meta = re.sub(r"^(\s*assetBundleVariant:).*", r"\1", meta, flags=re.M)
        dest.with_name(dest.name + ".meta").write_text(meta, encoding="utf-8", newline="\n")
        written.append({"source": str(index[guid].relative_to(original)).replace("\\", "/"),
                        "destination": str(destinations[guid]).replace("\\", "/"),
                        "sourceGUID": guid, "guid": new_guids[guid],
                        "sourceSHA256": hashlib.sha256(index[guid].read_bytes()).hexdigest(),
                        "bytes": dest.stat().st_size})
        parent = dest.parent
        while parent != PROJECT / "Assets":
            parent_meta = parent.with_name(parent.name + ".meta")
            if not parent_meta.exists():
                parent_meta.write_text("fileFormatVersion: 2\nguid: " + guid_for(str(parent.relative_to(PROJECT)).replace("\\", "/")) + "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n", encoding="utf-8")
            parent = parent.parent
    report = {"source": str(original).replace("\\", "/"), "pages": list(PAGES),
              "assets": written, "removedComponents": removed,
              "shaderRemaps": external_map,
              "notes": ["Exact original hierarchy, local fileIDs, static text, graphics and layout retained.",
                        "UGUI/TMP script references mapped to installed official package scripts.",
                        "Original custom controllers and tween components stripped; no original business implementation imported.",
                        "Populated source font atlases preserved; imported TMP fonts use static atlas mode.",
                        "Only bundled UI assets read. No device snapshots, application data, identifiers or user saves read."] + notes}
    output = PROJECT / "Tools/NutWithdrawalImportManifest.json"
    output.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    (PROJECT / "Tools/NutWithdrawalBindings.json").write_text(json.dumps(bindings, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"pages": len(PAGES), "assets": len(written), "bytes": sum(a["bytes"] for a in written),
                      "removedComponents": len(removed), "types": dict(Counter(Path(a["destination"]).suffix for a in written)),
                      "manifest": str(output)}, indent=2))


if __name__ == "__main__":
    main()
