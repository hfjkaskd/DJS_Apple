using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace FruitApkEarning
{
    /// <summary>Local APK flow accounting. A state change is visible only after its separate save commits.</summary>
    public sealed class ApkEarningService
    {
        private static ApkEarningService instance;
        public static ApkEarningService Instance { get { return instance ?? (instance = new ApkEarningService()); } }
        private ApkEarningEngine engine;
        private string savePath;
        public bool Available { get; private set; }
        public bool RecoveredFromBackup { get; private set; }
        public string Error { get; private set; }
        public ApkEarningState State { get { return engine == null ? null : engine.State; } }
        public ApkEarningRules Rules { get { return engine == null ? null : engine.Rules; } }
        public event Action Changed;

        private ApkEarningService() { Error = ""; }

        public bool Init(int currentLevel = 1)
        {
            if (Available) return true;
            try {
                if (currentLevel < 1) throw new ArgumentOutOfRangeException(nameof(currentLevel));
                var config = Resources.Load<ApkEarningConfig>("ApkEarningConfig");
                if (config == null || config.Rules == null) throw new InvalidOperationException("ApkEarningConfig is missing.");
                config.Rules.Validate();
                savePath = Path.Combine(Application.persistentDataPath, "fruit-apk-earning-v1.dat");
                bool recovered;
                ApkEarningState saved = ApkEarningStorage.Load(savePath, config.Rules, out recovered);
                if (saved == null) saved = MigrateLegacy(currentLevel);
                var candidate = new ApkEarningEngine(config.Rules, saved);
                long now = UtcNow();
                candidate.Tick(now);
                // A new session resumes gameplay, not the previous session's
                // reward modal. Preserve its earned money and early records in
                // the same atomic save, before any observer can see this state.
                // Init also retries failed in-session saves. Keep that live
                // reward available for the original Claim action to retry.
                if (engine == null) candidate.ResolveRestoredRewards(now);
                ApkEarningStorage.Save(savePath, candidate.State, recovered);
                engine = candidate;
                RecoveredFromBackup = recovered;
                Available = true; Error = "";
            }
            catch (Exception exception) { SetUnavailable(exception); NotifyChanged(); return false; }
            NotifyChanged();
            return true;
        }

        public bool BeginRun(string id, int initialSequence, int level)
        {
            bool changed = Mutate(candidate => candidate.BeginRun(id, initialSequence, level, UtcNow()));
            // Resuming an already committed checkpoint is a successful begin without a new write.
            return changed || (Available && string.IsNullOrEmpty(Error) && HasRun(id, level));
        }
        public bool RecordMatch(string id, int sequence, int level, bool last = false)
        { return Mutate(candidate => candidate.RecordMatch(id, sequence, level, UtcNow(), last)); }
        public bool RecordWin(string id, int level)
        { return Mutate(candidate => candidate.RecordWin(id, level, UtcNow())); }
        public bool Claim(string id) { return Mutate(candidate => candidate.Claim(id, UtcNow())); }
        // Only the explicitly labelled local video simulator may call this after
        // its completion. A cancelled simulation does not write or notify.
        public bool ClaimWithTestVideo(string id, bool completed)
        { return completed && Mutate(candidate => candidate.ClaimWithTestVideo(id, true, UtcNow())); }
        public bool RefreshSimulatedStats(int minimum, int maximumExclusive)
        {
            if (!Available || minimum < 0 || maximumExclusive <= minimum) return false;
            long now = UtcNow();
            if (State.StatsDisplayUtcDay >= now / 86400) return true;
            return Mutate(candidate => candidate.RefreshSimulatedStats(minimum, maximumExclusive, now));
        }
        public bool ArchiveEarlyReward(int ordinal)
        {
            if (Mutate(candidate => candidate.ArchiveEarlyReward(ordinal, UtcNow()))) return true;
            ApkRecord record = Available ? FindRecord(State, ordinal) : null;
            return record != null && record.Archived;
        }
        public bool SubmitEarlyRecord(int ordinal)
        {
            long utc = UtcNow();
            if (Mutate(candidate => {
                ApkRecord record = FindRecord(candidate.State, ordinal);
                if (record == null) return false;
                if (!record.Archived && !candidate.ArchiveEarlyReward(ordinal, utc)) return false;
                // Account confirmation may submit a newly claimed record directly. Both operations commit together.
                return candidate.SubmitEarlyRecord(ordinal, utc);
            })) return true;
            ApkRecord submitted = Available ? FindRecord(State, ordinal) : null;
            return submitted != null && submitted.Pending;
        }
        public bool SetActivationStep(int completedStep)
        {
            if (Available && completedStep >= 0 && completedStep <= 6 && State.ActivationStep == completedStep) return true;
            return Mutate(candidate => candidate.SetActivationStep(completedStep, UtcNow()));
        }
        public bool ApplyActivationBalance()
        {
            if (Available && State.ActivationBalanceApplied) return true;
            return Mutate(candidate => candidate.ApplyActivationBalance(UtcNow()));
        }
        public bool CompleteActivation()
        {
            if (Available && State.Activated) return true;
            return Mutate(candidate => candidate.CompleteActivation(UtcNow()));
        }
        public bool CompleteWelcome()
        {
            if (Available && State.WelcomeShown) return true;
            return Mutate(candidate => { candidate.State.WelcomeShown = true; return true; });
        }
        public bool CompleteDiamondGuide()
        {
            if (Available && State.DiamondGuideShown) return true;
            return Mutate(candidate => {
                if (candidate.State.CurrentLevel < candidate.Rules.DiamondFirstLevel) return false;
                candidate.State.DiamondGuideShown = true;
                return true;
            });
        }

        public bool Tick()
        {
            if (!Available) return false;
            long now = UtcNow();
            // UI polling must not clone the complete history every frame or second.
            if (!engine.NeedsTick(now)) return false;
            return Mutate(candidate => candidate.Tick(now));
        }

        public bool Flush()
        {
            if (!Available) return false;
            // All accepted mutations have already been flushed and atomically committed.
            return Tick() || Available;
        }

        public ApkRequirements GetRequirements(bool diamonds, int tier)
        {
            if (Available) {
                try { return engine.GetRequirements(diamonds, tier); }
                catch (ArgumentOutOfRangeException) { }
            }
            var unavailable = new ApkRequirements();
            unavailable.Items.Add(new ApkRequirement { Key = "unavailable", Known = false });
            return unavailable;
        }
        public int GetCashStage() { return Available ? engine.GetCashStage() : 0; }

        private bool HasRun(string id, int level)
        {
            for (int i = 0; i < State.Runs.Count; i++)
                if (State.Runs[i].Id == id && State.Runs[i].Level == level) return true;
            return false;
        }
        private static ApkRecord FindRecord(ApkEarningState state, int ordinal)
        {
            for (int i = 0; i < state.Records.Count; i++) if (state.Records[i].Ordinal == ordinal) return state.Records[i];
            return null;
        }

        private static ApkEarningState MigrateLegacy(int currentLevel)
        {
            // Read the old format without initializing its service: that initialization ticks and rewrites its file.
            // Keeping both old save copies untouched permits a safe return to the former flow.
            string legacyPath = Path.Combine(Application.persistentDataPath, "fruit-withdrawal-simulation-v1.dat");
            bool recovered;
            var legacy = FruitWithdrawal.WithdrawalStorage.Load(legacyPath, out recovered);
            return new ApkEarningState {
                CashCents = legacy == null ? 0 : legacy.CashCents,
                Diamonds = legacy == null ? 0 : checked(legacy.DiamondUnits * ApkEarningRules.DiamondScale),
                CurrentLevel = currentLevel,
                HighestClearedLevel = currentLevel - 1
            };
        }

        private bool Mutate(Func<ApkEarningEngine, bool> operation)
        {
            if (!Available) return false;
            try {
                var candidate = new ApkEarningEngine(engine.Rules, ApkEarningStorage.Clone(engine.State));
                if (!operation(candidate)) { Error = ""; return false; }
                ApkEarningStorage.Save(savePath, candidate.State);
                engine = candidate; Error = "";
            }
            catch (Exception exception) { SetUnavailable(exception); NotifyChanged(); return false; }
            NotifyChanged();
            return true;
        }

        private void SetUnavailable(Exception exception)
        {
            Available = false;
            Error = "Local rewards are unavailable; no new reward or request was confirmed. " + exception.Message;
            Debug.LogError(Error);
        }
        private void NotifyChanged()
        {
            Action handler = Changed;
            if (handler == null) return;
            foreach (Action observer in handler.GetInvocationList()) {
                try { observer(); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }
        private static long UtcNow() { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
    }

    /// <summary>Bounded, checksummed schema with no reflection-based player-state serialization.</summary>
    internal static class ApkEarningStorage
    {
        private const uint Magic = 0x46414531;
        private const int Version = 3, MaxBytes = 16 * 1024 * 1024, MaxEntries = 100000;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        internal static ApkEarningState Load(string path, ApkEarningRules rules, out bool recovered)
        {
            recovered = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("An earning save path is required.");
            bool primary = File.Exists(path), backup = File.Exists(path + ".bak");
            if (!primary && !backup) {
                if (File.Exists(path + ".tmp")) throw new InvalidDataException("An incomplete earning save needs recovery.");
                return null;
            }
            Exception primaryError = null;
            if (primary) {
                try { return Read(path, rules); }
                catch (Exception error) when (IsSaveError(error)) { primaryError = error; }
            }
            if (backup) {
                try { var state = Read(path + ".bak", rules); recovered = true; return state; }
                catch (Exception error) when (IsSaveError(error)) { throw new InvalidDataException("Both earning save copies are unreadable.", error); }
            }
            throw new InvalidDataException("The earning save is unreadable and no backup is available.", primaryError);
        }

        internal static void Save(string path, ApkEarningState state, bool preserveBackup = false)
        {
            byte[] payload = Encode(state), digest;
            using (SHA256 sha = SHA256.Create()) digest = sha.ComputeHash(payload);
            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            string temporary = path + ".tmp";
            using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(file, Utf8)) {
                writer.Write(Magic); writer.Write(Version); writer.Write(payload.Length); writer.Write(digest); writer.Write(payload);
                writer.Flush(); file.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temporary, path, preserveBackup ? null : path + ".bak");
            else File.Move(temporary, path);
        }

        internal static ApkEarningState Clone(ApkEarningState state)
        {
            var copy = new ApkEarningState {
                Version = state.Version, CashCents = state.CashCents, Diamonds = state.Diamonds,
                CurrentLevel = state.CurrentLevel, HighestClearedLevel = state.HighestClearedLevel,
                TodayWins = state.TodayWins, LoginDays = state.LoginDays, AccountLevel = state.AccountLevel,
                Activated = state.Activated, ActivationPending = state.ActivationPending,
                WelcomeShown = state.WelcomeShown,
                CashGuidanceComplete = state.CashGuidanceComplete, DiamondGuideShown = state.DiamondGuideShown,
                ActivationStep = state.ActivationStep, LuckyMatches = state.LuckyMatches, CashTargetCents = state.CashTargetCents,
                LastUtcDay = state.LastUtcDay, LastLuckyUtc = state.LastLuckyUtc, NextRewardSequence = state.NextRewardSequence,
                LastLocalDay = state.LastLocalDay, ActivationBalanceApplied = state.ActivationBalanceApplied, RewardVideos = state.RewardVideos,
                StatsDisplayUtcDay = state.StatsDisplayUtcDay, SimulatedDailyAttempts = state.SimulatedDailyAttempts
            };
            foreach (var reward in state.PendingRewards) copy.PendingRewards.Add(new ApkReward {
                Id = reward.Id, Kind = reward.Kind, CashCents = reward.CashCents, Diamonds = reward.Diamonds,
                CreatedUtc = reward.CreatedUtc, Level = reward.Level, EarlyOrdinal = reward.EarlyOrdinal });
            foreach (var record in state.Records) copy.Records.Add(new ApkRecord {
                Ordinal = record.Ordinal, AmountCents = record.AmountCents, CreatedUtc = record.CreatedUtc,
                SubmittedUtc = record.SubmittedUtc, Archived = record.Archived, Pending = record.Pending });
            foreach (var run in state.Runs) copy.Runs.Add(new ApkRunCheckpoint {
                Id = run.Id, Level = run.Level, LastSequence = run.LastSequence, Won = run.Won });
            return copy;
        }

        private static byte[] Encode(ApkEarningState state)
        {
            if (state == null || state.Version != Version || state.PendingRewards.Count > MaxEntries ||
                state.Runs.Count > MaxEntries || state.Records.Count > 2) throw new InvalidDataException("Invalid earning save state.");
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Utf8)) {
                writer.Write(state.Version); writer.Write(state.CashCents); writer.Write(state.Diamonds);
                writer.Write(state.CurrentLevel); writer.Write(state.HighestClearedLevel); writer.Write(state.TodayWins);
                writer.Write(state.LoginDays); writer.Write(state.AccountLevel); writer.Write(state.Activated);
                writer.Write(state.ActivationPending); writer.Write(state.CashGuidanceComplete); writer.Write(state.DiamondGuideShown);
                writer.Write(state.ActivationStep); writer.Write(state.LuckyMatches); writer.Write(state.CashTargetCents);
                writer.Write(state.LastUtcDay); writer.Write(state.LastLuckyUtc); writer.Write(state.NextRewardSequence);
                writer.Write(state.PendingRewards.Count);
                foreach (var reward in state.PendingRewards) {
                    WriteText(writer, reward.Id); WriteText(writer, reward.Kind); writer.Write(reward.CashCents);
                    writer.Write(reward.Diamonds); writer.Write(reward.CreatedUtc); writer.Write(reward.Level); writer.Write(reward.EarlyOrdinal);
                }
                writer.Write(state.Records.Count);
                foreach (var record in state.Records) {
                    writer.Write(record.Ordinal); writer.Write(record.AmountCents); writer.Write(record.CreatedUtc);
                    writer.Write(record.SubmittedUtc); writer.Write(record.Archived); writer.Write(record.Pending);
                }
                writer.Write(state.Runs.Count);
                foreach (var run in state.Runs) {
                    WriteText(writer, run.Id); writer.Write(run.Level); writer.Write(run.LastSequence); writer.Write(run.Won);
                }
                writer.Write(state.WelcomeShown);
                writer.Write(state.LastLocalDay); writer.Write(state.ActivationBalanceApplied); writer.Write(state.RewardVideos);
                writer.Write(state.StatsDisplayUtcDay); writer.Write(state.SimulatedDailyAttempts);
                writer.Flush();
                if (stream.Length > MaxBytes) throw new InvalidDataException("Earning history exceeds the supported save size.");
                return stream.ToArray();
            }
        }

        private static ApkEarningState Read(string path, ApkEarningRules rules)
        {
            long length = new FileInfo(path).Length;
            if (length < 44 || length > MaxBytes + 44) throw new InvalidDataException("Invalid earning save size.");
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(file, Utf8)) {
                if (reader.ReadUInt32() != Magic) throw new InvalidDataException("Unsupported earning save signature.");
                int version = reader.ReadInt32();
                if (version < 1 || version > Version) throw new InvalidDataException("Unsupported earning save version.");
                int payloadLength = ReadCount(reader, MaxBytes);
                if (payloadLength != length - 44) throw new InvalidDataException("Invalid earning payload length.");
                byte[] expected = ReadExact(reader, 32), payload = ReadExact(reader, payloadLength), actual;
                using (SHA256 sha = SHA256.Create()) actual = sha.ComputeHash(payload);
                int difference = 0;
                for (int i = 0; i < actual.Length; i++) difference |= actual[i] ^ expected[i];
                if (difference != 0) throw new InvalidDataException("The earning save checksum failed.");
                ApkEarningState state = Decode(payload, version);
                // A valid checksum alone does not establish a valid domain state.
                new ApkEarningEngine(rules, state);
                return state;
            }
        }

        private static ApkEarningState Decode(byte[] payload, int version)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(stream, Utf8)) {
                var state = new ApkEarningState {
                    Version = reader.ReadInt32(), CashCents = reader.ReadInt64(), Diamonds = reader.ReadInt64(),
                    CurrentLevel = reader.ReadInt32(), HighestClearedLevel = reader.ReadInt32(), TodayWins = reader.ReadInt32(),
                    LoginDays = reader.ReadInt32(), AccountLevel = reader.ReadInt32(), Activated = reader.ReadBoolean(),
                    ActivationPending = reader.ReadBoolean(), CashGuidanceComplete = reader.ReadBoolean(), DiamondGuideShown = reader.ReadBoolean(),
                    ActivationStep = reader.ReadInt32(), LuckyMatches = reader.ReadInt32(), CashTargetCents = reader.ReadInt64(),
                    LastUtcDay = reader.ReadInt64(), LastLuckyUtc = reader.ReadInt64(), NextRewardSequence = reader.ReadInt64()
                };
                if (state.Version != version) throw new InvalidDataException("Unsupported earning state version.");
                int count = ReadCount(reader, MaxEntries);
                for (int i = 0; i < count; i++) state.PendingRewards.Add(new ApkReward {
                    Id = ReadText(reader), Kind = ReadText(reader), CashCents = reader.ReadInt64(), Diamonds = reader.ReadInt64(),
                    CreatedUtc = reader.ReadInt64(), Level = reader.ReadInt32(), EarlyOrdinal = reader.ReadInt32() });
                count = ReadCount(reader, 2);
                for (int i = 0; i < count; i++) state.Records.Add(new ApkRecord {
                    Ordinal = reader.ReadInt32(), AmountCents = reader.ReadInt64(), CreatedUtc = reader.ReadInt64(),
                    SubmittedUtc = reader.ReadInt64(), Archived = reader.ReadBoolean(), Pending = reader.ReadBoolean() });
                count = ReadCount(reader, MaxEntries);
                for (int i = 0; i < count; i++) state.Runs.Add(new ApkRunCheckpoint {
                    Id = ReadText(reader), Level = reader.ReadInt32(), LastSequence = reader.ReadInt32(), Won = reader.ReadBoolean() });
                state.WelcomeShown = version >= 2 ? reader.ReadBoolean() : state.CurrentLevel > 1;
                if (version >= 3) {
                    state.LastLocalDay = reader.ReadInt64(); state.ActivationBalanceApplied = reader.ReadBoolean();
                    state.RewardVideos = reader.ReadInt32();
                    state.StatsDisplayUtcDay = reader.ReadInt64(); state.SimulatedDailyAttempts = reader.ReadInt32();
                }
                else {
                    // Old activation was committed with its balance. Never replay
                    // that grant when upgrading an already activated account.
                    state.ActivationBalanceApplied = state.Activated;
                    if (state.Activated) state.ActivationStep = 6;
                }
                state.Version = Version;
                if (stream.Position != stream.Length) throw new InvalidDataException("Unexpected trailing earning state.");
                return state;
            }
        }

        private static bool IsSaveError(Exception error)
        { return error is IOException || error is InvalidDataException || error is InvalidOperationException || error is ArgumentException || error is OverflowException; }
        private static void WriteText(BinaryWriter writer, string text)
        {
            byte[] bytes = Utf8.GetBytes(text ?? "");
            if (bytes.Length > 512) throw new InvalidDataException("An earning identifier is too long.");
            writer.Write(bytes.Length); writer.Write(bytes);
        }
        private static string ReadText(BinaryReader reader) { return Utf8.GetString(ReadExact(reader, ReadCount(reader, 512))); }
        private static int ReadCount(BinaryReader reader, int maximum)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maximum) throw new InvalidDataException("Invalid earning collection length.");
            return count;
        }
        private static byte[] ReadExact(BinaryReader reader, int count)
        {
            byte[] bytes = reader.ReadBytes(count);
            if (bytes.Length != count) throw new EndOfStreamException("Incomplete earning save.");
            return bytes;
        }
    }
}
