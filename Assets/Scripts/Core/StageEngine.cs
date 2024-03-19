using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class StageEngine : MonoBehaviour
    {
        public int ObjectsOnStage;
        public int GraphicsOnStage;

        public static bool beingQuit;

        void Start()
        {
            useGUILayout = false;
        }

        void LateUpdate()
        {
            Stage.inst.InternalUpdate();

            ObjectsOnStage = Stats.ObjectCount;
            GraphicsOnStage = Stats.GraphicsCount;
        }

        void OnGUI()
        {
            Stage.inst.HandleGUIEvents(Event.current);
        }

        void OnApplicationQuit()
        {
            if (Application.isEditor)
            {
                beingQuit = true;
                UIPackage.RemoveAllPackages();
                Stage.inst.Dispose();
            }
        }

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitializeOnLoad()
        {
            beingQuit = false;
        }
#endif
    }
}