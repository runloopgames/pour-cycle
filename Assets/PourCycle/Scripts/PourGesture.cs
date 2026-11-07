using System;
namespace PourCycle {
 public enum PourAction {None,Pour,Hold,Release}
 // Input timing is separate from Unity callbacks so short/long press cannot both fire.
 public sealed class PourGesture {
  public bool Active {get;private set;}public bool Tilted {get;private set;}
  public float Elapsed {get;private set;}public float Threshold {get;private set;}
  bool fired;
  public float Progress=>Active&&!Tilted?Math.Min(1,Elapsed/Threshold):0;
  public void Begin(bool tilt,float threshold){Active=true;Tilted=tilt;Elapsed=0;Threshold=Math.Max(.1f,threshold);fired=false;}
  public PourAction Tick(float delta,bool tilt){
   if(!Active)return PourAction.None;
   if(tilt!=Tilted){Cancel();return PourAction.None;}
   Elapsed+=Math.Max(0,delta);
   if(!Tilted&&!fired&&Elapsed>=Threshold){fired=true;return PourAction.Release;}return PourAction.None;
  }
  public PourAction End(){if(!Active)return PourAction.None;Active=false;if(fired)return PourAction.None;return Tilted?PourAction.Hold:Elapsed>=Threshold?PourAction.Release:PourAction.Pour;}
  public void Cancel(){Active=false;Elapsed=0;fired=false;}
 }
}
