using System;
using FairyGUI.Utils;

namespace FairyGUI
{
	public class ControllerAction
	{
		public string[] fromPage;
		public string[] toPage;

		public static ControllerAction CreateAction(string type)
		{
			switch (type)
			{
				case "play_transition":
					return new PlayTransitionAction();

				case "change_page":
					return new ChangePageAction();
			}
			return null;
		}

		public ControllerAction()
		{
		}

		public void Run(Controller controller, string prevPage, string curPage)
		{
			if ((fromPage == null || fromPage.Length == 0 || Array.IndexOf(fromPage, prevPage) != -1)
				&& (toPage == null || toPage.Length == 0 || Array.IndexOf(toPage, curPage) != -1))
				Enter(controller);
			else
				Leave(controller);
		}

		virtual protected void Enter(Controller controller)
		{

		}

		virtual protected void Leave(Controller controller)
		{

		}

		virtual public void Setup(XML xml)
		{
			fromPage = xml.GetAttributeArray("fromPage");
			toPage = xml.GetAttributeArray("toPage");
		}
	}
}
