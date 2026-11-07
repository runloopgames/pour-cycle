using System;
namespace PourCycle {
 public sealed class OrderPatienceController {
  readonly int[] ids=new int[3]; readonly float[] remaining=new float[3]; float duration;
  public OrderPatienceController(float seconds){duration=Math.Max(1,seconds);}
  public void SetDuration(float seconds){float next=Math.Max(1,seconds);if(Math.Abs(next-duration)<.01f)return;for(int i=0;i<remaining.Length;i++)remaining[i]=Math.Min(next,remaining[i]*(next/duration));duration=next;}
  public void Sync(OrderManager orders){for(int i=0;i<3;i++)if(ids[i]!=orders.Slots[i].Id){ids[i]=orders.Slots[i].Id;remaining[i]=duration;}}
  public void Tick(OrderManager orders,float dt,bool frozen){Sync(orders);if(frozen)return;for(int i=0;i<3;i++)if(!orders.Slots[i].Complete)remaining[i]=Math.Max(0,remaining[i]-dt);}
  public float Fraction(int slot)=>remaining[slot]/duration;
  public float Seconds(int slot)=>remaining[slot];
  public bool Expired(int slot)=>remaining[slot]<=0;
 }
}
