using UnityEngine;
using FairyGUI;

public class CoolComponent : GComponent
{
    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);

        GGraph graph = this.GetChild("effect").asGraph;

        Object prefab = Resources.Load("Flame");
        GameObject go = (GameObject)Object.Instantiate(prefab);
        graph.SetNativeObject(new GoWrapper(go));
    }
}
