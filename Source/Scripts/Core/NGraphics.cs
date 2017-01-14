using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NGraphics
	{
		/// <summary>
		/// 
		/// </summary>
		public Vector3[] vertices { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Vector2[] uv { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Color32[] colors { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int[] triangles { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int vertCount { get; private set; }

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
		public GameObject gameObject { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public bool grayed;

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
		public uint maskFrameId;

		/// <summary>
		/// 
		/// </summary>
		public Matrix4x4? vertexMatrix;

		/// <summary>
		/// 
		/// </summary>
		public Vector3? cameraPosition;

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

		float _alpha;
		//透明度改变需要通过修改顶点颜色实现，但顶点颜色本身可能就带有透明度，所以这里要有一个备份
		byte[] _alphaBackup;

		StencilEraser _stencilEraser;

		/// <summary>
		/// 写死的一些三角形顶点组合，避免每次new
		/// 1---2
		/// | / |
		/// 0---3
		/// </summary>
		public static int[] TRIANGLES = new int[] { 0, 1, 2, 2, 3, 0 };

		/// <summary>
		/// 
		/// </summary>
		public static int[] TRIANGLES_9_GRID = new int[] { 
			4,0,1,1,5,4,
			5,1,2,2,6,5,
			6,2,3,3,7,6,
			8,4,5,5,9,8,
			9,5,6,6,10,9,
			10,6,7,7,11,10,
			12,8,9,9,13,12,
			13,9,10,10,14,13,
			14,10,11,
			11,15,14
		};

		/// <summary>
		/// 
		/// </summary>
		public static int[] TRIANGLES_4_GRID = new int[] { 
			4, 0, 5,
			4, 5, 1, 
			4, 1, 6, 
			4, 6, 2,
			4, 2, 7,
			4, 7, 3,
			4, 3, 8, 
			4, 8, 0
		};

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameObject"></param>
		public NGraphics(GameObject gameObject)
		{
			this.gameObject = gameObject;
			_alpha = 1f;
			_shader = ShaderConfig.imageShader;
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_5
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#else
		meshRenderer.castShadows = false;
#endif
			meshRenderer.receiveShadows = false;
			mesh = new Mesh();
			mesh.MarkDynamic();

			meshFilter.hideFlags = DisplayOptions.hideFlags;
			meshRenderer.hideFlags = DisplayOptions.hideFlags;
			mesh.hideFlags = DisplayOptions.hideFlags;

			Stats.LatestGraphicsCreation++;
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
				if (_customMatarial && _material != null)
				{
					if (Application.isPlaying)
						Material.Destroy(_material);
					else
						Material.DestroyImmediate(_material);
				}

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
			if (_manager != null)
				_manager.Release();

			if (_texture != null)
				_manager = MaterialManager.GetInstance(_texture, _shader, _materialKeywords);
			else
				_manager = null;
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
		public void SetStencilEraserOrder(int value)
		{
			if (_stencilEraser != null)
				_stencilEraser.meshRenderer.sortingOrder = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (mesh != null)
			{
				if (Application.isPlaying)
					Mesh.Destroy(mesh);
				else
					Mesh.DestroyImmediate(mesh);
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
		public void UpdateMaterial(UpdateContext context)
		{
			Stats.GraphicsCount++;

			NMaterial nm = null;
			if (_manager != null && !_customMatarial)
			{
				nm = _manager.GetMaterial(this, context);
				_material = nm.material;
				if ((object)_material != (object)meshRenderer.sharedMaterial && (object)_material.mainTexture != null)
					meshRenderer.sharedMaterial = _material;

				if (nm.combined)
					_material.SetTexture("_AlphaTex", _manager.texture.alphaTexture.nativeTexture);
			}

			if (maskFrameId != 0 && maskFrameId != UpdateContext.frameId)
			{
				//曾经是遮罩对象，现在不是了
				if (_stencilEraser != null)
					_stencilEraser.enabled = false;
			}

			if (_material != null)
			{
				if (blendMode != BlendMode.Normal) //GetMateria已经保证了不同的blendMode会返回不同的共享材质，所以这里可以放心设置
					BlendModeUtils.Apply(_material, blendMode);

				bool clearStencil = false;
				if (context.clipped)
				{
					if (maskFrameId != UpdateContext.frameId && context.rectMaskDepth > 0) //在矩形剪裁下，且不是遮罩对象
					{
						_material.SetVector("_ClipBox", context.clipInfo.clipBox);
						if (context.clipInfo.soft)
							_material.SetVector("_ClipSoftness", context.clipInfo.softness);
					}

					if (context.stencilReferenceValue > 0)
					{
						if (maskFrameId == UpdateContext.frameId) //是遮罩
						{
							if (context.stencilReferenceValue == 1)
							{
								_material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
								_material.SetInt("_Stencil", 1);
								_material.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
								_material.SetInt("_StencilReadMask", 255);
								_material.SetInt("_ColorMask", 0);
							}
							else
							{
								_material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
								_material.SetInt("_Stencil", context.stencilReferenceValue | (context.stencilReferenceValue - 1));
								_material.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
								_material.SetInt("_StencilReadMask", context.stencilReferenceValue - 1);
								_material.SetInt("_ColorMask", 0);
							}

							//设置擦除stencil的drawcall
							if (_stencilEraser == null)
							{
								_stencilEraser = new StencilEraser(gameObject.transform);
								_stencilEraser.meshFilter.mesh = mesh;
							}
							else
								_stencilEraser.enabled = true;

							if (nm != null)
							{
								NMaterial eraserNm = _manager.GetMaterial(this, context);
								eraserNm.stencilSet = true;
								Material eraserMat = eraserNm.material;
								if ((object)eraserMat != (object)_stencilEraser.meshRenderer.sharedMaterial)
									_stencilEraser.meshRenderer.sharedMaterial = eraserMat;

								int refValue = context.stencilReferenceValue - 1;
								eraserMat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
								eraserMat.SetInt("_Stencil", refValue);
								eraserMat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
								eraserMat.SetInt("_StencilReadMask", refValue);
								eraserMat.SetInt("_ColorMask", 0);
							}
						}
						else
						{
							int refValue = context.stencilReferenceValue | (context.stencilReferenceValue - 1);
							_material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
							_material.SetInt("_Stencil", refValue);
							_material.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
							_material.SetInt("_StencilReadMask", refValue);
							_material.SetInt("_ColorMask", 15);
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
					_material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
					_material.SetInt("_Stencil", 0);
					_material.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
					_material.SetInt("_StencilReadMask", 255);
					_material.SetInt("_ColorMask", 15);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertCount"></param>
		public void Alloc(int vertCount)
		{
			if (vertices == null || vertices.Length != vertCount)
			{
				vertices = new Vector3[vertCount];
				uv = new Vector2[vertCount];
				colors = new Color32[vertCount];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void UpdateMesh()
		{
			if (meshModifier != null)
				meshModifier();

			Vector3[] vertices = this.vertices;
			vertCount = vertices.Length;
			if (vertexMatrix != null)
			{
				Matrix4x4 mm = (Matrix4x4)vertexMatrix;
				Vector3 camPos = cameraPosition != null ? (Vector3)cameraPosition : Vector3.zero;
				Vector3 center = new Vector3(camPos.x, camPos.y, 0);
				center -= mm.MultiplyPoint(center);
				for (int i = 0; i < vertCount; i++)
				{
					Vector3 pt = vertices[i];
					pt = mm.MultiplyPoint(pt);
					pt += center;
					Vector3 vec = pt - camPos;
					float lambda = -camPos.z / vec.z;
					pt.x = camPos.x + lambda * vec.x;
					pt.y = camPos.y + lambda * vec.y;
					pt.z = 0;

					vertices[i] = pt;
				}
			}

			Color32[] colors = this.colors;
			for (int i = 0; i < vertCount; i++)
			{
				Color32 col = colors[i];
				if (col.a != 255)
				{
					if (_alphaBackup == null)
						_alphaBackup = new byte[vertCount];
				}
				col.a = (byte)(col.a * _alpha);
				colors[i] = col;
			}

			if (_alphaBackup != null)
			{
				if (_alphaBackup.Length < vertCount)
					_alphaBackup = new byte[vertCount];

				for (int i = 0; i < vertCount; i++)
					_alphaBackup[i] = colors[i].a;
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles;
			mesh.colors32 = colors;
			meshFilter.mesh = mesh;

			if (_stencilEraser != null)
				_stencilEraser.meshFilter.mesh = mesh;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="color"></param>
		/// <param name="allColors"></param>
		public void SetOneQuadMesh(Rect drawRect, Rect uvRect, Color color, Color[] allColors = null)
		{
			//当四边形发生形变时，只用两个三角面表达会造成图形的变形较严重，这里做一个优化，自动增加更多的面
			if (vertexMatrix != null)
			{
				Alloc(9);

				FillVerts(0, drawRect);
				FillUV(0, uvRect);

				Vector2 camPos = cameraPosition != null ? (Vector2)cameraPosition : Vector2.zero;
				if (camPos.x == 0)
					camPos.x = drawRect.x + drawRect.width / 2;
				if (camPos.y == 0)
					camPos.y = -(drawRect.y + drawRect.height / 2);
				float cx = uvRect.x + (camPos.x - drawRect.x) / drawRect.width * uvRect.width;
				float cy = uvRect.y - (camPos.y - drawRect.y) / drawRect.height * uvRect.height;

				vertices[4] = new Vector3(camPos.x, camPos.y, 0);
				vertices[5] = new Vector3(drawRect.xMin, camPos.y, 0);
				vertices[6] = new Vector3(camPos.x, -drawRect.yMin, 0);
				vertices[7] = new Vector3(drawRect.xMax, camPos.y, 0);
				vertices[8] = new Vector3(camPos.x, -drawRect.yMax, 0);

				uv[4] = new Vector2(cx, cy);
				uv[5] = new Vector2(uvRect.xMin, cy);
				uv[6] = new Vector2(cx, uvRect.yMax);
				uv[7] = new Vector2(uvRect.xMax, cy);
				uv[8] = new Vector2(cx, uvRect.yMin);

				this.triangles = TRIANGLES_4_GRID;
			}
			else
			{
				Alloc(4);
				FillVerts(0, drawRect);
				FillUV(0, uvRect);
				this.triangles = TRIANGLES;
			}

			if (allColors != null)
			{
				Color32[] arr = this.colors;
				int count = arr.Length;
				for (int i = 0; i < count; i++)
					arr[i] = allColors[i % allColors.Length];
			}
			else
				FillColors(color);
			UpdateMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		/// <param name="allColors"></param>
		public void DrawRect(Rect vertRect, int lineSize, Color lineColor, Color fillColor, Color[] allColors)
		{
			if (lineSize == 0)
			{
				SetOneQuadMesh(new Rect(0, 0, vertRect.width, vertRect.height), new Rect(0, 0, 1, 1), fillColor, allColors);
			}
			else
			{
				Alloc(20);

				Rect rect;
				//left,right
				rect = Rect.MinMaxRect(0, 0, lineSize, vertRect.height);
				FillVerts(0, rect);
				rect = Rect.MinMaxRect(vertRect.width - lineSize, 0, vertRect.width, vertRect.height);
				FillVerts(4, rect);

				//top, bottom
				rect = Rect.MinMaxRect(lineSize, 0, vertRect.width - lineSize, lineSize);
				FillVerts(8, rect);
				rect = Rect.MinMaxRect(lineSize, vertRect.height - lineSize, vertRect.width - lineSize, vertRect.height);
				FillVerts(12, rect);

				//middle
				rect = Rect.MinMaxRect(lineSize, lineSize, vertRect.width - lineSize, vertRect.height - lineSize);
				FillVerts(16, rect);

				rect = new Rect(0, 0, 1, 1);
				int i;
				for (i = 0; i < 5; i++)
					FillUV(i * 4, rect);

				Color32[] arr = this.colors;
				if (allColors != null)
				{
					int colorCnt = allColors.Length;
					for (i = 0; i < 20; i++)
						arr[i] = allColors[i % colorCnt];
				}
				else
				{
					Color32 col32 = lineColor;
					for (i = 0; i < 16; i++)
						arr[i] = col32;

					col32 = fillColor;
					for (i = 16; i < 20; i++)
						arr[i] = col32;
				}

				FillTriangles();
				UpdateMesh();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="fillColor"></param>
		/// <param name="allColors"></param>
		public void DrawEllipse(Rect vertRect, Color fillColor, Color[] allColors)
		{
			float radiusX = vertRect.width / 2;
			float radiusY = vertRect.height / 2;
			int numSides = Mathf.CeilToInt(Mathf.PI * (radiusX + radiusY) / 4);
			if (numSides < 6) numSides = 6;
			Alloc(numSides + 1);

			float angleDelta = 2 * Mathf.PI / numSides;
			float angle = 0;

			vertices[0] = new Vector3(radiusX, -radiusY);
			for (int i = 1; i <= numSides; i++)
			{
				vertices[i] = new Vector3(Mathf.Cos(angle) * radiusX + radiusX,
					Mathf.Sin(angle) * radiusY - radiusY, 0);
				angle += angleDelta;
			}

			AllocTriangleArray(numSides * 3);

			int[] triangles = this.triangles;
			int k = 0;
			for (int i = 1; i < numSides; i++)
			{
				triangles[k++] = i + 1;
				triangles[k++] = i;
				triangles[k++] = 0;
			}
			triangles[k++] = 1;
			triangles[k++] = numSides;
			triangles[k++] = 0;

			if (allColors != null)
			{
				int colorCnt = allColors.Length;
				Color32[] arr = this.colors;
				arr[0] = allColors[0];
				colorCnt--;
				for (int i = 1; i <= numSides; i++)
					arr[i] = allColors[(i - 1) % colorCnt + 1];
			}
			else
				FillColors(fillColor);

			UpdateMesh();
		}

		static List<int> sRestIndices = new List<int>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="fillColor"></param>
		/// <param name="allColors"></param>
		public void DrawPolygon(Vector2[] points, Color fillColor, Color[] allColors)
		{
			int numVertices = points.Length;
			if (numVertices < 3)
				return;

			int numTriangles = numVertices - 2;
			int i, restIndexPos, numRestIndices;
			int k = 0;

			Alloc(numVertices);

			for (i = 0; i < numVertices; i++)
				vertices[i] = new Vector3(points[i].x, -points[i].y);

			// Algorithm "Ear clipping method" described here:
			// -> https://en.wikipedia.org/wiki/Polygon_triangulation
			//
			// Implementation inspired by:
			// -> http://polyk.ivank.net
			// -> Starling

			AllocTriangleArray(numTriangles * 3);

			sRestIndices.Clear();
			for (i = 0; i < numVertices; ++i)
				sRestIndices.Add(i);

			restIndexPos = 0;
			numRestIndices = numVertices;

			Vector2 a, b, c, p;
			int otherIndex;
			bool earFound;
			int i0, i1, i2;

			while (numRestIndices > 3)
			{
				earFound = false;
				i0 = sRestIndices[restIndexPos % numRestIndices];
				i1 = sRestIndices[(restIndexPos + 1) % numRestIndices];
				i2 = sRestIndices[(restIndexPos + 2) % numRestIndices];

				a = points[i0];
				b = points[i1];
				c = points[i2];

				if ((a.y - b.y) * (c.x - b.x) + (b.x - a.x) * (c.y - b.y) >= 0)
				{
					earFound = true;
					for (i = 3; i < numRestIndices; ++i)
					{
						otherIndex = sRestIndices[(restIndexPos + i) % numRestIndices];
						p = points[otherIndex];

						if (ToolSet.IsPointInTriangle(ref p, ref  a, ref b, ref c))
						{
							earFound = false;
							break;
						}
					}
				}

				if (earFound)
				{
					triangles[k++] = i0;
					triangles[k++] = i1;
					triangles[k++] = i2;
					sRestIndices.RemoveAt((restIndexPos + 1) % numRestIndices);

					numRestIndices--;
					restIndexPos = 0;
				}
				else
				{
					restIndexPos++;
					if (restIndexPos == numRestIndices) break; // no more ears
				}
			}
			triangles[k++] = sRestIndices[0];
			triangles[k++] = sRestIndices[1];
			triangles[k++] = sRestIndices[2];

			if (allColors != null)
			{
				int colorCnt = allColors.Length;
				Color32[] arr = this.colors;
				for (i = 0; i < numVertices; i++)
					arr[i] = allColors[i % colorCnt];
			}
			else
				FillColors(fillColor);

			UpdateMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="amount"></param>
		/// <param name="origin"></param>
		/// <param name="clockwise"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		public void Fill(FillMethod method, float amount, int origin, bool clockwise, Rect vertRect, Rect uvRect)
		{
			amount = Mathf.Clamp01(amount);
			switch (method)
			{
				case FillMethod.Horizontal:
					Alloc(4);
					FillUtils.FillHorizontal((OriginHorizontal)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Vertical:
					Alloc(4);
					FillUtils.FillVertical((OriginVertical)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial90:
					Alloc(4);
					FillUtils.FillRadial90((Origin90)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial180:
					Alloc(8);
					FillUtils.FillRadial180((Origin180)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial360:
					Alloc(12);
					FillUtils.FillRadial360((Origin360)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;
			}
		}

		/// <summary>
		/// 从当前顶点缓冲区位置开始填入一个矩形的四个顶点
		/// </summary>
		/// <param name="index">填充位置顶点索引</param>
		/// <param name="rect"></param>
		public void FillVerts(int index, Rect rect)
		{
			vertices[index] = new Vector3(rect.xMin, -rect.yMax, 0f);
			vertices[index + 1] = new Vector3(rect.xMin, -rect.yMin, 0f);
			vertices[index + 2] = new Vector3(rect.xMax, -rect.yMin, 0f);
			vertices[index + 3] = new Vector3(rect.xMax, -rect.yMax, 0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public void FillUV(int index, Rect rect)
		{
			uv[index] = new Vector2(rect.xMin, rect.yMin);
			uv[index + 1] = new Vector2(rect.xMin, rect.yMax);
			uv[index + 2] = new Vector2(rect.xMax, rect.yMax);
			uv[index + 3] = new Vector2(rect.xMax, rect.yMin);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void FillColors(Color value)
		{
			Color32[] arr = this.colors;
			int count = arr.Length;
			Color32 col32 = value;
			for (int i = 0; i < count; i++)
				arr[i] = col32;
		}

		void AllocTriangleArray(int requestSize)
		{
			if (this.triangles == null
				|| this.triangles.Length != requestSize
				|| this.triangles == TRIANGLES
				|| this.triangles == TRIANGLES_9_GRID
				|| this.triangles == TRIANGLES_4_GRID)
				this.triangles = new int[requestSize];
		}

		/// <summary>
		/// 
		/// </summary>
		public void FillTriangles()
		{
			int vertCount = this.vertices.Length;
			AllocTriangleArray((vertCount >> 1) * 3);

			int k = 0;
			for (int i = 0; i < vertCount; i += 4)
			{
				triangles[k++] = i;
				triangles[k++] = i + 1;
				triangles[k++] = i + 2;

				triangles[k++] = i + 2;
				triangles[k++] = i + 3;
				triangles[k++] = i;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="triangles"></param>
		public void FillTriangles(int[] triangles)
		{
			this.triangles = triangles;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearMesh()
		{
			if (vertCount > 0)
			{
				vertCount = 0;

				mesh.Clear();
				meshFilter.mesh = mesh;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void Tint(Color value)
		{
			if (this.colors == null || vertCount == 0)
				return;

			Color32 value32 = value;
			int count = this.colors.Length;
			for (int i = 0; i < count; i++)
			{
				Color32 col = value32;
				if (col.a != 255)
				{
					if (_alphaBackup == null)
						_alphaBackup = new byte[vertCount];
				}
				col.a = (byte)(_alpha * 255);
				this.colors[i] = col;
			}

			if (_alphaBackup != null)
			{
				if (_alphaBackup.Length < vertCount)
					_alphaBackup = new byte[vertCount];

				for (int i = 0; i < vertCount; i++)
					_alphaBackup[i] = this.colors[i].a;
			}

			mesh.colors32 = this.colors;
		}

		/// <summary>
		/// 
		/// </summary>
		public float alpha
		{
			get { return _alpha; }
			set
			{
				if (_alpha != value)
				{
					_alpha = value;

					if (vertCount > 0)
					{
						int count = this.colors.Length;
						for (int i = 0; i < count; i++)
						{
							Color32 col = this.colors[i];
							col.a = (byte)(_alpha * (_alphaBackup != null ? _alphaBackup[i] : (byte)255));
							this.colors[i] = col;
						}
						mesh.colors32 = this.colors;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="verts"></param>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public static void FillVertsOfQuad(Vector3[] verts, int index, Rect rect)
		{
			verts[index] = new Vector3(rect.xMin, -rect.yMax, 0f);
			verts[index + 1] = new Vector3(rect.xMin, -rect.yMin, 0f);
			verts[index + 2] = new Vector3(rect.xMax, -rect.yMin, 0f);
			verts[index + 3] = new Vector3(rect.xMax, -rect.yMax, 0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public static void FillUVOfQuad(Vector2[] uv, int index, Rect rect)
		{
			uv[index] = new Vector2(rect.xMin, rect.yMin);
			uv[index + 1] = new Vector2(rect.xMin, rect.yMax);
			uv[index + 2] = new Vector2(rect.xMax, rect.yMax);
			uv[index + 3] = new Vector2(rect.xMax, rect.yMin);
		}
	}

	class StencilEraser
	{
		public GameObject gameObject;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;

		public StencilEraser(Transform parent)
		{
			gameObject = new GameObject("Eraser");
			ToolSet.SetParent(gameObject.transform, parent);
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_5
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
