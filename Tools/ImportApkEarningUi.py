"""Import the audited Color Nut Party earning artwork and prefab geometry only.

No source code, SDK, project settings, gameplay scene, or source save is copied.
Source custom components are recorded for adapter authoring, then removed. Run
while Unity is stopped; the saved result is ordinary prefab-first Unity assets.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import uuid
from pathlib import Path

PANELS = (
    "LocalMainPanelTop", "TXPanel", "TXCoinPanel", "TXUserInfoPanel",
    "TXUserInfoSurePanel", "TXChannelPanel", "LocalWithdrawalConditions",
    "TXProgress0Panel", "TXTooFastHintPanel", "TXLevelPanel", "TXGuideLoadingPanel",
    "LocalWithdrawalVerification", "TXGuideTargetCompletePanel", "TXRecordGuidePanel",
    "LocalEarningSequence", "LocalCashHistory", "LocalCashHistoryEntry",
    "LocalCashHistoryRow", "LocalLuckyReward", "LocalSuccess", "RewardPanel",
    "LocalUserLevel", "NewbieGuidePanel",
)
EXTRA_ASSETS = (
    "Resources/prefabs/items/Item_Big.prefab",
    "Resources/prefabs/items/LocalLuckyItem.prefab",
    "Resources/prefabs/playerinfo/PlayerInfo.prefab",
    "Resources/prefabs/Tip.prefab",
    "Resources/atlas/localearningmessages/cash-banner.png",
    "Resources/atlas/localearningmessages/portrait-banner.png",
    "Resources/atlas/octobergarden/amount-selected.png",
    "Resources/atlas/octobergarden/amount-normal.png",
    "Resources/atlas/Golds/US3.asset",
    "Resources/atlas/Golds/US1.asset",
    "Resources/atlas/octobergarden/main-milestone-normal.png",
    "Resources/atlas/octobergarden/main-milestone-active.png",
    "Resources/atlas/newbieguidepanel/dian.asset",
    "Resources/atlas/newbieguidepanel/dian1.asset",
    "Resources/atlas/paysimple/Venmo.asset",
    "Resources/atlas/paysimple/Zelle.asset",
    "Resources/atlas/paysimple/PayPal.asset",
    "Resources/atlas/pay/Venmo.asset",
    "Resources/atlas/pay/Zelle.asset",
    "Resources/atlas/pay/paypal.asset",
    "Resources/atlas/pay/Other.asset",
    "Resources/atlas/items/Coin2.asset",
    "Resources/atlas/rewardactivation/reward1.png",
    "Resources/atlas/rewardactivation/reward2.png",
    "Resources/atlas/rewardactivation/reward3.png",
    "Resources/atlas/rewardactivation/reward4.png",
    "Resources/atlas/rewardactivation/reward5.png",
)
YAML_SUFFIXES = {".prefab", ".asset", ".mat", ".controller", ".overridecontroller", ".anim"}
ALLOWED_SUFFIXES = YAML_SUFFIXES | {".png", ".jpg", ".jpeg", ".tga", ".ttf", ".otf", ".shader", ".cginc", ".hlsl"}
GUID = re.compile(r"guid: ([a-f0-9]{32})")
REFERENCE = re.compile(r"\{fileID: (-?\d+)(?:, guid: ([a-f0-9]{32}), type: \d+)?\}")
DOCUMENT = re.compile(r"(?m)^--- !u!(\d+) &(-?\d+)([^\n]*)\n")
BUILTIN_GUIDS = {"0000000000000000e000000000000000", "0000000000000000f000000000000000", "00000000000000000000000000000000"}
IMPORT_NAMESPACE = uuid.UUID("37a5a75b-4054-4ce6-b5b3-8936148c0555")
APK_SHA256 = "c0c4e5de0dacdf0f8e7376c8a270bf2bd11ab6d06b6989994f0858a73266b238"


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def meta_guid(path: Path) -> str:
    match = re.search(r"(?m)^guid: ([a-f0-9]{32})$", read(path))
    if not match:
        raise ValueError(f"Missing GUID in {path}")
    return match[1]


def index_guids(root: Path, exclude: Path | None = None) -> dict[str, Path]:
    result = {}
    for meta in root.rglob("*.meta"):
        if exclude is not None and meta.is_relative_to(exclude):
            continue
        try:
            guid = meta_guid(meta)
        except (UnicodeError, ValueError):
            continue
        asset = meta.with_suffix("")
        if guid in result and result[guid] != asset:
            raise ValueError(f"Duplicate source GUID {guid}: {asset}, {result[guid]}")
        result[guid] = asset
    return result


def documents(text: str) -> tuple[str, list[dict]]:
    matches = list(DOCUMENT.finditer(text))
    return text[:matches[0].start()] if matches else text, [
        {"classId": int(m[1]), "fileId": int(m[2]), "stripped": "stripped" in m[3],
         "text": text[m.start():matches[i + 1].start() if i + 1 < len(matches) else len(text)]}
        for i, m in enumerate(matches)
    ]


def field_reference(text: str, field: str):
    match = re.search(r"(?m)^  " + re.escape(field) + r": (\{fileID: [^\n]+\})", text)
    return REFERENCE.fullmatch(match[1]) if match else None


def serialized_array(text: str, name: str, keep) -> str:
    """Filter complete entries of a PrefabInstance serialization array."""
    pattern = re.compile(r"(?ms)^(    " + re.escape(name) + r":)\n(.*?)(?=^    [a-zA-Z_]|^  [a-zA-Z_]|\Z)")
    def replace(match):
        entries = re.split(r"(?m)(?=^    - )", match[2])
        entries = [entry for entry in entries if entry.strip() and keep(entry)]
        return match[1] + ("\n" + "".join(entries) if entries else " []\n")
    return pattern.sub(replace, text)


def clear_persistent_calls(text: str) -> str:
    lines = text.splitlines(keepends=True)
    out, i = [], 0
    while i < len(lines):
        line = lines[i]
        if re.match(r"^\s*m_Calls:\s*$", line):
            indent = len(line) - len(line.lstrip())
            out.append(" " * indent + "m_Calls: []\n")
            i += 1
            while i < len(lines):
                next_indent = len(lines[i]) - len(lines[i].lstrip())
                if lines[i].strip() and (next_indent < indent or (next_indent == indent and not lines[i].lstrip().startswith("- "))):
                    break
                i += 1
            continue
        out.append(line)
        i += 1
    return "".join(out)


class Importer:
    def __init__(self, source: Path, target: Path, apk: Path, unity: Path):
        self.source = source.resolve()
        self.source_assets = self.source / "Assets"
        self.target = target.resolve()
        self.output = self.target / "Assets/ApkEarningReference"
        self.apk = apk.resolve()
        self.unity = unity.resolve()
        if self.source == self.target or self.output == self.source or self.source.is_relative_to(self.output):
            raise ValueError("Source and bounded target must be separate")
        self.guids = index_guids(self.source_assets)
        self.target_guids = index_guids(self.target / "Assets", self.output)
        self.package_scripts = {}
        for prefix in ("com.unity.ugui@", "com.unity.textmeshpro@"):
            for package in (self.source / "Library/PackageCache").glob(prefix + "*"):
                for guid, asset in index_guids(package).items():
                    if asset.suffix == ".cs" and "/Editor/" not in asset.as_posix() and "/Tests/" not in asset.as_posix():
                        self.package_scripts[guid] = asset
        self.cache, self.removed, self.assets, self.records = {}, {}, {}, []
        self.package_refs, self.builtin_refs, self.missing, self.shader_includes = {}, set(), [], []
        self.overrides = []

    def source_relative(self, path: Path) -> str:
        return path.relative_to(self.source_assets).as_posix()

    def target_path(self, path: Path) -> Path:
        relative = path.relative_to(self.source_assets)
        if path.suffix == ".prefab":
            return self.output / "Resources/ApkEarningReference/Panels" / path.name
        if relative.parts[0] == "Resources":
            relative = Path(*relative.parts[1:])
        return self.output / "Visuals" / relative

    def get_documents(self, path: Path):
        if path not in self.cache:
            self.cache[path] = documents(read(path))
        return self.cache[path]

    def removed_ids(self, path: Path) -> set[int]:
        if path in self.removed:
            return self.removed[path]
        removed = self.removed[path] = set()
        for doc in self.get_documents(path)[1]:
            if doc["classId"] != 114:
                continue
            script = field_reference(doc["text"], "m_Script")
            if script and script[2] not in self.package_scripts:
                if script[2] not in self.guids or self.guids[script[2]].suffix != ".cs":
                    self.fail(f"Unresolved component script: {script[2]} in {path}")
                removed.add(doc["fileId"])
            elif doc["stripped"]:
                origin = field_reference(doc["text"], "m_CorrespondingSourceObject")
                if origin and self.is_removed(origin, path):
                    removed.add(doc["fileId"])
        return removed

    def is_removed(self, ref, context: Path) -> bool:
        if ref is None:
            return False
        file_id, guid = int(ref[1]), ref[2]
        if guid:
            source = self.guids.get(guid)
            return source is not None and source.suffix == ".prefab" and file_id in self.removed_ids(source)
        return file_id in self.removed_ids(context)

    def fail(self, message: str):
        self.missing.append(message)
        raise ValueError(message)

    def object_info(self, path: Path, file_id: int, visiting=None) -> dict:
        visiting = set() if visiting is None else visiting
        key = (path, file_id)
        if key in visiting or file_id == 0:
            return {"fileId": file_id}
        visiting.add(key)
        by_id = {d["fileId"]: d for d in self.get_documents(path)[1]}
        doc = by_id.get(file_id)
        if doc is None:
            return {"fileId": file_id, "path": None}
        info = {"fileId": file_id, "classId": doc["classId"]}
        origin = field_reference(doc["text"], "m_CorrespondingSourceObject")
        if doc["stripped"] and origin and origin[2] in self.guids:
            base = self.object_info(self.guids[origin[2]], int(origin[1]), visiting)
            info.update({"path": base.get("path"), "sourcePrefab": self.source_relative(self.guids[origin[2]]), "sourceFileId": int(origin[1])})
            return info
        if doc["classId"] == 1:
            name = re.search(r"(?m)^  m_Name: (.*)$", doc["text"])
            name = name[1] if name else "?"
            transform = next((d for d in by_id.values() if d["classId"] in (4, 224) and
                              field_reference(d["text"], "m_GameObject") is not None and
                              int(field_reference(d["text"], "m_GameObject")[1]) == file_id), None)
            parent = field_reference(transform["text"], "m_Father") if transform else None
            parent_info = self.object_info(path, int(parent[1]), visiting) if parent and int(parent[1]) else {}
            info["path"] = "/".join(p for p in (parent_info.get("path"), name) if p)
            info["name"] = name
            if transform:
                info["transformFileId"] = transform["fileId"]
                parent_doc = by_id.get(int(parent[1])) if parent and int(parent[1]) else None
                child_list = re.search(r"(?ms)^  m_Children:\n(.*?)(?=^  \w)", parent_doc["text"]) if parent_doc else None
                child_ids = [int(ref[1]) for ref in REFERENCE.finditer(child_list[1])] if child_list else []
                info["siblingIndex"] = child_ids.index(transform["fileId"]) if transform["fileId"] in child_ids else 0
        else:
            owner = field_reference(doc["text"], "m_GameObject")
            if owner:
                info["path"] = self.object_info(path, int(owner[1]), visiting).get("path")
                info["gameObjectFileId"] = int(owner[1])
            script = field_reference(doc["text"], "m_Script")
            if script:
                asset = self.guids.get(script[2], self.package_scripts.get(script[2]))
                info["componentType"] = asset.stem if asset else script[2]
        return info

    def prefab_record(self, path: Path) -> dict:
        docs = self.get_documents(path)[1]
        components = []
        for doc in docs:
            if doc["fileId"] not in self.removed_ids(path):
                continue
            script = field_reference(doc["text"], "m_Script")
            script_path = self.guids.get(script[2]) if script else None
            bindings = []
            array_field, array_index = None, -1
            for line in doc["text"].splitlines():
                # Arrays and nested controller fields are kept verbatim as well.
                top_field = re.match(r"^  ([A-Za-z_][A-Za-z_0-9]*):", line)
                if top_field:
                    array_field, array_index = top_field[1], -1
                if line.startswith("  - "):
                    array_index += 1
                for ref in REFERENCE.finditer(line):
                    prefix = line[:ref.start()].strip().rstrip(": ")
                    if prefix.startswith("m_") or prefix in ("m_Script", "m_GameObject"):
                        continue
                    if array_index >= 0 and array_field:
                        member = prefix.removeprefix("- ").removeprefix("-")
                        prefix = f"{array_field}[{array_index}]" + ("." + member if member else "")
                    value = {"field": prefix, "fileId": int(ref[1]), "guid": ref[2]}
                    if ref[2] in self.guids:
                        value["sourceAsset"] = self.source_relative(self.guids[ref[2]])
                    elif not ref[2] and int(ref[1]):
                        value.update(self.object_info(path, int(ref[1])))
                    bindings.append(value)
            components.append({**self.object_info(path, doc["fileId"]),
                               "script": self.source_relative(script_path) if script_path else None,
                               "bindings": bindings, "serializedComponent": doc["text"]})
        target = self.target_path(path)
        return {"source": self.source_relative(path), "target": target.relative_to(self.target).as_posix(),
                "sourceResourcePath": self.source_relative(path).removeprefix("Resources/").removesuffix(".prefab"),
                "resourcePath": "ApkEarningReference/Panels/" + path.stem,
                "guid": meta_guid(path.with_suffix(path.suffix + ".meta")),
                "hierarchy": [self.object_info(path, d["fileId"]) for d in docs if d["classId"] in (1, 4, 224, 114)],
                "removedControllers": components}

    def sanitize_prefab(self, path: Path) -> str:
        header, docs = self.get_documents(path)
        removed = self.removed_ids(path)
        parts = [header]
        for doc in docs:
            if doc["fileId"] in removed:
                continue
            text = doc["text"]
            text = re.sub(r"(?m)^  - component: \{fileID: (-?\d+)\}\n", lambda m: "" if int(m[1]) in removed else m[0], text)
            if doc["classId"] == 1001:
                text = serialized_array(text, "m_Modifications", lambda entry: not self.is_removed(REFERENCE.search(entry), path))
                text = serialized_array(text, "m_RemovedComponents", lambda entry: not self.is_removed(REFERENCE.search(entry), path))
                text = serialized_array(text, "m_AddedComponents", lambda entry: not self.is_removed(re.search(r"addedObject: (\{fileID: [^\n]+\})", entry) and REFERENCE.search(entry.split("addedObject:", 1)[1]), path))
            text = REFERENCE.sub(lambda ref: "{fileID: 0}" if self.is_removed(ref, path) else ref[0], text)
            text = clear_persistent_calls(text)
            parts.append(text)
        self.records.append(self.prefab_record(path))
        return "".join(parts)

    def collect(self, path: Path):
        path = path.resolve()
        if path in self.assets:
            return
        if not path.is_relative_to(self.source_assets) or not path.is_file():
            self.fail(f"Missing or out-of-source asset: {path}")
        if path.suffix.lower() not in ALLOWED_SUFFIXES:
            self.fail(f"Unexpected non-visual dependency: {path}")
        meta = path.with_suffix(path.suffix + ".meta")
        if not meta.exists():
            self.fail(f"Missing metadata: {meta}")
        guid = meta_guid(meta)
        if guid in self.target_guids:
            self.fail(f"Existing target GUID collision: {path} -> {self.target_guids[guid]}")
        text = None
        if path.suffix == ".prefab":
            text = self.sanitize_prefab(path)
        elif path.suffix in YAML_SUFFIXES:
            text = read(path)
            for doc in documents(text)[1]:
                if doc["classId"] == 114:
                    script = field_reference(doc["text"], "m_Script")
                    if script and script[2] not in self.package_scripts:
                        self.fail(f"Custom ScriptableObject survived visual dependency filtering: {path}")
            text = clear_persistent_calls(text)
        elif path.suffix in (".shader", ".cginc", ".hlsl"):
            text = read(path)
        data = text.encode("utf-8") if text is not None else path.read_bytes()
        self.assets[path] = {"source": self.source_relative(path), "target": self.target_path(path).relative_to(self.target).as_posix(),
                             "guid": guid, "sourceSha256": sha(path.read_bytes()), "targetSha256": sha(data), "bytes": len(data), "data": data}
        if path.suffix in YAML_SUFFIXES:
            for dependency in set(GUID.findall(text)):
                if dependency in BUILTIN_GUIDS:
                    self.builtin_refs.add(dependency)
                elif dependency in self.package_scripts:
                    self.package_refs[dependency] = self.package_scripts[dependency].relative_to(self.source).as_posix()
                elif dependency in self.guids:
                    self.collect(self.guids[dependency])
                else:
                    self.fail(f"Unresolved external GUID {dependency} in {path}")
        if path.suffix in (".shader", ".cginc", ".hlsl"):
            for include in re.findall(r'^\s*#include\s+["<]([^">]+)[">]', text, re.M):
                local = (path.parent / include).resolve()
                builtin = self.unity / "Data/CGIncludes" / include
                if local.is_file():
                    self.shader_includes.append({"shader": self.source_relative(path), "include": include, "source": self.source_relative(local)})
                    self.collect(local)
                elif builtin.is_file():
                    self.shader_includes.append({"shader": self.source_relative(path), "include": include, "unityBuiltin": True})
                else:
                    self.fail(f"Undeclared shader include {include} in {path}")

    def validate(self):
        targets = {}
        for path, asset in self.assets.items():
            target = asset["target"].lower()
            if target in targets:
                self.fail(f"Target asset collision: {path} and {targets[target]}")
            targets[target] = path
            if path.suffix != ".prefab":
                continue
            text = asset["data"].decode()
            _, docs = documents(text)
            ids = {d["fileId"] for d in docs}
            for doc in docs:
                script = field_reference(doc["text"], "m_Script")
                if script and script[2] not in self.package_scripts:
                    self.fail(f"Custom script survived sanitization: {path}")
                for ref in REFERENCE.finditer(doc["text"]):
                    if not ref[2] and int(ref[1]) and int(ref[1]) not in ids:
                        self.fail(f"Dangling local fileID {ref[1]} in {path}")
                if re.search(r"m_Calls:\n\s*-", doc["text"]):
                    self.fail(f"Persistent Unity event survived in {path}")

    def write(self, missing_only=False) -> dict:
        if sha(self.apk.read_bytes()) != APK_SHA256:
            self.fail("APK does not match the audited reference SHA256")
        for name in PANELS:
            self.collect(self.source_assets / "Resources/prefabs/panels" / (name + ".prefab"))
        for name in EXTRA_ASSETS:
            self.collect(self.source_assets / name)
        self.validate()
        # Validate everything before any project mutation. Writes stay in one new root.
        for source, asset in self.assets.items():
            target = self.target / asset["target"]
            target.parent.mkdir(parents=True, exist_ok=True)
            if not target.exists() or (not missing_only and target.read_bytes() != asset["data"]):
                target.write_bytes(asset["data"])
            target.with_suffix(target.suffix + ".meta").write_bytes(source.with_suffix(source.suffix + ".meta").read_bytes())
        manifest = {"schemaVersion": 1, "referenceApk": str(self.apk), "apkSha256": APK_SHA256,
                    "sourceProject": str(self.source), "targetProject": str(self.target),
                    "scope": "Visual-only prefab import; source custom components removed; no gameplay, SDK, payment or save code imported",
                    "prefabs": sorted(self.records, key=lambda r: r["source"]),
                    "assets": [{k: v for k, v in a.items() if k != "data"} for _, a in sorted(self.assets.items())],
                    "packageScriptDependencies": self.package_refs, "builtinGuids": sorted(self.builtin_refs),
                    "shaderIncludes": self.shader_includes, "apkLayoutOverrides": self.overrides,
                    "layoutAudit": "APK matches source. TXPanel/main/Image has two differently sized siblings; retain both source fileIDs and geometry.",
                    "unresolvedExternalGuids": self.missing,
                    "validation": {"customScripts": 0, "persistentUnityEvents": 0, "danglingLocalFileIds": 0,
                                   "guidCollisions": 0, "assetCount": len(self.assets), "prefabCount": len(self.records)}}
        manifest_path = self.output / "apk-earning-ui-import.json"
        manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        self.write_meta(manifest_path, False)
        for folder in [self.output, *sorted(p for p in self.output.rglob("*") if p.is_dir())]:
            self.write_meta(folder, True)
        return {"manifest": str(manifest_path), "assets": len(self.assets), "prefabs": len(self.records),
                "bytes": sum(a["bytes"] for a in self.assets.values()), "removedComponents": sum(len(r["removedControllers"]) for r in self.records),
                "apkLayoutOverrides": len(self.overrides), "unresolved": self.missing}

    def write_meta(self, path: Path, folder: bool):
        meta = path.with_suffix(path.suffix + ".meta")
        if meta.exists():
            return
        relative = path.relative_to(self.target / "Assets").as_posix()
        guid = uuid.uuid5(IMPORT_NAMESPACE, relative).hex
        text = f"fileFormatVersion: 2\nguid: {guid}\n"
        text += "folderAsset: yes\nDefaultImporter:\n" if folder else "TextScriptImporter:\n"
        text += "  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        meta.write_text(text, encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=Path("C:/Projects/com.nut.sort.relax"))
    parser.add_argument("--target", type=Path, default=Path(__file__).resolve().parents[1] / "output_Unity")
    parser.add_argument("--apk", type=Path, default=Path("C:/Projects/com.nut.sort.relax/Builds/Android/ColorNutParty-1.0.1-20261013-app-icon-fix.apk"))
    parser.add_argument("--unity", type=Path, default=Path("C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor"))
    parser.add_argument("--missing-only", action="store_true", help="Add visual dependencies without overwriting existing authored prefabs")
    args = parser.parse_args()
    print(json.dumps(Importer(args.source, args.target, args.apk, args.unity).write(args.missing_only), indent=2))


if __name__ == "__main__":
    main()
