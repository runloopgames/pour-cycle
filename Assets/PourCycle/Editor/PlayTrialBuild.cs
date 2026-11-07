using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace PourCycle.Editor {
 public sealed class PlayTrialBuild : IPreprocessBuildWithReport {
  public int callbackOrder=>0;
  public void OnPreprocessBuild(BuildReport report){
   if(report.summary.platform!=BuildTarget.Android)return;
   if((int)PlayerSettings.Android.minSdkVersion<24)throw new BuildFailedException("Play Trials AAR requires Android API 24 or newer.");
   if(!File.Exists("Assets/Plugins/Android/playcore_trials_proguarded.aar"))throw new BuildFailedException("Google Play Trials AAR is missing.");
   if(!File.Exists("Assets/Plugins/Android/proguard-user.txt"))throw new BuildFailedException("Play Trials JNI keep rules are missing.");
   const string manifest="Assets/Plugins/Android/AndroidManifest.xml";
   if(!File.Exists(manifest)||!File.ReadAllText(manifest).Contains("com.google.android.play.trials.PROGRESSION_TRIAL_ENABLED"))throw new BuildFailedException("Progression Trial manifest property is missing.");
  }
  [MenuItem("POUR CYCLE/Build Google Play AAB")]
  public static void BuildAab(){
   bool previous=EditorUserBuildSettings.buildAppBundle;
   try{
    EditorUserBuildSettings.buildAppBundle=true;Directory.CreateDirectory("Builds/Android");
    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/PourCycle/Scenes/PourCycle.unity"},target=BuildTarget.Android,locationPathName="Builds/Android/PourCycle.aab",options=BuildOptions.None});
    if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("AAB build failed: "+report.summary.result);
   }finally{EditorUserBuildSettings.buildAppBundle=previous;}
  }
 }
}
