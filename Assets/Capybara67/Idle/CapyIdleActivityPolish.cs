using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  Text basketSeries,searchInstruction;
  Button searchHelp,musicRepeat,brushButton;
  readonly RectTransform[] memoryFaces=new RectTransform[6],noteInstruments=new RectTransform[4],musicSteps=new RectTransform[4],paintPatterns=new RectTransform[6];
  readonly SoftShape[] musicStepFill=new SoftShape[4];
  readonly bool[] memoryVisible=new bool[6],memoryKnown=new bool[6];
  readonly int[] paintPatternShown={-1,-1,-1,-1,-1,-1};
  readonly Tween[] memoryFlips=new Tween[6];
  readonly Button[] picnicBaskets=new Button[3];
  RectTransform picnicFlight,paintEyes,searchGuide;
  CapyCareGesture picnicDrag;
  readonly RectTransform[] picnicBasketArt=new RectTransform[3],picnicGuides=new RectTransform[3],picnicChecks=new RectTransform[3],melodyChecks=new RectTransform[4],paintGuides=new RectTransform[6];
  RectTransform brushSample;
  Text paintColourConfirmation;
  int brushSampleShown=-1,picnicHintGroup=-1,musicCompletedSteps;
  double musicSuccessUntil,paintLastAction,paintFeedbackUntil;
  int picnicFlyingGroup;
  double picnicFlightAt=-100;
  bool picnicDragging;

  void BasketPoint(PointerEventData e,Slider slider){
   if(PresentationPaused||Page!="activity-play")return;
   if(RectTransformUtility.ScreenPointToLocalPointInRectangle(activityField,e.position,e.pressEventCamera,out var local)){
    float x=Mathf.Clamp01((local.x-activityField.rect.xMin-25)/382f);Model.ActivityBasket(x);slider.SetValueWithoutNotify(x);RefreshActivity();
   }
  }
  void BuildBasketTouch(Slider slider){
   var surface=Shape(activityField,"Touch the meadow",0,0,432,444,"FFFFFF00",0);surface.raycastTarget=true;surface.transform.SetAsFirstSibling();
   var gesture=surface.gameObject.AddComponent<CapyCareGesture>();gesture.begin=e=>{if(PresentationPaused)return false;BasketPoint(e,slider);return true;};gesture.move=e=>BasketPoint(e,slider);
   var trigger=surface.gameObject.AddComponent<EventTrigger>();var entry=new EventTrigger.Entry{eventID=EventTriggerType.PointerDown};entry.callback.AddListener(e=>BasketPoint((PointerEventData)e,slider));trigger.triggers.Add(entry);
  }
  static void MorePicnicFood(Transform r,int group){
   if(group==0){
    Oval(r,"Apple shadow",8,60,65,14,"8A61442A",0);
    var apple=Rounded(0,42,26,10,79,3,120,23,164,3,215,14,240,45,229,110,194,147,150,145,120,139,79,148,40,133,13,99);
    Contour(r,"Whole apple outline",7,16,67,55,Ink,apple);Contour(r,"Whole apple flesh",10,18,61,50,"E88476",apple);
    Shape(r,"Apple shine",17,25,10,19,"FFC5A0",0,true);Stroke(r,"Apple dimple",33,24,45,25,"BB5450",2);Stroke(r,"Apple stem",39,23,45,5,Ink,3);Leaf(r,44,5,27,15,"92B56E",-15);
   }else if(group==1){
    var cucumber=Oval(r,"Cucumber",18,9,39,65,"87B174",3);cucumber.rectTransform.localEulerAngles=new Vector3(0,0,-24);
    Stroke(r,"Cucumber highlight",32,18,29,53,"CDE3A6",4);for(int n=0;n<9;n++)Oval(r,"Cucumber dot",27+(n*11)%22,22+(n*13)%39,3,4,"567E50",0);Leaf(r,45,1,26,13,"689957",12);
   }else{
    for(int n=0;n<5;n++){float x=9+(n%3)*20,y=23+(n/3)*22;Oval(r,"Blueberry",x,y,29,29,n%2==0?"8395C0":"A2A9CE",2);Star(r,x+10,y+4,10,"596D9A");Shape(r,"Berry glint",x+5,y+7,7,5,"E7E6F7",0,true);}Leaf(r,27,7,35,18,"83AB78",-15);
   }
  }
  void TurnMemoryCard(int n,bool visible){
   if(memoryKnown[n]&&memoryVisible[n]==visible)return;
   bool animate=memoryKnown[n]&&!Model.S.calm;memoryKnown[n]=true;memoryVisible[n]=visible;
   if(memoryFlips[n]!=null&&memoryFlips[n].IsActive())memoryFlips[n].Kill();
   var face=memoryFaces[n];face.localScale=Vector3.one;
   if(!animate){memoryFront[n].gameObject.SetActive(visible);memoryBack[n].gameObject.SetActive(!visible);return;}
   var seq=DOTween.Sequence().Append(face.DOScaleX(.06f,.1f)).AppendCallback(()=>{memoryFront[n].gameObject.SetActive(visible);memoryBack[n].gameObject.SetActive(!visible);}).Append(face.DOScaleX(1,.16f).SetEase(Ease.OutBack));
   memoryFlips[n]=tweens.Track(seq,face.gameObject);
  }
  static void Marimba(Transform p,int note){
   InkBox(p,"Instrument cradle",28,14,116,70,"8D6148",14,3);Shape(p,"Cradle velvet",34,19,104,57,"553F45",10);
   for(int n=0;n<3;n++){
    float x=40+n*31,y=20+n*3;InkBox(p,"Wooden tone bar",x,y,25,52-n*4,PaintColours[note],6,2);Stroke(p,"Bar grain",x+7,y+12,x+7,y+32,"FFF1CB",1.5f);
    Oval(p,"Tuning pin",x+9,y+5,6,6,"B58C69",1);Oval(p,"Tuning pin",x+9,y+39-n*4,6,6,"B58C69",1);
   }
   Stroke(p,"Resting mallet",31,74,120,29,"6A4933",5);Stroke(p,"Mallet highlight",33,73,115,31,"F0D29E",2);Oval(p,"Felt mallet",110,18,23,23,"FFF1CA",2);
  }
  void SubmitPicnic(int group,Vector2 from){
   var a=Model.S.activity;if(a==null||a.complete||PresentationPaused||a.elapsed<a.readyAt)return;
   int oldGroup=IdleModel.PicnicGroup(a),oldVariant=a.variant+a.score;
   if(Model.ActivitySort(group)){
    picnicFlyingGroup=group;picnicFlightAt=a.elapsed;picnicHintGroup=-1;
    if(!Model.S.calm){Clear(picnicFlight);PicnicFood(picnicFlight,oldGroup,oldVariant,112);picnicFlight.gameObject.SetActive(true);picnicFlight.anchoredPosition=from;picnicFlightStart=from;picnicFlight.SetAsLastSibling();}
    HarvestFeedback(new Vector2(73+143*group,-345));Sound(feedClip,.14f);
   }else{picnicHintGroup=oldGroup;PlayroomPop(picnicFoodArt);Sound(petClip,.08f);}
   Save();RefreshPlayroom();
  }
  Vector2 picnicFlightStart;
  void BuildPicnicDrag(){
   var touch=Shape(picnicPlate,"Drag the food",0,0,112,112,"FFFFFF00",0);touch.raycastTarget=true;touch.transform.SetAsFirstSibling();
   picnicDrag=touch.gameObject.AddComponent<CapyCareGesture>();
   picnicDrag.begin=e=>{var a=Model.S.activity;if(PresentationPaused||a.complete||a.elapsed<a.readyAt)return false;picnicDragging=true;return true;};
   picnicDrag.move=e=>{if(PresentationPaused){picnicDrag.Cancel();return;}if(RectTransformUtility.ScreenPointToLocalPointInRectangle(activityField,e.position,e.pressEventCamera,out var local))picnicPlate.anchoredPosition=local-new Vector2(56,-56);};
   picnicDrag.finish=e=>{picnicDragging=false;int group=-1;for(int n=0;n<3;n++)if(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)picnicBaskets[n].transform,e.position,e.pressEventCamera))group=n;var from=picnicPlate.anchoredPosition;if(group>=0)SubmitPicnic(group,from);else RefreshPlayroom();};
   picnicDrag.cancelled=()=>{picnicDragging=false;if(picnicPlate)picnicPlate.anchoredPosition=new Vector2(158,-82);};
  }
  static RectTransform PlayroomCheck(Transform parent,string name,float x,float y,float size){
   var r=Rect(parent,name,x,y,size,size);Oval(r,"Check background",0,0,size,size,"E6F1CC",1.5f);
   Stroke(r,"Check short",size*.24f,size*.51f,size*.43f,size*.70f,"447A56",2.5f);
   Stroke(r,"Check long",size*.43f,size*.70f,size*.77f,size*.29f,"447A56",2.5f);return r;
  }
  static int PaintTheme(IdleActivity a)=>a.patternVersion>=2?a.variant:0;
  static Rect PaintRegion(IdleActivity a,int n){
   int theme=PaintTheme(a);
   if(theme==1)return new Rect(n%2==0?82:235,118+n/2*85,115,102-n/2*12);
   if(theme==2)return new Rect(106+n%3*75,151+n/3*85,74,88);
   float angle=(-90+n*60)*Mathf.Deg2Rad;return new Rect(169+Mathf.Cos(angle)*87,164+Mathf.Sin(angle)*80,94,96);
  }
  static void PaintPattern(Transform p,int style,float w,float h){
   if(style==1)for(int n=0;n<7;n++){float a=n*Mathf.PI*2/7;Shape(p,"Painted dot",w*.46f+Mathf.Cos(a)*w*.25f,h*.46f+Mathf.Sin(a)*h*.25f,8,8,"FFF6D9",0,true);}
   if(style==2)for(int n=0;n<3;n++)Stroke(p,"Painted stripe",w*.27f,h*(.3f+n*.2f),w*.73f,h*(.3f+n*.2f),"FFF0D2",5);
  }
  static void PaintingBase(Transform p,IdleActivity a){
   InkBox(p,"Canvas frame",20,15,392,393,"DAB489",12,3);Shape(p,"Cotton canvas",29,24,374,375,"FFFAE8",8);
   for(int n=0;n<4;n++){float x=n%2==0?37:389,y=n<2?32:384;Oval(p,"Brass frame pin",x,y,7,7,"E7BD76",1);}
   int theme=PaintTheme(a);
   if(theme==0){Stroke(p,"Flower stem",215,231,215,371,"6D9C6B",11);Leaf(p,216,313,89,34,"B3D294",-14);Leaf(p,130,292,87,37,"87B78B",9);}
   else if(theme==1){for(int n=0;n<5;n++)ActivityFlower(p,58+n*73,360+n%2*9,1.1f,PaintColours[n]);Stroke(p,"Butterfly antenna",208,162,188,121,Ink,3);Stroke(p,"Butterfly antenna",224,162,246,121,Ink,3);Oval(p,"Antenna tip",180,113,13,13,"F0CB7E",2);Oval(p,"Antenna tip",240,113,13,13,"F0CB7E",2);}
   else{Oval(p,"Shell shadow",79,340,308,28,"D7DDAD",0);Oval(p,"Turtle foot",107,322,58,48,"91BB8A",2);Oval(p,"Turtle foot",258,322,58,48,"91BB8A",2);Oval(p,"Turtle tail",70,242,53,27,"91BB8A",2);Oval(p,"Turtle head",326,218,71,72,"B0CF98",3);Oval(p,"Turtle shell rim",90,131,250,222,"759C72",3);}
  }
  static RectTransform PaintingFace(Transform p,IdleActivity a){
   int theme=PaintTheme(a);RectTransform face;
   if(theme==2){face=Rect(p,"Turtle face",328,220,67,68);Oval(face,"Blush",10,39,16,10,"EBA99B",0);Stroke(face,"Smile",37,45,45,47,Ink,2);}
   else{if(theme==1)Oval(p,"Butterfly body",198,201,36,137,"D9AD74",2);face=Rect(p,"Paint friend face",176,171,80,81);if(theme==1){face.anchoredPosition=new Vector2(184,-157);face.localScale=Vector3.one*.8f;}Oval(face,"Flower centre",0,0,80,81,"F4CF78",3);Oval(face,"Blush",10,43,14,9,"EDAB8C",0);Oval(face,"Blush",57,43,14,9,"EDAB8C",0);Stroke(face,"Smile",29,53,40,58,Ink,2);Stroke(face,"Smile",40,58,52,51,Ink,2);}
   var eyes=Rect(face,"Blinking eyes",0,0,80,81);Oval(eyes,"Happy eye",19,28,6,8,Ink,0);Oval(eyes,"Happy eye",54,28,6,8,Ink,0);return eyes;
  }
 }
 // Own only the short playroom response. Never cancel button hover/press tweens.
 public sealed class CapyPlayroomPopMotion : MonoBehaviour {
  Tween pop;
  public void Play(CapyTweenDriver driver,bool calm){
   Cancel();if(calm||!driver||!isActiveAndEnabled)return;
   transform.localScale=Vector3.one*.92f;
   pop=driver.Track(transform.DOScale(1,.25f).SetEase(Ease.OutBack),gameObject);
  }
  void Cancel(){if(pop!=null&&pop.IsActive())pop.Kill();pop=null;transform.localScale=Vector3.one;}
  void OnDisable(){Cancel();}
  void OnDestroy(){Cancel();}
 }

}
