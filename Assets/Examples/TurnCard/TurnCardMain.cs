using UnityEngine;
using FairyGUI;

public class TurnCardMain : MonoBehaviour
{
    GComponent _mainView;
    Card _c0;
    Card _c1;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        UIPackage.AddPackage("UI/TurnCard");
        UIObjectFactory.SetPackageItemExtension("ui://TurnCard/CardComponent", typeof(Card));
    }

    void Start()
    {
        _mainView = this.GetComponent<UIPanel>().ui;

        _c0 = (Card)_mainView.GetChild("c0");

        _c1 = (Card)_mainView.GetChild("c1");
        _c1.SetPerspective();
        
        _c0.onClick.Add(__clickCard);
        _c1.onClick.Add(__clickCard);
    }

    void __clickCard(EventContext context)
    {
        Card card = (Card)context.sender;
        card.Turn();
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}