using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PourCycle {
 public static class UiFactory {
  public static readonly Color Ink=new Color(.09f,.14f,.15f), Lime=new Color(.70f,.92f,.015f), Paper=new Color(.974f,.978f,.955f), Grey=new Color(.82f,.85f,.81f);
  static Sprite disc,ring,rounded,foamOuter,foamInner; static TMP_FontAsset font;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetGeneratedSprites(){disc=ring=rounded=foamOuter=foamInner=null;}
  public static Color ColorOf(Ingredient i){return new[]{Color.white,new Color(0f,.68f,.83f),new Color(.68f,.015f,.23f),new Color(1f,.62f,.79f),Lime,new Color(.88f,.94f,.92f),new Color(.90f,.035f,.19f)}[(int)i];}
  public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var g=new GameObject(name,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
  public static Image Box(Transform parent,string name,float x,float y,float w,float h,Color color,bool circle=false,bool outline=false){var r=Rect(parent,name,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;if(circle)im.sprite=Circle(outline);return im;}
  public static TextMeshProUGUI Text(Transform parent,string value,float x,float y,float w,float h,float size,Color? color=null){
   if(font==null)font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
   var r=Rect(parent,value,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color??Ink;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;
  }
  public static Image Button(Transform parent,string label,float x,float y,float w,float h,System.Action action){RoundBox(parent,label+" shadow",x,y+8,w,h,Ink);var im=RoundBox(parent,label,x,y,w,h,Grey);im.raycastTarget=true;im.gameObject.AddComponent<PointerDownButton>().Press=action;Text(im.transform,label,w/2,h/2,w-12,h,28);return im;}
  public static Image RoundBox(Transform parent,string name,float x,float y,float w,float h,Color color){
   var im=Box(parent,name,x,y,w,h,color);im.sprite=Rounded();im.type=Image.Type.Sliced;return im;
  }
  static Sprite Rounded(){
   if(rounded!=null)return rounded;
   const int n=64;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){
    float dx=Mathf.Max(Mathf.Abs(x+0.5f-32)-16,0),dy=Mathf.Max(Mathf.Abs(y+0.5f-32)-16,0);
    pixels[y*n+x]=new Color(1,1,1,Mathf.Clamp01(16-Mathf.Sqrt(dx*dx+dy*dy)));
   }
   tex.SetPixels(pixels);tex.Apply();rounded=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(18,18,18,18));return rounded;
  }
  public static void Ice(Transform parent,float x,float y,float size){
   RoundBox(parent,"ice edge",x,y,size,size,new Color(1,1,1,.85f));
   RoundBox(parent,"ice interior",x,y,size-3,size-3,new Color(.45f,.89f,.95f,.45f));
   Box(parent,"ice glint",x-size*.22f,y,2,size*.50f,Color.white);
  }
  static Sprite Circle(bool outline){
   if(outline && ring!=null)return ring;if(!outline && disc!=null)return disc;
   const int n=256;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));float a=Mathf.Clamp01(127-d);if(outline)a*=Mathf.Clamp01(d-117);pixels[y*n+x]=new Color(1,1,1,a);}
   tex.SetPixels(pixels);tex.Apply();var s=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100);if(outline)ring=s;else disc=s;return s;
  }
  static Sprite FoamShape(bool inner){
   if(inner&&foamInner!=null)return foamInner;if(!inner&&foamOuter!=null)return foamOuter;
   const int n=256;float inset=inner?6:0;
   // Reuse the visible foam cap from the completed drinks: three bubbles sit on
   // the glass rim and three round bubbles overlap them above. Keeping the top
   // bubble joined to its neighbours avoids a head/body silhouette.
   float[] cx={64,128,192,86,128,170},cy={78,72,78,126,134,126},radius={50,56,50,43,48,43};
   var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;var pixels=new Color[n*n];
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){
    var point=new Vector2(x+.5f,y+.5f);float edge=999;
    for(int i=0;i<cx.Length;i++)edge=Mathf.Min(edge,Vector2.Distance(point,new Vector2(cx[i],cy[i]))-(radius[i]-inset));
    if(inner)edge=Mathf.Min(edge,Vector2.Distance(point,new Vector2(128,108))-48);
    pixels[y*n+x]=new Color(1,1,1,Mathf.Clamp01(.5f-edge));
   }
   tex.SetPixels(pixels);tex.Apply();var sprite=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100);if(inner)foamInner=sprite;else foamOuter=sprite;return sprite;
  }
  public static void FoamIcon(Transform parent,float x,float y,float size,Color? fill=null,Color? stroke=null){
   Color f=fill??Color.white,s=stroke??Ink;
   var outer=Rect(parent,"foam outer silhouette",x,y,size,size);var outerImage=outer.gameObject.AddComponent<Image>();outerImage.sprite=FoamShape(false);outerImage.color=s;outerImage.raycastTarget=false;
   var inner=Rect(parent,"foam inner silhouette",x,y,size,size);var innerImage=inner.gameObject.AddComponent<Image>();innerImage.sprite=FoamShape(true);innerImage.color=f;innerImage.raycastTarget=false;
  }
  public static void LimeSliceIcon(Transform parent,float x,float y,float size){
   Box(parent,"lime peel",x,y,size,size,Lime,true);Box(parent,"lime flesh",x,y,size*.78f,size*.78f,new Color(.92f,1f,.55f),true);Box(parent,"lime outline",x,y,size,size,Ink,true,true);
   for(int i=0;i<6;i++){float a=i*Mathf.PI/3f;var segment=Box(parent,"lime segment",x+Mathf.Cos(a)*size*.19f,y+Mathf.Sin(a)*size*.19f,size*.36f,2,Color.white);segment.rectTransform.localRotation=Quaternion.Euler(0,0,-i*60);}
   Box(parent,"lime cut",x+size*.12f,y-size*.05f,size*.58f,3,Color.white);
  }
  public static void CherryIcon(Transform parent,float x,float y,float size){
   Box(parent,"cherry body",x,y+size*.08f,size*.62f,size*.62f,new Color(.95f,.05f,.08f),true);Box(parent,"cherry shade",x+size*.08f,y+size*.14f,size*.42f,size*.42f,new Color(.65f,.02f,.05f,.35f),true);Box(parent,"cherry shine",x-size*.10f,y-size*.03f,size*.13f,size*.13f,Color.white,true);
   Box(parent,"cherry stem",x+size*.16f,y-size*.25f,4,size*.46f,Ink);Box(parent,"cherry stem top",x+size*.27f,y-size*.44f,size*.24f,4,Ink);Box(parent,"cherry leaf",x+size*.29f,y-size*.32f,size*.26f,size*.12f,new Color(.24f,.53f,.13f),true);
  }
  public static void ToppingIcon(Transform parent,Ingredient item,float x,float y,float size){if(item==Ingredient.LIME)LimeSliceIcon(parent,x,y,size);else if(item==Ingredient.CHERRY)CherryIcon(parent,x,y,size);}
  public static bool IsTopping(Ingredient item)=>item==Ingredient.LIME||item==Ingredient.CHERRY;
  public static void Icon(Transform parent,Ingredient item,float x,float y,float size){
   Box(parent,"ingredient disc",x,y,size,size,Color.white,true);
   if(item==Ingredient.FOAM){FoamIcon(parent,x,y-size*.04f,size*.85f);return;}
   if(item==Ingredient.LIME){LimeSliceIcon(parent,x,y,size*.72f);return;}
   if(item==Ingredient.CHERRY){CherryIcon(parent,x,y,size*.82f);return;}
   RoundBox(parent,"glass outline",x,y,size*.52f,size*.70f,Ink);
   RoundBox(parent,"glass inside",x,y,size*.46f,size*.64f,new Color(.88f,.94f,.92f));
   if(item!=Ingredient.CUP)RoundBox(parent,"drink",x,y+size*.02f,size*.41f,size*.55f,ColorOf(item));
   Box(parent,"glass reflection",x-size*.16f,y,size*.025f,size*.50f,Color.white);
   if(item!=Ingredient.CUP){Ice(parent,x+size*.08f,y+size*.16f,size*.10f);Ice(parent,x-size*.04f,y+size*.08f,size*.09f);}
  }
 }
}
