"""Offline authoring tool. Writes static UGUI assets; never runs in the player.

Run from output_Unity. Layout, colors, labels and Button references remain editable
in the resulting Prefabs. Existing project prefab content is preserved.
"""
from pathlib import Path
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[1]
IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"
BUTTON = "4e29b1a8efbd4b44bb3f3716e73f07ff"
TEXT = "3f8a1c2b9d4e5f60718293a4b5c6d7e8"
FONT = "05f05f2053e2fef4d92a08aa0d1478de"
UI_SCRIPT = "harvest-rewards-ui"
PREFAB = "Assets/res/local/harvest/HarvestRewardsUI.prefab"


def guid(name):
    return hashlib.md5(("djs-apple/" + name).encode()).hexdigest()


def write(path, value):
    file = ROOT / path
    file.parent.mkdir(parents=True, exist_ok=True)
    file.write_text(value, encoding="utf-8", newline="\n")


def metadata(path, name, folder=False, script=False):
    body = f"fileFormatVersion: 2\nguid: {guid(name)}\n"
    if folder:
        body += "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
    elif script:
        body += "MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {fileID: 0}\n"
    else:
        body += "PrefabImporter:\n  externalObjects: {}\n"
    write(path + ".meta", body + "  userData:\n  assetBundleName:\n  assetBundleVariant:\n")


def rgba(hex_value):
    v = hex_value.lstrip("#")
    if len(v) == 6:
        v += "ff"
    a = [int(v[i:i + 2], 16) / 255 for i in range(0, 8, 2)]
    return "{" + ", ".join(f"{k}: {n:.6f}" for k, n in zip("rgba", a)) + "}"


def ref(n):
    return f"{{fileID: {n}}}"


def sprite_ref(path):
    meta = (ROOT / (path + '.meta')).read_text(encoding='utf-8-sig')
    asset_guid = re.search(r'^guid: ([a-f0-9]+)', meta, re.M).group(1)
    return f'{{fileID: 21300000, guid: {asset_guid}, type: 2}}'


BASE = "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"


