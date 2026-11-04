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
  string ActivityName(int kind)=>kind==0?"МАНДАРИНОВАЯ КОРЗИНКА":"СЕКРЕТЫ САДА";
  void ActivityHub(){
   Label(panel,"Activities intro","Поиграем вместе?",24,102,432,44,28,align:TextAnchor.MiddleCenter);
   Label(panel,"Activities note","Монеты и настроение за заботу.\nМожно вернуться в любой момент.",30,152,420,70,22,align:TextAnchor.MiddleCenter);
   var pending=Model.S.activity!=null&&!Model.S.activity.complete;
   for(int k=0;k<2;k++){
    int kind=k;float y=248+k*208;PaperCard(panel,"Activity card "+k,24,y,432,192);
    var art=Rect(panel,"Activity illustration",40,y+20,90,100);if(k==0){ActivityBasketArt(art,0,42,.75f);MiniFruit(art,26,19,43);}else{MiniFruit(art,19,26,51);Leaf(art,-2,57,49,33,"80A26A",-15);Leaf(art,58,29,35,54,"A6C48B",15);ActivityFlower(art,5,2,.9f,"F4C4AE");}
    Label(panel,"Activity name",k==0?"Мандариновая\nкорзинка":"Секреты сада",142,y+12,295,72,26);
    Label(panel,"Activity rule",k==0?"Двигай корзинку":"Найди 6 мандаринов",142,y+84,295,36,22);
    var b=Button(panel,"Start activity "+k,pending&&Model.S.activity.kind==k?"ПРОДОЛЖИТЬ":"ИГРАТЬ · 25 СЕК",142,y+126,292,60,()=>{if(Model.StartActivity(kind)){Save();Show("activity-play");}},"55E6C1",22);
    b.interactable=Model.Stage>=2&&Model.S.starter&&(!pending||Model.S.activity.kind==k);
   }
   Label(panel,"Activity availability",Model.Stage<2?"Первая миска откроет занятия":pending?"Сначала закончим сохранённую игру":"Без платы за вход и ожидания",28,688,424,80,22,align:TextAnchor.MiddleCenter);
   Button(panel,"Activities care",activityReturn=="home"?"К КАПИБАРЕ":"К ЗАБОТЕ",24,774,432,60,()=>Show(activityReturn));
  }
  static void MiniFruit(Transform p,float x,float y,float size)=>ActivityMandarin(p,x,y,size);
  void ActivityPlayPage(){
   var a=Model.S.activity;if(a==null){Show("activities");return;}if(a.complete){Show("activity-result");return;}
   activityShownScore=a.score;activitySaveAge=0;
   Label(panel,"Activity instruction",a.kind==0?"Веди корзинку пальцем":"Найди мандарины среди листьев",24,99,432,64,23,align:TextAnchor.MiddleCenter);
   activityTime=Label(panel,"Activity time","",26,163,210,38,23);
   activityScore=Label(panel,"Activity score","",246,163,208,38,23,align:TextAnchor.MiddleRight);
   Shape(panel,"Activity track",24,210,432,10,"D2C29A",5);activityProgress=Shape(panel,"Activity progress",24,210,432,10,"81AC70",5);
   activityField=Rect(panel,"Activity garden",24,238,432,444);
   activityField.gameObject.AddComponent<RectMask2D>();BuildActivityGarden();activityGuide=null;
   if(a.kind==0){
    for(int n=0;n<8;n++){activityFruit[n]=Rect(activityField,"Falling fruit "+n,0,0,44,44);MiniFruit(activityFruit[n],0,0,40);}
    activityBasket=ActivityBasketArt(activityField,166,340,1);
    var harvest=Rect(activityBasket,"Collected fruit",0,0,108,40);harvest.SetSiblingIndex(4);
    for(int n=0;n<8;n++){basketHarvest[n]=Rect(harvest,"Harvest "+n,9+(n%4)*22,-8-(n/4)*13,26,26);MiniFruit(basketHarvest[n],0,0,27);}
    Label(panel,"Basket hint","←  Двигай ползунок  →",24,687,432,38,23,align:TextAnchor.MiddleCenter);
    var track=InkBox(panel,"Basket control",24,738,432,72,"B4C995",25,3);Shape(track.transform,"Control rail",33,33,366,7,"8DA778",3);track.raycastTarget=true;
    var area=Rect(track.transform,"Handle area",30,0,372,72);var handle=Oval(area,"Handle",0,4,64,64,"F1CC81");handle.raycastTarget=true;handle.rectTransform.sizeDelta=new Vector2(64,-8);handle.rectTransform.anchoredPosition=Vector2.zero;
    var paint=handle.transform.GetChild(0).GetComponent<RectTransform>();paint.anchorMin=Vector2.zero;paint.anchorMax=Vector2.one;paint.offsetMin=Vector2.one*3;paint.offsetMax=Vector2.one*-3;
    var slider=track.gameObject.AddComponent<Slider>();slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.value=(float)a.basket;
    activityGuide=Rect(panel,"Gesture hint",170,730,36,36);Oval(activityGuide,"Finger",0,0,25,34,"FFF0CB",2);Mark(activityGuide,"↔",26,0,45,36,24,Ink);
    slider.onValueChanged.AddListener(v=>{if(!PresentationPaused&&Page=="activity-play")Model.ActivityBasket(v);});
   }else{
    for(int n=0;n<6;n++){
     int index=n;float x=22+((n+a.seed)%3)*132+((n*7+a.seed*11)%17),y=67+(n/3)*158+((n*23+a.seed*17)%38);
     var b=Button(activityField,"Hidden mandarin "+n,"",x,y,80,80,()=>{if(Model.ActivityFind(index)){activityFoundAt[index]=(float)a.elapsed;HarvestFeedback(activityFindOrigin[index]+new Vector2(40,-40));Save();}},"FFFFFF00");
     activityFruit[n]=b.GetComponent<RectTransform>();activityFindOrigin[n]=activityFruit[n].anchoredPosition;activityFindArt[n]=Rect(b.transform,"Found fruit art",14,18,52,52);MiniFruit(activityFindArt[n],0,0,52);Leaf(b.transform,-7,45,52,43,"7FA666",-12);Leaf(b.transform,45,21,38,57,"94B97B",18);
     Leaf(activityField,x-8,y-20,45,39,"ABC888",-28);
    }
    Label(panel,"Search hint","Листья подскажут, где искать.\nОшибки ничего не отнимают.",30,704,420,92,23,align:TextAnchor.MiddleCenter);
   }
   if(a.kind==1){var tray=ActivityBasketArt(activityField,164,371,.68f);for(int n=0;n<6;n++){basketHarvest[n]=Rect(tray,"Found fruit "+n,12+(n%3)*26,-4-(n/3)*17,28,28);MiniFruit(basketHarvest[n],0,0,30);}}
   RefreshActivity();
  }
  void RefreshActivity(){
   var a=Model.S.activity;if(a==null||!activityTime)return;
   activityTime.text="Ещё "+System.Math.Ceiling(25-a.elapsed)+" сек";activityScore.text="Найдено: "+a.score;
   activityProgress.rectTransform.sizeDelta=new Vector2(432*(1-(float)a.elapsed/25),10);
   if(a.kind==0){
    activityBasket.anchoredPosition=new Vector2(382*(float)a.basket-25,-340);
    for(int n=0;n<8;n++){double age=IdleModel.FruitAge(a,n);activityFruit[n].gameObject.SetActive(age>=0&&age<3.2);activityFruit[n].anchoredPosition=new Vector2(382*(float)IdleModel.FruitX(a,n)+5,-(float)(age/3.2*328));activityFruit[n].localEulerAngles=new Vector3(0,0,Model.S.calm?0:(float)age*16-12);}
   }else for(int n=0;n<6;n++){
    bool found=(a.mask&(1<<n))!=0;float flight=(float)a.elapsed-activityFoundAt[n];bool flying=found&&!Model.S.calm&&flight<.55f;
    activityFruit[n].gameObject.SetActive(!found||flying);
    activityFruit[n].GetComponent<Button>().interactable=!found;
    activityFruit[n].anchoredPosition=activityFindOrigin[n];
    activityFindArt[n].anchoredPosition=flying?Vector2.Lerp(new Vector2(14,-18),new Vector2(176,-344)-activityFindOrigin[n],Mathf.SmoothStep(0,1,flight/.55f)):new Vector2(14,-18);activityFindArt[n].localScale=Vector3.one*(flying?Mathf.Lerp(1,.15f,flight/.55f):1);
    activityFruit[n].localScale=Vector3.one*(flying||Model.S.calm||a.elapsed<10?1:1+.035f*Mathf.Sin((float)a.elapsed*2+n));
    if(basketHarvest[n])basketHarvest[n].gameObject.SetActive(n<a.score);

   }
   AnimateActivityGarden();
  }
  void TickActivity(float dt){
   if(Page!="activity-play"||PresentationPaused)return;
   Model.StepActivity(dt);activitySaveAge+=dt;var a=Model.S.activity;
   if(a==null)return;
   if(a.complete){Save();Sound(feedClip,.18f);Show("activity-result");return;}
   if(a.score!=activityShownScore){activityShownScore=a.score;if(a.kind==0)HarvestFeedback(new Vector2(382*(float)a.basket+25,-332));if(!Model.S.calm){activityScore.color=C("418355");tweens.Track(activityScore.DOColor(C("513821"),.45f),panel.gameObject);}Sound(careClip,.1f);Save();}
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
   Label(panel,"Activity congratulations","Спасибо за игру!",24,112,432,60,30,align:TextAnchor.MiddleCenter);
   var portrait=Rect(panel,"Happy activity capy",100,190,280,260);portrait.localScale=Vector3.one*.85f;DrawAvatar(portrait,Model.S.equipped[0]);if(!Model.S.calm){portrait.localScale=Vector3.one*.74f;tweens.Track(portrait.DOScale(.85f,.6f).SetEase(Ease.OutBack),panel.gameObject);}ActivityFlower(panel,63,374,1.2f,"F1B6A0");ActivityFlower(panel,358,373,1,"FFE8B6");
   PaperCard(panel,"Activity prize",24,454,432,164);Coin(panel,53,490,57);
   Label(panel,"Activity coins","+"+Number(a.reward)+" монет",123,470,310,55,30);
   Label(panel,"Activity saved","Награда уже сохранена",123,530,310,60,22);
   Button(panel,"Activity replay","ЕЩЁ РАЗ",24,650,432,70,()=>{if(Model.StartActivity(a.kind)){Save();Show("activity-play");}},"55E6C1");
   Button(panel,"Activity finish","К КАПИБАРЕ",24,742,432,70,()=>Show("home"));
  }
 }
}
