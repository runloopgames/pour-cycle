#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Media;
namespace PourCycle.Editor {
 [InitializeOnLoad] public static class CaptureMenu {
  static CaptureMenu(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Pour.Capture",false)){SessionState.SetBool("Pour.Capture",false);new GameObject("Capture verification").AddComponent<CaptureVerification>();}};}
  [MenuItem("POUR CYCLE/Capture carousel verification")]
  static void Run(){if(EditorApplication.isPlaying)return;SessionState.SetBool("Pour.Capture",true);EditorApplication.EnterPlaymode();}
 }
 public sealed class CaptureVerification:MonoBehaviour {
  string folder;GameController game;MediaEncoder encoder;
  IEnumerator Start(){
   try{
    folder=Path.GetFullPath("../Capture");Directory.CreateDirectory(folder);game=FindObjectOfType<GameController>();
    Time.captureFramerate=30;WriteLevelConfigList();
    game.SetHand(false);game.SetDebugMode(false);game.StartLevel(1);yield return Capture("01-Level-1-Start.png");
    yield return ForcedResult(1,true,"02-Level-1-Clear-Result.png");
    game.StartLevel(2);yield return Capture("03-Next-Level-Level-2-Start.png");
    yield return ForcedResult(2,false,"04-Not-Cleared-Result.png");
    yield return ForcedResult(5,true,"05-Level-5-Complete.png");
    game.StartPractice();yield return Capture("06-Practice-No-Timer.png");
    yield return Clip("07-Select-forward",false,180);
    yield return Clip("08-Tilt-Select-back",true,150);
    Debug.Log("POUR_CAPTURE_PASS: level start, clear, next level, not cleared, level 5 complete, practice, select/back rotation videos, LevelConfig list");
   }finally{Time.captureFramerate=0;}
  }
  void WriteLevelConfigList(){
   using(var w=new StreamWriter(Path.Combine(folder,"LevelConfig-list.txt"),false)){
    foreach(var l in game.Levels)w.WriteLine("LEVEL {0}: {1}s / Patience {2}s / Target {3} / Recipe {4}-{5} / Weights {6}:{7}:{8}",l.LevelNumber,l.SessionDuration,l.OrderPatience,l.TargetFlow,l.MinRecipeSteps,l.MaxRecipeSteps,l.OneStepWeight,l.TwoStepWeight,l.ThreeStepWeight);
   }
  }
  IEnumerator ForcedResult(int level,bool clear,string name){
   game.StartLevel(level);yield return new WaitForEndOfFrame();
   game.Session.PlayerSales=clear?100:0;game.Session.RivalSales=clear?0:100;game.Session.ResolvedCustomers=game.Session.TotalCustomers;
   game.Session.BestFlow=clear?450:0;game.Session.Cleared=clear?8:0;game.Session.AllClears=clear?1:0;game.Session.Remaining=.01f;
   yield return new WaitForSeconds(.15f);yield return Capture(name);
  }
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return new WaitForEndOfFrame();}
  IEnumerator Clip(string name,bool back,int count){
   game.StartPractice();game.SetDebugMode(false);
   var attributes=new VideoTrackAttributes{frameRate=new MediaRational(30),width=(uint)Screen.width,height=(uint)Screen.height,includeAlpha=false};
   encoder=new MediaEncoder(Path.Combine(folder,name+".mp4"),attributes);
   int presses=0;float previous=game.Carousel.CurrentRotation,original=game.Carousel.TargetRotation;
   for(int f=0;f<count;f++){
    if((f>=12 && f<108 && f%12==0)||(!back && f>=114 && f<134 && f%2==0)){game.SelectWithModifier(back);presses++;}
    yield return new WaitForEndOfFrame();
    float angle=game.Carousel.CurrentRotation;if(back?angle>previous+.001f:angle<previous-.001f)throw new System.Exception("Rotation reversed");previous=angle;
    var frame=ScreenCapture.CaptureScreenshotAsTexture();encoder.AddFrame(frame);if(f==65)File.WriteAllBytes(Path.Combine(folder,name+"-frame.png"),frame.EncodeToPNG());Destroy(frame);
   }
   encoder.Dispose();encoder=null;
   if(Mathf.Abs(game.Carousel.TargetRotation-original-(back?-60:60)*presses)>.01f)throw new System.Exception("Lost select input");
  }
  void OnDestroy(){if(encoder!=null)encoder.Dispose();Time.captureFramerate=0;}
 }
}
#endif
