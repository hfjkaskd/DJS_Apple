"""Reference / previous implementation / current live UI; images are never modified."""
import html
import json
from pathlib import Path


def main():
    base = Path(__file__).resolve().parents[1] / "output_Unity/Docs/CommercializationAB/Framework2Replica"
    output = base / "ReskinAlignment-20261026"
    approval = json.loads((base / "ReskinPreview-AllUI-20261023/coverage.json").read_text(encoding="utf-8-sig"))
    report = json.loads((output / "ActualCaptures/capture-manifest.json").read_text(encoding="utf-8-sig"))
    if not report["passed"] or report["captures"] != 45:
        raise RuntimeError("The final live capture run must pass before publishing its comparison.")
    actual = {item["approvalId"]: item for item in report["screenshots"]}
    previous = base / "ReskinImplementation-20261023/ActualCaptures"
    cards = []
    for board in approval["boards"]:
        for column, screen in enumerate(board["screens"]):
            ident = screen["id"]
            three = len(board["screens"]) == 3
            x, y, w, h = ((32 + 541 * column, 58, 524, 831) if three else (14 + 415 * column, 100, 397, 770))
            scale = 270 / w
            reference = f'../ReskinPreview-AllUI-20261023/{board["file"]}'
            figures = f'<figure><figcaption>确认稿</figcaption><div class="crop" style="width:270px;height:{h*scale:.2f}px"><img loading="lazy" src="{reference}" style="width:{1672*scale:.2f}px;left:{-x*scale:.2f}px;top:{-y*scale:.2f}px"></div></figure>'
            item = actual.get(ident)
            if item:
                filename = item["file"]
                if (previous / filename).exists():
                    figures += f'<figure><figcaption>修改前 · Unity 运行图</figcaption><img loading="lazy" src="../ReskinImplementation-20261023/ActualCaptures/{filename}"></figure>'
                figures += f'<figure><figcaption>本次修改后 · Unity 运行图</figcaption><a href="ActualCaptures/{filename}"><img loading="lazy" src="ActualCaptures/{filename}"></a></figure>'
            else:
                figures += '<figure><figcaption>共享组件</figcaption><p>组件随所属页面显示，此编号不是独立页面。</p></figure>'
            cards.append(f'<section id="page-{ident}" data-search="{ident} {html.escape(screen["caption"])} {html.escape(screen["template"])}"><h2>{ident:02d} {html.escape(screen["caption"])}</h2><p>{html.escape(screen["template"])}</p><div class="pair">{figures}</div></section>')
    head = '''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>框架2 · 本次校正对比</title><style>
    *{box-sizing:border-box}body{margin:0;background:#f1ebdf;color:#502919;font:15px system-ui,sans-serif}header{position:sticky;top:0;z-index:3;padding:16px 24px;background:#fff9edf0;border-bottom:1px solid #d9c39d;backdrop-filter:blur(8px)}h1{font-size:23px;margin:0 0 9px}header p{margin:5px 0;font-size:13px}input{width:520px;max-width:100%;padding:9px;border:1px solid #bd9d70;border-radius:7px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(880px,1fr));gap:20px;padding:20px}section{border:1px solid #d9c39d;border-radius:12px;background:#fffaf1;padding:16px}h2{font-size:20px;margin:0}section>p{color:#95704c;font-size:12px}.pair{display:flex;gap:12px;justify-content:center;align-items:flex-start}figure{margin:0;width:270px}figcaption{margin-bottom:8px;font-weight:600;font-size:13px}.crop{position:relative;overflow:hidden}.crop img{position:absolute;max-width:none}figure>a>img,figure>img{width:270px;display:block}a{color:#096d9d}@media(max-width:900px){main{display:block;padding:8px}section{overflow:auto;margin-bottom:14px}.pair{width:834px;justify-content:flex-start}}
    </style><header><h1>框架2 · 确认稿 / 修改前 / 本次修改后</h1><p>三列分别为原确认稿、上一版实际运行图、本次实际运行图。图片保留各自比例；确认稿部分手机框比例不同，未拉伸图片伪造一致性。</p><p>本次将确认稿的可复用控件材质、插画轮廓和逐页布局写入真实 Prefab；金额、文本、输入和 Button 仍由游戏实时驱动。测试存档、关卡、时间与示例值不同。</p><p><a href="ActualCaptures/capture-manifest.json">运行采集报告</a> · <a href="ActualCaptures/index.html">完整运行图</a></p><input id="search" placeholder="输入界面编号、中文名称或模板名"></header><main>'''
    tail = '''</main><script>document.querySelector('#search').addEventListener('input',event=>{const value=event.target.value.toLowerCase();document.querySelectorAll('section').forEach(section=>section.hidden=!section.dataset.search.toLowerCase().includes(value))})</script></html>'''
    (output / "index.html").write_text(head + "\n".join(cards) + tail, encoding="utf-8")
    workflow = json.loads((output / "WorkflowChecks/framework2-play-validation.json").read_text(encoding="utf-8-sig"))
    switches = json.loads((output / "ABSwitchChecks/host-integration.json").read_text(encoding="utf-8-sig"))
    if not workflow["passed"] or not switches["passed"]:
        raise RuntimeError("Workflow and A/B verification must both pass.")
    summary = {
        "actualStates": report["captures"], "authoredPrefabs": 35,
        "captureAssertions": report["assertions"], "workflowAssertions": workflow["assertions"],
        "abAssertions": switches["assertions"], "abSwitchCount": switches["switchCount"],
        "productConfigSha256": report["productConfigSha256"],
        "productionFilesUnchanged": report["productionFilesUnchanged"] and switches["productionFilesUnchanged"],
        "productionSelectionUnchanged": report["productionSelectionUnchanged"] and switches["productionSelectionUnchanged"],
        "pixelPerfectClaim": False, "physicalDeviceTested": False, "apkBuilt": False,
        "changes": [
            "Measured reference rectangles authored into saved prefabs.",
            "Approved frame/button/input artwork reused with authored UI UV patches.",
            "Rounded font, account cards, provider icon, help control, header spacing and footers corrected.",
            "Reward, wheel, wallet, history, settings and host layouts revised.",
            "B's selector now stages a choice and switches only on confirmation; A's original selector is unchanged.",
            "Overlay canvas design resolution aligned with the 1200x2132 business canvas."
        ],
        "remainingReferenceDifferences": [
            "Reference phone frames use different aspect ratios; live captures preserve a real 1080x1920 viewport.",
            "Underlying fruit boards, live balances, names and dates are source-driven rather than reference-image placeholders.",
            "NoticeView preserves the source timed-toast behavior instead of introducing the approval sheet's manual confirmation step.",
            "Lucky-spin reward types retain the source definitions; Undo uses the main game's actual Undo icon."
        ]
    }
    (output / "verification-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print(f"{len(cards)} approved states / {len(actual)} current runtime captures: {output / 'index.html'}")


if __name__ == "__main__":
    main()
