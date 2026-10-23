using System;

namespace WZSDK
{
    // Whitelisted business data only. Account contents and provider payloads never leave this boundary.
    internal static class Framework2BusinessAnalytics
    {
        internal static void Withdraw(string action, DataModel data, string result = null, int destination = -1)
        {
            try
            {
                if (action == "main_withdraw_check" || action == "main_withdraw_next")
                {
                    MahjongProductAnalytics.Track("withdraw_eligibility_change", "tier_id", data != null ? data.Num : 0,
                        "eligible", action == "main_withdraw_next", "reason", result);
                    return;
                }
                if (action != "receiver_validation") return;
                MahjongProductAnalytics.Track("withdraw_account_result", "result", result,
                    "valid", result == "saved", "persisted", result == "saved",
                    "channel", data != null ? data.Index : -1,
                    "reason", result == "saved" ? "" : "invalid_receiver");
            }
            catch { /* Analytics cannot interrupt a withdrawal continuation. */ }
        }

        internal static void TaskPresentation(string location, int stage, WithdrawalTaskPresentation task, bool visible)
        {
            try
            {
                var manager = GameManagerWZ.instance;
                MahjongProductAnalytics.State("f2_task_presentation", location,
                    "location", location, "stage", stage, "task_type", task.Type, "target", task.Target,
                    "progress", task.Current, "complete", task.IsComplete, "valid", task.IsValid,
                    "visible", visible);
            }
            catch { }
        }
    }
}
