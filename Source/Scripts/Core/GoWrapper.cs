using UnityEngine;
using FairyGUI;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GoWrapper is class for wrapping common gameobject into UI display list.
	/// </summary>
	public class GoWrapper : DisplayObject
	{
		protected GameObject _owner;
		protected Renderer[] _renders;

		public GoWrapper(GameObject go)
		{
			_owner = go;
			this._skipInFairyBatching = true;
			CreateGameObject("GoWrapper");
			ToolSet.SetParent(_owner.transform, this.cachedTransform);
			CacheRenderers();
		}

		/// <summary>
		/// GoWrapper will cache all renderers of your gameobject on constructor. 
		/// If your gameobject change laterly, call this function to update the cache.
		/// GoWrapper会在构造函数里查询你的gameobject所有的Renderer并保存。如果你的gameobject
		/// 后续发生了改变，调用这个函数通知GoWrapper重新查询和保存。
		/// </summary>
		public void CacheRenderers()
		{
			_renders = _owner.GetComponentsInChildren<Renderer>(true);
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
				int cnt = _renders.Length;
				for (int i = 0; i < cnt; i++)
				{
					Renderer r = _renders[i];
					if (r != null)
						r.sortingOrder = value;
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

				Transform[] transforms = _owner.GetComponentsInChildren<Transform>(true);
				foreach (Transform t in transforms)
				{
					t.gameObject.layer = value;
				}
			}
		}

		public override void Dispose()
		{
			if (_owner != null)
				Object.Destroy(_owner);

			base.Dispose();
		}
	}
}