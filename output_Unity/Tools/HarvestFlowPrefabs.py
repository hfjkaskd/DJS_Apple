"""Static prefab authoring for the harvest flow. No runtime UI construction."""
import re
import json
from GenerateHarvestRewardsUI import Prefab, ROOT, ref, guid, write, metadata

COMMON = 'Assets/res/local/common/sprite/'
PANEL = COMMON + 'pop/PopBg.asset'
GREEN = COMMON + 'btn/Btn_Normal.asset'
ORANGE = COMMON + 'btn/Btn_Orange.asset'
GRAY = COMMON + 'btn/Btn_Gray.asset'
POPUP = 'Assets/res/local/harvest/HarvestRewardPopupUI.prefab'
CENTER = 'Assets/res/local/harvest/HarvestRewardsUI.prefab'
SLIDER = '67db9e8f0e2ae9c40bc1e2b64352a6b4'


def fields(values, arrays=None, base=False):
    s = '  m_OpenAniName:\n  m_IdleAniName:\n  m_CloseAniName:\n' if base else ''
    s += ''.join(f'  {k}: {ref(v)}\n' for k, v in values.items())
    for k, values in (arrays or {}).items():
        s += f'  {k}:\n' + ''.join(f'  - {ref(v)}\n' for v in values)
    return s


def panel(p, parent, name, x=0, y=0, w=800, h=300, color='ffffff'):
    n = p.node(name, parent, x=x, y=y, w=w, h=h)
    p.image(n, color, sprite=PANEL)
    return n


def icon(p, parent, x=0, y=0, w=130, h=130):
    n = p.node('HarvestFruit', parent, x=x, y=y, w=w, h=h)
    p.image(n, 'ffffff', sprite='Assets/Sprite/Fruit_4.asset', sliced=False)


def progress_bar(p, parent, name, y=0, w=720, h=32):
    n = p.node(name, parent, y=y, w=w, h=h)
    p.image(n, 'ffffff', sprite=COMMON+'ProgressBg.asset')
    fill = p.node(name + 'Fill', n, w=-8, h=-8, stretch=True)
    graphic = p.image(fill, 'ffffff', sprite=GREEN)
    return p.component(n, SLIDER, f'''  m_Navigation:
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
  m_Value: 0.35
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
''')


def track(p, parent, y=0, width=750, diameter=90):
    root = p.node('StageTrack', parent, y=y, w=width, h=diameter+20)
    line = p.node('TrackLine', root, w=width-diameter, h=16)
    p.image(line, 'd5b088')
    complete, current = [], []
    for i in range(6):
        n = p.node('Stage' + str(i+1), root, x=(i-2.5)*(width-diameter)/5, w=diameter, h=diameter)
        p.image(n, 'ffffff', sprite=GRAY)
        c = p.node('Complete', n, w=diameter, h=diameter, active=False)
        p.image(c, 'ffffff', sprite=GREEN)
        a = p.node('Current', n, w=diameter, h=diameter, active=i==0)
        p.image(a, 'ffffff', sprite=ORANGE)
        p.label('StageNumber', n, str(i+1), w=diameter, h=diameter-8, size=34 if diameter>70 else 25)
        complete.append(c['go'])
        current.append(a['go'])
    return root['go'], complete, current


