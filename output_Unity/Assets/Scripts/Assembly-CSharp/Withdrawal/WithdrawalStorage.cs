using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FruitWithdrawal
{
    /// <summary>Explicit bounded binary schema. No reflection or Unity serialization is used for player state.</summary>
    public static class WithdrawalStorage
    {
        private const uint Magic = 0x46574431;
        private const int Version = 1;
        private const int MaxBytes = 16 * 1024 * 1024;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        public static WithdrawalState Load(string path)
        {
            bool recovered;
            return Load(path, out recovered);
        }

        public static WithdrawalState Load(string path, out bool recoveredFromBackup)
        {
            recoveredFromBackup = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("A simulation save path is required.");
            bool primary = File.Exists(path), backup = File.Exists(path + ".bak");
            if (!primary && !backup) {
                if (File.Exists(path + ".tmp")) throw new InvalidDataException("An incomplete simulation save needs recovery.");
                return null;
            }
            Exception primaryError = null;
            if (primary) {
                try { return ReadFile(path); }
                catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is InvalidOperationException || ex is ArgumentException) { primaryError = ex; }
            }
            if (backup) {
                try { WithdrawalState restored = ReadFile(path + ".bak"); recoveredFromBackup = true; return restored; }
                catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is InvalidOperationException || ex is ArgumentException) {
                    throw new InvalidDataException("Both simulation save copies are unreadable.", ex);
                }
            }
            throw new InvalidDataException("The simulation save is unreadable and no backup is available.", primaryError);
        }

        public static void Save(string path, WithdrawalState state, bool preserveBackup = false)
        {
            if (string.IsNullOrEmpty(path) || state == null) throw new ArgumentException("A simulation save path and state are required.");
            byte[] payload = Encode(state);
            byte[] digest;
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

        public static WithdrawalState Clone(WithdrawalState state) { return Decode(Encode(state)); }

        private static WithdrawalState ReadFile(string path)
        {
            long length = new FileInfo(path).Length;
            if (length < 44 || length > MaxBytes + 44) throw new InvalidDataException("Invalid simulation save size.");
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(file, Utf8)) {
                if (reader.ReadUInt32() != Magic || reader.ReadInt32() != Version) throw new InvalidDataException("Unsupported simulation save format.");
                int payloadSize = reader.ReadInt32();
                if (payloadSize < 0 || payloadSize > MaxBytes || payloadSize != length - 44) throw new InvalidDataException("Invalid simulation payload size.");
                byte[] expected = ReadExact(reader, 32), payload = ReadExact(reader, payloadSize), actual;
                using (SHA256 sha = SHA256.Create()) actual = sha.ComputeHash(payload);
                int difference = 0;
                for (int i = 0; i < actual.Length; i++) difference |= actual[i] ^ expected[i];
                if (difference != 0) throw new InvalidDataException("The simulation save checksum failed.");
                return Decode(payload);
            }
        }

        private static byte[] Encode(WithdrawalState state)
        {
            state.Validate();
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Utf8)) {
                writer.Write(state.CashCents); writer.Write(state.DiamondUnits); writer.Write(state.FrozenCashCents); writer.Write(state.FrozenDiamondUnits);
                writer.Write(state.TotalWins); writer.Write(state.HighestClearedLevel); writer.Write(state.TodayWins); writer.Write(state.QualifiedCheckInDays);
                writer.Write(state.CurrentUtcDay); writer.Write(state.LastQualifiedUtcDay); writer.Write(state.LastUtcSeconds);
                writer.Write(state.ClockRollbackDetected); writer.Write(state.FirstWinBonusGranted);
                writer.Write(state.NextRewardNumber); writer.Write(state.NextRequestNumber); WriteText(writer, state.ActiveRunId);
                writer.Write(state.Runs.Count);
                for (int i = 0; i < state.Runs.Count; i++) {
                    WithdrawalRun run = state.Runs[i]; WriteText(writer, run.RunId); writer.Write(run.LastTripleSequence); writer.Write(run.Won);
                }
                writer.Write(state.PendingRewards.Count);
                for (int i = 0; i < state.PendingRewards.Count; i++) {
                    WithdrawalReward reward = state.PendingRewards[i];
                    WriteText(writer, reward.RewardId); WriteText(writer, reward.RunId); writer.Write((int)reward.Kind);
                    writer.Write(reward.CashCents); writer.Write(reward.DiamondUnits); writer.Write(reward.CreatedUtcSeconds);
                }
                writer.Write(state.Requests.Count);
                for (int i = 0; i < state.Requests.Count; i++) {
                    WithdrawalRequest request = state.Requests[i];
                    WriteText(writer, request.RequestId); WriteText(writer, request.TierId); WriteText(writer, request.ChannelId); WriteText(writer, request.ChannelLabel);
                    WriteTier(writer, request.Terms); writer.Write((int)request.Status); writer.Write(request.CompletedConditionsMask);
                    writer.Write(request.CreatedUtcSeconds); writer.Write(request.ReviewStartedUtcSeconds); writer.Write(request.ResolvedUtcSeconds);
                }
                writer.Flush();
                if (stream.Length > MaxBytes) throw new InvalidDataException("Simulation history exceeds the supported save size.");
                return stream.ToArray();
            }
        }

        private static WithdrawalState Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream, Utf8)) {
                var state = new WithdrawalState {
                    CashCents = reader.ReadInt64(), DiamondUnits = reader.ReadInt64(), FrozenCashCents = reader.ReadInt64(), FrozenDiamondUnits = reader.ReadInt64(),
                    TotalWins = reader.ReadInt32(), HighestClearedLevel = reader.ReadInt32(), TodayWins = reader.ReadInt32(), QualifiedCheckInDays = reader.ReadInt32(),
                    CurrentUtcDay = reader.ReadInt64(), LastQualifiedUtcDay = reader.ReadInt64(), LastUtcSeconds = reader.ReadInt64(),
                    ClockRollbackDetected = reader.ReadBoolean(), FirstWinBonusGranted = reader.ReadBoolean(),
                    NextRewardNumber = reader.ReadInt64(), NextRequestNumber = reader.ReadInt64(), ActiveRunId = ReadText(reader)
                };
                int count = ReadCount(reader, 100000);
                for (int i = 0; i < count; i++) state.Runs.Add(new WithdrawalRun { RunId = ReadText(reader), LastTripleSequence = reader.ReadInt32(), Won = reader.ReadBoolean() });
                count = ReadCount(reader, 100000);
                for (int i = 0; i < count; i++) state.PendingRewards.Add(new WithdrawalReward { RewardId = ReadText(reader), RunId = ReadText(reader),
                    Kind = (WithdrawalRewardKind)reader.ReadInt32(), CashCents = reader.ReadInt64(), DiamondUnits = reader.ReadInt64(), CreatedUtcSeconds = reader.ReadInt64() });
                count = ReadCount(reader, 10000);
                for (int i = 0; i < count; i++) state.Requests.Add(new WithdrawalRequest { RequestId = ReadText(reader), TierId = ReadText(reader),
                    ChannelId = ReadText(reader), ChannelLabel = ReadText(reader), Terms = ReadTier(reader), Status = (WithdrawalRequestStatus)reader.ReadInt32(),
                    CompletedConditionsMask = reader.ReadInt32(), CreatedUtcSeconds = reader.ReadInt64(),
                    ReviewStartedUtcSeconds = reader.ReadInt64(), ResolvedUtcSeconds = reader.ReadInt64() });
                if (stream.Position != stream.Length) throw new InvalidDataException("Unexpected trailing simulation state.");
                state.Validate();
                return state;
            }
        }

        private static void WriteTier(BinaryWriter writer, WithdrawalTierRule tier)
        {
            WriteText(writer, tier.Id); WriteText(writer, tier.Label); writer.Write((int)tier.Currency);
            writer.Write(tier.DisplayCents); writer.Write(tier.RequiredBalanceUnits); writer.Write(tier.RequiredPassLevels);
            writer.Write(tier.RequiredHiddenPassLevels); writer.Write(tier.RequiredStage2PassLevels); writer.Write(tier.RequiredDailyWins);
            writer.Write(tier.RequiredCheckInDays); writer.Write(tier.RequiredUserLevel); writer.Write(tier.ReviewSeconds); WriteText(writer, tier.EvidenceNote);
        }
        private static WithdrawalTierRule ReadTier(BinaryReader reader)
        {
            return new WithdrawalTierRule { Id = ReadText(reader), Label = ReadText(reader), Currency = (WithdrawalCurrency)reader.ReadInt32(),
                DisplayCents = reader.ReadInt64(), RequiredBalanceUnits = reader.ReadInt64(), RequiredPassLevels = reader.ReadInt32(),
                RequiredHiddenPassLevels = reader.ReadInt32(), RequiredStage2PassLevels = reader.ReadInt32(), RequiredDailyWins = reader.ReadInt32(),
                RequiredCheckInDays = reader.ReadInt32(), RequiredUserLevel = reader.ReadInt32(), ReviewSeconds = reader.ReadInt64(), EvidenceNote = ReadText(reader) };
        }
        private static void WriteText(BinaryWriter writer, string text)
        {
            byte[] bytes = Utf8.GetBytes(text ?? "");
            if (bytes.Length > 4096) throw new InvalidDataException("Simulation text is too long.");
            writer.Write(bytes.Length); writer.Write(bytes);
        }
        private static string ReadText(BinaryReader reader) { return Utf8.GetString(ReadExact(reader, ReadCount(reader, 4096))); }
        private static int ReadCount(BinaryReader reader, int maximum)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maximum) throw new InvalidDataException("Invalid simulation collection length.");
            return count;
        }
        private static byte[] ReadExact(BinaryReader reader, int count)
        {
            byte[] bytes = reader.ReadBytes(count);
            if (bytes.Length != count) throw new EndOfStreamException("Incomplete simulation state.");
            return bytes;
        }
    }
}
