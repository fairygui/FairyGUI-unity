using UnityEngine;
using FairyGUI;

public class ParticlesMain : MonoBehaviour
{
    GComponent _mainView;

    void Awake()
    {
        UIPackage.AddPackage("UI/Particles");

        UIObjectFactory.SetPackageItemExtension("ui://Particles/CoolComponent", typeof(CoolComponent));
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        Object prefab = Resources.Load("Flame");
        GameObject go = (GameObject)Object.Instantiate(prefab);
        _mainView.GetChild("holder").asGraph.SetNativeObject(new GoWrapper(go));

        _mainView.GetChild("c0").draggable = true;
        _mainView.GetChild("c1").draggable = true;
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}