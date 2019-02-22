using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// GGroup class.
	/// 组对象，对应编辑器里的高级组。
	/// </summary>
	public class GGroup : GObject
	{
		GroupLayoutType _layout;
		int _lineGap;
		int _columnGap;
		bool _percentReady;
		bool _boundsChanged;
		EventCallback0 _refreshDelegate;

		internal int _updating;

		public GGroup()
		{
			_refreshDelegate = EnsureBoundsCorrect;
		}

		/// <summary>
		/// Group layout type.
		/// </summary>
		public GroupLayoutType layout
		{
			get { return _layout; }
			set
			{
				if (_layout != value)
				{
					_layout = value;
					SetBoundsChangedFlag(true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int lineGap
		{
			get { return _lineGap; }
			set
			{
				if (_lineGap != value)
				{
					_lineGap = value;
					SetBoundsChangedFlag();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int columnGap
		{
			get { return _columnGap; }
			set
			{
				if (_columnGap != value)
				{
					_columnGap = value;
					SetBoundsChangedFlag();
				}
			}
		}

		/// <summary>
		/// Update group bounds.
		/// 更新组的包围.
		/// </summary>
		public void SetBoundsChangedFlag(bool childSizeChanged = false)
		{
			if (_updating == 0 && parent != null)
			{
				if (childSizeChanged)
					_percentReady = false;

				if (!_boundsChanged)
				{
					_boundsChanged = true;

					if (_layout != GroupLayoutType.None)
					{
						UpdateContext.OnBegin -= _refreshDelegate;
						UpdateContext.OnBegin += _refreshDelegate;
					}
				}
			}
		}

		public void EnsureBoundsCorrect()
		{
			if (!_boundsChanged)
				return;

			UpdateContext.OnBegin -= _refreshDelegate;
			_boundsChanged = false;

			if (parent == null)
				return;

			HandleLayout();

			UpdateBounds();
		}

		void UpdateBounds()
		{
			int cnt = parent.numChildren;
			int i;
			GObject child;
			float ax = int.MaxValue, ay = int.MaxValue;
			float ar = int.MinValue, ab = int.MinValue;
			float tmp;
			bool empty = true;

			for (i = 0; i < cnt; i++)
			{
				child = parent.GetChildAt(i);
				if (child.group != this)
					continue;

				tmp = child.xMin;
				if (tmp < ax)
					ax = tmp;
				tmp = child.yMin;
				if (tmp < ay)
					ay = tmp;
				tmp = child.xMin + child.width;
				if (tmp > ar)
					ar = tmp;
				tmp = child.yMin + child.height;
				if (tmp > ab)
					ab = tmp;

				empty = false;
			}

			if (!empty)
			{
				_updating = 1;
				SetXY(ax, ay);
				_updating = 2;
				SetSize(ar - ax, ab - ay);
			}
			else
			{
				_updating = 2;
				SetSize(0, 0);
			}

			_updating = 0;
		}

		void HandleLayout()
		{
			_updating |= 1;

			if (_layout == GroupLayoutType.Horizontal)
			{
				float curX = float.NaN;
				int cnt = parent.numChildren;
				for (int i = 0; i < cnt; i++)
				{
					GObject child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (float.IsNaN(curX))
						curX = (int)child.xMin;
					else
						child.xMin = curX;
					if (child.width != 0)
						curX += child.width + _columnGap;
				}
				if (!_percentReady)
					UpdatePercent();
			}
			else if (_layout == GroupLayoutType.Vertical)
			{
				float curY = float.NaN;
				int cnt = parent.numChildren;
				for (int i = 0; i < cnt; i++)
				{
					GObject child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (float.IsNaN(curY))
						curY = (int)child.yMin;
					else
						child.yMin = curY;
					if (child.height != 0)
						curY += child.height + _lineGap;
				}
				if (!_percentReady)
					UpdatePercent();
			}

			_updating &= 2;
		}

		void UpdatePercent()
		{
			_percentReady = true;

			int cnt = parent.numChildren;
			int i;
			GObject child;
			float size = 0;
			if (_layout == GroupLayoutType.Horizontal)
			{
				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					size += child.width;
				}

				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (size > 0)
						child._sizePercentInGroup = child.width / size;
					else
						child._sizePercentInGroup = 0;
				}
			}
			else
			{
				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					size += child.height;
				}

				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (size > 0)
						child._sizePercentInGroup = child.height / size;
					else
						child._sizePercentInGroup = 0;
				}
			}
		}

		internal void MoveChildren(float dx, float dy)
		{
			if ((_updating & 1) != 0 || parent == null)
				return;

			_updating |= 1;

			int cnt = parent.numChildren;
			int i;
			GObject child;
			for (i = 0; i < cnt; i++)
			{
				child = parent.GetChildAt(i);
				if (child.group == this)
				{
					child.SetXY(child.x + dx, child.y + dy);
				}
			}

			_updating &= 2;
		}

		internal void ResizeChildren(float dw, float dh)
		{
			if (_layout == GroupLayoutType.None || (_updating & 2) != 0 || parent == null)
				return;

			if (!_percentReady)
				UpdatePercent();

			int cnt = parent.numChildren;
			int i;
			GObject child;
			int numChildren = 0;
			float remainSize = 0;
			float remainPercent = 1;

			for (i = 0; i < cnt; i++)
			{
				child = parent.GetChildAt(i);
				if (child.group != this)
					continue;
				
				numChildren++;
			}

			if (_layout == GroupLayoutType.Horizontal)
			{
				_updating |= 2;
				remainSize = this.width - (numChildren - 1) * _columnGap;
				float curX = float.NaN;
				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (float.IsNaN(curX))
						curX = (int)child.xMin;
					else
						child.xMin = curX;
					child.SetSize(Mathf.Round(child._sizePercentInGroup / remainPercent * remainSize), child._rawHeight + dh, true);
					remainSize -= child.width;
					remainPercent -= child._sizePercentInGroup;
					curX += child.width + _columnGap;
				}
				_updating &= 1;
				UpdateBounds();
			}
			else if (_layout == GroupLayoutType.Vertical)
			{
				_updating |= 2;
				remainSize = this.height - (numChildren - 1) * _lineGap;
				float curY = float.NaN;
				for (i = 0; i < cnt; i++)
				{
					child = parent.GetChildAt(i);
					if (child.group != this)
						continue;

					if (float.IsNaN(curY))
						curY = (int)child.yMin;
					else
						child.yMin = curY;
					child.SetSize(child._rawWidth + dw, Mathf.Round(child._sizePercentInGroup / remainPercent * remainSize), true);
					remainSize -= child.height;
					remainPercent -= child._sizePercentInGroup;
					curY += child.height + _lineGap;
				}
				_updating &= 1;
				UpdateBounds();
			}
		}

		override protected void HandleAlphaChanged()
		{
			base.HandleAlphaChanged();

			if (this.underConstruct || parent == null)
				return;

			int cnt = parent.numChildren;
			float a = this.alpha;
			for (int i = 0; i < cnt; i++)
			{
				GObject child = parent.GetChildAt(i);
				if (child.group == this)
					child.alpha = a;
			}
		}

		override internal protected void HandleVisibleChanged()
		{
			if (parent == null)
				return;

			int cnt = parent.numChildren;
			for (int i = 0; i < cnt; i++)
			{
				GObject child = parent.GetChildAt(i);
				if (child.group == this)
					child.HandleVisibleChanged();
			}
		}

		override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_BeforeAdd(buffer, beginPos);

			buffer.Seek(beginPos, 5);

			_layout = (GroupLayoutType)buffer.ReadByte();
			_lineGap = buffer.ReadInt();
			_columnGap = buffer.ReadInt();
		}

		override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_AfterAdd(buffer, beginPos);

			if (!this.visible)
				HandleVisibleChanged();
		}
	}
}
