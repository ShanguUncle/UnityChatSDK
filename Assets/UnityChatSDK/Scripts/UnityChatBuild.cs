using System.IO;
using UnityEngine;
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

public class UnityChatBuild 
{
#if UNITY_IOS
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {

        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);

            //string tn = PBXProject.GetUnityTargetName();
#if !UNITY_2019_3_OR_NEWER
            string tn = "Unity-iPhone";
#else
            string tn = "UnityFramework";
#endif
            string g = project.TargetGuidByName(tn);
            project.AddBuildProperty(g, "OTHER_LDFLAGS", "-lturbojpeg");
            File.WriteAllText(sPath, project.WriteToString());
        }
    }
#endif
}