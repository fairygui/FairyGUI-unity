using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/*
	 * 当使用DLL形式的插件时，因为DLL默认是为移动平台编译的，所以不支持复制粘贴。将这个脚本放到工程里，并在游戏启动时调用CopyPastePatch.Apply()，可以在PC平台激活复制粘贴功能
	 */
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
	public class CopyPastePatch
	{
		public static void Apply()
		{
			Stage.inst.onCopy.Clear();
			Stage.inst.onCopy.Add(OnCopy);

			Stage.inst.onPaste.Clear();
			Stage.inst.onPaste.Add(OnPaste);
		}

		static void OnCopy(EventContext context)
		{
			TextEditor te = new TextEditor();
#if UNITY_5_3_OR_NEWER
			te.text = (string)context.data;
#else
			te.content = new GUIContent((string)context.data);
#endif
			te.OnFocus();
			te.Copy();
		}

		static void OnPaste(EventContext context)
		{
			TextField target = (TextField)context.data;
			TextEditor te = new TextEditor();
			te.Paste();
#if UNITY_5_3_OR_NEWER
			string value = te.text;
#else
			string value = te.content.text;
#endif
			if (!string.IsNullOrEmpty(value))
				target.ReplaceSelection(value);
		}
	}
#endif
}
