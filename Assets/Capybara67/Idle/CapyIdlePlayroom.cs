using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  readonly RectTransform[] memoryFront=new RectTransform[6],memoryBack=new RectTransform[6],musicGlow=new RectTransform[4];
  readonly Button[] memoryButtons=new Button[6],noteButtons=new Button[4],paintButtons=new Button[6];
  readonly SoftShape[] paintedPetals=new SoftShape[6],paletteRings=new SoftShape[5];
  static readonly string[] PaintColours={"F19C96","F2CD70","92C9A2","8EC9DD","C5A8DD"};
  static readonly string[] PaintColourNames={"Розовый","Жёлтый","Зелёный","Голубой","Сиреневый"};
  Text playroomHint,orchestraBanner;
  readonly RectTransform[] musicStars=new RectTransform[13];
  RectTransform picnicPlate,picnicFoodArt,paintFlower;
  Button paintingDone;
  int picnicShown=-1,musicLastCue=-1,playerNote=-1;
  double playerNoteUntil;

  static void ButterflyBadge(Transform p,float x,float y,float scale){
   var r=Rect(p,"Painted butterfly",x,y,80,80);r.localScale=Vector3.one*scale;
   Oval(r,"Upper left wing",4,4,34,40,"BDA2D2",2);Oval(r,"Upper right wing",38,4,34,40,"9CBBD8",2);
   Oval(r,"Lower left wing",13,35,25,29,"E9B5C6",2);Oval(r,"Lower right wing",38,35,25,29,"E9B5C6",2);
   Oval(r,"Wing jewel",14,12,12,17,"F9E2A1",1);Oval(r,"Wing jewel",49,12,12,17,"F9E2A1",1);
   Stroke(r,"Body",37,23,37,59,Ink,5);Stroke(r,"Feelers",36,25,28,12,Ink,2);Stroke(r,"Feelers",39,25,46,12,Ink,2);
  }
  static void ActivityMedallion(Transform p,int kind){
   Oval(p,"Medallion shadow",1,6,77,73,"65432D20",0);Oval(p,"Medallion",0,0,78,74,"FFF1CE",2);
   if(kind==0){ActivityBasketArt(p,8,37,.56f);ActivityMandarin(p,24,10,33);}
   if(kind==1){ActivityMandarin(p,21,15,40);Leaf(p,2,42,41,29,"8EB76C",-12);Leaf(p,45,27,26,42,"A5C882",10);}
   if(kind==2){InkBox(p,"Back card",12,12,37,48,"C5A8DD",7,2);InkBox(p,"Front card",32,23,36,45,"FFF7DC",7,2);Heart(p,39,33,23,"DD91A4");}
   if(kind==3){for(int n=0;n<4;n++){InkBox(p,"Marimba key",10+n*15,30-n*4,13,33+n*4,PaintColours[n],4,1.5f);}Stroke(p,"Mallet",15,13,51,38,Ink,3);Oval(p,"Mallet head",8,6,15,15,"EAC57F",1.5f);}
   if(kind==4){Shape(p,"Picnic cloth",10,40,60,25,"E5A496",5);Oval(p,"Plate",17,23,46,37,"FFFFFF",2);ActivityMandarin(p,24,19,27);Leaf(p,45,30,19,16,"94BA7B",-10);}
   if(kind==5){for(int n=0;n<6;n++){float a=n*Mathf.PI/3;Oval(p,"Paint petal",29+Mathf.Cos(a)*21,24+Mathf.Sin(a)*21,22,26,PaintColours[n%5],1.5f);}Oval(p,"Flower heart",27,25,27,27,"F8D783",2);}
  }
  static void PicnicFood(Transform p,int group,int variant,float size){
   var r=Rect(p,"Picnic food",0,0,80,80);r.localScale=Vector3.one*(size/80);
   if(variant%2==1){MorePicnicFood(r,group);return;}
   if(group==0){ActivityMandarin(r,8,14,62);if(variant%2==1)Leaf(r,45,0,30,19,"A4C57C",-10);}
   if(group==1){
    var carrot=Rounded(25,0,219,0,240,30,134,150,94,143,0,30);
    Contour(r,"Carrot outline",17,20,47,58,Ink,carrot);Contour(r,"Carrot flesh",20,22,41,51,"EDAA62",carrot);
    Leaf(r,22,1,25,30,"8EBB72",-15);Leaf(r,40,2,22,29,"ADC97F",18);
    Stroke(r,"Carrot stripe",25,35,40,40,"C77E45",2);Stroke(r,"Carrot stripe",39,51,51,48,"C77E45",2);Stroke(r,"Carrot shine",26,26,26,33,"FFE2A0",3);
   }
   if(group==2){
    for(int n=0;n<3;n++){float x=8+n*21,y=25-(n%2)*15;Oval(r,"Strawberry",x,y,30,40,n%2==0?"DC8D92":"E5A4AE",2);Leaf(r,x+2,y-6,27,14,"8DB777",-8);for(int j=0;j<4;j++)Oval(r,"Seed",x+8+j%2*10,y+14+j/2*11,3,4,"FFE9AB",0);}
   }
  }
  static void FinishedPainting(Transform p,IdleActivity a){
   var art=Rect(p,"Your finished painting",5,0,432,444);art.localScale=Vector3.one*.6f;PaintingBase(art,a);
   for(int n=0;n<6;n++){var r=PaintRegion(a,n);int colour=(a.colors>>(n*3))&7;Oval(art,"Your coloured region",r.x+1,r.y+1,r.width-2,r.height-2,colour==0?"E9E1D0":PaintColours[(colour-1)%5],2.5f);var pattern=Rect(art,"Your painted pattern",r.x,r.y,r.width,r.height);PaintPattern(pattern,(a.patterns>>(n*2))&3,r.width,r.height);}
   PaintingFace(art,a);
  }
  void PlayroomBackdrop(int kind){
   string[] skies={"F7DECE","303F62","F9DFB4","D0E9DB"};
   Shape(activityField,"Playroom sky",0,0,432,444,skies[kind-2],18);
   if(kind==3){
    Oval(activityField,"Moon halo",350,-3,62,62,"EFDFA023",0);Oval(activityField,"Moon",365,7,32,32,"F9E6B4",1);
    for(int n=0;n<13;n++){musicStars[n]=Rect(activityField,"Twinkling music star",15+n*31,17+(n*43)%86,16,16);Star(musicStars[n],0,0,8+n%3*3,n%2==0?"E6D799":"A4BCCB");}
    for(int n=0;n<6;n++)Leaf(activityField,-10+n*87,370,59,98,"617E7C",n%2==0?-16:16);
   }else{
    Shape(activityField,"Cloud",34,18,102,28,"FFF9E4",0,true);Shape(activityField,"Cloud",280,47,99,23,"FFF9E4",0,true);
    Oval(activityField,"Sun",322,6,52,52,"F5D790",1.5f);
    for(int n=0;n<9;n++)ActivityFlower(activityField,8+n*49,399+(n%2)*12,.8f,PaintColours[n%5]);
   }
   if(kind==2){InkBox(activityField,"Memory table",8,12,416,418,"C59A77",18,3);Shape(activityField,"Velvet cloth",16,20,400,402,"F5E8CB",14);for(int n=0;n<7;n++)Stroke(activityField,"Cloth stitching",25+n*59,31,45+n*59,31,"DAB79B",2);}
   if(kind==4){
    InkBox(activityField,"Picnic blanket",6,26,420,409,"F9EAC9",15,3);
    for(int row=0;row<7;row++)for(int col=0;col<7;col++)if((row+col)%2==0)Shape(activityField,"Gingham square",13+col*58,33+row*56,56,54,"E6B6A040",3);
    ActivityFlower(activityField,10,5,1.3f,"E6B3CE");ActivityFlower(activityField,394,6,1.2f,"FFF1BF");
   }
  }
  void BuildPlayroom(int kind){
   PlayroomBackdrop(kind);musicLastCue=-1;playerNote=-1;picnicShown=-1;picnicFlightAt=-100;picnicDragging=false;picnicHintGroup=-1;musicSuccessUntil=0;musicCompletedSteps=0;paintLastAction=Model.S.activity.elapsed;paintFeedbackUntil=0;brushSampleShown=-1;
   if(kind!=5)playroomHint=Label(panel,"Playroom hint","",28,kind==3?687:695,424,kind==3?63:104,23,align:TextAnchor.MiddleCenter);else playroomHint=null;
   if(kind==2)BuildMemory();else if(kind==3)BuildMusic();else if(kind==4)BuildPicnic();else BuildPainting();
  }
  void PlayroomPop(RectTransform target){
   if(!target)return;var motion=target.GetComponent<CapyPlayroomPopMotion>()??target.gameObject.AddComponent<CapyPlayroomPopMotion>();motion.Play(tweens,Model.S.calm);
  }
  void BuildMemory(){
   var a=Model.S.activity;
   for(int n=0;n<6;n++){
    int card=n;float x=22+n%3*134,y=58+n/3*170;
    var button=Button(activityField,"Memory card "+n,"",x,y,120,151,()=>{if(Model.ActivityCard(card)){PlayroomPop(memoryButtons[card].GetComponent<RectTransform>());Sound(careClip,.12f);if(Model.S.activity.second>=0&&(Model.S.activity.mask&(1<<card))!=0)HarvestFeedback(new Vector2(x+60,-y-70));Save();RefreshPlayroom();}},"FFF6DE");memoryButtons[n]=button;
    memoryKnown[n]=false;memoryFaces[n]=Rect(button.transform,"Turning card",0,0,120,151);memoryFaces[n].pivot=new Vector2(.5f,.5f);memoryFaces[n].anchoredPosition=new Vector2(60,-75.5f);memoryBack[n]=Rect(memoryFaces[n],"Card back",7,7,106,137);InkBox(memoryBack[n],"Quilted back",0,0,106,137,"C4AACF",12,2);
    for(int j=0;j<3;j++)Stroke(memoryBack[n],"Quilt stitch",12,26+j*38,93,26+j*38,"E3CEE4",2);
    Heart(memoryBack[n],31,45,45,"F6DEC1");for(int j=0;j<4;j++)ActivityFlower(memoryBack[n],j%2==0?7:80,j<2?7:103,.65f,"F8E7C4");
    memoryFront[n]=Rect(memoryFaces[n],"Card face",17,33,86,90);Oval(memoryFront[n],"Illustration halo",0,0,84,83,"F5DFAC",0);
    int face=IdleModel.MemoryFace(a,n);if(face==0)ActivityMandarin(memoryFront[n],5,7,70);else if(face==1)ButterflyBadge(memoryFront[n],3,6,1);else ActivityMushroom(memoryFront[n],11,5,1.8f);
    var stamp=Label(button.transform,"Pair mark","ПАРА!",7,112,106,31,20,"418355",TextAnchor.MiddleCenter);stamp.gameObject.SetActive(false);
   }
  }
  void BuildMusic(){
   orchestraBanner=Label(activityField,"Orchestra banner","СЛУШАЙ И СМОТРИ",21,45,390,43,24,"FFF1CE",TextAnchor.MiddleCenter);orchestraBanner.font=bold;
   for(int n=0;n<4;n++){musicSteps[n]=Rect(activityField,"Melody step "+n,151+n*34,98,24,24);var dot=Oval(musicSteps[n],"Step light",0,0,24,24,"F3D690",2);musicStepFill[n]=dot.transform.GetChild(0).GetComponent<SoftShape>();melodyChecks[n]=PlayroomCheck(musicSteps[n],"Melody check",0,0,24);}
   for(int n=0;n<4;n++){
    int note=n;float x=30+n%2*200,y=137+n/2*142;
    var b=Button(activityField,"Music note "+n,"",x,y,172,123,()=>{
     var a=Model.S.activity;if(PresentationPaused||a==null||a.complete||IdleModel.MusicShowing(a))return;
     int previousRound=a.round,previousLength=IdleModel.MusicLength(a);
     bool accepted=Model.ActivityNote(note);musicSuccessUntil=accepted?a.elapsed+.55:0;musicCompletedSteps=accepted&&a.round!=previousRound?previousLength:0;playerNote=note;playerNoteUntil=Model.S.activity.elapsed+.35;Sound(activityNotes[note],.3f);
     if(accepted)HarvestFeedback(new Vector2(x+86,-y-56));Save();RefreshPlayroom();
    },PaintColours[n]);noteButtons[n]=b;
    musicGlow[n]=Rect(b.transform,"Lit note",3,3,166,117);Shape(musicGlow[n],"Glow",0,0,166,117,"FFF5C994",17);
    noteInstruments[n]=Rect(b.transform,"Marimba instrument",0,0,172,90);Marimba(noteInstruments[n],n);
    Label(b.transform,"Note name",new[]{"ДО","РЕ","МИ","СОЛЬ"}[n],20,77,132,36,23,align:TextAnchor.MiddleCenter).font=bold;
   }
   musicRepeat=Button(panel,"Repeat melody","ПОСЛУШАТЬ ЕЩЁ",46,760,388,67,()=>{if(Model.RepeatMusic()){musicLastCue=-1;Save();RefreshPlayroom();}},"B7B4E4",22);
  }
  void BuildPicnic(){
   Oval(activityField,"Plate shadow",134,185,166,34,"805F4930",0);Oval(activityField,"Large picnic plate",126,57,180,159,"FFF9EA",3);Oval(activityField,"Plate inset",140,68,153,130,"E2EEE2",1.5f);
   picnicPlate=Rect(activityField,"Picnic serving",158,82,112,112);picnicFoodArt=Rect(picnicPlate,"Serving illustration",0,0,112,112);
   Label(activityField,"Sort invitation","В КАКУЮ КОРЗИНКУ?",20,222,392,38,23,align:TextAnchor.MiddleCenter).font=bold;
   for(int n=0;n<3;n++){
    int group=n;float x=6+n*143;
    var b=Button(activityField,"Picnic basket "+n,"",x,278,134,143,()=>SubmitPicnic(group,picnicPlate.anchoredPosition),new[]{"F1D69B","BAD19C","E8BAC1"}[n]);picnicBaskets[n]=b;
    picnicGuides[n]=Rect(b.transform,"Picnic basket guide",4,4,126,103);InkBox(picnicGuides[n],"Guide light",0,0,126,103,"FFF4B6",13,2);picnicGuides[n].gameObject.SetActive(false);
    picnicBasketArt[n]=Rect(b.transform,"Basket reaction root",0,0,134,105);
    var sample=Rect(picnicBasketArt[n],"Food sample",39,5,80,80);PicnicFood(sample,n,0,58);ActivityBasketArt(picnicBasketArt[n],32,67,.64f);
    picnicChecks[n]=PlayroomCheck(b.transform,"Picnic basket success",94,8,29);picnicChecks[n].gameObject.SetActive(false);
    Label(b.transform,"Basket category",new[]{"Фрукты","Овощи","Ягоды"}[n],4,109,126,30,21,align:TextAnchor.MiddleCenter).font=bold;
   }
   picnicPlate.SetAsLastSibling();BuildPicnicDrag();picnicFlight=Rect(activityField,"Flying picnic food",158,82,112,112);picnicFlight.gameObject.SetActive(false);
  }
  void BuildPainting(){
   InkBox(activityField,"Easel leg",91,21,19,419,"B68A60",6,2);InkBox(activityField,"Easel leg",322,21,19,419,"B68A60",6,2);
   var a=Model.S.activity;PaintingBase(activityField,a);
   brushButton=Button(activityField,"Paint brush","",76,15,280,67,()=>{if(PresentationPaused)return;a.brush=(a.brush+1)%3;paintLastAction=a.elapsed;Save();RefreshPlayroom();},"F0D6AF",21);
   var brushCaption=brushButton.GetComponentInChildren<Text>().rectTransform;brushCaption.anchorMin=brushCaption.anchorMax=brushCaption.pivot=new Vector2(0,1);brushCaption.anchoredPosition=new Vector2(65,-7);brushCaption.sizeDelta=new Vector2(206,53);
   brushSample=Rect(brushButton.transform,"Paint brush sample",12,11,44,44);
   paintFlower=Rect(activityField,"Painted flower",0,0,432,300);
   for(int n=0;n<6;n++){
    int petal=n;var region=PaintRegion(a,n);float x=region.x,y=region.y;
    var b=Button(paintFlower,"Paint petal "+n,"",x,y,region.width,region.height,()=>{if(!PresentationPaused&&Model.ActivityPaint(petal,Model.S.activity.selection)){paintLastAction=a.elapsed;paintFeedbackUntil=a.elapsed+.65;PlayroomPop(paintButtons[petal].GetComponent<RectTransform>());HarvestFeedback(new Vector2(x+region.width/2,-y-region.height/2));Sound(careClip,.12f);Save();RefreshPlayroom();}},"FFFFFF00");paintButtons[n]=b;
    paintedPetals[n]=Oval(b.transform,"Petal paint",1,1,region.width-2,region.height-2,"E9E1D0",2.5f);Shape(b.transform,"Petal glint",22,13,25,12,"FFFFFF70",0,true);paintPatterns[n]=Rect(b.transform,"Paint pattern",0,0,region.width,region.height);paintPatternShown[n]=-1;
    paintGuides[n]=Rect(b.transform,"Paint region guide",region.width*.5f-13,region.height*.5f-13,26,26);Oval(paintGuides[n],"Guide glow",0,0,26,26,"FFF1B1",0);Star(paintGuides[n],5,5,16,"C68E42");paintGuides[n].gameObject.SetActive(false);
   }
   paintEyes=PaintingFace(paintFlower,a);
   for(int n=0;n<5;n++){
    int colour=n;float x=24+n*88;var b=Button(panel,"Paint colour "+n,"",x,704,80,70,()=>{if(PresentationPaused)return;Model.S.activity.selection=colour;paintLastAction=a.elapsed;paintFeedbackUntil=0;Save();RefreshPlayroom();},PaintColours[n]);
    paletteRings[n]=Oval(b.transform,"Selected paint",23,15,34,34,"FFF5D5",2);Mark(b.transform,(n+1).ToString(),22,12,36,38,23,Ink);
   }
   Shape(activityField,"Paint colour caption backing",76,406,280,36,"FFF1CE",12);
   paintColourConfirmation=Label(activityField,"Paint colour confirmation","",82,406,268,35,20,align:TextAnchor.MiddleCenter);
   paintingDone=Button(panel,"Finish painting","ГОТОВО!",24,779,432,67,()=>{if(Model.FinishPainting()){Save();Show("activity-result");}},"55E6C1",24);
  }
  void RefreshPlayroom(){
   var a=Model.S.activity;if(a==null||a.complete)return;
   if(a.kind==2){
    bool preview=IdleModel.MemoryPreview(a);
    for(int n=0;n<6;n++){
     bool matched=(a.mask&(1<<n))!=0,open=preview||matched||a.first==n||a.second==n;
     TurnMemoryCard(n,open);memoryButtons[n].interactable=!preview&&!matched&&a.second<0&&a.first!=n;
     memoryButtons[n].transform.Find("Pair mark").gameObject.SetActive(matched);
    }
    playroomHint.text=preview?"Запомни картинки! Сейчас спрячутся":a.second>=0?((a.mask&(1<<a.second))!=0?"Есть пара! Молодец!":"Запомним картинки и попробуем ещё"):a.first>=0?"Где такая же картинка?":"Открывай по две карточки";
   }else if(a.kind==3){
    bool demo=IdleModel.MusicShowing(a);int cue=demo?IdleModel.MusicCue(a):-1;
    bool praised=a.elapsed<musicSuccessUntil, retry=demo&&a.elapsed<a.feedbackUntil;
    orchestraBanner.text=praised?"ВЕРНО!":retry?"ДАВАЙ ЕЩЁ РАЗ":demo?"СЛУШАЙ И СМОТРИ":"ПОВТОРИ МЕЛОДИЮ";
    orchestraBanner.color=C(praised?"BCE3B5":retry?"F3D690":"FFF1CE");
    for(int n=0;n<13;n++)musicStars[n].localScale=Vector3.one*(Model.S.calm?1:.85f+.15f*Mathf.Sin((float)a.elapsed*2+n));
    if(cue>=0){int token=a.round*10+(int)((a.elapsed-a.sequenceAt)/.7);if(token!=musicLastCue){musicLastCue=token;Sound(activityNotes[cue],.24f);}}
    for(int n=0;n<4;n++){bool lit=n==cue||a.elapsed<playerNoteUntil&&playerNote==n;musicGlow[n].gameObject.SetActive(lit);noteInstruments[n].anchoredPosition=new Vector2(0,lit&&!Model.S.calm?-4:0);noteButtons[n].interactable=!demo&&a.round<3;int length=praised&&musicCompletedSteps>0?musicCompletedSteps:IdleModel.MusicLength(a);musicSteps[n].gameObject.SetActive(n<length);musicSteps[n].anchoredPosition=new Vector2(216-((length-1)*34+24)/2f+n*34,-98);bool checkedStep=(!demo&&n<a.step)||(praised&&n<musicCompletedSteps);melodyChecks[n].gameObject.SetActive(checkedStep);musicStepFill[n].color=C(checkedStep?"81BC92":demo&&cue>=0&&n==(int)((a.elapsed-a.sequenceAt)/.7)?"FFF8DC":"F3D690");}
    musicRepeat.interactable=!demo&&a.round<3;
    playroomHint.text=a.round>=3?"Отличная мелодия!":demo?(a.feedbackUntil>a.elapsed?"Попробуем ещё!":"Смотри на огоньки"):("Повтори мелодию · "+(a.round+1)+" / 3");
   }else if(a.kind==4){
    if(!picnicDragging)picnicPlate.anchoredPosition=new Vector2(158,-82+(Model.S.calm?0:Mathf.Sin((float)a.elapsed*2)*3));
    bool ready=a.elapsed>=a.readyAt&&a.score<6;picnicPlate.gameObject.SetActive(ready);
    for(int n=0;n<3;n++){
     picnicBaskets[n].interactable=ready;
     bool guide=ready&&a.elapsed<a.feedbackUntil&&n==picnicHintGroup;
     bool caught=n==picnicFlyingGroup&&a.elapsed>=picnicFlightAt&&a.elapsed<picnicFlightAt+.42;
     picnicGuides[n].gameObject.SetActive(guide);picnicChecks[n].gameObject.SetActive(caught);
     float response=caught&&!Model.S.calm?Mathf.Sin(Mathf.Clamp01((float)(a.elapsed-picnicFlightAt)/.42f)*Mathf.PI)*5:0;
     picnicBasketArt[n].anchoredPosition=new Vector2(0,response);
    }
    if(ready&&picnicShown!=a.score){Clear(picnicFoodArt);PicnicFood(picnicFoodArt,IdleModel.PicnicGroup(a),a.variant+a.score,112);picnicShown=a.score;PlayroomPop(picnicFoodArt);}
    float flight=Mathf.Clamp01((float)(a.elapsed-picnicFlightAt)/.42f);picnicFlight.gameObject.SetActive(!Model.S.calm&&flight<1);
    if(flight<1){picnicFlight.anchoredPosition=Vector2.Lerp(picnicFlightStart,new Vector2(17+143*picnicFlyingGroup,-298),flight)+new Vector2(0,Mathf.Sin(flight*Mathf.PI)*45);picnicFlight.localScale=Vector3.one*Mathf.Lerp(1,.45f,flight);picnicFlight.localEulerAngles=new Vector3(0,0,Mathf.Sin(flight*Mathf.PI)*15);}
    playroomHint.text=a.elapsed<picnicFlightAt+.42?"Всё верно!":a.feedbackUntil>a.elapsed&&picnicHintGroup>=0?"Смотри: подсвечена нужная корзинка":"Перетащи еду в корзинку\nили просто нажми на неё";
   }else if(a.kind==5){
    for(int n=0;n<6;n++){int colour=(a.colors>>(n*3))&7;var paint=paintedPetals[n].transform.GetChild(0).GetComponent<SoftShape>();paint.color=C(colour==0?"E9E1D0":PaintColours[(colour-1)%5]);int pattern=(a.patterns>>(n*2))&3;if(paintPatternShown[n]!=pattern){Clear(paintPatterns[n]);var r=PaintRegion(a,n);PaintPattern(paintPatterns[n],pattern,r.width,r.height);paintPatternShown[n]=pattern;}}
    int unpainted=-1;for(int n=0;n<6;n++)if((a.mask&(1<<n))==0){unpainted=n;break;}
    for(int n=0;n<6;n++){bool guide=n==unpainted&&a.elapsed-paintLastAction>=2.5;paintGuides[n].gameObject.SetActive(guide);paintGuides[n].localScale=Vector3.one*(Model.S.calm?1:1+.08f*Mathf.Sin((float)a.elapsed*3));}
    if(brushSampleShown!=a.brush){Clear(brushSample);Oval(brushSample,"Brush swatch",0,0,44,44,"92C9A2",2);PaintPattern(brushSample,a.brush,44,44);brushSampleShown=a.brush;}
    paintColourConfirmation.text=PaintColourNames[a.selection]+(a.elapsed<paintFeedbackUntil?" — готово!":"");
    brushButton.GetComponentInChildren<Text>().text="Кисть: "+new[]{"гладко","точки","полоски"}[a.brush]+"  →";
    float blink=Model.S.calm?1:(a.elapsed%4.8>4.6?.15f:1);foreach(Transform eye in paintEyes)eye.localScale=new Vector3(1,blink,1);
    for(int n=0;n<5;n++)paletteRings[n].gameObject.SetActive(n==a.selection);
    paintingDone.interactable=a.score==6;paintingDone.GetComponentInChildren<Text>().text=a.score==6?"ГОТОВО!":"РАСКРАСЬ ЕЩЁ "+(6-a.score);
   }
  }
 }
}