class Prefab:
    def __init__(self, first=910000000):
        self.next_id = first
        self.nodes = []

    def alloc(self):
        self.next_id += 1
        return self.next_id

    def node(self, name, parent=None, x=0, y=0, w=100, h=100, anchor=(.5, .5), stretch=False, active=True):
        n = dict(go=self.alloc(), rt=self.alloc(), name=name, parent=parent, x=x, y=y, w=w, h=h,
                 anchor=anchor, stretch=stretch, active=active, children=[], extra=[])
        self.nodes.append(n)
        if isinstance(parent, dict):
            parent["children"].append(n["rt"])
        return n

    def component(self, node, script, data):
        component = self.alloc()
        b = f"--- !u!114 &{component}\nMonoBehaviour:\n{BASE}  m_GameObject: {ref(node['go'])}\n"
        b += f"  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {{fileID: 11500000, guid: {script}, type: 3}}\n  m_Name:\n  m_EditorClassIdentifier:\n{data}"
        node["extra"].append((component, b))
        return component

    def renderer(self, n):
        i = self.alloc()
        n["extra"].append((i, f"--- !u!222 &{i}\nCanvasRenderer:\n{BASE}  m_GameObject: {ref(n['go'])}\n  m_CullTransparentMesh: 1\n"))

    def canvas(self, n, sorting_order=2000):
        i = self.alloc()
        body = f"--- !u!223 &{i}\nCanvas:\n{BASE}  m_GameObject: {ref(n['go'])}\n"
        body += """  m_Enabled: 1
  serializedVersion: 3
  m_RenderMode: 0
  m_Camera: {fileID: 0}
  m_PlaneDistance: 100
  m_PixelPerfect: 0
  m_ReceivesEvents: 1
  m_OverrideSorting: 1
  m_OverridePixelPerfect: 0
  m_SortingBucketNormalizedSize: 0
  m_VertexColorAlwaysGammaSpace: 0
  m_AdditionalShaderChannelsFlag: 25
  m_UpdateRectTransformForStandalone: 0
  m_SortingLayerID: 0
  m_SortingOrder: SORTING_ORDER
  m_TargetDisplay: 0
"""
        n["extra"].append((i, body.replace("SORTING_ORDER", str(sorting_order))))
        self.component(n, "dc42784cf147c0c48a680349fa168899", """  m_IgnoreReversedGraphics: 1
  m_BlockingObjects: 0
  m_BlockingMask:
    serializedVersion: 2
    m_Bits: 4294967295
""")

    def image(self, n, color, raycast=False, sprite=None, sliced=True):
        self.renderer(n)
        return self.component(n, IMAGE, f"""  m_Material: {{fileID: 0}}
  m_Color: {rgba(color)}
  m_RaycastTarget: {int(raycast)}
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {sprite_ref(sprite) if sprite else ref(0)}
  m_Type: {1 if sprite and sliced else 0}
  m_PreserveAspect: {0 if sliced else 1}
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 3
""")

    def label(self, name, parent, text, x=0, y=0, w=880, h=70, size=36, color="754221", left=False):
        n = self.node(name, parent, x, y, w, h)
        self.renderer(n)
        component = self.component(n, TEXT, f"""  m_Material: {{fileID: 0}}
  m_Color: {rgba(color)}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: {json.dumps(text)}
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: {FONT}, type: 2}}
  m_sharedMaterial: {{fileID: 2100000, guid: 38ec4ccf33978154091d0d1eaab0f998, type: 2}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {rgba(color)}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontSize: {size}
  m_fontSizeBase: {size}
  m_fontWeight: 400
  m_enableAutoSizing: 1
  m_fontSizeMin: {max(22, size-6)}
  m_fontSizeMax: {size}
  m_fontStyle: 0
  m_HorizontalAlignment: {1 if left else 2}
  m_VerticalAlignment: 512
  m_textAlignment: 65535
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 4
  m_paragraphSpacing: 6
  m_enableWordWrapping: 1
  m_wordWrappingRatios: 0.4
  m_overflowMode: 0
  m_linkedTextComponent: {{fileID: 0}}
  m_enableKerning: 1
  m_enableExtraPadding: 0
  m_isRichText: 0
  m_parseCtrlCharacters: 1
  m_isOrthographic: 1
  m_isCullingEnabled: 0
  m_horizontalMapping: 0
  m_verticalMapping: 0
  m_geometrySortingOrder: 0
  m_VertexBufferAutoSizeReduction: 0
  m_useMaxVisibleDescender: 1
  m_pageToDisplay: 1
  m_margin: {{x: 4, y: 0, z: 4, w: 0}}
  m_isVolumetricText: 0
  m_hasFontAssetChanged: 0
""")
        return component

    def button(self, name, parent, label, x=0, y=0, w=400, h=100, color="ffffff", size=36, anchor=(.5, .5), sprite="Assets/res/local/common/sprite/btn/Btn_Normal.asset"):
        n = self.node(name, parent, x, y, w, h, anchor)
        img = self.image(n, color, True, sprite)
        btn = self.component(n, BUTTON, f"""  m_Navigation:
    m_Mode: 0
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.93, g: 0.97, b: 0.90, a: 1}}
    m_PressedColor: {{r: 0.76, g: 0.86, b: 0.68, a: 1}}
    m_SelectedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_DisabledColor: {{r: 0.55, g: 0.59, b: 0.50, a: 0.7}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {ref(img)}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
""")
        text_id = self.label(name + "Label", n, label, w=w-24, h=h-14, size=size, color="103d2c")
        return n, btn, text_id

    def render(self, header=True):
        result = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n" if header else ""
        for n in self.nodes:
            comps = [n['rt']] + [i for i, _ in n["extra"]]
            result += f"--- !u!1 &{n['go']}\nGameObject:\n{BASE}  serializedVersion: 6\n  m_Component:\n"
            result += "".join(f"  - component: {ref(i)}\n" for i in comps)
            result += f"  m_Layer: 5\n  m_Name: {n['name']}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: {int(n['active'])}\n"
            result += f"--- !u!224 &{n['rt']}\nRectTransform:\n{BASE}  m_GameObject: {ref(n['go'])}\n"
            result += "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n"
            result += "  m_Children:\n" + "".join(f"  - {ref(c)}\n" for c in n['children']) if n['children'] else "  m_Children: []\n"
            parent = n['parent']['rt'] if isinstance(n['parent'], dict) else n['parent'] or 0
            amin = (0, 0) if n['stretch'] else n['anchor']
            amax = (1, 1) if n['stretch'] else n['anchor']
            result += f"  m_Father: {ref(parent)}\n  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}\n  m_AnchorMin: {{x: {amin[0]}, y: {amin[1]}}}\n  m_AnchorMax: {{x: {amax[0]}, y: {amax[1]}}}\n  m_AnchoredPosition: {{x: {n['x']}, y: {n['y']}}}\n  m_SizeDelta: {{x: {n['w']}, y: {n['h']}}}\n  m_Pivot: {{x: 0.5, y: 0.5}}\n"
            result += "".join(b for _, b in n["extra"])
        return result


if __name__ == "__main__":
    from HarvestFlowPrefabs import generate
    generate()
