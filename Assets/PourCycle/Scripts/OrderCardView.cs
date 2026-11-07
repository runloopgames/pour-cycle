using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PourCycle {
 public sealed class OrderCardView {
  public RectTransform Root;
  public const float MissedDuration=.84f;
  readonly Image background,patienceFill,missedFlash;
  readonly Image[] frame=new Image[4];
  readonly CanvasGroup cardGroup,contentGroup;
  readonly TextMeshProUGUI time,missedLabel;
  readonly bool complete;
  bool missed;
  static Sprite fillSprite;
  static readonly Color Mint=new Color(.12f,.76f,.72f),Yellow=new Color(1f,.79f,.16f),Orange=new Color(1f,.43f,.12f),Red=new Color(.91f,.18f,.22f);
  public OrderCardView(Transform parent,OrderData order,int slot){
   complete=order.Complete;
   background=UiFactory.RoundBox(parent,"Order "+order.Id,190+350*slot,270,340,230,Color.white);
   Root=background.rectTransform;cardGroup=Root.gameObject.AddComponent<CanvasGroup>();
   patienceFill=UiFactory.Box(Root,"Patience area",170,115,340,230,Mint);
   if(fillSprite==null)fillSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
   patienceFill.sprite=fillSprite;patienceFill.type=Image.Type.Filled;patienceFill.fillMethod=Image.FillMethod.Horizontal;
   // Anchor the remaining area on the right: depletion travels left to right.
   patienceFill.fillOrigin=(int)Image.OriginHorizontal.Right;patienceFill.fillAmount=complete?0:1;
   frame[0]=UiFactory.Box(Root,"top frame",170,3,340,6,UiFactory.Grey);
   frame[1]=UiFactory.Box(Root,"bottom frame",170,227,340,6,UiFactory.Grey);
   frame[2]=UiFactory.Box(Root,"left frame",3,115,6,230,UiFactory.Grey);
   frame[3]=UiFactory.Box(Root,"right frame",337,115,6,230,UiFactory.Grey);
   var content=UiFactory.Rect(Root,"Recipe content",170,115,340,230);contentGroup=content.gameObject.AddComponent<CanvasGroup>();
   UiFactory.Text(content,"ORDER / "+order.Id.ToString("00"),170,24,320,34,22);
    if(complete){
     UiFactory.Box(content,"complete badge",170,98,84,84,UiFactory.Lime,true);
     // Liberation Sans does not contain the check-mark glyph. Draw it as two UI
     // strokes so it remains identical and warning-free on Android and iOS.
     var checkShort=UiFactory.Box(content,"complete check short",156,104,22,8,UiFactory.Ink);
     checkShort.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
     var checkLong=UiFactory.Box(content,"complete check long",179,95,45,8,UiFactory.Ink);
     checkLong.rectTransform.localRotation=Quaternion.Euler(0,0,47);
     UiFactory.Text(content,"COMPLETE",170,171,320,42,30);
    }
   else {
    int count=order.Recipe.Length-order.Step;
    float iconSize=count>=3?96:count==2?108:120;
    float spacing=count>=3?110:count==2?132:0;
    float nameSize=count>=3?19:count==2?21:23;
    for(int j=0;j<count;j++){
     float x=170+(j-(count-1)*.5f)*spacing;
     UiFactory.Icon(content,order.Recipe[order.Step+j],x,104,iconSize);
     UiFactory.Text(content,order.Recipe[order.Step+j].ToString(),x,171,106,27,nameSize);
     if(j<count-1)UiFactory.Text(content,">",x+spacing*.5f,104,22,28,20);
    }
   }
   time=UiFactory.Text(content,"",170,219,310,20,15);
   missedFlash=UiFactory.Box(Root,"Missed flash",170,115,340,230,Color.clear);
   missedLabel=UiFactory.Text(Root,"MISSED",170,114,320,70,44,Red);missedLabel.enabled=false;
   Patience(1,0,false);
  }
  public void Patience(float fraction,float seconds,bool demo,bool debug=false){
   if(missed)return;
   float f=demo?1:Mathf.Clamp01(fraction);
   Color color=PatienceColor(f);color.a=.18f;
   patienceFill.fillAmount=complete?0:f;patienceFill.color=color;
   bool danger=!complete&&!demo&&f<=.15f;
   Color border=complete?UiFactory.Lime:danger?Color.Lerp(new Color(1f,.62f,.64f),Red,.5f+.5f*Mathf.Sin(Time.time*4f)):UiFactory.Grey;
   for(int i=0;i<frame.Length;i++)frame[i].color=border;
   time.text=!debug?"":demo?"PRACTICE / NO RUSH":"PATIENCE  "+Mathf.CeilToInt(seconds)+"s";
  }
  public static Color PatienceColor(float f){
   if(f>.60f)return Mint;
   if(f>.35f)return Yellow;
   if(f>.15f)return Orange;
   return Red;
  }
  public void ShowMissed(float elapsed){
   missed=true;patienceFill.fillAmount=0;Root.localScale=Vector3.one;
   for(int i=0;i<frame.Length;i++)frame[i].color=Red;
   // One brief flash, then a readable hold, then fade the whole old card.
   Color flash=Red;flash.a=elapsed<.14f?.52f*(1-Mathf.Clamp01(elapsed/.14f)):0;
   missedFlash.color=flash;contentGroup.alpha=elapsed<.14f?1:0;
   missedLabel.enabled=elapsed>=.14f;
   cardGroup.alpha=1-Mathf.Clamp01((elapsed-.54f)/.30f);
  }
 }
}
