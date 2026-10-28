"""Publish unmodified approval and live renders with same-size geometry overlays."""
import html
import json
from pathlib import Path

BASE = Path(__file__).resolve().parents[1] / "output_Unity/Docs/CommercializationAB/Framework2Replica"
OUT = BASE / "ReskinLayoutParity-20261027"
CAPTURES = "PublishedCaptures" if (OUT/"PublishedCaptures/capture-manifest.json").exists() else "ActualCaptures"

# Actual viewport bounds on each immutable approval board (x, y, w, h).
PHONES = {
    1:(33,58,525,831),2:(580,58,514,831),3:(1115,58,523,831),
    4:(33,58,525,831),5:(580,58,514,831),6:(1115,58,523,831),
    7:(15,100,397,770),8:(428,100,402,770),9:(845,100,399,770),10:(1260,100,399,770),
    11:(21,78,401,813),12:(440,78,391,813),13:(846,78,388,813),14:(1250,78,401,813),
    15:(11,101,403,777),16:(428,101,402,777),17:(844,101,401,777),18:(1258,101,404,777),
    19:(21,81,397,793),20:(436,81,397,793),21:(845,81,393,793),22:(1254,81,396,793),
    23:(11,99,402,770),24:(428,99,400,770),25:(844,99,402,770),26:(1260,99,402,770),
    27:(13,106,409,763),28:(436,106,397,763),29:(847,106,392,763),30:(1253,106,404,763),
    31:(17,101,399,769),32:(432,101,397,769),33:(849,101,397,769),34:(1263,101,394,769),
    35:(27,146,414,722),36:(454,146,378,722),37:(842,146,378,722),38:(1230,146,415,722),
    39:(12,92,405,782),40:(434,92,397,782),41:(846,92,394,782),42:(1254,92,405,782),
    43:(18,98,418,770),44:(454,98,381,770),45:(852,98,383,770),46:(1251,98,407,770),
}

def rect(value):
    return tuple(value[key] for key in ("x", "y", "width", "height"))

def svg_boxes(boxes, phone, actual=False):
    px,py,pw,ph=phone
    shapes=[]
    for box in boxes:
        if not box["visible"]: continue
        x,y,w,h=rect(box["actual" if actual else "expected"])
        path=html.escape(box["path"])
        shapes.append(f'<rect x="{x-px:.3f}" y="{y-py:.3f}" width="{w:.3f}" height="{h:.3f}"><title>{path}</title></rect>')
    return f'<svg class="bounds" viewBox="0 0 {pw} {ph}" preserveAspectRatio="none">'+''.join(shapes)+'</svg>'

