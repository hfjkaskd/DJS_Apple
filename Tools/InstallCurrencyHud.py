"""Bounded prefab edits: replace the legacy coin entry and enable inventory-only Buttons.

Preserves every unrelated YAML document, including the game board and animations.
Run after the saved WithdrawalHud prefab has been authored in Unity.
"""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / 'output_Unity/Assets/res/local/coreplay/CorePlayUI.prefab'
HEADER = re.compile(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\n', re.M)
HUD_GUID = '2e84cbe0fcaebea4486e5ba7058de44b'
BUTTON_GUID = '4e29b1a8efbd4b44bb3f3716e73f07ff'


def main():
    original = PATH.read_bytes()
    text = original.decode('utf-8').replace('\r\n', '\n')
    matches = list(HEADER.finditer(text))
    docs = {m[2]: text[m.start(): matches[i + 1].start() if i + 1 < len(matches) else len(text)] for i, m in enumerate(matches)}
    updated = dict(docs)
    added = []
    button_count = 0
    for file_id, doc in docs.items():
        if 'guid: d9c7a76c884c2e964193a2b8a8f91417,' in doc:
            updated[file_id], count = re.subn(r'  m_CommonCoinBtn: \{fileID: -?\d+\}', '  m_CommonCoinBtn: {fileID: 0}', doc)
            assert count == 1
        if 'guid: 6453f6ab49a305a233fb78ef5df00050,' in doc:
            go_id = re.search(r'  m_GameObject: \{fileID: (\d+)\}', doc)[1]
            updated[go_id] = docs[go_id].replace('  m_IsActive: 1\n', '  m_IsActive: 0\n')
        if f'  m_SourcePrefab: {{fileID: 100100000, guid: {HUD_GUID}, type: 3}}' in doc:
            updated[file_id], count = re.subn(r'    m_Modifications:\n.*?(?=    m_RemovedComponents:)', '    m_Modifications: []\n', doc, flags=re.S)
            # A second run already has no overrides.
            assert count == 1 or '    m_Modifications: []\n' in doc
        if 'guid: e27dfa1edef8d05e624efed135e4c05e,' not in doc:
            continue
        go_id = re.search(r'  m_GameObject: \{fileID: (\d+)\}', doc)[1]
        components = re.findall(r'  - component: \{fileID: (\d+)\}', docs[go_id])
        if any(f'guid: {BUTTON_GUID},' in docs[c] for c in components):
            button_count += 1
            continue
        # The existing visible background is a child of this button root. Retire the
        # legacy empty CustomImage hit region and use the actual artwork as the target.
        graphics = [re.search(r'  m_ItemBG: \{fileID: (\d+)\}', doc)[1]]
        assert '  m_Sprite:' in docs[graphics[0]]
        updated[graphics[0]] = docs[graphics[0]].replace('  m_RaycastTarget: 0\n', '  m_RaycastTarget: 1\n')
        for component in components:
            if '  m_RaycastTarget:' in docs[component] and '  m_Sprite:' not in docs[component]:
                updated[component] = docs[component].replace('  m_RaycastTarget: 1\n', '  m_RaycastTarget: 0\n')
        button_id = str(7900401400010000 + button_count)
        assert button_id not in docs
        button_count += 1
        updated[go_id] = docs[go_id].replace('  m_Layer:', f'  - component: {{fileID: {button_id}}}\n  m_Layer:', 1)
        added.append(f'''--- !u!114 &{button_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {BUTTON_GUID}, type: 3}}
  m_Name:
  m_EditorClassIdentifier:
  m_Navigation:
    m_Mode: 0
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_PressedColor: {{r: 0.82, g: 0.88, b: 0.94, a: 1}}
    m_SelectedColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_DisabledColor: {{r: 0.65, g: 0.65, b: 0.65, a: 0.8}}
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
  m_TargetGraphic: {{fileID: {graphics[0]}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
''')
    assert button_count == 4, button_count
    result = text[:matches[0].start()] + ''.join(updated.values()) + ''.join(added)
    assert PATH.read_bytes() == original, 'Prefab changed during analysis'
    PATH.write_bytes(result.encode('utf-8'))
    changed = sum(docs[k] != updated[k] for k in docs)
    print(f'Currency HUD installed: {changed} bounded documents changed, {len(added)} standard Buttons added; all other documents unchanged.')


if __name__ == '__main__':
    main()
