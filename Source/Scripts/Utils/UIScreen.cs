using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// UI渲染目标屏幕信息的接口，可以自己实现
    /// </summary>
    public interface IUIScreen
    {
        int width { get; }
        int height { get; }
        float dpi { get; }
    }

    /// <summary>
    /// 默认使用Screen计算大小
    /// </summary>
    public class ScreenUIScreen : IUIScreen
    {
        public int width { get { return Screen.width; } }
        public int height { get { return Screen.height; } }
        public float dpi { get { return Screen.dpi; } }
    }

    /// <summary>
    /// 默认使用摄像机视口计算大小
    /// </summary>
    public class CameraUIScreen : IUIScreen
    {
        public int width { get { return StageCamera.main != null ? StageCamera.main.pixelWidth : 0; } }
        public int height { get { return StageCamera.main != null ? StageCamera.main.pixelHeight : 0; } }
        public float dpi { get { return Screen.dpi; } }
    }

    public static class UIScreen
    {
        static UIScreen()
        {
            FULL_VIEW_SCREEN = new ScreenUIScreen();
            CAMERA_VIEW_SCREEN = new CameraUIScreen();
            innerScreenImpl = FULL_VIEW_SCREEN;
        }

        public static IUIScreen FULL_VIEW_SCREEN;
        public static IUIScreen CAMERA_VIEW_SCREEN;
        public static IUIScreen innerScreenImpl { private get; set; }

        public static int width
        {
            get
            {
                return innerScreenImpl.width;
            }
        }

        public static int height
        {
            get
            {
                return innerScreenImpl.height;
            }
        }

        public static float dpi
        {
            get
            {
                return innerScreenImpl.dpi;
            }
        }

        /// <summary>
        /// Left-Bottom zero Screen coord to boxed coord
        /// </summary>
        public static Vector3 ScreenPosToUIScreenPos(Vector3 screenPos)
        {
            if (innerScreenImpl == FULL_VIEW_SCREEN)
                return screenPos;

            var deltaX = Screen.width - innerScreenImpl.width;
            var deltaY = Screen.height - innerScreenImpl.height;
            return new Vector3(screenPos.x - Mathf.CeilToInt((float)deltaX / 2), screenPos.y - Mathf.CeilToInt((float)deltaY / 2), screenPos.z);
        }

        /// <summary>
        /// Left-Bottom zero boxed coord to Screen coord
        /// </summary>
        public static Vector3 UIScreenPosToScreenPos(Vector3 uiScreenPos)
        {
            if (innerScreenImpl == FULL_VIEW_SCREEN)
                return uiScreenPos;

            var deltaX = Screen.width - innerScreenImpl.width;
            var deltaY = Screen.height - innerScreenImpl.height;
            return new Vector3(uiScreenPos.x + Mathf.CeilToInt((float)deltaX / 2), uiScreenPos.y + Mathf.CeilToInt((float)deltaY / 2), uiScreenPos.z);
        }
    }
}