using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  static readonly string[] NeedNames={"Сытость","Уют","Настроение"};
  static readonly string[] NeedColours={"E6AB65","77BDB5","DD91A4"};
  static readonly string[] ActionNames={"Листочек","Купаться","Играть","Погладить","Причесать","Отдохнуть"};
  static readonly string[] ActionNotes={"+25 сытости","+40 уюта","+30 настроения","+8 настроения","+15 уюта","Спокойный сон"};
  static readonly string[] ActionFeedback={"Хрум! Вкусный листочек","Свежо и уютно!","Мячик вернулся!","Капибара довольно урчит","Какая мягкая шёрстка!","Отдыхай. Монеты копятся"};
  readonly SoftShape[] needFills=new SoftShape[3];
  readonly Button[] needButtons=new Button[3];
  readonly float[] needShown=new float[3];
  readonly float[] needTargets=new float[3];
  readonly Tween[] needTweens=new Tween[3];
  CanvasGroup careFxOpacity,careHintOpacity;
  RectTransform careFx,careProp,careHintVisual;
  readonly RectTransform[] careParticles=new RectTransform[7];
  int careReaction=-1,hintNeed=-1;
  float careReactionAge=100,careHintAge=100,nextCareHint;
  public int CareReaction=>careReaction;
  public float CareReactionProgress=>Mathf.Clamp01(careReactionAge/2.5f);
  public bool CareReactionVisible=>careFxOpacity&&careFxOpacity.alpha>0;
  float CareSmile=>careReaction!=5&&evolutionAge>2.4f?JoyBlink(careReactionAge):0;
  static float JoyBlink(float age){
   float first=(age-.22f)/.42f,second=(age-1.05f)/.32f;
   return first>0&&first<1?Mathf.Sin(first*Mathf.PI)*.85f:second>0&&second<1?Mathf.Sin(second*Mathf.PI)*.65f:0;
  }
  // Only describe the actual change: a nearly full need never promises a full +40.
  static string CareGain(string name,double delta)=>Math.Abs(delta)<.05?"":name+" "+(delta>0?"+":"")+Math.Round(delta,1).ToString("0.#");
  string CareResult(double fullness,double comfort,double mood){
   string result="";
   foreach(var gain in new[]{CareGain("Сытость",Model.S.fullness-fullness),CareGain("уют",Model.S.comfort-comfort),CareGain("настроение",Model.S.mood-mood)})
    if(gain.Length>0)result+=(result.Length>0?" · ":"")+gain;
   return result.Length>0?result:"Капибаре уже хорошо";
  }

  void BuildCareHud(){
   for(int n=0;n<3;n++){
    int need=n;var b=Button(home,"Care need "+n,"",24+n*148,536,136,64,()=>{if(need==0)Show("food");else DoCare(need==1?1:2);});
    needButtons[n]=b;
    Label(b.transform,"Need title",NeedNames[n],5,1,126,32,22,"513821",TextAnchor.MiddleCenter);
    Shape(b.transform,"Need track",12,39,112,13,"BFA882",6);
    needFills[n]=Shape(b.transform,"Need fill",12,39,112,13,NeedColours[n],6);
    needShown[n]=needTargets[n]=(float)Model.Need(n);
   }
  }
  void RefreshCareHud(){
   for(int n=0;n<3;n++){
    SetNeedShown(n,needShown[n]);
    if(needButtons[n])needButtons[n].interactable=n==0||Model.ActionWait<=0;
   }
  }
  void SetNeedShown(int need,float value){
   needShown[need]=value;
   if(needFills[need])needFills[need].rectTransform.sizeDelta=new Vector2(112*Mathf.Clamp01(value/100),13);
  }
  string CareHint(){
   if(Model.S.careTutorial==0)return "Тапни по капибаре — она рада тебе";
   if(Model.S.careTutorial==1&&!Model.S.starter)return "Нажми «Сытость» — первая миска бесплатно";
   if(Model.S.careTutorial==2)return "«Уют» — купаться\n«Настроение» — играть";
   if(Model.S.resting)return "Тихий отдых · ещё "+Math.Ceiling(Model.S.restRemaining)+" с\nТапни по капибаре, чтобы разбудить";
   if(IsSleeping)return "Капибара дремлет. Тапни, чтобы разбудить";
   if(Model.CareMultiplier>=1.2)return "Спасибо! +20% за тап ещё "+Math.Ceiling(Model.S.careBuffUntil-Model.S.activeSeconds)+" с";
   if(Model.Happy&&(int)(lifeAge/6)%2==0)return "Капибаре хорошо · +10% за тап";
   return "Зажми капибару, чтобы погладить";
  }
  void CarePage(){
   Label(panel,"Care introduction","Всё для заботы — бесплатно",24,96,432,43,24,align:TextAnchor.MiddleCenter);
   for(int n=0;n<3;n++){
    int need=n;float x=24+n*148;PaperCard(panel,"Care state "+n,x,147,136,98);
    Label(panel,"Care need name",NeedNames[n],x+4,151,128,32,22,align:TextAnchor.MiddleCenter);
    var value=Label(panel,"Care value "+n,"",x+4,185,128,32,22,align:TextAnchor.MiddleCenter);
    Shape(panel,"Care track",x+12,222,112,10,"CABB90",5);
    var fill=Shape(panel,"Care page fill "+n,x+12,222,112,10,NeedColours[n],5);
    liveLabels.Add(()=>{value.text=Math.Round(Model.Need(need))+" / 100";fill.rectTransform.sizeDelta=new Vector2(112*(float)Model.Need(need)/100,10);});
   }
   for(int n=0;n<6;n++){
    int action=n;float x=24+n%2*222,y=266+n/2*124;
    var b=Button(panel,"Care action "+n,"",x,y,210,112,()=>DoCare(action),"F4CF80",22);
    var icon=Rect(b.transform,"Action icon",12,12,64,64);icon.localScale=Vector3.one*.7f;CareIcon(icon,n);
    Label(b.transform,"Action name",ActionNames[n],65,8,136,40,22);
    var effect=Label(b.transform,"Action effect",ActionNotes[n],10,62,190,39,22,align:TextAnchor.MiddleCenter);
    liveLabels.Add(()=>{
     double wait=Math.Max(Model.ActionWait,action==3?10-(Model.S.activeSeconds-Model.S.lastPet):0);
     b.interactable=wait<=0&&!(action==5&&Model.S.resting);
     effect.text=action==5&&Model.S.resting?"Отдых · "+Math.Ceiling(Model.S.restRemaining)+" с":wait>0?"Ещё "+Math.Ceiling(wait)+" с":ActionNotes[action];
    });
   }
   Button(panel,"Care food shop",Model.S.starter?"ЕДА ДЛЯ РОСТА":"ПЕРВАЯ МИСКА · БЕСПЛАТНО",24,660,432,70,()=>Show("food"),"55E6C1",23);
   Label(panel,"Care reassurance","Можно играть в своём темпе.\nПустые шкалы не отнимают\nмонеты, рост или покупки.",28,739,424,98,22,"C9BCD8",TextAnchor.MiddleCenter);
  }
  void DoCare(int action){
   if(!loaded||suspended||platformPaused)return;
   double fullness=Model.S.fullness,comfort=Model.S.comfort,mood=Model.S.mood;
   if(!Model.CareAction(action)){Toast(action==3?"Капибара довольна. Давай немного подождём":"Капибара заканчивает действие");return;}
   Save();Show("home");BeginCareReaction(action);
   Toast(ActionFeedback[action]+"\n"+(action==5?"Уют и настроение растут во сне":CareResult(fullness,comfort,mood)));Sound(action==0?feedClip:careClip,.16f);
  }
  void BeginCareReaction(int action){careReaction=action;careReactionAge=0;reactionAge=0;ConfigureCareFx();}
  void BuildCareVisuals(){
   careFx=Rect(hero,"Care action visuals",0,0,250,260);
   careFxOpacity=careFx.gameObject.AddComponent<CanvasGroup>();careFxOpacity.blocksRaycasts=false;careFxOpacity.interactable=false;careFxOpacity.alpha=0;
   careHintVisual=Rect(hero,"Care invitation",-12,90,55,58);
   careHintOpacity=careHintVisual.gameObject.AddComponent<CanvasGroup>();careHintOpacity.blocksRaycasts=false;careHintOpacity.interactable=false;careHintOpacity.alpha=0;
   ConfigureCareFx();
  }
  void ConfigureCareFx(){
   if(!careFx)return;Clear(careFx);Array.Clear(careParticles,0,careParticles.Length);
   careProp=Rect(careFx,"Care prop",0,0,240,260);
   if(careReaction<0)return;
   if(careReaction==1){
    Oval(careProp,"Bath shadow",3,228,238,28,"557A762D",0);
    InkBox(careProp,"Little bath",7,204,228,55,"88C5BC",24,3);
    Oval(careProp,"Water",10,197,222,29,"B4E3D9",2);Shape(careProp,"Water shine",35,204,80,3,"F3FFE8",1);
    for(int n=0;n<5;n++)Oval(careProp,"Foam",27+n*39,194+n%2*4,31,20,"FFF8DE",1);
   }else if(careReaction==0){DrawFoodIcon(careProp,0,92,193);}
   else if(careReaction==2){CareIcon(careProp,2);careProp.sizeDelta=new Vector2(64,64);}
   else if(careReaction==4){CareIcon(careProp,4);careProp.sizeDelta=new Vector2(64,64);}
   else if(careReaction==5){EffectMotif(careProp,11,"F3E6AD");}
   else {Heart(careProp,0,0,35,"EBA2AD");}
   for(int n=0;n<careParticles.Length;n++){
    var particle=Rect(careFx,"Care particle "+n,0,0,34,34);particle.localScale=Vector3.one*(.45f+n%3*.1f);
    EffectMotif(particle,careReaction==1?0:careReaction==2?1:careReaction==4?1:careReaction==5?11:careReaction==0?9:2,"F4D785");careParticles[n]=particle;
   }
  }
  static void CareIcon(Transform p,int action){
   if(action==0){DrawFoodIcon(p,0,0,0);return;}
   if(action==1){EffectMotif(p,8,"A9DFE4");EffectMotif(Rect(p,"Soap bubble",30,15,30,30),0,"A9DFE4");return;}
   if(action==2){
    Oval(p,"Play ball",4,1,49,49,"EDAE71",3);
    Stroke(p,"Ball seam",9,16,43,37,"FFF1CC",5);Stroke(p,"Ball seam",20,5,31,45,"77BDB5",6);
    Shape(p,"Ball gloss",14,8,12,5,"FFF7DC",2);return;
   }
   if(action==3){Heart(p,5,3,47,"DE91A4");Shape(p,"Heart shine",16,11,11,4,"FFE8DB",2);return;}
   if(action==4){
    InkBox(p,"Brush handle",23,28,13,27,"BF8454",5,2);InkBox(p,"Brush back",8,3,43,30,"E2B27B",10,2);
    for(int n=0;n<5;n++)Stroke(p,"Brush bristle",13+n*7,12,13+n*7,25,"FFF3CD",3);return;
   }
   EffectMotif(p,11,"F3E6AD");Star(p,33,18,20,"F0C579");
  }
  void AnimateCare(float dt){
   if(Model==null||suspended||platformPaused)return;
   for(int n=0;n<3;n++){
    float target=(float)Model.Need(n);
    if(Model.S.calm){if(needTweens[n].IsActive())needTweens[n].Kill();needShown[n]=needTargets[n]=target;}
    else if(Mathf.Abs(needTargets[n]-target)>.25f||(!needTweens[n].IsActive()&&Mathf.Abs(needShown[n]-target)>1)){
     int need=n;if(needTweens[n].IsActive())needTweens[n].Kill();needTargets[n]=target;
     needTweens[n]=tweens.Track(DOTween.To(()=>needShown[need],value=>SetNeedShown(need,value),target,.42f).SetEase(Ease.OutCubic),home.gameObject);
    }
   }
   careReactionAge+=dt;careHintAge+=dt;
   if(!home||!home.gameObject.activeSelf||!careFxOpacity)return;
   bool priority=evolutionAge<2.4f||mealAge<1.8f;
   float t=Mathf.Clamp01(careReactionAge/2.5f);
   careFxOpacity.alpha=priority||t>=1?0:Mathf.Min(1,(1-t)*5);
   if(careProp){
    float move=Model.S.calm?0:Mathf.Sin(t*Mathf.PI);
    if(careReaction==2){
     float arc=Model.S.calm?0:Mathf.Abs(Mathf.Sin(t*Mathf.PI*2));
     careProp.anchoredPosition=new Vector2(Model.S.calm?195:Mathf.Lerp(18,202,Mathf.Sin(t*Mathf.PI)),-210+arc*(1-t)*62);
     careProp.localEulerAngles=new Vector3(0,0,Model.S.calm?0:-Mathf.Sin(t*Mathf.PI)*210);
    }
    else if(careReaction==4){float stroke=Model.S.calm?0:Mathf.Sin(t*Mathf.PI*6);careProp.anchoredPosition=new Vector2(184+stroke*7,-82-stroke*8);careProp.localEulerAngles=new Vector3(0,0,stroke*10);}
    else if(careReaction==5)careProp.anchoredPosition=new Vector2(195,-25);
    else if(careReaction==3||careReaction==6)careProp.anchoredPosition=new Vector2(210,-70+move*15);
    else careProp.anchoredPosition=Vector2.zero;
   }
   for(int n=0;n<careParticles.Length;n++)if(careParticles[n]){
    careParticles[n].gameObject.SetActive(!Model.S.calm);
    float phase=Mathf.Clamp01((t-n*.045f)/.72f),wave=Mathf.Sin(phase*Mathf.PI);
    float x=n%2==0?4+n*3:218-n*2,y=-210+n*23;
    if(careReaction==1){x+=Mathf.Sin(phase*5+n)*11;y=-205+phase*(75+n*7);}
    else if(careReaction==2){x+=Mathf.Sin(phase*4+n)*16;y+=wave*45;}
    else {x+=Mathf.Sin(phase*3+n)*6;y+=phase*52;}
    careParticles[n].anchoredPosition=new Vector2(x,y);
    careParticles[n].localScale=Vector3.one*(.45f+n%3*.1f)*wave;
    careParticles[n].localEulerAngles=new Vector3(0,0,careReaction==1?0:Mathf.Sin(phase*4+n)*12);
   }
   if(lifeAge>=nextCareHint&&!priority&&t>=1){
    nextCareHint=lifeAge+90;hintNeed=-1;
    for(int n=0;n<3;n++)if(Model.Need(n)<30&&(hintNeed<0||Model.Need(n)<Model.Need(hintNeed)))hintNeed=n;
    careHintAge=0;Clear(careHintVisual);
    if(hintNeed>=0){var icon=Rect(careHintVisual,"Invitation icon",0,0,64,64);icon.localScale=Vector3.one*.65f;CareIcon(icon,hintNeed==0?0:hintNeed==1?1:2);}
   }
   if(careHintOpacity)careHintOpacity.alpha=hintNeed>=0&&careHintAge<3&&!priority&&t>=1?Mathf.Min(1,3-careHintAge):0;
  }
 }
}
