using System.Collections;
using BigInteger = System.Numerics.BigInteger;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
namespace PourCycle {
 public sealed class GameController : MonoBehaviour {
  enum ScreenMode { MainMenu, Gameplay, Settings, Pause, Result }
  public bool Preview04Mode=true; public bool DebugMode=false; [Range(.18f,.28f)] public float SelectDuration=.23f; public CarouselController Carousel=>carousel;
  public LevelConfig[] Levels=LevelConfig.Defaults();
  [Min(1)] public float SessionSeconds=60;
  [Range(5,90)] public float OrderPatienceSeconds=15;
  [Range(20,120)] public float PracticePatienceSeconds=60;
  [Tooltip("Keep Preview04 untimed so its reference scenario can be practiced.")]
  public bool FreezePatienceInPreview=true;
  [Range(.15f,1f)] public float ReleaseHoldSeconds=.35f;
  [Range(.12f,.18f)] public float ChainStepSeconds=.15f;
  public TiltInputController Tilt;
  readonly PourGesture pourGesture=new PourGesture();Image releaseFill;bool keyboardPour,resolvingChain;PointerDownButton[] resultButtons;float resultInputUnlockAt;bool resultWaitForPointerRelease;
#if UNITY_IOS
  enum IOSControlMode {Classic,TiltPour}
  const string IOSControlModeKey="PourCycle.iOS.Control";
  const float TiltPourActivationSeconds=.12f;
  readonly TiltPourGesture tiltPourGesture=new TiltPourGesture();
  IOSControlMode iosControlMode;
  bool IsTiltPourMode=>iosControlMode==IOSControlMode.TiltPour;
#endif
  bool PourInputActive=>pourGesture.Active
#if UNITY_IOS
   ||(IsTiltPourMode&&tiltPourGesture.Active)
#endif
  ;
  KeyCode pourKey;public int ActiveChainCount {get;private set;}public int LastChainCount {get;private set;}
  PourController pour; SessionController session; CarouselController carousel;
  OrderPatienceController patience; HandednessController hand; AudioController audioFx;
#if UNITY_ANDROID || UNITY_EDITOR
  IPlayTrialService playTrialService;
#endif
  RectTransform safe,board,effects; TextMeshProUGUI timer,status;
  OrderCardView[] cards; CupView[] cups; SelectedItemView selectedView;
  Image selectButton,pourButton,titlePlayButton; RectTransform ring,titleCarousel,titleFoam; RectTransform[] titleIcons,titleCupFills,titleCups,titleFlyers; TextMeshProUGUI[] titleOrders; RectTransform titleFlyer;
  ScreenMode mode=ScreenMode.MainMenu; bool busy,ending,resolvingMissed; Rect lastSafe; int sw,sh;
#if UNITY_ANDROID || UNITY_EDITOR
  bool trialCompletionRequested;
#endif
  LevelConfig currentConfig; BigInteger currentLevel=1;
  public BigInteger CurrentLevel=>currentLevel;
  string notice=""; float noticeUntil;
  public bool IsBusy=>busy;
  public PourController Rules=>pour;
  public SessionController Session=>session;
  public Ingredient Selection=>carousel==null?Ingredient.FOAM:carousel.Selected;
  public OrderPatienceController Patience=>patience;
  public void SetHand(bool left){hand.SetLeft(left);ApplyHandToTilt();Redraw();}
  void Awake(){
   Application.targetFrameRate=60;
#if UNITY_IOS && !UNITY_EDITOR
   Screen.orientation=ScreenOrientation.Portrait;
#endif
#if UNITY_IOS
   iosControlMode=PlayerPrefs.GetInt(IOSControlModeKey,0)==1?IOSControlMode.TiltPour:IOSControlMode.Classic;
#endif
#if UNITY_ANDROID || UNITY_EDITOR
   if(GetComponent<PlayTrialLifecycle>()==null)gameObject.AddComponent<PlayTrialLifecycle>();
   playTrialService=new PlayTrialService();
#endif
   hand=new HandednessController();audioFx=gameObject.AddComponent<AudioController>();
   if(Tilt==null)Tilt=gameObject.AddComponent<TiltInputController>();ApplyHandToTilt();
   var canvas=new GameObject("Portrait Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=0;
   safe=UiFactory.Rect(canvas.transform,"Safe portrait 1080x1920",0,0,1080,1920);
   if(Levels==null||Levels.Length==0)Levels=LevelConfig.Defaults();
   foreach(var level in Levels)if(level!=null&&level.HoldCapacity<=0)level.HoldCapacity=LevelConfig.DefaultHoldCapacity(level.LevelNumber);
   if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   DrawMainMenu();
  }
  void ApplyHandToTilt(){if(Tilt!=null&&hand!=null)Tilt.LeftHand=hand.Left;}
  void Fit(){
   Rect a=Screen.safeArea;if(a==lastSafe && sw==Screen.width && sh==Screen.height)return;
   lastSafe=a;sw=Screen.width;sh=Screen.height;
   float scale=Screen.width/1080f;safe.localScale=Vector3.one*Mathf.Min(a.width/1080f,a.height/1920f)/scale;
   safe.pivot=new Vector2(.5f,.5f);safe.anchorMin=safe.anchorMax=new Vector2(0,0);safe.anchoredPosition=a.center/scale;
  }
  public void StartSession(bool practice){
   if(practice)StartPractice();
   else StartLevel(1);
  }
  public void StartPractice(){
   StopAllCoroutines();CancelPourPress();resolvingChain=false;busy=false;ending=false;resolvingMissed=false;
#if UNITY_ANDROID || UNITY_EDITOR
   trialCompletionRequested=false;
#endif
   notice="";Preview04Mode=true;currentLevel=0;currentConfig=LevelConfig.Practice();mode=ScreenMode.Gameplay;Tilt.Calibrate();
   pour=new PourController(true,currentConfig);session=new SessionController(0,true,currentConfig.TotalCustomers);carousel=new CarouselController(currentConfig.ActiveIngredients);
   patience=new OrderPatienceController(PracticePatienceSeconds);patience.Sync(pour.Orders);DrawGameplay();
  }
  public void StartLevel(int level){StartLevel(new BigInteger(level));}
  public void StartLevel(BigInteger level){
   StopAllCoroutines();CancelPourPress();resolvingChain=false;busy=false;ending=false;resolvingMissed=false;
#if UNITY_ANDROID || UNITY_EDITOR
   trialCompletionRequested=false;
#endif
   Preview04Mode=false;currentLevel=BigInteger.Max(1,level);currentConfig=ConfigFor(currentLevel);
   notice="";noticeUntil=0;mode=ScreenMode.Gameplay;Tilt.Calibrate();
   pour=new PourController(false,currentConfig);session=new SessionController(0,false,currentConfig.TotalCustomers);carousel=new CarouselController(currentConfig.ActiveIngredients);
   patience=new OrderPatienceController(currentConfig.OrderPatience);patience.Sync(pour.Orders);DrawGameplay();
  }

  public LevelConfig ConfigFor(BigInteger level){
   level=BigInteger.Max(1,level);
   if(level>5)return LevelConfig.Generate(level,ConfigFor(5));
   if(Levels!=null)foreach(var config in Levels)if(config!=null&&config.LevelNumber==(int)level)return config;
   return LevelConfig.Defaults()[(int)level-1];
  }
  public void SetDebugMode(bool value){DebugMode=value;Redraw();}
  public void ShowHome(){StopAllCoroutines();CancelPourPress();resolvingChain=false;busy=false;ending=false;currentLevel=1;mode=ScreenMode.MainMenu;DrawMainMenu();}
  void ClearBoard(){
   CancelPourPress();
   if(board!=null){board.gameObject.SetActive(false);Destroy(board.gameObject);}
   if(effects!=null){effects.gameObject.SetActive(false);Destroy(effects.gameObject);}
   board=UiFactory.Rect(safe,"Board",540,960,1080,1920);
   UiFactory.Box(board,"Paper background",540,960,1080,1920,UiFactory.Paper);
   effects=UiFactory.Rect(safe,"Effects",540,960,1080,1920);
  }
  void Redraw(){
   if(mode==ScreenMode.MainMenu)DrawMainMenu();
   else if(mode==ScreenMode.Settings)DrawSettings(false);
   else if(mode==ScreenMode.Pause)DrawPause();
   else if(mode==ScreenMode.Result)ShowResult();
   else DrawGameplay();
  }
  void DrawMainMenu(){
   mode=ScreenMode.MainMenu;ClearBoard();
   titleIcons=null;titleCupFills=null;titleCups=null;titleFlyers=null;titleOrders=null;titleFoam=null;titleFlyer=null;
   UiFactory.Box(board,"title accent",540,20,1080,40,UiFactory.Lime);
   UiFactory.Text(board,"POUR CYCLE™",540,170,980,120,78,UiFactory.Ink);
   UiFactory.Text(board,"Hold. Release. Serve.",540,255,780,48,28,UiFactory.Ink);
   UiFactory.Box(board,"title rule",540,325,920,3,UiFactory.Grey);
   UiFactory.RoundBox(board,"drink showcase",400,740,720,800,Color.white);
   UiFactory.Text(board,"DRINK STAND",400,555,660,66,34,UiFactory.Ink);
   UiFactory.Box(board,"counter accent",400,1134,720,12,UiFactory.Lime);
   DrawTitleOrders();DrawTitleCups();DrawTitleCarousel();DrawTitleFlyer();
   titlePlayButton=UiFactory.Button(board,"PLAY",900,685,260,108,()=>StartLevel(1));titlePlayButton.color=UiFactory.Lime;titlePlayButton.GetComponentInChildren<TextMeshProUGUI>().fontSize=40;
   var practice=UiFactory.Button(board,"PRACTICE",900,830,260,100,()=>StartPractice());practice.color=new Color(.89f,.89f,.83f);practice.GetComponentInChildren<TextMeshProUGUI>().fontSize=34;
   var settings=UiFactory.Button(board,"SETTINGS",900,965,260,100,()=>DrawSettings(false));settings.color=new Color(.89f,.89f,.83f);settings.GetComponentInChildren<TextMeshProUGUI>().fontSize=34;
   UiFactory.Text(board,"Runloop Games™",540,1832,900,34,20,UiFactory.Ink);
  }
  void DrawTitleOrders(){
   titleOrders=new TextMeshProUGUI[3];
   Ingredient[,] sets={{Ingredient.RED,Ingredient.CHERRY,Ingredient.FOAM},{Ingredient.BLUE,Ingredient.LIME,Ingredient.FOAM},{Ingredient.CUP,Ingredient.PINK,Ingredient.FOAM}};
   for(int i=0;i<3;i++){
    var card=UiFactory.RoundBox(board,"title order",175+i*225,445,210,150,UiFactory.Paper).rectTransform;
    UiFactory.Box(card,"order accent",105,4,190,8,UiFactory.Lime);
    titleOrders[i]=UiFactory.Text(card,"ORDER / 0"+(i+1),105,23,194,28,18,UiFactory.Ink);
    for(int j=0;j<2;j++)UiFactory.Icon(card,sets[i,j],61+j*88,91,86);
   }
  }
  void DrawTitleCups(){
   titleCupFills=new RectTransform[4];titleCups=new RectTransform[3];
   for(int i=0;i<3;i++){
    float x=215+i*185;var cup=UiFactory.Rect(board,"title cup",x,840,205,330);titleCups[i]=cup;cup.localScale=Vector3.one*1.08f;
    UiFactory.Box(cup,"glass",102,170,150,242,new Color(.91f,.94f,.93f,.68f));
    UiFactory.Box(cup,"left glass",27,172,6,252,UiFactory.Ink);UiFactory.Box(cup,"right glass",177,172,6,252,UiFactory.Ink);
    UiFactory.Box(cup,"base",102,300,160,8,UiFactory.Ink);UiFactory.Box(cup,"rim",102,42,164,6,UiFactory.Ink);
    UiFactory.Box(cup,"shine",45,170,7,196,new Color(1,1,1,.45f));
    titleCupFills[i]=UiFactory.Box(cup,"title fill",102,286,140,0,UiFactory.ColorOf((Ingredient)(i+1))).rectTransform;titleCupFills[i].pivot=new Vector2(.5f,0);
    UiFactory.Ice(cup,75,253,20);UiFactory.Ice(cup,126,229,23);UiFactory.Box(cup,"front reflection",45,170,7,196,new Color(1,1,1,.8f));
    if(i==0)UiFactory.LimeSliceIcon(cup,35,48,64);if(i==1)UiFactory.CherryIcon(cup,159,36,70);
    if(i==2){titleFoam=UiFactory.Rect(cup,"title foam",102,36,168,78);UiFactory.FoamIcon(titleFoam,84,39,150,Color.white,UiFactory.Ink);}
   }
  }
  void DrawTitleCarousel(){
   titleCarousel=UiFactory.Rect(board,"title carousel",210,1410,900,900);
   UiFactory.Box(titleCarousel,"title lane",450,450,830,830,UiFactory.Lime,true);
   UiFactory.Box(titleCarousel,"title lane center",450,450,650,650,UiFactory.Paper,true);
   UiFactory.Text(titleCarousel,"SELECT\nHOLD\nRELEASE",450,450,310,190,44,UiFactory.Ink);
   Ingredient[] items=LevelConfig.ExtendedIngredients;titleIcons=new RectTransform[items.Length];
   for(int i=0;i<items.Length;i++){titleIcons[i]=UiFactory.Rect(titleCarousel,"title ingredient "+items[i],0,0,190,210);UiFactory.Icon(titleIcons[i],items[i],95,92,170);UiFactory.Text(titleIcons[i],items[i].ToString(),95,198,190,30,22);}
  }
  void DrawTitleFlyer(){
   titleFlyer=UiFactory.Rect(board,"title flying ingredient",0,0,96,96);
   Ingredient[] items={Ingredient.BLUE,Ingredient.RED,Ingredient.LIME,Ingredient.CHERRY,Ingredient.FOAM};titleFlyers=new RectTransform[items.Length];
   for(int i=0;i<items.Length;i++){titleFlyers[i]=UiFactory.Rect(titleFlyer,"flying "+items[i],48,48,96,96);UiFactory.Icon(titleFlyers[i],items[i],48,48,96);}
  }
  void AnimateTitle(){
   float loop=Mathf.Repeat(Time.time,4.2f)/4.2f;
   if(titleIcons!=null){float rotation=Time.time*18f,step=360f/titleIcons.Length;for(int i=0;i<titleIcons.Length;i++){float a=(i*step-rotation)*Mathf.Deg2Rad;titleIcons[i].anchoredPosition=new Vector2(450+Mathf.Cos(a)*355,-450-Mathf.Sin(a)*355);titleIcons[i].localRotation=Quaternion.identity;}}
   AnimateTitleCups(loop);
   if(titleOrders!=null){for(int i=0;i<3;i++)titleOrders[i].text="ORDER / 0"+(i+1);}
   AnimateTitleFlyer(loop);if(titlePlayButton!=null)titlePlayButton.rectTransform.localScale=Vector3.one*(1f+Mathf.Sin(Time.time*3f)*.025f);
  }
  void AnimateTitleCups(float loop){
   if(titleCupFills==null)return;
   bool incoming=loop>=.94f;
   float serve=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.88f,.94f,loop));
   float enter=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.94f,1f,loop));
   for(int i=0;i<3;i++)if(titleCups!=null&&i<titleCups.Length&&titleCups[i]!=null){
    float baseX=215+i*185;float x=incoming?Mathf.Lerp(-130,baseX,enter):baseX+serve*760;
    titleCups[i].anchoredPosition=new Vector2(x,-840);
   }
   if(incoming){SetTitleFill(0,UiFactory.ColorOf(Ingredient.BLUE),0);SetTitleFill(1,UiFactory.ColorOf(Ingredient.RED),0);SetTitleFill(2,UiFactory.ColorOf(Ingredient.LIME),0);if(titleFoam!=null)titleFoam.localScale=Vector3.zero;return;}
   SetTitleFill(0,UiFactory.ColorOf(Ingredient.BLUE),Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.30f,loop))*118f);
   SetTitleFill(1,UiFactory.ColorOf(Ingredient.RED),Mathf.SmoothStep(0,1,Mathf.InverseLerp(.36f,.48f,loop))*142f);
   SetTitleFill(2,UiFactory.ColorOf(Ingredient.LIME),Mathf.SmoothStep(0,1,Mathf.InverseLerp(.54f,.66f,loop))*112f);
   if(titleFoam!=null)titleFoam.localScale=Vector3.one*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,.82f,loop));
  }
  void SetTitleFill(int index,Color color,float height){if(titleCupFills==null||index>=titleCupFills.Length||titleCupFills[index]==null)return;var image=titleCupFills[index].GetComponent<Image>();if(image!=null)image.color=color;float h=Mathf.Clamp(height,0,180);titleCupFills[index].sizeDelta=new Vector2(140,h);titleCupFills[index].anchoredPosition=new Vector2(102,-286);}
  void AnimateTitleFlyer(float loop){
   if(titleFlyer==null)return;
   Ingredient[] items={Ingredient.BLUE,Ingredient.RED,Ingredient.LIME,Ingredient.CHERRY,Ingredient.FOAM};int[] cups={0,1,2,1,2};
   float[] starts={.04f,.20f,.36f,.52f,.68f};float[] ends={.13f,.29f,.45f,.61f,.77f};
   int active=-1;float u=0;
   for(int i=0;i<items.Length;i++)if(loop>=starts[i]&&loop<=ends[i]){active=i;u=Mathf.SmoothStep(0,1,Mathf.InverseLerp(starts[i],ends[i],loop));break;}
   if(titleIcons!=null)for(int i=0;i<titleIcons.Length;i++)if(titleIcons[i]!=null)titleIcons[i].gameObject.SetActive(active<0||i!=(int)items[active]);
   titleFlyer.gameObject.SetActive(active>=0);if(active<0)return;
   for(int i=0;i<titleFlyers.Length;i++)titleFlyers[i].gameObject.SetActive(i==active);
   Vector2 start=TitleIconBoardPosition(items[active]);
   Vector2 center=titleCarousel!=null?titleCarousel.anchoredPosition+new Vector2(450,-450):start;
   Vector2 outward=(start-center).sqrMagnitude>.001f?(start-center).normalized:Vector2.right;
   Vector2 tangent=new Vector2(-outward.y,outward.x);
   Vector2 pickup=start+outward*42f+tangent*24f;
   Vector2 end=new Vector2(215+cups[active]*185,-840);
   Vector2 p;
   if(u<.25f){float e=Mathf.SmoothStep(0,1,u/.25f);p=Vector2.Lerp(start,pickup,e);}
   else{float v=Mathf.SmoothStep(0,1,(u-.25f)/.75f);p=Vector2.Lerp(pickup,end,v)+new Vector2(0,Mathf.Sin(v*Mathf.PI)*-95);}
   titleFlyer.anchoredPosition=p;titleFlyer.localScale=Vector3.one*(1f-u*.18f);
  }
  Vector2 TitleIconBoardPosition(Ingredient item){
   if(titleIcons==null||titleCarousel==null)return Vector2.zero;
   RectTransform icon=titleIcons[(int)item];
   if(icon==null)return Vector2.zero;
   return titleCarousel.anchoredPosition+icon.anchoredPosition;
  }
  void DrawSettings(bool fromPause){
   mode=ScreenMode.Settings;ClearBoard();
   UiFactory.Text(board,"SETTINGS",540,310,900,100,62);
#if UNITY_IOS
   UiFactory.Text(board,"HAND",540,500,620,50,30);
   UiFactory.Button(board,hand.Left?"LEFT":"RIGHT",540,610,520,125,()=>{hand.Toggle();DrawSettings(fromPause);}).GetComponentInChildren<TextMeshProUGUI>().fontSize=40;
   UiFactory.Text(board,"CONTROL",540,790,620,50,30);
   UiFactory.Button(board,IsTiltPourMode?"TILT POUR":"CLASSIC",540,900,520,125,()=>{CancelPourPress();iosControlMode=IsTiltPourMode?IOSControlMode.Classic:IOSControlMode.TiltPour;PlayerPrefs.SetInt(IOSControlModeKey,(int)iosControlMode);PlayerPrefs.Save();DrawSettings(fromPause);}).GetComponentInChildren<TextMeshProUGUI>().fontSize=38;
   UiFactory.Text(board,IsTiltPourMode?"TILT POUR / TAP HOLD / LONG RELEASE":"TAP POUR / TILT HOLD",540,1000,800,40,20,UiFactory.Ink);
   UiFactory.Text(board,"Tilt Calibration",540,1160,620,50,30);
   UiFactory.Button(board,"CALIBRATE",540,1265,520,125,()=>{Tilt.Calibrate();notice="TILT CALIBRATED";noticeUntil=Time.time+1.2f;DrawSettings(fromPause);}).GetComponentInChildren<TextMeshProUGUI>().fontSize=36;
   UiFactory.Button(board,fromPause?"BACK":"HOME",540,1600,520,125,()=>{if(fromPause)DrawPause();else DrawMainMenu();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=36;
   if(Time.time<noticeUntil)UiFactory.Text(board,notice,540,1450,800,60,30,UiFactory.Ink);
#else
   UiFactory.Text(board,"HAND",540,560,620,50,30);
   UiFactory.Button(board,hand.Left?"LEFT":"RIGHT",540,665,520,125,()=>{hand.Toggle();DrawSettings(fromPause);}).GetComponentInChildren<TextMeshProUGUI>().fontSize=40;
   UiFactory.Text(board,"Tilt Calibration",540,850,620,50,30);
   UiFactory.Button(board,"CALIBRATE",540,955,520,125,()=>{Tilt.Calibrate();notice="TILT CALIBRATED";noticeUntil=Time.time+1.2f;DrawSettings(fromPause);}).GetComponentInChildren<TextMeshProUGUI>().fontSize=36;
   UiFactory.Button(board,fromPause?"BACK":"HOME",540,1240,520,125,()=>{if(fromPause)DrawPause();else DrawMainMenu();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=36;
   if(Time.time<noticeUntil)UiFactory.Text(board,notice,540,1120,800,60,30,UiFactory.Ink);
#endif
  }
  void DrawPause(){
   mode=ScreenMode.Pause;ClearBoard();
   UiFactory.Text(board,"PAUSE",540,360,900,100,66);
   UiFactory.Button(board,"RESUME",540,700,570,135,()=>{mode=ScreenMode.Gameplay;DrawGameplay();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=40;
   UiFactory.Button(board,"TILT CALIBRATION",540,880,570,135,()=>{Tilt.Calibrate();notice="TILT CALIBRATED";noticeUntil=Time.time+1.2f;DrawPause();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=34;
   UiFactory.Button(board,"HOME",540,1060,570,135,()=>ShowHome()).GetComponentInChildren<TextMeshProUGUI>().fontSize=40;
   if(Time.time<noticeUntil)UiFactory.Text(board,notice,540,1225,800,60,30,UiFactory.Ink);
  }
  void DrawGameplay(){
   ApplyHandToTilt();
   mode=ScreenMode.Gameplay;ClearBoard();
   UiFactory.Text(board,"POUR CYCLE™",540,62,610,72,50);
   if(!session.Demo)UiFactory.Text(board,"LEVEL "+currentLevel,120,68,210,58,38);
   timer=UiFactory.Text(board,"",540,128,980,46,29);
   UiFactory.Button(board,"II",1018,68,74,74,()=>{if(!busy)DrawPause();}).GetComponentInChildren<TextMeshProUGUI>().fontSize=30;
   cards=new OrderCardView[3];cups=new CupView[3];
   for(int i=0;i<3;i++){cards[i]=new OrderCardView(board,pour.Orders.Slots[i],i);cards[i].Patience(patience.Fraction(i),patience.Seconds(i),session.Demo&&FreezePatienceInPreview,DebugMode);cups[i]=new CupView(board,pour.Orders.Slots[i],i,pour.Orders.Slots[i].Step);}
   ring=carousel.Draw(board,hand);selectedView=new SelectedItemView(board,hand,carousel.Selected,pour.Buffer);
   selectButton=UiFactory.Button(board,"SELECT",hand.X(915),1580,310,135,()=>SelectWithModifier(Tilt.Modifier));
   string pourButtonTitle=
#if UNITY_IOS
    IsTiltPourMode?"POUR":"POUR&ON";
#else
    "POUR&ON";
#endif
   string pourButtonHint=
#if UNITY_IOS
    IsTiltPourMode?"TILT POUR / TAP HOLD":"TAP POUR / HOLD RELEASE";
#else
    "TAP POUR / HOLD RELEASE";
#endif
   pourButton=UiFactory.RoundBox(board,pourButtonTitle,hand.X(915),1745,310,135,UiFactory.Grey);pourButton.raycastTarget=true;
   releaseFill=UiFactory.Box(pourButton.transform,"Long press progress",0,128,0,7,new Color(.12f,.76f,.72f));releaseFill.rectTransform.pivot=new Vector2(0,.5f);
   UiFactory.Text(pourButton.transform,pourButtonTitle,155,58,298,72,34);
   UiFactory.Text(pourButton.transform,pourButtonHint,155,103,298,30,16);
   var input=pourButton.gameObject.AddComponent<PourPressButton>();input.Begin=()=>BeginPourPress(Tilt.Modifier);input.End=EndPourPress;input.Cancel=CancelPourPress;
   selectButton.GetComponentInChildren<TextMeshProUGUI>().fontSize=38;pourButton.GetComponentInChildren<TextMeshProUGUI>().fontSize=34;
   status=null;
   if(DebugMode){
    var debug=UiFactory.Box(board,"Debug panel",540,825,1030,135,UiFactory.Grey).rectTransform;
    status=UiFactory.Text(debug,"",515,24,1000,30,20);
    UiFactory.Text(debug,"A / SELECT   SPACE / POUR&ON   SHIFT / TILT   F1 / CLEAN",515,55,1000,25,18);
    UiFactory.Button(debug,hand.Left?"LEFT HAND":"RIGHT HAND",170,98,300,45,()=>{if(!busy){hand.Toggle();DrawGameplay();}});
    UiFactory.Button(debug,"RECALIBRATE",515,98,300,45,()=>Tilt.Calibrate());
    UiFactory.Button(debug,session.Demo?"PLAY L1":"PRACTICE",860,98,300,45,()=>StartSession(!session.Demo));
   }
   if(Time.time<noticeUntil&&notice!="")UiFactory.Text(effects,notice,300,885,420,72,42,UiFactory.Ink);
  }
  void Update(){
   ApplyHandToTilt();
   Fit();
   if(Input.GetKeyDown(KeyCode.F1)&&!busy){DebugMode=!DebugMode;Redraw();}
   if(mode==ScreenMode.MainMenu){AnimateTitle();if(Time.time>=noticeUntil&&notice!=""){notice="";Redraw();}return;}
   if(mode==ScreenMode.Settings){if(Time.time>=noticeUntil&&notice!=""){notice="";Redraw();}return;}
   if(mode==ScreenMode.Pause){if(Input.GetKeyDown(KeyCode.Escape))DrawGameplay();if(Time.time>=noticeUntil&&notice!=""){notice="";DrawPause();}return;}
   if(mode==ScreenMode.Result){UpdateResultButtons();return;}
   if(session==null)return;
   carousel.Duration=SelectDuration;carousel.Tick(Time.deltaTime);
   bool practice=session.Demo&&FreezePatienceInPreview;
   if(!practice)session.Tick(Time.deltaTime);
   if(session.Ended&&!busy&&!ending){ShowResult();return;}
   patience.Tick(pour.Orders,Time.deltaTime,practice);
   if(!busy && !practice && !resolvingMissed){bool[] expiredSlots=new bool[3];bool expired=false;for(int i=0;i<3;i++)if(!pour.Orders.Slots[i].Complete && patience.Expired(i)){expiredSlots[i]=true;expired=true;}
    if(expired){StartCoroutine(ResolveMissed(expiredSlots));return;}}
   timer.text="SALES "+session.PlayerSales+"     RIVAL "+session.RivalSales;
   if(status!=null)status.text=Time.time<noticeUntil?notice:
#if UNITY_IOS
    IsTiltPourMode?(PourInputActive?(Tilt.PourModifier?"POUR TILT ON":"POUR ARMED"):"TILT SELECT / HOLD POUR"):
#endif
    Tilt.Modifier?"TILT ON / BACK + HOLD":"UPRIGHT / SELECT + POUR";
   for(int i=0;i<3;i++)cards[i].Patience(patience.Fraction(i),patience.Seconds(i),practice,DebugMode);
   if(Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.RightArrow))SelectWithModifier(Tilt.Modifier);
   if(!PourInputActive&&!busy&&(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return))){BeginPourPress(Tilt.Modifier);keyboardPour=true;pourKey=Input.GetKeyDown(KeyCode.Space)?KeyCode.Space:KeyCode.Return;}
#if UNITY_IOS
   if(IsTiltPourMode)UpdateTiltPour();else UpdateClassicPour();
#else
   UpdateClassicPour();
#endif
   if(Input.GetKeyDown(KeyCode.Escape)&&!busy)DrawPause();
  }
  IEnumerator ResolveMissed(bool[] expiredSlots){
   resolvingMissed=true;busy=true;audioFx.Play("error");notice="";
   for(float t=0;t<OrderCardView.MissedDuration;t+=Time.deltaTime){
    for(int i=0;i<3;i++)if(expiredSlots[i])cards[i].ShowMissed(t);
    yield return null;
   }
   for(int i=0;i<3;i++)if(expiredSlots[i])cards[i].ShowMissed(OrderCardView.MissedDuration);
   yield return null;
   for(int i=0;i<3;i++)if(expiredSlots[i]){session.RecordMiss(currentConfig.BaseValueForSteps(pour.Orders.Slots[i].Recipe.Length));if(ShouldReplaceOrders())pour.Orders.ReplaceExpired(i);}
   patience.Sync(pour.Orders);DrawGameplay();
   busy=false;resolvingMissed=false;if(session.CustomersFinished)ShowResult();
  }
  IEnumerator EndLevelSafely(){ending=true;float wait=0;while(busy&&(resolvingMissed||resolvingChain||wait<.5f)){wait+=Time.deltaTime;yield return null;}StopAllCoroutines();CancelPourPress();resolvingChain=false;busy=false;ShowResult();}
  void UpdateClassicPour(){
   if(pourGesture.Active){
    var action=pourGesture.Tick(Time.unscaledDeltaTime,Tilt.Modifier);if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(310*pourGesture.Progress,7);
    if(action==PourAction.Release)ReleaseBuffer();
    else if(keyboardPour&&Input.GetKeyUp(pourKey))EndPourPress();
   }else if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(0,7);
  }
#if UNITY_IOS
  void UpdateTiltPour(){
   if(tiltPourGesture.Active){
    if(tiltPourGesture.Tick(Time.unscaledDeltaTime,Tilt.PourModifier))PourWithModifier(false);
    if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(310*tiltPourGesture.Progress,7);
    if(keyboardPour&&Input.GetKeyUp(pourKey))EndPourPress();
   }else if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(0,7);
  }
#endif
  public void SelectWithModifier(bool back){if(mode!=ScreenMode.Gameplay||busy||ending||session.Ended||PourInputActive)return;carousel.Step(back);audioFx.Play("select");DrawGameplay();StartCoroutine(Flash(selectButton));}
  IEnumerator Flash(Image target){target.color=UiFactory.Lime;yield return new WaitForSeconds(.14f);if(target!=null)target.color=UiFactory.Grey;}
  public void PourWithModifier(bool modifier){
   if(mode!=ScreenMode.Gameplay||busy||ending||session.Ended)return;
   var result=pour.Press(carousel.Selected,modifier);StartCoroutine(Flash(pourButton));
   if(!result.Success){audioFx.Play("error");StartCoroutine(Shake(result.Held?selectedView.Buffer:ring));return;}
   session.Record(result);StartCoroutine(AnimatePour(result));
  }
  public void BeginPourPress(bool modifier){
   if(mode!=ScreenMode.Gameplay||busy||ending||session.Ended||PourInputActive)return;
#if UNITY_IOS
   if(IsTiltPourMode){keyboardPour=false;tiltPourGesture.Begin(TiltPourActivationSeconds,ReleaseHoldSeconds);return;}
#endif
   keyboardPour=false;pourGesture.Begin(modifier,ReleaseHoldSeconds);
  }
  public void EndPourPress(){
#if UNITY_IOS
   if(IsTiltPourMode){
    var tiltAction=tiltPourGesture.End();keyboardPour=false;if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(0,7);
    if(tiltAction==PourAction.Release)ReleaseBuffer();else if(tiltAction==PourAction.Hold)PourWithModifier(true);
    return;
   }
#endif
   if(pourGesture.Active&&pourGesture.Tilted!=Tilt.Modifier){CancelPourPress();return;}
   var action=pourGesture.End();keyboardPour=false;
   if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(0,7);
   if(action==PourAction.Release)ReleaseBuffer();else if(action==PourAction.Pour||action==PourAction.Hold)PourWithModifier(action==PourAction.Hold);
  }
  public void CancelPourPress(){pourGesture.Cancel();
#if UNITY_IOS
   tiltPourGesture.Cancel();
#endif
   keyboardPour=false;if(releaseFill!=null)releaseFill.rectTransform.sizeDelta=new Vector2(0,7);}
  void OnApplicationFocus(bool focused){if(!focused)CancelPourPress();}
  void OnApplicationPause(bool paused){if(paused)CancelPourPress();}
  void OnDisable(){CancelPourPress();}
  public void ReleaseBuffer(){
   if(mode!=ScreenMode.Gameplay||busy||ending||session.Ended)return;
   if(pour.Buffer.Count<=0){StartCoroutine(Flash(pourButton));return;}
   ChainStep first;if(!pour.TryReleaseNext(out first)){audioFx.Play("error");StartCoroutine(Shake(selectedView.Buffer));return;}
   resolvingChain=true;busy=true;ActiveChainCount=0;StartCoroutine(AnimateChain(first));
  }
  bool ShouldReplaceOrders(){return session!=null&&(session.TotalCustomers<=0||session.ResolvedCustomers<session.TotalCustomers);}
  void ChainLabel(int count,bool summary=false,int completed=0,int bonus=0){
   string title=count==0?"RELEASE":"CHAIN "+count;
   if(summary){if(completed==3)title+=" / ALL CLEAR";else if(count>=5)title+=" / PERFECT FLOW";else if(count==4)title+=" / GREAT FLOW";else if(count==3)title+=" / GOOD FLOW";}
   var banner=UiFactory.Box(effects,"Chain banner",540,820,960,82,UiFactory.Paper).rectTransform;
   UiFactory.Text(banner,title,480,36,940,60,40,UiFactory.Ink);
   if(summary)UiFactory.Text(effects,"+"+(SessionController.FlowFor(completed)+bonus)+" FLOW",540,885,900,48,30,UiFactory.Ink);
  }
  IEnumerator AnimateChain(ChainStep step){
   int length=0,completed=0;ChainLabel(0);
   while(step!=null&&length<HoldBufferController.MaxSlots){
    int slot=System.Array.IndexOf(pour.Orders.Slots,step.Target);
    Vector2 start=SelectedItemView.SlotPosition(hand,step.BufferIndex),end=CupView.Mouth(slot);
    var dot=UiFactory.Rect(effects,"Chain ingredient "+step.Item,start.x,start.y,78,78);UiFactory.Icon(dot,step.Item,39,39,78);audioFx.Play("release",length+1);
    float flight=ChainStepSeconds*.65f;
    for(float t=0;t<flight;t+=Time.deltaTime){float u=Mathf.Clamp01(t/flight);Vector2 position=Vector2.Lerp(start,end,u)+Vector2.up*(-80*Mathf.Sin(Mathf.PI*u));dot.anchoredPosition=new Vector2(position.x,-position.y);yield return null;}
    length++;if(step.Completed)completed++;ActiveChainCount=length;
    DrawGameplay();cups[slot].Render(step.Target.Step,0);ChainLabel(length);
    for(float t=0;t<ChainStepSeconds-flight;t+=Time.deltaTime){cups[slot].Fill(Mathf.Clamp01(t/(ChainStepSeconds-flight)));yield return null;}
    cups[slot].Fill(1);
    ChainStep next;if(length<HoldBufferController.MaxSlots&&pour.TryReleaseNext(out next))step=next;else step=null;
   }
   int bonus=currentConfig.ChainBonus(length);session.RecordChain(length,completed,bonus);LastChainCount=length;
   int salesBonus=currentConfig.ChainSalesBonus(length);
   for(int i=0;i<3;i++)if(pour.Orders.Slots[i].Complete)session.RecordSales(currentConfig.BaseValueForSteps(pour.Orders.Slots[i].Recipe.Length),salesBonus);
   DrawGameplay();ChainLabel(length,true,completed,bonus);
   if(completed>0){audioFx.Play("complete",Mathf.Max(0,completed-1));yield return BounceCompletedCups();yield return new WaitForSeconds(.43f);}
   else{audioFx.Play("select");yield return new WaitForSeconds(.65f);}
   for(int i=0;i<3;i++)if(pour.Orders.Slots[i].Complete&&ShouldReplaceOrders())pour.Orders.Replace(i);
   patience.Sync(pour.Orders);DrawGameplay();ActiveChainCount=0;busy=false;resolvingChain=false;if(session.CustomersFinished)ShowResult();
  }
  IEnumerator Shake(RectTransform target){Vector2 start=target.anchoredPosition;for(float t=0;t<.22f;t+=Time.deltaTime){if(target==null)yield break;target.anchoredPosition=start+Vector2.right*Mathf.Sin(t*95)*9*(1-t/.22f);yield return null;}if(target!=null)target.anchoredPosition=start;}
  float CompletionBounceScale(float u){
   u=Mathf.Clamp01(u);
   if(u<.32f)return Mathf.Lerp(1f,1.08f,Mathf.SmoothStep(0,1,u/.32f));
   if(u<.62f)return Mathf.Lerp(1.08f,.97f,Mathf.SmoothStep(0,1,(u-.32f)/.30f));
   return Mathf.Lerp(.97f,1f,Mathf.SmoothStep(0,1,(u-.62f)/.38f));
  }
  IEnumerator BounceCompletedCups(){
   const float duration=.22f;
   for(float t=0;t<duration;t+=Time.deltaTime){float scale=CompletionBounceScale(t/duration);for(int i=0;i<3;i++)if(pour.Orders.Slots[i].Complete&&cups[i].Visual!=null)cups[i].Visual.localScale=Vector3.one*scale;yield return null;}
   for(int i=0;i<3;i++)if(pour.Orders.Slots[i].Complete&&cups[i].Visual!=null)cups[i].Visual.localScale=Vector3.one;
  }
  IEnumerator AnimatePour(PourResult r){
   busy=true;audioFx.Play(r.Held?"hold":r.Release?"release":"pour",r.HoldCount);
   int n=r.Held?1:r.Targets.Count;var dots=new RectTransform[n];var ends=new Vector2[n];var starts=new Vector2[n];var slots=new int[n];
   for(int j=0;j<n;j++){
    starts[j]=new Vector2(hand.X(835)+(r.Release?(j-1)*90:0),r.Release?1310:1045);
    slots[j]=r.Held?0:System.Array.IndexOf(pour.Orders.Slots,r.Targets[j]);
    ends[j]=r.Held?SelectedItemView.SlotPosition(hand,r.HoldCount-1):CupView.Mouth(slots[j])+Vector2.up*-45;
    dots[j]=UiFactory.Rect(effects,"Flying "+r.Item,starts[j].x,starts[j].y,78,78);UiFactory.Icon(dots[j],r.Item,39,39,78);
   }
   if(r.Release){UiFactory.Box(effects,"Release banner",540,915,1050,88,UiFactory.Lime);UiFactory.Text(effects,"RELEASE ALL",540,915,1000,64,48,UiFactory.Ink);}
   for(float t=0;t<.25f;t+=Time.deltaTime){float u=Mathf.Clamp01(t/.25f);float ease=u*u*(3-2*u);for(int j=0;j<n;j++){Vector2 pos=Vector2.Lerp(starts[j],ends[j],ease)+new Vector2(0,-Mathf.Sin(u*Mathf.PI)*150);dots[j].anchoredPosition=new Vector2(pos.x,-pos.y);}yield return null;}
   if(!r.Held){
    var streams=new RectTransform[n];var ripples=new RectTransform[n];
    for(int j=0;j<n;j++){
     cups[slots[j]].Render(r.Targets[j].Step,0);
     if(r.Item!=Ingredient.CUP){var mouth=CupView.Mouth(slots[j]);bool foam=r.Item==Ingredient.FOAM;streams[j]=UiFactory.Box(effects,"Pour stream",mouth.x,mouth.y+(foam?0:85),r.Release?16:10,foam?70:240,foam?Color.white:UiFactory.ColorOf(r.Item)).rectTransform;}
     if(r.Release){var mouth=CupView.Mouth(slots[j]);ripples[j]=UiFactory.Box(effects,"Release ripple",mouth.x,mouth.y+90,170,170,UiFactory.Lime,true,true).rectTransform;}
    }
    for(float t=0;t<.22f;t+=Time.deltaTime){float u=Mathf.Clamp01(t/.22f);for(int j=0;j<n;j++){cups[slots[j]].Fill(u);if(streams[j]!=null)streams[j].localScale=new Vector3(1,Mathf.Sin(Mathf.PI*u),1);if(ripples[j]!=null)ripples[j].localScale=Vector3.one*(1+u*.6f);}yield return null;}
   }
   DrawGameplay();
   int reward=SessionController.FlowFor(r.Completed);
   RectTransform banner=null;
   if(reward>0){audioFx.Play("complete",Mathf.Max(0,r.Completed-1));banner=UiFactory.Box(effects,"Flow reward",540,915,1050,88,UiFactory.Lime).rectTransform;
    string title=r.Completed==3?"TRIPLE FLOW  +450 / ALL CLEAR":r.Completed==2?"DOUBLE FLOW  +250":"+100 FLOW";
    UiFactory.Text(banner,title,525,44,1030,84,r.Completed==3?37:44);
   }
   if(r.Completed>0){
    yield return BounceCompletedCups();
    bool[] replaced=new bool[3];for(int i=0;i<3;i++)replaced[i]=pour.Orders.Slots[i].Complete;
    for(float t=0;t<.15f;t+=Time.deltaTime){for(int i=0;i<3;i++)if(replaced[i]){float x=190+350*i+t/.15f*1150;cards[i].Root.anchoredPosition=new Vector2(x,-245);cups[i].Root.anchoredPosition=new Vector2(x,-610);}yield return null;}
    for(int i=0;i<3;i++)if(replaced[i]){session.RecordSales(currentConfig.BaseValueForSteps(pour.Orders.Slots[i].Recipe.Length),0);if(ShouldReplaceOrders())pour.Orders.Replace(i);}patience.Sync(pour.Orders);DrawGameplay();
    for(float t=0;t<.15f;t+=Time.deltaTime){for(int i=0;i<3;i++)if(replaced[i]){float x=190+350*i-(1-t/.15f)*1150;cards[i].Root.anchoredPosition=new Vector2(x,-245);cups[i].Root.anchoredPosition=new Vector2(x,-610);}yield return null;}
    for(int i=0;i<3;i++){cards[i].Root.anchoredPosition=new Vector2(190+350*i,-245);cups[i].Root.anchoredPosition=new Vector2(190+350*i,-610);}
   }
   busy=false;if(session.CustomersFinished)ShowResult();
  }
  void ShowResult(){
   mode=ScreenMode.Result;ending=false;ClearBoard();
   bool cleared=session.Demo||session.PlayerSales>session.RivalSales;
#if UNITY_ANDROID || UNITY_EDITOR
   if(cleared&&!session.Demo)TryCompleteProgressionTrial();
#endif
   UiFactory.Box(board,"result accent",540,20,1080,40,UiFactory.Lime);
   UiFactory.RoundBox(board,"result card",540,890,1000,720,Color.white);
   UiFactory.LimeSliceIcon(board,120,375,110);UiFactory.CherryIcon(board,960,375,120);
   UiFactory.Text(board,"POUR CYCLE™",540,265,980,90,58);
   UiFactory.Text(board,session.Demo?"PRACTICE":"LEVEL "+currentLevel,540,365,980,70,46);
   UiFactory.Text(board,cleared?"LEVEL CLEAR":"NOT CLEARED",540,470,980,90,58,UiFactory.Ink);
   if(cleared){UiFactory.Text(board,"DRINKS SERVED",540,570,420,42,24,UiFactory.Ink);UiFactory.Icon(board,Ingredient.BLUE,390,645,118);UiFactory.Icon(board,Ingredient.RED,540,645,118);UiFactory.Icon(board,Ingredient.PINK,690,645,118);}
   string body="YOUR SALES   "+session.PlayerSales+"\n\nRIVAL SALES   "+session.RivalSales+"\n\nORDERS SERVED   "+session.OrdersServed+"\n\nCUSTOMERS LOST   "+session.CustomersLost+"\n\nBEST CHAIN   "+session.BestRelease+"\n\nALL CLEAR   "+session.AllClears;
   UiFactory.Text(board,body,540,975,900,520,35);
   bool hasNext=cleared&&!session.Demo;resultButtons=new PointerDownButton[hasNext?2:1];int buttonIndex=0;
   if(hasNext){var next=UiFactory.Button(board,"NEXT LEVEL",540,1375,360,125,()=>StartLevel(currentLevel+1));next.color=UiFactory.Lime;next.GetComponentInChildren<TextMeshProUGUI>().fontSize=36;resultButtons[buttonIndex++]=next.GetComponent<PointerDownButton>();}
   var title=UiFactory.Button(board,"TITLE",540,hasNext?1545:1485,360,125,()=>ShowHome());title.color=new Color(.89f,.89f,.83f);title.GetComponentInChildren<TextMeshProUGUI>().fontSize=36;resultButtons[buttonIndex++]=title.GetComponent<PointerDownButton>();
   LockResultButtons();
  }
#if UNITY_ANDROID || UNITY_EDITOR
  bool TryCompleteProgressionTrial(){
   if(currentLevel!=5||session==null||session.Demo||session.PlayerSales<=session.RivalSales)return false;
   if(trialCompletionRequested)return true;
   PlayTrialStatus status=playTrialService!=null?playTrialService.CheckStatus():PlayTrialStatus.Unknown;
   if(status!=PlayTrialStatus.UnderTrial)return false;
   trialCompletionRequested=true;PlayTrialService.Log("[PlayTrial] LEVEL 5 cleared; stopTrial requested");
   if(playTrialService.TryStopTrial()){PlayTrialService.Log("[PlayTrial] stopTrial completed");return true;}
   return false;
  }
#endif
  bool PointerHeld(){return Input.touchCount>0||Input.GetMouseButton(0);}
  void LockResultButtons(){resultInputUnlockAt=Time.unscaledTime+.4f;resultWaitForPointerRelease=PointerHeld();SetResultButtons(false);}
  void SetResultButtons(bool interactable){if(resultButtons==null)return;for(int i=0;i<resultButtons.Length;i++)if(resultButtons[i]!=null)resultButtons[i].enabled=interactable;}
  void UpdateResultButtons(){
   if(resultButtons==null||resultButtons.Length==0||resultButtons[0]==null||resultButtons[0].enabled)return;
   if(resultWaitForPointerRelease){if(PointerHeld())return;resultWaitForPointerRelease=false;}
   if(Time.unscaledTime<resultInputUnlockAt)return;
   SetResultButtons(true);
  }
 }
}
























