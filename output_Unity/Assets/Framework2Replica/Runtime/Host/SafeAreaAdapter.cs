using UnityEngine;

[System.Flags]
public enum SafeAreaEdges
{
	None = 0,
	Left = 1,
	Right = 2,
	Top = 4,
	Bottom = 8,
	Horizontal = Left | Right,
	All = Left | Right | Top | Bottom
}

/// <summary>
/// Converts Screen.safeArea pixels into this RectTransform parent's local
/// coordinates and applies only the requested edges. Original prefab offsets
/// are retained, so fixed-height top/bottom bars and stretched panels both work.
/// </summary>
public abstract class SafeAreaAdapter : MonoBehaviour
{
	RectTransform _rectTransform;
	RectTransform _parent;
	Canvas _canvas;
	Rect _lastParentRect;
	Matrix4x4 _lastParentMatrix;
	Vector2 _baseOffsetMin;
	Vector2 _baseOffsetMax;
	Rect _lastSafeArea;
	Vector2Int _lastScreenSize;
	Vector2 _lastAppliedOffsetMin;
	Vector2 _lastAppliedOffsetMax;
	Vector2 _lastAppliedDeltaMin;
	Vector2 _lastAppliedDeltaMax;
	bool _cached;
	bool _applying;
	bool _hasAppliedOffsets;

	protected abstract SafeAreaEdges Edges { get; }
	protected virtual bool PreserveExternalOffsets => false;
	protected virtual Rect GetSafeArea() => SafeAreaUtility.Current;

	protected virtual void Awake()
	{
		CacheBaseline();
	}

	protected virtual void OnEnable()
	{
		CacheBaseline();
		ApplyIfChanged(true);
	}

	protected virtual void Start()
	{
		ApplyIfChanged(true);
	}

	protected virtual void Update()
	{
		ApplyIfChanged(false);
	}

	protected virtual void LateUpdate()
	{
		// Animator evaluates after Update and may write the same RectTransform
		// position. Animated adapters compose that fresh value with the safe
		// inset here, instead of letting either system overwrite the other.
		if (PreserveExternalOffsets) ApplyIfChanged(true, true);
	}

	protected virtual void OnRectTransformDimensionsChange()
	{
		if (isActiveAndEnabled) ApplyIfChanged(false);
	}

	protected virtual void OnTransformParentChanged()
	{
		CacheParent();
		if (isActiveAndEnabled) ApplyIfChanged(true);
	}

	void CacheParent()
	{
		_parent = transform.parent as RectTransform;
		_canvas = GetComponentInParent<Canvas>();
	}

	void CacheBaseline()
	{
		if (_cached) return;
		_rectTransform = transform as RectTransform;
		if (_rectTransform == null) return;
		_baseOffsetMin = _rectTransform.offsetMin;
		_baseOffsetMax = _rectTransform.offsetMax;
		CacheParent();
		_cached = true;
	}

	void ApplyIfChanged(bool force, bool preserveExternalOffsets = false)
	{
		if (_applying) return;
		CacheBaseline();
		if (!_cached || Screen.width <= 0 || Screen.height <= 0) return;

		var safeArea = GetSafeArea();
		var screenSize = new Vector2Int(Screen.width, Screen.height);
		Rect parentRect = _parent != null ? _parent.rect : Rect.zero;
		Matrix4x4 parentMatrix = _parent != null ? _parent.localToWorldMatrix : Matrix4x4.identity;
		if (!force && safeArea == _lastSafeArea && screenSize == _lastScreenSize &&
			parentRect == _lastParentRect && parentMatrix == _lastParentMatrix) return;

		_lastSafeArea = safeArea;
		_lastScreenSize = screenSize;
		_lastParentRect = parentRect;
		_lastParentMatrix = parentMatrix;
		Apply(safeArea, preserveExternalOffsets);
	}

