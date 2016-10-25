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
		/// </summary>
		public EventListener onScroll { get; private set; }
		public EventListener onScrollEnd { get; private set; }
		public EventListener onPullDownRelease { get; private set; }
		public EventListener onPullUpRelease { get; private set; }

		public static ScrollPane draggingPane { get; private set; }

		float _viewWidth;
		float _viewHeight;
		float _contentWidth;
		float _contentHeight;
		ScrollType _scrollType;
		float _scrollSpeed;
		float _mouseWheelSpeed;
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

		float _yPerc;
		float _xPerc;
		float _xPos;
		float _yPos;
		int _xOverlap;
		int _yOverlap;

		float _time1, _time2;
		float _y1, _y2;
		float _x1, _x2;
		float _xOffset, _yOffset;
		bool _isMouseMoved;
		Vector2 _holdAreaPoint;
		bool _isHoldAreaDone;
		int _aniFlag;
		bool _scrollBarVisible;
		int _touchId;

		ThrowTween _throwTween;
		Tweener _tweener;
		int _tweening;

		EventCallback0 _refreshDelegate;
		EventCallback1 _touchEndDelegate;
		EventCallback1 _touchMoveDelegate;

		GComponent _owner;
		Container _maskContainer;
		Container _container;
		GScrollBar _hzScrollBar;
		GScrollBar _vtScrollBar;

		static int _gestureFlag;

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
			_touchEndDelegate = __touchEnd;
			_touchMoveDelegate = __touchMove;

			_throwTween = new ThrowTween();
			_owner = owner;

			_maskContainer = new Container();
			_owner.rootContainer.AddChild(_maskContainer);

			_container = _owner.container;
			_container.x = 0;
			_container.y = 0;
			_maskContainer.AddChild(_container);

			_scrollBarMargin = scrollBarMargin;
			_scrollType = scrollType;
			_scrollSpeed = UIConfig.defaultScrollSpeed;
			_mouseWheelSpeed = _scrollSpeed * 2;
			_softnessOnTopOrLeftSide = UIConfig.allowSoftnessOnTopOrLeftSide;

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
			_holdAreaPoint = new Vector2();
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
		public GComponent owner
		{
			get { return _owner; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool bouncebackEffect
		{
			get { return _bouncebackEffect; }
			set { _bouncebackEffect = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool touchEffect
		{
			get { return _touchEffect; }
			set { _touchEffect = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool inertiaDisabled
		{
			get { return _inertiaDisabled; }
			set { _inertiaDisabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool softnessOnTopOrLeftSide
		{
			get { return _softnessOnTopOrLeftSide; }
			set { _softnessOnTopOrLeftSide = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float scrollSpeed
		{
			get { return _scrollSpeed; }
			set
			{
				_scrollSpeed = value;
				if (_scrollSpeed == 0)
					_scrollSpeed = UIConfig.defaultScrollSpeed;
				_mouseWheelSpeed = _scrollSpeed * 2;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool snapToItem
		{
			get { return _snapToItem; }
			set { _snapToItem = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool mouseWheelEnabled
		{
			get { return _mouseWheelEnabled; }
			set { _mouseWheelEnabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float percX
		{
			get { return _xPerc; }
			set { SetPercX(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPercX(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();

			value = Mathf.Clamp01(value);
			if (value != _xPerc)
			{
				_xPerc = value;
				_xPos = _xPerc * _xOverlap;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float percY
		{
			get { return _yPerc; }
			set { SetPercY(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPercY(float value, bool ani)
		{
			_owner.EnsureBoundsCorrect();

			value = Mathf.Clamp01(value);
			if (value != _yPerc)
			{
				_yPerc = value;
				_yPos = _yPerc * _yOverlap;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float posX
		{
			get { return _xPos; }
			set { SetPosX(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPosX(float value, bool ani)
		{
			if (value != _xPos)
			{
				_xPos = Mathf.Clamp(value, 0, _xOverlap);
				_xPerc = _xOverlap == 0 ? 0 : _xPos / _xOverlap;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float posY
		{
			get { return _yPos; }
			set { SetPosY(value, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="ani"></param>
		public void SetPosY(float value, bool ani)
		{
			if (value != _yPos)
			{
				_yPos = Mathf.Clamp(value, 0, _yOverlap);
				_yPerc = _yOverlap == 0 ? 0 : _yPos / _yOverlap;
				PosChanged(ani);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isBottomMost
		{
			get { return _yPerc == 1 || _yOverlap == 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isRightMost
		{
			get { return _xPerc == 1 || _xOverlap == 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int currentPageX
		{
			get { return _pageMode ? Mathf.FloorToInt(_xPos / _pageSize.x) : 0; }
			set
			{
				if (_xOverlap > 0)
					this.SetPosX(value * _pageSize.x, false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int currentPageY
		{
			get { return _pageMode ? Mathf.FloorToInt(_yPos / _pageSize.y) : 0; }
			set
			{
				if (_yOverlap > 0)
					this.SetPosY(value * _pageSize.y, false);
			}
		}

		/// <summary>
		/// 这个值与PosX不同在于，他反映的是实时位置，而PosX在有缓动过程的情况下只是终值。
		/// </summary>
		public float scrollingPosX
		{
			get
			{
				if (_xOverlap == 0)
					return 0;

				float mx = _container.x;
				if (mx > 0)
					return 0;
				else if (mx < -_xOverlap)
					return _xOverlap;
				else
					return -mx;
			}
		}

		/// <summary>
		/// 这个值与PosY不同在于，他反映的是实时位置，而PosY在有缓动过程的情况下只是终值。
		/// </summary>
		public float scrollingPosY
		{
			get
			{
				if (_yOverlap == 0)
					return 0;

				float my = _container.y;
				if (my > 0)
					return 0;
				else if (my < -_yOverlap)
					return _yOverlap;
				else
					return -my;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float contentWidth
		{
			get
			{
				return _contentWidth;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float contentHeight
		{
			get
			{
				return _contentHeight;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float viewWidth
		{
			get { return _viewWidth; }
			set
			{
				value = value + _owner.margin.left + _owner.margin.right;
				if (_vtScrollBar != null)
					value += _vtScrollBar.width;
				_owner.width = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float viewHeight
		{
			get { return _viewHeight; }
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
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollUp(float speed, bool ani)
		{
			if (_pageMode)
				SetPosY(_yPos - _pageSize.y * speed, ani);
			else
				SetPosY(_yPos - _scrollSpeed * speed, ani);
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
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollDown(float speed, bool ani)
		{
			if (_pageMode)
				SetPosY(_yPos + _pageSize.y * speed, ani);
			else
				SetPosY(_yPos + _scrollSpeed * speed, ani);
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
		public void ScrollLeft(float speed, bool ani)
		{
			if (_pageMode)
				SetPosX(_xPos - _pageSize.x * speed, ani);
			else
				SetPosX(_xPos - _scrollSpeed * speed, ani);
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
		/// <param name="speed"></param>
		/// <param name="ani"></param>
		public void ScrollRight(float speed, bool ani)
		{
			if (_pageMode)
				SetPosX(_xPos + _pageSize.x * speed, ani);
			else
				SetPosX(_xPos + _scrollSpeed * speed, ani);
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

			if (_yOverlap > 0)
			{
				float bottom = _yPos + _viewHeight;
				if (setFirst || rect.y <= _yPos || rect.height >= _viewHeight)
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
					else if (rect.height <= _viewHeight / 2)
						SetPosY(rect.y + rect.height * 2 - _viewHeight, ani);
					else
						SetPosY(rect.y + rect.height - _viewHeight, ani);
				}
			}
			if (_xOverlap > 0)
			{
				float right = _xPos + _viewWidth;
				if (setFirst || rect.x <= _xPos || rect.width >= _viewWidth)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					SetPosX(rect.x, ani);
				}
				else if (rect.x + rect.width > right)
				{
					if (_pageMode)
						this.SetPosX(Mathf.Floor(rect.x / _pageSize.x) * _pageSize.x, ani);
					else if (rect.width <= _viewWidth / 2)
						SetPosX(rect.x + rect.width * 2 - _viewWidth, ani);
					else
						SetPosX(rect.x + rect.width - _viewWidth, ani);
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
			if (_yOverlap > 0)
			{
				float dist = obj.y + _container.y;
				if (dist < -obj.height - 20 || dist > _viewHeight + 20)
					return false;
			}
			if (_xOverlap > 0)
			{
				float dist = obj.x + _container.x;
				if (dist < -obj.width - 20 || dist > _viewWidth + 20)
					return false;
			}

			return true;
		}

		/// <summary>
		/// 当滚动面板处于拖拽滚动状态或即将进入进入拖拽状态时，可以调用此方法停止或禁止本次拖拽。
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

			_viewWidth = aWidth;
			_viewHeight = aHeight;
			if (_hzScrollBar != null && !_hScrollNone)
				_viewHeight -= _hzScrollBar.height;
			if (_vtScrollBar != null && !_vScrollNone)
				_viewWidth -= _vtScrollBar.width;
			_viewWidth -= (_owner.margin.left + _owner.margin.right);
			_viewHeight -= (_owner.margin.top + _owner.margin.bottom);

			_viewWidth = Mathf.Max(1, _viewWidth);
			_viewHeight = Mathf.Max(1, _viewHeight);
			_pageSize = new Vector2(_viewWidth, _viewHeight);

			HandleSizeChanged();
		}

		internal void SetContentSize(float aWidth, float aHeight)
		{
			if (Mathf.Approximately(_contentWidth, aWidth) && Mathf.Approximately(_contentHeight, aHeight))
				return;

			_contentWidth = aWidth;
			_contentHeight = aHeight;
			HandleSizeChanged();
		}

		internal void ChangeContentSizeOnScrolling(float deltaWidth, float deltaHeight, float deltaPosX, float deltaPosY)
		{
			_contentWidth += deltaWidth;
			_contentHeight += deltaHeight;

			if (_isMouseMoved)
			{
				if (deltaPosX != 0)
					_container.x -= deltaPosX;
				if (deltaPosY != 0)
					_container.y -= deltaPosY;

				ValidateHolderPos();

				_xOffset += deltaPosX;
				_yOffset += deltaPosY;

				_y1 = _y2 = _container.y;
				_x1 = _x2 = _container.x;

				_yPos = -_container.y;
				_xPos = -_container.x;
			}
			else if (_tweening == 2)
			{
				if (deltaPosX != 0)
				{
					_container.x -= deltaPosX;
					_throwTween.start.x -= deltaPosX;
				}
				if (deltaPosY != 0)
				{
					_container.y -= deltaPosY;
					_throwTween.start.y -= deltaPosY;
				}
			}

			HandleSizeChanged(true);
		}

		void HandleSizeChanged(bool onScrolling = false)
		{
			if (_displayInDemand)
			{
				if (_vtScrollBar != null)
				{
					if (_contentHeight <= _viewHeight)
					{
						if (!_vScrollNone)
						{
							_vScrollNone = true;
							_viewWidth += _vtScrollBar.width;
						}
					}
					else
					{
						if (_vScrollNone)
						{
							_vScrollNone = false;
							_viewWidth -= _vtScrollBar.width;
						}
					}
				}
				if (_hzScrollBar != null)
				{
					if (_contentWidth <= _viewWidth)
					{
						if (!_hScrollNone)
						{
							_hScrollNone = true;
							_viewHeight += _hzScrollBar.height;
						}
					}
					else
					{
						if (_hScrollNone)
						{
							_hScrollNone = false;
							_viewHeight -= _hzScrollBar.height;
						}
					}
				}
			}

			if (_vtScrollBar != null)
			{
				if (_viewHeight < _vtScrollBar.minSize)
					_vtScrollBar.displayObject.visible = false;
				else
				{
					_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
					if (_contentHeight == 0)
						_vtScrollBar.displayPerc = 0;
					else
						_vtScrollBar.displayPerc = Math.Min(1, _viewHeight / _contentHeight);
				}
			}
			if (_hzScrollBar != null)
			{
				if (_viewWidth < _hzScrollBar.minSize)
					_hzScrollBar.displayObject.visible = false;
				else
				{
					_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
					if (_contentWidth == 0)
						_hzScrollBar.displayPerc = 0;
					else
						_hzScrollBar.displayPerc = Math.Min(1, _viewWidth / _contentWidth);
				}
			}

			if (!_maskDisabled)
				_maskContainer.clipRect = new Rect(-owner._alignOffset.x, -owner._alignOffset.y, _viewWidth, _viewHeight);

			if (_scrollType == ScrollType.Horizontal || _scrollType == ScrollType.Both)
				_xOverlap = Mathf.CeilToInt(Math.Max(0, _contentWidth - _viewWidth));
			else
				_xOverlap = 0;
			if (_scrollType == ScrollType.Vertical || _scrollType == ScrollType.Both)
				_yOverlap = Mathf.CeilToInt(Math.Max(0, _contentHeight - _viewHeight));
			else
				_yOverlap = 0;

			if (_tweening == 2)
			{
				//如果是在缓动过程中，除发生边界改变，这里不做任何操作，让缓动处理去设置正确的percent和pos
				if (_yOverlap == 0)
				{
					_yPos = 0;
					_yPerc = 0;
				}

				if (_xOverlap == 0)
				{
					_xPos = 0;
					_xPerc = 0;
				}
			}
			else if (onScrolling)
			{
				//如果改变大小是在滚动的过程中发生的，那么保持位置不变，修改percent，但边界位置除外
				if (_yOverlap > 0)
				{
					if (_yPerc == 0 || _yPerc == 1)
					{
						_container.y = -_yPerc * _yOverlap;
						_yPos = -_container.y;
					}
					else
						_yPerc = _yPos / _yOverlap;
				}
				else
				{
					_yPos = 0;
					_yPerc = 0;
				}

				if (_xOverlap > 0)
				{
					if (_xPerc == 0 || _xPerc == 1)
					{
						_container.x = -_xPerc * _xOverlap;
						_xPos = -_container.x;
					}
					else
						_xPerc = _xPos / _xOverlap;
				}
				else
				{
					_xPos = 0;
					_xPerc = 0;
				}
			}
			else
			{
				//保持位置不变，修改percent
				if (_yOverlap > 0)
				{
					_yPerc = _yPos / _yOverlap;
				}
				else
				{
					_yPos = 0;
					_yPerc = 0;
				}

				if (_xOverlap > 0)
				{
					_xPerc = _xPos / _xOverlap;
				}
				else
				{
					_xPos = 0;
					_xPerc = 0;
				}

				ValidateHolderPos();
			}

			if (_vtScrollBar != null)
				_vtScrollBar.scrollPerc = _yPerc;
			if (_hzScrollBar != null)
				_hzScrollBar.scrollPerc = _xPerc;

			UpdateClipSoft();
		}

		void ValidateHolderPos()
		{
			if (_yOverlap == 0 || _container.y > 0)
				_container.y = 0;
			else if (_container.y < -_yOverlap)
				_container.y = -_yOverlap;

			if (_xOverlap == 0 || _container.x > 0)
				_container.x = 0;
			else if (_container.x < -_xOverlap)
				_container.x = -_xOverlap;
		}

		internal void UpdateClipSoft()
		{
			Vector2 softness = _owner.clipSoftness;
			if (softness.x != 0 || softness.y != 0)
			{
				_maskContainer.clipSoftness = new Vector4(
					//左边缘和上边缘感觉不需要效果，所以注释掉
					(_xPerc < 0.01 || !_softnessOnTopOrLeftSide) ? 0 : softness.x,
					(_yPerc < 0.01 || !_softnessOnTopOrLeftSide) ? 0 : softness.y,
					(_xOverlap == 0 || _xPerc > 0.99) ? 0 : softness.x,
					(_yOverlap == 0 || _yPerc > 0.99) ? 0 : softness.y);
			}
			else
				_maskContainer.clipSoftness = null;
		}

		private void PosChanged(bool ani)
		{
			if (_aniFlag == 0)
				_aniFlag = ani ? 1 : -1;
			else if (_aniFlag == 1 && !ani)
				_aniFlag = -1;

			_needRefresh = true;

			UpdateContext.OnBegin -= _refreshDelegate;
			UpdateContext.OnBegin += _refreshDelegate;

			//如果在甩手指滚动过程中用代码重新设置滚动位置，要停止滚动
			if (_tweening == 2) //kill throw tween only
				KillTween();
		}

		private void KillTween()
		{
			if (_tweening == 1)
			{
				_tweener.Complete();
			}
			else
			{
				_tweener.Kill();
				_tweener = null;
				_tweening = 0;

				ValidateHolderPos();
				OnScrollEnd();
				onScrollEnd.Call();
			}
		}

		private void Refresh()
		{
			if (_owner.displayObject == null || _owner.displayObject.isDisposed)
				return;

			_needRefresh = false;
			UpdateContext.OnBegin -= _refreshDelegate;

			if (_pageMode)
			{
				int page;
				float delta;
				if (_yOverlap > 0 && _yPerc != 1 && _yPerc != 0)
				{
					page = Mathf.FloorToInt(_yPos / _pageSize.y);
					delta = _yPos - page * _pageSize.y;
					if (delta > _pageSize.y / 2)
						page++;
					_yPos = page * _pageSize.y;
					if (_yPos > _yOverlap)
					{
						_yPos = _yOverlap;
						_yPerc = 1;
					}
					else
						_yPerc = _yPos / _yOverlap;
				}

				if (_xOverlap > 0 && _xPerc != 1 && _xPerc != 0)
				{
					page = Mathf.FloorToInt(_xPos / _pageSize.x);
					delta = _xPos - page * _pageSize.x;
					if (delta > _pageSize.x / 2)
						page++;
					_xPos = page * _pageSize.x;
					if (_xPos > _xOverlap)
					{
						_xPos = _xOverlap;
						_xPerc = 1;
					}
					else
						_xPerc = _xPos / _xOverlap;
				}
			}
			else if (_snapToItem)
			{
				float tmpX = _xPerc == 1 ? 0 : _xPos;
				float tmpY = _yPerc == 1 ? 0 : _yPos;
				_owner.GetSnappingPosition(ref tmpX, ref tmpY);
				if (_xPerc != 1 && !Mathf.Approximately(tmpX, _xPos))
				{
					_xPos = tmpX;
					_xPerc = _xPos / _xOverlap;
					if (_xPerc > 1)
					{
						_xPerc = 1;
						_xPos = _xOverlap;
					}
				}
				if (_yPerc != 1 && !Mathf.Approximately(tmpY, _yPos))
				{
					_yPos = tmpY;
					_yPerc = _yPos / _yOverlap;
					if (_yPerc > 1)
					{
						_yPerc = 1;
						_yPos = _yOverlap;
					}
				}
			}

			Refresh2();

			onScroll.Call();
			if (_needRefresh) //user change scroll pos in on scroll
			{
				_needRefresh = false;
				UpdateContext.OnBegin -= _refreshDelegate;

				Refresh2();
			}

			_aniFlag = 0;
		}

		void Refresh2()
		{
			float contentXLoc = (int)_xPos;
			float contentYLoc = (int)_yPos;

			if (_aniFlag == 1 && !_isMouseMoved)
			{
				float toX = _container.x;
				float toY = _container.y;

				if (_yOverlap > 0)
				{
					toY = -contentYLoc;
				}
				else
				{
					if (_container.y != 0)
						_container.y = 0;
				}
				if (_xOverlap > 0)
				{
					toX = -contentXLoc;
				}
				else
				{
					if (_container.x != 0)
						_container.x = 0;
				}

				if (toX != _container.x || toY != _container.y)
				{
					if (_tweener != null)
						KillTween();

					_tweener = DOTween.To(() => _container.xy, v => _container.xy = v, new Vector2(toX, toY), 0.5f)
						.SetEase(Ease.OutCubic)
						.SetUpdate(true)
						.OnUpdate(__tweenUpdate)
						.OnComplete(__tweenComplete);
					_tweening = 1;
				}
			}
			else
			{
				if (_tweener != null)
					KillTween();

				//如果在拖动的过程中Refresh，这里要进行处理，保证拖动继续正常进行
				if (_isMouseMoved)
				{
					_xOffset += _container.x - (-contentXLoc);
					_yOffset += _container.y - (-contentYLoc);
				}

				_container.SetXY(-contentXLoc, -contentYLoc);

				//如果在拖动的过程中Refresh，这里要进行处理，保证手指离开是滚动正常进行
				if (_isMouseMoved)
				{
					_y1 = _y2 = _container.y;
					_x1 = _x2 = _container.x;
				}

				if (_vtScrollBar != null)
					_vtScrollBar.scrollPerc = _yPerc;
				if (_hzScrollBar != null)
					_hzScrollBar.scrollPerc = _xPerc;

				UpdateClipSoft();
			}
		}

		void SyncPos()
		{
			if (_xOverlap > 0)
			{
				float mx = _container.x;
				if (mx > 0)
					_xPos = 0;
				else if (mx < -_xOverlap)
					_xPos = -_xOverlap;
				else
					_xPos = -mx;

				_xPerc = Mathf.Clamp01(_xPos / _xOverlap);
			}

			if (_yOverlap > 0)
			{
				float my = _container.y;
				if (my > 0)
					_yPos = 0;
				else if (my < -_yOverlap)
					_yPos = -_yOverlap;
				else
					_yPos = -my;

				_yPerc = Mathf.Clamp01(_yPos / _yOverlap);
			}
		}

		private float CalcYPerc()
		{
			if (_yOverlap == 0)
				return 0;

			float my = _container.y;
			float currY;
			if (my > 0)
				currY = 0;
			else if (my < -_yOverlap)
				currY = _yOverlap;
			else
				currY = -my;
			return currY / _yOverlap;
		}

		private float CalcXPerc()
		{
			if (_xOverlap == 0)
				return 0;

			float mx = _container.x;
			float currX;
			if (mx > 0)
				currX = 0;
			else if (mx < -_xOverlap)
				currX = _xOverlap;
			else
				currX = -mx;

			return currX / _xOverlap;
		}

		private void OnScrolling()
		{
			//这里不能直接使用_xPerc或者_yPerc，因为滚动可能处于过渡状态
			if (_vtScrollBar != null)
			{
				_vtScrollBar.scrollPerc = CalcYPerc();
				if (_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}
			if (_hzScrollBar != null)
			{
				_hzScrollBar.scrollPerc = CalcXPerc();
				if (_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}

			UpdateClipSoft();
		}

		private void OnScrollEnd()
		{
			if (_vtScrollBar != null)
			{
				if (_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}
			if (_hzScrollBar != null)
			{
				if (_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}

			UpdateClipSoft();
		}

		private void __touchBegin(EventContext context)
		{
			if (!_touchEffect)
				return;

			InputEvent evt = context.inputEvent;
			_touchId = evt.touchId;
			Vector2 pt = _owner.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (_tweener != null)
			{
				KillTween();
				Stage.inst.CancelClick(_touchId);
			}

			_y1 = _y2 = _container.y;
			_yOffset = pt.y - _container.y;

			_x1 = _x2 = _container.x;
			_xOffset = pt.x - _container.x;

			_time1 = _time2 = Time.time;
			_holdAreaPoint.x = pt.x;
			_holdAreaPoint.y = pt.y;
			_isHoldAreaDone = false;
			_isMouseMoved = false;

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

			Vector2 pt = _owner.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (float.IsNaN(pt.x))
				return;

			int sensitivity;
			if (Stage.touchScreen)
				sensitivity = UIConfig.touchScrollSensitivity;
			else
				sensitivity = 8;

			float diff;
			bool sv = false, sh = false, st = false;

			if (_scrollType == ScrollType.Vertical)
			{
				if (!_isHoldAreaDone)
				{
					//表示正在监测垂直方向的手势
					_gestureFlag |= 1;

					diff = Mathf.Abs(_holdAreaPoint.y - pt.y);
					if (diff < sensitivity)
						return;

					if ((_gestureFlag & 2) != 0) //已经有水平方向的手势在监测，那么我们用严格的方式检查是不是按垂直方向移动，避免冲突
					{
						float diff2 = Mathf.Abs(_holdAreaPoint.x - pt.x);
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

					diff = Mathf.Abs(_holdAreaPoint.x - pt.x);
					if (diff < sensitivity)
						return;

					if ((_gestureFlag & 1) != 0)
					{
						float diff2 = Mathf.Abs(_holdAreaPoint.y - pt.y);
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
					diff = Mathf.Abs(_holdAreaPoint.y - pt.y);
					if (diff < sensitivity)
					{
						diff = Mathf.Abs(_holdAreaPoint.x - pt.x);
						if (diff < sensitivity)
							return;
					}
				}

				sv = sh = true;
			}

			float t = Time.time;
			if (t - _time2 > 0.05f)
			{
				_time2 = _time1;
				_time1 = t;
				st = true;
			}

			if (sv)
			{
				float y = pt.y - _yOffset;
				if (y > 0)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.y = 0;
					else
						_container.y = (int)(y * 0.5f);
				}
				else if (y < -_yOverlap)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.y = -(int)_yOverlap;
					else
						_container.y = (int)((y - _yOverlap) * 0.5f);
				}
				else
				{
					_container.y = y;
				}

				if (st)
				{
					_y2 = _y1;
					_y1 = _container.y;
				}
			}

			if (sh)
			{
				float x = pt.x - _xOffset;
				if (x > 0)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.x = 0;
					else
						_container.x = (int)(x * 0.5f);
				}
				else if (x < 0 - _xOverlap)
				{
					if (!_bouncebackEffect || _inertiaDisabled)
						_container.x = -(int)_xOverlap;
					else
						_container.x = (int)((x - _xOverlap) * 0.5f);
				}
				else
				{
					_container.x = x;
				}

				if (st)
				{
					_x2 = _x1;
					_x1 = _container.x;
				}
			}

			SyncPos();

			draggingPane = this;
			_isHoldAreaDone = true;
			_isMouseMoved = true;
			OnScrolling();

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
			float time = Time.time - _time2;
			if (time == 0)
				time = 0.001f;
			float yVelocity = (_container.y - _y2) / time;
			float xVelocity = (_container.x - _x2) / time;
			float duration = 0.3f;

			_throwTween.start.x = _container.x;
			_throwTween.start.y = _container.y;

			Vector2 change1, change2;
			float endX = 0, endY = 0;
			int fireRelease = 0;

			if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Horizontal)
			{
				if (_container.x > UIConfig.touchDragSensitivity)
					fireRelease = 1;
				else if (_container.x < -_xOverlap - UIConfig.touchDragSensitivity)
					fireRelease = 2;

				change1.x = ThrowTween.CalculateChange(xVelocity, duration);
				change2.x = 0;
				endX = _container.x + change1.x;

				if (_pageMode && endX < 0 && endX > -_xOverlap)
				{
					int page = Mathf.FloorToInt(-endX / _pageSize.x);
					float testPageSize = Mathf.Min(_pageSize.x, _contentWidth - (page + 1) * _pageSize.x);
					float delta = -endX - page * _pageSize.x;
					//页面吸附策略
					if (Mathf.Abs(change1.x) > _pageSize.x)//如果滚动距离超过1页,则需要超过页面的一半，才能到更下一页
					{
						if (delta > testPageSize * 0.5f)
							page++;
					}
					else //否则只需要页面的1/3，当然，需要考虑到左移和右移的情况
					{
						if (delta > testPageSize * (change1.x < 0 ? 0.3f : 0.7f))
							page++;
					}

					//重新计算终点
					endX = -page * _pageSize.x;
					if (endX < -_xOverlap) //最后一页未必有_pageSizeH那么大
						endX = -_xOverlap;

					change1.x = endX - _container.x;
				}
			}
			else
				change1.x = change2.x = 0;

			if (_scrollType == ScrollType.Both || _scrollType == ScrollType.Vertical)
			{
				if (_container.y > UIConfig.touchDragSensitivity)
					fireRelease = 1;
				else if (_container.y < -_yOverlap - UIConfig.touchDragSensitivity)
					fireRelease = 2;

				change1.y = ThrowTween.CalculateChange(yVelocity, duration);
				change2.y = 0;
				endY = _container.y + change1.y;

				if (_pageMode && endY < 0 && endY > -_yOverlap)
				{
					int page = Mathf.FloorToInt(-endY / _pageSize.y);
					float testPageSize = Mathf.Min(_pageSize.y, _contentHeight - (page + 1) * _pageSize.y);
					float delta = -endY - page * _pageSize.y;
					if (Mathf.Abs(change1.y) > _pageSize.y)
					{
						if (delta > testPageSize * 0.5f)
							page++;
					}
					else
					{
						if (delta > testPageSize * (change1.y < 0 ? 0.3f : 0.7f))
							page++;
					}

					endY = -page * _pageSize.y;
					if (endY < -_yOverlap)
						endY = -_yOverlap;

					change1.y = endY - _container.y;
				}
			}
			else
				change1.y = change2.y = 0;

			if (_snapToItem && !_pageMode)
			{
				endX = -endX;
				endY = -endY;
				_owner.GetSnappingPosition(ref endX, ref endY);
				endX = -endX;
				endY = -endY;
				change1.x = endX - _container.x;
				change1.y = endY - _container.y;
			}

			if (_bouncebackEffect)
			{
				if (endX > 0)
					change2.x = 0 - _container.x - change1.x;
				else if (endX < -_xOverlap)
					change2.x = -_xOverlap - _container.x - change1.x;

				if (endY > 0)
					change2.y = 0 - _container.y - change1.y;
				else if (endY < -_yOverlap)
					change2.y = -_yOverlap - _container.y - change1.y;
			}
			else
			{
				if (endX > 0)
					change1.x = 0 - _container.x;
				else if (endX < -_xOverlap)
					change1.x = -_xOverlap - _container.x;

				if (endY > 0)
					change1.y = 0 - _container.y;
				else if (endY < -_yOverlap)
					change1.y = -_yOverlap - _container.y;
			}

			_throwTween.value = 0;
			_throwTween.change1 = change1;
			_throwTween.change2 = change2;

			if (_tweener != null)
				KillTween();

			_tweening = 2;
			_tweener = DOTween.To(() => _throwTween.value, v => _throwTween.value = v, 1, duration)
				.SetEase(Ease.OutCubic)
				.SetUpdate(true)
				.OnUpdate(__tweenUpdate2)
				.OnComplete(__tweenComplete2);

			if (fireRelease == 1)
				onPullDownRelease.Call();
			else if (fireRelease == 2)
				onPullUpRelease.Call();
		}

		private void __mouseWheel(EventContext context)
		{
			if (!_mouseWheelEnabled)
				return;

			InputEvent evt = context.inputEvent;
			int delta = evt.mouseWheelDelta;
			delta = Math.Sign(delta);
			if (_xOverlap > 0 && _yOverlap == 0)
			{
				if (_pageMode)
					SetPosX(_xPos + _pageSize.x * delta, false);
				else
					SetPosX(_xPos + _mouseWheelSpeed * delta, false);
			}
			else
			{
				if (_pageMode)
					SetPosY(_yPos + _pageSize.y * delta, false);
				else
					SetPosY(_yPos + _mouseWheelSpeed * delta, false);
			}
		}

		private void __rollOver(object e)
		{
			ShowScrollBar(true);
		}

		private void __rollOut(object e)
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
			_scrollBarVisible = (bool)obj && _viewWidth > 0 && _viewHeight > 0;
			if (_vtScrollBar != null)
				_vtScrollBar.displayObject.visible = _scrollBarVisible && !_vScrollNone;
			if (_hzScrollBar != null)
				_hzScrollBar.displayObject.visible = _scrollBarVisible && !_hScrollNone;
		}

		private void __tweenUpdate()
		{
			OnScrolling();
			onScroll.Call();
		}

		private void __tweenComplete()
		{
			_tweener = null;
			_tweening = 0;

			OnScrollEnd();
			onScroll.Call();
		}

		private void __tweenUpdate2()
		{
			_throwTween.Update(_container);

			SyncPos();
			OnScrolling();
			onScroll.Call();
		}

		private void __tweenComplete2()
		{
			_tweener = null;
			_tweening = 0;

			ValidateHolderPos();
			SyncPos();
			OnScrollEnd();
			onScroll.Call();
			onScrollEnd.Call();
		}

		class ThrowTween
		{
			public float value;
			public Vector2 start;
			public Vector2 change1, change2;

			const float checkpoint = 0.05f;

			public void Update(DisplayObject obj)
			{
				obj.SetXY((int)(start.x + change1.x * value + change2.x * value * value), (int)(start.y + change1.y * value + change2.y * value * value));
			}

			static public float CalculateChange(float velocity, float duration)
			{
				return (duration * checkpoint * velocity) / easeOutCubic(checkpoint, 0, 1, 1);
			}

			static float easeOutCubic(float t, float b, float c, float d)
			{
				return c * ((t = t / d - 1) * t * t + 1) + b;
			}
		}
	}
}