def build_center():
    p = Prefab()
    root = p.node('HarvestRewardsUI', w=0, h=0, stretch=True)
    p.canvas(root)
    p.image(root, '10291dc9', True)
    card = panel(p, root, 'FlowCard', w=960, h=1690)
    p.label('Title', card, 'HARVEST WALLET', y=728, w=760, h=80, size=53)
    _, close, _ = p.button('Close', card, 'X', x=407, y=741, w=88, h=88, sprite=ORANGE)
    p.label('SimulationNotice', card, 'SIMULATED REWARDS - NO CASH VALUE', y=650, w=850, h=50, size=25, color='8e5c36')
    _, back, _ = p.button('Back', card, '< BACK', x=-290, y=-715, w=225, h=86, size=28, sprite=ORANGE)
    _, rules, _ = p.button('Rules', card, 'RULES', y=-715, w=225, h=86, size=28, sprite=ORANGE)
    _, history, _ = p.button('History', card, 'RECORDS', x=290, y=-715, w=225, h=86, size=28, sprite=ORANGE)
    feedback = p.label('Feedback', card, '', y=-795, w=830, h=55, size=24)
    values = dict(closeButton=close, backButton=back, rulesButton=rules, historyButton=history, feedbackText=feedback)
    pages = {}
    for name in ['wallet', 'confirmation', 'progress', 'settlement', 'completed', 'history', 'rules']:
        pages[name] = p.node(name.title() + 'Page', card, w=850, h=1230, active=name=='wallet')
        values[name+'Root'] = pages[name]['go']

    wallet = pages['wallet']
    p.label('AvailableHeading', wallet, 'YOUR HARVEST BALANCE', y=537, size=29)
    icon(p, wallet, x=-290, y=427, w=120, h=130)
    values['availableText'] = p.label('AvailableValue', wallet, '137.61', x=50, y=427, w=560, h=115, size=76)
    values['frozenText'] = p.label('FrozenValue', wallet, 'In challenge: 0.00', y=334, w=760, h=48, size=27)
    p.label('ChooseTier', wallet, '1. CHOOSE YOUR TARGET', y=233, w=780, h=55, size=35)
    buttons, labels, marks = [], [], []
    for i, amount in enumerate(['1,000','1,500','3,000','5,000','8,000','10,000']):
        n, b, t = p.button('Tier'+str(i+1), wallet, amount, x=(i%3-1)*268, y=125-(i//3)*152, w=244, h=122, sprite=ORANGE, size=33)
        mark = p.node('SelectedMarker', n, y=-45, w=180, h=34, active=i==0)
        p.image(mark, 'ffffff', sprite=GREEN)
        p.label('SelectedLabel', mark, 'SELECTED', w=174, h=40, size=22)
        buttons.append(b); labels.append(t); marks.append(mark['go'])
    values['walletHintText'] = p.label('WalletHint', wallet, '862.39 more to reach this target.\nPlay levels and collect fruit rewards.', y=-174, w=780, h=120, size=33)
    _, values['pendingRewardButton'], values['pendingRewardText'] = p.button('PendingReward', wallet, 'COLLECT HARVEST REWARD', y=-285, w=765, h=84, size=29, sprite=ORANGE)
    p.label('WalletJourneyHint', wallet, 'Apply, then complete the six-stage journey.\nSee Rules for tasks and waiting times.', y=-393, w=770, h=90, size=25)
    _, values['applyButton'], values['applyText'] = p.button('Apply', wallet, 'KEEP PLAYING', y=-525, w=785, h=115, size=39)

    confirm = pages['confirmation']
    p.label('ConfirmHeading', confirm, '2. CONFIRM YOUR REQUEST', y=520, w=820, h=90, size=39)
    icon(p, confirm, y=374, w=145, h=145)
    values['confirmationAmountText'] = p.label('ConfirmationAmount', confirm, '1,000.00', y=217, h=100, size=66)
    values['confirmationSummaryText'] = p.label('ConfirmationSummary', confirm, 'This amount will be reserved.\n\nComplete 6 stages, each with a task and 24h wait.\nThen wait 168h. Minimum 13 days.\n\nSimulation only. No cash is paid.', y=-53, w=750, h=390, size=32)
    _, values['confirmButton'], _ = p.button('ConfirmApply', confirm, 'START CHALLENGE', y=-374, w=770, h=118, size=37)
    _, values['cancelButton'], _ = p.button('CancelApply', confirm, 'CHANGE TARGET', y=-526, w=770, h=106, size=33, sprite=ORANGE)

    stage = pages['progress']
    values['stageTitleText'] = p.label('StageTitle', stage, 'STAGE 1 / 6', y=519, w=820, h=90, size=48)
    _, complete, current = track(p, stage, y=369)
    p.label('CurrentTaskHeading', stage, 'YOUR CURRENT TASK', y=233, size=28)
    values['stageTaskText'] = p.label('StageTask', stage, 'Win 10 main levels', y=138, w=780, h=100, size=44)
    values['stageCountText'] = p.label('StageCount', stage, '3 / 10', y=47, size=38)
    values['stageProgressBar'] = progress_bar(p, stage, 'TaskProgress', y=-22)
    p.label('TimerHeading', stage, 'STAGE WAIT', y=-110, size=27)
    values['stageTimerText'] = p.label('StageTimer', stage, '0d 23:45:12', y=-180, h=85, size=47)
    values['stageHintText'] = p.label('StageHint', stage, 'Finish both the task and the timer.\nYour next stage opens automatically.', y=-327, w=770, h=155, size=29)
    _, values['playButton'], _ = p.button('ContinuePlaying', stage, 'CONTINUE PLAYING', y=-530, w=785, h=118, size=37)

    wait = pages['settlement']
    p.label('SettlementHeading', wait, 'ALL 6 STAGES COMPLETE', y=500, w=800, h=100, size=41)
    icon(p, wait, y=294, w=200, h=200)
    values['settlementAmountText'] = p.label('SettlementAmount', wait, '1,000.00', y=90, h=100, size=62)
    p.label('FinalWaitHeading', wait, 'FINAL WAIT REMAINING', y=-55, size=29)
    values['settlementTimerText'] = p.label('SettlementTimer', wait, '6d 23:59:59', y=-159, h=100, size=48)
    p.label('WaitDescription', wait, 'Your progress is saved.\nYou can close this page and keep playing.', y=-339, w=760, h=145, size=30)
    _, values['settlementCloseButton'], _ = p.button('SettlementClose', wait, 'BACK TO GAME', y=-530, w=785, h=118, size=38)

    done = pages['completed']
    p.label('CompletedHeading', done, 'HARVEST COMPLETE!', y=500, w=810, h=100, size=47)
    icon(p, done, y=281, w=240, h=240)
    values['completedAmountText'] = p.label('CompletedAmount', done, '1,000.00', y=54, h=110, size=70)
    values['completedDetailText'] = p.label('CompletedDetail', done, 'Challenge completed and recorded.\nThis was a simulation; no cash was paid.', y=-193, w=760, h=230, size=33)
    _, values['completedContinueButton'], _ = p.button('CompletedContinue', done, 'NEXT HARVEST', y=-530, w=785, h=118, size=38)

    hist = pages['history']
    p.label('HistoryHeading', hist, 'YOUR HARVEST RECORDS', y=520, w=820, h=100, size=39)
    values['historyText'] = p.label('HistoryBody', hist, 'No completed challenges yet.\nYour completed harvests will appear here.', y=-45, w=770, h=970, size=32, left=True)
    rule = pages['rules']
    p.label('RulesHeading', rule, 'THE FULL JOURNEY', y=525, w=820, h=100, size=43)
    values['rulesText'] = p.label('Terms', rule, 'Six stages. Each stage needs its task and timer.\nAll amounts are simulated reward units.', y=-49, w=770, h=1030, size=32, left=True)
    arrays = dict(tierButtons=buttons, tierLabels=labels, tierSelectedMarkers=marks, stageCompleteMarks=complete, stageCurrentMarks=current)
    summary = '{0} will be reserved for this request.\n\nComplete {1} stages with tasks and timers, then wait {2} hours. Minimum {3} days.\n\nOne active request. Each tier once.\nSimulation only; no cash payment.'
    unavailable = 'Rewards are temporarily unavailable. Keep your save data and restart the game.'
    p.component(root, guid('harvest-rewards-ui'), fields(values, arrays, True)+'  refreshIntervalSeconds: 1\n'
        +'  confirmationSummaryFormat: '+json.dumps(summary)+'\n  unavailableMessage: '+json.dumps(unavailable)+'\n')
    write(CENTER, p.render()); metadata(CENTER, 'harvest-rewards-prefab')
    return root['go']


def build_popup():
    p = Prefab(930000000)
    root = p.node('HarvestRewardPopupUI', w=0, h=0, stretch=True)
    p.canvas(root); p.image(root, '10291dbb', True)
    card = panel(p, root, 'RewardCard', w=890, h=1130)
    values = {}
    values['titleText'] = p.label('RewardTitle', card, 'HARVEST BONUS', y=450, w=690, h=100, size=48)
    _, values['closeButton'], _ = p.button('Close', card, 'X', x=366, y=462, w=84, h=84, sprite=ORANGE)
    icon(p, card, y=246, w=235, h=235)
    values['amountText'] = p.label('RewardAmount', card, '+10.00', y=52, h=130, w=780, size=81)
    values['balanceText'] = p.label('RewardBalance', card, 'Balance: 137.61', y=-46, w=770, h=56, size=29)
    values['hintText'] = p.label('RewardHint', card, 'Collect this reward and continue your harvest.', y=-140, w=740, h=108, size=29)
    _, values['claimButton'], values['claimText'] = p.button('ClaimReward', card, 'COLLECT', y=-282, w=740, h=115, size=42)
    _, values['adButton'], values['adText'] = p.button('BonusAd', card, 'WATCH AD FOR BONUS', y=-409, w=740, h=92, size=28, sprite=ORANGE)
    p.label('SimulationNotice', card, 'SIMULATED REWARD - NO CASH VALUE', y=-502, w=760, h=52, size=23)
    p.component(root, guid('harvest-reward-popup-ui'), fields(values, base=True)+'  refreshIntervalSeconds: 0.25\n  fruitTitle: HARVEST BONUS\n  winTitle: LEVEL COMPLETE\n  welcomeTitle: WELCOME GIFT\n'
        +'  welcomeHint: Added to your harvest balance.\n  rewardHint: Collect your reward and keep playing.\n'
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
    hud = p.node('HarvestRewardsHUD', root_transform, y=y, w=900, h=240 if not compact else 188, anchor=anchor)
    if dynamic_offset is not None:
        p.canvas(hud, dynamic_offset)
        p.component(hud, 'c51cf5ae94661753c94ece8e04a3c7ab', f'  offset: {dynamic_offset}\n  addGraphicRaycaster: 0\n  auto: 0\n')
    banner = panel(p, hud, 'BalanceBanner', y=48, w=900, h=116)
    icon(p, banner, x=-367, w=82, h=88)
    balance = p.label('HudBalance', banner, '137.61', x=-124, y=11, w=368, h=65, size=48)
    p.label('HudUnit', banner, 'SIMULATED REWARDS', x=-120, y=-34, w=390, h=30, size=22)
    _, button, _ = p.button('HarvestRewardsEntry', banner, 'WITHDRAW', x=274, w=295, h=94, size=35)
    goal_panel = panel(p, hud, 'GoalBanner', y=-42, w=870, h=66)
    goal = p.label('HudGoal', goal_panel, 'Earn 862.39 more to apply', w=840, h=49, size=26, color='654024')
    bar = progress_bar(p, hud, 'GoalProgress', y=-76, w=860, h=18)
    stage_root, complete, current = track(p, hud, y=-119, width=630, diameter=50)
    # Compact home/win HUD shares the same functional component; stage nodes are
    # kept within its reserved footprint and only shown for an active request.
    values = dict(walletButton=button, balanceText=balance, goalText=goal, goalProgressBar=bar, stageTrackRoot=stage_root)
    p.component(hud, guid('harvest-rewards-hud'), fields(values, dict(completedMarks=complete, currentMarks=current))+'  refreshIntervalSeconds: 1\n')
    pattern = rf'(^--- !u!224 &{root_transform}\n.*?  m_Children:)(.*?)(\n  m_Father:)'
    m = re.search(pattern, original, flags=re.M|re.S)
    if m is None: raise RuntimeError('Missing prefab root '+path)
    children = m.group(2).replace(' []','')
    original = original[:m.start()]+m.group(1)+children+'\n  - '+ref(hud['rt'])+m.group(3)+original[m.end():]
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


def generate():
    catalog('res/local/harvest/harvestrewardsui', build_center(), 'harvest-rewards-prefab')
    catalog('res/local/harvest/harvestrewardpopupui', build_popup(), 'harvest-reward-popup-prefab')
    reserve_gameplay_hud_area()
    add_hud('Assets/res/local/home/Home.prefab',224518489005915545,(.5,0),850,True)
    add_hud('Assets/res/local/coreplay/CorePlayUI.prefab',224429401810989285,(.5,1),-255)
    add_hud('Assets/res/local/coreplaywin/WinUI.prefab',224181586030114204,(.5,1),-245,True,40)
    metadata('Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardsHud.cs', 'harvest-rewards-hud', script=True)
    print('Authored wallet flow, reward popup and three harvest HUDs.')
