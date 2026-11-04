using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  RectTransform activityField,activityBasket;
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
    var art=Rect(panel,"Activity illustration",40,y+20,90,100);MiniFruit(art,10,12,54);Leaf(art,0,68,84,28,"87AB68");
    Label(panel,"Activity name",k==0?"Мандариновая\nкорзинка":"Секреты сада",142,y+12,295,72,26);
    Label(panel,"Activity rule",k==0?"Двигай корзинку":"Найди 6 мандаринов",142,y+84,295,36,22);
    var b=Button(panel,"Start activity "+k,pending&&Model.S.activity.kind==k?"ПРОДОЛЖИТЬ":"ИГРАТЬ · 25 СЕК",142,y+126,292,60,()=>{if(Model.StartActivity(kind)){Save();Show("activity-play");}},"55E6C1",22);
    b.interactable=Model.Stage>=2&&Model.S.starter&&(!pending||Model.S.activity.kind==k);
   }
   Label(panel,"Activity availability",Model.Stage<2?"Первая миска откроет занятия":pending?"Сначала закончим сохранённую игру":"Без платы за вход и ожидания",28,688,424,80,22,align:TextAnchor.MiddleCenter);
   Button(panel,"Activities care","К ЗАБОТЕ",24,774,432,60,()=>Show("care"));
  }
  static void MiniFruit(Transform p,float x,float y,float size){
   Oval(p,"Mandarin",x,y,size,size,"EEA345",2.5f);Shape(p,"Fruit light",x+size*.2f,y+size*.15f,size*.35f,size*.2f,"FFD17C",0,true);
   Leaf(p,x+size*.48f,y-7,size*.48f,size*.24f,"77A258",-12);
   for(int n=0;n<3;n++)Shape(p,"Peel dimple",x+size*(.5f+n*.1f),y+size*(.61f+(n%2)*.12f),3,3,"C87838",0,true);
  }
  void ActivityPlayPage(){
   var a=Model.S.activity;if(a==null){Show("activities");return;}if(a.complete){Show("activity-result");return;}
   activityShownScore=a.score;activitySaveAge=0;
   Label(panel,"Activity instruction",a.kind==0?"Веди корзинку пальцем":"Найди мандарины среди листьев",24,99,432,64,23,align:TextAnchor.MiddleCenter);
   activityTime=Label(panel,"Activity time","",26,163,210,38,23);
   activityScore=Label(panel,"Activity score","",246,163,208,38,23,align:TextAnchor.MiddleRight);
   Shape(panel,"Activity track",24,210,432,10,"D2C29A",5);activityProgress=Shape(panel,"Activity progress",24,210,432,10,"81AC70",5);
   activityField=Rect(panel,"Activity garden",24,238,432,444);
   Shape(activityField,"Garden sky",0,0,432,444,"DAE6D0",20);Shape(activityField,"Garden ground",0,270,432,174,"BFCE92",20);
   for(int n=0;n<7;n++){Oval(activityField,"Garden stepping stone",12+n*61,337+(n%2)*56,49,15,"F3E4BA",1);Leaf(activityField,n*65-8,13+(n%3)*22,53,75,"90B579",n%2==0?18:-15);}
   if(a.kind==0){
    for(int n=0;n<8;n++){activityFruit[n]=Rect(activityField,"Falling fruit "+n,0,0,44,44);MiniFruit(activityFruit[n],0,0,40);}
    activityBasket=Rect(activityField,"Basket",0,340,100,70);InkBox(activityBasket,"Wicker body",0,0,100,54,"CF9F60",12);InkBox(activityBasket,"Basket rim",-4,-4,108,15,"F2CA86",6);
    for(int n=0;n<5;n++)Stroke(activityBasket,"Weave",8,16+n*7,91,16+n*7,"9B6B40",2);
    for(int n=0;n<6;n++)Stroke(activityBasket,"Weave upright",12+n*15,13,12+n*15,49,"EBC482",2);
    Label(panel,"Basket hint","←  Двигай ползунок  →",24,687,432,38,23,align:TextAnchor.MiddleCenter);
    var track=Shape(panel,"Basket control",24,738,432,72,"B3C897",25);track.raycastTarget=true;
    var area=Rect(track.transform,"Handle area",30,0,372,72);var handle=Oval(area,"Handle",0,4,64,64,"F1CC81");handle.raycastTarget=true;handle.rectTransform.sizeDelta=new Vector2(64,-8);handle.rectTransform.anchoredPosition=Vector2.zero;
    var paint=handle.transform.GetChild(0).GetComponent<RectTransform>();paint.anchorMin=Vector2.zero;paint.anchorMax=Vector2.one;paint.offsetMin=Vector2.one*3;paint.offsetMax=Vector2.one*-3;
    var slider=track.gameObject.AddComponent<Slider>();slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.value=(float)a.basket;
    slider.onValueChanged.AddListener(v=>{if(!PresentationPaused&&Page=="activity-play")Model.ActivityBasket(v);});
   }else{
    for(int n=0;n<6;n++){
     int index=n;float x=22+((n+a.seed)%3)*132+((n*7+a.seed*11)%17),y=67+(n/3)*158+((n*23+a.seed*17)%38);
     var b=Button(activityField,"Hidden mandarin "+n,"",x,y,80,80,()=>{if(Model.ActivityFind(index)){Save();Sound(careClip,.12f);}},"FFFFFF00");
     activityFruit[n]=b.GetComponent<RectTransform>();MiniFruit(b.transform,14,18,52);Leaf(b.transform,-7,45,52,43,"7FA666",-12);Leaf(b.transform,45,21,38,57,"94B97B",18);
     Leaf(activityField,x-8,y-20,45,39,"ABC888",-28);
    }
    Label(panel,"Search hint","Листья подскажут, где искать.\nОшибки ничего не отнимают.",30,704,420,92,23,align:TextAnchor.MiddleCenter);
   }
   RefreshActivity();
  }
  void RefreshActivity(){
   var a=Model.S.activity;if(a==null||!activityTime)return;
   activityTime.text="Ещё "+System.Math.Ceiling(25-a.elapsed)+" сек";activityScore.text="Найдено: "+a.score;
   activityProgress.rectTransform.sizeDelta=new Vector2(432*(1-(float)a.elapsed/25),10);
   if(a.kind==0){
    activityBasket.anchoredPosition=new Vector2(382*(float)a.basket-25,-340);
    for(int n=0;n<8;n++){double age=IdleModel.FruitAge(a,n);activityFruit[n].gameObject.SetActive(age>=0&&age<3.2);activityFruit[n].anchoredPosition=new Vector2(382*(float)IdleModel.FruitX(a,n)+5,-(float)(age/3.2*328));}
   }else for(int n=0;n<6;n++){
    bool found=(a.mask&(1<<n))!=0;activityFruit[n].gameObject.SetActive(!found);
    activityFruit[n].localScale=Vector3.one*(Model.S.calm||a.elapsed<10?1:1+.035f*Mathf.Sin((float)a.elapsed*2+n));
   }
  }
  void TickActivity(float dt){
   if(Page!="activity-play"||PresentationPaused)return;
   Model.StepActivity(dt);activitySaveAge+=dt;var a=Model.S.activity;
   if(a==null)return;
   if(a.complete){Save();Sound(feedClip,.18f);Show("activity-result");return;}
   if(a.score!=activityShownScore){activityShownScore=a.score;Sound(careClip,.1f);Save();}
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
   var portrait=Rect(panel,"Happy activity capy",100,190,280,260);portrait.localScale=Vector3.one*.85f;DrawAvatar(portrait,Model.S.equipped[0]);
   PaperCard(panel,"Activity prize",24,454,432,164);Coin(panel,53,490,57);
   Label(panel,"Activity coins","+"+Number(a.reward)+" монет",123,470,310,55,30);
   Label(panel,"Activity saved","Награда уже сохранена",123,530,310,60,22);
   Button(panel,"Activity replay","ЕЩЁ РАЗ",24,650,432,70,()=>{if(Model.StartActivity(a.kind)){Save();Show("activity-play");}},"55E6C1");
   Button(panel,"Activity finish","К КАПИБАРЕ",24,742,432,70,()=>Show("home"));
  }
 }
}
