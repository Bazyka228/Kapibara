using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  RectTransform activityField,activityBasket;
  readonly RectTransform[] activityFindArt=new RectTransform[6];
  readonly Vector2[] activityFindOrigin=new Vector2[6];
  readonly RectTransform[] activityFruit=new RectTransform[8];
  Text activityTime,activityScore;
  SoftShape activityProgress;
  int activityShownScore;
  float activitySaveAge;
  string ActivityName(int kind)=>IdleModel.ActivityNames[Mathf.Clamp(kind,0,5)].ToUpperInvariant();
  static readonly string[] ActivityRules={"Лови фрукты","Ищи плоды","Найди пару","Повтори огоньки","Разложи еду","Краски и узоры"};
  static readonly string[] ActivityColours={"EEC77D","AACD91","E6B4BF","B7B4E4","EDB68D","9FD0CC"};
  void ActivityHub(){
   Label(panel,"Activities intro","ВЫБИРАЙ ИГРУ",24,100,432,47,26,align:TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Activities note","Играй, твори и получай монеты",24,147,432,36,22,align:TextAnchor.MiddleCenter);
   bool pending=Model.S.activity!=null&&!Model.S.activity.complete;
   for(int k=0;k<IdleModel.ActivityCount;k++){
    int kind=k;float x=24+k%2*222,y=190+k/2*195;
    var b=Button(panel,"Start activity "+k,"",x,y,210,184,()=>{if(Model.StartActivity(kind)){Save();Show("activity-play");}},ActivityColours[k]);
    var art=Rect(b.transform,"Activity illustration",9,17,80,80);art.localScale=Vector3.one*.9f;ActivityMedallion(art,k);
    Label(b.transform,"Activity name",k==5?"Рисуем\nвместе":IdleModel.ActivityNames[k],86,7,116,76,22).font=bold;
    Label(b.transform,"Activity rule",ActivityRules[k],12,91,186,44,20,align:TextAnchor.MiddleCenter);
    Shape(b.transform,"Play ribbon",12,141,186,32,"FFF1CE",12);
    Label(b.transform,"Play label",pending&&Model.S.activity.kind==k?"ПРОДОЛЖИТЬ":"ИГРАТЬ · "+IdleModel.ActivityDuration(k)+" с",17,138,176,39,21,align:TextAnchor.MiddleCenter).font=bold;
    b.interactable=Model.Stage>=2&&Model.S.starter&&(!pending||Model.S.activity.kind==k);
   }
   Label(panel,"Activity availability",Model.Stage<2?"Сначала покорми малыша":pending?"Твоя игра ждёт — продолжим?":"Выбирай любую. Вход бесплатный!",24,779,432,47,22,align:TextAnchor.MiddleCenter);
  }
  static void MiniFruit(Transform p,float x,float y,float size)=>ActivityMandarin(p,x,y,size);
  void ActivityPlayPage(){
   var a=Model.S.activity;if(a==null){Show("activities");return;}if(a.complete){Show("activity-result");return;}
   activityShownScore=a.score;activitySaveAge=0;searchHelp=null;searchGuide=null;
   Label(panel,"Activity instruction",(a.kind<2?IdleModel.ActivityVariantName(a.kind,a.variant):a.kind==5?new[]{"Цветочная мастерская","Бабочка-красавица","Весёлая черепашка"}[PaintTheme(a)]:IdleModel.ActivityNames[a.kind])+"\n"+(a.kind==0?"Веди корзинку прямо по полю":a.kind==1?"Найди 6 мандаринов":a.kind==5?"Краска → узор → рисунок":ActivityRules[a.kind]),24,99,432,64,23,align:TextAnchor.MiddleCenter);
   activityTime=Label(panel,"Activity time","",26,163,210,38,23);
   activityScore=Label(panel,"Activity score","",246,163,208,38,23,align:TextAnchor.MiddleRight);
   Shape(panel,"Activity track",24,210,432,10,"D2C29A",5);activityProgress=Shape(panel,"Activity progress",24,210,432,10,"81AC70",5);
   activityField=Rect(panel,"Activity garden",24,238,432,444);
   activityField.gameObject.AddComponent<RectMask2D>();BuildActivityGarden();activityGuide=null;
   if(a.kind>=2){BuildPlayroom(a.kind);RefreshActivity();return;}
   if(a.kind==0){
    for(int n=0;n<8;n++){activityFruit[n]=Rect(activityField,"Falling fruit "+n,0,0,44,44);if(n==7){Oval(activityFruit[n],"Last fruit halo",-8,-8,60,60,"F8DB8660",0);Star(activityFruit[n],34,-7,15,"E5B855");}PicnicFood(activityFruit[n],0,n%2,44);}
    activityBasket=ActivityBasketArt(activityField,166,340,1);
    var harvest=Rect(activityBasket,"Collected fruit",0,0,108,40);harvest.SetSiblingIndex(4);
    for(int n=0;n<8;n++){basketHarvest[n]=Rect(harvest,"Harvest "+n,9+(n%4)*22,-8-(n/4)*13,26,26);MiniFruit(basketHarvest[n],0,0,27);}
    basketSeries=Label(panel,"Basket hint","Лови подряд — собери серию!",24,687,432,38,23,align:TextAnchor.MiddleCenter);
    var track=InkBox(panel,"Basket control",24,738,432,72,"B4C995",25,3);Shape(track.transform,"Control rail",33,33,366,7,"8DA778",3);track.raycastTarget=true;
    var area=Rect(track.transform,"Handle area",30,0,372,72);var handle=Oval(area,"Handle",0,4,64,64,"F1CC81");handle.raycastTarget=true;handle.rectTransform.sizeDelta=new Vector2(64,-8);handle.rectTransform.anchoredPosition=Vector2.zero;
    var paint=handle.transform.GetChild(0).GetComponent<RectTransform>();paint.anchorMin=Vector2.zero;paint.anchorMax=Vector2.one;paint.offsetMin=Vector2.one*3;paint.offsetMax=Vector2.one*-3;
    var slider=track.gameObject.AddComponent<Slider>();slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.value=(float)a.basket;
    activityGuide=Rect(panel,"Gesture hint",170,730,36,36);Oval(activityGuide,"Finger",0,0,25,34,"FFF0CB",2);Mark(activityGuide,"↔",26,0,45,36,24,Ink);
    slider.onValueChanged.AddListener(v=>{if(!PresentationPaused&&Page=="activity-play")Model.ActivityBasket(v);});
    BuildBasketTouch(slider);
   }else{
    for(int n=0;n<6;n++){
     int index=n;float x=(float)IdleModel.SearchX(a,n),y=(float)IdleModel.SearchY(a,n);
     var b=Button(activityField,"Hidden mandarin "+n,"",x,y,80,80,()=>{if(Model.ActivityFind(index)){activityFoundAt[index]=(float)a.elapsed;HarvestFeedback(activityFindOrigin[index]+new Vector2(40,-40));Save();}},"FFFFFF00");
     activityFruit[n]=b.GetComponent<RectTransform>();activityFindOrigin[n]=activityFruit[n].anchoredPosition;activityFindArt[n]=Rect(b.transform,"Found fruit art",14,18,52,52);MiniFruit(activityFindArt[n],0,0,52);Leaf(b.transform,-7,45,52,43,"7FA666",-12);Leaf(b.transform,45,21,38,57,"94B97B",18);
     if(a.variant==1){ActivityFlower(b.transform,-2,35,1.4f,"EDB5AE");ActivityFlower(b.transform,57,38,1.3f,"FFF0CB");}
     else if(a.variant==2){ActivityMushroom(b.transform,-7,49,.85f);ActivityMushroom(b.transform,59,48,.65f);}
     Leaf(activityField,x-8,y-20,45,39,"ABC888",-28);
    }
    searchInstruction=Label(panel,"Search hint","Найди плоды среди растений",30,687,420,65,23,align:TextAnchor.MiddleCenter);
    searchHelp=Button(panel,"Search helper","БАБОЧКА, ПОМОГИ!",46,760,388,67,()=>{if(Model.HintActivity()){Save();RefreshActivity();}},"BBD1AA",22);
    searchGuide=Rect(activityField,"Helpful butterfly",0,0,80,80);ButterflyBadge(searchGuide,0,0,.5f);searchGuide.gameObject.SetActive(false);
   }
   if(a.kind==1){var tray=ActivityBasketArt(activityField,164,371,.68f);for(int n=0;n<6;n++){basketHarvest[n]=Rect(tray,"Found fruit "+n,12+(n%3)*26,-4-(n/3)*17,28,28);MiniFruit(basketHarvest[n],0,0,30);}}
   RefreshActivity();
  }
  void RefreshActivity(){
   var a=Model.S.activity;if(a==null||!activityTime)return;
   activityTime.text=a.kind==5&&a.score==6?"Готово!":"Ещё "+System.Math.Ceiling(IdleModel.ActivityDuration(a.kind)-a.elapsed)+" сек";activityScore.text=(a.kind==0?"Поймано: ":a.kind==1?"Найдено: ":"Готово: ")+a.score+" / "+IdleModel.ActivityGoal(a.kind);
   activityProgress.rectTransform.sizeDelta=new Vector2(432*(1-(float)a.elapsed/IdleModel.ActivityDuration(a.kind)),10);
   if(a.kind>=2){RefreshPlayroom();AnimateActivityGarden();return;}
   if(a.kind==0){
    basketSeries.text=a.combo>=2?"Подряд: "+a.combo+" · Лучшая серия: "+a.bestCombo:"Лови подряд — собери серию!";
    activityBasket.anchoredPosition=new Vector2(382*(float)a.basket-25,-340);
    for(int n=0;n<8;n++){double age=IdleModel.FruitAge(a,n),fall=IdleModel.FruitFallTime(a);float progress=Mathf.Clamp01((float)(age/fall));
     activityFruit[n].gameObject.SetActive(age>=0&&age<fall);
     float breeze=a.patternVersion>0&&a.variant==2&&!Model.S.calm?Mathf.Sin(progress*Mathf.PI*2)*22:0;
     activityFruit[n].anchoredPosition=new Vector2(382*(float)IdleModel.FruitX(a,n)+5+breeze,-progress*328);activityFruit[n].localEulerAngles=new Vector3(0,0,Model.S.calm?0:(float)age*16-12);}
   }else for(int n=0;n<6;n++){
    bool found=(a.mask&(1<<n))!=0;float flight=(float)a.elapsed-activityFoundAt[n];bool flying=found&&!Model.S.calm&&flight<.55f;
    activityFruit[n].gameObject.SetActive(!found||flying);
    activityFruit[n].GetComponent<Button>().interactable=!found;
    activityFruit[n].anchoredPosition=activityFindOrigin[n];
    activityFindArt[n].anchoredPosition=flying?Vector2.Lerp(new Vector2(14,-18),new Vector2(176,-344)-activityFindOrigin[n],Mathf.SmoothStep(0,1,flight/.55f)):new Vector2(14,-18);activityFindArt[n].localScale=Vector3.one*(flying?Mathf.Lerp(1,.15f,flight/.55f):1);
    activityFruit[n].localScale=Vector3.one*(flying||Model.S.calm||a.elapsed<10?1:1+.035f*Mathf.Sin((float)a.elapsed*2+n));
    if(basketHarvest[n])basketHarvest[n].gameObject.SetActive(n<a.score);

   }
   if(a.kind==1&&searchHelp){int target=IdleModel.SearchHint(a);bool hint=target>=0&&(a.elapsed<a.hintUntil||a.elapsed>6&&a.elapsed>a.feedbackUntil);searchGuide.gameObject.SetActive(hint);searchHelp.interactable=target>=0&&a.elapsed>=a.hintUntil;searchInstruction.text=hint?"Бабочка нашла что-то вкусное!":"Найди плоды среди растений";if(hint){searchGuide.anchoredPosition=activityFindOrigin[target]+new Vector2(42,21+(Model.S.calm?0:Mathf.Sin((float)a.elapsed*4)*6));searchGuide.SetAsLastSibling();}}
   AnimateActivityGarden();
  }
  void TickActivity(float dt){
   if(Page!="activity-play"||PresentationPaused)return;
   Model.StepActivity(dt);activitySaveAge+=dt;var a=Model.S.activity;
   if(a==null)return;
   if(a.complete){Save();Sound(feedClip,.18f);Show("activity-result");return;}
   if(a.score!=activityShownScore){activityShownScore=a.score;if(a.kind==0)HarvestFeedback(new Vector2(382*(float)a.basket+25,-332));if(!Model.S.calm){activityScore.color=C("418355");tweens.Track(activityScore.DOColor(C("513821"),.45f),activityScore.gameObject);}if(a.kind!=3)Sound(careClip,.1f);Save();}
   if(activitySaveAge>=2){Save();activitySaveAge=0;}
   RefreshActivity();
  }
  void ActivityExitPage(){
   Save();Label(panel,"Activity exit title","Вернуться к капибаре?",24,215,432,100,28,align:TextAnchor.MiddleCenter);
   Label(panel,"Activity exit note","Раунд сохранится.\nНаграда — после завершения.",30,335,420,100,24,align:TextAnchor.MiddleCenter);
   Button(panel,"Resume activity","ПРОДОЛЖИТЬ",24,490,432,72,()=>Show("activity-play"),"55E6C1");
   Button(panel,"Leave activity","К КАПИБАРЕ",24,584,432,72,()=>Show("home"));
  }
  void ActivityResultPage(){
   var a=Model.S.activity;if(a==null||!a.complete){Show("activities");return;}
   Label(panel,"Activity congratulations",a.newBest?"Новый рекорд!":"Спасибо за игру!",24,104,432,48,30,align:TextAnchor.MiddleCenter);
   Label(panel,"Activity result score",(a.kind==0?"Поймано: ":a.kind==1?"Найдено: ":"Готово: ")+a.score+" из "+IdleModel.ActivityGoal(a.kind)+" · Рекорд: "+Model.S.activityBest[a.kind],24,153,432,38,23,align:TextAnchor.MiddleCenter);
   var badge=Rect(panel,"Completed game badge",364,208,80,80);ActivityMedallion(badge,a.kind);
   var portrait=Rect(panel,"Happy activity capy",100,190,280,260);portrait.localScale=Vector3.one*.85f;if(a.kind==5)FinishedPainting(portrait,a);else DrawAvatar(portrait,Model.S.equipped[0]);if(!Model.S.calm){portrait.localScale=Vector3.one*.74f;tweens.Track(portrait.DOScale(.85f,.6f).SetEase(Ease.OutBack),portrait.gameObject);}ActivityFlower(panel,63,374,1.2f,"F1B6A0");ActivityFlower(panel,358,373,1,"FFE8B6");
   Label(panel,"Activity achievement",a.kind==0?"Лучшая серия: "+a.bestCombo:a.kind==2?"Найдено пар: "+a.score+" · Попыток: "+a.attempts:a.kind==5?"Твой рисунок — твои цвета!":"Капибара радуется вместе с тобой",24,404,432,40,21,align:TextAnchor.MiddleCenter);
   PaperCard(panel,"Activity prize",24,454,432,164);Coin(panel,53,490,57);
   Label(panel,"Activity coins","+"+Number(a.reward)+" монет",123,470,310,55,30);
   Label(panel,"Activity saved","Награда сохранена\nНастроение до +"+(a.score>0?10:1),123,530,310,60,22);
   Button(panel,"Activity replay","ЕЩЁ РАЗ",24,650,432,70,()=>{if(Model.StartActivity(a.kind)){Save();Show("activity-play");}},"55E6C1");
   Button(panel,"Activity finish","ДРУГИЕ ИГРЫ",24,742,432,70,()=>Show("activities"));
  }
 }
}
