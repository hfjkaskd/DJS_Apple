// Standalone presentation harness only. This file is outside Assets and never ships in Unity.
using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine {
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    public class Object {
        public static T Instantiate<T>(T original, Transform parent, bool world) where T : Object { return (T)(object)((GameObject)(object)original).Factory(); }
        public static void Destroy(Object value) { }
    }
    public class GameObject : Object {
        public Func<GameObject> Factory;
        public MonoBehaviour Component;
        public bool activeSelf = true;
        public void SetActive(bool value) { activeSelf = value; }
        public T GetComponent<T>() where T : class { return Component as T; }
    }
    public class MonoBehaviour : Object {
        public GameObject gameObject = new GameObject();
        public Transform transform = new RectTransform();
        public Canvas ParentCanvas;
        public int ParentLookupCount;
        public T GetComponentInParent<T>() where T : class { ParentLookupCount++; return ParentCanvas as T; }
        public Coroutine StartCoroutine(IEnumerator routine) { return new Coroutine(); }
        public void StopCoroutine(Coroutine routine) { }
    }
    public class Canvas : MonoBehaviour { public Canvas rootCanvas; }
    public class Transform : Object {
        public Transform parent;
        public Vector3 localPosition;
        public Vector3 localScale=Vector3.one;
        public Vector3 position { get { return TransformPoint(new Vector3()); } set { localPosition=parent==null?value:parent.InverseTransformPoint(value); } }
        public Vector3 TransformPoint(Vector3 p) {
            var value=new Vector3(localPosition.x+p.x*localScale.x,localPosition.y+p.y*localScale.y,localPosition.z+p.z*localScale.z);
            return parent==null?value:parent.TransformPoint(value);
        }
        public Vector3 InverseTransformPoint(Vector3 p) {
            var value=parent==null?p:parent.InverseTransformPoint(p);
            return new Vector3((value.x-localPosition.x)/localScale.x,(value.y-localPosition.y)/localScale.y,(value.z-localPosition.z)/localScale.z);
        }
    }
    public class RectTransform : Transform { public Rect rect = new Rect(); public Vector2 anchoredPosition,anchorMin,anchorMax,offsetMin,offsetMax; }
    public struct Rect {
        public float x,y,width,height;
        public Rect(float x,float y,float width,float height) { this.x=x;this.y=y;this.width=width;this.height=height; }
        public float xMin=>x;public float yMin=>y;public float xMax=>x+width;public float yMax=>y+height;
        public Vector2 center=>new Vector2(x+width/2,y+height/2);
    }
    public struct Vector2 {
        public static Vector2 zero => new Vector2(); public float x,y; public Vector2(float a,float b) { x=a;y=b; }
        public static Vector2 operator -(Vector2 a) { return new Vector2(-a.x,-a.y); }
        public static Vector2 operator *(Vector2 a,float b) { return new Vector2(a.x*b,a.y*b); }
    }
    public struct Vector3 {
        public float x,y,z; public Vector3(float x,float y,float z) { this.x=x;this.y=y;this.z=z; } public static Vector3 one => new Vector3 { x=1,y=1,z=1 };
        public static Vector3 operator *(Vector3 a,float b) { return new Vector3 { x=a.x*b,y=a.y*b,z=a.z*b }; }
    }
    public static class Mathf {
        public static float Min(float a,float b,float c) { return Math.Min(a,Math.Min(b,c)); }
        public static float Clamp(float value,float min,float max) { return Math.Max(min,Math.Min(max,value)); }
        public static float Clamp01(float value) { return Math.Max(0,Math.Min(1,value)); }
    }
    public class Coroutine { }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float seconds) { } }
    public static class Resources {
        public static readonly Dictionary<string,GameObject> Pages = new Dictionary<string,GameObject>();
        public static T Load<T>(string path) where T : class { GameObject page; return Pages.TryGetValue(path,out page) ? page as T : null; }
    }
    public static class Debug { public static void LogError(string value) { throw new Exception(value); } }
}
namespace TMPro { public sealed class TMP_Text { public string text; } }
namespace UnityEngine.UI {
    public sealed class Image { public enum Type { Simple,Sliced,Tiled,Filled } public Type type; public float fillAmount; public bool enabled=true; public UnityEngine.RectTransform rectTransform=new UnityEngine.RectTransform(); }
    public sealed class ClickEvent { private event Action handlers; public void AddListener(Action h) { handlers+=h; } public void Invoke() { handlers?.Invoke(); } }
    public sealed class Button {
        public bool interactable = true; public readonly ClickEvent onClick = new ClickEvent();
        public void Press() { if(interactable) onClick.Invoke(); }
    }
}
public enum PAIEAGDLCBJ { Top }
public class BaseUI : UnityEngine.MonoBehaviour {
    public virtual PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;
    protected virtual void Init() { } protected virtual void BeforeOpen() { } protected virtual void AfterOpen() { } protected virtual void BeforeClose() { }
    public void TestOpen() { BeforeOpen(); AfterOpen(); } public void TestClose() { BeforeClose(); }
}
public sealed class MgrUI {
    public static readonly MgrUI Instance = new MgrUI(); public Action OnClose;
    public void Open(string path) { } public void Close(string path,bool animated) { OnClose?.Invoke(); }
}
public static class MCCIJBJGMCK { public static bool IsLock() { return false; } }
namespace FruitWithdrawal {
    // Real Engine + binary clone, with an injectable clock. Reproduces synchronous Changed publication.
    public sealed class WithdrawalService {
        public static WithdrawalService Instance;
        private WithdrawalEngine engine;
        public long Utc = 100000;
        public bool IsAvailable = true;
        public string LastError => engine.LastError;
        public WithdrawalRules Rules => engine.Rules;
        public WithdrawalState State => engine.State;
        public event Action Changed;
        public WithdrawalService(WithdrawalRules rules,WithdrawalState state) { engine=new WithdrawalEngine(rules,state); engine.Tick(Utc); Instance=this; }
        private bool Mutate(Func<WithdrawalEngine,bool> action) {
            var candidate=new WithdrawalEngine(engine.Rules,WithdrawalStorage.Clone(engine.State));
            bool changed=action(candidate); if(!changed)return false;
            engine=candidate; Changed?.Invoke(); return true;
        }
        public bool Tick() { return Mutate(e=>e.Tick(Utc)); }
        public bool Apply(string tier,string channel) { return Mutate(e=>e.Apply(tier,channel,Utc)); }
        public bool CancelRequest(string id) { return Mutate(e=>e.CancelRequest(id,Utc)); }
        public bool ResolveSimulation(string id,bool approved) { return Mutate(e=>e.ResolveSimulation(id,approved,Utc)); }
        public bool ClaimAllRewards() { return Mutate(e=>e.ClaimAllRewards(Utc)); }
        public long GetRemainingReviewSeconds() { return engine.GetRemainingReviewSeconds(Utc); }
        public WithdrawalConditionProgress[] GetConditions(string tier) { return engine.GetConditions(tier); }
    }
}
