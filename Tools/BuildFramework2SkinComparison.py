"""Build a local reference/actual gallery without modifying any image pixels."""
import html
import json
from pathlib import Path


def main():
    project = Path(__file__).resolve().parents[1] / "output_Unity"
    root = project / "Docs/CommercializationAB/Framework2Replica"
    previews = root / "ReskinPreview-AllUI-20261023"
    output = root / "ReskinImplementation-20261023"
    approval = json.loads((previews / "coverage.json").read_text(encoding="utf-8-sig"))
    report = json.loads((output / "ActualCaptures/capture-manifest.json").read_text(encoding="utf-8-sig"))
    by_id = {item["approvalId"]: item for item in report["screenshots"]}
    cards = []
    for board in approval["boards"]:
        for col, screen in enumerate(board["screens"]):
            ident = screen["id"]
            actual = by_id.get(ident)
            three = len(board["screens"]) == 3
            x, y, w, h = ((32 + 541 * col, 58, 524, 831) if three else (14 + 415 * col, 100, 397, 770))
            scale = 300 / w
            crop_style = f"width:300px;height:{h*scale:.2f}px"
            img_style = f"width:{1672*scale:.2f}px;left:{-x*scale:.2f}px;top:{-y*scale:.2f}px"
            reference = f"../ReskinPreview-AllUI-20261023/{board['file']}"
            pair = f'<figure><figcaption>已确认效果图</figcaption><div class="crop" style="{crop_style}"><img loading="lazy" src="{reference}" style="{img_style}"></div></figure>'
            if actual:
                pair += f'<figure><figcaption>Unity 实际运行界面</figcaption><a href="ActualCaptures/{actual["file"]}"><img class="actual" loading="lazy" src="ActualCaptures/{actual["file"]}"></a></figure>'
            else:
                pair += '<figure><figcaption>共享组件总览</figcaption><p>46 为组件板，组件随所属页面接入；不计为独立运行页面。</p></figure>'
            cards.append(f'<section data-search="{ident} {html.escape(screen["caption"])} {html.escape(screen["template"])}"><h2>{ident:02d} {html.escape(screen["caption"])}</h2><p>{html.escape(screen["template"])} · {html.escape(screen["state"])}</p><div class="pair">{pair}</div></section>')
    page = '''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>框架2 · 确认稿 / Unity 实际界面对比</title><style>
    *{box-sizing:border-box}body{margin:0;background:#f5f0e7;color:#57301f;font:16px system-ui,sans-serif}header{position:sticky;top:0;z-index:3;background:#fff8edee;padding:16px 24px;border-bottom:1px solid #e0cba9;backdrop-filter:blur(10px)}h1{font-size:24px;margin:0 0 8px}header p{margin:5px 0;font-size:14px}input{width:min(100%,600px);padding:10px;border-radius:8px;border:1px solid #b79b75}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(640px,1fr));gap:24px;padding:24px}section{background:#fffaf2;border:1px solid #dec5a0;border-radius:16px;padding:16px;overflow:hidden}h2{font-size:20px;margin:0}section>p{font-size:13px;color:#835d43}.pair{display:flex;gap:16px;align-items:flex-start;justify-content:center}figure{margin:0;width:300px}figcaption{font-weight:600;margin-bottom:8px}.crop{position:relative;overflow:hidden;background:#ddd}.crop img{position:absolute;max-width:none}.actual{width:300px;display:block}a{color:#096cae}@media(max-width:680px){main{display:block;padding:8px}section{margin-bottom:16px;padding:8px}.pair{gap:6px;transform-origin:top left}figure{max-width:48vw}.crop{max-width:48vw}.actual{max-width:48vw}}
    </style><header><h1>框架2 · 确认稿与实际界面逐页对比</h1><p>45 个运行状态，24 个业务模板、3 个保留模板、4 个宿主面板及地区/阶段变体。右图为真实 Unity 运行实例的 1080×1920 离屏采集。</p><p>测试状态由隔离 QA 存档配置；效果图中的底图、头像、示例金额和日期不代表同一次会话。保留原玩法、业务数值和本地模拟流程。此页不宣称像素完全一致或手机验证完成。</p><p><a href="ActualCaptures/index.html">原始采集清单</a> · <a href="ActualCaptures/capture-manifest.json">验证报告</a> · <a href="art-generation.json">素材与提示集</a></p><input id="search" placeholder="筛选编号、中文名称或模板名"></header><main>'''
    page += "\n".join(cards)
    page += '''</main><script>document.querySelector('#search').addEventListener('input',e=>{let q=e.target.value.toLowerCase();document.querySelectorAll('section').forEach(s=>s.hidden=!s.dataset.search.toLowerCase().includes(q))})</script></html>'''
    (output / "index.html").write_text(page, encoding="utf-8")
    print(f"Comparison gallery: {len(cards)} references / {len(by_id)} actual captures -> {output / 'index.html'}")


if __name__ == "__main__":
    main()
