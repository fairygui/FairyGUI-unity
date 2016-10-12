using FairyGUI;
using FairyGUI.Utils;
using System.Collections;
using UnityEngine;

public class RenderImage
{
	public Transform modelRoot { get; private set; }

	Camera _camera;
	Image _image;
	Transform _root;
	Transform _background;
	Transform _model;
	RenderTexture _renderTexture;
	int _width;
	int _height;
	bool _cacheTexture;
	float _rotating;

	const int RENDER_LAYER = 0;
	const int HIDDEN_LAYER = 10;

	public RenderImage(GGraph holder)
	{
		_width = (int)holder.width;
		_height = (int)holder.height;
		_cacheTexture = true;

		this._image = new Image();
		holder.SetNativeObject(this._image);

		Object prefab = Resources.Load("RenderTexture/RenderImageCamera");
		GameObject go = (GameObject)Object.Instantiate(prefab);
		_camera = go.GetComponent<Camera>();
		_camera.transform.position = new Vector3(0, 1000, 0);
		_camera.cullingMask = 1 << RENDER_LAYER;
		_camera.enabled = false;
		Object.DontDestroyOnLoad(_camera.gameObject);

		this._root = new GameObject("RenderImage").transform;
		this._root.SetParent(_camera.transform, false);
		SetLayer(this._root.gameObject, HIDDEN_LAYER);

		this.modelRoot = new GameObject("model_root").transform;
		this.modelRoot.SetParent(this._root, false);

		this._background = new GameObject("background").transform;
		this._background.SetParent(this._root, false);

		this._image.onAddedToStage.Add(OnAddedToStage);
		this._image.onRemovedFromStage.Add(OnRemoveFromStage);

		if (this._image.stage != null)
			OnAddedToStage();
		else
			_camera.gameObject.SetActive(false);
	}

	public void Dispose()
	{
		Object.Destroy(_camera.gameObject);
		DestroyTexture();

		this._image.Dispose();
		this._image = null;
	}

	/// <summary>
	/// The rendertexture is not transparent. So if you want to the UI elements can be seen in the back of the models/particles in rendertexture,
	/// you can set a maximunm two images for background.
	/// Be careful if your image is 9 grid scaling, you must make sure the place holder is inside the middle box(dont cover from border to middle).
	/// </summary>
	/// <param name="image"></param>
	public void SetBackground(GObject image)
	{
		SetBackground(image, null);
	}

	/// <summary>
	/// The rendertexture is not transparent. So if you want to the UI elements can be seen in the back of the models/particles in rendertexture,
	/// you can set a maximunm two images for background.
	/// </summary>
	/// <param name="image1"></param>
	/// <param name="image2"></param>
	public void SetBackground(GObject image1, GObject image2)
	{
		Image source1 = (Image)image1.displayObject;
		Image source2 = image2 != null ? (Image)image2.displayObject : null;

		Vector3 pos = this._background.position;
		pos.z = _camera.farClipPlane;
		this._background.position = pos;

		Mesh mesh = new Mesh();
		Rect rect = this._image.TransformRect(new Rect(0, 0, this._width, this._height), source1);
		source1.PrintTo(mesh, rect);

		Vector2[] tmp = mesh.uv;
		if (source2 != null)
		{
			rect = this._image.TransformRect(new Rect(0, 0, this._width, this._height), source2);
			source2.PrintTo(mesh, rect);

#if UNITY_5
			mesh.uv2 = mesh.uv;
#else
			mesh.uv1 = mesh.uv;
#endif
			mesh.uv = tmp;
		}

		Vector2[] tmp2 = new Vector2[tmp.Length];
		FairyGUI.Utils.ToolSet.uvLerp(tmp, tmp2, 0, 1);

		int cnt = tmp2.Length;
		Vector3[] verts = mesh.vertices;
		for (int i = 0; i < cnt; i++)
		{
			Vector2 v2 = tmp2[i];
			verts[i] = new Vector3(v2.x * 2 - 1, v2.y * 2 - 1);
		}
		mesh.vertices = verts;

		MeshFilter meshFilter = this._background.gameObject.GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = this._background.gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		MeshRenderer meshRenderer = this._background.gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
			meshRenderer = this._background.gameObject.AddComponent<MeshRenderer>();
#if UNITY_5
		meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
		meshRenderer.castShadows = false;
#endif
		meshRenderer.receiveShadows = false;
		Shader shader = Shader.Find("Game/FullScreen");
		Material mat = new Material(shader);
		mat.mainTexture = source1.texture.nativeTexture;
		if (source2 != null)
			mat.SetTexture("_Tex2", source2.texture.nativeTexture);
		meshRenderer.material = mat;
	}

	public void LoadModel(string model)
	{
		this.UnloadModel();

		Object prefab = Resources.Load(model);
		GameObject go = ((GameObject)Object.Instantiate(prefab));
		_model = go.transform;
		_model.SetParent(this.modelRoot, false);
	}

	public void UnloadModel()
	{
		if (_model != null)
		{
			Object.Destroy(_model.gameObject);
			_model = null;
		}
		_rotating = 0;
	}

	public void StartRotate(float delta)
	{
		_rotating = delta;
	}

	public void StopRotate()
	{
		_rotating = 0;
	}

	void CreateTexture()
	{
		if (_renderTexture != null)
			return;

		_renderTexture = new RenderTexture(_width, _height, 24, RenderTextureFormat.ARGB32)
		{
			antiAliasing = 1,
			filterMode = FilterMode.Bilinear,
			anisoLevel = 0,
			useMipMap = false
		};
		this._image.texture = new NTexture(_renderTexture);
		this._image.blendMode = BlendMode.Off;
	}

	void DestroyTexture()
	{
		if (_renderTexture != null)
		{
			Object.Destroy(_renderTexture);
			_renderTexture = null;
			this._image.texture = null;
		}
	}

	void OnAddedToStage()
	{
		if (_renderTexture == null)
			CreateTexture();

		Timers.inst.AddUpdate(this.Render);
		_camera.gameObject.SetActive(true);

		Render();
	}

	void OnRemoveFromStage()
	{
		if (!_cacheTexture)
			DestroyTexture();

		Timers.inst.Remove(this.Render);
		_camera.gameObject.SetActive(false);
	}

	void Render(object param = null)
	{
		if (_rotating != 0 && this.modelRoot != null)
		{
			Vector3 localRotation = this.modelRoot.localRotation.eulerAngles;
			localRotation.y += _rotating;
			this.modelRoot.localRotation = Quaternion.Euler(localRotation);
		}

		SetLayer(this._root.gameObject, RENDER_LAYER);

		_camera.targetTexture = this._renderTexture;
		RenderTexture old = RenderTexture.active;
		RenderTexture.active = this._renderTexture;
		GL.Clear(true, true, Color.clear);
		_camera.Render();
		RenderTexture.active = old;

		SetLayer(this._root.gameObject, HIDDEN_LAYER);
	}

	void SetLayer(GameObject go, int layer)
	{
		Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in transforms)
		{
			t.gameObject.layer = layer;
		}
	}
}