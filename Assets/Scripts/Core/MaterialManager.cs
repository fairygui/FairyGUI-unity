using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Every texture and shader combination has a MaterialManager.
	/// </summary>
	public class MaterialManager
	{
		NTexture _texture;
		Shader _shader;
		string[] _keywords;
		List<NMaterial>[] _materials;

		internal string _managerKey;

		static string[][] internalKeywords = {
			null,
			new string[] { "GRAYED" },
			new string[] { "CLIPPED" },
			new string[] { "CLIPPED", "GRAYED" },
			new string[] { "SOFT_CLIPPED" },
			new string[] { "SOFT_CLIPPED", "GRAYED" },
			new string[] { "ALPHA_MASK" }
		};
		const int internalKeywordCount = 7;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		internal MaterialManager(NTexture texture, Shader shader, string[] keywords)
		{
			_texture = texture;
			_shader = shader;
			_keywords = keywords;
			_materials = new List<NMaterial>[internalKeywordCount * 2];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="materialType"></param>
		/// <param name="blendMode"></param>
		/// <param name="clipId"></param>
		/// <returns></returns>
		public NMaterial GetMaterial(int materialType, BlendMode blendMode, uint clipId)
		{
			uint frameId = UpdateContext.frameId;
			List<NMaterial> items;

			if (blendMode == BlendMode.Normal)
			{
				items = _materials[materialType];
				if (items == null)
				{
					items = new List<NMaterial>();
					_materials[materialType] = items;
				}
			}
			else
			{
				items = _materials[internalKeywordCount + materialType];
				if (items == null)
				{
					items = new List<NMaterial>();
					_materials[internalKeywordCount + materialType] = items;
				}
			}

			int cnt = items.Count;
			NMaterial result = null;
			for (int i = 0; i < cnt; i++)
			{
				NMaterial mat = items[i];
				if (mat.frameId == frameId)
				{
					if (materialType != 6 && mat.clipId == clipId && mat.blendMode == blendMode)
					{
						mat._firstInstance = false;
						return mat;
					}
				}
				else
				{
					result = mat;
					break;
				}
			}

			if (result != null)
			{
				result.frameId = frameId;
				result.clipId = clipId;
				result.blendMode = blendMode;

				if (result.combined)
					result.material.SetTexture(ShaderConfig._properyIDs._AlphaTex, _texture.alphaTexture);
			}
			else
			{
				result = CreateMaterial();
				string[] keywords = internalKeywords[materialType];
				if (keywords != null)
				{
					cnt = keywords.Length;
					for (int i = 0; i < cnt; i++)
						result.material.EnableKeyword(keywords[i]);
				}
				result.frameId = frameId;
				result.clipId = clipId;
				result.blendMode = blendMode;
				if (BlendModeUtils.Factors[(int)result.blendMode].pma)
					result.material.EnableKeyword("COLOR_FILTER");
				items.Add(result);
			}

			result._firstInstance = true;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		NMaterial CreateMaterial()
		{
			NMaterial nm = new NMaterial(_shader);
			nm.material.mainTexture = _texture.nativeTexture;
			if (_texture.alphaTexture != null)
			{
				nm.combined = true;
				nm.material.EnableKeyword("COMBINED");
				nm.material.SetTexture(ShaderConfig._properyIDs._AlphaTex, _texture.alphaTexture);
			}
			if (_keywords != null)
			{
				int cnt = _keywords.Length;
				for (int i = 0; i < cnt; i++)
					nm.material.EnableKeyword(_keywords[i]);
			}
			nm.material.hideFlags = DisplayOptions.hideFlags;

			return nm;
		}

		/// <summary>
		/// 
		/// </summary>
		public void DestroyMaterials()
		{
			int cnt = _materials.Length;
			for (int i = 0; i < cnt; i++)
			{
				List<NMaterial> items = _materials[i];
				if (items != null)
				{
					if (Application.isPlaying)
					{
						int cnt2 = items.Count;
						for (int j = 0; j < cnt2; j++)
							Object.Destroy(items[j].material);
					}
					else
					{
						int cnt2 = items.Count;
						for (int j = 0; j < cnt2; j++)
							Object.DestroyImmediate(items[j].material);
					}
					items.Clear();
				}
			}
		}

		public void RefreshMaterials()
		{
			int cnt = _materials.Length;
			bool hasAlphaTexture = _texture.alphaTexture != null;
			for (int i = 0; i < cnt; i++)
			{
				List<NMaterial> items = _materials[i];
				if (items != null)
				{
					int cnt2 = items.Count;
					for (int j = 0; j < cnt2; j++)
					{
						NMaterial nm = items[j];
						nm.material.mainTexture = _texture.nativeTexture;
						if (hasAlphaTexture)
						{
							if (!nm.combined)
							{
								nm.combined = true;
								nm.material.EnableKeyword("COMBINED");
							}
							nm.material.SetTexture(ShaderConfig._properyIDs._AlphaTex, _texture.alphaTexture);
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Release()
		{
			if (_keywords != null)
				_texture.DestroyMaterialManager(this);
		}
	}
}
