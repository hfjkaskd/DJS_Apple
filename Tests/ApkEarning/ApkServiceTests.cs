using System;
using System.IO;
using FruitApkEarning;

namespace UnityEngine {
    public class ScriptableObject { }
    [AttributeUsage(AttributeTargets.Class)] public sealed class CreateAssetMenuAttribute : Attribute { public string menuName; }
    public static class Resources { public static object Asset; public static T Load<T>(string path) where T:class { return Asset as T; } }
    public static class Application { public static string persistentDataPath; }
    public static class Debug { public static void LogError(object v) { } public static void LogWarning(object v) { } public static void LogException(Exception e) { } }
}
namespace CorePlay { public class LevelData { public string withdrawalRunId; public int elimGroupCount, totalItemCount, eliminatedItemCount; } }
public sealed class JEFOMCDAPGK {
    public static readonly JEFOMCDAPGK Instance=new JEFOMCDAPGK();
    public int CurMainLevelIndex=4;
    public bool SaveAllowed=true;
    public bool TrySaveToDisk() { return SaveAllowed; }
}
public static class ApkServiceTests {
    static int checks;
    static void Check(bool value,string name) { if(!value)throw new Exception(name); Console.WriteLine("PASS "+name);checks++; }
    static void WriteV1(string path,int level) {
        byte[] payload;
        using(var ms=new MemoryStream())using(var w=new BinaryWriter(ms)) {
            w.Write(1);w.Write(123L);w.Write(456L);w.Write(level);w.Write(level-1);w.Write(0);w.Write(1);w.Write(0);
            w.Write(false);w.Write(false);w.Write(true);w.Write(false);w.Write(0);w.Write(0);w.Write(0L);
            w.Write(1L);w.Write(100000L);w.Write(0L);w.Write(0);w.Write(0);w.Write(0);w.Flush();payload=ms.ToArray();
        }
        using(var sha=System.Security.Cryptography.SHA256.Create())using(var f=File.Create(path))using(var w=new BinaryWriter(f)) {
            w.Write(0x46414531u);w.Write(1);w.Write(payload.Length);w.Write(sha.ComputeHash(payload));w.Write(payload);
        }
    }
    public static void Main() {
        string root=Path.Combine(Path.GetTempPath(),"FruitApkServiceTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        UnityEngine.Application.persistentDataPath=root;
        var rules=ApkEarningRules.CreateDefault(); UnityEngine.Resources.Asset=new ApkEarningConfig{Rules=rules};
        Check(!ApkEarningService.Instance.Available&&string.IsNullOrEmpty(ApkEarningService.Instance.Error),"not initialized is distinguishable from a recoverable save failure");
        var presentation=(ApkEarningConfig)UnityEngine.Resources.Asset;
        presentation.DisplayLevels=new int[52];presentation.DisplayLevels[22]=5;presentation.DisplayLevels[51]=30;
        Check(presentation.GetDisplayLevel(22)==5&&presentation.GetDisplayLevel(51)==30&&presentation.GetDisplayLevel(99)==99,"display levels map source gates without changing actual board indexes");
        Check(presentation.PaymentChannels[0].Name=="Venmo"&&presentation.PaymentChannels[1].Name=="Zelle"&&!presentation.PaymentChannels[0].UsesEmail&&presentation.PaymentChannels[2].UsesEmail,"US channel definitions match phone and email modes");
        Check(presentation.PaymentChannels[0].Validate("Test user","","0000000000")==""&&presentation.PaymentChannels[1].Validate("Test user","valid@example.com","1")!="","phone channel validates phone rather than hidden email");
        Check(presentation.PaymentChannels[2].Validate("Test user","test@example.com","")==""&&presentation.PaymentChannels[2].Validate("Test user","invalid","0000000000")!="","email channel validates email rather than hidden phone");
        Check(presentation.PaymentChannels[0].FormatAccount("Test user","email@example.com","+1","0000000000")=="Test user\n+1 0000000000"&&presentation.PaymentChannels[2].FormatAccount("Test user","email@example.com","+1","0000000000")=="Test user\nemail@example.com","confirmation formats only selected channel account data");
        string sample=Path.Combine(root,"sample.dat");bool recovered;
        string v1=Path.Combine(root,"v1.dat");WriteV1(v1,1);var migrated=ApkEarningStorage.Load(v1,rules,out recovered);
        Check(migrated.Version==3&&!migrated.WelcomeShown&&migrated.CashCents==123&&migrated.Diamonds==456,"version 1 first-level save migrates without losing balances");
        WriteV1(v1,4);migrated=ApkEarningStorage.Load(v1,rules,out recovered);
        Check(migrated.Version==3&&migrated.WelcomeShown&&migrated.CurrentLevel==4,"version 1 returning player skips welcome");
        migrated.WelcomeShown=true;ApkEarningStorage.Save(v1,migrated);migrated=ApkEarningStorage.Load(v1,rules,out recovered);
        Check(migrated.WelcomeShown&&ApkEarningStorage.Clone(migrated).WelcomeShown,"welcome flag persists and survives clone");
        var state=new ApkEarningState{CashCents=123,Diamonds=456,CurrentLevel=4,HighestClearedLevel=3};
        var engine=new ApkEarningEngine(rules,state,()=>0.5);engine.BeginRun("a",0,4,100000);engine.RecordWin("a",4,100000);
        ApkEarningStorage.Save(sample,state);var loaded=ApkEarningStorage.Load(sample,rules,out recovered);
        Check(!recovered&&loaded.CashCents==123&&loaded.Diamonds==456&&loaded.PendingRewards.Count==1&&loaded.Runs[0].Won,"roundtrip preserves pending reward and checkpoint");
        var copy=ApkEarningStorage.Clone(state);copy.PendingRewards[0].CashCents++;copy.Runs[0].Won=false;
        Check(copy.PendingRewards[0].CashCents!=state.PendingRewards[0].CashCents&&state.Runs[0].Won,"clone isolates nested mutations");
        state.CashCents=999;ApkEarningStorage.Save(sample,state);File.WriteAllBytes(sample,new byte[]{1,2,3});
        loaded=ApkEarningStorage.Load(sample,rules,out recovered);Check(recovered&&loaded.CashCents==123,"corrupt primary recovers intact backup");
        ApkEarningStorage.Save(sample,loaded,true);loaded=ApkEarningStorage.Load(sample,rules,out recovered);Check(!recovered&&loaded.CashCents==123,"recovery rewrite keeps valid primary");
        File.WriteAllBytes(sample,new byte[]{1});File.WriteAllBytes(sample+".bak",new byte[]{2});bool rejected=false;
        try{ApkEarningStorage.Load(sample,rules,out recovered);}catch(InvalidDataException){rejected=true;}
        Check(rejected,"both corrupt copies rejected rather than reset");
        string incomplete=Path.Combine(root,"incomplete.dat");File.WriteAllText(incomplete+".tmp","partial");rejected=false;
        try{ApkEarningStorage.Load(incomplete,rules,out recovered);}catch(InvalidDataException){rejected=true;}
        Check(rejected,"incomplete first save never reseeds balances");
        string legacy=Path.Combine(root,"fruit-withdrawal-simulation-v1.dat");FruitWithdrawal.WithdrawalStorage.Save(legacy,new FruitWithdrawal.WithdrawalState{CashCents=200,DiamondUnits=33});byte[] old=File.ReadAllBytes(legacy);
        FruitWithdrawal.WithdrawalGameplayBridge.Initialize();var service=ApkEarningService.Instance;
        Check(service.Available&&service.State.CashCents==200&&service.State.Diamonds==3300&&service.State.CurrentLevel==4&&service.State.HighestClearedLevel==3,"migration scales diamonds and preserves actual game level");
        Check(Convert.ToBase64String(old)==Convert.ToBase64String(File.ReadAllBytes(legacy))&&!File.Exists(legacy+".bak"),"legacy save remains byte identical");
        var board=new CorePlay.LevelData{totalItemCount=6};JEFOMCDAPGK.Instance.SaveAllowed=false;
        Check(!FruitWithdrawal.WithdrawalGameplayBridge.BeginRun(board)&&service.State.Runs.Count==0,"failed gameplay save cannot register reward run");
        JEFOMCDAPGK.Instance.SaveAllowed=true;Check(FruitWithdrawal.WithdrawalGameplayBridge.BeginRun(board),"persisted gameplay run accepted");
        int notifications=0;bool publishedMatchesDisk=true;service.Changed+=()=>{notifications++;bool backup;var disk=ApkEarningStorage.Load(Path.Combine(root,"fruit-apk-earning-v1.dat"),rules,out backup);publishedMatchesDisk=publishedMatchesDisk&&disk.CashCents==service.State.CashCents;};
        board.elimGroupCount=1;board.eliminatedItemCount=3;Check(FruitWithdrawal.WithdrawalGameplayBridge.RecordTriple(board,4)&&notifications==1&&publishedMatchesDisk&&service.State.CashCents==210&&service.State.Diamonds==3400,"match persisted before Changed and live balances published");
        Check(!FruitWithdrawal.WithdrawalGameplayBridge.RecordTriple(board,4)&&notifications==1,"duplicate triple does not award twice");
        Check(FruitWithdrawal.WithdrawalGameplayBridge.RecordWin(board,4)&&service.State.PendingRewards.Count==1,"win records separate claimable reward");
        string reward=service.State.PendingRewards[0].Id;Check(service.Claim(reward)&&!service.Claim(reward),"claim persisted and idempotent");
        service.BeginRun("early-submit",0,1);service.RecordWin("early-submit",1);service.Claim("early-submit:level");long beforeSubmit=service.State.CashCents;
        string submitTemporary=Path.Combine(root,"fruit-apk-earning-v1.dat.tmp");Directory.CreateDirectory(submitTemporary);
        Check(!service.SubmitEarlyRecord(1)&&!service.State.Records[0].Archived&&!service.State.Records[0].Pending&&service.State.CashCents==beforeSubmit,"failed direct submit never archives or debits the visible record");
        Directory.Delete(submitTemporary);service.Init(4);int beforeNotifications=notifications;
        Check(service.SubmitEarlyRecord(1)&&service.State.Records[0].Archived&&service.State.Records[0].Pending&&service.State.CashCents==beforeSubmit-rules.EarlyFirstCents&&notifications==beforeNotifications+1,"direct early submission archives and marks pending in one persisted notification");
        beforeSubmit=service.State.CashCents;beforeNotifications=notifications;
        Check(service.SubmitEarlyRecord(1)&&service.State.CashCents==beforeSubmit&&notifications==beforeNotifications,"repeated pending submit succeeds without duplicate debit or save");
        Check(service.ArchiveEarlyReward(1)&&service.State.CashCents==beforeSubmit&&notifications==beforeNotifications,"already archived close/back is idempotently successful");
        string save=Path.Combine(root,"fruit-apk-earning-v1.dat");Directory.CreateDirectory(save+".tmp");var before=service.State;long cash=before.CashCents;int runCount=before.Runs.Count;
        Check(!service.BeginRun("failed",0,5)&&!service.Available&&!string.IsNullOrEmpty(service.Error)&&ReferenceEquals(before,service.State)&&service.State.CashCents==cash,"failed disk write does not publish candidate and exposes recovery");
        Check(!service.GetRequirements(false,0).Eligible,"unavailable service never reports eligible");
        Directory.Delete(save+".tmp");Check(service.Init(5)&&service.Available&&service.State.Runs.Count==runCount&&service.State.CashCents==cash,"retry reloads committed state without replaying failed mutation");
        Directory.CreateDirectory(save+".tmp");before=service.State;
        Check(!service.CompleteWelcome()&&!service.Available&&!service.State.WelcomeShown&&ReferenceEquals(before,service.State),"failed welcome save keeps first-run guide pending");
        Directory.Delete(save+".tmp");service.Init(5);beforeNotifications=notifications;
        Check(service.CompleteWelcome()&&service.State.WelcomeShown&&notifications==beforeNotifications+1,"welcome completion persists before notification");
        beforeNotifications=notifications;Check(service.CompleteWelcome()&&notifications==beforeNotifications,"repeated welcome completion is idempotent");
        cash=service.State.CashCents;long diamonds=service.State.Diamonds;Directory.CreateDirectory(save+".tmp");before=service.State;
        Check(!service.CompleteDiamondGuide()&&!service.Available&&!service.State.DiamondGuideShown&&ReferenceEquals(before,service.State),"failed diamond guide save leaves guide pending");
        Directory.Delete(save+".tmp");service.Init(5);beforeNotifications=notifications;
        Check(service.CompleteDiamondGuide()&&service.State.DiamondGuideShown&&service.State.CashCents==cash&&service.State.Diamonds==diamonds&&notifications==beforeNotifications+1,"diamond guide completion persists without granting preview currency");
        beforeNotifications=notifications;Check(service.CompleteDiamondGuide()&&notifications==beforeNotifications,"diamond guide completion is idempotent");
        loaded=ApkEarningStorage.Load(save,rules,out recovered);Check(loaded.DiamondGuideShown,"diamond guide acknowledgment survives a save reload");
        Console.WriteLine("APK_SERVICE_TESTS_PASS "+checks);
    }
}
