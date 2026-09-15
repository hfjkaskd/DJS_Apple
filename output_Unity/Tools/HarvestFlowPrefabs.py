"""Static harvest UI authored from measured v3 concept-image pixel rectangles.

The reference phone is 373 x 756 px. Every modal coordinate below is measured in
that image, then converted offline into a 1154.524 x 2340 design container.
Runtime only selects the already-authored pages and states.
"""
import re
import json
from GenerateHarvestRewardsUI import Prefab, ROOT, ref, guid, write, metadata

SPRITES = 'Assets/res/local/harvest/sprite/'
PANEL = SPRITES + 'PanelSkin.png'
INSET = SPRITES + 'InsetSkin.png'
TIER = SPRITES + 'TierSkin.png'
SELECTED = SPRITES + 'TierSelectedSkin.png'
GREEN = SPRITES + 'ButtonGreenSkin.png'
ORANGE = SPRITES + 'TitleOrangeSkin.png'
SECONDARY = SPRITES + 'SecondarySkin.png'
CASH = SPRITES + 'HarvestCash.png'
POPUP = 'Assets/res/local/harvest/HarvestRewardPopupUI.prefab'
CENTER = 'Assets/res/local/harvest/HarvestRewardsUI.prefab'
SLIDER = '67db9e8f0e2ae9c40bc1e2b64352a6b4'
REFERENCE_SCREEN = (453, 127, 373, 756)
REFERENCE_SCALE = 2340 / 756
DESIGN_WIDTH = 373 * REFERENCE_SCALE
CARD = (466, 224, 350, 607)


def fields(values, arrays=None, base=False):
    s = '  m_OpenAniName:\n  m_IdleAniName:\n  m_CloseAniName:\n' if base else ''
    s += ''.join(f'  {k}: {ref(v)}\n' for k, v in values.items())
    for k, values in (arrays or {}).items():
        s += f'  {k}:\n' + ''.join(f'  - {ref(v)}\n' for v in values)
    return s


class ReferenceLayout:
    """Translate measured image rectangles; no runtime coordinate calculations."""
    def __init__(self, prefab, root, screen=REFERENCE_SCREEN, scale=REFERENCE_SCALE):
        self.p = prefab
        self.scale = scale
        root['reference_rect'] = screen

    def rect(self, parent, box):
        x, y, w, h = box
        px, py, pw, ph = parent['reference_rect']
        s = self.scale
        return dict(x=(x+w/2-px-pw/2)*s, y=(py+ph/2-y-h/2)*s, w=w*s, h=h*s)

    def node(self, name, parent, box, active=True, rotation=0):
        node = self.p.node(name, parent, active=active, rotation=rotation, **self.rect(parent, box))
        node['reference_rect'] = box
        return node

    def image(self, name, parent, box, sprite, color='ffffff', sliced=True, active=True, rotation=0):
        node = self.node(name, parent, box, active=active, rotation=rotation)
        self.p.image(node, color, sprite=sprite, sliced=sliced, pixels_per_unit=1)
        return node

    def label(self, name, parent, text, box, size=16, color='623818', role='body', left=False, vertical='middle'):
        return self.p.label(name, parent, text, size=size*self.scale, color=color,
                            left=left, font_role=role, vertical=vertical, **self.rect(parent, box))

    def button(self, name, parent, text, box, size=20, sprite=GREEN, color='ffffff',
               text_color='fffbed', role='title', label_box=None):
        coords = self.rect(parent, box)
        label_coords = {}
        if label_box is not None:
            proxy = {'reference_rect': box}
            label_coords = {'label_'+k: v for k, v in self.rect(proxy, label_box).items()}
        node, button, label = self.p.button(name, parent, text, size=size*self.scale,
            color=color, sprite=sprite, text_color=text_color, font_role=role,
            **coords, **label_coords)
        node['reference_rect'] = box
        return node, button, label

    def cash(self, parent, box, rotation=0):
        return self.image('HarvestCash', parent, box, CASH, sliced=False, rotation=rotation)

    def progress(self, parent, name, box, value=.25):
        node = self.image(name, parent, box, SPRITES+'ProgressTrackSkin.png')
        fill = self.p.node(name+'Fill', node, w=-8, h=-8, stretch=True)
        graphic = self.p.image(fill, 'ffffff', sprite=SPRITES+'ProgressFillSkin.png', pixels_per_unit=1)
        return self.p.component(node, SLIDER, f'''  m_Navigation:
    m_Mode: 0
  m_Transition: 0
  m_Interactable: 0
  m_TargetGraphic: {ref(graphic)}
  m_FillRect: {ref(fill['rt'])}
  m_HandleRect: {{fileID: 0}}
  m_Direction: 0
  m_MinValue: 0
  m_MaxValue: 1
  m_WholeNumbers: 0
  m_Value: {value}
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
''')


