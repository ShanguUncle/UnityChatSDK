using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
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


#if UNITY_WEBGL
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.WebGL)
        {
            InjectMicrophoneScriptsForWebGL(pathToBuiltProject);
        }
    }
    private static void InjectMicrophoneScriptsForWebGL(string rootPath)
    {
        string pathToMicBridge = AssetDatabase.FindAssets("microphone-bridge")[0];
        string pathToSourceNativePlugins = new FileInfo(AssetDatabase.GUIDToAssetPath(pathToMicBridge)).DirectoryName + "/Native";
        string pathToExportNativePlugins = rootPath + "/Native";

        if (!Directory.Exists(pathToExportNativePlugins))
            Directory.CreateDirectory(pathToExportNativePlugins);

        //List<string> nativeFiles = new List<string>();

        FileInfo fileInfo;
        foreach (string file in Directory.GetFiles(pathToSourceNativePlugins))
        {
            if (file.EndsWith(".meta"))
                continue;

            fileInfo = new FileInfo(file);

            File.WriteAllText(pathToExportNativePlugins + "/" + fileInfo.Name, MinifyContent(File.ReadAllText(file)));

            //nativeFiles.Add(fileInfo.Name);
        }

        string indexHTML = rootPath + "/index.html";

        if (File.Exists(indexHTML))
        {
            string[] indexTextLines = File.ReadAllLines(indexHTML);
            List<string> exportLines = new List<string>();

            for (int i = 0; i < indexTextLines.Length; i++)
            {
                exportLines.Add(indexTextLines[i]);

                if (indexTextLines[i].Contains("<head>"))
                {
                    exportLines.Add("    <script type=\"text/javascript\" src=\"./Native/unity-webgl-tools.js\"></script>");
                    exportLines.Add("    <script type=\"text/javascript\" src=\"./Native/microphone.js\"></script>");
                }
            }

            File.WriteAllLines(indexHTML, exportLines);
        }
        else
        {
            throw new System.Exception("File index.html not found! Cannot proceed with adding connection to library.");
        }
    }
    private static string MinifyContent(string content)
    {
        // todo implement minifier
        return content;
    }
#endif

}