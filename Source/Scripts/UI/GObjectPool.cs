using System;
using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// GObjectPool is use for GObject pooling.
	/// </summary>
	public class GObjectPool
	{
		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		/// <param name="obj"></param>
		public delegate void InitCallbackDelegate(GObject obj);

		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		public InitCallbackDelegate initCallback;

		Dictionary<string, Queue<GObject>> _pool;

		public GObjectPool()
		{
			_pool = new Dictionary<string, Queue<GObject>>();
		}

		/// <summary>
		/// Dispose all objects in the pool.
		/// </summary>
		public void Clear()
		{
			foreach (KeyValuePair<string, Queue<GObject>> kv in _pool)
			{
				Queue<GObject> list = kv.Value;
				foreach (GObject obj in list)
					obj.Dispose();
			}
			_pool.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		public int count
		{
			get { return _pool.Count; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public GObject GetObject(string url)
		{
			Queue<GObject> arr;
			if (!_pool.TryGetValue(url, out arr))
			{
				arr = new Queue<GObject>();
				_pool.Add(url, arr);
			}

			if (arr.Count > 0)
			{
				return arr.Dequeue();
			}

			GObject obj = UIPackage.CreateObjectFromURL(url);
			if (obj != null)
			{
				if (initCallback != null)
					initCallback(obj);
			}

			return obj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void ReturnObject(GObject obj)
		{
			string url = obj.resourceURL;
			Queue<GObject> arr;
			if (!_pool.TryGetValue(url, out arr))
			{
				arr = new Queue<GObject>();
				_pool.Add(url, arr);
			}
			
			arr.Enqueue(obj);
		}
	}
}
