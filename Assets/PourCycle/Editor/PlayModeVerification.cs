using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace PourCycle.Editor {
 // Drives the same controller entry points as the buttons; only runs by explicit menu request.
 [InitializeOnLoad]
 public static class PlayModeVerification {
  static int step; static double next,clearCapture,releaseCapture; static GameController game; static string folder;static string captureName;static float savedPatience;
  static PlayModeVerification(){EditorApplication.playModeStateChanged+=Changed;}
  [MenuItem("POUR CYCLE/Verify Preview04 in Play Mode")]
  public static void Begin(){if(EditorApplication.isPlaying)return;SessionState.SetBool("PourCycle.Verify",true);EditorApplication.EnterPlaymode();}
  static void Changed(PlayModeStateChange state){
   if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("PourCycle.Verify",false)){
    SessionState.SetBool("PourCycle.Verify",false);step=0;next=EditorApplication.timeSinceStartup+2;
    folder=Path.GetFullPath("../../work/playmode-captures");Directory.CreateDirectory(folder);
    EditorApplication.update+=Tick;
   }
   if(state==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Tick;
  }
  static void Select(Ingredient ingredient){int limit=0;while(game.Selection!=ingredient && limit++<6)game.SelectWithModifier(false);}
  static void Check(bool condition,string message){if(!condition)throw new Exception("POUR_CYCLE_PLAYMODE_FAIL: "+message);}
  static void Capture(string name){ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));}
  static void Tick(){
   if(releaseCapture>0 && EditorApplication.timeSinceStartup>=releaseCapture){Capture("03-release-flight");releaseCapture=0;}
   if(clearCapture>0 && EditorApplication.timeSinceStartup>=clearCapture){Capture(captureName);clearCapture=0;}
   if(EditorApplication.timeSinceStartup<next)return;
   try{
    if(game==null)game=UnityEngine.Object.FindObjectOfType<GameController>();if(game==null)return;
    if(game.IsBusy)return;
    switch(step++){
     case 0:game.StartSession(true);Capture("01-start");break;
     case 1:Select(Ingredient.RED);game.PourWithModifier(false);break;
     case 2:Check(game.Rules.Buffer.Count==1,"buffer retained");Select(Ingredient.BLUE);game.PourWithModifier(false);break;
     case 3:Select(Ingredient.LIME);game.PourWithModifier(false);break;
     case 4:Check(game.Rules.Orders.Slots[2].Id==4,"order 4 replacement");Select(Ingredient.FOAM);game.PourWithModifier(true);break;
     case 5:Select(Ingredient.BLUE);game.PourWithModifier(false);break;
     case 6:Select(Ingredient.FOAM);game.PourWithModifier(true);break;
     case 7:Check(game.Rules.Buffer.Count==3,"hold 3");Capture("02-ready");break;
     case 8:game.PourWithModifier(false);releaseCapture=EditorApplication.timeSinceStartup+.12;Check(game.Session.AllClears==1 && game.Session.Cleared==4,"all clear and count");clearCapture=EditorApplication.timeSinceStartup+.62;captureName="03-triple-flow";break;
     case 9:Check(game.Session.FlowScore==550 && game.Session.BestFlow==450 && game.Rules.Buffer.Count==0,"FLOW statistics");game.Session.Demo=false;game.Session.Remaining=1;break;
     case 10:Check(game.Session.Ended,"session expiration");var selected=game.Selection;game.SelectWithModifier(false);game.PourWithModifier(true);Check(game.Selection==selected && game.Rules.Buffer.Count==0,"result blocks inputs");Capture("04-results");break;
     case 11:game.StartSession(true);game.Rules.Orders.Slots[0]=new OrderData(0,Ingredient.CUP,Ingredient.BLUE,Ingredient.RED);Select(Ingredient.CUP);Capture("05-cup-needed");break;
     case 12:game.PourWithModifier(false);break;
     case 13:Capture("06-empty-cup");Select(Ingredient.BLUE);game.PourWithModifier(false);break;
     case 14:Capture("07-blue-layer");Select(Ingredient.RED);game.PourWithModifier(false);clearCapture=EditorApplication.timeSinceStartup+.62;captureName="08-stacked-layers";break;
     case 15:savedPatience=game.Levels[0].OrderPatience;game.Levels[0].OrderPatience=1;game.StartSession(false);break;
     case 16:Check(game.Rules.Orders.Slots[0].Id>3,"runtime patience replacement");Capture("09-patience");break;
     default:Debug.Log("POUR_CYCLE_PLAYMODE_PASS: Preview04, FLOW 550, best 450, stacked cups, CUP gating, expiry, result input lock");game.Levels[0].OrderPatience=savedPatience;game.SessionSeconds=60;game.StartSession(true);EditorApplication.update-=Tick;break;
    }
    next=EditorApplication.timeSinceStartup+(step==10||step==16?1.4:.65);
   }catch(Exception e){Debug.LogException(e);EditorApplication.update-=Tick;}
  }
 }
}


