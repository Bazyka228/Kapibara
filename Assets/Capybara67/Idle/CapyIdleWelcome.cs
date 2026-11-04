using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  RectTransform welcomePose;
  CapyPortraitMotion welcomeEyes;
  float welcomeAge,welcomePetAge=10;
  CanvasGroup welcomeHeart;
  Tween welcomeHeartTween;
  bool welcomeOfflinePending,welcomeGiftPending;
  bool welcomeBirthPending,welcomeBirthActive;
  RectTransform welcomeBirthLeft,welcomeBirthRight;
  CanvasGroup welcomeBirthSparkles;
  Text welcomeBirthCaption;
  InputField welcomeNameInput;
  Text welcomeNameCount;
  bool welcomeNameActive;
  public bool WelcomeNameInputFocused=>Page=="welcome"&&welcomeNameActive&&welcomeNameInput&&welcomeNameInput.isFocused;
  const float WelcomeBirthDuration=3.4f;

  void WelcomePage(){
   welcomeAge=0;welcomePetAge=10;
   welcomeNameInput=null;welcomeNameCount=null;welcomeNameActive=false;
   welcomeBirthActive=welcomeBirthPending&&!Model.S.birthIntroSeen;
   welcomeBirthPending=false;
   welcomeBirthLeft=welcomeBirthRight=null;welcomeBirthCaption=null;welcomeBirthSparkles=null;
   if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();
   welcomeHeart=null;welcomePose=null;welcomeEyes=null;
   if(welcomeBirthActive){
    // Commit at entry: closing/reloading during the scene must not replay it.
    // This flag never grants or replaces the existing starter bowl.
    Model.S.birthIntroSeen=true;Model.S.birthNamePending=true;Save();WelcomeBirthPage();return;
   }
   if(Model.S.birthNamePending){WelcomeNamePage();return;}
   var sky=Rect(panel,"Welcome sky",0,0,480,850).gameObject.AddComponent<CapySceneryGradient>();
   sky.top=sky.bottom=C("A9DAD0");sky.raycastTarget=false;
   var garden=Rect(panel,"Welcome garden",0,-111,480,850);DrawScene(garden,null);
   // Keep this entrance garden sunny even when the saved room is a night scene.
   Leaf(panel,7,52,39,68,"77A653",-23);Leaf(panel,439,49,37,70,"96BC79",23);
   WoodPanel(panel,"Welcome title board",28,73,424,143);
   Label(panel,"Welcome eyebrow","ТВОЙ УЮТНЫЙ САД",38,27,404,37,22,"513821",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome title","КАПИБАРА",42,86,396,69,46,"FFF0C8",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome subtitle","6–7 · ЛОВИ ВАЙБ",46,154,388,45,28,"FFF0C8",TextAnchor.MiddleCenter).font=bold;
   Stroke(panel,"Sign cord",66,0,66,73,"7F6247",3);Stroke(panel,"Sign cord",414,0,414,73,"7F6247",3);
   var portrait=Rect(panel,"Welcome capybara",120,300,240,250);DrawAvatar(portrait);
   welcomePose=portrait.Find("Illustrated pose") as RectTransform;
   welcomeEyes=portrait.GetComponentInChildren<CapyPortraitMotion>();
   var heart=Rect(portrait,"Welcome heart",211,53,40,44);EffectMotif(heart,2,"EBA2AD");
   welcomeHeart=heart.gameObject.AddComponent<CanvasGroup>();welcomeHeart.alpha=0;welcomeHeart.blocksRaycasts=false;
   Button(panel,"Welcome pet","",82,301,316,250,()=>{
    welcomePetAge=0;if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();welcomeHeart.alpha=1;
    if(!Model.S.calm)welcomeHeartTween=tweens.Track(DOTween.Sequence().AppendInterval(.65f).Append(welcomeHeart.DOFade(0,.4f)),heart.gameObject);
    Sound(careClip,.1f);
   },"FFFFFF00");
   Flower(panel,29,537,"E8AF9B",1.3f);Flower(panel,424,534,"FFF2C7",1.25f);
   var name=Label(panel,"Welcome pet name",Model.S.petName,50,238,380,52,28,"513821",TextAnchor.MiddleCenter);name.font=bold;name.supportRichText=false;name.resizeTextForBestFit=true;name.resizeTextMinSize=18;name.resizeTextMaxSize=28;
   PaperCard(panel,"Welcome invitation",20,581,440,244);
   Label(panel,"Welcome promise","Корми · заботься · наряжай",36,597,408,42,24,"513821",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome progress",Model.S.starter?IdleModel.Stages[Model.Stage-1]+" · этап "+Model.Stage+" / 5":"Первая миска — в подарок",36,640,408,37,22,"72543A",TextAnchor.MiddleCenter);
   Button(panel,"Welcome start",Model.S.starter?"ПРОДОЛЖИТЬ":"НАЧАТЬ ЗАБОТУ",40,693,400,76,BeginFromWelcome,"55E6C1",26);
   Label(panel,"Welcome save note","Твой сад сохраняется автоматически",31,781,418,34,22,"72543A",TextAnchor.MiddleCenter);
   if(!Model.S.calm){pageOpacity.alpha=.75f;pageTween=tweens.Track(pageOpacity.DOFade(1,.35f).SetEase(Ease.OutQuad),panel.gameObject);}
  }
  void BeginFromWelcome(){
   if(Page!="welcome")return;
   // Both the scene button and its skip lead to naming; this step survives reload.
   if(Model.S.birthNamePending){if(!welcomeNameActive)Show("welcome");return;}
   welcomeBirthActive=false;
   if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();
   Show(welcomeOfflinePending?"offline":"home");
  }
  void WelcomeNamePage(){
   welcomeNameActive=true;
   var sky=Rect(panel,"Name garden sky",0,0,480,850).gameObject.AddComponent<CapySceneryGradient>();sky.top=C("B9DDD2");sky.bottom=C("F4E8BA");sky.raycastTarget=false;
   var garden=Rect(panel,"Name familiar garden",0,-111,480,850);DrawScene(garden,null);
   PaperCard(panel,"Name invitation",20,31,440,797);
   Label(panel,"Name title","Как зовут малыша?",42,51,396,65,32,"513821",TextAnchor.MiddleCenter).font=bold;
   var portrait=Rect(panel,"Welcome capybara",120,111,240,250);DrawAvatar(portrait,null,true,1,true);
   welcomePose=portrait.Find("Illustrated pose") as RectTransform;
   Label(panel,"Name hint","Придумай имя для друга",40,369,400,46,24,"72543A",TextAnchor.MiddleCenter);
   InkBox(panel,"Name field outline",39,432,402,86,"C1A477",14,2);
   var fieldRect=Rect(panel,"Birth name input",44,437,392,76);
   var fieldImage=fieldRect.gameObject.AddComponent<Image>();fieldImage.color=C("FFF9E8");
   welcomeNameInput=fieldRect.gameObject.AddComponent<InputField>();welcomeNameInput.targetGraphic=fieldImage;
   var inputText=Label(fieldRect,"Name text","",14,2,364,72,27,"513821");inputText.supportRichText=false;inputText.font=bold;inputText.horizontalOverflow=HorizontalWrapMode.Overflow;
   var placeholder=Label(fieldRect,"Name placeholder","Например, Капи",14,2,364,72,27,"A38A69");placeholder.supportRichText=false;
   welcomeNameInput.textComponent=inputText;welcomeNameInput.placeholder=placeholder;
   welcomeNameInput.lineType=InputField.LineType.SingleLine;welcomeNameInput.characterLimit=IdleModel.PetNameLimit;
   welcomeNameInput.contentType=InputField.ContentType.Custom;welcomeNameInput.characterValidation=InputField.CharacterValidation.None;
   welcomeNameInput.keyboardType=TouchScreenKeyboardType.Default;
   welcomeNameInput.onValidateInput=(text,index,c)=>IdleModel.IsPetNameCharacter(c)?c:'\0';
   welcomeNameCount=Label(panel,"Name length","",319,523,111,39,23,"72543A",TextAnchor.MiddleRight);
   Label(panel,"Name alphabet","Буквы, цифры, пробел и дефис",43,562,394,44,22,"72543A",TextAnchor.MiddleCenter);
   welcomeNameInput.onValueChanged.AddListener(UpdateWelcomeNameDraft);
   welcomeNameInput.onEndEdit.AddListener(_=>{if(welcomeNameActive&&Page=="welcome")Save();});
   string draft=Model.S.petName==IdleModel.DefaultPetName?"":Model.S.petName;
   welcomeNameInput.SetTextWithoutNotify(draft);UpdateWelcomeNameDraft(draft);
   Button(panel,"Birth name confirm","ЭТО МОЙ ДРУГ!",40,627,400,76,()=>CompleteWelcomeName(false),"55E6C1",26);
   Button(panel,"Birth name later","ОСТАВИТЬ КАПИ",40,718,400,72,()=>CompleteWelcomeName(true),"FFFFFF00",24);
   // Do not focus automatically: mobile keyboards open only after a deliberate tap.
  }
  void UpdateWelcomeNameDraft(string value){
   if(!welcomeNameActive||!welcomeNameInput)return;
   string draft=IdleModel.FilterPetNameInput(value);
   if(welcomeNameInput.text!=draft)welcomeNameInput.SetTextWithoutNotify(draft);
   Model.S.petName=IdleModel.NormalizePetName(draft);
   if(welcomeNameCount)welcomeNameCount.text=draft.Length+" / "+IdleModel.PetNameLimit;
  }
  void CompleteWelcomeName(bool later){
   if(Page!="welcome"||!welcomeNameActive||!Model.S.birthNamePending||PresentationPaused)return;
   Model.S.petName=later?IdleModel.DefaultPetName:IdleModel.NormalizePetName(welcomeNameInput?welcomeNameInput.text:null);
   Model.S.birthNamePending=false;welcomeNameActive=false;
   if(welcomeNameInput)welcomeNameInput.DeactivateInputField();
   Save();BeginFromWelcome();
  }
  void WelcomeBirthPage(){
   var sky=Rect(panel,"Birth morning sky",0,0,480,850).gameObject.AddComponent<CapySceneryGradient>();
   sky.top=C("B9DDD2");sky.bottom=C("F4E8BA");sky.raycastTarget=false;
   var garden=Rect(panel,"Birth familiar garden",0,-111,480,850);DrawScene(garden,null);
   Shape(panel,"Birth sun halo",165,72,150,150,"FFF5CF55",0,true);
   Oval(panel,"Birth morning sun",199,104,82,82,"F8DB8D",2);
   Leaf(panel,13,50,34,63,"83AF65",-25);Leaf(panel,434,48,32,65,"96BC79",25);
   Label(panel,"Birth eyebrow","ОДНАЖДЫ В ТВОЁМ САДУ…",33,31,414,42,23,"513821",TextAnchor.MiddleCenter).font=bold;
   PaperCard(panel,"Birth title paper",35,200,410,105);
   Label(panel,"Birth title","Появился малыш!",47,214,386,72,34,"513821",TextAnchor.MiddleCenter).font=bold;
   // The existing newborn art remains untouched; only the surrounding leaves open.
   Shape(panel,"Birth cradle shadow",92,539,296,29,"65432D22",0,true);
   Oval(panel,"Birth cradle rim",93,442,294,115,"A7BD78",3);
   Oval(panel,"Birth cradle linen",106,447,268,95,"FFF1CB",2);
   for(int n=0;n<7;n++)Stroke(panel,"Cradle stitch",125+n*37,522,132+n*37,531,"D4B884",2);
   var portrait=Rect(panel,"Welcome capybara",120,292,240,250);DrawAvatar(portrait,null,true,1,true);
   welcomePose=portrait.Find("Illustrated pose") as RectTransform;
   welcomeEyes=portrait.GetComponentInChildren<CapyPortraitMotion>();
   var shadow=portrait.Find("Ground shadow");if(shadow)shadow.gameObject.SetActive(false);
   welcomeBirthLeft=Rect(panel,"Birth opening left leaves",92,405,150,145);
   welcomeBirthRight=Rect(panel,"Birth opening right leaves",238,405,150,145);
   BirthLeafCover(welcomeBirthLeft,false);BirthLeafCover(welcomeBirthRight,true);
   Flower(panel,65,548,"E8AF9B",1.1f);Flower(panel,398,548,"FFF2C7",1.1f);
   var sparkles=Rect(panel,"Birth gentle sparkles",92,350,296,174);
   welcomeBirthSparkles=sparkles.gameObject.AddComponent<CanvasGroup>();welcomeBirthSparkles.alpha=0;welcomeBirthSparkles.blocksRaycasts=false;
   EffectMotif(Rect(sparkles,"Birth heart",128,0,40,44),2,"EBA2AD");
   EffectMotif(Rect(sparkles,"Birth sparkle left",18,44,25,28),0,"F3C56E");
   EffectMotif(Rect(sparkles,"Birth sparkle right",251,24,25,28),0,"F3C56E");
   PaperCard(panel,"Birth invitation",20,584,440,261);
   welcomeBirthCaption=Label(panel,"Birth caption","Тихо… кто-то просыпается",33,595,414,54,25,"513821",TextAnchor.MiddleCenter);welcomeBirthCaption.font=bold;
   Label(panel,"Welcome progress","Ему нужны забота и друг",37,649,406,36,23,"72543A",TextAnchor.MiddleCenter);
   Button(panel,"Welcome start","ПОЗНАКОМИТЬСЯ",40,696,400,76,BeginFromWelcome,"55E6C1",26);
   Button(panel,"Birth skip","ПРОПУСТИТЬ",135,773,210,67,BeginFromWelcome,"FFFFFF00",22);
   // Normal animation has a short anticipation, then unfolds without a timed gate.
   // Calm mode reveals the baby immediately and retains all controls.
   AnimateWelcomeBirth();
  }
  void BirthLeafCover(Transform p,bool right){
   Leaf(p,right?14:8,12,124,123,right?"95B86D":"83AA63",right?20:-20);
   Leaf(p,right?24:2,64,111,77,right?"B0C780":"9FBD76",right?42:-42);
  }
  void AnimateWelcomeBirth(){
   if(!welcomeBirthActive)return;
   float t=Model.S.calm?1:Mathf.Clamp01((welcomeAge-.45f)/2.35f);
   float ease=t*t*(3-2*t);
   if(welcomeBirthLeft){welcomeBirthLeft.anchoredPosition=new Vector2(92-78*ease,-405-42*ease);welcomeBirthLeft.localEulerAngles=new Vector3(0,0,27*ease);}
   if(welcomeBirthRight){welcomeBirthRight.anchoredPosition=new Vector2(238+78*ease,-405-42*ease);welcomeBirthRight.localEulerAngles=new Vector3(0,0,-27*ease);}
   if(welcomeBirthSparkles)welcomeBirthSparkles.alpha=Model.S.calm?.65f:Mathf.Clamp01((welcomeAge-1.7f)/.65f)*(.7f+.3f*Mathf.Sin(Mathf.Min(welcomeAge,WelcomeBirthDuration)*2));
   if(welcomeBirthCaption)welcomeBirthCaption.text=Model.S.calm||welcomeAge>=WelcomeBirthDuration?"Теперь вы вместе!":"Тихо… кто-то просыпается";
  }
  void AnimateWelcome(float dt){
   if(Page!="welcome"||PresentationPaused||Model==null)return;
   welcomeAge+=dt;welcomePetAge+=dt;
   AnimateWelcomeBirth();
   bool calm=Model.S.calm;
   if(calm&&welcomeHeart){if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();welcomeHeart.alpha=welcomePetAge<1?1:0;}
   if(welcomeEyes)welcomeEyes.SetBlink(calm?0:Mathf.Max(BlinkAt(welcomeAge),JoyBlink(welcomePetAge)));
   if(welcomePose){
    float breath=calm?0:Mathf.Sin(welcomeAge*1.8f)*.006f;
    var scale=new Vector3(1-breath*.3f,1+breath,1);welcomePose.localScale=scale;
    welcomePose.anchoredPosition=new Vector2(120*(1-scale.x),-244*(1-scale.y));
   }
  }
 }
}
