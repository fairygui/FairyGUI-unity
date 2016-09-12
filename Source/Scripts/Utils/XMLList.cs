using System;
using System.Collections.Generic;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class XMLList : IEnumerable<XML>
	{
		List<XML> _list;

		internal XMLList()
		{
			_list = new List<XML>();
		}

		internal XMLList(List<XML> list)
		{
			_list = list;
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public XML this[int index]
		{
			get { return _list[index]; }
		}

		public IEnumerator<XML> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		internal void Add(XML xml)
		{
			_list.Add(xml);
		}

		internal void Clear()
		{
			_list.Clear();
		}

		static List<XML> _tmpList = new List<XML>();
		internal XMLList Filter(string selector)
		{
			bool allFit = true;
			_tmpList.Clear();
			foreach (XML xml in _list)
			{
				if (xml.name != selector)
					allFit = false;
				else
					_tmpList.Add(xml);
			}

			if (allFit)
				return this;
			else
			{
				XMLList ret = new XMLList(_tmpList);
				_tmpList = new List<XML>();
				return ret;
			}
		}

		internal XML Find(string selector)
		{
			foreach (XML xml in _list)
			{
				if (xml.name == selector)
					return xml;
			}
			return null;
		}
	}
}
