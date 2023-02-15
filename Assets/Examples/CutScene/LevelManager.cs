using System.Collections;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using FairyGUI;

public class LevelManager : MonoBehaviour
{
    static LevelManager _instance;
    public static LevelManager inst
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("LevelManager");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<LevelManager>();
            }
            return _instance;
        }
    }

    GComponent _cutSceneView;
    GComponent _mainView;

    public LevelManager()
    {
    }

    public void Init()
    {
        _cutSceneView = UIPackage.CreateObject("CutScene", "CutScene").asCom;
        _cutSceneView.SetSize(GRoot.inst.width, GRoot.inst.height);
        _cutSceneView.AddRelation(GRoot.inst, RelationType.Size);

        _mainView = UIPackage.CreateObject("CutScene", "Main").asCom;
        _mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
        _mainView.AddRelation(GRoot.inst, RelationType.Size);

        _mainView.GetChild("n0").onClick.Add(() =>
        {
            LoadLevel("scene1");
        });

        _mainView.GetChild("n1").onClick.Add(() =>
        {
            LoadLevel("scene2");
        });
    }

    public void LoadLevel(string levelName)
    {
        StartCoroutine(DoLoad(levelName));
        GRoot.inst.AddChild(_cutSceneView);
    }

    IEnumerator DoLoad(string sceneName)
    {
        GRoot.inst.AddChild(_cutSceneView);
        GProgressBar pb = _cutSceneView.GetChild("pb").asProgress;
        pb.value = 0;
#if UNITY_5_3_OR_NEWER
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
#else
        AsyncOperation op = Application.LoadLevelAsync(sceneName);
#endif
        float startTime = Time.time;
        while (!op.isDone || pb.value != 100)
        {
            int value = (int)((Time.time - startTime) * 100f / 3f);
            if (value > 100)
                value = 100;
            pb.value = value;
            yield return null;
        }

        GRoot.inst.RemoveChild(_cutSceneView);
        GRoot.inst.AddChild(_mainView);
    }
}
