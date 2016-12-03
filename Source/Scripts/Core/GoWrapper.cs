using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GoWrapper is class for wrapping common gameobject into UI display list.
	/// </summary>
	public class GoWrapper : DisplayObject
	{
		protected GameObject _wrapTarget;
		protected Renderer[] _renders;

		/// <summary>
		/// 
		/// </summary>
		public GoWrapper()
		{
			this._skipInFairyBatching = true;
			CreateGameObject("GoWrapper");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="go"></param>
		public GoWrapper(GameObject go)
		{
			this._skipInFairyBatching = true;
			CreateGameObject("GoWrapper");

			this.wrapTarget = go;
		}

		/// <summary>
		/// 设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
		/// </summary>
		public GameObject wrapTarget
		{
			get
			{
				return _wrapTarget;
			}

			set
			{
				if (_wrapTarget != null)
					_wrapTarget.transform.parent = null;

				_wrapTarget = value;
				if (_wrapTarget != null)
				{
					ToolSet.SetParent(_wrapTarget.transform, this.cachedTransform);
					CacheRenderers();

					Transform[] transforms = _wrapTarget.GetComponentsInChildren<Transform>(true);
					int lv = this.layer;
					foreach (Transform t in transforms)
					{
						t.gameObject.layer = lv;
					}
				}
				else
				{
					_renders = null;
					_wrapTarget = null;
				}
			}
		}

		/// <summary>
		/// GoWrapper will cache all renderers of your gameobject on constructor. 
		/// If your gameobject change laterly, call this function to update the cache.
		/// GoWrapper会在构造函数里查询你的gameobject所有的Renderer并保存。如果你的gameobject
		/// 后续发生了改变，调用这个函数通知GoWrapper重新查询和保存。
		/// </summary>
		public void CacheRenderers()
		{
			_renders = _wrapTarget.GetComponentsInChildren<Renderer>(true);
			int cnt = _renders.Length;
			for (int i = 0; i < cnt; i++)
			{
				Renderer r = _renders[i];
				if ((r is SkinnedMeshRenderer) || (r is MeshRenderer))
				{
					//Set the object rendering in Transparent Queue as UI objects
					if (r.sharedMaterial != null)
						r.sharedMaterial.renderQueue = 3000;
				}
			}
		}

		public override int renderingOrder
		{
			get
			{
				return base.renderingOrder;
			}
			set
			{
				base.renderingOrder = value;
				if (_renders != null)
				{
					int cnt = _renders.Length;
					for (int i = 0; i < cnt; i++)
					{
						Renderer r = _renders[i];
						if (r != null)
							r.sortingOrder = value;
					}
				}
			}
		}

		public override int layer
		{
			get
			{
				return base.layer;
			}
			set
			{
				base.layer = value;

				if (_renders != null)
				{
					Transform[] transforms = _wrapTarget.GetComponentsInChildren<Transform>(true);
					foreach (Transform t in transforms)
					{
						t.gameObject.layer = value;
					}
				}
			}
		}

		public override void Dispose()
		{
			if (_wrapTarget != null)
			{
				Object.Destroy(_wrapTarget);
				_wrapTarget = null;
			}

			base.Dispose();
		}
	}
}