using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class UnityChatBuild 
{
#if UNITY_IOS
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {

        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string filePath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(filePath);

            //GetUnityMainTargetGuid get app Guid,GetUnityFrameworkTargetGuid() get plugins Guid
            string guid = project.GetUnityFrameworkTargetGuid();
#if !UNITY_2019_3_OR_NEWER
      guid = project.TargetGuidByName(project.GetUnityTargetName());
#endif
            project.AddBuildProperty(guid, "OTHER_LDFLAGS", "-lturbojpeg");
            File.WriteAllText(filePath, project.WriteToString());
        }
    }
#endif
}