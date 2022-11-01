using UnityEngine;
using FairyGUI;
using System.IO;

/// <summary>
/// Extend the ability of GLoader
/// </summary>
public class MyGLoader : GLoader
{
    protected override void LoadExternal()
    {
        IconManager.inst.LoadIcon(this.url, OnLoadSuccess, OnLoadFail);
    }

    protected override void FreeExternal(NTexture texture)
    {
        texture.refCount--;
    }

    void OnLoadSuccess(NTexture texture)
    {
        if (string.IsNullOrEmpty(this.url))
            return;

        this.onExternalLoadSuccess(texture);
    }

    void OnLoadFail(string error)
    {
        Debug.Log("load " + this.url + " failed: " + error);
        this.onExternalLoadFailed();
    }
}
