using System;
using System.IO;
using FruitWithdrawal;
public static class WithdrawalCoreTests
{
    const long T = 2000000000;
    static int passed;
    static void Check(bool condition, string reason) { if (!condition) throw new Exception(reason); }
    static void Test(string name, Action test) { test(); passed++; Console.WriteLine("PASS " + name); }
    static WithdrawalEngine Fresh() { return new WithdrawalEngine(WithdrawalRules.CreateDefault()); }
    static void Win(WithdrawalEngine e, string run, int level, long utc) { e.BeginRun(run, utc); Check(e.RecordWin(run, level, false, utc), "win should record"); }
    static void Main()
    {
        Test("zero initial balances and explicit claiming", () => {
            var e = Fresh(); Check(e.State.CashCents == 0 && e.State.DiamondUnits == 0, "nonzero initial wallet");
            e.BeginRun("r", T); e.RecordTriple("r", 2, false, T); Check(e.State.CashCents == 0, "unclaimed reward entered wallet");
            Check(e.State.PendingRewards.Count == 1 && e.State.PendingRewards[0].CashCents == 20, "match batch amount");
            Check(!e.RecordTriple("r", 2, false, T), "duplicate sequence paid");
            string reward = e.State.PendingRewards[0].RewardId; Check(e.ClaimReward(reward, T), "claim failed");
            Check(!e.ClaimReward(reward, T), "duplicate claim paid"); Check(e.State.CashCents == 20 && e.State.DiamondUnits == 200, "claim totals");
        });
        Test("legacy match baseline migrates once and does not overwrite resumed progress", () => {
            var e = Fresh(); Check(e.BeginRun("legacy", T, 17), "legacy run not created");
            Check(e.State.PendingRewards.Count == 0 && e.State.Runs[0].LastTripleSequence == 17, "migration generated rewards");
            Check(e.RecordTriple("legacy", 18, false, T), "first new match not recorded");
            Check(e.State.PendingRewards[0].CashCents == 10 && e.State.PendingRewards[0].DiamondUnits == 100, "old 17 matches paid retroactively");
            Check(e.ClaimAllRewards(T), "new match reward not claimable");
            var resumed = new WithdrawalEngine(e.Rules, WithdrawalStorage.Clone(e.State));
            resumed.BeginRun("legacy", T, 99);
            Check(resumed.State.Runs[0].LastTripleSequence == 18, "new baseline overwrote a restored checkpoint");
            Check(resumed.RecordTriple("legacy", 19, false, T), "existing checkpoint lost next match");
            resumed.BeginRun("legacy", T, 0);
            Check(!resumed.RecordTriple("legacy", 19, false, T), "smaller baseline allowed duplicate reward");
            Check(resumed.ClaimAllRewards(T) && resumed.State.CashCents == 20 && resumed.State.DiamondUnits == 200, "migrated run totals incorrect");
            Check(!resumed.BeginRun("invalid-baseline", T, -1) && resumed.State.Runs.Count == 1, "negative baseline created a run");
        });
        Test("first win once, unclaimed survives next run, batch claim", () => {
            var e = Fresh(); Win(e,"a",1,T); Check(!e.RecordWin("a",1,false,T), "win duplicated");
            e.BeginRun("b",T); Check(e.State.PendingRewards.Count == 1, "next run dropped offer");
            e.RecordTriple("b",1,false,T); e.RecordWin("b",2,false,T);
            Check(e.State.PendingRewards.Count == 3, "three offers expected"); Check(e.ClaimAllRewards(T), "batch claim failed");
            Check(e.State.CashCents == 210 && e.State.DiamondUnits == 32100, "first bonus repeated or totals wrong");
            Check(!e.ClaimAllRewards(T), "empty batch mutated");
        });
        Test("freeze one request and cannot approve early", () => {
            var e = Fresh(); Win(e,"a",1,T); e.ClaimAllRewards(T);
            Check(e.Apply("cash-040","test-wallet",T), "apply failed");
            Check(e.State.CashCents == 60 && e.State.FrozenCashCents == 40, "reservation wrong");
            string id = e.State.ActiveRequest.RequestId;
            Check(!e.Apply("cash-060","test-bank",T), "multiple active requests");
            Check(!e.ResolveSimulation(id,true,T+59), "early approval accepted");
            Check(e.State.ActiveRequest.Status == WithdrawalRequestStatus.ReviewPending, "review changed early");
            e.Tick(T+60); Check(e.State.ActiveRequest == null && e.State.Requests[0].Status == WithdrawalRequestStatus.SimulatedCompleted, "review did not finish as simulation");
            Check(e.State.FrozenCashCents == 0 && e.State.CashCents == 60, "review settlement amount");
            Check(!e.ResolveSimulation(id,true,T+61), "terminal duplicated");
            Check(!e.Apply("cash-040","test-wallet",T+61), "completed tier allowed twice");
        });
        Test("cancel and reject refund once", () => {
            var e = Fresh(); Win(e,"a",1,T); e.ClaimAllRewards(T); e.Apply("cash-060","test-wallet",T);
            string id = e.State.ActiveRequest.RequestId; Check(e.CancelRequest(id,T), "cancel failed");
            Check(e.State.CashCents == 100 && e.State.FrozenCashCents == 0, "cancel refund");
            Check(!e.CancelRequest(id,T), "cancel repeated"); e.Apply("cash-060","test-bank",T);
            Check(e.ResolveSimulation(e.State.ActiveRequest.RequestId,false,T), "reject failed");
            Check(e.State.CashCents == 100 && e.State.Requests[1].Status == WithdrawalRequestStatus.Rejected, "reject refund");
        });
        Test("all seven conditions are published, frozen terms, two qualifying UTC days", () => {
            var rules = WithdrawalRules.CreateDefault(); rules.DailyWinsForCheckIn = 2;
            var tier = rules.Tiers[2]; tier.RequiredBalanceUnits=100; tier.RequiredPassLevels=2; tier.RequiredHiddenPassLevels=3;
            tier.RequiredStage2PassLevels=4; tier.RequiredDailyWins=2; tier.RequiredCheckInDays=2; tier.RequiredUserLevel=3; tier.ReviewSeconds=5;
            var initial = new WithdrawalState { CashCents=100 }; var e = new WithdrawalEngine(rules,initial);
            Check(e.GetConditions("cash-main").Length == 7, "not all prerequisites published");
            Check(e.Apply("cash-main","test-wallet",T), "initial apply failed");
            e.Rules.Tiers[2].RequiredStage2PassLevels = 999;
            Win(e,"a",1,T); Win(e,"b",2,T); Check(e.State.QualifiedCheckInDays == 1, "first day not credited");
            e.Tick(T+86400); Check(e.State.TodayWins == 0 && e.State.QualifiedCheckInDays == 1, "UTC rollover credited unplayed day");
            Win(e,"c",3,T+86400); Win(e,"d",4,T+86400);
            Check(e.State.QualifiedCheckInDays == 2 && e.State.ActiveRequest.Status == WithdrawalRequestStatus.ReviewPending, "fixed terms or second qualified day failed");
            Check(e.State.ActiveRequest.Terms.RequiredStage2PassLevels == 4, "request terms changed");
            e.Tick(T+86405); Check(e.State.Requests[0].Status == WithdrawalRequestStatus.SimulatedCompleted, "complete path failed");
        });
        Test("clock rollback clamps and does not reset qualified day", () => {
            var e=Fresh(); Win(e,"r",1,T); e.Tick(T-86400);
            Check(e.State.LastUtcSeconds==T && e.State.TodayWins==1 && e.State.ClockRollbackDetected, "clock rollback not clamped");
        });
        Test("restart preserves rewards, reservations and deduplication", () => {
            var e=Fresh(); Win(e,"r",1,T); e.ClaimAllRewards(T); e.Apply("cash-040","test-wallet",T);
            var clone=WithdrawalStorage.Clone(e.State); var resumed=new WithdrawalEngine(e.Rules,clone);
            Check(!resumed.RecordWin("r",1,false,T), "restored win duplicated");
            Check(resumed.State.FrozenCashCents==40, "reservation lost"); resumed.Tick(T+600);
            Check(resumed.State.Requests[0].Status==WithdrawalRequestStatus.SimulatedCompleted, "offline review did not finish");
        });
        Test("binary atomic save and verified backup recovery", () => {
            string folder=Path.Combine(Path.GetTempPath(),"fruit-withdrawal-probe-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
            string file=Path.Combine(folder,"state.dat"); var e=Fresh(); e.BeginRun("r",T); WithdrawalStorage.Save(file,e.State);
            e.RecordTriple("r",1,false,T); WithdrawalStorage.Save(file,e.State);
            Check(WithdrawalStorage.Load(file).PendingRewards.Count==1,"save roundtrip");
            File.WriteAllBytes(file,new byte[]{1,2}); bool recovered; var old=WithdrawalStorage.Load(file,out recovered);
            Check(recovered && old.PendingRewards.Count==0,"valid backup recovery failed");
            File.WriteAllBytes(file+".bak",new byte[]{2,3}); bool failed=false;
            try { WithdrawalStorage.Load(file); } catch(InvalidDataException) { failed=true; } Check(failed,"corrupt state silently reset");
            string other=Path.Combine(folder,"atomic.dat"); WithdrawalStorage.Save(other,e.State); Directory.CreateDirectory(other+".tmp");
            e.RecordTriple("r",2,false,T); failed=false;
            try { WithdrawalStorage.Save(other,e.State); } catch(IOException) { failed=true; } catch(UnauthorizedAccessException) { failed=true; }
            Check(failed && WithdrawalStorage.Load(other).PendingRewards[0].CashCents==10,"failed save changed committed state");
            string orphan=Path.Combine(folder,"orphan.dat"); File.WriteAllBytes(orphan+".tmp",new byte[]{1}); failed=false;
            try { WithdrawalStorage.Load(orphan); } catch(InvalidDataException) { failed=true; } Check(failed,"orphan initial save silently reset");
        });
        Test("invalid rules and fabricated terminal rejected", () => {
            var rules=WithdrawalRules.CreateDefault(); rules.Tiers[1].Id=rules.Tiers[0].Id; bool failed=false;
            try { new WithdrawalEngine(rules); } catch(InvalidOperationException) { failed=true; } Check(failed,"duplicate tier accepted");
            var e=Fresh(); Win(e,"r",1,T); e.ClaimAllRewards(T); e.Apply("cash-040","test-wallet",T);
            e.State.Requests[0].CompletedConditionsMask=0; failed=false;
            try { WithdrawalStorage.Clone(e.State); } catch(InvalidOperationException) { failed=true; } Check(failed,"review without prerequisites accepted");
        });
        Console.WriteLine(passed+" passed, 0 failed");
    }
}
