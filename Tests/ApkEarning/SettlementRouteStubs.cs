using System;
using System.Collections;
using System.Collections.Generic;

// Minimal dependencies for compiling the production settlement router in isolation.
// These do not initialize Unity, mutate rewards, or access any player save.
public class FOLJNEPEKCA<T> where T : new() { public static T Instance = new T(); }
public enum OGPEIPAJIDB { MainLevelWin }
public class WinUI { public static OGPEIPAJIDB openWinUIFrom; public static int wonLevel; }

public class EDLHEMMBABM : FOLJNEPEKCA<EDLHEMMBABM>
{
    public int Generation = 1, StartedLevels;
    public int GetGeneration() { return Generation; }
    public IEnumerator ParseConfig(CorePlay.LevelConfig config, bool replay, Action done)
    {
        // The production ParseConfig also increments generation before its first yield.
        Generation++;
        StartedLevels++;
        yield return null;
    }
}

public class JEFOMCDAPGK : FOLJNEPEKCA<JEFOMCDAPGK>
{
    public GameData Data = new GameData();
    public LevelData MainLevelData = new LevelData();
}
public class GameData { public bool isLevelFinished = true; }
public class LevelData { public void Reset(bool replay) {} }
public class PLMIHDHFAAL : FOLJNEPEKCA<PLMIHDHFAAL>
{
    public CorePlay.LevelConfig GetMainLevelConfig() { return new CorePlay.LevelConfig(); }
}
public class BMNFNJFCPHG : FOLJNEPEKCA<BMNFNJFCPHG>
{
    public bool Rated = true;
    public bool HasRated() { return Rated; }
}
public class MCCIJBJGMCK
{
    public static bool Locked;
    public static bool IsLock() { return Locked; }
}
public class MgrUI : FOLJNEPEKCA<MgrUI>
{
    public int LegacyWinOpens, RatingOpens;
    public void Open(string path)
    {
        if (path == "coreplaywin/WinUI") LegacyWinOpens++;
        else if (path == "pops/rate/RateUI") RatingOpens++;
        else throw new InvalidOperationException("Unexpected UI path: " + path);
    }
    public void DelayOpen(string path, float delay) { Open(path); }
}
public class GameAudio { public static void Play(DLMJOHCOJKN sound) {} }
public enum DLMJOHCOJKN { Play_sfx_ui_panel_game_win }

namespace CorePlay
{
    public class LevelConfig {}
    public class CorePlayUI : FOLJNEPEKCA<CorePlayUI>
    {
        public bool IsOpening = true;
        public void UpdateBehavior() {}
    }
}
namespace UnityEngine
{
    public class WaitForSeconds { public WaitForSeconds(float time) {} }
    public class Debug { public static void LogError(string error) { throw new InvalidOperationException(error); } }
}
namespace Module.Setting { public class UIRate { public static Action onCloseAction; } }
namespace Framework.Base.UtilModule
{
    public class CoroutineManager : FOLJNEPEKCA<CoroutineManager>
    {
        private readonly List<IEnumerator> pending = new List<IEnumerator>();
        public void StartCor(IEnumerator routine)
        {
            // Match Unity's synchronous execution up to the first yield.
            if (routine.MoveNext()) pending.Add(routine);
        }
        public void Tick()
        {
            var snapshot = pending.ToArray();
            foreach (var routine in snapshot)
                if (!routine.MoveNext()) pending.Remove(routine);
        }
    }
}
namespace FruitApkEarning
{
    public class State { public List<object> PendingRewards = new List<object>(); }
    public class ApkEarningService : FOLJNEPEKCA<ApkEarningService>
    {
        public bool Available = true;
        public State State = new State();
        public string Error = "";
    }
    public class ApkEarningHost
    {
        public static bool IsOpen;
        public static int Opens;
        public static Action Completion;
        public static void Open(bool diamond, bool rewards, Action callback)
        {
            if (IsOpen || MCCIJBJGMCK.Locked) return;
            if (diamond || !rewards) throw new InvalidOperationException("Settlement must open cash rewards.");
            IsOpen = true;
            Opens++;
            Completion = callback;
        }
    }
}
