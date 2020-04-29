#if FAIRYGUI_DRAGONBONES

using UnityEngine;
using DragonBones;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GLoader3D : GObject
    {
        UnityArmatureComponent _armatureComponent;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public UnityArmatureComponent armatureComponent
        {
            get { return _armatureComponent; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="anchor"></param>
        public void SetDragonBones(DragonBonesData asset, int width, int height, Vector2 anchor)
        {
            if (_armatureComponent != null)
                FreeDragonBones();

            _armatureComponent = UnityFactory.factory.BuildArmatureComponent(asset.armatureNames[0], asset.name, null, asset.name);
            _armatureComponent.gameObject.transform.localScale = new Vector3(100, 100, 1);
            _armatureComponent.gameObject.transform.localPosition = new Vector3(anchor.x, -anchor.y, 0);
            SetWrapTarget(_armatureComponent.gameObject, true, width, height);

            OnChangeDragonBones(null);
        }

        protected void LoadDragonBones()
        {
            DragonBonesData asset = (DragonBonesData)_contentItem.skeletonAsset;
            if (asset == null)
                return;

            SetDragonBones(asset, _contentItem.width, _contentItem.height, _contentItem.skeletonAnchor);
        }

        protected void OnChangeDragonBones(string propertyName)
        {
            if (_armatureComponent == null)
                return;

            if (!string.IsNullOrEmpty(_animationName))
            {
                if (_playing)
                    _armatureComponent.animation.Play(_animationName, _loop ? 0 : 1);
                else
                    _armatureComponent.animation.GotoAndStopByFrame(_animationName, (uint)_frame);
            }
            else
                _armatureComponent.animation.Reset();
        }

        protected void FreeDragonBones()
        {
            if (_armatureComponent != null)
            {
                _armatureComponent.Dispose();
                if (Application.isPlaying)
                    GameObject.Destroy(_armatureComponent.gameObject);
                else
                    GameObject.DestroyImmediate(_armatureComponent.gameObject);
            }
        }
    }
}

#endif