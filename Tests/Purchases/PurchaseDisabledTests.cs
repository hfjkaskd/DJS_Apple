// Production CorePlayItemBtn compiled against minimal in-memory presentation/gameplay doubles.
// No Unity Editor process, reflection, or player save access.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

static class PurchaseDisabledTests
{
    static int passed;
    static void Main()
    {
        Run("all stocked tools consume inventory once and never coins", StockedTools);
        Run("empty tools never purchase even with ample coins", EmptyTools);
        Run("free grants immediately restore usable inventory", FreeGrant);
        Run("stale positive UI cannot use an exhausted tool", StaleInventory);
        Run("global and per-button locks preserve inventory", Locks);
        Run("locked levels and unavailable actions preserve inventory", Unavailable);
        Run("standard Button is sole dispatcher after repeated initialization", OneDispatcher);
        Console.WriteLine(passed+" purchase-removal checks passed.");
    }
    static void Run(string name,Action run) { run();passed++;Console.WriteLine("PASS "+name); }
    static void Equal<T>(T expected,T actual) { if(!EqualityComparer<T>.Default.Equals(expected,actual))throw new Exception("Expected "+expected+"; got "+actual); }
    static CorePlayItemBtn Setup(POJCEPBNNIP type,int count,out Button button)
    {
        GNEJJHEDEBL.Reset();EDLHEMMBABM.Instance.Used=0;EDLHEMMBABM.Instance.CanUse=true;
        JEFOMCDAPGK.Instance.CurMainLevelIndex=20;MCCIJBJGMCK.Locked=false;
        GNEJJHEDEBL.Instance.Counts[POJCEPBNNIP.Coin]=1000000;
        GNEJJHEDEBL.Instance.Counts[type]=count;
        var item=new CorePlayItemBtn();button=new Button();item.gameObject.Attach(button);
        item.Init(type);return item;
    }
    static void StockedTools()
    {
        foreach(var type in new[]{POJCEPBNNIP.Undo,POJCEPBNNIP.Shuffle,POJCEPBNNIP.Magic,POJCEPBNNIP.Extra}) {
            Button button;Setup(type,2,out button);Equal(true,button.interactable);button.Press();
            Equal(1,GNEJJHEDEBL.Instance.GetItemCount(type,true));Equal(1,EDLHEMMBABM.Instance.Used);Equal(1000000,GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin,true));
        }
    }
    static void EmptyTools()
    {
        foreach(var type in new[]{POJCEPBNNIP.Undo,POJCEPBNNIP.Shuffle,POJCEPBNNIP.Magic,POJCEPBNNIP.Extra}) {
            Button button;Setup(type,0,out button);Equal(false,button.interactable);button.onClick.Invoke();
            Equal(0,EDLHEMMBABM.Instance.Used);Equal(1000000,GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin,true));
        }
    }
    static void FreeGrant()
    {
        Button button;Setup(POJCEPBNNIP.Magic,0,out button);
        GNEJJHEDEBL.Instance.Grant(POJCEPBNNIP.Magic,1);Equal(true,button.interactable);button.Press();
        Equal(1,EDLHEMMBABM.Instance.Used);Equal(false,button.interactable);
    }
    static void StaleInventory()
    {
        Button button;Setup(POJCEPBNNIP.Shuffle,1,out button);GNEJJHEDEBL.Instance.Counts[POJCEPBNNIP.Shuffle]=0;
        button.Press();Equal(0,EDLHEMMBABM.Instance.Used);Equal(false,button.interactable);
    }
    static void Locks()
    {
        Button button;var item=Setup(POJCEPBNNIP.Undo,2,out button);
        MCCIJBJGMCK.Locked=true;button.onClick.Invoke();Equal(0,EDLHEMMBABM.Instance.Used);
        MCCIJBJGMCK.Locked=false;item.DisableClick();button.onClick.Invoke();Equal(0,EDLHEMMBABM.Instance.Used);
        item.EnableClick();button.Press();Equal(1,EDLHEMMBABM.Instance.Used);
    }
    static void Unavailable()
    {
        Button button;var item=Setup(POJCEPBNNIP.Extra,2,out button);
        EDLHEMMBABM.Instance.CanUse=false;item.Refresh();button.Press();Equal(0,EDLHEMMBABM.Instance.Used);
        JEFOMCDAPGK.Instance.CurMainLevelIndex=1;EDLHEMMBABM.Instance.CanUse=true;item.Refresh();button.Press();Equal(0,EDLHEMMBABM.Instance.Used);
    }
    static void OneDispatcher()
    {
        Button button;var item=Setup(POJCEPBNNIP.Undo,2,out button);
        int legacyCount=0;var legacy=new InputMono { onClick=go=>legacyCount++ };item.gameObject.Attach(legacy);
        item.Init(POJCEPBNNIP.Undo);item.Init(POJCEPBNNIP.Undo);
        Equal(false,legacy.enabled);Equal(true,legacy.onClick==null);Equal(1,button.onClick.Count);
        button.Press();Equal(1,EDLHEMMBABM.Instance.Used);Equal(0,legacyCount);
    }
}
namespace UnityEngine {
    public sealed class SerializeField:Attribute { }
    public class GameObject {
        readonly Dictionary<Type,object> components=new Dictionary<Type,object>();
        public readonly Transform transform;public bool activeSelf=true;
        public GameObject() { transform=new Transform { gameObject=this }; }
        public void Attach<T>(T component) { components[typeof(T)]=component; }
        public T GetComponent<T>() where T:class { object value;return components.TryGetValue(typeof(T),out value)?value as T:null; }
        public void SetActive(bool value) { activeSelf=value; }
    }
    public class MonoBehaviour {
        public GameObject gameObject=new GameObject();public bool enabled=true;public Transform transform=>gameObject.transform;
        public T GetComponent<T>() where T:class { return gameObject.GetComponent<T>(); }
    }
    public class Transform {
        public GameObject gameObject;public Transform parent;
        public Transform Find(string path) { return null; }
        public T GetComponent<T>() where T:class { return gameObject.GetComponent<T>(); }
    }
    public class Sprite { }
    public class Animation { public object this[string name]=>null;public void Play(string name) { } }
}
namespace TMPro { }
namespace UnityEngine.UI {
    public class Image:MonoBehaviour { public Sprite sprite;public void SetNativeSize() { } }
    public class ClickEvent {
        readonly List<Action> actions=new List<Action>();public int Count=>actions.Count;
        public void AddListener(Action action){actions.Add(action);}public void RemoveListener(Action action){actions.RemoveAll(a=>a==action);}
        public void Invoke(){foreach(var action in actions.ToArray())action();}
    }
    public class Button { public bool interactable=true;public readonly ClickEvent onClick=new ClickEvent();public void Press(){if(interactable)onClick.Invoke();} }
}
public class TextMeshProCustom { public string text; }
public class InputMono:MonoBehaviour { public Action<GameObject> onClick; }
public static class MCCIJBJGMCK { public static bool Locked;public static bool IsLock(){return Locked;} }
public enum FFMLGGBCOOO { neutral }
public static class DLMJOHCOJKN { public const int Play_sfx_ui_button_common=1; }
public static class GameAudio { public static void Play(int id) { } }
public static class GameRes { public static Sprite LoadSprite(string path){return null;} }
public class MgrGlobalUI { public static readonly MgrGlobalUI Instance=new MgrGlobalUI();public void ShowGlobalText(string message,FFMLGGBCOOO kind){} }
public class OJEEJGGLNPC { public static readonly OJEEJGGLNPC Instance=new OJEEJGGLNPC();public string GetText(string key){return key;} }
public class JEFOMCDAPGK { public static readonly JEFOMCDAPGK Instance=new JEFOMCDAPGK();public int CurMainLevelIndex; }
public class GNEJJHEDEBL {
    public static readonly GNEJJHEDEBL Instance=new GNEJJHEDEBL();public static event Action<POJCEPBNNIP> OnItemChanged;
    public readonly Dictionary<POJCEPBNNIP,int> Counts=new Dictionary<POJCEPBNNIP,int>();
    public static void Reset(){Instance.Counts.Clear();OnItemChanged=null;}
    public int GetItemUnlockLevel(POJCEPBNNIP type){return 2;}
    public int GetItemCount(POJCEPBNNIP type,bool temporary){int count;return Counts.TryGetValue(type,out count)?count:0;}
    public void Grant(POJCEPBNNIP type,int count){Counts[type]=GetItemCount(type,true)+count;OnItemChanged?.Invoke(type);}
}
public class EDLHEMMBABM {
    public static readonly EDLHEMMBABM Instance=new EDLHEMMBABM();public int Used;public bool CanUse;
    public bool CheckCanUndo(){return CanUse;}public bool CheckCanShuffle(){return CanUse;}public bool CheckCanMagic(){return CanUse;}public bool CheckCanAddOne(){return CanUse;}
    bool Use(POJCEPBNNIP type){if(!CanUse)return false;Used++;GNEJJHEDEBL.Instance.Grant(type,-1);return true;}
    public bool Undo(){return Use(POJCEPBNNIP.Undo);}public bool Shuffle(){return Use(POJCEPBNNIP.Shuffle);}public bool Magic(){return Use(POJCEPBNNIP.Magic);}public bool AddOne(){return Use(POJCEPBNNIP.Extra);}
}
