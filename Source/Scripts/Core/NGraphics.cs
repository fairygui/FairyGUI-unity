using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NGraphics : IMeshFactory
	{
		/// <summary>
		/// 
		/// </summary>
		public GameObject gameObject { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public MeshFilter meshFilter { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public MeshRenderer meshRenderer { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Mesh mesh { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public BlendMode blendMode;

		/// <summary>
		/// 不参与剪裁
		/// </summary>
		public bool dontClip;

		/// <summary>
		/// 
		/// </summary>
		public delegate void MeshModifier();

		/// <summary>
		/// 当Mesh更新时被调用
		/// </summary>
		public MeshModifier meshModifier;

		NTexture _texture;
		string _shader;
		Material _material;
		bool _customMatarial;
		MaterialManager _manager;
		string[] _materialKeywords;
		IMeshFactory _meshFactory;

		float _alpha;
		Color _color;
		bool _meshDirty;
		Rect _contentRect;
		FlipType _flip;
		Matrix4x4? _vertexMatrix;
		Vector3? _cameraPosition;

		bool hasAlphaBackup;
		List<byte> _alphaBackup; //透明度改变需要通过修改顶点颜色实现，但顶点颜色本身可能就带有透明度，所以这里要有一个备份

		bool _isMask;
		StencilEraser _stencilEraser;

#if !(UNITY_5_2 || UNITY_5_3_OR_NEWER)
		Vector3[] _vertices;
		Vector2[] _uv;
		int[] _triangles;
#endif

#if !UNITY_5_6_OR_NEWER
		Color32[] _colors;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameObject"></param>
		public NGraphics(GameObject gameObject)
		{
			this.gameObject = gameObject;

			_alpha = 1f;
			_shader = ShaderConfig.imageShader;
			_color = Color.white;
			_meshFactory = this;

			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#else
			meshRenderer.castShadows = false;
#endif
			meshRenderer.receiveShadows = false;

			mesh = new Mesh();
			mesh.name = gameObject.name;
			mesh.MarkDynamic();

			meshFilter.mesh = mesh;

			meshFilter.hideFlags = DisplayOptions.hideFlags;
			meshRenderer.hideFlags = DisplayOptions.hideFlags;
			mesh.hideFlags = DisplayOptions.hideFlags;

			Stats.LatestGraphicsCreation++;
		}

		/// <summary>
		/// 
		/// </summary>
		public IMeshFactory meshFactory
		{
			get { return _meshFactory; }
			set
			{
				_meshFactory = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetMeshFactory<T>() where T : IMeshFactory, new()
		{
			if (!(_meshFactory is T))
			{
				_meshFactory = new T();
				_meshDirty = true;
			}
			return (T)_meshFactory;
		}

		/// <summary>
		/// 
		/// </summary>
		public Rect contentRect
		{
			get { return _contentRect; }
			set
			{
				_contentRect = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FlipType flip
		{
			get { return _flip; }
			set
			{
				if (_flip != value)
				{
					_flip = value;
					_meshDirty = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				if (_texture != value)
				{
					_texture = value;
					if (_customMatarial && _material != null)
						_material.mainTexture = _texture != null ? _texture.nativeTexture : null;
					_meshDirty = true;
					UpdateManager();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string shader
		{
			get { return _shader; }
			set
			{
				_shader = value;
				UpdateManager();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shader"></param>
		/// <param name="texture"></param>
		public void SetShaderAndTexture(string shader, NTexture texture)
		{
			_shader = shader;
			_texture = texture;
			if (_customMatarial && _material != null)
				_material.mainTexture = _texture != null ? _texture.nativeTexture : null;
			_meshDirty = true;
			UpdateManager();
		}

		/// <summary>
		/// 
		/// </summary>
		public Material material
		{
			get { return _material; }
			set
			{
				_material = value;
				if (_material != null)
				{
					_customMatarial = true;
					meshRenderer.sharedMaterial = _material;
					if (_texture != null)
						_material.mainTexture = _texture.nativeTexture;
				}
				else
				{
					_customMatarial = false;
					meshRenderer.sharedMaterial = null;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string[] materialKeywords
		{
			get { return _materialKeywords; }
			set
			{
				_materialKeywords = value;
				UpdateManager();
			}
		}

		void UpdateManager()
		{
			MaterialManager mm;
			if (_texture != null)
				mm = _texture.GetMaterialManager(_shader, _materialKeywords);
			else
				mm = null;
			if (_manager != null && _manager != mm)
				_manager.Release();
			_manager = mm;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool enabled
		{
			get { return meshRenderer.enabled; }
			set { meshRenderer.enabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int sortingOrder
		{
			get { return meshRenderer.sortingOrder; }
			set { meshRenderer.sortingOrder = value; }
		}

		internal void _SetAsMask(bool value)
		{
			if (_isMask != value)
			{
				_isMask = value;
				if (_isMask)
				{
					//设置擦除stencil的drawcall
					if (_stencilEraser == null)
					{
						_stencilEraser = new StencilEraser(gameObject.transform);
						_stencilEraser.meshFilter.mesh = mesh;
					}
					else
						_stencilEraser.enabled = true;
				}
				else
				{
					if (_stencilEraser != null)
						_stencilEraser.enabled = false;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		internal void _SetStencilEraserOrder(int value)
		{
			_stencilEraser.meshRenderer.sortingOrder = value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public Color color
		{
			get { return _color; }
			set { _color = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public void Tint()
		{
			if (_meshDirty)
				return;

			int vertCount = mesh.vertexCount;
			if (vertCount == 0)
				return;

#if !UNITY_5_6_OR_NEWER
			Color32[] colors = _colors;
			if (colors == null)
				colors = mesh.colors32;
#else
			VertexBuffer vb = VertexBuffer.Begin();
			mesh.GetColors(vb.colors);
			List<Color32> colors = vb.colors;
#endif
			for (int i = 0; i < vertCount; i++)
			{
				Color32 col = _color;
				col.a = (byte)(_alpha * (hasAlphaBackup ? _alphaBackup[i] : (byte)255));
				colors[i] = col;
			}

#if !UNITY_5_6_OR_NEWER
			mesh.colors32 = colors;
#else
			mesh.SetColors(vb.colors);
			vb.End();
#endif
		}

		void ChangeAlpha(float value)
		{
			_alpha = value;

			int vertCount = mesh.vertexCount;
			if (vertCount == 0)
				return;

#if !UNITY_5_6_OR_NEWER
			Color32[] colors = _colors;
			if (colors == null)
				colors = mesh.colors32;
#else
			VertexBuffer vb = VertexBuffer.Begin();
			mesh.GetColors(vb.colors);
			List<Color32> colors = vb.colors;
#endif
			for (int i = 0; i < vertCount; i++)
			{
				Color32 col = colors[i];
				col.a = (byte)(_alpha * (hasAlphaBackup ? _alphaBackup[i] : (byte)255));
				colors[i] = col;
			}

#if !UNITY_5_6_OR_NEWER
			mesh.colors32 = colors;
#else
			mesh.SetColors(vb.colors);
			vb.End();
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		public Matrix4x4? vertexMatrix
		{
			get { return _vertexMatrix; }
			set
			{
				_vertexMatrix = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3? cameraPosition
		{
			get { return _cameraPosition; }
			set
			{
				_cameraPosition = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetMeshDirty()
		{
			_meshDirty = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool UpdateMesh()
		{
			if (_meshDirty)
			{
				UpdateMeshNow();
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (mesh != null)
			{
				if (Application.isPlaying)
					UnityEngine.Object.Destroy(mesh);
				else
					UnityEngine.Object.DestroyImmediate(mesh);
				mesh = null;
			}
			if (_manager != null)
			{
				_manager.Release();
				_manager = null;
			}
			_material = null;
			meshRenderer = null;
			meshFilter = null;
			_stencilEraser = null;
			meshModifier = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="alpha"></param>
		/// <param name="grayed"></param>
		public void Update(UpdateContext context, float alpha, bool grayed)
		{
			Stats.GraphicsCount++;

			if (_meshDirty)
			{
				_alpha = alpha;
				UpdateMeshNow();
			}
			else if (_alpha != alpha)
				ChangeAlpha(alpha);

			uint clipId = context.clipInfo.clipId;
			int matType = 0;
			NMaterial nm = null;
			bool firstInstance = true;
			if (!_customMatarial)
			{
				if (_manager != null)
				{
					if (context.clipped && !dontClip)
					{
						if (_isMask)
							matType = 6;
						else if (context.rectMaskDepth == 0)
							matType = grayed ? 1 : 0;
						else if (context.clipInfo.soft)
							matType = grayed ? 5 : 4;
						else
							matType = grayed ? 3 : 2;
					}
					else
					{
						clipId = 0;
						matType = grayed ? 1 : 0;
					}

					nm = _manager.GetMaterial(matType, blendMode, clipId, out firstInstance);
					_material = nm.material;
					if ((object)_material != (object)meshRenderer.sharedMaterial)
						meshRenderer.sharedMaterial = _material;
				}
				else
				{
					_material = null;
					if ((object)meshRenderer.sharedMaterial != null)
						meshRenderer.sharedMaterial = null;
				}
			}

			if (firstInstance && (object)_material != null)
			{
				if (blendMode != BlendMode.Normal) //GetMateria已经保证了不同的blendMode会返回不同的共享材质，所以这里可以放心设置
					BlendModeUtils.Apply(_material, blendMode);

				bool clearStencil = false;
				if (context.clipped)
				{
					if (!_isMask && context.rectMaskDepth > 0) //在矩形剪裁下，且不是遮罩对象
					{
						_material.SetVector(ShaderConfig._properyIDs._ClipBox, context.clipInfo.clipBox);
						if (context.clipInfo.soft)
							_material.SetVector(ShaderConfig._properyIDs._ClipSoftness, context.clipInfo.softness);
					}

					if (context.stencilReferenceValue > 0)
					{
						if (_isMask) //是遮罩
						{
							if (context.stencilReferenceValue == 1)
							{
								_material.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
								_material.SetInt(ShaderConfig._properyIDs._Stencil, 1);
								_material.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
								_material.SetInt(ShaderConfig._properyIDs._StencilReadMask, 255);
								_material.SetInt(ShaderConfig._properyIDs._ColorMask, 0);
							}
							else
							{
								_material.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
								_material.SetInt(ShaderConfig._properyIDs._Stencil, context.stencilReferenceValue | (context.stencilReferenceValue - 1));
								_material.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
								_material.SetInt(ShaderConfig._properyIDs._StencilReadMask, context.stencilReferenceValue - 1);
								_material.SetInt(ShaderConfig._properyIDs._ColorMask, 0);
							}

							if (nm != null)
							{
								NMaterial eraserNm = _manager.GetMaterial(matType, blendMode, clipId, out firstInstance);
								eraserNm.stencilSet = true;
								Material eraserMat = eraserNm.material;
								if ((object)eraserMat != (object)_stencilEraser.meshRenderer.sharedMaterial)
									_stencilEraser.meshRenderer.sharedMaterial = eraserMat;

								int refValue = context.stencilReferenceValue - 1;
								eraserMat.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
								eraserMat.SetInt(ShaderConfig._properyIDs._Stencil, refValue);
								eraserMat.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
								eraserMat.SetInt(ShaderConfig._properyIDs._StencilReadMask, refValue);
								eraserMat.SetInt(ShaderConfig._properyIDs._ColorMask, 0);
							}
						}
						else
						{
							int refValue = context.stencilReferenceValue | (context.stencilReferenceValue - 1);
							if (context.clipInfo.reversedMask)
								_material.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.NotEqual);
							else
								_material.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
							_material.SetInt(ShaderConfig._properyIDs._Stencil, refValue);
							_material.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
							_material.SetInt(ShaderConfig._properyIDs._StencilReadMask, refValue);
							_material.SetInt(ShaderConfig._properyIDs._ColorMask, 15);
						}
						if (nm != null)
							nm.stencilSet = true;
					}
					else
						clearStencil = nm == null || nm.stencilSet;
				}
				else
					clearStencil = nm == null || nm.stencilSet;

				if (clearStencil)
				{
					if (nm != null)
						nm.stencilSet = false;

					_material.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
					_material.SetInt(ShaderConfig._properyIDs._Stencil, 0);
					_material.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
					_material.SetInt(ShaderConfig._properyIDs._StencilReadMask, 255);
					_material.SetInt(ShaderConfig._properyIDs._ColorMask, 15);
				}
			}
		}

		void UpdateMeshNow()
		{
			_meshDirty = false;

			if (_texture == null || _meshFactory == null)
			{
				if (mesh.vertexCount > 0)
				{
					mesh.Clear();

					if (meshModifier != null)
						meshModifier();
				}
				return;
			}

			VertexBuffer vb = VertexBuffer.Begin();
			vb.contentRect = _contentRect;
			vb.uvRect = _texture.uvRect;
			vb.vertexColor = _color;
			_meshFactory.OnPopulateMesh(vb);

			int vertCount = vb.currentVertCount;
			if (vertCount == 0)
			{
				if (mesh.vertexCount > 0)
				{
					mesh.Clear();

					if (meshModifier != null)
						meshModifier();
				}
				vb.End();
				return;
			}

			if (_flip != FlipType.None)
			{
				bool h = _flip == FlipType.Horizontal || _flip == FlipType.Both;
				bool v = _flip == FlipType.Vertical || _flip == FlipType.Both;
				float xMax = _contentRect.xMax;
				float yMax = _contentRect.yMax;
				for (int i = 0; i < vertCount; i++)
				{
					Vector3 vec = vb.vertices[i];
					if (h)
						vec.x = xMax - (vec.x - _contentRect.x);
					if (v)
						vec.y = -(yMax - (-vec.y - _contentRect.y));
					vb.vertices[i] = vec;
				}
				if (!(h && v))
					vb.triangles.Reverse();
			}

			if (_texture.rotated)
			{
				float xMin = _texture.uvRect.xMin;
				float yMin = _texture.uvRect.yMin;
				float yMax = _texture.uvRect.yMax;
				float tmp;
				for (int i = 0; i < vertCount; i++)
				{
					Vector2 vec = vb.uv0[i];
					tmp = vec.y;
					vec.y = yMin + vec.x - xMin;
					vec.x = xMin + yMax - tmp;
					vb.uv0[i] = vec;
				}
			}

			hasAlphaBackup = vb._alphaInVertexColor;
			if (hasAlphaBackup)
			{
				if (_alphaBackup == null)
					_alphaBackup = new List<byte>();
				else
					_alphaBackup.Clear();
				for (int i = 0; i < vertCount; i++)
				{
					Color32 col = vb.colors[i];
					_alphaBackup.Add(col.a);

					col.a = (byte)(col.a * _alpha);
					vb.colors[i] = col;
				}
			}
			else if (_alpha != 1)
			{
				for (int i = 0; i < vertCount; i++)
				{
					Color32 col = vb.colors[i];
					col.a = (byte)(col.a * _alpha);
					vb.colors[i] = col;
				}
			}

			if (_vertexMatrix != null)
			{
				Matrix4x4 mm = (Matrix4x4)_vertexMatrix;
				Vector3 camPos = _cameraPosition != null ? (Vector3)_cameraPosition : Vector3.zero;
				Vector3 center = new Vector3(camPos.x, camPos.y, 0);
				center -= mm.MultiplyPoint(center);
				for (int i = 0; i < vertCount; i++)
				{
					Vector3 pt = vb.vertices[i];
					pt = mm.MultiplyPoint(pt);
					pt += center;
					Vector3 vec = pt - camPos;
					float lambda = -camPos.z / vec.z;
					pt.x = camPos.x + lambda * vec.x;
					pt.y = camPos.y + lambda * vec.y;
					pt.z = 0;

					vb.vertices[i] = pt;
				}
			}

			mesh.Clear();

#if !(UNITY_5_2 || UNITY_5_3_OR_NEWER)
			if (_vertices == null || _vertices.Length != vertCount)
			{
				_vertices = new Vector3[vertCount];
				_uv = new Vector2[vertCount];
				_colors = new Color32[vertCount];
			}
			vb.vertices.CopyTo(_vertices);
			vb.uv0.CopyTo(_uv);
			vb.colors.CopyTo(_colors);

			if (_triangles == null || _triangles.Length != vb.triangles.Count)
				_triangles = new int[vb.triangles.Count];
			vb.triangles.CopyTo(_triangles);

			mesh.vertices = _vertices;
			mesh.uv = _uv;
			mesh.triangles = _triangles;
			mesh.colors32 = _colors;
#else

#if !UNITY_5_6_OR_NEWER
			_colors = null;
#endif
			mesh.SetVertices(vb.vertices);
			mesh.SetUVs(0, vb.uv0);
			mesh.SetColors(vb.colors);
			mesh.SetTriangles(vb.triangles, 0);
#endif

			vb.End();

			if (meshModifier != null)
				meshModifier();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rect rect = texture.GetDrawRect(vb.contentRect);

			if (_vertexMatrix != null)//画多边形时，要对UV处理才能有正确的显示，暂时未掌握，这里用更多的面来临时解决。
			{
				int hc = (int)Mathf.Min(Mathf.CeilToInt(rect.width / 30), 9);
				int vc = (int)Mathf.Min(Mathf.CeilToInt(rect.height / 30), 9);
				int eachPartX = Mathf.FloorToInt(rect.width / hc);
				int eachPartY = Mathf.FloorToInt(rect.height / vc);
				float x, y;
				for (int i = 0; i <= vc; i++)
				{
					if (i == vc)
						y = rect.yMax;
					else
						y = rect.y + i * eachPartY;
					for (int j = 0; j <= hc; j++)
					{
						if (j == hc)
							x = rect.xMax;
						else
							x = rect.x + j * eachPartX;
						vb.AddVert(new Vector3(x, y, 0));
					}
				}

				for (int i = 0; i < vc; i++)
				{
					int k = i * (hc + 1);
					for (int j = 1; j <= hc; j++)
					{
						int m = k + j;
						vb.AddTriangle(m - 1, m, m + hc);
						vb.AddTriangle(m, m + hc + 1, m + hc);
					}
				}
			}
			else
			{
				vb.AddQuad(rect, vb.vertexColor, vb.uvRect);
				vb.AddTriangles();
			}
		}
	}

	class StencilEraser
	{
		public GameObject gameObject;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;

		public StencilEraser(Transform parent)
		{
			gameObject = new GameObject("StencilEraser");
			ToolSet.SetParent(gameObject.transform, parent);

			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#else
			meshRenderer.castShadows = false;
#endif
			meshRenderer.receiveShadows = false;

			gameObject.layer = parent.gameObject.layer;
			gameObject.hideFlags = parent.gameObject.hideFlags;
			meshFilter.hideFlags = parent.gameObject.hideFlags;
			meshRenderer.hideFlags = parent.gameObject.hideFlags;
		}

		public bool enabled
		{
			get { return meshRenderer.enabled; }
			set { meshRenderer.enabled = value; }
		}
	}
}
