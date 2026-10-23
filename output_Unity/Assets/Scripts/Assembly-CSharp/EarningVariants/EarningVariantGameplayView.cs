using CorePlay;
using UnityEngine;

namespace FruitEarningVariants
{
    /// <summary>Bindings authored on CorePlayUI; no runtime hierarchy or style construction.</summary>
    public sealed class EarningVariantGameplayView : MonoBehaviour
    {
        public GameObject ATopRoot, ALevelSettingsRow;
        public RectTransform GameplayViewport;
        public CorePlayUI Owner;
        private float lastInset = -1;
        private RectTransform canvasRect;
        private Vector2 lastViewportSize;
        private float nextLayoutCheck;
        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas == null ? null : canvas.rootCanvas.transform as RectTransform;
            if (Owner != null && Owner.m_FieldBackdrop != null) Owner.m_FieldBackdrop.gameObject.SetActive(EarningVariantSelection.IsB);
            if (EarningVariantSelection.IsB)
            {
                ATopRoot.SetActive(false);
                ALevelSettingsRow.SetActive(false);
                if (Owner != null && GameplayViewport != null) Owner.FitGameField(GameplayViewport);
            }
        }
        private void LateUpdate()
        {
            if (!EarningVariantSelection.IsB || Owner == null || GameplayViewport == null || Time.unscaledTime < nextLayoutCheck) return;
            nextLayoutCheck = Time.unscaledTime + .25f;
            RefreshLayout();
        }
        /// <summary>Uses the current canvas projection for normal layout changes and diagnostic captures alike.</summary>
        public void RefreshLayout()
        {
            if (!EarningVariantSelection.IsB || Owner == null || GameplayViewport == null) return;
            var host = EarningVariantController.Instance;
            if (host == null || host.Runtime == null) return;
            if (canvasRect == null) return;
            float inset = host.Runtime.GetGameplayTopInset(canvasRect.rect.height);
            if (Mathf.Abs(inset - lastInset) < 1f && (GameplayViewport.rect.size - lastViewportSize).sqrMagnitude < 1f) return;
            lastInset = inset;
            GameplayViewport.offsetMax = new Vector2(GameplayViewport.offsetMax.x, -inset);
            lastViewportSize = GameplayViewport.rect.size;
            Owner.FitGameField(GameplayViewport);
        }
    }
}
