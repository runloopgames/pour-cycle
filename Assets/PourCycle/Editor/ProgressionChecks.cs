using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BigInteger=System.Numerics.BigInteger;
namespace PourCycle.Editor {
 [InitializeOnLoad] public static class ProgressionChecks {
  static int checks;
  sealed class TrialProbe : IPlayTrialService {public PlayTrialStatus Status;public int StopCalls;public PlayTrialStatus CheckStatus()=>Status;public bool TryStopTrial(){StopCalls++;return true;}}
  static ProgressionChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Pour.ProgressionTests",false)){SessionState.SetBool("Pour.ProgressionTests",false);EditorApplication.delayCall+=Verify;}};}
  public static void Run(){SessionState.SetBool("Pour.ProgressionTests",true);EditorApplication.EnterPlaymode();}
  static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;Debug.Log("PASS: "+message);}
  static void Verify(){
   int exit=0;
   try{
    Debug.Log("CORE_CHECKS: "+CoreChecks.Run());
    var g=UnityEngine.Object.FindObjectOfType<GameController>();if(g==null)g=new GameObject("Progression test").AddComponent<GameController>();
    var result=typeof(GameController).GetMethod("ShowResult",BindingFlags.NonPublic|BindingFlags.Instance);
    var buttons=typeof(GameController).GetField("resultButtons",BindingFlags.NonPublic|BindingFlags.Instance);
    g.StartLevel(5);
    for(int n=5;n<=7;n++){
     Check(g.CurrentLevel==n,"start LEVEL "+n);g.Session.PlayerSales=200;g.Session.RivalSales=100;g.Session.ResolvedCustomers=g.Session.TotalCustomers;
     result.Invoke(g,null);
     var bs=(PointerDownButton[])buttons.GetValue(g);Check(bs.Length==2,"normal result has NEXT LEVEL");bs[0].Press();Check(g.CurrentLevel==n+1,"result button transition "+n+" -> "+(n+1));
    }
    g.StartSession(false);Check(g.CurrentLevel==1,"new play always starts LEVEL 1");
    g.StartLevel(8);g.Session.PlayerSales=0;g.Session.RivalSales=100;result.Invoke(g,null);Check(((PointerDownButton[])buttons.GetValue(g)).Length==1,"loss has no NEXT LEVEL");
    var huge=BigInteger.Parse("999999999999999999999999999999999999");g.StartLevel(huge);g.Session.PlayerSales=1;result.Invoke(g,null);var hugeButtons=(PointerDownButton[])buttons.GetValue(g);hugeButtons[0].Press();Check(g.CurrentLevel==huge+1,"progression beyond Int64 does not overflow");
    for(int level=6;level<=130;level++){
     var c=g.ConfigFor(level);float expected=Math.Max(10,32f-.4f*Math.Min(55,level-6));Check(Math.Abs(c.OrderPatience-expected)<.001f&&c.TotalCustomers<=30&&c.TotalCustomers%3==0,"bounded difficulty "+level);
    }
    for(int level=6;level<=8;level++){
     bool cherry=false,lime=false;var c=g.ConfigFor(level);
     var p=new PourController(false,c,42);
     for(int i=0;i<300;i++){var o=p.Orders.Slots[0];cherry|=o.Recipe.Contains(Ingredient.CHERRY);lime|=o.Recipe.Contains(Ingredient.LIME);Check(o.Recipe.All(x=>c.ActiveIngredients.Contains(x)),"recipe available in ring");while(!o.Complete)p.Press(o.Next,false);Check(o.Complete,"order processed");p.Orders.Replace(0);}
     Check(cherry&&lime,"Cherry and Lime generated and served LEVEL "+level);
     p.Orders.Slots[0]=new OrderData(0,Ingredient.CHERRY,Ingredient.LIME);p.Press(Ingredient.CHERRY,true);p.Press(Ingredient.LIME,true);ChainStep step;while(p.TryReleaseNext(out step)){}Check(p.Orders.Slots[0].Complete&&p.Buffer.Count==0,"Cherry/Lime hold chain LEVEL "+level);
    }
    Ingredient[][] expectedIngredients={new[]{Ingredient.BLUE,Ingredient.RED},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM,Ingredient.LIME,Ingredient.CHERRY}};
    for(int level=1;level<=5;level++){var c=g.ConfigFor(level);Check(c.OrderPatience==45&&c.EffectiveHoldCapacity==level+1&&c.ActiveIngredients.SequenceEqual(expectedIngredients[level-1]),"staged LEVEL "+level);var staged=new PourController(false,c,100+level);for(int sample=0;sample<120;sample++){var o=staged.Orders.Slots[0];Check(o.Recipe.All(c.ActiveIngredients.Contains),"unlocked recipe LEVEL "+level);while(!o.Complete)staged.Press(o.Next,false);staged.Orders.Replace(0);}g.StartLevel(level);g.Session.PlayerSales=1;g.Session.RivalSales=0;if(level!=5)result.Invoke(g,null);if(level!=5)Check(((PointerDownButton[])buttons.GetValue(g)).Length==2,"win condition LEVEL "+level);}
    var trialField=typeof(GameController).GetField("playTrialService",BindingFlags.NonPublic|BindingFlags.Instance);
    var trialProbe=new TrialProbe{Status=PlayTrialStatus.UnderTrial};trialField.SetValue(g,trialProbe);g.Session.PlayerSales=1;g.Session.RivalSales=0;result.Invoke(g,null);result.Invoke(g,null);Check(trialProbe.StopCalls==1,"LEVEL 5 stops progression trial once");
    g.StartLevel(5);var purchasedProbe=new TrialProbe{Status=PlayTrialStatus.NotUnderTrial};trialField.SetValue(g,purchasedProbe);g.Session.PlayerSales=1;g.Session.RivalSales=0;result.Invoke(g,null);Check(purchasedProbe.StopCalls==0,"purchased user continues without trial stop");
    g.StartLevel(8);g.StartPractice();Check(g.Session.Demo&&g.Rules.Buffer.Capacity==6,"practice keeps original buffer rules");
    g.ShowHome();g.StartSession(false);Check(g.CurrentLevel==1,"returning to PLAY starts LEVEL 1");
    Debug.Log("PROGRESSION_PASS checks="+checks);
   }catch(Exception e){Debug.LogException(e);exit=1;}
   EditorApplication.Exit(exit);
  }
 }
}
