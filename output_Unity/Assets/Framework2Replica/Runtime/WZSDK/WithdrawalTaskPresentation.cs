using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
    // Read-only presentation of the same requirements used by GameManagerWZ.
    internal struct WithdrawalTaskPresentation
    {
        public int Type;
        public int Target;
        public double Current;
        public bool IsValid => Type >= 1 && Type <= 4 && Target > 0;
        public bool IsComplete => IsValid && Current >= Target;
        public float Fill => IsValid ? Mathf.Clamp01((float)(Current / Target)) : 0f;

        public static WithdrawalTaskPresentation Read(GameManagerWZ manager, int stage)
        {
            var result = new WithdrawalTaskPresentation();
            if (manager == null || !manager.TryGetStageRequirement(stage, out result.Type, out result.Target))
                return result;
            switch (result.Type)
            {
                case 1: result.Current = Math.Max(0, manager.currentLv - 1); break;
                case 2: result.Current = Math.Max(0d, manager.GetCoin()); break;
                case 3: result.Current = Math.Max(0, manager.stage5CurrentCycle); break;
                case 4:
                    result.Current = manager.currentStageAdWatchOwnerStage == stage
                        ? Math.Max(0, manager.currentStageAdWatchCount) : 0;
                    break;
            }
            // A completed stage can remain visible during its withdrawal flow.
            if (stage < manager.currentStage)
                result.Current = Math.Max(result.Current, result.Target);
            return result;
        }

        public string Objective(GameManagerWZ manager)
        {
            if (!IsValid) return string.Empty;
            string key;
            switch (Type)
            {
                case 1: key = "3101"; break;
                case 2: key = "3102"; break;
                case 3: key = "3103"; break;
                case 4: key = "3104"; break;
                default: return string.Empty;
            }
            string target = FormatValue(manager, Target);
            return string.Format(CultureInfo.CurrentCulture, Text(key), target);
        }

        public string Progress(GameManagerWZ manager)
        {
            if (!IsValid) return string.Empty;
            return FormatValue(manager, Math.Min(Current, Target)) + "/" + FormatValue(manager, Target);
        }

        private string FormatValue(GameManagerWZ manager, double value)
        {
            return Type == 2 && manager != null
                ? manager.FormatRegionalValue(value)
                : value.ToString("0", CultureInfo.CurrentCulture);
        }

        public static string Text(string key)
        {
            LocalizationManager localization = LocalizationManager.Instance;
            if (localization == null || !localization.HasText(key)) return string.Empty;
            return localization.GetText(key) ?? string.Empty;
        }

        public static void OwnDynamicText(Text text)
        {
            if (text == null) return;
            LocalizedText binding = text.GetComponent<LocalizedText>();
            if (binding != null) binding.SetTextId(string.Empty);
        }

        public static bool ShouldResumeCompletedLevel(GameManagerWZ manager, DataModel data)
        {
            return manager != null && data != null && !data.CloseWithoutAdvance &&
                !data.RetryWithdraw && data.Special == DataModel.SpecialNone &&
                manager.currentState == GameManagerWZ.GAME_STATE.FINISH &&
                (data.CloseType == 1 || manager.HasPendingHostStageTutorial);
        }
    }
}