	void Apply(Rect safeArea, bool preserveExternalOffsets)
	{
		if (SafeAreaUtility.TryGetLocalRect(_parent, _canvas, safeArea, out Rect local)) {
			ApplyInsets(
				Mathf.Max(0f, local.xMin - _parent.rect.xMin),
				Mathf.Max(0f, _parent.rect.xMax - local.xMax),
				Mathf.Max(0f, _parent.rect.yMax - local.yMax),
				Mathf.Max(0f, local.yMin - _parent.rect.yMin),
				preserveExternalOffsets);
			return;
		}

		var scale = _canvas != null ? Mathf.Max(0.0001f, _canvas.scaleFactor) : 1f;
		ApplyInsets(
			safeArea.xMin / scale,
			(Screen.width - safeArea.xMax) / scale,
			(Screen.height - safeArea.yMax) / scale,
			safeArea.yMin / scale,
			preserveExternalOffsets);
	}

	void ApplyInsets(
		float left,
		float right,
		float top,
		float bottom,
		bool preserveExternalOffsets)
	{
		_applying = true;
		try {
			var currentMin = _rectTransform.offsetMin;
			var currentMax = _rectTransform.offsetMax;
			var applyLeft = (Edges & SafeAreaEdges.Left) != 0;
			var applyRight = (Edges & SafeAreaEdges.Right) != 0;
			var applyTop = (Edges & SafeAreaEdges.Top) != 0;
			var applyBottom = (Edges & SafeAreaEdges.Bottom) != 0;
			var deltaMin = Vector2.zero;
			var deltaMax = Vector2.zero;

			// A fixed-size edge bar must move as one piece. Changing only one
			// offset shrinks it, which can reduce a 240-unit top bar to almost
			// zero on devices with a tall notch. Stretched containers should
			// continue to inset their controlled edges instead.
			var fixedHorizontal = Mathf.Approximately(
				_rectTransform.anchorMin.x, _rectTransform.anchorMax.x);
			var fixedVertical = Mathf.Approximately(
				_rectTransform.anchorMin.y, _rectTransform.anchorMax.y);

			if (fixedHorizontal && applyLeft != applyRight) {
				var shift = applyLeft ? left : -right;
				deltaMin.x = shift;
				deltaMax.x = shift;
			} else {
				if (applyLeft) deltaMin.x = left;
				if (applyRight) deltaMax.x = -right;
			}

			if (fixedVertical && applyTop != applyBottom) {
				var shift = applyBottom ? bottom : -top;
				deltaMin.y = shift;
				deltaMax.y = shift;
			} else {
				if (applyBottom) deltaMin.y = bottom;
				if (applyTop) deltaMax.y = -top;
			}

			Vector2 min;
			Vector2 max;
			if (preserveExternalOffsets) {
				// If no external system touched a component since our last pass,
				// remove our previous delta first. If Animator did touch it, the
				// current component is already the new raw animated value.
				var rawMin = currentMin;
				var rawMax = currentMax;
				if (_hasAppliedOffsets) {
					if (Mathf.Approximately(currentMin.x, _lastAppliedOffsetMin.x))
						rawMin.x -= _lastAppliedDeltaMin.x;
					if (Mathf.Approximately(currentMin.y, _lastAppliedOffsetMin.y))
						rawMin.y -= _lastAppliedDeltaMin.y;
					if (Mathf.Approximately(currentMax.x, _lastAppliedOffsetMax.x))
						rawMax.x -= _lastAppliedDeltaMax.x;
					if (Mathf.Approximately(currentMax.y, _lastAppliedOffsetMax.y))
						rawMax.y -= _lastAppliedDeltaMax.y;
				}
				min = rawMin + deltaMin;
				max = rawMax + deltaMax;
			} else {
				min = currentMin;
				max = currentMax;
				if (applyLeft || (fixedHorizontal && applyRight))
					min.x = _baseOffsetMin.x + deltaMin.x;
				if (applyRight || (fixedHorizontal && applyLeft))
					max.x = _baseOffsetMax.x + deltaMax.x;
				if (applyBottom || (fixedVertical && applyTop))
					min.y = _baseOffsetMin.y + deltaMin.y;
				if (applyTop || (fixedVertical && applyBottom))
					max.y = _baseOffsetMax.y + deltaMax.y;
			}

			_rectTransform.offsetMin = min;
			_rectTransform.offsetMax = max;
			_lastAppliedOffsetMin = _rectTransform.offsetMin;
			_lastAppliedOffsetMax = _rectTransform.offsetMax;
			_lastAppliedDeltaMin = deltaMin;
			_lastAppliedDeltaMax = deltaMax;
			_hasAppliedOffsets = true;
		} finally {
			_applying = false;
		}
	}
}
