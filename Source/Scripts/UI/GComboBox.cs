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

		protected GObject _titleObject;
		protected GObject _iconObject;
		protected GList _list;

		protected string[] _items;
		protected string[] _icons;
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
		/// Icon of the combobox.
		/// </summary>
		override public string icon
		{
			get
			{
				if (_iconObject != null)
					return _iconObject.icon;
				else
					return null;
			}

			set
			{
				if (_iconObject != null)
					_iconObject.icon = value;
				UpdateGear(7);
			}
		}

		/// <summary>
		/// Title of the combobox.
		/// </summary>
		public string title
		{
			get
			{
				if (_titleObject != null)
					return _titleObject.text;
				else
					return null;
			}
			set
			{
				if (_titleObject != null)
					_titleObject.text = value;
				UpdateGear(6);
			}
		}

		/// <summary>
		/// Same of the title.
		/// </summary>
		override public string text
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// Text color
		/// </summary>
		public Color titleColor
		{
			get
			{
				if (_titleObject is GTextField)
					return ((GTextField)_titleObject).color;
				else if (_titleObject is GLabel)
					return ((GLabel)_titleObject).titleColor;
				else if (_titleObject is GButton)
					return ((GButton)_titleObject).titleColor;
				else
					return Color.black;
			}
			set
			{
				if (_titleObject is GTextField)
					((GTextField)_titleObject).color = value;
				else if (_titleObject is GLabel)
					((GLabel)_titleObject).titleColor = value;
				else if (_titleObject is GButton)
					((GButton)_titleObject).titleColor = value;
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
					if (_icons != null && _selectedIndex < _icons.Length)
						this.icon = _icons[_selectedIndex];
				}
				else
				{
					this.text = string.Empty;
					if (_icons != null)
						this.icon = null;
					_selectedIndex = -1;
				}
				_itemsUpdated = true;
			}
		}

		public string[] icons
		{
			get { return _icons; }
			set
			{
				_icons = value;
				if (_icons != null && _selectedIndex != -1 && _selectedIndex < _icons.Length)
					this.icon = _icons[_selectedIndex];
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
				if (_selectedIndex >= 0 && _selectedIndex < _items.Length)
				{
					this.text = (string)_items[_selectedIndex];
					if (_icons != null && _selectedIndex < _icons.Length)
						this.icon = _icons[_selectedIndex];
				}
				else
				{
					this.text = string.Empty;
					if (_icons != null)
						this.icon = null;
				}
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

		protected void SetCurrentState()
		{
			if (this.grayed && _buttonController != null && _buttonController.HasPage(GButton.DISABLED))
				SetState(GButton.DISABLED);
			else
				SetState(_over ? GButton.OVER : GButton.UP);
		}

		override protected void HandleGrayedChanged()
		{
			if (_buttonController != null && _buttonController.HasPage(GButton.DISABLED))
			{
				if (this.grayed)
					SetState(GButton.DISABLED);
				else
					SetState(GButton.UP);
			}
			else
				base.HandleGrayedChanged();
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
			_titleObject = GetChild("title");
			_iconObject = GetChild("icon");

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

				dropdown.SetHome(this);
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
				str = ix.GetAttribute("icon");
				if (str != null)
				{
					if (_icons == null)
						_icons = new string[col.Count];
					_icons[i] = str;
				}
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

			str = xml.GetAttribute("icon");
			if (str != null && str.Length > 0)
				this.icon = str;
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
				item.icon = (_icons != null && i < _icons.Length) ? _icons[i] : null;
				item.name = i < _values.Length ? _values[i] : string.Empty;
			}
		}

		private void __popupWinClosed(object obj)
		{
			dropdown.displayObject.onRemovedFromStage.Remove(__popupWinClosed);
			SetCurrentState();
		}

		private void __clickItem(EventContext context)
		{
			if (dropdown.parent is GRoot)
				((GRoot)dropdown.parent).HidePopup(dropdown);
			_selectedIndex = int.MinValue;
			this.selectedIndex = _list.GetChildIndex((GObject)context.data);

			onChanged.Call();
		}

		private void __rollover()
		{
			_over = true;
			if (_down || dropdown != null && dropdown.parent != null)
				return;

			SetCurrentState();
		}

		private void __rollout()
		{
			_over = false;
			if (_down || dropdown != null && dropdown.parent != null)
				return;

			SetCurrentState();
		}

		private void __touchBegin(EventContext context)
		{
			if (context.initiator is InputTextField)
				return;

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
					SetCurrentState();
			}
		}
	}
}
