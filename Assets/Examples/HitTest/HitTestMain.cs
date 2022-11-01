using UnityEngine;
using FairyGUI;

public class HitTestMain : MonoBehaviour
{
    Transform cube;

    void Start()
    {
        Application.targetFrameRate = 60;

        cube = GameObject.Find("Cube").transform;

        Stage.inst.onTouchBegin.Add(OnTouchBegin);
    }

    void OnTouchBegin()
    {
        if (!Stage.isTouchOnUI)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Stage.inst.touchPosition.x, Screen.height - Stage.inst.touchPosition.y));
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == cube)
                {
                    Debug.Log("Hit the cube");
                }
            }
        }
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}