using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LotteryPanel : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass39_0
	{
		public LotteryPanel _003C_003E4__this;

		public int counting;
	}

	[CompilerGenerated]
	private sealed class _003COnInit_003Ed__39 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public LotteryPanel _003C_003E4__this;

		private _003C_003Ec__DisplayClass39_0 _003C_003E8__1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003COnInit_003Ed__39(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private const int VisibleCenterOffset = 2;

	public List<Text> textList;

	public List<Text> textList1;

	public List<Text> textList2;

	public List<Text> textList3;

	public float H;

	public AnimationCurve line;

	public Text goldText;

	public Image icon;

	public int randomInt;

	[SerializeField]
	private float digitStaggerDelay = 0.5f;

	[SerializeField]
	private float stepDuration = 0.01f;

	[SerializeField]
	private float baseRollCount = 100f;

	[SerializeField]
	private int settleStepCount = 20;

	public List<int> currentList;

	private readonly List<List<Text>> textLists = new List<List<Text>>();

	private readonly List<int> randomList = new List<int>();

	private readonly List<Tween> activeTweens = new List<Tween>();

	private Coroutine initCoroutine;

	private bool currentRollIsRewardRoll;

	private int currentTargetValue;

	public event Action<LotteryPanel, bool, int> RollCompleted;

	public event Action<LotteryPanel, bool> RollStarted;

	private void OnDisable()
	{
		StopRolling();
	}

	private void OnDestroy()
	{
		StopRolling();
	}

	public void SetRandomInt(int value)
	{
		randomInt = Mathf.Max(0, value);
	}

	public void ShowStaticValue(int value)
	{
		StopRolling();
		if (!TryInitializeTextLists())
		{
			return;
		}

		SetRandomList(Mathf.Max(0, value));
		currentList.Clear();
		for (int digitIndex = 0; digitIndex < textLists.Count; digitIndex++)
		{
			int offset = digitIndex < randomList.Count
				? randomList[digitIndex]
				: 0;
			currentList.Add(offset);
			List<Text> texts = textLists[digitIndex];
			for (int row = 0; row < texts.Count; row++)
			{
				Text text = texts[row];
				if (text == null)
				{
					continue;
				}
				RectTransform rect = text.rectTransform;
				if (rect != null)
				{
					rect.anchoredPosition =
						new Vector2(0f, H * row - H * VisibleCenterOffset);
				}
				text.text = ((offset + row) % 10).ToString();
			}
		}
	}

	public void PlayEnterRoll()
	{
		StartRoll(0, false);
	}

	public void PlayResultRoll()
	{
		StartRoll(randomInt, true);
	}

	private void StartRoll(int targetValue, bool isRewardRoll)
	{
		currentRollIsRewardRoll = isRewardRoll;
		currentTargetValue = Mathf.Max(0, targetValue);
		RollStarted?.Invoke(this, isRewardRoll);
		RefreshRoll(currentTargetValue);
	}

	private void RefreshRoll(int targetValue)
	{
		StopRolling();
		if (!TryInitializeTextLists())
		{
			OnRollComplete();
			return;
		}

		currentList.Clear();
		for (int i = 0; i < textLists.Count; i++)
		{
			currentList.Add(0);
		}
		SetRandomList(targetValue);
		initCoroutine = StartCoroutine(OnInit());
	}

	private bool TryInitializeTextLists()
	{
		textLists.Clear();
		AddTextList(textList);
		AddTextList(textList1);
		AddTextList(textList2);
		AddTextList(textList3);

		if (textList == null || textList.Count == 0 || textList[0] == null)
		{
			return false;
		}
		RectTransform rect = textList[0].rectTransform;
		if (rect == null)
		{
			return false;
		}
		H = rect.sizeDelta.y;
		return true;
	}

	private void StopRolling()
	{
		if (initCoroutine != null)
		{
			StopCoroutine(initCoroutine);
			initCoroutine = null;
		}
		for (int i = 0; i < activeTweens.Count; i++)
		{
			Tween tween = activeTweens[i];
			if (tween != null && tween.IsActive())
			{
				tween.Kill();
			}
		}
		activeTweens.Clear();
	}

	private void AddTextList(List<Text> tlist)
	{
		if (tlist != null && tlist.Count > 0)
		{
			textLists.Add(tlist);
		}
	}

	public void SetRandomList(int num)
	{
		UnityEngine.Debug.Log("Random num:" + num);
		randomList.Clear();
		int remaining = Mathf.Max(0, num);
		do
		{
			randomList.Add((remaining % 10 + 8) % 10);
			bool hasMoreDigits = remaining > 9;
			remaining /= 10;
			if (!hasMoreDigits)
			{
				break;
			}
		}
		while (true);

		while (randomList.Count < textLists.Count)
		{
			randomList.Add(8);
		}
	}

	[IteratorStateMachine(typeof(_003COnInit_003Ed__39))]
	private IEnumerator OnInit()
	{
		yield return null;
		initCoroutine = null;
		activeTweens.Clear();

		int rollingCount = 0;
		int baseSteps = GetBaseSpinSteps();
		int stopIntervalSteps = GetStopIntervalSteps();
		for (int digitIndex = 0; digitIndex < textLists.Count; digitIndex++)
		{
			int capturedIndex = digitIndex;
			List<Text> capturedTexts = textLists[digitIndex];
			int current = digitIndex < currentList.Count
				? currentList[digitIndex]
				: 0;
			int targetOffset = digitIndex < randomList.Count
				? randomList[digitIndex]
				: 8;
			int totalSteps = baseSteps + targetOffset;
			rollingCount++;
			Tween tween = CreateRollTween(
				capturedTexts, current, totalSteps);
			tween.OnComplete(() =>
			{
				int finalDigit = (totalSteps + current) % 10;
				for (int row = 0; row < capturedTexts.Count; row++)
				{
					Text text = capturedTexts[row];
					if (text != null)
					{
						text.text = ((row + finalDigit) % 10).ToString();
					}
				}
				if (capturedIndex < currentList.Count)
				{
					currentList[capturedIndex] = finalDigit;
				}
				rollingCount--;
				if (rollingCount <= 0)
				{
					OnRollComplete();
				}
			});
			activeTweens.Add(tween);
			baseSteps += stopIntervalSteps;
		}
	}

	private Tween CreateRollTween(List<Text> tlist, int current, int totalSteps)
	{
		int settleSteps = Mathf.Clamp(settleStepCount, 0, totalSteps);
		int fastSteps = totalSteps - settleSteps;
		Sequence sequence = DOTween.Sequence();
		if (fastSteps > 0)
		{
			Tween fastTween = DOTween.To(
					() => 0f,
					value => UpdateRollingTexts(tlist, current, value),
					fastSteps,
					stepDuration * fastSteps)
				.SetEase(Ease.Linear);
			sequence.Append(fastTween);
		}
		if (settleSteps > 0)
		{
			Tween settleTween = DOTween.To(
					() => (float)fastSteps,
					value => UpdateRollingTexts(tlist, current, value),
					totalSteps,
					stepDuration * settleSteps)
				.SetEase(Ease.OutCubic);
			sequence.Append(settleTween);
		}
		return sequence;
	}

	private void UpdateRollingTexts(List<Text> tlist, int current, float stepValue)
	{
		if (tlist == null)
		{
			return;
		}
		int currentStep = Mathf.FloorToInt(stepValue);
		float curveValue = line != null
			? line.Evaluate(stepValue)
			: stepValue - currentStep;
		for (int row = 0; row < tlist.Count; row++)
		{
			Text text = tlist[row];
			if (text == null)
			{
				continue;
			}
			RectTransform rect = text.rectTransform;
			if (rect != null)
			{
				rect.anchoredPosition = new Vector2(
					0f,
					-(H * curveValue) - H + H * row);
			}
			text.text = (((currentStep + current) % 10 + row) % 10)
				.ToString();
		}
	}

	private int GetBaseSpinSteps()
	{
		return Mathf.Max(1, (int)(baseRollCount / 10f)) * 10;
	}

	private int GetStopIntervalSteps()
	{
		if (digitStaggerDelay <= 0f || stepDuration <= 0f)
		{
			return 0;
		}
		return Mathf.Max(
			1,
			(int)(digitStaggerDelay / (stepDuration * 10f))) * 10;
	}

	private void OnRollComplete()
	{
		bool wasRewardRoll = currentRollIsRewardRoll;
		int target = currentTargetValue;
		currentRollIsRewardRoll = false;
		currentTargetValue = 0;
		RollCompleted?.Invoke(this, wasRewardRoll, target);
	}
}
