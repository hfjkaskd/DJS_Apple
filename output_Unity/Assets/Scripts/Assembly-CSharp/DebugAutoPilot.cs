using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 调试自动驾驶：通过 -autopilot 启动参数激活。
/// 自动执行 首页→开始→通关 流程并定时截图，用于与目标录屏比对。
/// </summary>
public class DebugAutoPilot : MonoBehaviour
{
	private string shotDir;

	private int shotIndex;

	// 对齐目标录屏：赢下第1-6关，第7关开局后停止
	private int levelsToWin = 6;

	private bool usedShuffle;

	private bool usedMagic;

	private bool usedExtra;

	private bool shopVisited;

	public static bool IsEnabled()
	{
		string[] args = Environment.GetCommandLineArgs();
		foreach (string arg in args)
		{
			if (arg == "-autopilot")
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		shotDir = "/tmp/autoshots";
		Directory.CreateDirectory(shotDir);
		foreach (string f in Directory.GetFiles(shotDir))
		{
			File.Delete(f);
		}
		StartCoroutine(ShotLoop());
		StartCoroutine(FlowLoop());
	}

	private IEnumerator ShotLoop()
	{
		while (shotIndex < 400)
		{
			ScreenCapture.CaptureScreenshot(Path.Combine(shotDir, $"shot_{shotIndex:D4}.png"));
			shotIndex++;
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator FlowLoop()
	{
		yield return new WaitUntil(() => GameEntry.Instance != null && GameEntry.Instance.InitFinish);
		Debug.Log("[AutoPilot] 初始化完成");
		yield return new WaitForSeconds(2f);

		DumpUIState();
		ProbeTextRendering();

		// 首启：Loading页“开始”按钮；非首启：首页 Play
		GameLoadingUI loading = FindObjectOfType<GameLoadingUI>();
		if (loading != null && GameLoadingUI.StartMode)
		{
			GameObject startBtn = GetField<GameObject>(loading, "btnStart");
			Debug.Log("[AutoPilot] 点击开始按钮");
			yield return ClickWhenUnlocked(startBtn);
		}
		else
		{
			HomeUI home = FindObjectOfType<HomeUI>();
			if (home != null)
			{
				GameObject playBtn = GetField<GameObject>(home, "m_PlayBtn");
				Debug.Log("[AutoPilot] 点击首页Play");
				yield return ClickWhenUnlocked(playBtn);
			}
		}

		int wonCount = 0;
		float safetyTimer = 0f;
		while (wonCount < levelsToWin && safetyTimer < 400f)
		{
			yield return new WaitForSeconds(0.35f);
			safetyTimer += 0.35f;

			WinUI win = FindObjectOfType<WinUI>();
			if (win != null)
			{
				Debug.Log($"[AutoPilot] 胜利页出现 (第{wonCount + 1}次)");
				yield return new WaitForSeconds(5f);
				// 5胜宝箱：进度满时宝箱可能延迟弹出（开箱动画+奖励飞出），持续等到领取按钮出现
				if (WinUI.boxCompleted)
				{
					GameObject claim = null;
					float boxWait = 0f;
					while (boxWait < 15f)
					{
						LevelBoxReward reward = FindObjectOfType<LevelBoxReward>();
						claim = reward != null ? GetField<GameObject>(reward, "m_ClaimBtn") : null;
						if (claim != null && claim.activeInHierarchy)
						{
							break;
						}
						yield return new WaitForSeconds(0.5f);
						boxWait += 0.5f;
					}
					if (claim != null && claim.activeInHierarchy)
					{
						Debug.Log("[AutoPilot] 点击领取宝箱奖励");
						yield return new WaitForSeconds(1f);
						yield return ClickWhenUnlocked(claim);
						yield return new WaitForSeconds(3f);
					}
					else
					{
						Debug.Log("[AutoPilot] 宝箱领取按钮未出现（超时）");
					}
				}
				List<GameObject> playBtns = GetField<List<GameObject>>(win, "m_PlayBtns");
				GameObject next = null;
				if (playBtns != null)
				{
					next = playBtns.Find(b => b != null && b.activeInHierarchy);
				}
				if (next != null)
				{
					Debug.Log("[AutoPilot] 点击下一关");
					yield return ClickWhenUnlocked(next);
				}
				wonCount++;
				yield return new WaitForSeconds(2f);
				continue;
			}

			LoseUI lose = FindObjectOfType<LoseUI>();
			if (lose != null)
			{
				Debug.Log("[AutoPilot] 失败页出现，点击重开");
				GameObject restart = GetField<GameObject>(lose, "m_RestartBtn");
				yield return ClickWhenUnlocked(restart);
				yield return new WaitForSeconds(2f);
				continue;
			}

			// 新道具解锁弹窗：按录屏节奏停留约2.5s后领取
			NewItemPop pop = FindObjectOfType<NewItemPop>();
			if (pop != null)
			{
				Debug.Log("[AutoPilot] 新道具弹窗出现，领取");
				yield return new WaitForSeconds(2.5f);
				GameObject claimBtn = GetField<GameObject>(pop, "m_ClaimBtn");
				yield return ClickWhenUnlocked(claimBtn);
				yield return new WaitForSeconds(2f);
				continue;
			}

			// 评分弹窗：按录屏在第6关胜利前出现，约2s后关闭
			Module.Setting.UIRate rate = FindObjectOfType<Module.Setting.UIRate>();
			if (rate != null)
			{
				Debug.Log("[AutoPilot] 评分弹窗出现，关闭");
				yield return new WaitForSeconds(2f);
				GameObject closeBtn = GetField<GameObject>(rate, "m_CloseBtn");
				yield return ClickWhenUnlocked(closeBtn);
				yield return new WaitForSeconds(1f);
				continue;
			}

			// 商店已打开：停留约4s后关闭（录屏第5关 103.2–107.5）
			ShopUI shop = FindObjectOfType<ShopUI>();
			if (shop != null)
			{
				yield return new WaitForSeconds(4f);
				Debug.Log("[AutoPilot] 关闭商店");
				GameObject shopClose = GetField<GameObject>(shop, "m_CloseBtn");
				yield return ClickWhenUnlocked(shopClose);
				yield return new WaitForSeconds(1f);
				continue;
			}

			if (EDLHEMMBABM.Instance != null && !MCCIJBJGMCK.IsLock())
			{
				int curLevel = JEFOMCDAPGK.Instance.CurMainLevelIndex;
				CorePlay.CorePlayUI ui = CorePlay.CorePlayUI.Instance;
				// 录屏：第3关解锁后使用一次洗牌；第4关使用一次魔法
				if (curLevel == 3 && !usedShuffle && ui != null)
				{
					usedShuffle = true;
					Debug.Log("[AutoPilot] 使用洗牌");
					CorePlayItemBtn shuffleBtn = ui.GetItemBtn(POJCEPBNNIP.Shuffle);
					if (shuffleBtn != null)
					{
						yield return ClickWhenUnlocked(shuffleBtn.gameObject);
					}
					yield return new WaitForSeconds(2.5f);
					continue;
				}
				if (curLevel == 4 && !usedMagic && ui != null)
				{
					// 魔法需槽内有目标：先点一个再施法
					TryClickBestItem();
					yield return new WaitForSeconds(1f);
					usedMagic = true;
					Debug.Log("[AutoPilot] 使用魔法");
					CorePlayItemBtn magicBtn = ui.GetItemBtn(POJCEPBNNIP.Magic);
					if (magicBtn != null)
					{
						yield return ClickWhenUnlocked(magicBtn.gameObject);
					}
					yield return new WaitForSeconds(3f);
					continue;
				}
				// 录屏：第5关中途打开商店
				if (curLevel == 5 && !shopVisited && ui != null && safetyTimer > 0f)
				{
					shopVisited = true;
					Debug.Log("[AutoPilot] 打开商店");
					CommonCoinBtn coinBtn = GetField<CommonCoinBtn>(ui, "m_CommonCoinBtn");
					if (coinBtn != null)
					{
						yield return ClickWhenUnlocked(coinBtn.gameObject);
					}
					continue;
				}
				// 录屏：第6关约155.6s 使用一次 +1 槽（槽容量 7→8）
				if (curLevel == 6 && !usedExtra && ui != null)
				{
					usedExtra = true;
					Debug.Log("[AutoPilot] 使用+1槽");
					CorePlayItemBtn extraBtn = ui.GetItemBtn(POJCEPBNNIP.Extra);
					if (extraBtn != null)
					{
						yield return ClickWhenUnlocked(extraBtn.gameObject);
					}
					yield return new WaitForSeconds(2f);
					continue;
				}
				TryClickBestItem();
			}
		}
		Debug.Log($"[AutoPilot] 完成，共胜利 {wonCount} 次");
		// 第7关开局静止展示（对齐录屏结尾），多截几张后退出
		yield return new WaitForSeconds(8f);
		Debug.Log("[AutoPilot] DONE");
		Application.Quit();
	}

	private void TryClickBestItem()
	{
		// 引导阶段1：跟随手指点击目标（与录屏一致先点蓝莓）；目标点完则等待三消完成
		if (NewPlayerGuider.Instance != null)
		{
			FieldInfo phaseField = typeof(NewPlayerGuider).GetField("m_Phase", BindingFlags.NonPublic | BindingFlags.Instance);
			int phase = phaseField != null ? (int)phaseField.GetValue(NewPlayerGuider.Instance) : 0;
			if (phase == 1)
			{
				List<CollectItem> guideTargets = GetField<List<CollectItem>>(NewPlayerGuider.Instance, "m_Phase1Targets");
				if (guideTargets != null && guideTargets.Count > 0 && guideTargets[0] != null)
				{
					CollectItem gt = guideTargets[0];
					InputMono ginput = gt.GetComponent<InputMono>();
					if (ginput != null)
					{
						ginput.onDown?.Invoke(gt.gameObject);
						ginput.onUp?.Invoke(gt.gameObject);
						ginput.onClick?.Invoke(gt.gameObject);
					}
				}
				return;
			}
		}
		List<CollectItem> total = GetField<List<CollectItem>>(EDLHEMMBABM.Instance, "TotalItems");
		List<CollectItem> slot = GetField<List<CollectItem>>(EDLHEMMBABM.Instance, "CollectAreaList");
		if (total == null)
		{
			return;
		}
		MethodInfo canInteract = typeof(CollectItem).GetMethod("CanInteract", BindingFlags.NonPublic | BindingFlags.Instance);
		Dictionary<int, List<CollectItem>> clickable = new Dictionary<int, List<CollectItem>>();
		foreach (CollectItem item in total)
		{
			if (item == null || item.Status != LECIONHKEJL.OnField || item.IsMoving)
			{
				continue;
			}
			if (!(bool)canInteract.Invoke(item, null))
			{
				continue;
			}
			if (!clickable.TryGetValue(item.Type, out var list))
			{
				list = new List<CollectItem>();
				clickable[item.Type] = list;
			}
			list.Add(item);
		}
		if (clickable.Count == 0)
		{
			return;
		}
		Dictionary<int, int> slotCount = new Dictionary<int, int>();
		int slotUsed = 0;
		if (slot != null)
		{
			foreach (CollectItem s in slot)
			{
				if (s != null)
				{
					slotUsed++;
					slotCount.TryGetValue(s.Type, out var c);
					slotCount[s.Type] = c + 1;
				}
			}
		}
		// 优先选择“槽内已有+可点击 >= 3”的类型；槽内数量多者优先
		int bestType = -1;
		int bestScore = int.MinValue;
		foreach (var kv in clickable)
		{
			slotCount.TryGetValue(kv.Key, out var inSlot);
			int need = 3 - inSlot;
			if (kv.Value.Count + inSlot < 3)
			{
				continue;
			}
			// 槽位安全检查：完成该组还需要 need 个位置
			if (slotUsed + Mathf.Max(need, 1) > 7 && inSlot == 0)
			{
				continue;
			}
			int score = inSlot * 100 + kv.Value.Count;
			if (score > bestScore)
			{
				bestScore = score;
				bestType = kv.Key;
			}
		}
		if (bestType < 0)
		{
			return;
		}
		CollectItem target = clickable[bestType][0];
		InputMono input = target.GetComponent<InputMono>();
		if (input != null)
		{
			input.onDown?.Invoke(target.gameObject);
			input.onUp?.Invoke(target.gameObject);
			input.onClick?.Invoke(target.gameObject);
		}
	}

	private IEnumerator ClickWhenUnlocked(GameObject go)
	{
		if (go == null)
		{
			yield break;
		}
		float wait = 0f;
		while (MCCIJBJGMCK.IsLock() && wait < 10f)
		{
			yield return new WaitForSeconds(0.2f);
			wait += 0.2f;
		}
		InputMono input = go.GetComponent<InputMono>();
		if (input != null)
		{
			input.onDown?.Invoke(go);
			input.onUp?.Invoke(go);
			input.onClick?.Invoke(go);
		}
	}

	private void DumpUIState()
	{
		Debug.Log($"[AutoPilot] Screen {Screen.width}x{Screen.height}");
		Camera cam = Camera.main;
		if (cam != null)
		{
			Debug.Log($"[AutoPilot] Camera pos={cam.transform.position} ortho={cam.orthographic} size={cam.orthographicSize} cullingMask={cam.cullingMask}");
		}
		foreach (Canvas canvas in FindObjectsOfType<Canvas>())
		{
			RectTransform rt = canvas.transform as RectTransform;
			Debug.Log($"[AutoPilot] Canvas '{canvas.name}' mode={canvas.renderMode} cam={(canvas.worldCamera != null ? canvas.worldCamera.name : "null")} order={canvas.sortingOrder} size={rt.rect.size} scale={canvas.transform.localScale} pos={canvas.transform.position} active={canvas.gameObject.activeInHierarchy} enabled={canvas.enabled}");
		}
		HomeUI home = FindObjectOfType<HomeUI>();
		if (home != null)
		{
			Transform t = home.transform;
			RectTransform rt = t as RectTransform;
			Debug.Log($"[AutoPilot] Home rect={rt.rect.size} lossyScale={t.lossyScale} pos={t.position} anchoredPos={rt.anchoredPosition} active={home.gameObject.activeInHierarchy}");
			var imgs = home.GetComponentsInChildren<UnityEngine.UI.Image>(false);
			Debug.Log($"[AutoPilot] Home visible images={imgs.Length}");
			for (int i = 0; i < Mathf.Min(3, imgs.Length); i++)
			{
				try
				{
					Debug.Log($"[AutoPilot]   img[{i}] {imgs[i].name} sprite={(imgs[i].sprite != null ? imgs[i].sprite.name : "null")} color={imgs[i].color}");
				}
				catch (System.Exception e)
				{
					Debug.Log($"[AutoPilot]   img[{i}] 读取异常: {e.Message}");
				}
			}
		}
		else
		{
			Debug.Log("[AutoPilot] HomeUI 未找到!");
		}
	}

	private void ProbeTextRendering()
	{
		var texts = FindObjectsOfType<TMPro.TMP_Text>();
		Debug.Log($"[AutoPilot] 场景内TMP文本数={texts.Length}");
		for (int i = 0; i < Mathf.Min(4, texts.Length); i++)
		{
			var t = texts[i];
			try
			{
				t.ForceMeshUpdate(true, true);
				Debug.Log($"[AutoPilot]   text[{i}] '{t.name}' str='{t.text}' font={(t.font != null ? t.font.name : "null")} 顶点={t.mesh.vertexCount} cull={t.canvasRenderer.cull} depth={t.canvasRenderer.absoluteDepth} mat={(t.fontMaterial != null ? t.fontMaterial.shader.name : "null")} active={t.isActiveAndEnabled}");
			}
			catch (System.Exception e)
			{
				Debug.Log($"[AutoPilot]   text[{i}] '{t.name}' 异常: {e.Message}");
			}
		}
	}

	private static T GetField<T>(object obj, string name) where T : class
	{
		FieldInfo field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		return field?.GetValue(obj) as T;
	}
}
