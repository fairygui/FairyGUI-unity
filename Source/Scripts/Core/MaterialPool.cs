using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	class MaterialPool
	{
		List<NMaterial> _items;
		List<NMaterial> _blendItems;
		MaterialManager _manager;
		string[] _variants;
		bool _notShared;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="variants"></param>
		/// <param name="notShared"></param>
		public MaterialPool(MaterialManager manager, string[] variants, bool notShared)
		{
			_manager = manager;
			_variants = variants;
			_notShared = notShared;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public NMaterial Get()
		{
			List<NMaterial> items;

			if (_manager.blendMode == BlendMode.Normal)
			{
				if (_items == null)
					_items = new List<NMaterial>();
				items = _items;
			}
			else
			{
				if (_blendItems == null)
					_blendItems = new List<NMaterial>();
				items = _blendItems;
			}

			int cnt = items.Count;
			NMaterial result = null;
			for (int i = 0; i < cnt; i++)
			{
				NMaterial mat = items[i];
				if (mat.frameId == _manager.frameId)
				{
					if (!_notShared && mat.clipId == _manager.clipId && mat.blendMode == _manager.blendMode)
						return mat;
				}
				else if (result == null)
					result = mat;
			}

			if (result != null)
			{
				result.frameId = _manager.frameId;
				result.clipId = _manager.clipId;
				result.blendMode = _manager.blendMode;
			}
			else
			{
				result = _manager.CreateMaterial();
				if (_variants != null)
				{
					foreach (string v in _variants)
						result.material.EnableKeyword(v);
				}
				result.frameId = _manager.frameId;
				result.clipId = _manager.clipId;
				result.blendMode = _manager.blendMode;
				items.Add(result);
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			if (_items != null)
				_items.Clear();

			if (_blendItems != null)
				_blendItems.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (_items != null)
			{
				if (Application.isPlaying)
				{
					foreach (NMaterial nm in _items)
						Material.Destroy(nm.material);
				}
				else
				{
					foreach (NMaterial nm in _items)
						Material.DestroyImmediate(nm.material);
				}
				_items = null;
			}

			if (_blendItems != null)
			{
				if (Application.isPlaying)
				{
					foreach (NMaterial nm in _blendItems)
						Material.Destroy(nm.material);
				}
				else
				{
					foreach (NMaterial nm in _blendItems)
						Material.DestroyImmediate(nm.material);
				}
				_blendItems = null;
			}
		}
	}
}
