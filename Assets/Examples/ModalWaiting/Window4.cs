using FairyGUI;

public class Window4 : Window
{
    public Window4()
    {
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("ModalWaiting", "TestWin").asCom;
        this.contentPane.GetChild("n1").onClick.Add(OnClick);
    }

    void OnClick()
    {
        this.ShowModalWait();
        Timers.inst.Add(3, 1, (object param) => { this.CloseModalWait(); });
    }
}
