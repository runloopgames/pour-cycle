using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.IO;
namespace PourCycle.Editor {
 [InitializeOnLoad]
 public static class PrototypeSetup {
  static PrototypeSetup(){EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode){EnsureResources();if(!Application.isBatchMode && !File.Exists("Assets/PourCycle/Scenes/PourCycle.unity"))Setup();}};}
  static void EnsureResources(){
   if(File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))return;
   var package=UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.textmeshpro");
   if(package!=null)AssetDatabase.ImportPackage(Path.Combine(package.resolvedPath,"Package Resources/TMP Essential Resources.unitypackage"),false);
  }
  [MenuItem("POUR CYCLE/Build scene and verify")]
  public static void Setup() {
   EnsureResources();
   Verify();
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var camera=new GameObject("Camera").AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.96f,.95f,.91f);camera.gameObject.AddComponent<AudioListener>();
   var game=new GameObject("POUR CYCLE");var tilt=game.AddComponent<TiltInputController>();game.AddComponent<GameController>().Tilt=tilt;
   Directory.CreateDirectory("Assets/PourCycle/Scenes");
   EditorSceneManager.SaveScene(scene,"Assets/PourCycle/Scenes/PourCycle.unity");
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/PourCycle/Scenes/PourCycle.unity",true)};
   PlayerSettings.companyName="Prototype Lab";PlayerSettings.productName="POUR CYCLE";
   PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;
   PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.runloopgames.pourcycle");
   PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,"com.runloopgames.pourcycle");
   // Runtime-generated TMP text needs its shader preserved in player builds.
   var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
   var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");
   var shader=Shader.Find("TextMeshPro/Distance Field");
   if(shader!=null){bool found=false;for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;
    if(!found){shaders.InsertArrayElementAtIndex(shaders.arraySize);shaders.GetArrayElementAtIndex(shaders.arraySize-1).objectReferenceValue=shader;graphics.ApplyModifiedProperties();}}
   AssetDatabase.SaveAssets(); Debug.Log("POUR_CYCLE_SETUP_OK");
  }
  [MenuItem("POUR CYCLE/Run core checks")]
  public static void Verify(){Debug.Log("POUR_CYCLE_TESTS_PASS: "+CoreChecks.Run());}
  [MenuItem("POUR CYCLE/Build Windows player")]
  public static void BuildWindows(){
   if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode before building.");
   Verify();Directory.CreateDirectory("../PourCycleWindows");PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/PourCycle/Scenes/PourCycle.unity"},locationPathName="../PourCycleWindows/PourCycle.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Player build failed: "+report.summary.result);
   Debug.Log("POUR_CYCLE_WINDOWS_BUILD_PASS");
  }
 }
}