def main():
    approval=json.loads((BASE/"ReskinPreview-AllUI-20261023/coverage.json").read_text(encoding="utf-8-sig"))
    report=json.loads((OUT/CAPTURES/"capture-manifest.json").read_text(encoding="utf-8-sig"))
    if not report["passed"] or report["captures"]!=45: raise RuntimeError("Incomplete live capture run")
    actual={item["approvalId"]:item for item in report["screenshots"]}
    sections=[]
    all_errors=[]
    for board in approval["boards"]:
        for screen in board["screens"]:
            ident=screen["id"];item=actual.get(ident)
            if item is None: continue
            x,y,w,h=PHONES[ident];s=310/w
            reference=f'../ReskinPreview-AllUI-20261023/{board["file"]}'
            crop=f'<div class="phone" style="height:{h*s:.2f}px"><img class="reference" src="{reference}" style="width:{1672*s:.2f}px;left:{-x*s:.2f}px;top:{-y*s:.2f}px"></div>'
            figures=f'<figure><figcaption>确认稿 · 保留原始比例</figcaption>{crop}</figure>'
            before=f'../ReskinAlignment-20261026/ActualCaptures/{item["file"]}'
            figures+=f'<figure><figcaption>上一版 · 原生运行图</figcaption><img src="{before}"></figure>'
            figures+=f'<figure><figcaption>本次 · 原生运行图</figcaption><a href="ActualCaptures/{item["file"]}"><img src="ActualCaptures/{item["file"]}"></a></figure>'
            boxes=item.get("layout",[])
            errors=[b["maxEdgeError"] for b in boxes if b["visible"]]
            all_errors.extend(errors)
            metrics="该页面没有单独的测量目标。"
            if item.get("referenceAspectFile"):
                phone=rect(item["referencePhone"]);pw,ph=phone[2:]
                figures+=f'<figure><figcaption>本次 · 参考画幅真实渲染</figcaption><div class="phone" style="height:{310*ph/pw:.2f}px"><img src="ActualCaptures/{item["referenceAspectFile"]}" width="310">{svg_boxes(boxes,phone)}{svg_boxes(boxes,phone,True)}</div></figure>'
                metrics=f'测量 {len(errors)} 个可见控件；相对已标定边界的最大偏差 {max(errors,default=0):.3f} 参考像素。'
            sections.append(f'<section id="page-{ident}" data-search="{ident} {html.escape(screen["caption"])} {html.escape(screen["template"])}"><h2>{ident:02d} {html.escape(screen["caption"])}</h2><p>{metrics}</p><div class="row">{figures}</div></section>')
    header='''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>框架2 · 布局逐页核对</title><style>
    *{box-sizing:border-box}body{margin:0;background:#eae5dc;color:#472a1c;font:14px system-ui,sans-serif}header{padding:18px 24px;background:#fff9ef;position:sticky;top:0;z-index:10;border-bottom:1px solid #cdbb9f}h1{font-size:22px;margin:0 0 9px}header p{margin:5px 0;max-width:1200px}header input{padding:8px;width:340px}main{padding:20px}section{padding:18px;background:#fffaf3;border:1px solid #d0bfa3;border-radius:12px;margin-bottom:20px;overflow:auto}h2{margin:0;font-size:20px}.row{display:flex;gap:18px;align-items:flex-start;min-width:1294px}figure{width:310px;margin:0;flex-shrink:0}figcaption{font-weight:bold;margin-bottom:8px}figure>img,figure>a>img{display:block;width:310px}.phone{position:relative;width:310px;overflow:hidden}.reference{position:absolute;max-width:none}.bounds{display:none;position:absolute;inset:0;width:100%;height:100%;fill:none;stroke:#00e5ff;stroke-width:1}.bounds+svg{stroke:#f33282;stroke-width:.5}.show-bounds .bounds{display:block}a{color:#006ca0}
    </style><header><h1>框架2 · 面板比例、位置和间距核对</h1><p>确认稿、上一版、本次原生运行图、本次参考画幅运行图。两种画幅均由同一批真实 Prefab 渲染，未把截图拉伸成效果图。</p><p>蓝线为标定边界，红线为运行时测得边界。数值用于确认布局映射没有被脚本或布局组件覆盖；不代表字体、游戏场景和所有美术像素完全相同。不同阶段和地区使用保存在 Prefab 中的布局配置。</p><p><a href="ActualCaptures/capture-manifest.json">运行与测量记录</a> · <a href="reference-layout.json">确认稿边界标定</a></p><input id="search" placeholder="搜索页面编号或名称"> <label><input type="checkbox" id="bounds" style="width:auto">显示控件边界</label></header><main>'''
    tail='''</main><script>document.querySelector('#search').oninput=e=>document.querySelectorAll('section').forEach(s=>s.hidden=!s.dataset.search.toLowerCase().includes(e.target.value.toLowerCase()));document.querySelector('#bounds').onchange=e=>document.body.classList.toggle('show-bounds',e.target.checked);</script></html>'''
    document=(header+'\n'.join(sections)+tail).replace('src="ActualCaptures/','src="'+CAPTURES+'/').replace('href="ActualCaptures/','href="'+CAPTURES+'/')
    if (BASE/"ReskinHudParity-20261027/index.html").exists():
        document=document.replace('<header>','<header><p style="padding:9px;background:#fff1c5;border:1px solid #d8b357"><b>顶部 HUD 已再次更新。</b> 本页保留上一轮全页截图；<a href="../ReskinHudParity-20261027/index.html">查看最新确认稿与运行图对比</a>。</p>',1)
    (OUT/"index.html").write_text(document,encoding="utf-8")
    # A compact, two-column delivery page; only HTML cropping, no altered PNGs.
    item=actual[7];x,y,w,h=PHONES[7];scale=330/w
    quick=f'''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>账户页 · 确认稿与运行图</title><style>
    *{{box-sizing:border-box}}body{{margin:0;background:#f4eee3;color:#512918;font:14px system-ui}}h1{{font-size:20px;margin:16px 20px 6px}}p{{margin:4px 20px 14px}}main{{display:flex;gap:16px;padding:0 20px 20px}}figure{{margin:0;width:330px}}figcaption{{margin-bottom:8px;font-weight:bold}}.crop{{position:relative;overflow:hidden;width:330px;height:{h*scale}px}}.crop img{{position:absolute;width:{1672*scale}px;left:{-x*scale}px;top:{-y*scale}px}}figure>img{{width:330px;display:block}}a{{color:#006c9c}}
    </style><h1>账户页 · 确认稿 / 本次运行图</h1><p>面板、标题、渠道卡片、输入框和按钮按同一尺寸对照。<a href="index.html">查看全部 45 个状态</a></p><main><figure><figcaption>确认稿</figcaption><div class="crop"><img src="../ReskinPreview-AllUI-20261023/board-03.png"></div></figure><figure><figcaption>Unity 实际运行 · 同参考画幅</figcaption><img src="ActualCaptures/{item['referenceAspectFile']}"></figure></main></html>'''
    (OUT/"quick-compare.html").write_text(quick.replace('ActualCaptures/',CAPTURES+'/'),encoding="utf-8")
    summary={"states":len(actual),"visibleBoundsMeasured":len(all_errors),"maxReferencePixelError":max(all_errors,default=0),
             "productConfigSha256":report["productConfigSha256"],"productionFilesUnchanged":report["productionFilesUnchanged"],
             "fullPixelParity":False,"apkBuilt":False,"physicalDeviceTested":False}
    workflow=json.loads((OUT/"WorkflowChecks/framework2-play-validation.json").read_text(encoding="utf-8-sig"))
    ab=json.loads((OUT/"ABSwitchChecks/host-integration.json").read_text(encoding="utf-8-sig"))
    if not workflow["passed"] or not ab["passed"]: raise RuntimeError("Workflow and A/B checks must pass")
    summary.update(workflowAssertions=workflow["assertions"],abAssertions=ab["assertions"],abSwitchCount=ab["switchCount"],
                   productionSelectionUnchanged=report["productionSelectionUnchanged"] and ab["productionSelectionUnchanged"],
                   remainingDifferences=["The source game scene, live accounts, dates, balances and regional localization remain dynamic.",
                    "The source verification states and conditional paths remain unchanged, even where the art board simplifies them.",
                    "The notice retains its original automatic dismissal timer; Confirm and Close optionally dismiss the same notice.",
                    "Reference phones have different aspect ratios. Native captures use uniform scale and real viewport cropping.",
                    "Geometry measurements are against the recorded control rectangles, not an all-pixel image equality test."],
                   changes=["Uniform scaling replaces independent width/height scaling.",
                    "Design-size authoring resolves detached stretch roots and top anchors correctly.",
                    "Stage and regional layouts are serialized in prefabs and selected by existing presentation state.",
                    "Optional exchange, continued-task, notice and tutorial interfaces are included.",
                    "Input error spacing, font fitting, wallet paragraphs and source-following guide placement are corrected."])
    (OUT/"layout-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding="utf-8")
    print(json.dumps(summary))

if __name__=="__main__":main()
