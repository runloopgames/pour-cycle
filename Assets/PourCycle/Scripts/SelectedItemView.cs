using UnityEngine;
namespace PourCycle {
 public sealed class SelectedItemView {
  public RectTransform Buffer;readonly HandednessController hand;
  public static Vector2 SlotPosition(HandednessController hand,int index)=>new Vector2(hand.X(875)+(index%3-1)*116,1336+(index/3)*102);
  public SelectedItemView(Transform parent,HandednessController hand,Ingredient item,HoldBufferController hold){
   this.hand=hand;float x=hand.X(875);
   UiFactory.Box(parent,"Selected white disc",x,1040,370,370,Color.white,true);
   UiFactory.Box(parent,"Selected ring",x,1040,370,370,UiFactory.Ink,true,true);
   UiFactory.Text(parent,"SELECTED",x,892,330,28,23);
   UiFactory.Icon(parent,item,x,1035,235);
   UiFactory.Text(parent,item.ToString(),x,1180,330,40,32);
   Buffer=UiFactory.Rect(parent,"Hold buffer",x,1380,400,260);
   UiFactory.Text(Buffer,"HOLD BUFFER",200,19,390,30,24);
   for(int j=0;j<HoldBufferController.MaxSlots;j++){
    float sx=84+j%3*116,sy=86+j/3*102;bool unlocked=j<hold.Capacity;
    UiFactory.Box(Buffer,"Slot "+j,sx,sy,96,96,j<hold.Count?UiFactory.Lime:unlocked?Color.white:new Color(.79f,.80f,.77f),true);
    UiFactory.Box(Buffer,"slot outline",sx,sy,92,92,unlocked?UiFactory.Ink:new Color(.67f,.69f,.66f),true,true);
    if(j<hold.Count)UiFactory.Icon(Buffer,hold[j],sx,sy,88);
    else if(unlocked)UiFactory.Box(Buffer,"empty slot",sx,sy,18,18,UiFactory.Grey,true,true);
    else UiFactory.Text(Buffer,"LOCK",sx,sy,100,30,18,new Color(.44f,.47f,.43f));
   }
   UiFactory.Text(Buffer,"HOLD POUR TO RELEASE",200,248,400,20,18);
  }
 }
}
