using System;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GComboBox class.
	/// </summary>
	public class GComboBox : GComponent
	{
		/// <summary>
		/// Visible item count of the drop down list.
		/// </summary>
		public int visibleItemCount;

		/// <summary>
		/// Dispatched when selection was changed.
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public GComponent dropdown;

		protected GTextField _titleObject;
		protected GList _list;

		protected string[] _items;
		protected string[] _values;
		protected string _popupDirection;

		bool _itemsUpdated;
		int _selectedIndex;
		Controller _buttonController;

		bool _down;
		bool _over;

		public GComboBox()
		{
			visibleItemCount = UIConfig.defaultComboBoxVisibleItemCount;
			_itemsUpdated = true;
			_selectedIndex = -1;
			_items = new string[0];
			_values = new string[0];
			_popupDirection = "down";

			onChanged = new EventListener(this, "onChanged");
		}

		/// <summary>
		/// Text display in combobox.
		/// </summary>
		override public string text
		{
			get
			{
				if (_titleObject != null)
					return _titleObject.text;
				else
					return string.Empty;
			}
			set
			{
				if (_titleObject != null)
					_titleObject.text = value;
			}
		}

		/// <summary>
		/// Text color
		/// </summary>
		public Color titleColor
		{
			get
			{
				if (_titleObject != null)
					return _titleObject.color;
				else
					return Color.black;
			}
			set
			{
				if (_titleObject != null)
					_titleObject.color = value;
			}
		}

		/// <summary>
		/// Items to build up drop down list.
		/// </summary>
		public string[] items
		{
			get
			{
				return _items;
			}
			set
			{
				if (value == null)
					_items = new string[0];
				else
					_items = (string[])value.Clone();
				if (_items.Length > 0)
				{
					if (_selectedIndex >= _items.Length)
						_selectedIndex = _items.Length - 1;
					else if (_selectedIndex == -1)
						_selectedIndex = 0;
					this.text = _items[_selectedIndex];
				}
				else
					this.text = string.Empty;
				_itemsUpdated = true;
			}
		}

		/// <summary>
		/// Values, should be same size of the items. 
		/// </summary>
		public string[] values
		{
			get
			{
				return _values;
			}
			set
			{
				if (value == null)
					_values = new string[0];
				else
					_values = (string[])value.Clone();
			}
		}

		/// <summary>
		/// Selected index.
		/// </summary>
		public int selectedIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				if (_selectedIndex == value)
					return;

				_selectedIndex = value;
				if (selectedIndex >= 0 && selectedIndex < _items.Length)
					this.text = (string)_items[_selectedIndex];
				else
					this.text = string.Empty;
			}
		}

		/// <summary>
		/// Selected value.
		/// </summary>
		public string value
		{
			get
			{
				if (_selectedIndex >= 0 && _selectedIndex < _values.Length)
					return _values[_selectedIndex];
				else
					return null;
			}
			set
			{
				this.selectedIndex = Array.IndexOf(_values, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string popupDirection
		{
			get { return _popupDirection; }
			set { _popupDirection = value; }
		}

		protected void SetState(string value)
		{
			if (_buttonController != null)
				_buttonController.selectedPage = value;
		}

		public override void Dispose()
		{
			if (dropdown != null)
			{
				dropdown.Dispose();
				dropdown = null;
			}
			base.Dispose();
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

			XML xml = cxml.GetNode("ComboBox");

			string str;

			_buttonController = GetController("button");
			_titleObject = GetChild("title") as GTextField;

			str = xml.GetAttribute("dropdown");
			if (str != null && str.Length > 0)
			{
				dropdown = UIPackage.CreateObjectFromURL(str) as GComponent;
				if (dropdown == null)
				{
					Debug.LogWarning("FairyGUI: " + this.resourceURL + " should be a component.");
					return;
				}

				_list = dropdown.GetChild("list") as GList;
				if (_list == null)
				{
					Debug.LogWarning("FairyGUI: " + this.resourceURL + ": should container a list component named list.");
					return;
				}
				_list.onClickItem.Add(__clickItem);

				_list.AddRelation(dropdown, RelationType.Width);
				_list.RemoveRelation(dropdown, RelationType.Height);

				dropdown.AddRelation(_list, RelationType.Height);
				dropdown.RemoveRelation(_list, RelationType.Width);
			}

			displayObject.onRollOver.Add(__rollover);
			displayObject.onRollOut.Add(__rollout);
			displayObject.onTouchBegin.Add(__touchBegin);
			displayObject.onTouchEnd.Add(__touchEnd);
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

			XML xml = cxml.GetNode("ComboBox");
			if (xml == null)
				return;

			string str;
			str = xml.GetAttribute("titleColor");
			if (str != null)
				this.titleColor = ToolSet.ConvertFromHtmlColor(str);
			visibleItemCount = xml.GetAttributeInt("visibleItemCount", visibleItemCount);
			_popupDirection = xml.GetAttribute("direction", _popupDirection);

			XMLList col = xml.Elements("item");
			_items = new string[col.Count];
			_values = new string[col.Count];
			int i = 0;
			foreach (XML ix in col)
			{
				_items[i] = ix.GetAttribute("title");
				_values[i] = ix.GetAttribute("value");
				i++;
			}

			str = xml.GetAttribute("title");
			if (str != null && str.Length > 0)
			{
				this.text = str;
				_selectedIndex = Array.IndexOf(_items, str);
			}
			else if (_items.Length > 0)
			{
				_selectedIndex = 0;
				this.text = _items[0];
			}
			else
				_selectedIndex = -1;
		}

		public void UpdateDropdownList()
		{
			if (_itemsUpdated)
			{
				_itemsUpdated = false;
				RenderDropdownList();
				_list.ResizeToFit(visibleItemCount);
			}
		}

		protected void ShowDropdown()
		{
			UpdateDropdownList();
			if (_list.selectionMode == ListSelectionMode.Single)
				_list.selectedIndex = -1;
			dropdown.width = this.width;

			this.root.TogglePopup(dropdown, this, _popupDirection == "up" ? (object)false : (_popupDirection == "auto" ? null : (object)true));
			if (dropdown.parent != null)
			{
				dropdown.displayObject.onRemovedFromStage.Add(__popupWinClosed);
				SetState(GButton.DOWN);
			}
		}

		virtual protected void RenderDropdownList()
		{
			_list.RemoveChildrenToPool();
			int cnt = _items.Length;
			for (int i = 0; i < cnt; i++)
			{
				GObject item = _list.AddItemFromPool();
				item.text = _items[i];
				item.name = i < _values.Length ? _values[i] : string.Empty;
			}
		}

		private void __popupWinClosed(object obj)
		{
			dropdown.displayObject.onRemovedFromStage.Remove(__popupWinClosed);
			if (_over)
				SetState(GButton.OVER);
			else
				SetState(GButton.UP);
		}

		private void __clickItem(EventContext context)
		{
			if (dropdown.parent is GRoot)
				((GRoot)dropdown.parent).HidePopup(dropdown);
			_selectedIndex = _list.GetChildIndex((GObject)context.data);
			if (_selectedIndex >= 0)
				this.text = _items[_selectedIndex];
			else
				this.text = string.Empty;

			onChanged.Call();
		}

		private void __rollover()
		{
			_over = true;
			if (_down || dropdown != null && dropdown.parent != null)
				return;

			SetState(GButton.OVER);
		}

		private void __rollout()
		{
			_over = false;
			if (_down || dropdown != null && dropdown.parent != null)
				return;

			SetState(GButton.UP);
		}

		private void __touchBegin(EventContext context)
		{
			_down = true;

			if (dropdown != null)
				ShowDropdown();

			context.CaptureTouch();
		}

		private void __touchEnd(EventContext context)
		{
			if (_down)
			{
				if (this.displayObject == null || this.displayObject.isDisposed)
					return;

				_down = false;
				if (dropdown != null && dropdown.parent != null)
				{
					if (_over)
						SetState(GButton.OVER);
					else
						SetState(GButton.UP);
				}
			}
		}
	}
}
