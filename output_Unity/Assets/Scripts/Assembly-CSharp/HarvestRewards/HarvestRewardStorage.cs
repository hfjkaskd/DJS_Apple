using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>Explicit binary schema: no reflection, enum names or serializer field-name dependencies.</summary>
public static class HarvestRewardStorage
{
    private const int Magic = 0x48525631;
    private const int MaximumBytes = 1048576;

    public static void Save(string path, HarvestRewardState state)
    {
        byte[] payload;
        using (MemoryStream memory = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memory)) WriteState(writer, state);
            payload = memory.ToArray();
        }
        if (payload.Length > MaximumBytes) throw new InvalidDataException("Harvest save exceeds its limit.");
        byte[] digest;
        using (SHA256 sha = SHA256.Create()) digest = sha.ComputeHash(payload);
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        string temporary = path + ".tmp";
        using (FileStream file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        using (BinaryWriter writer = new BinaryWriter(file))
        {
            writer.Write(Magic);
            writer.Write(payload.Length);
            writer.Write(digest);
            writer.Write(payload);
            writer.Flush();
            file.Flush(true);
        }
        if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
        else File.Move(temporary, path);
    }

    public static HarvestRewardState Load(string path)
    {
        if (!File.Exists(path))
        {
            // An existing journal/backup means this is an interrupted save, not a new account.
            if (File.Exists(path + ".bak") || File.Exists(path + ".tmp")) throw new InvalidDataException("Harvest primary save missing; recovery is required.");
            return new HarvestRewardState();
        }
        try
        {
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(file))
            {
                if (file.Length < 40 || file.Length > MaximumBytes + 40 || reader.ReadInt32() != Magic) throw new InvalidDataException("Invalid harvest save header.");
                int length = reader.ReadInt32();
                if (length < 1 || length > MaximumBytes || file.Length != length + 40L) throw new InvalidDataException("Invalid harvest save length.");
                byte[] expected = reader.ReadBytes(32);
                byte[] payload = reader.ReadBytes(length);
                byte[] actual;
                using (SHA256 sha = SHA256.Create()) actual = sha.ComputeHash(payload);
                int different = 0;
                for (int i = 0; i < actual.Length; i++) different |= actual[i] ^ expected[i];
                if (different != 0) throw new InvalidDataException("Harvest save checksum mismatch.");
                using (MemoryStream memory = new MemoryStream(payload, false))
                using (BinaryReader values = new BinaryReader(memory))
                {
                    HarvestRewardState result = ReadState(values);
                    if (memory.Position != memory.Length) throw new InvalidDataException("Unexpected trailing harvest data.");
                    return result;
                }
            }
        }
        catch (EndOfStreamException exception) { throw new InvalidDataException("Truncated harvest save.", exception); }
    }

    public static void Validate(HarvestRewardState state, HarvestRewardSettings settings)
    {
        if (state == null || state.Version != HarvestRewardState.CurrentVersion) throw new InvalidDataException("Unsupported harvest save version.");
        if (state.AvailableCents < 0 || state.FrozenCents < 0 || state.LifetimeEarnedCents < 0 || state.InGameAdSuccessCount < 0 || state.WinAdSuccessCount < 0 || state.FirstAccelerationPaidCents < 0 || state.LastUtcSeconds < 0 || state.NextRewardSequence < 1 || double.IsNaN(state.EffectivePlaySeconds) || double.IsInfinity(state.EffectivePlaySeconds) || state.EffectivePlaySeconds < 0) throw new InvalidDataException("Invalid harvest ledger values.");
        if (state.CompletedTierIndexes == null || state.Settlements == null || state.CompletedTierIndexes.Count > settings.ThresholdCents.Length || state.Settlements.Count != state.CompletedTierIndexes.Count) throw new InvalidDataException("Invalid harvest settlement records.");
        for (int i = 0; i < state.CompletedTierIndexes.Count; i++)
        {
            int tier = state.CompletedTierIndexes[i];
            if (tier < 0 || tier >= settings.ThresholdCents.Length || state.CompletedTierIndexes.IndexOf(tier) != i) throw new InvalidDataException("Duplicate or invalid harvest tier.");
            HarvestSettlementRecord record = state.Settlements[i];
            if (record == null || string.IsNullOrEmpty(record.RequestId) || record.TierIndex != tier || record.AmountCents <= 0 || record.CompletedUtcSeconds < 0) throw new InvalidDataException("Invalid harvest settlement.");
        }
        HarvestRequestState request = state.ActiveRequest;
        if (request == null)
        {
            if (state.FrozenCents != 0) throw new InvalidDataException("Harvest frozen balance has no request.");
        }
        else
        {
            if (string.IsNullOrEmpty(request.RequestId) || request.TierIndex < 0 || request.TierIndex >= settings.ThresholdCents.Length || state.CompletedTierIndexes.Contains(request.TierIndex) || request.AmountCents <= 0 || request.AmountCents != state.FrozenCents || (request.Status != HarvestRequestState.StageActive && request.Status != HarvestRequestState.SettlementWait) || request.StageIndex < 0 || request.StageIndex >= settings.Stages.Length || request.StageProgress < 0 || request.StageProgress > settings.Stages[request.StageIndex].Target || request.StageStartedUtcSeconds < 0 || request.SettlementStartedUtcSeconds < 0) throw new InvalidDataException("Invalid active harvest request.");
            if (request.Status == HarvestRequestState.SettlementWait && (request.StageIndex != settings.Stages.Length - 1 || request.StageProgress < settings.Stages[request.StageIndex].Target)) throw new InvalidDataException("Invalid harvest settlement phase.");
        }
        HarvestRunState run = state.ActiveRun;
        if (run != null && (string.IsNullOrEmpty(run.RunId) || run.LastTripleIndex < -1 || run.TriplesSinceOffer < 0 || double.IsNaN(run.LastOfferEffectiveSeconds) || double.IsInfinity(run.LastOfferEffectiveSeconds) || run.LastOfferEffectiveSeconds < 0 || run.LastOfferEffectiveSeconds > state.EffectivePlaySeconds)) throw new InvalidDataException("Invalid harvest run.");
        ValidateOffer(state.PendingInGameReward, HarvestRewardOffer.InGame);
        ValidateOffer(state.PendingWinReward, HarvestRewardOffer.Win);
    }

    private static void ValidateOffer(HarvestRewardOffer offer, int kind)
    {
        if (offer != null && (offer.Kind != kind || string.IsNullOrEmpty(offer.RewardId) || string.IsNullOrEmpty(offer.RunId) || offer.BaseCents <= 0 || offer.DisplayCents < offer.BaseCents || offer.CreatedUtcSeconds < 0)) throw new InvalidDataException("Invalid pending harvest reward.");
    }

    private static void WriteState(BinaryWriter writer, HarvestRewardState s)
    {
        writer.Write(s.Version); writer.Write(s.AvailableCents); writer.Write(s.FrozenCents); writer.Write(s.LifetimeEarnedCents);
        writer.Write(s.TutorialGiftClaimed); writer.Write(s.FirstInGameClaimed); writer.Write(s.FirstWinClaimed); writer.Write(s.ReachedFirstTarget);
        writer.Write(s.InGameAdSuccessCount); writer.Write(s.WinAdSuccessCount); writer.Write(s.EffectivePlaySeconds); writer.Write(s.FirstAccelerationPaidCents);
        writer.Write(s.MilestoneClaimMask); writer.Write(s.LastUtcSeconds); writer.Write(s.ClockRollbackDetected); writer.Write(s.NextRewardSequence);
        writer.Write(s.CompletedTierIndexes.Count);
        for (int i = 0; i < s.CompletedTierIndexes.Count; i++) writer.Write(s.CompletedTierIndexes[i]);
        writer.Write(s.Settlements.Count);
        for (int i = 0; i < s.Settlements.Count; i++)
        {
            HarvestSettlementRecord r = s.Settlements[i];
            WriteText(writer, r.RequestId); writer.Write(r.TierIndex); writer.Write(r.AmountCents); writer.Write(r.CompletedUtcSeconds);
        }
        writer.Write(s.ActiveRun != null);
        if (s.ActiveRun != null)
        {
            HarvestRunState r = s.ActiveRun;
            WriteText(writer, r.RunId); writer.Write(r.LastTripleIndex); writer.Write(r.WinRecorded); writer.Write(r.AwaitingWin); writer.Write(r.TriplesSinceOffer); writer.Write(r.LastOfferEffectiveSeconds);
        }
        WriteOffer(writer, s.PendingInGameReward); WriteOffer(writer, s.PendingWinReward);
        writer.Write(s.ActiveRequest != null);
        if (s.ActiveRequest != null)
        {
            HarvestRequestState r = s.ActiveRequest;
            WriteText(writer, r.RequestId); writer.Write(r.TierIndex); writer.Write(r.AmountCents); writer.Write(r.Status); writer.Write(r.StageIndex); writer.Write(r.StageProgress); writer.Write(r.StageStartedUtcSeconds); writer.Write(r.StageTaskCompletedUtcSeconds); writer.Write(r.SettlementStartedUtcSeconds);
        }
    }

    private static HarvestRewardState ReadState(BinaryReader reader)
    {
        HarvestRewardState s = new HarvestRewardState();
        s.Version = reader.ReadInt32();
        if (s.Version != HarvestRewardState.CurrentVersion) throw new InvalidDataException("Unsupported harvest schema.");
        s.AvailableCents = reader.ReadInt64(); s.FrozenCents = reader.ReadInt64(); s.LifetimeEarnedCents = reader.ReadInt64();
        s.TutorialGiftClaimed = reader.ReadBoolean(); s.FirstInGameClaimed = reader.ReadBoolean(); s.FirstWinClaimed = reader.ReadBoolean(); s.ReachedFirstTarget = reader.ReadBoolean();
        s.InGameAdSuccessCount = reader.ReadInt32(); s.WinAdSuccessCount = reader.ReadInt32(); s.EffectivePlaySeconds = reader.ReadDouble(); s.FirstAccelerationPaidCents = reader.ReadInt64();
        s.MilestoneClaimMask = reader.ReadInt32(); s.LastUtcSeconds = reader.ReadInt64(); s.ClockRollbackDetected = reader.ReadBoolean(); s.NextRewardSequence = reader.ReadInt64();
        int count = ReadCount(reader);
        for (int i = 0; i < count; i++) s.CompletedTierIndexes.Add(reader.ReadInt32());
        count = ReadCount(reader);
        for (int i = 0; i < count; i++) s.Settlements.Add(new HarvestSettlementRecord { RequestId = ReadText(reader), TierIndex = reader.ReadInt32(), AmountCents = reader.ReadInt64(), CompletedUtcSeconds = reader.ReadInt64() });
        if (reader.ReadBoolean()) s.ActiveRun = new HarvestRunState { RunId = ReadText(reader), LastTripleIndex = reader.ReadInt32(), WinRecorded = reader.ReadBoolean(), AwaitingWin = reader.ReadBoolean(), TriplesSinceOffer = reader.ReadInt32(), LastOfferEffectiveSeconds = reader.ReadDouble() };
        s.PendingInGameReward = ReadOffer(reader); s.PendingWinReward = ReadOffer(reader);
        if (reader.ReadBoolean()) s.ActiveRequest = new HarvestRequestState { RequestId = ReadText(reader), TierIndex = reader.ReadInt32(), AmountCents = reader.ReadInt64(), Status = reader.ReadInt32(), StageIndex = reader.ReadInt32(), StageProgress = reader.ReadInt32(), StageStartedUtcSeconds = reader.ReadInt64(), StageTaskCompletedUtcSeconds = reader.ReadInt64(), SettlementStartedUtcSeconds = reader.ReadInt64() };
        return s;
    }

    private static int ReadCount(BinaryReader reader)
    {
        int value = reader.ReadInt32();
        if (value < 0 || value > 32) throw new InvalidDataException("Harvest collection exceeds its limit.");
        return value;
    }

    private static void WriteOffer(BinaryWriter writer, HarvestRewardOffer offer)
    {
        writer.Write(offer != null);
        if (offer == null) return;
        WriteText(writer, offer.RewardId); WriteText(writer, offer.RunId); writer.Write(offer.Kind); writer.Write(offer.CreatedUtcSeconds); writer.Write(offer.IsFirstFree); writer.Write(offer.BaseCents); writer.Write(offer.DisplayCents);
    }

    private static HarvestRewardOffer ReadOffer(BinaryReader reader)
    {
        if (!reader.ReadBoolean()) return null;
        return new HarvestRewardOffer { RewardId = ReadText(reader), RunId = ReadText(reader), Kind = reader.ReadInt32(), CreatedUtcSeconds = reader.ReadInt64(), IsFirstFree = reader.ReadBoolean(), BaseCents = reader.ReadInt64(), DisplayCents = reader.ReadInt64() };
    }

    private static void WriteText(BinaryWriter writer, string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 128) throw new InvalidDataException("Invalid harvest identifier.");
        writer.Write(value);
    }

    private static string ReadText(BinaryReader reader)
    {
        string value = reader.ReadString();
        if (string.IsNullOrEmpty(value) || value.Length > 128) throw new InvalidDataException("Invalid harvest identifier.");
        return value;
    }
}
