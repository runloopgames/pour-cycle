using UnityEngine;
namespace PourCycle {
 public sealed class CupView {
  public RectTransform Root {get;private set;}
  public RectTransform Visual=>contents;
  RectTransform contents; readonly OrderData order;
  RectTransform latestLayer,latestFoam,latestSurface,latestTopping;float latestBottom,latestHeight;
  public static Vector2 Mouth(int slot)=>new Vector2(190+350*slot,487);
  public CupView(Transform parent,OrderData data,int slot,int visibleStep){
   order=data;Root=UiFactory.Rect(parent,"Making cup "+data.Id,190+350*slot,610,270,360);
   Root.localScale=Vector3.one*1.45f;UiFactory.Text(Root,"ORDER "+data.Id.ToString("00"),135,18,250,35,22);Render(visibleStep);
  }
  public void Render(int step,float newest=1){
   latestLayer=null;latestFoam=null;latestSurface=null;latestTopping=null;
   if(contents!=null){contents.gameObject.SetActive(false);Object.Destroy(contents.gameObject);}
   contents=UiFactory.Rect(Root,"Cup contents",135,170,270,340);
   bool hasCup=System.Array.IndexOf(order.Recipe,Ingredient.CUP)<0;
   for(int i=0;i<step;i++)if(order.Recipe[i]==Ingredient.CUP)hasCup=true;
   if(!hasCup){UiFactory.Box(contents,"cup placeholder",135,190,178,225,UiFactory.Grey);UiFactory.Text(contents,"CUP\nNEEDED",135,180,155,100,26);return;}
   if(order.Complete&&step==order.Step)UiFactory.RoundBox(contents,"finished glow",135,188,232,286,new Color(UiFactory.Lime.r,UiFactory.Lime.g,UiFactory.Lime.b,.18f));
   UiFactory.RoundBox(contents,"glass",135,188,178,236,new Color(.88f,.94f,.92f));
   RectTransform foamTop=null;var toppingLayers=new RectTransform[Mathf.Max(0,step)];int toppingLayerCount=0;
   int liquidTotal=0;foreach(var item in order.Recipe)if(item!=Ingredient.CUP && item!=Ingredient.FOAM && !UiFactory.IsTopping(item))liquidTotal++;
   float layerHeight=206f/Mathf.Max(1,liquidTotal),bottom=300;
   int toppingCount=0;
   for(int i=0;i<step;i++){
    Ingredient ingredient=order.Recipe[i];float progress=i==step-1?newest:1;
    if(ingredient==Ingredient.CUP)continue;
    if(ingredient==Ingredient.FOAM){
     var foam=UiFactory.Rect(contents,"White foam on rim",135,80,190,82);foamTop=foam;foam.localScale=new Vector3(1,Mathf.Lerp(.1f,1,progress),1);if(i==step-1)latestFoam=foam;
     UiFactory.FoamIcon(foam,95,39,160,Color.white,UiFactory.Ink);
    }else if(UiFactory.IsTopping(ingredient)){
     float x=ingredient==Ingredient.LIME?70+toppingCount*52:178-toppingCount*42;float y=ingredient==Ingredient.LIME?70:50;
     var topping=UiFactory.Rect(contents,"Topping "+ingredient,x,y,70,70);topping.localScale=Vector3.one*Mathf.Lerp(.15f,1,progress);UiFactory.ToppingIcon(topping,ingredient,35,35,ingredient==Ingredient.LIME?62:58);toppingLayers[toppingLayerCount++]=topping;toppingCount++;if(i==step-1)latestTopping=topping;
    }else{
     float height=layerHeight*progress;var layer=UiFactory.Box(contents,"Layer "+i+" "+ingredient,135,bottom-height/2,170,height,UiFactory.ColorOf(ingredient));
     var surface=UiFactory.Box(contents,"meniscus "+i,135,bottom-height+2,170,3,new Color(1,1,1,.45f));
     if(i==step-1){latestLayer=layer.rectTransform;latestSurface=surface.rectTransform;latestBottom=bottom;latestHeight=layerHeight;}bottom-=layerHeight;
    }
   }
   UiFactory.Box(contents,"left glass",45,190,5,238,UiFactory.Ink);UiFactory.Box(contents,"right glass",225,190,5,238,UiFactory.Ink);
   UiFactory.Box(contents,"glass base",135,310,186,6,UiFactory.Ink);UiFactory.Box(contents,"rim",135,72,190,5,UiFactory.Ink);
   if(step>0){UiFactory.Ice(contents,102,240,23);UiFactory.Ice(contents,162,262,25);UiFactory.Ice(contents,132,212,21);}
   UiFactory.Box(contents,"reflection",59,187,6,194,new Color(1,1,1,.55f));
   if(foamTop!=null)foamTop.SetAsLastSibling();
   for(int i=0;i<toppingLayerCount;i++)toppingLayers[i].SetAsLastSibling();
   if(order.Complete&&step==order.Step){UiFactory.Box(contents,"finish sparkle left",35,105,16,16,UiFactory.Lime,true);UiFactory.Box(contents,"finish sparkle right",235,135,20,20,UiFactory.Lime,true);UiFactory.Box(contents,"finish sparkle top",205,62,12,12,UiFactory.Lime,true);}
   UiFactory.Text(contents,step==0?"READY":order.Complete && step==order.Step?"FINISHED":"IN THE MAKING",135,327,270,28,19);
  }
  public void Fill(float fraction){float u=Mathf.Clamp01(fraction);if(latestLayer!=null){float height=latestHeight*u;latestLayer.sizeDelta=new Vector2(160,height);latestLayer.anchoredPosition=new Vector2(135,-latestBottom+height/2);latestSurface.anchoredPosition=new Vector2(135,-latestBottom+height-2);}if(latestFoam!=null)latestFoam.localScale=new Vector3(1,Mathf.Lerp(.1f,1,u),1);if(latestTopping!=null)latestTopping.localScale=Vector3.one*Mathf.Lerp(.15f,1,u);}
 }
}
