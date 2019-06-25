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

		int _maskFlag;
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
				if (_meshFactory != value)
				{
					_meshFactory = value;
					_meshDirty = true;
				}
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
			if (!_customMatarial)
			{
				if (_manager != null)
				{
					if (context.clipped && !dontClip)
					{
						if (_maskFlag == 1)
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

					nm = _manager.GetMaterial(matType, blendMode, clipId);
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

			if ((nm == null || nm._firstInstance) && (object)_material != null)
			{
				if (blendMode != BlendMode.Normal) //GetMateria已经保证了不同的blendMode会返回不同的共享材质，所以这里可以放心设置
					BlendModeUtils.Apply(_material, blendMode);

				bool clearStencil = nm == null || nm.stencilSet;
				if (context.clipped)
				{
					if (_maskFlag != 1 && context.rectMaskDepth > 0) //在矩形剪裁下，且不是遮罩对象
					{
						_material.SetVector(ShaderConfig._properyIDs._ClipBox, context.clipInfo.clipBox);
						if (context.clipInfo.soft)
							_material.SetVector(ShaderConfig._properyIDs._ClipSoftness, context.clipInfo.softness);
					}

					if (context.stencilReferenceValue > 0)
					{
						if (_maskFlag == 1) //是遮罩
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
						clearStencil = false;
					}
				}

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

			if (_maskFlag != 0)
			{
				if (_maskFlag == 1)
					_maskFlag = 2;
				else
				{
					if (_stencilEraser != null)
						_stencilEraser.enabled = false;

					_maskFlag = 0;
				}
			}
		}

		internal void _PreUpdateMask(UpdateContext context)
		{
			//_maskFlag: 0-new mask, 1-active mask, 2-mask complete
			if (_maskFlag == 0)
			{
				if (_stencilEraser == null)
				{
					_stencilEraser = new StencilEraser(gameObject.transform);
					_stencilEraser.meshFilter.mesh = mesh;
				}
				else
					_stencilEraser.enabled = true;
			}

			_maskFlag = 1;

			if (_manager != null)
			{
				NMaterial nm = _manager.GetMaterial(6, blendMode, context.clipInfo.clipId);
				Material mat = nm.material;
				if ((object)mat != (object)_stencilEraser.meshRenderer.sharedMaterial)
					_stencilEraser.meshRenderer.sharedMaterial = mat;

				int refValue = context.stencilReferenceValue - 1;
				mat.SetInt(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
				mat.SetInt(ShaderConfig._properyIDs._Stencil, refValue);
				mat.SetInt(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
				mat.SetInt(ShaderConfig._properyIDs._StencilReadMask, refValue);
				mat.SetInt(ShaderConfig._properyIDs._ColorMask, 0);
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
			if (_flip != FlipType.None)
			{
				if (_flip == FlipType.Horizontal || _flip == FlipType.Both)
				{
					float tmp = vb.uvRect.xMin;
					vb.uvRect.xMin = vb.uvRect.xMax;
					vb.uvRect.xMax = tmp;
				}
				if (_flip == FlipType.Vertical || _flip == FlipType.Both)
				{
					float tmp = vb.uvRect.yMin;
					vb.uvRect.yMin = vb.uvRect.yMax;
					vb.uvRect.yMax = tmp;
				}
			}
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

			if (_texture.rotated)
			{
				float xMin = _texture.uvRect.xMin;
				float yMin = _texture.uvRect.yMin;
				float yMax = _texture.uvRect.yMax;
				float tmp;
				for (int i = 0; i < vertCount; i++)
				{
					Vector4 vec = vb.uv0[i];
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

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
			if (vb._isArbitraryQuad)
				vb.FixUVForArbitraryQuad();

			mesh.SetVertices(vb.vertices);
			mesh.SetUVs(0, vb.uv0);
			mesh.SetColors(vb.colors);
			mesh.SetTriangles(vb.triangles, 0);

#if !UNITY_5_6_OR_NEWER
			_colors = null;
#endif

#else
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
#endif
			vb.End();

			if (meshModifier != null)
				meshModifier();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rect rect = texture.GetDrawRect(vb.contentRect);

			vb.AddQuad(rect, vb.vertexColor, vb.uvRect);
			vb.AddTriangles();
			vb._isArbitraryQuad = vertexMatrix != null;
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
