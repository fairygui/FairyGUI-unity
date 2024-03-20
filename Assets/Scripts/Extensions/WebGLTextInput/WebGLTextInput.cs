#if UNITY_WEBGL && FAIRYGUI_WEBGL_TEXT_INPUT

using System;
using System.Runtime.InteropServices;
using AOT;
using FairyGUI;
using UnityEngine;

public static class WebGLTextInput
{
    static WebGLTextInput()
    {
        WebGLTextInputInit(OnInput, OnBlur);
    }

    public static void Start(InputTextField target)
    {
        WebGLTextInputSetText(target.text,
            !target.textField.singleLine,
            ColorUtility.ToHtmlStringRGBA(target.textFormat.color),
            target.textFormat.size,
            target.textFormat.font,
            target.maxLength);

        WebGLInput.captureAllKeyboardInput = false;

        SyncTransform(target);
    }

    public static void Stop()
    {
        WebGLTextInputHide();
    }

    public static void SyncTransform(InputTextField target)
    {
        Rect rect = target.TransformRect(new Rect(0, 0, target.width, target.height), null);
        rect.min = StageCamera.main.WorldToScreenPoint(rect.min);
        rect.max = StageCamera.main.WorldToScreenPoint(rect.max);
        rect.y = Screen.height - rect.y - rect.height;

        WebGLTextInputShow(rect.x, rect.y, target.width, target.height,
            rect.width / target.width, rect.height / target.height);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    static void OnInput(string value)
    {
        var focus = Stage.inst.focus as InputTextField;
        if (focus != null)
            focus.ReplaceText(value);
    }

    [MonoPInvokeCallback(typeof(Action))]
    static void OnBlur()
    {
        WebGLInput.captureAllKeyboardInput = true;

        var focus = Stage.inst.focus as InputTextField;
        if (focus != null)
            Stage.inst.SetFocus(null, true);
    }

    [DllImport("__Internal")]
    public static extern void WebGLTextInputInit(Action<string> onInputCallback, Action onBlurCallback);

    [DllImport("__Internal")]
    public static extern void WebGLTextInputSetText(string text, bool multiline, string color, int fontSize, string fontFace, int maxLength);

    [DllImport("__Internal")]
    public static extern void WebGLTextInputShow(float x, float y, float width, float height, float scaleX, float scaleY);

    [DllImport("__Internal")]
    public static extern void WebGLTextInputHide();
}

#endif