def design_container(p, root, screen, occupied, dimmer):
    design = p.node('DesignRoot', root, w=DESIGN_WIDTH, h=2340)
    p.component(root, guid('harvest-reference-layout'), f'''  contentRoot: {ref(design['rt'])}
  dimmerRoot: {ref(dimmer['rt'])}
  designResolution: {{x: {DESIGN_WIDTH}, y: 2340}}
  occupiedSize: {{x: {occupied[0]}, y: {occupied[1]}}}
''')
    return design, ReferenceLayout(p, design, screen)


def track(layout, parent, box, diameter=30, compact=False):
    x, y, w, h = box
    root = layout.node('StageTrack', parent, box)
    line = layout.node('TrackLine', root, (x+diameter/2, y+diameter/2-2, w-diameter, 4))
    layout.p.image(line, 'c5b6aa')
    complete, current = [], []
    for i in range(6):
        left = x+i*(w-diameter)/5
        bubble = (left, y, diameter, diameter)
        node = layout.image('Stage'+str(i+1), root, bubble, SPRITES+'StageGraySkin.png')
        done = layout.image('Complete', node, bubble, SPRITES+'StageGreenSkin.png', active=False)
        active = layout.image('Current', node, bubble, SPRITES+'StageOrangeSkin.png', active=i==0)
        if not compact:
            layout.image('CompletedCheck', done, bubble, SPRITES+'CheckBadge.png', sliced=False)
        label_box = bubble if compact else (left-2, y+diameter+4, diameter+4, 19)
        layout.label('StageNumber', node, str(i+1), label_box, size=9 if compact else 13,
                     role='body', color='623818')
        complete.append(done['go'])
        current.append(active['go'])
    return root['go'], complete, current


def cash_stack(layout, parent, center_x, top_y, width=238):
    # Three references to one small icon are authored into the prefab.
    s = width/238
    layout.cash(parent, (center_x-119*s, top_y+42*s, 121*s, 97*s), rotation=12)
    layout.cash(parent, (center_x-2*s, top_y+45*s, 121*s, 96*s), rotation=-12)
    layout.cash(parent, (center_x-62*s, top_y, 124*s, 112*s))


