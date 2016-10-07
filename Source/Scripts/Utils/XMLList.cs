using System;
using System.Collections.Generic;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class XMLList : IEnumerable<XML>
	{
		internal List<XML> _list;

		internal XMLList()
		{
			_list = new List<XML>();
		}

		internal XMLList(List<XML> list)
		{
			_list = list;
		}

		internal void Add(XML xml)
		{
			_list.Add(xml);
		}

		internal void Clear()
		{
			_list.Clear();
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
			return _list.GetEnumerator();
		}

		public Enumerator GetEnumerator(string selector)
		{
			return new Enumerator(_list, selector);
		}

		static List<XML> _tmpList = new List<XML>();
		public XMLList Filter(string selector)
		{
			bool allFit = true;
			_tmpList.Clear();
			int cnt = _list.Count;
			for (int i = 0; i < cnt; i++)
			{
				XML xml = _list[i];
				if (xml.name == selector)
					_tmpList.Add(xml);
				else
					allFit = false;
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

		public XML Find(string selector)
		{
			int cnt = _list.Count;
			for (int i = 0; i < cnt; i++)
			{
				XML xml = _list[i];
				if (xml.name == selector)
					return xml;
			}
			return null;
		}

		public struct Enumerator
		{
			List<XML> _source;
			string _selector;
			int _index;
			int _total;
			XML _current;

			public Enumerator(List<XML> source, string selector)
			{
				_source = source;
				_selector = selector;
				_index = -1;
				if (_source != null)
					_total = _source.Count;
				else
					_total = 0;
				_current = null;
			}

			public XML Current
			{
				get { return _current; }
			}

			public bool MoveNext()
			{
				while (++_index < _total)
				{
					_current = _source[_index];
					if (_current.name == _selector)
						return true;
				}

				return false;
			}

			public void Reset()
			{
				_index = -1;
			}
		}
	}
}
