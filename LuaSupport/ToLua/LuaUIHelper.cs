using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using LuaInterface;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class LuaUIHelper
	{
		static Dictionary<string, LuaFunction> packageItemExtensions = new Dictionary<string, LuaFunction>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="luaClass"></param>
		public static void SetExtension(string url, System.Type baseType, LuaFunction extendFunction)
		{
			UIObjectFactory.SetPackageItemExtension(url, baseType);
			packageItemExtensions[url] = extendFunction;
		}

		[NoToLuaAttribute]
		public static void ConnectLua(GComponent gcom)
		{
			LuaFunction extendFunction;
			if (LuaUIHelper.packageItemExtensions.TryGetValue(gcom.resourceURL, out extendFunction))
			{
				extendFunction.BeginPCall();
				extendFunction.Push(gcom);
				extendFunction.PCall();
				extendFunction.EndPCall();
			}
		}
	}

	public class GLuaComponent : GComponent
	{
		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class GLuaLabel : GLabel
	{
		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class GLuaButton : GButton
	{
		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class GLuaProgressBar : GProgressBar
	{
		public LuaTable luaObj;

		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class GLuaSlider : GSlider
	{
		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class GLuaComboBox : GComboBox
	{
		[NoToLuaAttribute]
		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			LuaUIHelper.ConnectLua(this);
		}
	}

	public class LuaWindow : Window
	{
		LuaFunction _OnInit;
		LuaFunction _DoHideAnimation;
		LuaFunction _DoShowAnimation;
		LuaFunction _OnShown;
		LuaFunction _OnHide;

		public void ConnectLua(LuaTable luaObj)
		{
			_OnInit = luaObj.GetLuaFunction("OnInit");
			_DoHideAnimation = luaObj.GetLuaFunction("DoHideAnimation");
			_DoShowAnimation = luaObj.GetLuaFunction("DoShowAnimation");
			_OnShown = luaObj.GetLuaFunction("OnShown");
			_OnHide = luaObj.GetLuaFunction("OnHide");
		}

		protected override void OnInit()
		{
			if (_OnInit != null)
			{
				_OnInit.BeginPCall();
				_OnInit.Push(this);
				_OnInit.PCall();
				_OnInit.EndPCall();
			}
		}

		protected override void DoHideAnimation()
		{
			if (_DoHideAnimation != null)
			{
				_DoHideAnimation.BeginPCall();
				_DoHideAnimation.Push(this);
				_DoHideAnimation.PCall();
				_DoHideAnimation.EndPCall();
			}
			else
				base.DoHideAnimation();
		}

		protected override void DoShowAnimation()
		{
			if (_DoShowAnimation != null)
			{
				_DoShowAnimation.BeginPCall();
				_DoShowAnimation.Push(this);
				_DoShowAnimation.PCall();
				_DoShowAnimation.EndPCall();
			}
			else
				base.DoShowAnimation();
		}

		protected override void OnShown()
		{
			base.OnShown();

			if (_OnShown != null)
			{
				_OnShown.BeginPCall();
				_OnShown.Push(this);
				_OnShown.PCall();
				_OnShown.EndPCall();
			}
		}

		protected override void OnHide()
		{
			base.OnHide();

			if (_OnHide != null)
			{
				_OnHide.BeginPCall();
				_OnHide.Push(this);
				_OnHide.PCall();
				_OnHide.EndPCall();
			}
		}
	}
}