"""Publish untouched reference/live HUD crops and a focused comparison page."""
import html
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / "output_Unity/Docs/CommercializationAB/Framework2Replica"
OUT = BASE / "ReskinHudParity-20261027"


def main():
    source = Path(sys.argv[1]).resolve()
    report = json.loads((source / "capture-manifest.json").read_text(encoding="utf-8-sig"))
    if not report["passed"] or report["suite"] != "focused-hud-four-states" or report["captures"] != 4:
        raise RuntimeError("A passing, complete focused HUD run is required")
    if not report["productionFilesUnchanged"] or not report["productionSelectionUnchanged"]:
        raise RuntimeError("Production save/selection preservation is required")
    captures = OUT / "ActualCaptures"
    captures.mkdir(parents=True, exist_ok=True)
    # Copy-only publication: do not move, unlink, overwrite or alter older capture suites.
    for path in source.rglob("*"):
        if path.is_file():
            target = captures / path.relative_to(source)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(path, target)
    cards = []
    labels = {1: "与确认稿相同的数值", 33: "初始关卡 · 零余额", 34: "广告任务 · 1/3", 47: "长金额 · 2,999.50"}
    for state in report["screenshots"]:
        ident = state["approvalId"]
        name = html.escape(state["file"])
        cards.append(f'<article><h3>{labels[ident]}</h3><img src="ActualCaptures/HudCrops/{name}">'
                     f'<p><a href="ActualCaptures/{name}">1080×1920 完整运行图</a> · '
                     f'<a href="ActualCaptures/ReferenceAspect/{name}">参考画幅完整图</a></p></article>')
    document = '''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>顶部 HUD · 确认稿 / Unity 实际运行</title><style>
*{box-sizing:border-box}body{margin:24px;background:#eee8dd;color:#492d1a;font:15px system-ui,sans-serif}h1{font-size:25px;margin:0 0 10px}h2{font-size:20px}h3{font-size:16px;margin:0 0 8px}p{line-height:1.5}main{max-width:1130px}.pair{display:flex;gap:18px;flex-wrap:wrap}figure{margin:0;width:525px;max-width:100%}figcaption{font-weight:700;margin-bottom:8px}figure img{display:block;width:100%}section,article{background:#fffaf0;border:1px solid #d8c8ac;border-radius:12px;padding:18px;margin:20px 0}.overlay{position:relative;width:525px;max-width:100%;aspect-ratio:525/145;overflow:hidden}.overlay img{position:absolute;inset:0;width:100%;height:100%}.controls{display:flex;gap:12px;align-items:center;flex-wrap:wrap;margin-top:14px}button{padding:7px 14px;border:1px solid #b89b71;border-radius:6px;background:white;color:#492d1a;cursor:pointer}input{width:240px}article img{width:100%;max-width:1050px}a{color:#00678e}small{color:#765e45}summary{cursor:pointer;font-weight:700}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(340px,1fr));gap:15px}.grid article{margin:0}pre{white-space:pre-wrap}
</style><main><h1>顶部 HUD · 确认稿与实际运行</h1><p>等级、纸币、金额栏、Cash、设置、任务进度槽及数字字形均在实际 B Prefab 中替换。下方为真实 Unity 场景的运行渲染。</p>
<section><div class="pair"><figure><figcaption>确认稿原始裁切</figcaption><img src="ActualCaptures/approved-hud.png"></figure><figure><figcaption>本次 Unity 实际运行 · 相同显示尺寸</figcaption><img src="ActualCaptures/HudCrops/01-GamePlayerView.png"></figure></div></section>
<section><h2>叠加检查</h2><div class="overlay"><img src="ActualCaptures/approved-hud.png"><img id="live" style="opacity:.5" src="ActualCaptures/HudCrops/01-GamePlayerView.png"></div><div class="controls"><button data-opacity="0">确认稿</button><button data-opacity="1">实际运行</button><button data-opacity=".5">50% 叠加</button><input id="opacity" type="range" min="0" max="1" step=".01" value=".5"><output id="value">50%</output></div><p><small>两图统一为 525×145 显示尺寸。参考为 board-01 原图裁切；实际图是完整 1050×1662 运行渲染的顶部像素裁切。背景保持实际游戏场景。</small></p></section>
<h2>其他实时数值状态</h2><div class="grid">''' + "".join(cards) + '''</div>
<section><details><summary>验证记录与范围</summary><p>本轮只核对顶部 HUD，四种状态均通过；Cash 标准 Button 可打开原有提现页面。确认稿和运行图均未重绘或替换成整张静态界面。字体使用拆分字形显示实际数据；未出现于确认稿的字形保留完整数字及地区符号支持。</p><p>A 方案、产品数值配置、生产存档及 A/B 选择均保留。完整商业化流程和 A/B 切换的既有检查见上一轮记录。本轮为 Unity 离屏运行渲染，并非手机截图或全画面逐像素相等证明。</p><p><a href="ActualCaptures/capture-manifest.json">本轮验证明细</a> · <a href="../ReskinLayoutParity-20261027/index.html">上一轮全页记录</a> · <a href="ActualCaptures/HUD-comparison.png">上下排列对比图（上：确认稿，下：实际）</a></p></details></section></main>
<script>const slider=document.querySelector('#opacity'),live=document.querySelector('#live'),value=document.querySelector('#value');function setOpacity(v){slider.value=v;live.style.opacity=v;value.textContent=Math.round(v*100)+'%'}slider.oninput=()=>setOpacity(slider.value);document.querySelectorAll('[data-opacity]').forEach(b=>b.onclick=()=>setOpacity(b.dataset.opacity));</script></html>'''
    (OUT / "index.html").write_text(document, encoding="utf-8")
    visible = [box for state in report["screenshots"] for box in state["layout"] if box["visible"]]
    summary = dict(suite=report["suite"], passed=True, states=4, assertions=report["assertions"],
                   visibleBounds=len(visible), maxAuthoredReferenceEdgeError=max(x["maxEdgeError"] for x in visible),
                   productConfigSha256=report["productConfigSha256"], productionFilesUnchanged=True,
                   productionSelectionUnchanged=True, fullImagePixelParity=False, apkBuilt=False,
                   physicalDeviceTested=False, sourceCaptureDirectory=str(source))
    (OUT / "summary.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False))


if __name__ == "__main__":
    main()
