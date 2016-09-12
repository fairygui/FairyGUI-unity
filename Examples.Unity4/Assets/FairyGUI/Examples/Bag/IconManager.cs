using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;

public delegate void LoadCompleteCallback(NTexture texture);
public delegate void LoadErrorCallback(string error);

 /// <summary>
 /// Use to load icons from asset bundle, and pool them
 /// </summary>
public class IconManager : MonoBehaviour
{
	static IconManager _instance;
	public static IconManager inst
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("IconManager");
				DontDestroyOnLoad(go);
				_instance = go.AddComponent<IconManager>();
			}
			return _instance;
		}
	}

	public const int POOL_CHECK_TIME = 30;
	public const int MAX_POOL_SIZE = 10;

	List<LoadItem> _items;
	bool _started;
	Hashtable _pool;
	string _basePath;

	void Awake()
	{
		_items = new List<LoadItem>();
		_pool = new Hashtable();
		_basePath = Application.streamingAssetsPath.Replace("\\", "/") + "/FairyGUI-Examples/";
		if (Application.platform != RuntimePlatform.Android)
			_basePath = "file:///" + _basePath;

		StartCoroutine(FreeIdleIcons());
	}

	public void LoadIcon(string url,
					LoadCompleteCallback onSuccess,
					LoadErrorCallback onFail)
	{
		LoadItem item = new LoadItem();
		item.url = url;
		item.onSuccess = onSuccess;
		item.onFail = onFail;
		_items.Add(item);
		if(!_started)
			StartCoroutine(Run());
	}

	IEnumerator Run()
	{
		_started = true;

		LoadItem item = null;
		while (true)
		{
			if (_items.Count > 0)
			{
				item = _items[0];
				_items.RemoveAt(0);
			}
			else
				break;

			if (_pool.ContainsKey(item.url))
			{
				//Debug.Log("hit " + item.url);

				NTexture texture = (NTexture)_pool[item.url];
				texture.refCount++;

				if (item.onSuccess != null)
					item.onSuccess(texture);

				continue;
			}

			WWW www = new WWW(_basePath + item.url + ".ab");
			yield return www;

			if (string.IsNullOrEmpty(www.error))
			{
				AssetBundle bundle = www.assetBundle;
				if (bundle == null)
				{
					Debug.LogWarning("Run Window->Build FairyGUI example Bundles first.");
					if (item.onFail != null)
						item.onFail(www.error);
					continue;
				}
#if UNITY_5
				NTexture texture = new NTexture(bundle.LoadAllAssets<Texture2D>()[0]);
#else
				NTexture texture = new NTexture((Texture2D)bundle.mainAsset);
#endif
				texture.refCount++;
				bundle.Unload(false);

				_pool[item.url] = texture;

				if (item.onSuccess != null)
					item.onSuccess(texture);
			}
			else
			{
				if (item.onFail != null)
					item.onFail(www.error);
			}
		}

		_started = false;
	}

	IEnumerator FreeIdleIcons()
	{
		yield return new WaitForSeconds(POOL_CHECK_TIME); //check the pool every 30 seconds

		int cnt = _pool.Count;
		if (cnt > MAX_POOL_SIZE)
		{
			ArrayList toRemove = null;
			foreach (DictionaryEntry de in _pool)
			{
				string key = (string)de.Key;
				NTexture texture = (NTexture)de.Value;
				if (texture.refCount == 0)
				{
					if (toRemove == null)
						toRemove = new ArrayList();
					toRemove.Add(key);
					texture.Dispose();

					//Debug.Log("free icon " + de.Key);

					cnt--;
					if (cnt <= 8)
						break;
				}
			}
			if (toRemove != null)
			{
				foreach (string key in toRemove)
					_pool.Remove(key);
			}
		}
	}

}

class LoadItem
{
	public string url;
	public LoadCompleteCallback onSuccess;
	public LoadErrorCallback onFail;
}
