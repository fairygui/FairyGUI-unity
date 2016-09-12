namespace FairyGUI
{
	/// <summary>
	/// GGroup class.
	/// 组对象，对应编辑器里的高级组。
	/// </summary>
	public class GGroup : GObject
	{
		internal bool _updating;
		bool _empty;

		public GGroup()
		{
		}

		/// <summary>
		/// Update group bounds.
		/// 更新组的包围.
		/// </summary>
		public void UpdateBounds()
		{
			if (_updating || parent == null)
				return;

			int cnt = parent.numChildren;
			int i;
			GObject child;
			float ax = int.MaxValue, ay = int.MaxValue;
			float ar = int.MinValue, ab = int.MinValue;
			float tmp;
			_empty = true;
			for (i = 0; i < cnt; i++)
			{
				child = parent.GetChildAt(i);
				if (child.group == this)
				{
					tmp = child.x;
					if (tmp < ax)
						ax = tmp;
					tmp = child.y;
					if (tmp < ay)
						ay = tmp;
					tmp = child.x + child.width;
					if (tmp > ar)
						ar = tmp;
					tmp = child.y + child.height;
					if (tmp > ab)
						ab = tmp;
					_empty = false;
				}
			}

			_updating = true;
			if (!_empty)
			{
				SetXY(ax, ay);
				SetSize(ar - ax, ab - ay);
			}
			else
				SetSize(0, 0);
			_updating = false;
		}

		internal void MoveChildren(float dx, float dy)
		{
			if (_updating || parent == null)
				return;

			_updating = true;
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
			_updating = false;
		}

		override protected void UpdateAlpha()
		{
			base.UpdateAlpha();

			if (this.underConstruct)
				return;

			int cnt = parent.numChildren;
			int i;
			GObject child;
			for (i = 0; i < cnt; i++)
			{
				child = parent.GetChildAt(i);
				if (child.group == this)
					child.alpha = this.alpha;
			}
		}
	}
}
