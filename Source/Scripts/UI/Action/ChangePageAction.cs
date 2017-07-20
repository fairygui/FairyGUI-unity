using FairyGUI.Utils;

namespace FairyGUI
{
	public class ChangePageAction : ControllerAction
	{
		public string objectId;
		public string controllerName;
		public string targetPage;

		public ChangePageAction()
		{
		}

		override protected void Enter(Controller controller)
		{
			if (string.IsNullOrEmpty(controllerName))
				return;

			GComponent gcom;
			if (!string.IsNullOrEmpty(objectId))
				gcom = controller.parent.GetChildById(objectId) as GComponent;
			else
				gcom = controller.parent;
			if (gcom != null)
			{
				Controller cc = gcom.GetController(controllerName);
				if (cc != null && cc != controller && !cc.changing)
					cc.selectedPageId = targetPage;
			}
		}

		override public void Setup(XML xml)
		{
			base.Setup(xml);

			objectId = xml.GetAttribute("objectId");
			controllerName = xml.GetAttribute("controller");
			targetPage = xml.GetAttribute("targetPage");
		}
	}
}
