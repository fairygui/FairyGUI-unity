﻿using UnityEngine;
using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// GoWrapper is class for wrapping common gameobject into UI display list.
    /// </summary>
    public class GoWrapper : DisplayObject
    {
        /// <summary>
        /// 被包装对象的材质是否支持Stencil。如果支持，则会自动设置这些材质的stecnil相关参数，从而实现对包装对象的遮罩
        /// </summary>
        public bool supportStencil;

        protected GameObject _wrapTarget;
        protected List<Renderer> _renderers;
        protected List<Material> _materialsBackup;
        protected List<Material> _materials;
        protected List<int> _sortingOrders;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        protected Canvas _canvas;
#endif
        protected bool _cloneMaterial;

        /// <summary>
        /// 
        /// </summary>
        public GoWrapper()
        {
            _skipInFairyBatching = true;

            _materialsBackup = new List<Material>();
            _materials = new List<Material>();
            _renderers = new List<Renderer>();
            _sortingOrders = new List<int>();
            _cloneMaterial = false;

            CreateGameObject("GoWrapper");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go">包装对象。</param>
        public GoWrapper(GameObject go) : this()
        {
            SetWrapTarget(go, false);
        }

        /// <summary>
        /// 设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// 对象包含的所有材质不会被复制，如果材质已经是公用的，这可能影响到其他对象。如果希望自动复制，改为使用SetWrapTarget(target, true)设置。
        /// </summary>
        public GameObject wrapTarget
        {
            get { return _wrapTarget; }
            set { SetWrapTarget(value, false); }
        }

        [Obsolete("setWrapTarget is deprecated. Use SetWrapTarget instead.")]
        public void setWrapTarget(GameObject target, bool cloneMaterial)
        {
            SetWrapTarget(target, cloneMaterial);
        }

        /// <summary>
        ///  设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cloneMaterial">如果true，则复制材质，否则直接使用sharedMaterial。</param>
        public void SetWrapTarget(GameObject target, bool cloneMaterial)
        {
            RecoverMaterials();

            _cloneMaterial = cloneMaterial;
            if (_wrapTarget != null)
                ToolSet.SetParent(_wrapTarget.transform, null);

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            _canvas = null;
#endif
            _wrapTarget = target;
            _renderers.Clear();
            _sortingOrders.Clear();
            _materials.Clear();

            if (_wrapTarget != null)
            {
                ToolSet.SetParent(_wrapTarget.transform, this.cachedTransform);
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                _canvas = _wrapTarget.GetComponent<Canvas>();
                if (_canvas != null)
                {
                    _canvas.renderMode = RenderMode.WorldSpace;
                    _canvas.worldCamera = StageCamera.main;
                    _canvas.overrideSorting = true;

                    RectTransform rt = _canvas.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.position = new Vector3(0, 0, 0);
                    this.SetSize(rt.rect.width, rt.rect.height);
                }
                else
#endif
                {
                    CacheRenderers();
                    this.SetSize(0, 0);
                }

                Transform[] transforms = _wrapTarget.GetComponentsInChildren<Transform>(true);
                int lv = this.layer;
                foreach (Transform t in transforms)
                {
                    t.gameObject.layer = lv;
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
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            if (_canvas != null)
                return;
#endif
            RecoverMaterials();

            _renderers.Clear();
            _sortingOrders.Clear();
            _materials.Clear();

            _wrapTarget.GetComponentsInChildren<Renderer>(true, _renderers);

            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                Renderer r = _renderers[i];
                if (r == null)
                    continue;

                bool shouldSetRenderQueue = (r is SkinnedMeshRenderer) || (r is MeshRenderer);

                Material[] mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                    continue;

                int mcnt = mats.Length;
                for (int j = 0; j < mcnt; j++)
                {
                    Material mat = mats[j];
                    if (mat == null)
                        continue;

                    //确保相同的材质不会复制两次
                    int k = _materialsBackup.IndexOf(mat);
                    if (k == -1) //未备份
                    {
                        _materialsBackup.Add(mat);
                        if (_cloneMaterial)
                        {
                            mat = new Material(mat);
                            mats[j] = mat;
                            _materials.Add(mat); //保存新创建的材质
                        }
                        else
                            _materials.Add(mat); //直接使用已有材质
                    }
                    else if (_cloneMaterial)
                    {
                        mat = _materials[k];
                        mats[j] = mat;
                    }

                    if (shouldSetRenderQueue) //Set the object rendering in Transparent Queue as UI objects
                        mat.renderQueue = 3000;
                }

                if (_cloneMaterial)
                    r.sharedMaterials = mats;
            }

            if (!_cloneMaterial)
                _materialsBackup.Clear();

            _renderers.Sort(CompareSortingOrder);
            _sortingOrders.Capacity = cnt;
            for (int i = 0; i < cnt; i++)
                _sortingOrders.Add(_renderers[i].sortingOrder);
        }

        static int CompareSortingOrder(Renderer c1, Renderer c2)
        {
            return c1.sortingOrder - c2.sortingOrder;
        }

        void RecoverMaterials()
        {
            if (_materialsBackup.Count == 0)
                return;

            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                Renderer r = _renderers[i];
                if (r == null)
                    continue;

                Material[] mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                    continue;

                int mcnt = mats.Length;
                for (int j = 0; j < mcnt; j++)
                {
                    Material mat = mats[j];

                    int k = _materials.IndexOf(mat);
                    if (k != -1)
                        mats[j] = _materialsBackup[k];
                }

                r.sharedMaterials = mats;
            }

            cnt = _materials.Count;
            for (int i = 0; i < cnt; i++)
                Material.DestroyImmediate(_materials[i]);

            _materialsBackup.Clear();
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

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                if (_canvas != null)
                    _canvas.sortingOrder = value;
#endif
                int cnt = _renderers.Count;
                for (int i = 0; i < cnt; i++)
                {
                    Renderer r = _renderers[i];
                    if (r != null)
                    {
                        if (i != 0 && _sortingOrders[i] != _sortingOrders[i - 1])
                            value = UpdateContext.current.renderingOrder++;
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

                if (_wrapTarget)
                {
                    Transform[] transforms = _wrapTarget.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in transforms)
                    {
                        t.gameObject.layer = value;
                    }
                }
            }
        }

        override public void Update(UpdateContext context)
        {
            if (supportStencil)
            {
                int cnt = _materials.Count;
                for (int i = 0; i < cnt; i++)
                {
                    Material mat = _materials[i];
                    if (mat != null)
                    {
                        if (context.clipped && context.stencilReferenceValue > 0)
                        {
                            mat.SetFloat(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                            mat.SetFloat(ShaderConfig._properyIDs._Stencil, context.stencilCompareValue);
                            mat.SetFloat(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                            mat.SetFloat(ShaderConfig._properyIDs._StencilReadMask, context.stencilReferenceValue | (context.stencilReferenceValue - 1));
                            mat.SetFloat(ShaderConfig._properyIDs._ColorMask, 15);
                        }
                        else
                        {
                            mat.SetFloat(ShaderConfig._properyIDs._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
                            mat.SetFloat(ShaderConfig._properyIDs._Stencil, 0);
                            mat.SetFloat(ShaderConfig._properyIDs._StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                            mat.SetFloat(ShaderConfig._properyIDs._StencilReadMask, 255);
                            mat.SetFloat(ShaderConfig._properyIDs._ColorMask, 15);
                        }
                    }
                }
            }

            base.Update(context);
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            if (_wrapTarget != null)
            {
                UnityEngine.Object.Destroy(_wrapTarget);
                _wrapTarget = null;

                if (_materialsBackup.Count > 0)
                { //如果有备份，说明材质是复制出来的，应该删除
                    int cnt = _materials.Count;
                    for (int i = 0; i < cnt; i++)
                        Material.DestroyImmediate(_materials[i]);
                }
            }

            _renderers = null;
            _materials = null;
            _materialsBackup = null;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            _canvas = null;
#endif
            base.Dispose();
        }
    }
}