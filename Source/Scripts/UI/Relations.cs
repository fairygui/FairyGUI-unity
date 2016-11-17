using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Relations
	{
		GObject _owner;
		List<RelationItem> _items;

		public GObject handling;

		public Relations(GObject owner)
		{
			_owner = owner;
			_items = new List<RelationItem>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		public void Add(GObject target, RelationType relationType)
		{
			Add(target, relationType, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		/// <param name="usePercent"></param>
		public void Add(GObject target, RelationType relationType, bool usePercent)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Add(relationType, usePercent);
					return;
				}
			}
			RelationItem newItem = new RelationItem(_owner);
			newItem.target = target;
			newItem.Add(relationType, usePercent);
			_items.Add(newItem);
		}

		void AddItems(GObject target, string sidePairs)
		{
			RelationItem newItem = new RelationItem(_owner);
			newItem.target = target;
			
			int start = 0;
			int end = 0;
			int ms;
			int c1;
			int c2;
			bool usePercent;
			RelationType tid;
			//0 gc 解析版本
			while (end < sidePairs.Length)
			{
				start = end;
				end = sidePairs.IndexOf(",", start);
				if (end == -1)
					end = sidePairs.Length;
				usePercent = sidePairs[end - 1] == '%';
				ms = sidePairs.IndexOf('-', start, end-start);
				end++;
				if (ms != -1)
				{
					c1 = ((int)sidePairs[start]);
					c2 = ((int)sidePairs[ms + 1]);
				}
				else
				{
					c1 = ((int)sidePairs[start]);
					c2 = c1;
				}

				switch (c1)
				{
					case 119://width
						tid = RelationType.Width;
						break;

					case 104://height
						tid = RelationType.Height;
						break;

					case 109://middle
						tid = RelationType.Middle_Middle;
						break;

					case 99://center
						tid = RelationType.Center_Center;
						break;

					case 108: //left
						if (ms - start > 4) //leftext
						{
							if (c2 == 108)
								tid = RelationType.LeftExt_Left;
							else
								tid = RelationType.LeftExt_Right;
						}
						else
						{
							switch (c2)
							{
								case 108:
									tid = RelationType.Left_Left;
									break;

								case 114:
									tid = RelationType.Left_Right;
									break;

								case 99:
									tid = RelationType.Left_Center;
									break;

								default:
									throw new ArgumentException("invalid relation type: " + sidePairs);
							}
						}
						break;

					case 114: //right
						if (ms - start > 5) //rightext
						{
							if (c2 == 108)
								tid = RelationType.RightExt_Left;
							else
								tid = RelationType.RightExt_Right;
						}
						else
						{
							switch (c2)
							{
								case 108:
									tid = RelationType.Right_Left;
									break;

								case 114:
									tid = RelationType.Right_Right;
									break;

								case 99:
									tid = RelationType.Right_Center;
									break;

								default:
									throw new ArgumentException("invalid relation type: " + sidePairs);
							}
						}
						break;

					case 116://top
						if (ms - start > 3) //topext
						{
							if (c2 == 116)
								tid = RelationType.TopExt_Top;
							else
								tid = RelationType.TopExt_Bottom;
						}
						else
						{
							switch (c2)
							{
								case 116:
									tid = RelationType.Top_Top;
									break;

								case 98:
									tid = RelationType.Top_Bottom;
									break;

								case 109:
									tid = RelationType.Top_Middle;
									break;

								default:
									throw new ArgumentException("invalid relation type: " + sidePairs);
							}
						}
						break;

					case 98://bottom
						if (ms - start > 6) //bottomext
						{
							if (c2 == 116)
								tid = RelationType.BottomExt_Top;
							else
								tid = RelationType.BottomExt_Bottom;
						}
						else
						{
							switch (c2)
							{
								case 116:
									tid = RelationType.Bottom_Top;
									break;

								case 98:
									tid = RelationType.Bottom_Bottom;
									break;

								case 109:
									tid = RelationType.Bottom_Middle;
									break;

								default:
									throw new ArgumentException("invalid relation type: " + sidePairs);
							}
						}
						break;

					default:
						throw new ArgumentException("invalid relation type: " + sidePairs);
				}

				newItem.InternalAdd(tid, usePercent);
			}

			_items.Add(newItem);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		public void Remove(GObject target, RelationType relationType)
		{
			int cnt = _items.Count;
			int i = 0;
			while (i < cnt)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Remove(relationType);
					if (item.isEmpty)
					{
						item.Dispose();
						_items.RemoveAt(i);
						cnt--;
						continue;
					}
					else
						i++;
				}
				i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool Contains(GObject target)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				if (item.target == target)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		public void ClearFor(GObject target)
		{
			int cnt = _items.Count;
			int i = 0;
			while (i < cnt)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Dispose();
					_items.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearAll()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				item.Dispose();
			}
			_items.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public void CopyFrom(Relations source)
		{
			ClearAll();

			List<RelationItem> arr = source._items;
			foreach (RelationItem ri in arr)
			{
				RelationItem item = new RelationItem(_owner);
				item.CopyFrom(ri);
				_items.Add(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			ClearAll();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dWidth"></param>
		/// <param name="dHeight"></param>
		public void OnOwnerSizeChanged(float dWidth, float dHeight)
		{
			int cnt = _items.Count;
			if (cnt == 0)
				return;

			for (int i = 0; i < cnt; i++)
				_items[i].ApplyOnSelfSizeChanged(dWidth, dHeight);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isEmpty
		{
			get
			{
				return _items.Count == 0;
			}
		}

		public void Setup(XML xml)
		{
			XMLList.Enumerator et = xml.GetEnumerator("relation");

			string targetId;
			GObject target;
			while (et.MoveNext())
			{
				XML cxml = et.Current;
				targetId = cxml.GetAttribute("target");
				if (_owner.parent != null)
				{
					if (targetId != null && targetId != "")
						target = _owner.parent.GetChildById(targetId);
					else
						target = _owner.parent;
				}
				else
				{
					//call from component construction
					target = ((GComponent)_owner).GetChildById(targetId);
				}
				if (target != null)
					AddItems(target, cxml.GetAttribute("sidePair"));
			}
		}
	}
}
