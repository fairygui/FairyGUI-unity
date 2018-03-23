using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StageEngine : MonoBehaviour
	{
		public int ObjectsOnStage;
		public int GraphicsOnStage;

		public static bool beingQuit;

		void LateUpdate()
		{
			Stage.inst.InternalUpdate();

			ObjectsOnStage = Stats.ObjectCount;
			GraphicsOnStage = Stats.GraphicsCount;
		}

#if !NO_ONGUI && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        void Start()
        {
            useGUILayout = false;
        }

        void OnGUI()
		{
			Stage.inst.HandleGUIEvents(Event.current);
		}
#else
        private Coroutine _EmulateCoroutine = null;
        private List<KeyCode> _allKeyCode = new List<KeyCode>();
        private List<KeyCode> _downKey = new List<KeyCode>();
        private List<EmulateGUIEvent> _eventQ = new List<EmulateGUIEvent>();
        private void Start()
        {
            useGUILayout = false;
            foreach(var kc in Enum.GetValues(typeof(KeyCode)))
            {
                _allKeyCode.Add((KeyCode)kc);
            }
            _EmulateCoroutine = StartCoroutine(DoEmulateGUIEvent());
        }

        private EventModifiers modifiers
        {
            get
            {
                uint mod = 0;
                foreach(var downkeycode in _downKey)
                {
                    if (downkeycode == KeyCode.LeftShift || downkeycode == KeyCode.RightShift)
                    {
                        mod = (mod | (uint)EventModifiers.Shift);
                    }
                    if (downkeycode == KeyCode.LeftControl || downkeycode == KeyCode.RightControl)
                    {
                        mod = (mod | (uint)EventModifiers.Control);
                    }
                    if (downkeycode == KeyCode.LeftAlt || downkeycode == KeyCode.RightAlt)
                    {
                        mod = (mod | (uint)EventModifiers.Alt);
                    }
                }
                
                return (EventModifiers)mod;
            }
        }

        IEnumerator DoEmulateGUIEvent()
        {
            var yieldwait = new UnityEngine.WaitForEndOfFrame();
            do 
            {
                yield return yieldwait;

                // 为了将GUIEvent推到LateUpdate之后
                foreach(var ege in _eventQ)
                {
                    Stage.inst.HandleEmulatedEvents(ege);
                }
                _eventQ.Clear();

            } while (true);
        }

        private void OnDestroy()
        {
            if (_EmulateCoroutine != null)
            {
                StopCoroutine(_EmulateCoroutine);
                _EmulateCoroutine = null;
            }
        }

        private void Update()
        {
            bool isBackspace = false;
            bool isReturn = false;
            foreach(var kc in _allKeyCode)
            {
                var isDown = false;
                foreach (var downkeycode in _downKey)
                {
                    if (downkeycode == kc)
                    {
                        isDown = true;
                        break;
                    }
                }
                if (Input.GetKey(kc))
                {
                    if (!isDown)
                    {
                        // 按下
                        _downKey.Add(kc);
                        var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.KeyDown,
                            kc,
                            modifiers,
                            '\0',
                            Vector2.zero
                            );
                        _eventQ.Add(ege);
                        isBackspace = kc == KeyCode.Backspace;
                        isReturn = kc == KeyCode.Return;
                    }
                }
                else
                {
                    if (isDown)
                    {
                        // 弹起
                        _downKey.Remove(kc);
                        var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.KeyUp,
                            kc,
                            modifiers,
                            '\0',
                            Vector2.zero
                            );
                        _eventQ.Add(ege);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Input.inputString))
            {
                char character = '\0';
                bool isBackspaced = false;
                bool isReturned = false;
                foreach (var c in Input.inputString)
                {
                    if (c != '\b' && c != '\r' && c != '\n')
                    {
                        character = c;
                    }
                    else
                    {
                        isBackspaced = (c == '\b') && !isBackspace;
                        isReturned = (c == '\r' || c == '\n') && !isReturn;
                    }
                }
                if (isBackspaced)
                {
                    var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.KeyDown,
                            KeyCode.Backspace,
                            modifiers,
                            '\0',
                            Vector2.zero
                            );
                    _eventQ.Add(ege);
                }
                if (isReturned)
                {
                    var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.KeyDown,
                            KeyCode.Return,
                            modifiers,
                            '\0',
                            Vector2.zero
                            );
                    _eventQ.Add(ege);
                }

                if (character != '\0')
                {
                    var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.KeyDown,
                            KeyCode.None,
                            modifiers,
                            character,
                            Vector2.zero
                            );
                    _eventQ.Add(ege);
                }
            }

            if (Input.mouseScrollDelta.sqrMagnitude > 0.001f)
            {
                // 滚轮
                var ege = new EmulateGUIEvent(
                            EmulateGUIEventType.scrollWheel,
                            KeyCode.None,
                            modifiers,
                            '\0',
                            Vector2.zero - Input.mouseScrollDelta
                            );
                _eventQ.Add(ege);
            }
        }
#endif


#if !UNITY_5_4_OR_NEWER
		void OnLevelWasLoaded()
		{
			StageCamera.CheckMainCamera();
		}
#endif

        void OnApplicationQuit()
		{
			beingQuit = true;

			if (Application.isEditor)
				UIPackage.RemoveAllPackages(true);
		}
	}
}