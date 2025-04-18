#if UNITY_EDITOR
using NWH.Common.AssetInfo;
using UnityEditor;

namespace NWH.DWP2
{
    public class InitializationMethodsDWP2 : CommonInitializationMethods
    {
        [InitializeOnLoadMethod]
        static void AddDWP2Defines()
        {
            AddDefines("NWH_DWP2");
        }

        [InitializeOnLoadMethod]
        static void ShowDWP2WelcomeWindow()
        {
            ShowWelcomeWindow("Dynamic Water Physics 2");
        }
    }
}
#endif