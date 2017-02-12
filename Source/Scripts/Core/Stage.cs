using System.Collections.Generic;
using UnityEngine;
using System.Text;
using FairyGUI.Utils;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Stage : Container
	{
		/// <summary>
		/// 
		/// </summary>
		public static bool touchScreen { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public static bool keyboardInput { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int stageHeight { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int stageWidth { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public float soundVolume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onStageResized { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchMove { get; private set; }

		DisplayObject _touchTarget;
		DisplayObject _focused;
		UpdateContext _updateContext;
		List<DisplayObject> _rollOutChain;
		List<DisplayObject> _rollOverChain;
		TouchInfo[] _touches;
		int _touchCount;
		Vector2 _touchPosition;
		int _frameGotHitTarget;
		int _frameGotTouchPosition;
		bool _customInput;
		Vector2 _customInputPos;
		bool _customInputButtonDown;

		EventCallback1 _focusRemovedDelegate;

		AudioSource _audio;

#pragma warning disable 0649
		IKeyboard _keyboard;
#pragma warning restore 0649

		List<NTexture> _toCollectTextures = new List<NTexture>();

		static Stage _inst;
		/// <summary>
		/// 
		/// </summary>
		public static Stage inst
		{
			get
			{
				if (_inst == null)
					Instantiate();

				return _inst;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void Instantiate()
		{
			if (_inst == null)
			{
				_inst = new Stage();
				GRoot._inst = new GRoot();
				GRoot._inst.ApplyContentScaleFactor();
				_inst.AddChild(GRoot._inst.displayObject);

				StageCamera.CheckMainCamera();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Stage()
			: base()
		{
			_inst = this;
			soundVolume = 1;

			_updateContext = new UpdateContext();
			stageWidth = Screen.width;
			stageHeight = Screen.height;
			_frameGotHitTarget = -1;

			if (Application.platform == RuntimePlatform.WindowsPlayer
				|| Application.platform == RuntimePlatform.WindowsEditor
				|| Application.platform == RuntimePlatform.OSXPlayer
				|| Application.platform == RuntimePlatform.OSXEditor)
				touchScreen = false;
			else
				touchScreen = Input.touchSupported && SystemInfo.deviceType != DeviceType.Desktop;

			_touches = new TouchInfo[5];
			for (int i = 0; i < _touches.Length; i++)
				_touches[i] = new TouchInfo();

			if (!touchScreen)
				_touches[0].touchId = 0;

			_rollOutChain = new List<DisplayObject>();
			_rollOverChain = new List<DisplayObject>();

			onStageResized = new EventListener(this, "onStageResized");
			onTouchMove = new EventListener(this, "onTouchMove");

			StageEngine engine = GameObject.FindObjectOfType<StageEngine>();
			if (engine != null)
				Object.Destroy(engine.gameObject);

			this.gameObject.name = "Stage";
			this.gameObject.layer = LayerMask.NameToLayer(StageCamera.LayerName);
			this.gameObject.AddComponent<StageEngine>();
			this.gameObject.AddComponent<UIContentScaler>();
			this.gameObject.SetActive(true);
			Object.DontDestroyOnLoad(this.gameObject);

			this.cachedTransform.localScale = new Vector3(StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel);

			EnableSound();

			if (touchScreen)
			{
#if !(UNITY_WEBPLAYER || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
				_keyboard = new FairyGUI.TouchScreenKeyboard();
				keyboardInput = true;
#endif
			}

			Timers.inst.Add(5, 0, RunTextureCollector);

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
#endif
			_focusRemovedDelegate = OnFocusRemoved;
		}

#if UNITY_5_4_OR_NEWER
		void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
		{
			StageCamera.CheckMainCamera();
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject touchTarget
		{
			get
			{
				if (_frameGotHitTarget != Time.frameCount)
					GetHitTarget();

				if (_touchTarget == this)
					return null;
				else
					return _touchTarget;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool isTouchOnUI
		{
			get
			{
				return _inst != null && _inst.touchTarget != null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject focus
		{
			get
			{
				if (_focused != null && _focused.isDisposed)
					_focused = null;
				return _focused;
			}
			set
			{
				if (_focused == value)
					return;

				DisplayObject oldFocus = _focused;
				_focused = value;
				if (_focused == this)
					_focused = null;

				if (oldFocus != null)
				{
					if (oldFocus is InputTextField)
						((InputTextField)oldFocus).onFocusOut.Call();

					oldFocus.onRemovedFromStage.RemoveCapture(_focusRemovedDelegate);
				}

				if (_focused != null)
				{
					if (_focused is InputTextField)
						((InputTextField)_focused).onFocusIn.Call();

					_focused.onRemovedFromStage.AddCapture(_focusRemovedDelegate);
				}
			}
		}

		void OnFocusRemoved(EventContext context)
		{
			if (_focused == null)
				return;

			if (context.sender == _focused)
				this.focus = null;
			else
			{
				DisplayObject currentObject = _focused.parent;
				while (currentObject != null)
				{
					if (currentObject == context.sender)
					{
						this.focus = null;
						break;
					}
					currentObject = currentObject.parent;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 touchPosition
		{
			get
			{
				UpdateTouchPosition();
				return _touchPosition;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		/// <returns></returns>
		public Vector2 GetTouchPosition(int touchId)
		{
			UpdateTouchPosition();

			if (touchId < 0)
				return _touchPosition;

			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId == touchId)
					return new Vector2(touch.x, touch.y);
			}

			return _touchPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		public int touchCount
		{
			get { return _touchCount; }
		}

		public int[] GetAllTouch(int[] result)
		{
			if (result == null)
				result = new int[_touchCount];
			int i = 0;
			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId != -1)
				{
					result[i++] = touch.touchId;
					if (i >= result.Length)
						break;
				}
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ResetInputState()
		{
			for (int j = 0; j < 5; j++)
				_touches[j].Reset();

			if (!touchScreen)
				_touches[0].touchId = 0;

			_touchCount = 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		public void CancelClick(int touchId)
		{
			for (int j = 0; j < 5; j++)
			{
				TouchInfo touch = _touches[j];
				if (touch.touchId == touchId)
					touch.clickCancelled = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void EnableSound()
		{
			if (_audio == null)
			{
				_audio = gameObject.AddComponent<AudioSource>();
				_audio.bypassEffects = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void DisableSound()
		{
			if (_audio != null)
			{
				Object.DestroyObject(_audio);
				_audio = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volumeScale"></param>
		public void PlayOneShotSound(AudioClip clip, float volumeScale)
		{
			if (_audio != null && this.soundVolume > 0)
				_audio.PlayOneShot(clip, volumeScale * this.soundVolume);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		public void PlayOneShotSound(AudioClip clip)
		{
			if (_audio != null && this.soundVolume > 0)
				_audio.PlayOneShot(clip, this.soundVolume);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <param name="autocorrection"></param>
		/// <param name="multiline"></param>
		/// <param name="secure"></param>
		/// <param name="alert"></param>
		/// <param name="textPlaceholder"></param>
		/// <param name="keyboardType"></param>
		/// <param name="hideInput"></param>
		public void OpenKeyboard(string text, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder, int keyboardType, bool hideInput)
		{
			if (_keyboard != null)
				_keyboard.Open(text, autocorrection, multiline, secure, alert, textPlaceholder, keyboardType, hideInput);
		}

		/// <summary>
		/// 
		/// </summary>
		public void CloseKeyboard()
		{
			if (_keyboard != null)
				_keyboard.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="screenPos"></param>
		/// <param name="buttonDown"></param>
		public void SetCustomInput(Vector2 screenPos, bool buttonDown)
		{
			_customInput = true;
			_customInputButtonDown = buttonDown;
			_customInputPos = screenPos;
			_frameGotHitTarget = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="screenPos"></param>
		/// <param name="buttonDown"></param>
		/// <param name="buttonUp"></param>
		public void SetCustomInput(Vector2 screenPos, bool buttonDown, bool buttonUp)
		{
			_customInput = true;
			if (buttonDown)
				_customInputButtonDown = true;
			else if (buttonUp)
				_customInputButtonDown = false;
			_customInputPos = screenPos;
			_frameGotHitTarget = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hit"></param>
		/// <param name="buttonDown"></param>
		public void SetCustomInput(ref RaycastHit hit, bool buttonDown)
		{
			Vector2 screenPos = Camera.main.WorldToScreenPoint(hit.point);
			HitTestContext.CacheRaycastHit(Camera.main, ref hit);
			SetCustomInput(screenPos, buttonDown);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hit"></param>
		/// <param name="buttonDown"></param>
		/// <param name="buttonUp"></param>
		public void SetCustomInput(ref RaycastHit hit, bool buttonDown, bool buttonUp)
		{
			Vector2 screenPos = Camera.main.WorldToScreenPoint(hit.point);
			HitTestContext.CacheRaycastHit(Camera.main, ref hit);
			SetCustomInput(screenPos, buttonDown, buttonUp);
		}

		internal void InternalUpdate()
		{
			HandleEvents();

			_updateContext.Begin();
			Update(_updateContext);
			_updateContext.End();

			if (DynamicFont.textRebuildFlag)
			{
				//字体贴图更改了，重新渲染一遍，防止本帧文字显示错误
				_updateContext.Begin();
				Update(_updateContext);
				_updateContext.End();

				DynamicFont.textRebuildFlag = false;
			}
		}

		void GetHitTarget()
		{
			if (_frameGotHitTarget == Time.frameCount)
				return;

			_frameGotHitTarget = Time.frameCount;

			if (_customInput)
			{
				Vector2 pos = _customInputPos;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = _touches[0];
				_touchTarget = HitTest(pos, true);
				touch.target = _touchTarget;
			}
			else if (touchScreen)
			{
				for (int i = 0; i < Input.touchCount; ++i)
				{
					Touch uTouch = Input.GetTouch(i);
					if (uTouch.phase == TouchPhase.Stationary)
						continue;

					Vector2 pos = uTouch.position;
					pos.y = stageHeight - pos.y;

					_touchTarget = null;
					TouchInfo touch = null;
					for (int j = 0; j < 5; j++)
					{
						if (_touches[j].touchId == uTouch.fingerId)
						{
							touch = _touches[j];
							break;
						}

						if (_touches[j].touchId == -1)
						{
							touch = _touches[j];
							//下面的赋值避免了touchMove在touchDown前触发
							touch.x = uTouch.position.x;
							touch.y = stageHeight - uTouch.position.y;
						}
					}
					if (touch == null)
						return;

					touch.touchId = uTouch.fingerId;
					DisplayObject ht = HitTest(pos, true);
					touch.target = ht;
					if (ht != null)
						_touchTarget = ht;
				}
			}
			else
			{
				Vector2 pos = Input.mousePosition;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = _touches[0];
				if (pos.x < 0 || pos.y < 0)
				{
					pos.x = touch.x;
					pos.y = touch.y;
				}

				_touchTarget = HitTest(pos, true);
				touch.target = _touchTarget;
			}

			HitTestContext.ClearRaycastHitCache();
		}

		internal void HandleScreenSizeChanged()
		{
			stageWidth = Screen.width;
			stageHeight = Screen.height;

			this.cachedTransform.localScale = new Vector3(StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel, StageCamera.UnitsPerPixel);

			UIContentScaler scaler = this.gameObject.GetComponent<UIContentScaler>();
			scaler.ApplyChange();
			GRoot.inst.ApplyContentScaleFactor();

			onStageResized.Call();
		}

		internal void HandleGUIEvents(Event evt)
		{
			if (evt.rawType == EventType.KeyDown && evt.keyCode != KeyCode.None)
			{
				TouchInfo touch = _touches[0];
				touch.keyCode = evt.keyCode;
				touch.modifiers = evt.modifiers;
				InputEvent.shiftDown = (evt.modifiers & EventModifiers.Shift) != 0;

				touch.UpdateEvent();
				DisplayObject f = this.focus;
				if (f != null)
					f.onKeyDown.BubbleCall(touch.evt);
				else
					this.onKeyDown.Call(touch.evt);
			}
			else if (evt.rawType == EventType.KeyUp)
			{
				TouchInfo touch = _touches[0];
				touch.modifiers = evt.modifiers;
			}
			else if (evt.type == EventType.scrollWheel)
			{
				if (_touchTarget != null)
				{
					TouchInfo touch = _touches[0];
					touch.mouseWheelDelta = (int)evt.delta.y;
					touch.UpdateEvent();
					_touchTarget.onMouseWheel.BubbleCall(touch.evt);
				}
			}
		}

		void HandleEvents()
		{
			GetHitTarget();

			if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
				InputEvent.shiftDown = false;
			else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				InputEvent.shiftDown = true;

			UpdateTouchPosition();

			if (_customInput)
			{
				HandleCustomInput();
				_customInput = false;
			}
			else if (touchScreen)
				HandleTouchEvents();
			else
				HandleMouseEvents();

			if (_focused is InputTextField)
				HandleTextInput();
		}

		void UpdateTouchPosition()
		{
			if (_frameGotTouchPosition != Time.frameCount)
			{
				_frameGotTouchPosition = Time.frameCount;
				if (_customInput)
				{
					_touchPosition = _customInputPos;
					_touchPosition.y = stageHeight - _touchPosition.y;
				}
				else if (touchScreen)
				{
					for (int i = 0; i < Input.touchCount; ++i)
					{
						Touch uTouch = Input.GetTouch(i);
						_touchPosition = uTouch.position;
						_touchPosition.y = stageHeight - _touchPosition.y;
					}
				}
				else
				{
					Vector2 pos = Input.mousePosition;
					if (pos.x >= 0 && pos.y >= 0) //编辑器环境下坐标有时是负
					{
						pos.y = stageHeight - pos.y;
						_touchPosition = pos;
					}
				}
			}
		}

		void HandleTextInput()
		{
			InputTextField textField = (InputTextField)_focused;
			if (!textField.editable)
				return;

			if (_keyboard != null)
			{
				string s = _keyboard.GetInput();
				if (s != null)
					textField.ReplaceText(s);

				if (_keyboard.done)
					this.focus = null;
			}
			else
			{
				if (!string.IsNullOrEmpty(Input.inputString))
				{
					StringBuilder sb = new StringBuilder();
					int len = Input.inputString.Length;
					for (int i = 0; i < len; ++i)
					{
						char ch = Input.inputString[i];
						if (ch >= ' ') sb.Append(ch.ToString());
					}
					if (sb.Length > 0)
						textField.ReplaceSelection(sb.ToString());
				}
			}
		}

		void HandleCustomInput()
		{
			Vector2 pos = _customInputPos;
			pos.y = stageHeight - pos.y;
			TouchInfo touch = _touches[0];

			if (touch.x != pos.x || touch.y != pos.y)
			{
				touch.x = pos.x;
				touch.y = pos.y;
				touch.UpdateEvent();
				onTouchMove.Call(touch.evt);

				if (touch.lastRollOver != touch.target)
					HandleRollOver(touch);
			}

			if (_customInputButtonDown)
			{
				if (!touch.began)
				{
					_touchCount = 1;
					touch.begin();
					this.focus = touch.target;

					if (touch.target != null)
					{
						touch.UpdateEvent();
						touch.target.onTouchBegin.BubbleCall(touch.evt);
					}
				}
			}
			else if (touch.began)
			{
				_touchCount = 0;
				touch.end();

				DisplayObject clickTarget = touch.ClickTest();
				if (clickTarget != null)
				{
					touch.UpdateEvent();
					clickTarget.onClick.BubbleCall(touch.evt);
				}
			}
		}

		void HandleMouseEvents()
		{
			TouchInfo touch = _touches[0];
			if (touch.x != _touchPosition.x || touch.y != _touchPosition.y)
			{
				touch.x = _touchPosition.x;
				touch.y = _touchPosition.y;
				touch.UpdateEvent();
				onTouchMove.Call(touch.evt);

				if (touch.lastRollOver != touch.target)
					HandleRollOver(touch);
			}

			if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				if (!touch.began)
				{
					_touchCount = 1;
					touch.begin();
					touch.button = Input.GetMouseButtonDown(2) ? 2 : (Input.GetMouseButtonDown(1) ? 1 : 0);
					this.focus = touch.target;

					if (touch.target != null)
					{
						touch.UpdateEvent();
						touch.target.onTouchBegin.BubbleCall(touch.evt);
					}
				}
			}
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
			{
				if (touch.began)
				{
					_touchCount = 0;
					touch.end();

					DisplayObject clickTarget = touch.ClickTest();
					if (clickTarget != null)
					{
						touch.UpdateEvent();

						if (Input.GetMouseButtonUp(1))
							clickTarget.onRightClick.BubbleCall(touch.evt);
						else
							clickTarget.onClick.BubbleCall(touch.evt);
					}
				}
			}
		}

		void HandleTouchEvents()
		{
			int tc = Input.touchCount;
			for (int i = 0; i < tc; ++i)
			{
				Touch uTouch = Input.GetTouch(i);

				if (uTouch.phase == TouchPhase.Stationary)
					continue;

				Vector2 pos = uTouch.position;
				pos.y = stageHeight - pos.y;

				TouchInfo touch = null;
				for (int j = 0; j < 5; j++)
				{
					if (_touches[j].touchId == uTouch.fingerId)
					{
						touch = _touches[j];
						break;
					}
				}
				if (touch == null)
					continue;

				if (touch.x != pos.x || touch.y != pos.y)
				{
					touch.x = pos.x;
					touch.y = pos.y;
					touch.UpdateEvent();
					onTouchMove.Call(touch.evt);

					//no rollover/rollout on mobile
				}

				if (uTouch.phase == TouchPhase.Began)
				{
					if (!touch.began)
					{
						_touchCount++;
						touch.begin();
						this.focus = touch.target;

						if (touch.target != null)
						{
							touch.UpdateEvent();
							touch.target.onTouchBegin.BubbleCall(touch.evt);
						}
					}
				}
				else if (uTouch.phase == TouchPhase.Canceled || uTouch.phase == TouchPhase.Ended)
				{
					if (touch.began)
					{
						_touchCount--;
						touch.end();

						DisplayObject clickTarget = touch.ClickTest();
						if (clickTarget != null)
						{
							touch.clickCount = uTouch.tapCount;
							touch.UpdateEvent();
							clickTarget.onClick.BubbleCall(touch.evt);
						}
					}

					touch.Reset();
				}
			}
		}

		void HandleRollOver(TouchInfo touch)
		{
			DisplayObject element;
			element = touch.lastRollOver;
			while (element != null)
			{
				_rollOutChain.Add(element);
				element = element.parent;
			}

			touch.lastRollOver = touch.target;

			element = touch.target;
			int i;
			while (element != null)
			{
				i = _rollOutChain.IndexOf(element);
				if (i != -1)
				{
					_rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
					break;
				}
				_rollOverChain.Add(element);

				element = element.parent;
			}

			int cnt = _rollOutChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOutChain[i];
					if (element.stage != null)
						element.onRollOut.Call();
				}
				_rollOutChain.Clear();
			}

			cnt = _rollOverChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOverChain[i];
					if (element.stage != null)
						element.onRollOver.Call();
				}
				_rollOverChain.Clear();
			}
		}

		/// <summary>
		/// 设置UIPanel/UIPainter等的渲染层次，由UIPanel等内部调用。开发者不需要调用。
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		public void ApplyPanelOrder(Container target)
		{
			int sortingOrder = target._panelOrder;
			int numChildren = Stage.inst.numChildren;
			int i = 0;
			int j;
			int curIndex = -1;
			for (; i < numChildren; i++)
			{
				DisplayObject obj = Stage.inst.GetChildAt(i);
				if (obj == target)
				{
					curIndex = i;
					continue;
				}

				if (obj == GRoot.inst.displayObject)
					j = 1000;
				else if (obj is Container)
					j = ((Container)obj)._panelOrder;
				else
					continue;

				if (sortingOrder <= j)
				{
					if (curIndex != -1)
						Stage.inst.AddChildAt(target, i - 1);
					else
						Stage.inst.AddChildAt(target, i);
					break;
				}
			}
			if (i == numChildren)
				Stage.inst.AddChild(target);
		}

		/// <summary>
		/// Adjust display order of all UIPanels rendering in worldspace by their z order.
		/// </summary>
		/// <param name="panelSortingOrder">Only UIPanel.sortingOrder equals to this value will be involve in this sorting</param>
		public void SortWorldSpacePanelsByZOrder(int panelSortingOrder)
		{
			if (sTempList1 == null)
			{
				sTempList1 = new List<DisplayObject>();
				sTempList2 = new List<int>();
			}

			int numChildren = Stage.inst.numChildren;
			for (int i = 0; i < numChildren; i++)
			{
				Container obj = Stage.inst.GetChildAt(i) as Container;
				if (obj == null || obj.renderMode != RenderMode.WorldSpace || obj._panelOrder != panelSortingOrder)
					continue;

				//借用一下tmpBounds
				obj._internal_bounds[0] = obj.cachedTransform.position.z;
				obj._internal_bounds[1] = i;

				sTempList1.Add(obj);
				sTempList2.Add(i);
			}

			sTempList1.Sort(CompareZ);

			ChangeChildrenOrder(sTempList2, sTempList1);

			sTempList1.Clear();
			sTempList2.Clear();
		}

		static List<DisplayObject> sTempList1;
		static List<int> sTempList2;
		static int CompareZ(DisplayObject c1, DisplayObject c2)
		{
			int ret = ((Container)c2)._internal_bounds[0].CompareTo(((Container)c1)._internal_bounds[0]);
			if (ret == 0)
			{
				//如果大家z值一样，使用原来的顺序，防止不停交换顺序（闪烁）
				return c1._internal_bounds[1].CompareTo(c2._internal_bounds[1]);
			}
			else
				return ret;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public void MonitorTexture(NTexture texture)
		{
			if (_toCollectTextures.IndexOf(texture) == -1)
				_toCollectTextures.Add(texture);
		}

		void RunTextureCollector(object param)
		{
			int cnt = _toCollectTextures.Count;
			float curTime = Time.time;
			int i = 0;
			while (i < cnt)
			{
				NTexture texture = _toCollectTextures[i];
				if (texture.disposed)
				{
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else if (curTime - texture.lastActive > 5)
				{
					texture.Dispose(true);
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}

		internal void AddTouchEndMonitor(int touchId, EventDispatcher target)
		{
			TouchInfo touch = null;
			if (touchId == -1)
				touch = _touches[0];
			else
			{
				for (int j = 0; j < 5; j++)
				{
					touch = _touches[j];
					if (touch.touchId == touchId)
						break;
				}
			}
			if (touch.touchEndMonitors.IndexOf(target) == -1)
				touch.touchEndMonitors.Add(target);
		}

		internal Transform CreatePoolManager(string name)
		{
			GameObject go = new GameObject("[" + name + "]");
			go.SetActive(false);

			Transform t = go.transform;
			ToolSet.SetParent(t, cachedTransform);

			return t;
		}
	}

	class TouchInfo
	{
		public float x;
		public float y;
		public int touchId;
		public int clickCount;
		public KeyCode keyCode;
		public EventModifiers modifiers;
		public int mouseWheelDelta;
		public int button;

		public float downX;
		public float downY;
		public bool began;
		public bool clickCancelled;
		public float lastClickTime;
		public DisplayObject target;
		public List<DisplayObject> downTargets;
		public DisplayObject lastRollOver;
		public List<EventDispatcher> touchEndMonitors;

		public InputEvent evt;

		static List<EventBridge> sHelperChain = new List<EventBridge>();

		public TouchInfo()
		{
			evt = new InputEvent();
			downTargets = new List<DisplayObject>();
			touchEndMonitors = new List<EventDispatcher>();
			Reset();
		}

		public void Reset()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			button = 0;
			keyCode = KeyCode.None;
			modifiers = 0;
			mouseWheelDelta = 0;
			lastClickTime = 0;
			began = false;
			target = null;
			downTargets.Clear();
			lastRollOver = null;
			clickCancelled = false;
			touchEndMonitors.Clear();
		}

		public void UpdateEvent()
		{
			evt.touchId = this.touchId;
			evt.x = this.x;
			evt.y = this.y;
			evt.clickCount = this.clickCount;
			evt.keyCode = this.keyCode;
			evt.modifiers = this.modifiers;
			evt.mouseWheelDelta = this.mouseWheelDelta;
			evt.button = this.button;
		}

		public void begin()
		{
			began = true;
			clickCancelled = false;
			downX = x;
			downY = y;

			downTargets.Clear();
			if (target != null)
			{
				downTargets.Add(target);
				DisplayObject obj = target;
				while (obj != null)
				{
					downTargets.Add(obj);
					obj = obj.parent;
				}
			}
		}

		public void end()
		{
			began = false;

			if (target != null)
			{
				UpdateEvent();

				if (touchEndMonitors.Count > 0)
				{
					int len = touchEndMonitors.Count;
					for (int i = 0; i < len; i++)
						touchEndMonitors[i].GetChainBridges("onTouchEnd", sHelperChain, false);
					target.BubbleEvent("onTouchEnd", evt, sHelperChain);
					sHelperChain.Clear();
				}
				else
					target.onTouchEnd.BubbleCall(evt);
			}

			touchEndMonitors.Clear();

			if (Time.realtimeSinceStartup - lastClickTime < 0.35f)
			{
				if (clickCount == 2)
					clickCount = 1;
				else
					clickCount++;
			}
			else
				clickCount = 1;
			lastClickTime = Time.realtimeSinceStartup;
		}

		public DisplayObject ClickTest()
		{
			if (downTargets.Count == 0
				|| clickCancelled
				|| Mathf.Abs(x - downX) > 50 || Mathf.Abs(y - downY) > 50)
				return null;

			DisplayObject obj = downTargets[0];
			if (obj.stage != null) //依然派发到原来的downTarget，虽然可能它已经偏离当前位置，主要是为了正确处理点击缩放的效果
				return obj;

			obj = target;
			while (obj != null)
			{
				int i = downTargets.IndexOf(obj);
				if (i != -1 && obj.stage != null)
					return obj;

				obj = obj.parent;
			}

			return obj;
		}
	}
}