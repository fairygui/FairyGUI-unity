using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("Window/Build FairyGUI Example Bundles")]
    public static void Builde()
    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        for (int i = 0; i < 10; i++)
        {
            AssetImporter.GetAtPath("Assets/FairyGUI/Examples/Resources/Icons/i" + i + ".png").assetBundleName = "fairygui-examples/i" + i + ".ab";
        }

        AssetImporter.GetAtPath("Assets/FairyGUI/Examples/Resources/UI/BundleUsage_fui.bytes").assetBundleName = "fairygui-examples/bundleusage.ab";
        AssetImporter.GetAtPath("Assets/FairyGUI/Examples/Resources/UI/BundleUsage_atlas0.png").assetBundleName = "fairygui-examples/bundleusage.ab";

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.Android);
#else
        for (int i = 0; i < 10; i++)
        {
            Object obj = AssetDatabase.LoadAssetAtPath("Assets/FairyGUI/Examples/Resources/Icons/i"+i+".png", typeof(Object));
            BuildPipeline.BuildAssetBundle(obj, null, Path.Combine(Application.streamingAssetsPath, "fairygui-examples/i" + i + ".ab"), 
                BuildAssetBundleOptions.CollectDependencies, BuildTarget.Android);
        }

        Object mainAsset = AssetDatabase.LoadAssetAtPath("Assets/FairyGUI/Examples/Resources/UI/BundleUsage_fui.bytes", typeof(Object));
        Object[] assets = new Object[] { 
            AssetDatabase.LoadAssetAtPath("Assets/FairyGUI/Examples/Resources/UI/BundleUsage_atlas0.png", typeof(Object))
        };

        BuildPipeline.BuildAssetBundle(mainAsset, assets, Path.Combine(Application.streamingAssetsPath, "fairygui-examples/bundleusage.ab"), 
            BuildAssetBundleOptions.CollectDependencies, BuildTarget.Android);
        AssetDatabase.Refresh();
#endif
    }
}