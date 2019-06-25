using FairyGUI;
using UnityEngine;

public class GlobalWaiting : GComponent
{
	GObject _obj;

	public override void ConstructFromXML(FairyGUI.Utils.XML xml)
	{
		base.ConstructFromXML(xml);

		_obj = this.GetChild("n1");
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		float i = _obj.rotation;
		i += 5;
		if (i > 360)
			i = i % 360;
		_obj.rotation = i;
	}
}
