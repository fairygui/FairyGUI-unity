using UnityEngine;
using FairyGUI;

public class RenderTextureMain : MonoBehaviour
{
    GComponent _mainView;
    Window3 _testWin;

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;

        _testWin = new Window3();

        _mainView.GetChild("n2").onClick.Add(() => { _testWin.Show();  });
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}