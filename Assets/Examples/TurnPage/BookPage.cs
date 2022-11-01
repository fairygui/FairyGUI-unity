using UnityEngine;
using FairyGUI;

class BookPage : GComponent
{
    Controller _style;
    GoWrapper _modelWrapper;
    GObject _pageNumber;

    public override void ConstructFromXML(FairyGUI.Utils.XML xml)
    {
        base.ConstructFromXML(xml);
        
        _style = GetController("style");

        _pageNumber = GetChild("pn");

        _modelWrapper = new GoWrapper();
        GetChild("model").asGraph.SetNativeObject(_modelWrapper);
    }

    public void render(int pageIndex)
    {
        _pageNumber.text = (pageIndex + 1).ToString();

        if (pageIndex == 0)
            _style.selectedIndex = 0; //pic page
        else if (pageIndex == 2)
        {
            if (_modelWrapper.wrapTarget == null)
            {
                Object prefab = Resources.Load("Role/npc3");
                GameObject go = (GameObject)Object.Instantiate(prefab);
                go.transform.localPosition = new Vector3(0, 0, 1000);
                go.transform.localScale = new Vector3(120, 120, 120);
                go.transform.localEulerAngles = new Vector3(0, 100, 0);

                _modelWrapper.SetWrapTarget(go, true);
            }

            _style.selectedIndex = 2; //show a model
        }
        else
            _style.selectedIndex = 1; //empty page
    }
}
