using System.Collections;
using UnityEngine;
using FairyGUI;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif

/// <summary>
/// Demonstrated how to load UI package from assetbundle. The bundle can be build from the Window Menu->Build FairyGUI example bundles.
/// </summary>
class BundleUsageMain : MonoBehaviour
{
    GComponent _mainView;

    void Start()
    {
        Application.targetFrameRate = 60;

        Stage.inst.onKeyDown.Add(OnKeyDown);

        StartCoroutine(LoadUIPackage());
    }

    IEnumerator LoadUIPackage()
    {
        string url = Application.streamingAssetsPath.Replace("\\", "/") + "/fairygui-examples/bundleusage.ab";
        if (Application.platform != RuntimePlatform.Android)
            url = "file:///" + url;

#if UNITY_2017_2_OR_NEWER
#if UNITY_2018_1_OR_NEWER
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
        UnityWebRequest www = UnityWebRequest.GetAssetBundle(url);
#endif
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
#else
        WWW www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            AssetBundle bundle = www.assetBundle;
#endif
            if (bundle == null)
            {
                Debug.LogWarning("Run Window->Build FairyGUI example Bundles first.");
                yield return 0;
            }
            UIPackage.AddPackage(bundle);

            _mainView = UIPackage.CreateObject("BundleUsage", "Main").asCom;
            _mainView.fairyBatching = true;
            _mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
            _mainView.AddRelation(GRoot.inst, RelationType.Size);

            GRoot.inst.AddChild(_mainView);
            _mainView.GetTransition("t0").Play();
        }
        else
            Debug.LogError(www.error);
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}
