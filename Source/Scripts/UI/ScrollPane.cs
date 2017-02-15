using System;
using UnityEngine;
using DG.Tweening;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ScrollPane : EventDispatcher
	{
		/// <summary>
		/// Dispatched when scrolling.
		/// 在滚动时派发该事件。
		/// </summary>
		public EventListener onScroll { get; private set; }

		/// <summary>
		/// 在滚动结束时派发该事件。
		/// </summary>
		public EventListener onScrollEnd { get; private set; }

		/// <summary>
		/// 向下拉过上边缘后释放则派发该事件。
		/// </summary>
		public EventListener onPullDownRelease { get; private set; }

		/// <summary>
		/// 向上拉过下边缘后释放则派发该事件。
		/// </summary>
		public EventListener onPullUpRelease { get; private set; }

		/// <summary>
		/// 当前被拖拽的滚动面板。同一时间只能有一个在进行此操作。
		/// </summary>
		public static ScrollPane draggingPane { get; private set; }

		ScrollType _scrollType;
		float _scrollStep;
		float _mouseWheelStep;
		Margin _scrollBarMargin;
		bool _bouncebackEffect;
		bool _touchEffect;
		bool _scrollBarDisplayAuto;
		bool _vScrollNone;
		bool _hScrollNone;
		bool _needRefresh;

		bool _displayOnLeft;
		bool _snapToItem;
		bool _displayInDemand;
		bool _mouseWheelEnabled;
		bool _softnessOnTopOrLeftSide;
		bool _pageMode;
		Vector2 _pageSize;
		bool _inertiaDisabled;
		bool _maskDisabled;
		float _decelerationRate;

		float _xPos;
		float _yPos;

		Vector2 _viewSize;
		Vector2 _contentSize;
		Vector2 _overlapSize;
		Vector2 _containerPos;
		Vector2 _beginTouchPos;
		Vector2 _lastTouchPos;
		Vector2 _lastTouchGlobalPos;
		Vector2 _velocity;
		float _velocityScale;
		float _lastMoveTime;
		bool _isMouseMoved;
		bool _isHoldAreaDone;
		int _aniFlag;
		bool _scrollBarVisible;
		int _touchId;
		internal int _loop;

		int _tweening;
		Vector2 _tweenStart;
		Vector2 _tweenChange;
		Vector2 _tweenTime;
		Vector2 _tweenDuration;

		EventCallback0 _refreshDelegate;
		EventCallback1 _touchEndDelegate;
		EventCallback1 _touchMoveDelegate;
		TimerCallback _tweenUpdateDelegate;

		GComponent _owner;
		Container _maskContainer;
		Container _container;
		GScrollBar _hzScrollBar;
		GScrollBar _vtScrollBar;

		static int _gestureFlag;

		const float TWEEN_TIME_GO = 0.5f; //调用SetPos(ani)时使用的缓动时间
		const float TWEEN_TIME_DEFAULT = 0.3f; //惯性滚动的最小缓动时间
		const float PULL_RATIO = 0.3f; //下拉过顶或者上拉过底时允许超过的距离占显示区域的比例

		public ScrollPane(GComponent owner,
									ScrollType scrollType,
									Margin scrollBarMargin,
									ScrollBarDisplayType scrollBarDisplay,
									int flags,
									string vtScrollBarRes,
									string hzScrollBarRes)
		{
			onScroll = new EventListener(this, "onScroll");
			onScrollEnd = new EventListener(this, "onScrollEnd");
			onPullDownRelease = new EventListener(this, "onPullDownRelease");
			onPullUpRelease = new EventListener(this, "onPullUpRelease");

			_refreshDelegate = Refresh;
			_tweenUpdateDelegate = TweenUpdate;
			_touchEndDelegate = __touchEnd;
			_touchMoveDelegate = __touchMove;

			_owner = owner;

			_maskContainer = new Container();
			_owner.rootContainer.AddChild(_maskContainer);

			_container = _owner.container;
			_container.SetXY(0, 0);
			_maskContainer.AddChild(_container);

			_scrollBarMargin = scrollBarMargin;
			_scrollType = scrollType;
			_scrollStep = UIConfig.defaultScrollStep;
			_mouseWheelStep = _scrollStep * 2;
			_softnessOnTopOrLeftSide = UIConfig.allowSoftnessOnTopOrLeftSide;
			_decelerationRate = UIConfig.defaultScrollDecelerationRate;

			_displayOnLeft = (flags & 1) != 0;
			_snapToItem = (flags & 2) != 0;
			_displayInDemand = (flags & 4) != 0;
			_pageMode = (flags & 8) != 0;
			if ((flags & 16) != 0)
				_touchEffect = true;
			else if ((flags & 32) != 0)
				_touchEffect = false;
			else
				_touchEffect = UIConfig.defaultScrollTouchEffect;
			if ((flags & 64) != 0)
				_bouncebackEffect = true;
			else if ((flags & 128) != 0)
				_bouncebackEffect = false;
			else
				_bouncebackEffect = UIConfig.defaultScrollBounceEffect;
			_inertiaDisabled = (flags & 256) != 0;
			_maskDisabled = (flags & 512) != 0;

			_scrollBarVisible = true;
			_mouseWheelEnabled = true;
			_pageSize = Vector2.one;

			if (scrollBarDisplay == ScrollBarDisplayType.Default)
			{
				if (Application.isMobilePlatform)
					scrollBarDisplay = ScrollBarDisplayType.Auto;
				else
					scrollBarDisplay = UIConfig.defaultScrollBarDisplay;
			}

			if (scrollBarDisplay != ScrollBarDisplayType.Hidden)
			{
				if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Vertical)
				{
					string res = string.IsNullOrEmpty(vtScrollBarRes) ? UIConfig.verticalScrollBar : vtScrollBarRes;
					if (!string.IsNullOrEmpty(res))
					{
						_vtScrollBar = UIPackage.CreateObjectFromURL(res) as GScrollBar;
						if (_vtScrollBar == null)
							Debug.LogWarning("FairyGUI: cannot create scrollbar from " + res);
						else
						{
							_vtScrollBar.SetScrollPane(this, true);
							_owner.rootContainer.AddChild(_vtScrollBar.displayObject);
						}
					}
				}
				if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Horizontal)
				{
					string res = string.IsNullOrEmpty(hzScrollBarRes) ? UIConfig.horizontalScrollBar : hzScrollBarRes;
					if (!string.IsNullOrEmpty(res))
					{
						_hzScrollBar = UIPackage.CreateObjectFromURL(res) as GScrollBar;
						if (_hzScrollBar == null)
							Debug.LogWarning("FairyGUI: cannot create scrollbar from " + res);
						else
						{
							_hzScrollBar.SetScrollPane(this, false);
							_owner.rootContainer.AddChild(_hzScrollBar.displayObject);
						}
					}
				}

				_scrollBarDisplayAuto = scrollBarDisplay == ScrollBarDisplayType.Auto;
				if (_scrollBarDisplayAuto)
				{
					if (_vtScrollBar != null)
						_vtScrollBar.displayObject.visible = false;
					if (_hzScrollBar != null)
						_hzScrollBar.displayObject.visible = false;
					_scrollBarVisible = false;

					_owner.rootContainer.onRollOver.Add(__rollOver);
					_owner.rootContainer.onRollOut.Add(__rollOut);
				}
			}
			else
				_mouseWheelEnabled = false;

			if (!_maskDisabled && (_vtScrollBar != null || _hzScrollBar != null))
			{
				//当有滚动条对象时，为了避免滚动条变化时触发重新合批，这里给rootContainer也加上剪裁。但这可能会增加额外dc。
				_owner.rootContainer.clipRect = new Rect(0, 0, _owner.width, _owner.height);
			}

			SetSize(owner.width, owner.height);

			_owner.rootContainer.onMouseWheel.Add(__mouseWheel);
			_owner.rootContainer.onTouchBegin.Add(__touchBegin);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (_tweening != 0)
				Timers.inst.Remove(_tweenUpdateDelegate);
		}

		/// <summary>
		/// 
		/// </summary>
		public GComponent owner
		{
			get { return _owner; }
		}

		/// <summary>
		/// 滚动到达边缘时是否允许回弹效果。
		/// </summary>
		public bool bouncebackEffect
		{
			get { return _bouncebackEffect; }
			set { _bouncebackEffect = value; }
		}

		/// <summary>
		/// 是否允许拖拽内容区域进行滚动。
		/// </summary>
		public bool touchEffect
		{
			get { return _touchEffect; }
			set { _touchEffect = value; }
		}

		/// <summary>
		/// 是否允许惯性滚动。
		/// </summary>
		public bool inertiaDisabled
		{
			get { return _inertiaDisabled; }
			set { _inertiaDisabled = value; }
		}

		/// <summary>
		/// 是否允许在左/上边缘显示虚化效果。
		/// </summary>
		public bool softnessOnTopOrLeftSide
		{
			get { return _softnessOnTopOrLeftSide; }
			set { _softnessOnTopOrLeftSide = value; }
		}

		[Obsolete("ScrollPane.scrollSpeed is deprecated. Use scrollStep instead.")]
		public float scrollSpeed
		{
			get { return this.scrollStep; }
			set { this.scrollStep = value; }
		}

		/// <summary>
		/// 当调用ScrollPane.scrollUp/Down/Left/Right时，或者点击滚动条的上下箭头时，滑动的距离。
		/// 鼠标滚轮触发一次滚动的距离设定为defaultScrollStep*2
		/// </summary>
		public float scrollStep
		{
			get { return _scrollStep; }
			set
			{
				_scrollStep = value;
				if (_scrollStep == 0)
					_scrollStep = UIConfig.defaultScrollStep;
				_mouseWheelStep = _scrollStep * 2;
			}
		}

		/// <summary>
		/// 滚动位置是否保持贴近在某个元件的边缘。
		/// </summary>
		public bool snapToItem
		{
			get { return _snapToItem; }
			set { _snapToItem = value; }
		}

		/// <summary>
		/// 是否允许使用鼠标滚轮进行滚动。
		/// </summary>
		public bool mouseWheelEnabled
		{
			get { return _mouseWheelEnabled; }
			set { _mouseWheelEnabled = value; }
		}

		/// <summary>
		/// 当处于惯性滚动时减速的速率。默认值是UIConfig.defaultScrollDecelerationRate。
		/// 越接近1，减速越慢，意味着滑动的时间和距离更长。
		/// </summary>
		public float decelerationRate
		{
			get { return _decelerationRate; }
			set { _decelerationRate = value; }
		}

		/// <summary>
		/// 当前X轴滚动位置百分比，0~1（包含）。
		/// </summary>
		public float percX
		{
			get { return _overlapSize.x == 0 ? 0 : _xPos / _overlapSize.x; }
			set { SetPercX(value, false); }
		}

		/// <summary>
		/// 设置当前X轴滚动位置百分比，0~1（包含）。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetPercX(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();
			SetPosX(_overlapSize.x * Mathf.Clamp01(value), ani);
		}

		/// <summary>
		/// 当前Y轴滚动位置百分比，0~1（包含）。
		/// </summary>
		public float percY
		{
			get { return _overlapSize.y == 0 ? 0 : _yPos / _overlapSize.y; }
			set { SetPercY(value, false); }
		}

		/// <summary>
		/// 设置当前Y轴滚动位置百分比，0~1（包含）。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetPercY(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();
			SetPosY(_overlapSize.y * Mathf.Clamp01(value), ani);
		}

		/// <summary>
		/// 当前X轴滚动位置，值范围是viewWidth与contentWidth之差。
		/// </summary>
		public float posX
		{
			get { return _xPos; }
			set { SetPosX(value, false); }
		}

		/// <summary>
		/// 设置当前X轴滚动位置。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetPosX(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();

			value = Mathf.Clamp(value, 0, _overlapSize.x);
			if (value != _xPos)
			{
				_xPos = value;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 当前Y轴滚动位置，值范围是viewHeight与contentHeight之差。
		/// </summary>
		public float posY
		{
			get { return _yPos; }
			set { SetPosY(value, false); }
		}

		/// <summary>
		/// 设置当前Y轴滚动位置。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetPosY(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();

			value = Mathf.Clamp(value, 0, _overlapSize.y);
			if (value != _yPos)
			{
				_yPos = value;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 返回当前滚动位置是否在最下边。
		/// </summary>
		public bool isBottomMost
		{
			get { return _yPos == _overlapSize.y || _overlapSize.y == 0; }
		}

		/// <summary>
		/// 返回当前滚动位置是否在最右边。
		/// </summary>
		public bool isRightMost
		{
			get { return _xPos == _overlapSize.x || _overlapSize.x == 0; }
		}

		/// <summary>
		/// 如果处于分页模式，返回当前在X轴的页码。
		/// </summary>
		public int currentPageX
		{
			get { return _pageMode ? Mathf.FloorToInt(_xPos / _pageSize.x) : 0; }
			set
			{
				if (_overlapSize.x > 0)
					this.SetPosX(value * _pageSize.x, false);
			}
		}

		/// <summary>
		/// 如果处于分页模式，可设置X轴的页码。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetCurrentPageX(int value, bool ani)
		{
			if (_overlapSize.x > 0)
				this.SetPosX(value * _pageSize.x, ani);
		}

		/// <summary>
		/// 如果处于分页模式，返回当前在Y轴的页码。
		/// </summary>
		public int currentPageY
		{
			get { return _pageMode ? Mathf.FloorToInt(_yPos / _pageSize.y) : 0; }
			set
			{
				if (_overlapSize.y > 0)
					this.SetPosY(value * _pageSize.y, false);
			}
		}

		/// <summary>
		/// 如果处于分页模式，可设置Y轴的页码。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani">是否使用缓动到达目标。</param>
		public void SetCurrentPageY(int value, bool ani)
		{
			if (_overlapSize.y > 0)
				this.SetPosY(value * _pageSize.y, ani);
		}

		/// <summary>
		/// 这个值与PosX不同在于，他反映的是实时位置，而PosX在有缓动过程的情况下只是终值。
		/// </summary>
		public float scrollingPosX
		{
			get
			{
				return Mathf.Clamp(-_container.x, 0, _overlapSize.x);
			}
		}

		/// <summary>
		/// 这个值与PosY不同在于，他反映的是实时位置，而PosY在有缓动过程的情况下只是终值。
		/// </summary>
		public float scrollingPosY
		{
			get
			{
				return Mathf.Clamp(-_container.y, 0, _overlapSize.y);
			}
		}

		/// <summary>
		/// 显示内容宽度。
		/// </summary>
		public float contentWidth
		{
			get
			{
				return _contentSize.x;
			}
		}

		/// <summary>
		/// 显示内容高度。
		/// </summary>
		public float contentHeight
		{
			get
			{
				return _contentSize.y;
			}
		}

		/// <summary>
		/// 显示区域宽度。
		/// </summary>
		public float viewWidth
		{
			get { return _viewSize.x; }
			set
			{
				value = value + _owner.margin.left + _owner.margin.right;
				if (_vtScrollBar != null)
					value += _vtScrollBar.width;
				_owner.width = value;
			}
		}

		/// <summary>
		/// 显示区域高度。
		/// </summary>
		public float viewHeight
		{
			get { return _viewSize.y; }
			set
			{
				value = value + _owner.margin.top + _owner.margin.bottom;
				if (_hzScrollBar != null)
					value += _hzScrollBar.height;
				_owner.height = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollTop()
		{
			ScrollTop(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ani"></param>
		public void ScrollTop(bool ani)
		{
			this.SetPercY(0, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollBottom()
		{
			ScrollBottom(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ani"></param>
		public void ScrollBottom(bool ani)
		{
			this.SetPercY(1, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollUp()
		{
			ScrollUp(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratio"></param>
		/// <param name="ani"></param>
		public void ScrollUp(float ratio, bool ani)
		{
			if (_pageMode)
				SetPosY(_yPos - _pageSize.y * ratio, ani);
			else
				SetPosY(_yPos - _scrollStep * ratio, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollDown()
		{
			ScrollDown(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratio"></param>
		/// <param name="ani"></param>
		public void ScrollDown(float ratio, bool ani)
		{
			if (_pageMode)
				SetPosY(_yPos + _pageSize.y * ratio, ani);
			else
				SetPosY(_yPos + _scrollStep * ratio, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollLeft()
		{
			ScrollLeft(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollLeft(float ratio, bool ani)
		{
			if (_pageMode)
				SetPosX(_xPos - _pageSize.x * ratio, ani);
			else
				SetPosX(_xPos - _scrollStep * ratio, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ScrollRight()
		{
			ScrollRight(1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ratio"></param>
		/// <param name="ani"></param>
		public void ScrollRight(float ratio, bool ani)
		{
			if (_pageMode)
				SetPosX(_xPos + _pageSize.x * ratio, ani);
			else
				SetPosX(_xPos + _scrollStep * ratio, ani);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		public void ScrollToView(GObject obj)
		{
			ScrollToView(obj, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		/// <param name="ani">If moving to target position with animation</param>
		public void ScrollToView(GObject obj, bool ani)
		{
			ScrollToView(obj, ani, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj can be any object on stage, not limited to the direct child of this container.</param>
		/// <param name="ani">If moving to target position with animation</param>
		/// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
		public void ScrollToView(GObject obj, bool ani, bool setFirst)
		{
			_owner.EnsureBoundsCorrect();
			if (_needRefresh)
				Refresh();

			Rect rect = new Rect(obj.x, obj.y, obj.width, obj.height);
			if (obj.parent != _owner)
				rect = obj.parent.TransformRect(rect, _owner);
			ScrollToView(rect, ani, setFirst);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect">Rect in local coordinates</param>
		/// <param name="ani">If moving to target position with animation</param>
		/// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
		public void ScrollToView(Rect rect, bool ani, bool setFirst)
		{
			_owner.EnsureBoundsCorrect();
			if (_needRefresh)
				Refresh();

			if (_overlapSize.y > 0)
			{
				float bottom = _yPos + _viewSize.y;
				if (setFirst || rect.y <= _yPos || rect.height >= _viewSize.y)
				{
					if (_pageMode)
						this.SetPosY(Mathf.Floor(rect.y / _pageSize.y) * _pageSize.y, ani);
					else
						SetPosY(rect.y, ani);
				}
				else if (rect.y + rect.height > bottom)
				{
					if (_pageMode)
						this.SetPosY(Mathf.Floor(rect.y / _pageSize.y) * _pageSize.y, ani);
					else if (rect.height <= _viewSize.y / 2)
						SetPosY(rect.y + rect.height * 2 - _viewSize.y, ani);
					else
						SetPosY(rect.y + rect.height - _viewSize.y, ani);
				}
			}
			if (_overlapSize.x > 0)
			{
				float right = _xPos + _viewSize.x;
				if (setFirst || rect.x <= _xPos || rect.width >= _viewSize.x)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					SetPosX(rect.x, ani);
				}
				else if (rect.x + rect.width > right)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					else if (rect.width <= _viewSize.x / 2)
						SetPosX(rect.x + rect.width * 2 - _viewSize.x, ani);
					else
						SetPosX(rect.x + rect.width - _viewSize.x, ani);
				}
			}

			if (!ani && _needRefresh)
				Refresh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj">obj must be the direct child of this container</param>
		/// <returns></returns>
		public bool IsChildInView(GObject obj)
		{
			if (_overlapSize.y > 0)
			{
				float dist = obj.y + _container.y;
				if (dist < -obj.height - 20 || dist > _viewSize.y + 20)
					return false;
			}
			if (_overlapSize.x > 0)
			{
				float dist = obj.x + _container.x;
				if (dist < -obj.width - 20 || dist > _viewSize.x + 20)
					return false;
			}

			return true;
		}

		/// <summary>
		/// 当滚动面板处于拖拽滚动状态或即将进入拖拽状态时，可以调用此方法停止或禁止本次拖拽。
		/// </summary>
		public void CancelDragging()
		{
			Stage.inst.onTouchMove.Remove(_touchMoveDelegate);
			Stage.inst.onTouchEnd.Remove(_touchEndDelegate);

			if (draggingPane == this)
				draggingPane = null;

			_gestureFlag = 0;
			_isMouseMoved = false;
		}

		internal void OnOwnerSizeChanged()
		{
			SetSize(_owner.width, _owner.height);
			PosChanged(false);
		}

		internal void AdjustMaskContainer()
		{
			float mx, my;
			if (_displayOnLeft && _vtScrollBar != null)
				mx = Mathf.FloorToInt(_owner.margin.left + _vtScrollBar.width);
			else
				mx = Mathf.FloorToInt(_owner.margin.left);
			my = Mathf.FloorToInt(_owner.margin.top);
			mx += _owner._alignOffset.x;
			my += _owner._alignOffset.y;

			_maskContainer.SetXY(mx, my);
		}

		void SetSize(float aWidth, float aHeight)
		{
			AdjustMaskContainer();

			if (_hzScrollBar != null)
			{
				_hzScrollBar.y = aHeight - _hzScrollBar.height;
				if (_vtScrollBar != null)
				{
					_hzScrollBar.width = aWidth - _vtScrollBar.width - _scrollBarMargin.left - _scrollBarMargin.right;
					if (_displayOnLeft)
						_hzScrollBar.x = _scrollBarMargin.left + _vtScrollBar.width;
					else
						_hzScrollBar.x = _scrollBarMargin.left;
				}
				else
				{
					_hzScrollBar.width = aWidth - _scrollBarMargin.left - _scrollBarMargin.right;
					_hzScrollBar.x = _scrollBarMargin.left;
				}
			}
			if (_vtScrollBar != null)
			{
				if (!_displayOnLeft)
					_vtScrollBar.x = aWidth - _vtScrollBar.width;
				if (_hzScrollBar != null)
					_vtScrollBar.height = aHeight - _hzScrollBar.height - _scrollBarMargin.top - _scrollBarMargin.bottom;
				else
					_vtScrollBar.height = aHeight - _scrollBarMargin.top - _scrollBarMargin.bottom;
				_vtScrollBar.y = _scrollBarMargin.top;
			}

			_viewSize.x = aWidth;
			_viewSize.y = aHeight;
			if (_hzScrollBar != null && !_hScrollNone)
				_viewSize.y -= _hzScrollBar.height;
			if (_vtScrollBar != null && !_vScrollNone)
				_viewSize.x -= _vtScrollBar.width;
			_viewSize.x -= (_owner.margin.left + _owner.margin.right);
			_viewSize.y -= (_owner.margin.top + _owner.margin.bottom);

			_viewSize.x = Mathf.Max(1, _viewSize.x);
			_viewSize.y = Mathf.Max(1, _viewSize.y);
			_pageSize.x = _viewSize.x;
			_pageSize.y = _viewSize.y;

			HandleSizeChanged();
		}

		internal void SetContentSize(float aWidth, float aHeight)
		{
			if (Mathf.Approximately(_contentSize.x, aWidth) && Mathf.Approximately(_contentSize.y, aHeight))
				return;

			_contentSize.x = aWidth;
			_contentSize.y = aHeight;
			HandleSizeChanged();
		}

		/// <summary>
		/// 内部使用。由虚拟列表调用。在滚动时修改显示内容的大小，需要进行修正，避免滚动跳跃。
		/// </summary>
		/// <param name="deltaWidth"></param>
		/// <param name="deltaHeight"></param>
		/// <param name="deltaPosX"></param>
		/// <param name="deltaPosY"></param>
		internal void ChangeContentSizeOnScrolling(float deltaWidth, float deltaHeight, float deltaPosX, float deltaPosY)
		{
			bool isRightmost = _xPos == _overlapSize.x;
			bool isBottom = _yPos == _overlapSize.y;

			_contentSize.x += deltaWidth;
			_contentSize.y += deltaHeight;
			HandleSizeChanged();

			if (_tweening == 1)
			{
				//如果原来滚动位置是贴边，加入处理继续贴边。
				if (deltaWidth != 0 && isRightmost && _tweenChange.x < 0)
				{
					_xPos = _overlapSize.x;
					_tweenChange.x = -_xPos - _tweenStart.x;
				}

				if (deltaHeight != 0 && isBottom && _tweenChange.y < 0)
				{
					_yPos = _overlapSize.y;
					_tweenChange.y = -_yPos - _tweenStart.y;
				}
			}
			else if (_tweening == 2)
			{
				//重新调整起始位置，确保能够顺滑滚下去
				if (deltaPosX != 0)
				{
					_container.x -= deltaPosX;
					_tweenStart.x -= deltaPosX;
					_xPos = -_container.x;
				}
				if (deltaPosY != 0)
				{
					_container.y -= deltaPosY;
					_tweenStart.y -= deltaPosY;
					_yPos = -_container.y;
				}
			}
			else if (_isMouseMoved)
			{
				if (deltaPosX != 0)
				{
					_container.x -= deltaPosX;
					_containerPos.x -= deltaPosX;
					_xPos = -_container.x;
				}
				if (deltaPosY != 0)
				{
					_container.y -= deltaPosY;
					_containerPos.y -= deltaPosY;
					_yPos = -_container.y;
				}
			}
			else
			{
				//如果原来滚动位置是贴边，加入处理继续贴边。
				if (deltaWidth != 0 && isRightmost)
				{
					_xPos = _overlapSize.x;
					_container.x = -_xPos;
				}

				if (deltaHeight != 0 && isBottom)
				{
					_yPos = _overlapSize.y;
					_container.y = -_yPos;
				}
			}
		}

		void HandleSizeChanged()
		{
			if (_displayInDemand)
			{
				if (_vtScrollBar != null)
				{
					if (_contentSize.y <= _viewSize.y)
					{
						if (!_vScrollNone)
						{
							_vScrollNone = true;
							_viewSize.x += _vtScrollBar.width;
						}
					}
					else
					{
						if (_vScrollNone)
						{
							_vScrollNone = false;
							_viewSize.x -= _vtScrollBar.width;
						}
					}
				}
				if (_hzScrollBar != null)
				{
					if (_contentSize.x <= _viewSize.x)
					{
						if (!_hScrollNone)
						{
							_hScrollNone = true;
							_viewSize.y += _hzScrollBar.height;
						}
					}
					else
					{
						if (_hScrollNone)
						{
							_hScrollNone = false;
							_viewSize.y -= _hzScrollBar.height;
						}
					}
				}
			}

			if (_vtScrollBar != null)
			{
				if (_viewSize.y < _vtScrollBar.minSize)
					_vtScrollBar.displayObject.visible = false;
				else
				{
					_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
					if (_contentSize.y == 0)
						_vtScrollBar.displayPerc = 0;
					else
						_vtScrollBar.displayPerc = Math.Min(1, _viewSize.y / _contentSize.y);
				}
			}
			if (_hzScrollBar != null)
			{
				if (_viewSize.x < _hzScrollBar.minSize)
					_hzScrollBar.displayObject.visible = false;
				else
				{
					_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
					if (_contentSize.x == 0)
						_hzScrollBar.displayPerc = 0;
					else
						_hzScrollBar.displayPerc = Math.Min(1, _viewSize.x / _contentSize.x);
				}
			}

			if (!_maskDisabled)
				_maskContainer.clipRect = new Rect(-owner._alignOffset.x, -owner._alignOffset.y, _viewSize.x, _viewSize.y);

			if (_scrollType == ScrollType.Horizontal || _scrollType == ScrollType.Both)
				_overlapSize.x = Mathf.CeilToInt(Math.Max(0, _contentSize.x - _viewSize.x));
			else
				_overlapSize.x = 0;
			if (_scrollType == ScrollType.Vertical || _scrollType == ScrollType.Both)
				_overlapSize.y = Mathf.CeilToInt(Math.Max(0, _contentSize.y - _viewSize.y));
			else
				_overlapSize.y = 0;

			//边界检查
			_xPos = Mathf.Clamp(_xPos, 0, _overlapSize.x);
			_yPos = Mathf.Clamp(_yPos, 0, _overlapSize.y);
			_container.SetXY(Mathf.Clamp(_container.x, -_overlapSize.x, 0), Mathf.Clamp(_container.y, -_overlapSize.y, 0));

			SyncScrollBar();
		}

		private void PosChanged(bool ani)
		{
			//只要有1处要求不要缓动，那就不缓动
			if (_aniFlag == 0)
				_aniFlag = ani ? 1 : -1;
			else if (_aniFlag == 1 && !ani)
				_aniFlag = -1;

			_needRefresh = true;

			UpdateContext.OnBegin -= _refreshDelegate;
			UpdateContext.OnBegin += _refreshDelegate;
		}

		private void Refresh()
		{
			_needRefresh = false;
			UpdateContext.OnBegin -= _refreshDelegate;

			if (_owner.displayObject == null || _owner.displayObject.isDisposed)
				return;

			if (_pageMode || _snapToItem)
			{
				Vector2 pos = new Vector2(-_xPos, -_yPos);
				AlignPosition(ref pos, false);
				_xPos = -pos.x;
				_yPos = -pos.y;
			}

			Refresh2();

			onScroll.Call();
			if (_needRefresh) //在onScroll事件里开发者可能修改位置，这里再刷新一次，避免闪烁
			{
				_needRefresh = false;
				UpdateContext.OnBegin -= _refreshDelegate;

				Refresh2();
			}

			SyncScrollBar();
			_aniFlag = 0;
		}

		void Refresh2()
		{
			if (_aniFlag == 1 && !_isMouseMoved)
			{
				Vector2 pos;

				if (_overlapSize.x > 0)
					pos.x = -(int)_xPos;
				else
				{
					if (_container.x != 0)
						_container.x = 0;
					pos.x = _container.x;
				}
				if (_overlapSize.y > 0)
					pos.y = -(int)_yPos;
				else
				{
					if (_container.y != 0)
						_container.y = 0;
					pos.y = _container.y;
				}

				if (pos.x != _container.x || pos.y != _container.y)
				{
					_tweening = 1;
					_tweenTime = Vector2.zero;
					_tweenDuration = new Vector2(TWEEN_TIME_GO, TWEEN_TIME_GO);
					_tweenStart = _container.xy;
					_tweenChange = pos - _tweenStart;
					Timers.inst.AddUpdate(_tweenUpdateDelegate);
				}
				else if (_tweening != 0)
					KillTween();
			}
			else
			{
				if (_tweening != 0)
					KillTween();

				_container.SetXY((int)-_xPos, (int)-_yPos);

				LoopCheckingCurrent();
			}
		}

		internal void UpdateClipSoft()
		{
			Vector2 softness = _owner.clipSoftness;
			if (softness.x != 0 || softness.y != 0)
			{
				_maskContainer.clipSoftness = new Vector4(
					//左边缘和上边缘感觉不需要效果，所以注释掉
					(_xPos < 0.01f || !_softnessOnTopOrLeftSide) ? 0 : softness.x,
					(_yPos < 0.01f || !_softnessOnTopOrLeftSide) ? 0 : softness.y,
					(_overlapSize.x == 0 || _xPos - _overlapSize.x > -0.01f) ? 0 : softness.x,
					(_overlapSize.y == 0 || _yPos - _overlapSize.y > -0.01f) ? 0 : softness.y);
			}
			else
				_maskContainer.clipSoftness = null;
		}

		private void SyncScrollBar(bool end = false)
		{
			if (_vtScrollBar != null)
			{
				_vtScrollBar.scrollPerc = _overlapSize.y == 0 ? 0 : Mathf.Clamp(-_container.y, 0, _overlapSize.y) / _overlapSize.y;
				if (_scrollBarDisplayAuto)
					ShowScrollBar(!end);
			}
			if (_hzScrollBar != null)
			{
				_hzScrollBar.scrollPerc = _overlapSize.x == 0 ? 0 : Mathf.Clamp(-_container.x, 0, _overlapSize.x) / _overlapSize.x;
				if (_scrollBarDisplayAuto)
					ShowScrollBar(!end);
			}

			UpdateClipSoft();
		}

		private void __touchBegin(EventContext context)
		{
			if (!_touchEffect)
				return;

			InputEvent evt = context.inputEvent;
			Vector2 pt = _owner.GlobalToLocal(evt.position);
			_touchId = evt.touchId;

			if (_tweening != 0)
			{
				KillTween();
				Stage.inst.CancelClick(_touchId);
			}

			_containerPos = _container.xy;
			_beginTouchPos = _lastTouchPos = pt;
			_lastTouchGlobalPos = evt.position;
			_isHoldAreaDone = false;
			_isMouseMoved = false;
			_velocity = Vector2.zero;
			_velocityScale = 1;
			_lastMoveTime = Time.unscaledTime;

			Stage.inst.onTouchMove.Add(_touchMoveDelegate);
			Stage.inst.onTouchEnd.Add(_touchEndDelegate);
		}

		private void __touchMove(EventContext context)
		{
			if (!_touchEffect || _owner.displayObject == null || _owner.displayObject.isDisposed)
				return;

			if (draggingPane != null && draggingPane != this || GObject.draggingObject != null) //已经有其他拖动
				return;

			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Vector2 pt = _owner.GlobalToLocal(evt.position);
			if (float.IsNaN(pt.x))
				return;

			int sensitivity;
			if (Stage.touchScreen)
				sensitivity = UIConfig.touchScrollSensitivity;
			else
				sensitivity = 8;

			float diff;
			bool sv = false, sh = false;

			if (_scrollType == ScrollType.Vertical)
			{
				if (!_isHoldAreaDone)
				{
					//表示正在监测垂直方向的手势
					_gestureFlag |= 1;

					diff = Mathf.Abs(_beginTouchPos.y - pt.y);
					if (diff < sensitivity)
						return;

					if ((_gestureFlag & 2) != 0) //已经有水平方向的手势在监测，那么我们用严格的方式检查是不是按垂直方向移动，避免冲突
					{
						float diff2 = Mathf.Abs(_beginTouchPos.x - pt.x);
						if (diff < diff2) //不通过则不允许滚动了
							return;
					}
				}

				sv = true;
			}
			else if (_scrollType == ScrollType.Horizontal)
			{
				if (!_isHoldAreaDone)
				{
					_gestureFlag |= 2;

					diff = Mathf.Abs(_beginTouchPos.x - pt.x);
					if (diff < sensitivity)
						return;

					if ((_gestureFlag & 1) != 0)
					{
						float diff2 = Mathf.Abs(_beginTouchPos.y - pt.y);
						if (diff < diff2)
							return;
					}
				}

				sh = true;
			}
			else
			{
				_gestureFlag = 3;

				if (!_isHoldAreaDone)
				{
					diff = Mathf.Abs(_beginTouchPos.y - pt.y);
					if (diff < sensitivity)
					{
						diff = Mathf.Abs(_beginTouchPos.x - pt.x);
						if (diff < sensitivity)
							return;
					}
				}

				sv = sh = true;
			}

			Vector2 newPos = _containerPos + pt - _beginTouchPos;
			newPos.x = (int)newPos.x;
			newPos.y = (int)newPos.y;

			if (sv)
			{
				if (newPos.y > 0)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.y = 0;
					else
						_container.y = (int)(newPos.y * PULL_RATIO);
				}
				else if (newPos.y < -_overlapSize.y)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.y = -_overlapSize.y;
					else
						_container.y = (int)((newPos.y + _overlapSize.y) * PULL_RATIO - _overlapSize.y);
				}
				else
					_container.y = newPos.y;
			}

			if (sh)
			{
				if (newPos.x > 0)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.x = 0;
					else
						_container.x = (int)(newPos.x * PULL_RATIO);
				}
				else if (newPos.x < 0 - _overlapSize.x)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.x = -_overlapSize.x;
					else
						_container.x = (int)((newPos.x + _overlapSize.x) * PULL_RATIO - _overlapSize.x);
				}
				else
					_container.x = newPos.x;
			}

			//更新速度
			float deltaTime = Time.unscaledDeltaTime;
			float elapsed = (Time.unscaledTime - _lastMoveTime) * 60 - 1;
			if (elapsed > 1) //速度衰减
				_velocity = _velocity * Mathf.Pow(0.833f, elapsed);
			Vector2 deltaPosition = pt - _lastTouchPos;
			if (!sh)
				deltaPosition.x = 0;
			if (!sv)
				deltaPosition.y = 0;
			_velocity = Vector2.Lerp(_velocity, deltaPosition / deltaTime, deltaTime * 10);

			/*速度计算使用的是本地位移，但在后续的惯性滚动判断中需要用到屏幕位移，所以这里要记录一个位移的比例。
			 *后续的处理要使用这个比例但不使用坐标转换的方法的原因是，在曲面UI等异形UI中，还无法简单地进行屏幕坐标和本地坐标的转换。
			 */
			Vector2 deltaGlobalPosition = _lastTouchGlobalPos - evt.position;
			if (deltaPosition.x != 0)
				_velocityScale = Mathf.Abs(deltaGlobalPosition.x / deltaPosition.x);
			else if (deltaPosition.y != 0)
				_velocityScale = Mathf.Abs(deltaGlobalPosition.y / deltaPosition.y);

			_lastTouchPos = pt;
			_lastTouchGlobalPos = evt.position;
			_lastMoveTime = Time.unscaledTime;

			//同步更新pos值
			if (_overlapSize.x > 0)
				_xPos = Mathf.Clamp(-_container.x, 0, _overlapSize.x);
			if (_overlapSize.y > 0)
				_yPos = Mathf.Clamp(-_container.y, 0, _overlapSize.y);

			//循环滚动特别检查
			if (_loop != 0)
			{
				newPos = _container.xy;
				if (LoopCheckingCurrent())
					_containerPos += _container.xy - newPos;
			}

			draggingPane = this;
			_isHoldAreaDone = true;
			_isMouseMoved = true;

			SyncScrollBar();
			onScroll.Call();
		}

		private void __touchEnd(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			if (_touchId != evt.touchId)
				return;

			Stage.inst.onTouchMove.Remove(_touchMoveDelegate);
			Stage.inst.onTouchEnd.Remove(_touchEndDelegate);

			if (draggingPane == this)
				draggingPane = null;

			_gestureFlag = 0;

			if (!_isMouseMoved || _owner.displayObject == null || _owner.displayObject.isDisposed
				 || !_touchEffect || _inertiaDisabled)
			{
				_isMouseMoved = false;
				return;
			}

			_isMouseMoved = false;

			//更新速度
			float elapsed = (Time.unscaledTime - _lastMoveTime) * 60 - 1;
			if (elapsed > 1)
				_velocity = _velocity * Mathf.Pow(0.833f, elapsed);

			//根据速度计算目标位置和需要时间
			_tweenStart = _container.xy;
			Vector2 endPos = UpdateTargetAndDuration(_tweenStart);
			Vector2 oldChange = endPos - _tweenStart;

			//调整目标位置
			LoopCheckingTarget(ref endPos);
			if (_pageMode || _snapToItem)
				AlignPosition(ref endPos, true);

			_tweenChange = endPos - _tweenStart;
			if (_tweenChange.x == 0 && _tweenChange.y == 0)
				return;

			//如果目标位置已调整，随之调整需要时间
			if (_pageMode || _snapToItem)
			{
				FixDuration(0, oldChange.x);
				FixDuration(1, oldChange.y);
			}

			_tweening = 2;
			_tweenTime = Vector2.zero;
			Timers.inst.AddUpdate(_tweenUpdateDelegate);

			if (_container.x > UIConfig.touchDragSensitivity || _container.y > UIConfig.touchDragSensitivity)
				onPullDownRelease.Call();
			else if (_container.x < -_overlapSize.x - UIConfig.touchDragSensitivity
				|| _container.y < -_overlapSize.y - UIConfig.touchDragSensitivity)
				onPullUpRelease.Call();
		}

		private void __mouseWheel(EventContext context)
		{
			if (!_mouseWheelEnabled)
				return;

			InputEvent evt = context.inputEvent;
			int delta = evt.mouseWheelDelta;
			delta = Math.Sign(delta);
			if (_overlapSize.x > 0 && _overlapSize.y == 0)
			{
				if (_pageMode)
					SetPosX(_xPos + _pageSize.x * delta, false);
				else
					SetPosX(_xPos + _mouseWheelStep * delta, false);
			}
			else
			{
				if (_pageMode)
					SetPosY(_yPos + _pageSize.y * delta, false);
				else
					SetPosY(_yPos + _mouseWheelStep * delta, false);
			}
		}

		private void __rollOver()
		{
			ShowScrollBar(true);
		}

		private void __rollOut()
		{
			ShowScrollBar(false);
		}

		private void ShowScrollBar(bool val)
		{
			if (val)
			{
				__showScrollBar(true);
				Timers.inst.Remove(__showScrollBar);
			}
			else
				Timers.inst.Add(0.5f, 1, __showScrollBar, val);
		}

		private void __showScrollBar(object obj)
		{
			_scrollBarVisible = (bool)obj && _viewSize.x > 0 && _viewSize.y > 0;
			if (_vtScrollBar != null)
				_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
			if (_hzScrollBar != null)
				_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
		}

		/// <summary>
		/// 对当前的滚动位置进行循环滚动边界检查。当到达边界时，回退一半内容区域（循环滚动内容大小通常是真实内容大小的偶数倍）。
		/// </summary>
		/// <returns></returns>
		bool LoopCheckingCurrent()
		{
			bool changed = false;
			if (_loop == 1 && _overlapSize.x > 0)
			{
				if (_xPos < 0.001f)
				{
					_xPos += _contentSize.x / 2;
					changed = true;

				}
				else if (_xPos >= _overlapSize.x)
				{
					_xPos -= _contentSize.x / 2;
					changed = true;
				}
			}
			else if (_loop == 2 && _overlapSize.y > 0)
			{
				if (_yPos < 0.001f)
				{
					_yPos += _contentSize.y / 2;
					changed = true;
				}
				else if (_yPos >= _overlapSize.y)
				{
					_yPos += _contentSize.y / 2 - _overlapSize.y;
					changed = true;
				}
			}

			if (changed)
				_container.SetXY((int)-_xPos, (int)-_yPos);

			return changed;
		}

		/// <summary>
		/// 对目标位置进行循环滚动边界检查。当到达边界时，回退一半内容区域（循环滚动内容大小通常是真实内容大小的偶数倍）。
		/// </summary>
		/// <param name="endPos"></param>
		void LoopCheckingTarget(ref Vector2 endPos)
		{
			if (_loop == 1)
				LoopCheckingTarget(ref endPos, 0);

			if (_loop == 2)
				LoopCheckingTarget(ref endPos, 1);
		}

		void LoopCheckingTarget(ref Vector2 endPos, int axis)
		{
			if (endPos[axis] > 0)
			{
				float tmp = _tweenStart[axis] - _contentSize[axis] / 2;
				if (tmp <= 0 && tmp >= -_overlapSize[axis])
				{
					endPos[axis] -= _contentSize[axis] / 2;
					_tweenStart[axis] = tmp;
				}
			}
			else if (endPos[axis] < -_overlapSize[axis])
			{
				float tmp = _tweenStart[axis] + _contentSize[axis] / 2;
				if (tmp <= 0 && tmp >= -_overlapSize.x)
				{
					endPos[axis] += _contentSize[axis] / 2;
					_tweenStart[axis] = tmp;
				}
			}
		}

		/// <summary>
		/// 从oldPos滚动至pos，调整pos位置对齐页面、对齐item等（如果需要）。
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="inertialScrolling"></param>
		void AlignPosition(ref Vector2 pos, bool inertialScrolling)
		{
			if (_pageMode)
			{
				pos.x = AlignByPage(pos.x, 0, inertialScrolling);
				pos.y = AlignByPage(pos.y, 1, inertialScrolling);
			}
			else if (_snapToItem)
			{
				float tmpX = -pos.x;
				float tmpY = -pos.y;
				_owner.GetSnappingPosition(ref tmpX, ref tmpY);
				if (pos.x < 0 && pos.x > -_overlapSize.x)
					pos.x = -tmpX;
				if (pos.y < 0 && pos.y > -_overlapSize.y)
					pos.y = -tmpY;
			}
		}

		/// <summary>
		/// 从oldPos滚动至pos，调整目标位置到对齐页面。
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="axis"></param>
		/// <param name="inertialScrolling"></param>
		/// <returns></returns>
		float AlignByPage(float pos, int axis, bool inertialScrolling)
		{
			int page;

			if (pos > 0)
				page = 0;
			else if (pos < -_overlapSize[axis])
				page = Mathf.CeilToInt(_contentSize[axis] / _pageSize[axis]) - 1;
			else
			{
				page = Mathf.FloorToInt(-pos / _pageSize[axis]);
				float change = inertialScrolling ? (pos - _containerPos[axis]) : (pos - _container.xy[axis]);
				float testPageSize = Mathf.Min(_pageSize[axis], _contentSize[axis] - (page + 1) * _pageSize[axis]);
				float delta = -pos - page * _pageSize[axis];

				//页面吸附策略
				if (Mathf.Abs(change) > _pageSize[axis])//如果滚动距离超过1页,则需要超过页面的一半，才能到更下一页
				{
					if (delta > testPageSize * 0.5f)
						page++;
				}
				else //否则只需要页面的1/3，当然，需要考虑到左移和右移的情况
				{
					if (delta > testPageSize * (change < 0 ? 0.3f : 0.7f))
						page++;
				}

				//重新计算终点
				pos = -page * _pageSize[axis];
				if (pos < -_overlapSize[axis]) //最后一页未必有pageSize那么大
					pos = -_overlapSize[axis];
			}

			//惯性滚动模式下，会增加判断尽量不要滚动超过一页
			if (inertialScrolling)
			{
				float oldPos = _tweenStart[axis];
				int oldPage;
				if (oldPos > 0)
					oldPage = 0;
				else if (oldPos < -_overlapSize[axis])
					oldPage = Mathf.CeilToInt(_contentSize[axis] / _pageSize[axis]) - 1;
				else
					oldPage = Mathf.FloorToInt(-oldPos / _pageSize[axis]);
				int startPage = Mathf.FloorToInt(-_containerPos[axis] / _pageSize[axis]);
				if (Mathf.Abs(page - startPage) > 1 && Mathf.Abs(oldPage - startPage) <= 1)
				{
					if (page > startPage)
						page = startPage + 1;
					else
						page = startPage - 1;
					pos = -page * _pageSize[axis];
				}
			}

			return pos;
		}

		/// <summary>
		/// 根据当前速度，计算滚动的目标位置，以及到达时间。
		/// </summary>
		/// <param name="orignPos"></param>
		/// <returns></returns>
		Vector2 UpdateTargetAndDuration(Vector2 orignPos)
		{
			Vector2 ret = Vector2.zero;
			ret.x = UpdateTargetAndDuration(orignPos.x, 0);
			ret.y = UpdateTargetAndDuration(orignPos.y, 1);
			return ret;
		}

		float UpdateTargetAndDuration(float pos, int axis)
		{
			float v = _velocity[axis];
			float duration = 0;
			if (pos > 0)
				pos = 0;
			else if (pos < -_overlapSize[axis])
				pos = -_overlapSize[axis];
			else
			{
				//以屏幕像素为基准
				float v2 = Mathf.Abs(v) * _velocityScale;
				//在移动设备上，需要对不同分辨率做一个适配，我们的速度判断以1136分辨率为基准
				if (Stage.touchScreen)
					v2 *= 1136f / Mathf.Max(Screen.width, Screen.height);
				//这里有一些阈值的处理，因为在低速内，不希望产生较大的滚动（甚至不滚动）
				float ratio = 0;
				if (_pageMode || !Stage.touchScreen)
				{
					if (v2 > 500)
						ratio = Mathf.Pow((v2 - 500) / 500, 2);
				}
				else
				{
					if (v2 > 1000)
						ratio = Mathf.Pow((v2 - 1000) / 1000, 2);
				}

				if (ratio != 0)
				{
					if (ratio > 1)
						ratio = 1;

					v2 *= ratio;
					v *= ratio;
					_velocity[axis] = v;

					//算法：v*（_decelerationRate的n次幂）= 60，即在n帧后速度降为60（假设每秒60帧）。
					duration = Mathf.Log(60 / v2, _decelerationRate) / 60;

					//计算距离要使用本地速度
					//理论公式貌似滚动的距离不够，改为经验公式
					//float change = (int)((v/ 60 - 1) / (1 - _decelerationRate));
					float change = (int)(v * duration * 0.4f);
					pos += change;
				}
			}

			if (duration < TWEEN_TIME_DEFAULT)
				duration = TWEEN_TIME_DEFAULT;
			_tweenDuration[axis] = duration;

			return pos;
		}

		/// <summary>
		/// 根据修改后的tweenChange重新计算减速时间。
		/// </summary>
		void FixDuration(int axis, float oldChange)
		{
			if (_tweenChange[axis] == 0 || Mathf.Abs(_tweenChange[axis]) >= Mathf.Abs(oldChange))
				return;

			float newDuration = Mathf.Abs(_tweenChange[axis] / oldChange) * _tweenDuration[axis];
			if (newDuration < TWEEN_TIME_DEFAULT)
				newDuration = TWEEN_TIME_DEFAULT;

			_tweenDuration[axis] = newDuration;
		}

		void KillTween()
		{
			if (_tweening == 1) //取消类型为1的tween需立刻设置到终点
			{
				_container.xy = _tweenStart + _tweenChange;
				onScroll.Call();
			}

			_tweening = 0;
			Timers.inst.Remove(_tweenUpdateDelegate);
			onScrollEnd.Call();
		}

		void TweenUpdate(object param)
		{
			if (_owner.displayObject == null || _owner.displayObject.isDisposed)
			{
				Timers.inst.Remove(_tweenUpdateDelegate);
				return;
			}

			float nx = RunTween(0);
			float ny = RunTween(1);

			_container.SetXY(nx, ny);

			if (_tweening == 2)
			{
				if (_overlapSize.x > 0)
					_xPos = Mathf.Clamp(-nx, 0, _overlapSize.x);
				if (_overlapSize.y > 0)
					_yPos = Mathf.Clamp(-ny, 0, _overlapSize.y);
			}

			if (_tweenChange.x == 0 && _tweenChange.y == 0)
			{
				_tweening = 0;
				Timers.inst.Remove(_tweenUpdateDelegate);

				LoopCheckingCurrent();

				SyncScrollBar(true);
				onScroll.Call();
				onScrollEnd.Call();
			}
			else
			{
				SyncScrollBar(false);
				onScroll.Call();
			}
		}

		float RunTween(int axis)
		{
			float newValue;
			if (_tweenChange[axis] != 0)
			{
				_tweenTime[axis] += Time.unscaledDeltaTime;
				if (_tweenTime[axis] >= _tweenDuration[axis])
				{
					newValue = _tweenStart[axis] + _tweenChange[axis];
					_tweenChange[axis] = 0;
				}
				else
				{
					float ratio = EaseFunc(_tweenTime[axis], _tweenDuration[axis]);
					newValue = _tweenStart[axis] + (int)(_tweenChange[axis] * ratio);
				}

				if (_tweening == 2 && _bouncebackEffect)
				{
					if (newValue > 20 && _tweenChange[axis] > 0
						|| newValue > 0 && _tweenChange[axis] == 0)//开始回弹
					{
						_tweenTime[axis] = 0;
						_tweenDuration[axis] = TWEEN_TIME_DEFAULT;
						_tweenChange[axis] = -newValue;
						_tweenStart[axis] = newValue;
					}
					else if (newValue < -_overlapSize[axis] - 20 && _tweenChange[axis] < 0
						|| newValue < -_overlapSize[axis] && _tweenChange[axis] == 0)//开始回弹
					{
						_tweenTime[axis] = 0;
						_tweenDuration[axis] = TWEEN_TIME_DEFAULT;
						_tweenChange[axis] = -_overlapSize[axis] - newValue;
						_tweenStart[axis] = newValue;
					}
				}
				else
				{
					if (newValue > 0)
					{
						newValue = 0;
						_tweenChange[axis] = 0;
					}
					else if (newValue < -_overlapSize[axis])
					{
						newValue = -_overlapSize[axis];
						_tweenChange[axis] = 0;
					}
				}
			}
			else
				newValue = _container.xy[axis];

			return newValue;
		}

		static float EaseFunc(float t, float d)
		{
			return (t = t / d - 1) * t * t + 1;//cubicOut
		}
	}
}
