using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FruitApkEarning
{
    // All visual nodes and aliases are saved in the prefab by ApkEarningUIAuthoring.
    public sealed class ApkEarningPageView : MonoBehaviour
    {
        [Serializable] public sealed class TextSlot { public string Key; public TMP_Text Target; }
        [Serializable] public sealed class ButtonSlot { public string Key; public Button Target; }
        [Serializable] public sealed class ObjectSlot { public string Key; public GameObject Target; }
        [Serializable] public sealed class ImageSlot
        {
            public string Key;
            public Image Target;
            public bool ResizeWidthWithProgress;
            public float FullProgressWidth;
            // Saved with the prefab. A positive height enables proportional round caps
            // even when the earned amount is narrower than the normal bar height.
            public float FullProgressHeight;
            public float FullProgressPixelsPerUnit;
        }
        [Serializable] public sealed class InputSlot { public string Key; public TMP_InputField Target; }
        [Serializable] public sealed class ResourceImageSlot { public Image Target; public string Path; }
        public TextSlot[] Texts = Array.Empty<TextSlot>();
        public ButtonSlot[] Buttons = Array.Empty<ButtonSlot>();
        public ObjectSlot[] Objects = Array.Empty<ObjectSlot>();
        public ImageSlot[] Images = Array.Empty<ImageSlot>();
        public InputSlot[] Inputs = Array.Empty<InputSlot>();
        public ResourceImageSlot[] ResourceImages = Array.Empty<ResourceImageSlot>();
        public RectTransform ArtworkRoot;
        public float ReferenceWidth = 1080;
        public float MinimumReferenceHeight = 1800;
        public bool FitWidth;
        public Color EmphasisColor = new Color32(53, 110, 29, 255);
        public bool TintDiamondOptions;
        public Color SelectedOptionColor = new Color32(210, 246, 160, 255);
        public Color NormalOptionColor = Color.white;
        [Header("Verification presentation")]
        [Min(1)] public int VerificationStepCount = 3;
        public string VerificationStepPrefix = "path:main/Step/";
        public string VerificationConfirmKey = "SureBtn";
        [Min(0)] public float VerificationFirstDelay = .3f;
        [Min(0)] public float VerificationRowSpacing = 1.5f;
        [Min(0)] public float VerificationRowDuration = .1f;
        [Min(0)] public float VerificationContentDelay = 1f;
        [Min(0)] public float VerificationContentDuration = .1f;
        [Min(0)] public float VerificationConfirmDuration = .3f;
        [Header("Stage review presentation")]
        [Min(0)] public float StageRowDuration = .1f;
        [Min(0)] public float StageContentDuration = .2f;
        [Min(0)] public float StageFirstDelay = .3f;
        [Min(0)] public float StageFirstSpacing = 2f;
        [Min(0)] public float StageFirstContentDelay = 1.5f;
        [Min(0)] public float StageHintDelay = 2f;
        [Min(0)] public float StageResumeContentDelay = .5f;
        [Min(0)] public float StageThirdRowDelay = .9f;
        [Min(0)] public float StageThirdContentDelay = 1f;
        [Min(0)] public float StageLastRowDelay = 2.5f;
        [Min(0)] public float StageButtonDuration = .3f;
        [Header("Bonus presentation")]
        [Min(1)] public int BonusRowCount = 5;
        [Min(0)] public float BonusRevealInterval = .35f;
        [Min(0)] public float BonusPulseDuration = .5f;
        public AnimationCurve BonusPulseCurve = new AnimationCurve(
            new Keyframe(0, 1), new Keyframe(.35f, 1.035f), new Keyframe(1, 1));
        [Header("Receipt presentation")]
        [Min(1)] public int ReceiptIconCount = 5;
        [Min(0)] public float ReceiptCollectDelay = 1f;
        [Min(0)] public float ReceiptFlightDuration = .65f;
        [Min(0)] public float ReceiptFlightStagger = .075f;
        public AnimationCurve ReceiptFlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private readonly Dictionary<Button, UnityAction> listeners = new Dictionary<Button, UnityAction>();
        private bool fitting;
        private bool resourceImagesLoaded;
        private int presentationVersion;
        private PresentationKind presentationKind;
        private VerificationVisual[] verificationVisuals;
        private VerificationVisual[] stageVisuals;
        private BonusVisual[] bonusVisuals;
        private ReceiptVisual[] receiptVisuals;
        private Button verificationConfirm;
        private Vector3 verificationConfirmScale;
        private bool verificationConfirmInteractable;
        private Button stageConfirm;
        private Vector3 stageConfirmScale;
        private bool stageConfirmInteractable;
        public bool IsPresentationPlaying => presentationKind != PresentationKind.None;
        public bool VerificationReady { get; private set; }

        private enum PresentationKind { None, Verification, Stage, Bonuses, Receipt }
        private struct VerificationVisual
        {
            public Transform Row, Content;
            public GameObject Done, Loading;
            public Vector3 RowScale, ContentScale;
            public bool DoneVisible, LoadingVisible;
        }
        private struct BonusVisual
        {
            public Transform Row;
            public Vector3 Scale;
        }
        private struct ReceiptVisual
        {
            public RectTransform Icon;
            public Vector3 LocalPosition;
        }

        public TMP_Text GetText(string key) { foreach(var s in Texts) if(s.Key==key)return s.Target; return null; }
        public Button GetButton(string key) { foreach(var s in Buttons) if(s.Key==key)return s.Target; return null; }
        public GameObject GetObject(string key) { foreach(var s in Objects) if(s.Key==key)return s.Target; return null; }
        public Image GetImage(string key) { foreach(var s in Images) if(s.Key==key)return s.Target; return null; }
        public TMP_InputField GetInput(string key) { foreach(var s in Inputs) if(s.Key==key)return s.Target; return null; }
        public void SetText(string key,string value) { var t=GetText(key); if(t!=null&&t.text!=value)t.text=value; }
        public void SetVisible(string key,bool visible) { var o=GetObject(key); if(o!=null&&o.activeSelf!=visible)o.SetActive(visible); }
        public void SetProgress(string key,float value)
        {
            foreach(var slot in Images)
            {
                if(slot.Key!=key)continue;
                var target=slot.Target;if(target==null)return;
                float progress=Mathf.Clamp01(value);
                if(slot.ResizeWidthWithProgress)
                {
                    var size=target.rectTransform.sizeDelta;
                    size.x=slot.FullProgressWidth*progress;
                    if(slot.FullProgressHeight>0)
                    {
                        // Do not impose a minimum width: tiny balances must not look
                        // more complete than they are. Scale the cap into a small dot.
                        size.y=progress>0?Mathf.Min(slot.FullProgressHeight,size.x):slot.FullProgressHeight;
                        target.pixelsPerUnitMultiplier=size.y>0
                            ?slot.FullProgressPixelsPerUnit*slot.FullProgressHeight/size.y
                            :slot.FullProgressPixelsPerUnit;
                    }
                    target.rectTransform.sizeDelta=size;
                    target.enabled=size.x>0;
                }
                else target.fillAmount=progress;
                return;
            }
        }
        public void Bind(string key,Action action)
        {
            var button=GetButton(key); if(button==null)return;
            UnityAction previous;
            if(listeners.TryGetValue(button,out previous))button.onClick.RemoveListener(previous);
            UnityAction next=()=>action(); listeners[button]=next; button.onClick.AddListener(next);
        }
        public void Unbind()
        {
            foreach(var item in listeners)if(item.Key!=null)item.Key.onClick.RemoveListener(item.Value);
            listeners.Clear();
        }
        // These coroutines animate already configured nodes only. The owner must save
        // rewards before starting a receipt; this view has no account or award callbacks.
        public IEnumerator PlayVerificationSteps()
        {
            CancelPresentation();
            CacheVerificationVisuals();
            int version = ++presentationVersion;
            presentationKind = PresentationKind.Verification;
            VerificationReady = false;
            verificationConfirmScale = verificationConfirm.transform.localScale;
            verificationConfirmInteractable = verificationConfirm.interactable;
            verificationConfirm.interactable = false;
            verificationConfirm.transform.localScale = Vector3.zero;
            for (int i = 0; i < verificationVisuals.Length; i++)
            {
                var visual = verificationVisuals[i];
                visual.Done.SetActive(false);
                visual.Loading.SetActive(true);
                visual.Row.localScale = WithHeight(visual.RowScale, 0);
                visual.Content.localScale = WithHeight(visual.ContentScale, 0);
            }
            float first = Mathf.Max(0, VerificationFirstDelay);
            float spacing = Mathf.Max(0, VerificationRowSpacing);
            float rowDuration = Mathf.Max(0, VerificationRowDuration);
            float contentDelay = Mathf.Max(0, VerificationContentDelay);
            float contentDuration = Mathf.Max(0, VerificationContentDuration);
            float confirmDuration = Mathf.Max(0, VerificationConfirmDuration);
            float confirmAt = first + (verificationVisuals.Length - 1) * spacing + rowDuration + contentDelay;
            float end = confirmAt + Mathf.Max(contentDuration, confirmDuration);
            float elapsed = 0;
            try
            {
                while (PresentationAlive(version))
                {
                    for (int i = 0; i < verificationVisuals.Length; i++)
                    {
                        var visual = verificationVisuals[i];
                        float rowAge = elapsed - first - i * spacing;
                        float contentAge = rowAge - rowDuration - contentDelay;
                        visual.Row.localScale = WithHeight(visual.RowScale, OutQuad(Progress(rowAge, rowDuration)));
                        visual.Content.localScale = WithHeight(visual.ContentScale, OutQuad(Progress(contentAge, contentDuration)));
                        bool done = contentAge >= 0;
                        if (visual.Done.activeSelf != done) visual.Done.SetActive(done);
                        if (visual.Loading.activeSelf == done) visual.Loading.SetActive(!done);
                    }
                    verificationConfirm.transform.localScale = verificationConfirmScale * OutQuad(Progress(elapsed - confirmAt, confirmDuration));
                    if (elapsed >= end)
                    {
                        VerificationReady = true;
                        verificationConfirm.interactable = verificationConfirmInteractable;
                        presentationKind = PresentationKind.None;
                        yield break;
                    }
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }
            finally
            {
                if (presentationVersion == version && presentationKind == PresentationKind.Verification)
                    CancelPresentation();
            }
        }

        public IEnumerator PlayBonusSequence(float minimumSeconds)
        {
            CancelPresentation();
            CacheBonusVisuals();
            int version = ++presentationVersion;
            presentationKind = PresentationKind.Bonuses;
            float interval = Mathf.Max(0, BonusRevealInterval);
            float duration = Mathf.Max(0, BonusPulseDuration);
            float end = Mathf.Max(Mathf.Max(0, minimumSeconds), (bonusVisuals.Length - 1) * interval + duration);
            float elapsed = 0;
            try
            {
                while (PresentationAlive(version))
                {
                    for (int i = 0; i < bonusVisuals.Length; i++)
                    {
                        float age = elapsed - i * interval;
                        float scale = age >= 0 && age < duration && BonusPulseCurve != null
                            ? BonusPulseCurve.Evaluate(age / duration) : 1;
                        bonusVisuals[i].Row.localScale = bonusVisuals[i].Scale * scale;
                    }
                    if (elapsed >= end) yield break;
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }
            finally
            {
                if (presentationVersion == version) CancelPresentation();
            }
        }

        public IEnumerator PlayStageSteps(bool resume)
        {
            CancelPresentation();
            CacheStageVisuals();
            int version = ++presentationVersion;
            presentationKind = PresentationKind.Stage;
            stageConfirm.interactable = false;
            stageConfirm.transform.localScale = Vector3.zero;
            for (int i = 0; i < stageVisuals.Length; i++)
            {
                var visual = stageVisuals[i];
                visual.Row.localScale = WithHeight(visual.RowScale, resume && i < 2 ? 1 : 0);
                if (visual.Content != null) visual.Content.localScale = WithHeight(visual.ContentScale, resume && i == 0 ? 1 : 0);
                RestoreStageMarkers(visual);
            }
            float rowDuration = Mathf.Max(0, StageRowDuration);
            float contentDuration = Mathf.Max(0, StageContentDuration);
            float first = Mathf.Max(0, StageFirstDelay);
            float second = first + Mathf.Max(0, StageFirstSpacing);
            float firstContentAt = first + rowDuration + Mathf.Max(0, StageFirstContentDelay);
            float secondContentAt = Mathf.Max(0, StageResumeContentDelay);
            float thirdAt = Mathf.Max(0, StageThirdRowDelay);
            float thirdContentAt = thirdAt + rowDuration + Mathf.Max(0, StageThirdContentDelay);
            float lastAt = Mathf.Max(0, StageLastRowDelay);
            float confirmAt = lastAt + rowDuration;
            float buttonDuration = Mathf.Max(0, StageButtonDuration);
            float end = resume
                ? Mathf.Max(confirmAt + buttonDuration, Mathf.Max(secondContentAt, thirdContentAt) + contentDuration)
                : Mathf.Max(second + rowDuration + Mathf.Max(0, StageHintDelay), firstContentAt + contentDuration);
            float elapsed = 0;
            try
            {
                while (PresentationAlive(version))
                {
                    if (resume)
                    {
                        SetStageContent(1, elapsed - secondContentAt, contentDuration);
                        SetStageRow(2, elapsed - thirdAt, rowDuration);
                        SetStageContent(2, elapsed - thirdContentAt, contentDuration);
                        SetStageRow(3, elapsed - lastAt, rowDuration);
                        stageConfirm.transform.localScale = stageConfirmScale * OutQuad(Progress(elapsed - confirmAt, buttonDuration));
                    }
                    else
                    {
                        SetStageRow(0, elapsed - first, rowDuration);
                        SetStageContent(0, elapsed - firstContentAt, contentDuration);
                        SetStageRow(1, elapsed - second, rowDuration);
                    }
                    if (elapsed >= end)
                    {
                        if (resume) stageConfirm.interactable = stageConfirmInteractable;
                        presentationKind = PresentationKind.None;
                        yield break;
                    }
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }
            finally
            {
                if (presentationVersion == version && presentationKind == PresentationKind.Stage)
                    CancelPresentation();
            }
        }

        public IEnumerator PlayReceipt(Transform target, float minimumSeconds)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            CancelPresentation();
            CacheReceiptVisuals();
            int version = ++presentationVersion;
            presentationKind = PresentationKind.Receipt;
            float collectAt = Mathf.Max(0, ReceiptCollectDelay);
            float duration = Mathf.Max(0, ReceiptFlightDuration);
            float stagger = Mathf.Max(0, ReceiptFlightStagger);
            // Never cut off the last icon when a caller configures a shorter hold.
            float end = Mathf.Max(Mathf.Max(0, minimumSeconds), collectAt + (receiptVisuals.Length - 1) * stagger + duration);
            float elapsed = 0;
            try
            {
                while (PresentationAlive(version) && target != null)
                {
                    for (int i = 0; i < receiptVisuals.Length; i++)
                    {
                        var visual = receiptVisuals[i];
                        float age = elapsed - collectAt - i * stagger;
                        bool visible = age >= 0 && age < duration;
                        if (visual.Icon.gameObject.activeSelf != visible) visual.Icon.gameObject.SetActive(visible);
                        if (!visible) continue;
                        float progress = Progress(age, duration);
                        float position = ReceiptFlightCurve == null ? progress : ReceiptFlightCurve.Evaluate(progress);
                        Vector3 origin = visual.Icon.parent.TransformPoint(visual.LocalPosition);
                        visual.Icon.position = Vector3.LerpUnclamped(origin, target.position, position);
                    }
                    if (elapsed >= end) yield break;
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }
            finally
            {
                if (presentationVersion == version) CancelPresentation();
            }
        }

        public void CancelPresentation()
        {
            ++presentationVersion;
            VerificationReady = false;
            if (presentationKind == PresentationKind.Verification)
            {
                if (verificationConfirm != null)
                {
                    verificationConfirm.transform.localScale = verificationConfirmScale;
                    verificationConfirm.interactable = verificationConfirmInteractable;
                }
                if (verificationVisuals != null)
                    foreach (var visual in verificationVisuals)
                    {
                        if (visual.Row != null) visual.Row.localScale = visual.RowScale;
                        if (visual.Content != null) visual.Content.localScale = visual.ContentScale;
                        if (visual.Done != null) visual.Done.SetActive(visual.DoneVisible);
                        if (visual.Loading != null) visual.Loading.SetActive(visual.LoadingVisible);
                    }
            }
            else if (presentationKind == PresentationKind.Stage)
            {
                if (stageConfirm != null)
                {
                    stageConfirm.transform.localScale = stageConfirmScale;
                    stageConfirm.interactable = stageConfirmInteractable;
                }
                if (stageVisuals != null)
                    foreach (var visual in stageVisuals)
                    {
                        if (visual.Row != null) visual.Row.localScale = visual.RowScale;
                        if (visual.Content != null) visual.Content.localScale = visual.ContentScale;
                        RestoreStageMarkers(visual);
                    }
            }
            else if (presentationKind == PresentationKind.Bonuses && bonusVisuals != null)
            {
                foreach (var visual in bonusVisuals)
                    if (visual.Row != null) visual.Row.localScale = visual.Scale;
            }
            else if (presentationKind == PresentationKind.Receipt && receiptVisuals != null)
            {
                foreach (var visual in receiptVisuals)
                    if (visual.Icon != null)
                    {
                        visual.Icon.gameObject.SetActive(false);
                        visual.Icon.localPosition = visual.LocalPosition;
                    }
            }
            presentationKind = PresentationKind.None;
        }

        private void CacheVerificationVisuals()
        {
            if (VerificationStepCount <= 0) throw new InvalidOperationException("Verification rows are not configured.");
            verificationConfirm = GetButton(VerificationConfirmKey);
            if (verificationConfirm == null) throw new InvalidOperationException("Missing verification confirmation Button.");
            if (verificationVisuals != null && verificationVisuals.Length == VerificationStepCount) return;
            var visuals = new VerificationVisual[VerificationStepCount];
            for (int i = 0; i < visuals.Length; i++)
            {
                string prefix = VerificationStepPrefix + (i + 1).ToString(CultureInfo.InvariantCulture);
                var row = RequireObject(prefix);
                var content = RequireObject(prefix + "/Content");
                var done = RequireObject(prefix + "/Done");
                var loading = RequireObject(prefix + "/Loading");
                visuals[i] = new VerificationVisual
                {
                    Row = row.transform, Content = content.transform, Done = done, Loading = loading,
                    RowScale = row.transform.localScale, ContentScale = content.transform.localScale,
                    DoneVisible = done.activeSelf, LoadingVisible = loading.activeSelf
                };
            }
            verificationVisuals = visuals;
        }
        private void CacheStageVisuals()
        {
            if (stageVisuals != null) return;
            var confirm = GetButton(VerificationConfirmKey);
            if (confirm == null) throw new InvalidOperationException("Missing stage confirmation Button.");
            var visuals = new VerificationVisual[4];
            for (int i = 0; i < visuals.Length; i++)
            {
                string prefix = VerificationStepPrefix + (i + 1).ToString(CultureInfo.InvariantCulture);
                var row = RequireObject(prefix);
                var content = i < 3 ? RequireObject(prefix + "/Content") : null;
                var done = i < 2 ? RequireObject(prefix + "/Done") : null;
                var loading = RequireObject(prefix + "/Loading");
                visuals[i] = new VerificationVisual
                {
                    Row = row.transform, Content = content == null ? null : content.transform,
                    Done = done, Loading = loading, RowScale = row.transform.localScale,
                    ContentScale = content == null ? Vector3.one : content.transform.localScale,
                    DoneVisible = done != null && done.activeSelf, LoadingVisible = loading.activeSelf
                };
            }
            stageConfirm = confirm;
            stageConfirmScale = confirm.transform.localScale;
            stageConfirmInteractable = confirm.interactable;
            stageVisuals = visuals;
        }
        private void SetStageRow(int index, float age, float duration)
        {
            var visual = stageVisuals[index];
            visual.Row.localScale = WithHeight(visual.RowScale, OutQuad(Progress(age, duration)));
        }
        private void SetStageContent(int index, float age, float duration)
        {
            var visual = stageVisuals[index];
            visual.Content.localScale = WithHeight(visual.ContentScale, OutQuad(Progress(age, duration)));
        }
        private static void RestoreStageMarkers(VerificationVisual visual)
        {
            // Animation advances presentation, not withdrawal eligibility. A pending
            // requirement keeps the status the host supplied even after its row opens.
            if (visual.Done != null) visual.Done.SetActive(visual.DoneVisible);
            if (visual.Loading != null) visual.Loading.SetActive(visual.LoadingVisible);
        }
        private void CacheBonusVisuals()
        {
            if (BonusRowCount <= 0) throw new InvalidOperationException("Bonus rows are not configured.");
            if (bonusVisuals != null && bonusVisuals.Length == BonusRowCount) return;
            var visuals = new BonusVisual[BonusRowCount];
            for (int i = 0; i < visuals.Length; i++)
            {
                var row = RequireObject("bonusRows[" + i.ToString(CultureInfo.InvariantCulture) + "]").transform;
                visuals[i] = new BonusVisual { Row = row, Scale = row.localScale };
            }
            bonusVisuals = visuals;
        }
        private void CacheReceiptVisuals()
        {
            if (ReceiptIconCount <= 0) throw new InvalidOperationException("Receipt icons are not configured.");
            if (receiptVisuals != null && receiptVisuals.Length == ReceiptIconCount) return;
            var visuals = new ReceiptVisual[ReceiptIconCount];
            for (int i = 0; i < visuals.Length; i++)
            {
                var icon = RequireObject("flightIcons[" + i.ToString(CultureInfo.InvariantCulture) + "]").transform as RectTransform;
                if (icon == null || icon.parent == null) throw new InvalidOperationException("Receipt icons require a configured RectTransform parent.");
                visuals[i] = new ReceiptVisual { Icon = icon, LocalPosition = icon.localPosition };
            }
            receiptVisuals = visuals;
        }
        private GameObject RequireObject(string key)
        {
            var target = GetObject(key);
            if (target == null) throw new InvalidOperationException("Missing earning presentation binding: " + key);
            return target;
        }
        private bool PresentationAlive(int version)
            => this != null && isActiveAndEnabled && presentationVersion == version;
        private static float Progress(float age, float duration)
            => age < 0 ? 0 : duration <= 0 ? 1 : Mathf.Clamp01(age / duration);
        private static float OutQuad(float progress) => progress * (2 - progress);
        private static Vector3 WithHeight(Vector3 scale, float progress)
            => new Vector3(scale.x, scale.y * progress, scale.z);
        private void OnDisable(){CancelPresentation();}
        private void OnDestroy(){CancelPresentation();Unbind();}
        private void OnEnable(){RefreshLayout();}
        private void OnRectTransformDimensionsChange(){RefreshLayout();}
        private void LoadResourceImages()
        {
            if (resourceImagesLoaded) return;
            resourceImagesLoaded = true;
            if (ResourceImages == null || ResourceImages.Length == 0) return;
            // Cache only for this load pass. Duplicate slots share the same Sprite,
            // while the prefab stores paths instead of holding large artwork assets.
            var loaded = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (ResourceImageSlot slot in ResourceImages)
            {
                if (slot == null || slot.Target == null)
                {
                    Debug.LogError(name + ": resource artwork has no Image target.", this);
                    continue;
                }
                Sprite sprite = null;
                if (string.IsNullOrWhiteSpace(slot.Path))
                    Debug.LogError(name + ": resource artwork has an empty path.", this);
                else if (!loaded.TryGetValue(slot.Path, out sprite))
                {
                    sprite = Resources.Load<Sprite>(slot.Path);
                    loaded.Add(slot.Path, sprite);
                    if (sprite == null)
                        Debug.LogError(name + ": missing resource artwork Sprite at " + slot.Path, this);
                }
                slot.Target.overrideSprite = null;
                slot.Target.sprite = sprite;
                if (sprite == null) slot.Target.enabled = false;
            }
        }
        public void RefreshLayout()
        {
            if(fitting)return;
            fitting=true;
            try
            {
                LoadResourceImages();
                if(!FitWidth||ArtworkRoot==null||ArtworkRoot==transform||ReferenceWidth<=0)return;
                var rect=transform as RectTransform;
                if(rect==null||rect.rect.width<=0||rect.rect.height<=0)return;
                float scale=rect.rect.width/ReferenceWidth;
                if(MinimumReferenceHeight>0)scale=Mathf.Min(scale,rect.rect.height/MinimumReferenceHeight);
                if(scale<=0)return;
                ArtworkRoot.sizeDelta=new Vector2(ReferenceWidth,rect.rect.height/scale);
                ArtworkRoot.localScale=Vector3.one*scale;
            }
            finally {fitting=false;}
        }
    }
}
