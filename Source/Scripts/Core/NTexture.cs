using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		/// <summary>
		/// 
		/// </summary>
		public Texture nativeTexture { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public NTexture alphaTexture { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public NTexture root { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Rect uvRect { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, MaterialManager> materialManagers { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int refCount;

		/// <summary>
		/// 
		/// </summary>
		public bool disposed;

		/// <summary>
		/// 
		/// </summary>
		public float lastActive;

		/// <summary>
		/// 
		/// </summary>
		public bool storedODisk;

		Rect? _region;

		static Texture2D CreateEmptyTexture()
		{
			Texture2D emptyTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
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
				_empty.Dispose(true);
				_empty = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public NTexture(Texture texture)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, 1, 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture texture, float xScale, float yScale)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, xScale, yScale);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture texture, Rect region)
		{
			root = this;
			nativeTexture = texture;
			_region = region;
			uvRect = new Rect(region.x / nativeTexture.width, 1 - region.yMax / nativeTexture.height,
				region.width / nativeTexture.width, region.height / nativeTexture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		public NTexture(NTexture root, Rect region)
		{
			this.root = root;
			nativeTexture = root.nativeTexture;

			if (root._region != null)
			{
				region.x += ((Rect)root._region).x;
				region.y += ((Rect)root._region).y;
			}
			_region = region;

			uvRect = new Rect(region.x * root.uvRect.width / nativeTexture.width, 1 - region.yMax * root.uvRect.height / nativeTexture.height,
				region.width * root.uvRect.width / nativeTexture.width, region.height * root.uvRect.height / nativeTexture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		public int width
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).width;
				else
					return nativeTexture.width;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).height;
				else
					return nativeTexture.height;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void DestroyMaterials()
		{
			if (materialManagers != null && materialManagers.Count > 0)
			{
				foreach (KeyValuePair<string, MaterialManager> kv in materialManagers)
				{
					kv.Value.Dispose();
				}
				materialManagers.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="allowDestroyingAssets"></param>
		public void Dispose(bool allowDestroyingAssets)
		{
			if (!disposed)
			{
				disposed = true;

				DestroyMaterials();
				if (root == this && nativeTexture != null && allowDestroyingAssets)
				{
					if (storedODisk)
						Resources.UnloadAsset(nativeTexture);
					else
						Texture.DestroyImmediate(nativeTexture, true);
				}
				nativeTexture = null;
				root = null;
			}
		}
	}
}
