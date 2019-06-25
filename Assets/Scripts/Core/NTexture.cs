using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum DestroyMethod
	{
		Destroy,
		Unload,
		None
	}

	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		/// <summary>
		/// 
		/// </summary>
		public Rect uvRect;

		/// <summary>
		/// 
		/// </summary>
		public bool rotated;

		/// <summary>
		/// 
		/// </summary>
		public int refCount;

		/// <summary>
		/// 
		/// </summary>
		public float lastActive;

		/// <summary>
		/// 
		/// </summary>
		public DestroyMethod destroyMethod;

		Texture _nativeTexture;
		Texture _alphaTexture;

		Rect _region;
		Vector2 _offset;
		Vector2 _originalSize;

		NTexture _root;
		Dictionary<string, MaterialManager> _materialManagers;

		internal static Texture2D CreateEmptyTexture()
		{
			Texture2D emptyTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
			emptyTexture.name = "White Texture";
			emptyTexture.hideFlags = DisplayOptions.hideFlags;
			emptyTexture.SetPixel(0, 0, Color.white);
			emptyTexture.Apply();
			return emptyTexture;
		}

		static NTexture _empty;

		/// <summary>
		/// 
		/// </summary>
		public static NTexture Empty
		{
			get
			{
				if (_empty == null)
					_empty = new NTexture(CreateEmptyTexture());

				return _empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void DisposeEmpty()
		{
			if (_empty != null)
			{
				NTexture tmp = _empty;
				_empty = null;
				tmp.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public NTexture(Texture texture) : this(texture, null, 1, 1)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture texture, Texture alphaTexture, float xScale, float yScale)
		{
			_root = this;
			_nativeTexture = texture;
			_alphaTexture = alphaTexture;
			uvRect = new Rect(0, 0, xScale, yScale);
			_originalSize = new Vector2(texture.width, texture.height);
			_region = new Rect(0, 0, _originalSize.x, _originalSize.y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture texture, Rect region)
		{
			_root = this;
			_nativeTexture = texture;
			_region = region;
			_originalSize = new Vector2(_region.width, _region.height);
			uvRect = new Rect(region.x / _nativeTexture.width, 1 - region.yMax / _nativeTexture.height,
				region.width / _nativeTexture.width, region.height / _nativeTexture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		/// <param name="rotated"></param>
		public NTexture(NTexture root, Rect region, bool rotated)
		{
			_root = root;
			this.rotated = rotated;
			region.x += root._region.x;
			region.y += root._region.y;
			uvRect = new Rect(region.x * root.uvRect.width / root.width, 1 - region.yMax * root.uvRect.height / root.height,
				region.width * root.uvRect.width / root.width, region.height * root.uvRect.height / root.height);
			if (rotated)
			{
				float tmp = region.width;
				region.width = region.height;
				region.height = tmp;

				tmp = uvRect.width;
				uvRect.width = uvRect.height;
				uvRect.height = tmp;
			}
			_region = region;
			_originalSize = _region.size;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		/// <param name="rotated"></param>
		/// <param name="originalSize"></param>
		/// <param name="offset"></param>
		public NTexture(NTexture root, Rect region, bool rotated, Vector2 originalSize, Vector2 offset)
			: this(root, region, rotated)
		{
			_originalSize = originalSize;
			_offset = offset;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sprite"></param>
		public NTexture(Sprite sprite)
		{
			Rect rect = sprite.rect;
			rect.y = sprite.texture.height - rect.yMax;

			_root = this;
			_nativeTexture = sprite.texture;
			_region = rect;
			_originalSize = new Vector2(_region.width, _region.height);
			uvRect = new Rect(_region.x / _nativeTexture.width, 1 - _region.yMax / _nativeTexture.height,
				_region.width / _nativeTexture.width, _region.height / _nativeTexture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		public int width
		{
			get { return (int)_region.width; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get { return (int)_region.height; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 offset
		{
			get { return _offset; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 originalSize
		{
			get { return _originalSize; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawRect"></param>
		/// <returns></returns>
		public Rect GetDrawRect(Rect drawRect)
		{
			if (_originalSize.x == _region.width && _originalSize.y == _region.height)
				return drawRect;

			float sx = drawRect.width / _originalSize.x;
			float sy = drawRect.height / _originalSize.y;
			return new Rect(_offset.x * sx, _offset.y * sy, _region.width * sx, _region.height * sy);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		public void GetUV(Vector2[] uv)
		{
			uv[0] = uvRect.position;
			uv[1] = new Vector2(uvRect.xMin, uvRect.yMax);
			uv[2] = new Vector2(uvRect.xMax, uvRect.yMax);
			uv[3] = new Vector2(uvRect.xMax, uvRect.yMin);
			if (rotated)
			{
				float xMin = uvRect.xMin;
				float yMin = uvRect.yMin;
				float yMax = uvRect.yMax;

				float tmp;
				for (int i = 0; i < 4; i++)
				{
					Vector2 m = uv[i];
					tmp = m.y;
					m.y = yMin + m.x - xMin;
					m.x = xMin + yMax - tmp;
					uv[i] = m;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture root
		{
			get { return _root; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool disposed
		{
			get { return _root == null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture nativeTexture
		{
			get { return _root != null ? _root._nativeTexture : null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture alphaTexture
		{
			get { return _root != null ? _root._alphaTexture : null; }
		}

		static uint _gCounter;

		/// <summary>
		/// 
		/// </summary>
		public MaterialManager GetMaterialManager(string shaderName, string[] keywords)
		{
			if (_root != this)
			{
				if (_root == null)
					return null;
				else
					return _root.GetMaterialManager(shaderName, keywords);
			}

			if (_materialManagers == null)
				_materialManagers = new Dictionary<string, MaterialManager>();

			string key = shaderName;
			if (keywords != null)
			{
				//对于带指定关键字的，目前的设计是不参加共享材质了，因为逻辑会变得更复杂
				key = shaderName + "_" + _gCounter++;
			}

			MaterialManager mm;
			if (!_materialManagers.TryGetValue(key, out mm))
			{
				mm = new MaterialManager(this, ShaderConfig.GetShader(shaderName), keywords);
				mm._managerKey = key;
				_materialManagers.Add(key, mm);
			}

			return mm;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="manager"></param>
		public void DestroyMaterialManager(MaterialManager manager)
		{
			_materialManagers.Remove(manager._managerKey);
			manager.DestroyMaterials();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Unload()
		{
			Unload(false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Unload(bool destroyMaterials)
		{
			if (this == _empty)
				return;

			if (_root != this)
				throw new System.Exception("Unload is not allow to call on none root NTexture.");

			if (_nativeTexture != null)
			{
				if (destroyMethod == DestroyMethod.Destroy)
				{
					Object.DestroyImmediate(_nativeTexture, true);
					if (_alphaTexture != null)
						Object.DestroyImmediate(_alphaTexture, true);
				}
				else if (destroyMethod == DestroyMethod.Unload)
				{
					Resources.UnloadAsset(_nativeTexture);
					if (_alphaTexture != null)
						Resources.UnloadAsset(_alphaTexture);
				}

				_nativeTexture = null;
				_alphaTexture = null;

				if (destroyMaterials)
					DestroyMaterials();
				else
					RefreshMaterials();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nativeTexture"></param>
		/// <param name="alphaTexture"></param>
		public void Reload(Texture nativeTexture, Texture alphaTexture)
		{
			if (_root != this)
				throw new System.Exception("Reload is not allow to call on none root NTexture.");

			_nativeTexture = nativeTexture;
			_alphaTexture = alphaTexture;

			RefreshMaterials();
		}

		void RefreshMaterials()
		{
			if (_materialManagers != null && _materialManagers.Count > 0)
			{
				Dictionary<string, MaterialManager>.Enumerator iter = _materialManagers.GetEnumerator();
				while (iter.MoveNext())
					iter.Current.Value.RefreshMaterials();
				iter.Dispose();
			}
		}

		void DestroyMaterials()
		{
			if (_materialManagers != null && _materialManagers.Count > 0)
			{
				Dictionary<string, MaterialManager>.Enumerator iter = _materialManagers.GetEnumerator();
				while (iter.MoveNext())
					iter.Current.Value.DestroyMaterials();
				iter.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (this == _empty)
				return;

			if (_root == this)
				Unload(true);
			_root = null;
		}
	}
}
