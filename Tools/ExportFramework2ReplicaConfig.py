"""Export the reference's live Framework 2 product, without Mahjong board data.

Requires PyYAML. Uses BaseLoader deliberately: Unity serializes int arrays as
hexadecimal strings (01000000), which ordinary YAML 1.1 readers treat as octal.
Run from anywhere with --source <reference repo> --project <target Unity project>.
"""

import argparse
import hashlib
import json
import re
import struct
from pathlib import Path

import yaml


SOURCE_PATH = "Assets/AddressableAssets/GameContent/WithdrawalFramework2/Data/MahjongLegacyFrameworkConfig.asset"
KEEP = (
    "currencyDatabase", "localizationDatabase", "economy", "sourceDefaults",
    "iaaMode", "hudBalanceAnimationSeconds", "timedRewardStartLevel",
    "useMahjongInGameRewardTrigger", "inGameRewardTriggerDelaySeconds",
    "inGameRewardProgressRatio", "inGameRewardForcedProgressRatio",
    "inGameRewardMinimumRoundSeconds", "gameplayGuideWithdrawalStage",
    "guideRewardDelaySeconds", "linearLevelTitleFormat", "refusalsBeforeAd",
    "refuseInterstitialLevelRange", "refuseInterstitialPercent",
)


def number(value):
    return float(value) if any(c in value.lower() for c in ".e") else int(value)


def numbers(value):
    if isinstance(value, dict):
        return {k: numbers(v) for k, v in value.items()}
    if isinstance(value, list):
        return [numbers(v) for v in value]
    return number(value)


