using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("Window/Build FairyGUI Example Bundles")]
    public static void Build()
    {
        string basePath = "Assets/FairyGUI/Examples/Resources/";
        if (AssetImporter.GetAtPath(basePath + "Icons/i0.png") == null)
            basePath = "Assets/Examples/Resources/";

        for (int i = 0; i < 10; i++)
        {
            AssetImporter.GetAtPath(basePath + "Icons/i" + i + ".png").assetBundleName = "fairygui-examples/i" + i + ".ab";
        }

        AssetImporter.GetAtPath(basePath + "UI/BundleUsage_fui.bytes").assetBundleName = "fairygui-examples/bundleusage.ab";
        AssetImporter.GetAtPath(basePath + "UI/BundleUsage_atlas0.png").assetBundleName = "fairygui-examples/bundleusage.ab";

        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}