def build_center():
    p = Prefab()
    root = p.node('HarvestRewardsUI', w=0, h=0, stretch=True)
    p.canvas(root)
    dimmer = p.node('Dimmer', root, w=DESIGN_WIDTH, h=2340)
    p.image(dimmer, '112b4596', True)
    design, ui = design_container(p, root, REFERENCE_SCREEN,
        (350*REFERENCE_SCALE, 632*REFERENCE_SCALE), dimmer)
    card = ui.image('FlowCard', design, CARD, PANEL)
    title_tab = ui.image('TitleTab', card, (521, 199, 237, 49), ORANGE)
    title = ui.label('Title', title_tab, 'HARVEST WALLET', (530, 204, 219, 38), 22, 'fffbed', 'title')
    _, close, _ = ui.button('Close', card, '', (769, 213, 46, 45), sprite=SPRITES+'CloseSkin.png')
    ui.label('SimulationNotice', card, 'SIMULATED - NO CASH VALUE', (480, 253, 322, 20), 11, '6d381b')
    footer = ui.node('DetailFooter', card, (479, 770, 316, 47), active=False)
    _, back, _ = ui.button('Back', footer, 'Back', (481, 772, 96, 36), 13, SECONDARY, text_color='70533e', role='body')
    _, rules, _ = ui.button('Rules', footer, 'Rules', (590, 772, 96, 36), 13, SECONDARY, text_color='70533e', role='body')
    _, history, _ = ui.button('History', footer, 'Records', (699, 772, 96, 36), 13, SECONDARY, text_color='70533e', role='body')
    feedback = ui.label('Feedback', card, '', (489, 655, 305, 50), 13, vertical='top')
    values = dict(titleText=title, closeButton=close, backButton=back, rulesButton=rules,
                  historyButton=history, detailFooterRoot=footer['go'], feedbackText=feedback)
    pages = {}
    for name in ['wallet', 'confirmation', 'progress', 'settlement', 'completed', 'history', 'rules']:
        pages[name] = ui.node(name.title()+'Page', card, CARD, active=name=='wallet')
        values[name+'Root'] = pages[name]['go']

    wallet = pages['wallet']
    balance = ui.image('BalanceCard', wallet, (485, 277, 309, 103), INSET)
    ui.cash(balance, (520, 285, 83, 81))
    values['availableText'] = ui.label('AvailableValue', balance, '247.61', (611, 274.5, 169, 64), 43, role='amount', left=True)
    ui.label('AvailableHeading', balance, 'Available rewards', (616, 329, 165, 23), 16, left=True)
    values['frozenText'] = ui.label('FrozenValue', balance, 'In challenge: 0.00', (616, 351, 168, 19), 12, left=True)
    ui.label('ChooseTier', wallet, 'Choose your target', (494, 388, 294, 30), 18)
    buttons, labels, marks = [], [], []
    for i, amount in enumerate(['1,000', '1,500', '3,000', '5,000', '8,000', '10,000']):
        x, y = 485+(i%2)*163, 423+(i//2)*66
        box = (x, y, 148, 57)
        node, button, label = ui.button('Tier'+str(i+1), wallet, amount, box, 23,
            TIER, text_color='623818', role='amount', label_box=(x+51, y+5, 90, 48))
        mark = ui.image('SelectedMarker', node, box, SELECTED, active=i==0)
        ui.image('SelectedBadge', mark, (x+122, y-2, 26, 26), SPRITES+'CheckBadge.png', sliced=False)
        node['children'].remove(mark['rt'])
        node['children'].insert(0, mark['rt'])
        ui.cash(node, (x+9, y+7, 42, 43))
        buttons.append(button); labels.append(label); marks.append(mark['go'])
    values['walletHintText'] = ui.label('WalletHint', wallet, '752.39 more to apply', (490, 619, 301, 29), 17)
    values['walletProgressBar'] = ui.progress(wallet, 'WalletProgress', (493, 652, 256, 21))
    values['percentageText'] = ui.label('WalletPercentage', wallet, '25%', (752, 651, 39, 23), 13)
    journey = ui.node('WalletJourneyHint', wallet, (489, 679, 305, 22))
    ui.label('JourneyText', journey, '6 stages + final wait after applying', (489, 679, 305, 22), 11, color='765644')
    values['walletJourneyHintRoot'] = journey['go']
    pending, values['pendingRewardButton'], values['pendingRewardText'] = ui.button(
        'PendingReward', wallet, 'Collect harvest reward', (499, 677, 285, 28),
        12, SECONDARY, text_color='70533e', role='body')
    pending['active'] = False
    _, values['applyButton'], values['applyText'] = ui.button(
        'Apply', wallet, 'KEEP HARVESTING', (491, 711, 297, 60), 23)
    _, values['walletRulesButton'], _ = ui.button('WalletRules', wallet, 'Rules',
        (510, 782, 122, 35), 13, SECONDARY, text_color='70533e', role='body')
    _, values['walletHistoryButton'], _ = ui.button('WalletHistory', wallet, 'Records',
        (647, 782, 122, 35), 13, SECONDARY, text_color='70533e', role='body')

    confirm = pages['confirmation']
    ui.label('ConfirmHeading', confirm, 'Confirm your request', (487, 281, 306, 32), 21)
    amount_card = ui.image('ConfirmAmountCard', confirm, (485, 325, 309, 98), INSET)
    ui.cash(amount_card, (514, 339, 72, 67))
    values['confirmationAmountText'] = ui.label('ConfirmationAmount', amount_card, '1,000.00', (594, 337, 188, 43), 34, role='amount', left=True)
    ui.label('RequestUnit', amount_card, 'Request amount', (595, 381, 189, 23), 15, left=True)
    summary = '{0} will be reserved for this request.\n\nComplete {1} stages with tasks and timers, then wait {2} hours. Minimum {3} days.\n\nOne active request. Each tier once.\nSimulation only; no cash payment.'
    summary_card = ui.image('SummaryCard', confirm, (485, 436, 309, 221), INSET)
    values['confirmationSummaryText'] = ui.label('ConfirmationSummary', summary_card,
        summary.format('1,000.00', 6, 168, 13), (499, 450, 281, 193), 14, left=True, vertical='top')
    _, values['confirmButton'], _ = ui.button('ConfirmApply', confirm, 'START CHALLENGE', (491, 671, 297, 59), 23)
    _, values['cancelButton'], _ = ui.button('CancelApply', confirm, 'Change target', (510, 740, 258, 29), 13,
        SECONDARY, text_color='70533e', role='body')

    stage = pages['progress']
    request = ui.image('RequestCard', stage, (487, 277, 304, 65), INSET)
    ui.cash(request, (520, 282, 57, 55))
    values['stageRequestAmountText'] = ui.label('StageRequestAmount', request, 'Request: 1,000.00',
        (585, 285, 198, 29), 19, role='body', left=True)
    ui.label('RequestStatus', request, 'In challenge', (586, 311, 195, 22), 14, left=True)
    _, complete, current = track(ui, stage, (487, 357, 306, 54), diameter=30)
    values['stageTitleText'] = ui.label('StageTitle', stage, 'Stage 2 of 6', (489, 416, 300, 29), 20)
    task = ui.image('TaskCard', stage, (487, 451, 304, 139), INSET)
    fruits = ui.node('TaskFruits', task, (557, 457, 168, 47), active=False)
    for i in range(3):
        ui.image('TaskFruit'+str(i+1), fruits, (567+i*53, 461, 38, 39), 'Assets/Sprite/Fruit_4.asset', sliced=False)
    levels = ui.node('TaskLevels', task, (608, 457, 62, 47))
    ui.image('TaskLevelStar', levels, (619, 461, 39, 39), 'Assets/res/local/pops/rate/sprite/Star.asset', sliced=False)
    values['stageTaskFruitsRoot'] = fruits['go']
    values['stageTaskLevelsRoot'] = levels['go']
    values['stageTaskText'] = ui.label('StageTask', task, 'Make 260 fruit triples', (497, 505, 284, 27), 17)
    values['stageCountText'] = ui.label('StageCount', task, '86 / 260', (510, 526.5, 260, 36), 24, role='amount')
    values['stageProgressBar'] = ui.progress(task, 'TaskProgress', (509, 561, 225, 18), .33)
    values['stagePercentageText'] = ui.label('StagePercentage', task, '33%', (736, 558, 47, 24), 13)
    timer = ui.image('TimerCard', stage, (487, 598, 304, 64), INSET)
    ui.image('StageClock', timer, (533, 605, 48, 49), SPRITES+'ClockSkin.png', sliced=False)
    ui.label('TimerHeading', timer, 'Stage wait', (600, 604, 181, 23), 15, left=True)
    values['stageTimerText'] = ui.label('StageTimer', timer, '18:42:10', (599, 620.5, 185, 38), 25, role='amount', left=True)
    values['stageHintText'] = ui.label('StageHint', stage, 'Complete both the task and timer',
        (499, 665, 280, 24), 11, color='765644')
    _, values['playButton'], _ = ui.button('ContinuePlaying', stage, 'CONTINUE PLAYING', (490, 697, 300, 60), 23)

    wait = pages['settlement']
    ui.label('SettlementHeading', wait, 'All 6 stages complete', (488, 281, 303, 36), 22)
    cash_stack(ui, wait, 640, 327, 182)
    values['settlementAmountText'] = ui.label('SettlementAmount', wait, '1,000.00', (496, 452, 287, 50), 39, role='amount')
    final_card = ui.image('FinalWaitCard', wait, (487, 516, 304, 94), INSET)
    ui.label('FinalWaitHeading', final_card, 'Final wait remaining', (501, 527, 277, 28), 17)
    values['settlementTimerText'] = ui.label('SettlementTimer', final_card, '6d 23:59:59', (498, 561, 284, 36), 28, role='amount')
    ui.label('WaitDescription', wait, 'Your progress is saved.\nYou can keep playing while you wait.', (503, 627, 276, 53), 15)
    _, values['settlementCloseButton'], _ = ui.button('SettlementClose', wait, 'BACK TO GAME', (490, 697, 300, 60), 23)

    done = pages['completed']
    ui.label('CompletedHeading', done, 'Harvest complete!', (488, 280, 303, 37), 24)
    cash_stack(ui, done, 640, 328, 199)
    values['completedAmountText'] = ui.label('CompletedAmount', done, '1,000.00', (495, 472, 289, 48), 39, role='amount')
    detail = ui.image('CompletedDetailCard', done, (487, 539, 304, 124), INSET)
    values['completedDetailText'] = ui.label('CompletedDetail', detail,
        'Challenge completed and recorded.\nThis was a simulation; no cash was paid.', (502, 555, 274, 92), 16)
    _, values['completedContinueButton'], _ = ui.button('CompletedContinue', done, 'NEXT HARVEST', (490, 697, 300, 60), 23)

    hist = pages['history']
    ui.cash(hist, (489, 282, 35, 32))
    ui.label('HistoryHeading', hist, 'Your completed harvests', (533, 282, 251, 30), 18)
    records = ui.image('HistoryCard', hist, (485, 326, 309, 426), INSET)
    values['historyText'] = ui.label('HistoryBody', records,
        'No completed challenges yet.\nYour completed harvests will appear here.',
        (501, 342, 277, 394), 15, left=True, vertical='top')
    rules_page = pages['rules']
    ui.label('RulesHeading', rules_page, 'The full journey', (491, 281, 298, 32), 21)
    rules_card = ui.image('RulesCard', rules_page, (485, 324, 309, 430), INSET)
    values['rulesText'] = ui.label('Terms', rules_card,
        'Six stages. Each stage needs its task and timer.\nAll amounts are simulated reward units.',
        (499, 338, 281, 400), 14, left=True, vertical='top')
    arrays = dict(tierButtons=buttons, tierLabels=labels, tierSelectedMarkers=marks,
                  stageCompleteMarks=complete, stageCurrentMarks=current)
    titles = ['HARVEST WALLET', 'CONFIRM REQUEST', 'HARVEST JOURNEY', 'FINAL WAIT',
              'HARVEST COMPLETE', 'HARVEST RECORDS', 'HARVEST RULES']
    unavailable = 'Rewards are temporarily unavailable. Keep your save data and restart the game.'
    p.component(root, guid('harvest-rewards-ui'), fields(values, arrays, True)+'  refreshIntervalSeconds: 1\n'
        +'  pageTitles:\n'+''.join('  - '+json.dumps(t)+'\n' for t in titles)
        +'  stageRequestAmountFormat: "Request: {0}"\n  completedTierLabel: Done\n'
        +'  confirmationSummaryFormat: '+json.dumps(summary)+'\n  unavailableMessage: '+json.dumps(unavailable)+'\n')
    write(CENTER, p.render()); metadata(CENTER, 'harvest-rewards-prefab')
    return root['go']


def build_popup():
    p = Prefab(930000000)
    root = p.node('HarvestRewardPopupUI', w=0, h=0, stretch=True)
    p.canvas(root)
    dimmer = p.node('Dimmer', root, w=DESIGN_WIDTH, h=2340)
    p.image(dimmer, '112b457d', True)
    design, ui = design_container(p, root, (848, 127, 373, 756),
        (343*REFERENCE_SCALE, 495*REFERENCE_SCALE), dimmer)
    card = ui.image('RewardCard', design, (863, 270, 342, 472), PANEL)
    title = ui.image('TitleTab', card, (921, 247, 227, 51), ORANGE)
    values = {}
    values['titleText'] = ui.label('Title', title, 'LEVEL COMPLETE', (930, 253, 209, 39), 22, 'fffbed', 'title')
    _, values['closeButton'], _ = ui.button('Close', card, '', (1158, 257, 47, 46), sprite=SPRITES+'CloseSkin.png')
    ui.label('SimulationNotice', card, 'SIMULATED - NO CASH VALUE', (895, 303, 279, 21), 11, color='6d381b')
    ui.image('RewardRays', card, (896, 327, 275, 206), SPRITES+'RewardRays.png', sliced=False)
    cash_stack(ui, card, 1033, 357, 238)
    values['amountText'] = ui.label('RewardAmount', card, '+10.00', (902, 475, 268, 70), 48, role='amount')
    values['hintText'] = ui.label('RewardHint', card, 'Harvest reward', (913, 533, 245, 27), 21)
    balance = ui.image('RewardBalanceCard', card, (897, 563, 271, 36), INSET)
    values['balanceText'] = ui.label('RewardBalance', balance, 'Balance after claim: 257.61', (906, 568, 253, 25), 15)
    _, values['claimButton'], values['claimText'] = ui.button('ClaimReward', card, 'COLLECT', (892, 610, 282, 66), 29)
    _, values['adButton'], values['adText'] = ui.button('BonusAd', card, 'Ad bonus unavailable',
        (921, 686, 224, 35), 13, SECONDARY, text_color='70533e', role='body')
    p.component(root, guid('harvest-reward-popup-ui'), fields(values, base=True)
        +'  refreshIntervalSeconds: 0.25\n  fruitTitle: HARVEST BONUS\n  winTitle: LEVEL COMPLETE\n  welcomeTitle: WELCOME GIFT\n'
        +'  welcomeHint: Added to your harvest balance.\n  rewardHint: Harvest reward\n'
        +'  balanceAfterClaimFormat: "Balance after claim: {0}"\n  currentBalanceFormat: "Balance: {0}"\n'
        +'  unavailableMessage: Rewards are paused. Keep your save data and restart the game.\n')
    write(POPUP, p.render()); metadata(POPUP, 'harvest-reward-popup-prefab')
    metadata('Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardPopupUI.cs', 'harvest-reward-popup-ui', script=True)
    return root['go']


def add_hud(path, root_transform, anchor, y, compact=False, dynamic_offset=None):
    original = (ROOT/path).read_text(encoding='utf-8-sig')
    original = re.sub(r'^--- !u!\d+ &920\d+\n.*?(?=^--- !u!|\Z)', '', original, flags=re.M|re.S)
    original = re.sub(r'^  - \{fileID: 920\d+\}\n', '', original, flags=re.M)
    original = re.sub(r'^  m_HarvestRewardsButton:.*\n', '', original, flags=re.M)
    p = Prefab(920000000)
    scale = 1080/349
    # Keep the reference width, tightening vertical gaps to fit between the
    # existing header and the highest authored fruit on the shortest canvas.
    design_height = 93
    height = design_height*scale
    # Fixed lower edge retains the already-validated gameplay clearance.
    hud = p.node('HarvestRewardsHUD', root_transform, y=y, w=1080, h=height, anchor=anchor)
    p.component(hud, guid('harvest-hud-fit'), '  referenceCanvasWidth: 1154.524\n')
    ui = ReferenceLayout(p, hud, (70, 223+(height/2-136)/scale, 349, design_height), scale)
    if dynamic_offset is not None:
        p.canvas(hud, dynamic_offset)
        p.component(hud, 'c51cf5ae94661753c94ece8e04a3c7ab', f'  offset: {dynamic_offset}\n  addGraphicRaycaster: 0\n  auto: 0\n')
    ui.image('BalanceBanner', hud, (70, 223, 349, design_height), SPRITES+'HudPanelSkin.png')
    ui.cash(hud, (85, 227, 47, 42))
    balance = ui.label('HudBalance', hud, '247.61', (142, 225, 135, 35), 23, role='amount')
    ui.label('HudUnit', hud, 'SIMULATED REWARDS', (141, 253, 137, 16), 10)
    _, button, _ = ui.button('HarvestRewardsEntry', hud, 'WITHDRAW', (285, 229, 119, 41),
        15, text_color='174b26')
    line = ui.node('HudDivider', hud, (92, 271, 306, 2))
    p.image(line, 'd8983f')
    goal = ui.label('HudGoal', hud, '752.39 to next target', (100, 274, 301, 19), 12)
    bar = ui.progress(hud, 'GoalProgress', (92, 296, 213, 14))
    fraction = ui.label('HudGoalFraction', hud, '247.61 / 1,000', (307, 293, 94, 20), 11)
    stage_root, complete, current = track(ui, hud, (98, 294, 296, 18), diameter=18, compact=True)
    values = dict(walletButton=button, balanceText=balance, goalText=goal,
                  goalProgressBar=bar, goalFractionText=fraction, stageTrackRoot=stage_root)
    p.component(hud, guid('harvest-rewards-hud'), fields(values,
        dict(completedMarks=complete, currentMarks=current))+'  refreshIntervalSeconds: 1\n')
    pattern = rf'(^--- !u!224 &{root_transform}\n.*?  m_Children:)(.*?)(\n  m_Father:)'
    match = re.search(pattern, original, flags=re.M|re.S)
    if match is None:
        raise RuntimeError('Missing prefab root '+path)
    children = match.group(2).replace(' []', '')
    original = original[:match.start()]+match.group(1)+children+'\n  - '+ref(hud['rt'])+match.group(3)+original[match.end():]
    write(path, original.rstrip()+'\n'+p.render(False))


def catalog(key, root, asset_name):
    path='Assets/Resources/GameResCatalog.asset'
    text=(ROOT/path).read_text(encoding='utf-8-sig')
    text=re.sub(rf'^  - key: {key}\n    asset:[^\n]*\n','',text,flags=re.M)
    text+=f'  - key: {key}\n    asset: {{fileID: {root}, guid: {guid(asset_name)}, type: 3}}\n'
    write(path,text)


def reserve_gameplay_hud_area():
    """Keep the board in its authored Map coordinate system, below the HUD.

    The full-screen Game image and basket stay unchanged. All level/undo positions
    remain local to Map/Layer; runtime reparenting normalizes fruit root scale.
    """
    path = 'Assets/res/local/coreplay/CorePlayUI.prefab'
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    text, count = re.subn(r'^  m_BgTrans: \{fileID: \d+\}$',
                         '  m_BgTrans: {fileID: 224989434536655964}', text, flags=re.M)
    if count != 1:
        raise RuntimeError('Expected one CorePlayUI board reference')
    pattern = r'^--- !u!224 &224989434536655964\n.*?(?=^--- !u!|\Z)'
    match = re.search(pattern, text, flags=re.M|re.S)
    if match is None:
        raise RuntimeError('Missing authored Game/Map RectTransform')
    block = re.sub(r'^  m_LocalScale:.*$', '  m_LocalScale: {x: 0.7, y: 0.7, z: 1}',
                   match.group(0), flags=re.M)
    block = re.sub(r'^  m_AnchoredPosition:.*$', '  m_AnchoredPosition: {x: 0, y: -100}',
                   block, flags=re.M)
    write(path, text[:match.start()] + block + text[match.end():])


def position_gameplay_header():
    """Move only the existing UI/Top Rect; opening clips do not animate this root."""
    path = 'Assets/res/local/coreplay/CorePlayUI.prefab'
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    pattern = r'^--- !u!224 &224894225446638996\n.*?(?=^--- !u!|\Z)'
    match = re.search(pattern, text, flags=re.M|re.S)
    if match is None:
        raise RuntimeError('Missing authored CorePlayUI UI/Top RectTransform')
    block, count = re.subn(r'^  m_AnchoredPosition:.*$',
                          '  m_AnchoredPosition: {x: 0, y: -72}', match.group(0), flags=re.M)
    if count != 1:
        raise RuntimeError('Expected one CorePlayUI header position')
    write(path, text[:match.start()] + block + text[match.end():])


def generate():
    catalog('res/local/harvest/harvestrewardsui', build_center(), 'harvest-rewards-prefab')
    catalog('res/local/harvest/harvestrewardpopupui', build_popup(), 'harvest-reward-popup-prefab')
    reserve_gameplay_hud_area()
    position_gameplay_header()
    add_hud('Assets/res/local/home/Home.prefab',224518489005915545,(.5,0),850,True)
    add_hud('Assets/res/local/coreplay/CorePlayUI.prefab',224429401810989285,(.5,1),-290)
    add_hud('Assets/res/local/coreplaywin/WinUI.prefab',224181586030114204,(.5,1),-245,True,40)
    metadata('Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardsHud.cs', 'harvest-rewards-hud', script=True)
    print('Authored wallet flow, reward popup and three harvest HUDs.')
