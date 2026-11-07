using UnityEngine;
namespace PourCycle {
 public sealed class CarouselController {
  readonly Ingredient[] ingredients; int selectedIndex;
  public Ingredient Selected => ingredients[Mathf.Clamp(selectedIndex,0,ingredients.Length-1)];
  public float CurrentRotation {get;private set;}
  public float TargetRotation {get;private set;}
  public float Duration=.23f;
  bool leftHand; float from,elapsed,markerAngle; RectTransform[] icons; RectTransform marker;
  public CarouselController():this(LevelConfig.StandardIngredients){}
  public CarouselController(Ingredient[] active){ingredients=(active!=null&&active.Length>0)?active:LevelConfig.StandardIngredients;}
  float StepAngle=>360f/Mathf.Max(1,ingredients.Length);
  public void Step(bool back){selectedIndex=(selectedIndex+(back?ingredients.Length-1:1))%ingredients.Length;from=CurrentRotation;TargetRotation+=back?-StepAngle:StepAngle;elapsed=0;}
  public void Tick(float dt){elapsed+=dt;float u=Mathf.Clamp01(elapsed/Duration);CurrentRotation=Mathf.LerpUnclamped(from,TargetRotation,1-Mathf.Pow(1-u,3));Position();}
  void Position(){if(icons==null)return;float step=StepAngle;float radius=ingredients.Length>6?365f:370f;for(int i=0;i<ingredients.Length;i++){float a=(i*step-CurrentRotation+markerAngle)*Mathf.Deg2Rad;icons[i].anchoredPosition=new Vector2(450+Mathf.Cos(a)*radius,-450-Mathf.Sin(a)*radius);icons[i].localRotation=Quaternion.identity;}if(marker!=null){float a=markerAngle*Mathf.Deg2Rad;marker.anchoredPosition=new Vector2(450+Mathf.Cos(a)*radius,-450-Mathf.Sin(a)*radius);}}
  public RectTransform Draw(Transform parent,HandednessController hand){
   leftHand=hand!=null&&hand.Left;markerAngle=leftHand?180f:0f;
   var root=UiFactory.Rect(parent,"Carousel",hand.X(205),1395,900,900);
   UiFactory.Box(root,"lane",450,450,850,850,UiFactory.Lime,true);UiFactory.Box(root,"lane center",450,450,660,660,UiFactory.Paper,true);UiFactory.Text(root,"POUR\nCYCLE",450,450,320,180,66);
   float markerSize=ingredients.Length>6?208:224;
   marker=UiFactory.Box(root,"selected ring",450,450,markerSize,markerSize,UiFactory.Ink,true,true).rectTransform;
   icons=new RectTransform[ingredients.Length];
   for(int i=0;i<ingredients.Length;i++){
    float w=ingredients.Length>6?190:210,h=ingredients.Length>6?210:230,size=ingredients.Length>6?170:190;
    icons[i]=UiFactory.Rect(root,"Upright "+ingredients[i],0,0,w,h);
    UiFactory.Icon(icons[i],ingredients[i],w*.5f,h*.45f,size);
    UiFactory.Text(icons[i],ingredients[i].ToString(),w*.5f,h-10,w,32,ingredients.Length>6?22:25);
   }
   Position();return root;
  }
 }
}