def int_array(value):
    data = bytes.fromhex(value)
    if len(data) % 4:
        raise ValueError("Unity int array is not aligned to four bytes")
    return list(struct.unpack("<" + "i" * (len(data) // 4), data))


def export(source, project):
    source_file = source / SOURCE_PATH
    raw = source_file.read_bytes()
    lines = raw.decode("utf-8-sig").splitlines()
    text = "\n".join(line for line in lines if not line.startswith(("%", "---")))
    original = yaml.load(text, Loader=yaml.BaseLoader)["MonoBehaviour"]["framework2"]
    result = {key: original[key] for key in KEEP}

    # Every field in localization is a string. Do not reinterpret numeric text,
    # ISO country codes, localized yes/no, or an empty string as a YAML scalar.
    currency = result["currencyDatabase"]
    for profile in currency["profiles"]:
        profile["payTypes"] = int_array(profile["payTypes"])
        for key in ("multiplier", "exchangeRate", "fractionDigits"):
            profile[key] = number(profile[key])
        profile["symbolAfterAmount"] = profile["symbolAfterAmount"] == "1"
    for channel in currency["channels"]:
        channel["id"] = int(channel["id"])
        channel["inputKind"] = int(channel["inputKind"])

    economy = result["economy"]
    strings = {
        "gem": {"LuckMin"},
        "payRegions": {"code", "mark", "lang"},
        "withdrawalBarrage": {"play", "Stage"},
    }
    for key, value in list(economy.items()):
        if key == "luckyRewardLevels":
            economy[key] = int_array(value)
        elif key == "rewards":
            economy[key] = {
                k: int_array(v) if k == "gemExchangeCosts" else numbers(v)
                for k, v in value.items()
            }
        elif key in strings:
            economy[key] = [
                {k: v if k in strings[key] else numbers(v) for k, v in row.items()}
                for row in value
            ]
        else:
            economy[key] = numbers(value)

    result["sourceDefaults"] = {
        key: value == "1" if key == "EnableLuckyRewardOnPourComplete" else number(value)
        for key, value in result["sourceDefaults"].items()
    }
    for key in KEEP[4:]:
        if key in ("iaaMode", "useMahjongInGameRewardTrigger"):
            result[key] = result[key] == "1"
        elif key != "linearLevelTitleFormat":
            result[key] = numbers(result[key])

    def find_asset_references(value):
        if isinstance(value, dict):
            if "guid" in value or "fileID" in value:
                raise ValueError("Inline product unexpectedly contains a Unity asset reference")
            for child in value.values():
                find_asset_references(child)
        elif isinstance(value, list):
            for child in value:
                find_asset_references(child)
    find_asset_references(result)

    config_relative = "Assets/Framework2Replica/Resources/Framework2Replica/ProductConfig.json"
    path = project / config_relative
    path.parent.mkdir(parents=True, exist_ok=True)
    payload = (json.dumps(result, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    path.write_bytes(payload)
    meta = path.with_suffix(path.suffix + ".meta")
    if not meta.exists():
        meta.write_text("fileFormatVersion: 2\nguid: 18c70a8d3d5641eb952faf149db83985\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")

    fingerprint = {
        "sourceRepository": "https://github.com/bizza-game/Mahjong-Slide-Zen_AndroidWZ-DJS",
        "sourceCommit": "df1d9c11816bb87d972b40bb9def3447fe171e0a",
        "sourcePath": SOURCE_PATH,
        "sourceSha256": hashlib.sha256(raw).hexdigest(),
        "exportPath": config_relative,
        "exportSha256": hashlib.sha256(payload).hexdigest(),
        "exportBytes": len(payload),
        "retainedRoots": list(KEEP),
        "omittedMahjongHostRoots": [k for k in original if k not in KEEP],
        "economyCanonicalSha256": hashlib.sha256(json.dumps(result["economy"], sort_keys=True, separators=(",", ":")).encode()).hexdigest(),
        "stageRequirements": result["economy"]["stages"],
        "normalCoinHardCap": result["economy"]["normalCoinHardCap"],
        "note": "Live inline product data, not the retired Integration economy asset. Packed arrays decoded little-endian; scalar types match source DTOs. No gameplay/withdrawal values redesigned. Local simulation, not payment confirmation.",
    }
    record = project / "Docs/CommercializationAB/Framework2Replica/source-config-fingerprint.json"
    record.parent.mkdir(parents=True, exist_ok=True)
    record.write_text(json.dumps(fingerprint, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"config": str(path), "record": str(record), "bytes": len(payload), "sha256": fingerprint["exportSha256"]}))
    export_layout_baseline(source, record.parent)


def export_layout_baseline(source, directory):
    resource_root = source / "Assets/AddressableAssets/GameContent/WithdrawalFramework2"
    prefabs = []
    for path in sorted(resource_root.rglob("*.prefab")):
        text = path.read_text("utf-8-sig")
        rects = []
        for match in re.finditer(r"^--- !u!224 &(\d+)\s*\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S):
            node = yaml.load(match[2], Loader=yaml.BaseLoader)["RectTransform"]
            fields = {
                key: [float(v) for v in node[key].values()]
                for key in ("m_LocalPosition", "m_LocalRotation", "m_LocalScale", "m_AnchorMin", "m_AnchorMax", "m_AnchoredPosition", "m_SizeDelta", "m_Pivot")
            }
            rects.append({"fileId": match[1], **fields})
        prefabs.append({"path": path.relative_to(resource_root).as_posix(), "sourceSha256": hashlib.sha256(path.read_bytes()).hexdigest(), "rects": rects})
    record = directory / "source-layout-fingerprint.json"
    record.write_text(json.dumps({
        "sourceCommit": "df1d9c11816bb87d972b40bb9def3447fe171e0a",
        "note": "Serialized reference RectTransforms; audits unchanged source UI layout without starting source game logic. Host canvas adaptation is tested separately.",
        "prefabs": prefabs,
    }, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"layoutBaseline": str(record), "prefabs": len(prefabs), "rects": sum(len(p["rects"]) for p in prefabs)}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--project", type=Path, required=True)
    args = parser.parse_args()
    export(args.source.resolve(), args.project.resolve())
