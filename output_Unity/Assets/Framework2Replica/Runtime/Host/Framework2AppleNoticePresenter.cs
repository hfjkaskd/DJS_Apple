using UnityEngine;
using UnityEngine.UI;
using WZSDK;

// Optional dismissal and read-only amount presentation for the authored B notice.
// The source NoticeView still owns its original automatic dismissal timer.
public sealed class Framework2AppleNoticePresenter : MonoBehaviour
{
    [SerializeField] Button confirm,close;
    [SerializeField] GameObject amountRoot;
    [SerializeField] Text amount,message;
    [SerializeField] Vector2 genericPosition,genericSize,amountPosition,amountSize;
    public void Configure(Button primary,Button exit,GameObject valueRoot,Text value,Text body,Vector2 normalPosition,Vector2 normalSize)
    {
        confirm=primary;close=exit;amountRoot=valueRoot;amount=value;message=body;
        genericPosition=normalPosition;genericSize=normalSize;
        amountPosition=body.rectTransform.anchoredPosition;amountSize=body.rectTransform.sizeDelta;
    }
    void OnEnable(){confirm.onClick.AddListener(Dismiss);close.onClick.AddListener(Dismiss);}
    void OnDisable(){confirm.onClick.RemoveListener(Dismiss);close.onClick.RemoveListener(Dismiss);}
    void Dismiss()=>UIManager.instance?.CloseView(EUIType.NoticeView);
    public void Present(object[] args)
    {
        bool hasAmount=args!=null && args.Length>1 && args[1] is double;
        amountRoot.SetActive(hasAmount);
        message.rectTransform.anchoredPosition=hasAmount?amountPosition:genericPosition;
        message.rectTransform.sizeDelta=hasAmount?amountSize:genericSize;
        if(hasAmount && GameManagerWZ.instance!=null)amount.text=GameManagerWZ.instance.FormatRegionalValue((double)args[1]);
    }
}
