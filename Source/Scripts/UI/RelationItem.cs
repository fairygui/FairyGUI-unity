using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	class RelationDef
	{
		public bool percent;
		public RelationType type;

		public void copyFrom(RelationDef source)
		{
			this.percent = source.percent;
			this.type = source.type;
		}
	}

	class RelationItem
	{
		GObject _owner;
		GObject _target;
		List<RelationDef> _defs;
		Vector4 _targetData;

		public RelationItem(GObject owner)
		{
			_owner = owner;
			_defs = new List<RelationDef>();
		}

		public GObject target
		{
			get { return _target; }
			set
			{
				if (_target != value)
				{
					if (_target != null)
						ReleaseRefTarget(_target);
					_target = value;
					if (_target != null)
						AddRefTarget(_target);
				}
			}
		}

		public void Add(RelationType relationType, bool usePercent)
		{
			if (relationType == RelationType.Size)
			{
				Add(RelationType.Width, usePercent);
				Add(RelationType.Height, usePercent);
				return;
			}

			int dc = _defs.Count;
			for (int k = 0; k < dc; k++)
			{
				if (_defs[k].type == relationType)
					return;
			}

			InternalAdd(relationType, usePercent);
		}

		public void InternalAdd(RelationType relationType, bool usePercent)
		{
			if (relationType == RelationType.Size)
			{
				InternalAdd(RelationType.Width, usePercent);
				InternalAdd(RelationType.Height, usePercent);
				return;
			}

			RelationDef info = new RelationDef();
			info.percent = usePercent;
			info.type = relationType;
			_defs.Add(info);

			//当使用中线关联时，因为需要除以2，很容易因为奇数宽度/高度造成小数点坐标；当使用百分比时，也会造成小数坐标；
			//所以设置了这类关联的对象，自动启用pixelSnapping
			if (usePercent || relationType == RelationType.Left_Center || relationType == RelationType.Center_Center || relationType == RelationType.Right_Center
					|| relationType == RelationType.Top_Middle || relationType == RelationType.Middle_Middle || relationType == RelationType.Bottom_Middle)
				_owner.pixelSnapping = true;
		}

		public void Remove(RelationType relationType)
		{
			if (relationType == RelationType.Size)
			{
				Remove(RelationType.Width);
				Remove(RelationType.Height);
				return;
			}

			int dc = _defs.Count;
			for (int k = 0; k < dc; k++)
			{
				if (_defs[k].type == relationType)
				{
					_defs.RemoveAt(k);
					break;
				}
			}
		}

		public void CopyFrom(RelationItem source)
		{
			this.target = source.target;

			_defs.Clear();
			foreach (RelationDef info in source._defs)
			{
				RelationDef info2 = new RelationDef();
				info2.copyFrom(info);
				_defs.Add(info2);
			}
		}

		public void Dispose()
		{
			if (_target != null)
			{
				ReleaseRefTarget(_target);
				_target = null;
			}
		}

		public bool isEmpty
		{
			get { return _defs.Count == 0; }
		}

		public void ApplyOnSelfSizeChanged(float dWidth, float dHeight)
		{
			int cnt = _defs.Count;
			if (cnt == 0)
				return;

			float ox = _owner.x;
			float oy = _owner.y;

			for (int i = 0; i < cnt; i++)
			{
				RelationDef info = _defs[i];
				switch (info.type)
				{
					case RelationType.Center_Center:
					case RelationType.Right_Center:
						_owner.x -= dWidth / 2;
						break;

					case RelationType.Right_Left:
					case RelationType.Right_Right:
						_owner.x -= dWidth;
						break;

					case RelationType.Middle_Middle:
					case RelationType.Bottom_Middle:
						_owner.y -= dHeight / 2;
						break;
					case RelationType.Bottom_Top:
					case RelationType.Bottom_Bottom:
						_owner.y -= dHeight;
						break;
				}
			}

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}
		}

		void ApplyOnXYChanged(RelationDef info, float dx, float dy)
		{
			float tmp;
			switch (info.type)
			{
				case RelationType.Left_Left:
				case RelationType.Left_Center:
				case RelationType.Left_Right:
				case RelationType.Center_Center:
				case RelationType.Right_Left:
				case RelationType.Right_Center:
				case RelationType.Right_Right:
					_owner.x += dx;
					break;

				case RelationType.Top_Top:
				case RelationType.Top_Middle:
				case RelationType.Top_Bottom:
				case RelationType.Middle_Middle:
				case RelationType.Bottom_Top:
				case RelationType.Bottom_Middle:
				case RelationType.Bottom_Bottom:
					_owner.y += dy;
					break;

				case RelationType.Width:
				case RelationType.Height:
					break;

				case RelationType.LeftExt_Left:
				case RelationType.LeftExt_Right:
					tmp = _owner.x;
					_owner.x += dx;
					_owner.width = _owner._rawWidth - (_owner.x - tmp);
					break;

				case RelationType.RightExt_Left:
				case RelationType.RightExt_Right:
					_owner.width = _owner._rawWidth + dx;
					break;

				case RelationType.TopExt_Top:
				case RelationType.TopExt_Bottom:
					tmp = _owner.y;
					_owner.y += dy;
					_owner.height = _owner._rawHeight - (_owner.y - tmp);
					break;

				case RelationType.BottomExt_Top:
				case RelationType.BottomExt_Bottom:
					_owner.height = _owner._rawHeight + dy;
					break;
			}
		}

		void ApplyOnSizeChanged(RelationDef info)
		{
			float targetX, targetY;
			if (_target != _owner.parent)
			{
				targetX = _target.x;
				targetY = _target.y;
			}
			else
			{
				targetX = 0;
				targetY = 0;
			}
			float v, tmp;

			switch (info.type)
			{
				case RelationType.Left_Left:
					if (info.percent && _target == _owner.parent)
					{
						v = _owner.x - targetX;
						if (info.percent)
							v = v / _targetData.z * _target._rawWidth;
						_owner.x = targetX + v;
					}
					break;
				case RelationType.Left_Center:
					v = _owner.x - (targetX + _targetData.z / 2);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + _target._rawWidth / 2 + v;
					break;
				case RelationType.Left_Right:
					v = _owner.x - (targetX + _targetData.z);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + _target._rawWidth + v;
					break;
				case RelationType.Center_Center:
					v = _owner.x + _owner._rawWidth / 2 - (targetX + _targetData.z / 2);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + _target._rawWidth / 2 + v - _owner._rawWidth / 2;
					break;
				case RelationType.Right_Left:
					v = _owner.x + _owner._rawWidth - targetX;
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + v - _owner._rawWidth;
					break;
				case RelationType.Right_Center:
					v = _owner.x + _owner._rawWidth - (targetX + _targetData.z / 2);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + _target._rawWidth / 2 + v - _owner._rawWidth;
					break;
				case RelationType.Right_Right:
					v = _owner.x + _owner._rawWidth - (targetX + _targetData.z);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					_owner.x = targetX + _target._rawWidth + v - _owner._rawWidth;
					break;

				case RelationType.Top_Top:
					if (info.percent && _target == _owner.parent)
					{
						v = _owner.y - targetY;
						if (info.percent)
							v = v / _targetData.w * _target._rawHeight;
						_owner.y = targetY + v;
					}
					break;
				case RelationType.Top_Middle:
					v = _owner.y - (targetY + _targetData.w / 2);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + _target._rawHeight / 2 + v;
					break;
				case RelationType.Top_Bottom:
					v = _owner.y - (targetY + _targetData.w);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + _target._rawHeight + v;
					break;
				case RelationType.Middle_Middle:
					v = _owner.y + _owner._rawHeight / 2 - (targetY + _targetData.w / 2);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + _target._rawHeight / 2 + v - _owner._rawHeight / 2;
					break;
				case RelationType.Bottom_Top:
					v = _owner.y + _owner._rawHeight - targetY;
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + v - _owner._rawHeight;
					break;
				case RelationType.Bottom_Middle:
					v = _owner.y + _owner._rawHeight - (targetY + _targetData.w / 2);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + _target._rawHeight / 2 + v - _owner._rawHeight;
					break;
				case RelationType.Bottom_Bottom:
					v = _owner.y + _owner._rawHeight - (targetY + _targetData.w);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					_owner.y = targetY + _target._rawHeight + v - _owner._rawHeight;
					break;

				case RelationType.Width:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceWidth - _target.initWidth;
					else
						v = _owner._rawWidth - _targetData.z;
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					if (_target == _owner.parent)
						_owner.SetSize(_target._rawWidth + v, _owner._rawHeight, true);
					else
						_owner.width = _target._rawWidth + v;
					break;
				case RelationType.Height:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceHeight - _target.initHeight;
					else
						v = _owner._rawHeight - _targetData.w;
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					if (_target == _owner.parent)
						_owner.SetSize(_owner._rawWidth, _target._rawHeight + v, true);
					else
						_owner.height = _target._rawHeight + v;
					break;

				case RelationType.LeftExt_Left:
					break;
				case RelationType.LeftExt_Right:
					v = _owner.x - (targetX + _targetData.z);
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					tmp = _owner.x;
					_owner.x = targetX + _target._rawWidth + v;
					_owner.width = _owner._rawWidth - (_owner.x - tmp);
					break;
				case RelationType.RightExt_Left:
					break;
				case RelationType.RightExt_Right:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceWidth - (targetX + _target.initWidth);
					else
						v = _owner._rawWidth - (targetX + _targetData.z);
					if (_owner != _target.parent)
						v += _owner.x;
					if (info.percent)
						v = v / _targetData.z * _target._rawWidth;
					if (_owner != _target.parent)
						_owner.width = targetX + _target._rawWidth + v - _owner.x;
					else
						_owner.width = targetX + _target._rawWidth + v;
					break;
				case RelationType.TopExt_Top:
					break;
				case RelationType.TopExt_Bottom:
					v = _owner.y - (targetY + _targetData.w);
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					tmp = _owner.y;
					_owner.y = targetY + _target._rawHeight + v;
					_owner.height = _owner._rawHeight - (_owner.y - tmp);
					break;
				case RelationType.BottomExt_Top:
					break;
				case RelationType.BottomExt_Bottom:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceHeight - (targetY + _target.initHeight);
					else
						v = _owner._rawHeight - (targetY + _targetData.w);
					if (_owner != _target.parent)
						v += _owner.y;
					if (info.percent)
						v = v / _targetData.w * _target._rawHeight;
					if (_owner != _target.parent)
						_owner.height = targetY + _target._rawHeight + v - _owner.y;
					else
						_owner.height = targetY + _target._rawHeight + v;
					break;
			}
		}

		void AddRefTarget(GObject target)
		{
			if (target != _owner.parent)
				target.onPositionChanged.Add(__targetXYChanged);
			target.onSizeChanged.Add(__targetSizeChanged);
			_targetData.x = _target.x;
			_targetData.y = _target.y;
			_targetData.z = _target._rawWidth;
			_targetData.w = _target._rawHeight;
		}

		void ReleaseRefTarget(GObject target)
		{
			target.onPositionChanged.Remove(__targetXYChanged);
			target.onSizeChanged.Remove(__targetSizeChanged);
		}

		void __targetXYChanged(EventContext context)
		{
			if (_owner.relations.handling != null
				|| _owner.group != null && _owner.group._updating)
			{
				_targetData.x = _target.x;
				_targetData.y = _target.y;
				return;
			}

			_owner.relations.handling = (GObject)context.sender;

			float ox = _owner.x;
			float oy = _owner.y;
			float dx = _target.x - _targetData.x;
			float dy = _target.y - _targetData.y;

			int cnt = _defs.Count;
			for (int i = 0; i < cnt; i++)
				ApplyOnXYChanged(_defs[i], dx, dy);

			_targetData.x = _target.x;
			_targetData.y = _target.y;

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}

			_owner.relations.handling = null;
		}

		void __targetSizeChanged(EventContext context)
		{
			if (_owner.relations.handling != null)
			{
				_targetData.z = _target._rawWidth;
				_targetData.w = _target._rawHeight;
				return;
			}

			_owner.relations.handling = (GObject)context.sender;

			float ox = _owner.x;
			float oy = _owner.y;
			float ow = _owner._rawWidth;
			float oh = _owner._rawHeight;

			int cnt = _defs.Count;
			for (int i = 0; i < cnt; i++)
				ApplyOnSizeChanged(_defs[i]);

			_targetData.z = _target._rawWidth;
			_targetData.w = _target._rawHeight;

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}

			if (!Mathf.Approximately(ow, _owner._rawWidth) || !Mathf.Approximately(oh, _owner._rawHeight))
			{
				ow = _owner._rawWidth - ow;
				oh = _owner._rawHeight - oh;

				_owner.UpdateGearFromRelations(2, ow, oh);
			}

			_owner.relations.handling = null;
		}
	}
}
