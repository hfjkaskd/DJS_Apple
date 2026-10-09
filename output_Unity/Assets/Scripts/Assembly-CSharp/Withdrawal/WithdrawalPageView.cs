using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitWithdrawal
{
    public enum WithdrawalAction
    {
        Close, Cash, Diamonds, SelectTier, Apply, Confirm, Progress, Records,
        Rules, Rewards, Claim, Back, Cancel, CompleteTest, RejectTest, SelectRecord
    }

    [Serializable]
    public sealed class WithdrawalTextBinding
    {
        public string Key;
        public TMP_Text Text;
    }

    [Serializable]
    public sealed class WithdrawalButtonBinding
    {
        public Button Button;
        public WithdrawalAction Action;
        public int Index;
    }

    [Serializable]
    public sealed class WithdrawalObjectBinding
    {
        public string Key;
        public GameObject Object;
    }

    [Serializable]
    public sealed class WithdrawalProgressBinding
    {
        public string Key;
        public Image Fill;
    }

    /// <summary>Bindings authored into the imported prefab; no runtime UI construction.</summary>
    public sealed class WithdrawalPageView : MonoBehaviour
    {
        [SerializeField] public WithdrawalTextBinding[] Texts = Array.Empty<WithdrawalTextBinding>();
        [SerializeField] public WithdrawalButtonBinding[] Buttons = Array.Empty<WithdrawalButtonBinding>();
        [SerializeField] public WithdrawalObjectBinding[] Objects = Array.Empty<WithdrawalObjectBinding>();
        [SerializeField] public WithdrawalProgressBinding[] Progress = Array.Empty<WithdrawalProgressBinding>();
        [SerializeField] public Image ProgressFill;
        [SerializeField] public RectTransform Content;
        [SerializeField] public Vector2 DesignSize = new Vector2(1080, 1920);
        [SerializeField] public Vector2 DesignCenter;
        private Action<WithdrawalAction, int> handler;
        private bool bound;

        public void Bind(Action<WithdrawalAction, int> callback)
        {
            handler = callback;
            if (bound) return;
            bound = true;
            for (int i = 0; i < Buttons.Length; i++)
            {
                WithdrawalButtonBinding binding = Buttons[i];
                if (binding.Button != null)
                    binding.Button.onClick.AddListener(() => handler?.Invoke(binding.Action, binding.Index));
            }
            Fit();
        }

        public void SetText(string key, string value)
        {
            for (int i = 0; i < Texts.Length; i++)
                if (Texts[i].Key == key && Texts[i].Text != null && Texts[i].Text.text != value)
                    Texts[i].Text.text = value;
        }

        public void SetVisible(string key, bool visible)
        {
            for (int i = 0; i < Objects.Length; i++)
                if (Objects[i].Key == key && Objects[i].Object != null && Objects[i].Object.activeSelf != visible)
                    Objects[i].Object.SetActive(visible);
        }

        public void SetButton(WithdrawalAction action, bool enabled, int index = -1)
        {
            for (int i = 0; i < Buttons.Length; i++)
                if (Buttons[i].Action == action && (index < 0 || Buttons[i].Index == index)
                    && Buttons[i].Button != null) Buttons[i].Button.interactable = enabled;
        }

        public void SetProgress(float progress)
        {
            ApplyProgress(ProgressFill, progress);
        }

        public void SetProgress(string key, float progress)
        {
            for (int i = 0; i < Progress.Length; i++)
                if (Progress[i].Key == key && Progress[i].Fill != null)
                    ApplyProgress(Progress[i].Fill, progress);
        }

        private static void ApplyProgress(Image fill, float progress)
        {
            if (fill == null) return;
            float amount = Mathf.Clamp01(progress);
            if (fill.type == Image.Type.Sliced)
            {
                RectTransform rect = fill.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(amount, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                fill.enabled = amount > 0f;
            }
            else fill.fillAmount = amount;
        }

        private void OnRectTransformDimensionsChange() { Fit(); }

        private void Fit()
        {
            if (Content == null || !(transform is RectTransform root)) return;
            float scale = Mathf.Min(root.rect.width / DesignSize.x, root.rect.height / DesignSize.y, 1f);
            if (scale > 0)
            {
                Content.localScale = Vector3.one * scale;
                Content.anchoredPosition = -DesignCenter * scale;
            }
        }
    }
}
