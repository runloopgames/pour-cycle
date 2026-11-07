using System;
namespace PourCycle {
 public sealed class HoldBufferController {
  public const int MaxSlots=6;
  readonly Ingredient[] items=new Ingredient[MaxSlots];
  public int Capacity {get;private set;}
  public int Count {get;private set;}
  public Ingredient this[int index] {get{if(index<0||index>=Count)throw new ArgumentOutOfRangeException(nameof(index));return items[index];}}

  public HoldBufferController(int capacity=3){Capacity=Math.Max(1,Math.Min(MaxSlots,capacity));}
  public bool Add(Ingredient item){if(Count>=Capacity)return false;items[Count++]=item;return true;}
  public void RemoveAt(int index){if(index<0||index>=Count)throw new ArgumentOutOfRangeException(nameof(index));for(int i=index;i<Count-1;i++)items[i]=items[i+1];items[--Count]=default(Ingredient);}
  public void Consume(int count){while(count-->0&&Count>0)RemoveAt(0);}
 }
}

