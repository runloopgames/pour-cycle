using System.Collections.Generic;
namespace PourCycle {
 public sealed class PourResult {
  public bool Success,Held,Release;public Ingredient Item;public int HoldCount;
  public List<OrderData> Targets=new List<OrderData>();
  public int Completed=>Targets.FindAll(o=>o.Complete).Count;
 }
 // Each resolved step records its source and destination; presentation may later group steps into a burst.
 public sealed class ChainStep {
  public readonly Ingredient Item;public readonly int BufferIndex,PreviousStep;public readonly OrderData Target;
  public bool Completed=>PreviousStep+1==Target.Recipe.Length;
  public ChainStep(Ingredient item,int index,OrderData target,int previous){Item=item;BufferIndex=index;Target=target;PreviousStep=previous;}
 }
 public sealed class PourController {
  public readonly OrderManager Orders;public readonly HoldBufferController Buffer;
  public PourController(bool demo,int seed=0):this(demo,demo?LevelConfig.Practice():LevelConfig.Defaults()[0],seed){}
  public PourController(bool demo,LevelConfig level,int seed=0){
   level=level??LevelConfig.Defaults()[0];Orders=new OrderManager(demo,level,seed);Buffer=new HoldBufferController(level.EffectiveHoldCapacity);
   if(demo)Buffer.Add(Ingredient.FOAM);
  }
  public PourResult Press(Ingredient item,bool tilt){
   var r=new PourResult{Item=item};
   if(tilt){r.Held=true;r.Success=Buffer.Add(item);r.HoldCount=Buffer.Count;return r;}
   r.Targets=Orders.Apply(item,1);r.Success=r.Targets.Count>0;return r;
  }
  public bool TryReleaseNext(out ChainStep step){
   for(int i=0;i<Buffer.Count;i++){
    Ingredient item=Buffer[i];OrderData oldest=null;
    foreach(var order in Orders.Slots)if(!order.Complete&&order.Next==item&&(oldest==null||order.Id<oldest.Id))oldest=order;
    if(oldest==null)continue;
    step=new ChainStep(item,i,oldest,oldest.Step);oldest.Step++;Buffer.RemoveAt(i);return true;
   }
   step=null;return false;
  }
 }
}
