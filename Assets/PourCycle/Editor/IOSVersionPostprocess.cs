#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace PourCycle.Editor {
 public static class IOSVersionPostprocess {
  public const string MarketingVersion="2.1.0";

  [PostProcessBuild(100)]
  public static void Apply(BuildTarget target,string path){
   if(target!=BuildTarget.iOS)return;

   string plistPath=Path.Combine(path,"Info.plist");
   var plist=new PlistDocument();plist.ReadFromFile(plistPath);
   plist.root.SetString("CFBundleShortVersionString",MarketingVersion);
   plist.WriteToFile(plistPath);

   string projectPath=PBXProject.GetPBXProjectPath(path);
   var project=new PBXProject();project.ReadFromFile(projectPath);
   project.SetBuildProperty(project.GetUnityMainTargetGuid(),"MARKETING_VERSION",MarketingVersion);
   project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(),"MARKETING_VERSION",MarketingVersion);
   project.WriteToFile(projectPath);
  }
 }
}
#endif
