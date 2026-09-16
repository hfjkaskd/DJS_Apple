using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FruitWithdrawal
{
    /// <summary>Same local simulation and data path in Editor and player. No network/payment SDK is connected.</summary>
    public sealed class WithdrawalService
    {
        private static WithdrawalService instance;
        public static WithdrawalService Instance { get { if (instance == null) instance = new WithdrawalService(); return instance; } }
        private WithdrawalEngine engine;
        private string savePath;
        public bool IsAvailable { get; private set; }
        public bool RecoveredFromBackup { get; private set; }
        public string LastError { get; private set; }
        public bool IsSimulation { get { return true; } }
        public WithdrawalRules Rules { get { return engine == null ? null : engine.Rules; } }
        public WithdrawalState State { get { return engine == null ? null : engine.State; } }
        public IReadOnlyList<WithdrawalReward> PendingRewards { get { return engine == null ? (IReadOnlyList<WithdrawalReward>)EmptyRewards : engine.State.PendingRewards; } }
        public WithdrawalRequest ActiveRequest { get { return engine == null ? null : engine.State.ActiveRequest; } }
        public int UserLevel { get { return engine == null ? 0 : engine.UserLevel; } }
        public event Action Changed;
        private static readonly WithdrawalReward[] EmptyRewards = new WithdrawalReward[0];
        private static readonly WithdrawalConditionProgress[] EmptyConditions = new WithdrawalConditionProgress[0];

        private WithdrawalService() { Init(); }

        public bool Init()
        {
            if (IsAvailable) return true;
            try {
                WithdrawalConfig config = Resources.Load<WithdrawalConfig>("WithdrawalConfig");
                if (config == null || config.Rules == null) throw new InvalidOperationException("WithdrawalConfig is missing. The simulation is unavailable.");
                config.Rules.Validate();
                savePath = Path.Combine(Application.persistentDataPath, "fruit-withdrawal-simulation-v1.dat");
                bool recovered;
                WithdrawalState saved = WithdrawalStorage.Load(savePath, out recovered);
                var candidate = new WithdrawalEngine(config.Rules, saved);
                candidate.Tick(UtcNow());
                WithdrawalStorage.Save(savePath, candidate.State, recovered);
                engine = candidate; RecoveredFromBackup = recovered;
                IsAvailable = true; LastError = "";
                return true;
            }
            catch (Exception ex) { SetUnavailable(ex); return false; }
        }

        public string BeginRun(string persistedRunId = null, int initialSequence = 0)
        {
            if (!IsAvailable) return null;
            string runId = string.IsNullOrEmpty(persistedRunId) ? Guid.NewGuid().ToString("N") : persistedRunId;
            bool changed = Mutate(candidate => candidate.BeginRun(runId, UtcNow(), initialSequence));
            return IsAvailable && (changed || string.IsNullOrEmpty(LastError)) ? runId : null;
        }
        public bool RecordTriple(string runId, int sequence, bool tutorial = false)
        {
            return Mutate(candidate => candidate.RecordTriple(runId, sequence, tutorial, UtcNow()));
        }
        public bool RecordWin(string runId, int wonLevel, bool tutorial = false)
        {
            return Mutate(candidate => candidate.RecordWin(runId, wonLevel, tutorial, UtcNow()));
        }
        public bool ClaimReward(string rewardId) { return Mutate(candidate => candidate.ClaimReward(rewardId, UtcNow())); }
        public bool ClaimAllRewards() { return Mutate(candidate => candidate.ClaimAllRewards(UtcNow())); }
        public bool Apply(string tierId, string testChannelId) { return Mutate(candidate => candidate.Apply(tierId, testChannelId, UtcNow())); }
        public bool CancelRequest(string requestId) { return Mutate(candidate => candidate.CancelRequest(requestId, UtcNow())); }
        public bool ResolveSimulation(string requestId, bool approved) { return Mutate(candidate => candidate.ResolveSimulation(requestId, approved, UtcNow())); }

        public bool Tick()
        {
            if (!IsAvailable) return false;
            long utc = UtcNow();
            if (!engine.RequiresTick(utc)) return false;
            return Mutate(candidate => candidate.Tick(utc));
        }

        public bool Flush()
        {
            if (!IsAvailable) return false;
            try { WithdrawalStorage.Save(savePath, engine.State); LastError = ""; return true; }
            catch (Exception ex) { SetUnavailable(ex); NotifyChanged(); return false; }
        }

        public WithdrawalConditionProgress[] GetConditions(string tierId)
        {
            return engine == null ? EmptyConditions : engine.GetConditions(tierId);
        }
        public long GetRemainingReviewSeconds() { return engine == null ? 0 : engine.GetRemainingReviewSeconds(UtcNow()); }

        private bool Mutate(Func<WithdrawalEngine, bool> operation)
        {
            if (!IsAvailable) return false;
            bool changed;
            try {
                var candidate = new WithdrawalEngine(engine.Rules, WithdrawalStorage.Clone(engine.State));
                changed = operation(candidate);
                LastError = candidate.LastError;
                if (!changed) return false;
                WithdrawalStorage.Save(savePath, candidate.State);
                engine = candidate;
            }
            catch (Exception ex) { SetUnavailable(ex); NotifyChanged(); return false; }
            NotifyChanged();
            return true;
        }

        private void SetUnavailable(Exception error)
        {
            IsAvailable = false;
            LastError = "Test simulation is unavailable; no success was confirmed. " + error.Message;
            Debug.LogError(LastError);
        }
        private void NotifyChanged() { Action handler = Changed; if (handler != null) handler(); }
        private static long UtcNow() { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
    }
}
