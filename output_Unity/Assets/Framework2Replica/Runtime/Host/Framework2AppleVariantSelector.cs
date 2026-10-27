using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>B skin's approved two-step selector. Does not store progress or switch scenes.</summary>
public sealed class Framework2AppleVariantSelector : MonoBehaviour
{
    [SerializeField] private Button confirm;
    [SerializeField] private Image cardA,cardB;
    [SerializeField] private GameObject checkA,checkB;
    private bool pending;
    private Action<bool> apply;
    public Button ConfirmButton=>confirm;
    public void Configure(Button button,Image a,Image b,GameObject ca,GameObject cb)
    {confirm=button;cardA=a;cardB=b;checkA=ca;checkB=cb;}
    public void Bind(Action<bool> callback)
    {apply=callback;confirm.onClick.AddListener(Confirm);}
    public void ResetSelection(bool useB,bool available)
    {pending=useB;confirm.interactable=available;Refresh();}
    public void Choose(bool useB){pending=useB;Refresh();}
    private void Refresh()
    {
        checkA.SetActive(!pending);checkB.SetActive(pending);
        cardA.color=pending?Color.white:new Color(.87f,1f,.75f);
        cardB.color=pending?new Color(.87f,1f,.75f):Color.white;
    }
    private void Confirm(){apply?.Invoke(pending);}
}
