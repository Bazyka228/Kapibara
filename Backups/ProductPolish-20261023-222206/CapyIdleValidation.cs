using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Capybara67.Idle;
using DG.Tweening;
namespace Capybara67.Editor {
 [InitializeOnLoad] public static class CapyIdleValidation {
  static bool running,capture;static int checks;const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  static int renderErrors;
  static void TrackRenderError(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)renderErrors++;}
  static CapyIdleValidation(){EditorApplication.update+=Request;}
  static void Request(){if(!File.Exists("Temp/Capy67-idle-checks")||running||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   if(!EditorApplication.isPlaying){if(EditorApplication.isPlayingOrWillChangePlaymode)return;CapyIdleSetup.Open();SessionState.SetBool("CapyIdle.auto",true);SessionState.SetBool("CapyIdle.background",Application.runInBackground);Application.runInBackground=true;EditorApplication.EnterPlaymode();return;}
   var game=UnityEngine.Object.FindAnyObjectByType<CapyIdleGame>();if(!game||game.Model==null)return;File.Delete("Temp/Capy67-idle-checks");game.StartCoroutine(RunSafely(game));
  }
  // Unity does not propagate exceptions from a yielded child iterator into its parent.
  // Drive children here so disposing the parent always restores the profile and unlocks reload.
  static IEnumerator RunSafely(CapyIdleGame game){
   var pending=new Stack<IEnumerator>();pending.Push(Run(game));
   try{
    while(pending.Count>0){
     var current=pending.Peek();bool next=false;object yielded=null;Exception error=null;
     try{next=current.MoveNext();if(next)yielded=current.Current;}catch(Exception e){error=e;}
     if(error!=null){File.WriteAllText("Logs/Capy67-idle-checks.txt","FAIL: "+error.Message+" after "+checks);Debug.LogException(error);yield break;}
     if(!next){pending.Pop();(current as IDisposable)?.Dispose();continue;}
     if(yielded is IEnumerator child)pending.Push(child);else yield return yielded;
    }
   }finally{while(pending.Count>0)(pending.Pop() as IDisposable)?.Dispose();}
  }
  static void Check(bool ok,string label){if(!ok){File.WriteAllText("Logs/Capy67-idle-checks.txt","FAIL: "+label+" after "+checks);throw new Exception("Idle integration: "+label);}checks++;}
  static void Call(CapyIdleGame g,string method,params object[] args)=>typeof(CapyIdleGame).GetMethod(method,F).Invoke(g,args);
  static Button Find(CapyIdleGame g,string name){foreach(var b in g.GetComponentsInChildren<Button>(true))if(b.name==name&&b.gameObject.activeInHierarchy)return b;return null;}
  static bool Clickable(Button b){if(!b)return false;Canvas.ForceUpdateCanvases();var r=(RectTransform)b.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);return hits.Count>0&&hits[0].gameObject==b.gameObject;}
  // Export only the game's rendered frame, never the desktop. Explicit opt-in for visual QA.
  static void Frame(string name){if(!capture)return;var texture=ScreenCapture.CaptureScreenshotAsTexture();try{File.WriteAllBytes("Logs/GardenReview/"+name+".png",texture.EncodeToPNG());}finally{UnityEngine.Object.Destroy(texture);}}
  static void Typography(CapyIdleGame g,string page){
   foreach(var text in g.GetComponentsInChildren<Text>()){
    if(string.IsNullOrEmpty(text.text))continue;
    Check(text.font&&text.font.name.StartsWith("Nunito"),"Nunito font on "+page+"/"+text.name);
    if(text.preferredHeight>text.rectTransform.rect.height+.5f)
     File.AppendAllText("Logs/Capy67-layout.txt",page+"/"+text.name+": "+text.preferredHeight+" > "+text.rectTransform.rect.height+" : "+text.text.Replace('\n',' ') +"\n");
   }
  }
  static IEnumerator CheckEyePixels(CapyIdleGame game,Image portrait,string label){
   var motion=portrait.GetComponent<CapyPortraitMotion>();var r=portrait.rectTransform;
   bool wasSuspended=(bool)typeof(CapyIdleGame).GetField("suspended",F).GetValue(game);
   float oldClosure=motion.Closure;Texture2D before=null,after=null;
   typeof(CapyIdleGame).GetField("suspended",F).SetValue(game,true);
   try{
    Check(!ShaderUtil.ShaderHasError(portrait.material.shader),"eye shader compiles "+label);
    motion.SetBlink(0);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
    before=ScreenCapture.CaptureScreenshotAsTexture();var vertices=portrait.canvasRenderer.GetMesh().vertices;
    motion.SetBlink(1);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
    after=ScreenCapture.CaptureScreenshotAsTexture();var closedVertices=portrait.canvasRenderer.GetMesh().vertices;
    Check(vertices.Length==4&&closedVertices.Length==4,"portrait remains a quad "+label);
    for(int i=0;i<vertices.Length;i++)Check(Vector3.Distance(vertices[i],closedVertices[i])<.00001f,"blink leaves vertex stationary "+label+" "+i);
    var a=before.GetPixels32();var b=after.GetPixels32();var corners=new Vector3[4];r.GetWorldCorners(corners);
    float minX=float.MaxValue,minY=float.MaxValue,maxX=float.MinValue,maxY=float.MinValue;
    foreach(var corner in corners){var screen=RectTransformUtility.WorldToScreenPoint(null,corner);minX=Mathf.Min(minX,screen.x);minY=Mathf.Min(minY,screen.y);maxX=Mathf.Max(maxX,screen.x);maxY=Mathf.Max(maxY,screen.y);}
    var padding=new Vector2(2/Mathf.Max(1,maxX-minX),2/Mathf.Max(1,maxY-minY));int outside=0,inside=0,visibleLidPixels=0;
    for(int y=Mathf.Max(0,Mathf.FloorToInt(minY));y<Mathf.Min(before.height,Mathf.CeilToInt(maxY));y++)
     for(int x=Mathf.Max(0,Mathf.FloorToInt(minX));x<Mathf.Min(before.width,Mathf.CeilToInt(maxX));x++){
      int p=y*before.width+x;int delta=Mathf.Abs(a[p].r-b[p].r)+Mathf.Abs(a[p].g-b[p].g)+Mathf.Abs(a[p].b-b[p].b);
      bool dark=b[p].r<100&&b[p].g<80&&b[p].b<70;if(delta<=3&&!dark)continue;
      Vector2 local;RectTransformUtility.ScreenPointToLocalPointInRectangle(r,new Vector2(x+.5f,y+.5f),null,out local);
      var normalized=new Vector2((local.x-r.rect.x)/r.rect.width,(local.y-r.rect.y)/r.rect.height);
      if(motion.IsEyePixel(normalized,Vector2.zero)&&dark)visibleLidPixels++;
      if(delta<=3)continue;
      if(motion.IsEyePixel(normalized,padding))inside++;else outside++;
     }
    bool failedEye=outside!=0||(motion.AnimatedEyeCount>0&&(inside==0||visibleLidPixels<motion.AnimatedEyeCount*2));
    if(capture&&(failedEye||label=="base-2"||label=="base-sleep-2"||label=="duck-apron-2")){
     File.WriteAllBytes("Logs/GardenReview/eyes-open-"+label+".png",before.EncodeToPNG());
     File.WriteAllBytes("Logs/GardenReview/eyes-closed-"+label+".png",after.EncodeToPNG());
    }
    Check(outside==0,"blink changes no pixels outside eyes "+label+" ("+outside+")");
    Check(motion.AnimatedEyeCount==0?inside==0:inside>0,"open eyes close and already closed eyes stay still "+label+" (inside="+inside+", eyes="+motion.AnimatedEyeCount+")");
    Check(motion.AnimatedEyeCount==0||visibleLidPixels>=motion.AnimatedEyeCount*2,"closed eyelids remain visible at game resolution "+label+" ("+visibleLidPixels+")");
   }finally{
    if(before)UnityEngine.Object.Destroy(before);if(after)UnityEngine.Object.Destroy(after);
    motion.SetBlink(oldClosure);typeof(CapyIdleGame).GetField("suspended",F).SetValue(game,wasSuspended);
   }
  }
  static void CheckFiveStageProgression(){
   double[] oldGrowth={0,6,360,1800,6000,14000,30000,70000,150000};int[] expected={1,2,3,3,3,4,4,4,5};
   for(int i=0;i<oldGrowth.Length;i++){
    var save=new IdleSave{growth=oldGrowth[i],coins=12345,stars=17,bed=8,quest=24,cares=123,feeds=45,form="chill",starter=true};
    save.owned.AddRange(new[]{"moon","classic","cap","scarf"});save.forms.Add("chill");save.equipped=new[]{"cap","scarf","moon","","","classic"};
    var restored=new IdleModel(JsonUtility.FromJson<IdleSave>(JsonUtility.ToJson(save)));
    Check(restored.Stage==expected[i]&&restored.S.growth==oldGrowth[i],"old growth remains intact at former age "+(i+1));
    Check(restored.S.coins==12345&&restored.S.stars==17&&restored.S.bed==8&&restored.S.quest==24&&restored.S.cares==123&&restored.S.feeds==45&&restored.S.starter,"old economy and care progress retained "+i);
    Check(restored.S.form=="chill"&&restored.S.owned.Contains("cap")&&restored.S.owned.Contains("scarf")&&restored.S.owned.Contains("set_postman")&&restored.S.owned.Contains("set_snowberry")&&restored.S.equipped[0]=="set_postman"&&restored.S.equipped[1]=="","old form and outfit retained "+i);
    Check(restored.StageFraction>=0&&restored.StageFraction<=1&&!double.IsNaN(restored.StageFraction),"finite five-stage growth fraction "+i);
   }
   var model=new IdleModel();
   for(int stage=2;stage<=5;stage++){model.S.growth=IdleModel.Growth[stage-1]-.001;Check(model.Stage==stage-1,"growth just before transition "+stage);model.S.growth+=.001;Check(model.Stage==stage,"growth reaches transition "+stage);}
   model.S.growth=150000;model.S.coins=1e9;
   for(int bed=0;bed<8;bed++)Check(model.BuyBed(),"all bed upgrades remain reachable "+bed);
   foreach(var item in IdleCatalog.Items)Check(item.stage>=1&&item.stage<=5,"wardrobe unlock uses existing stage "+item.id);
   foreach(var food in IdleModel.Foods)Check(food.stage>=1&&food.stage<=5,"food unlock uses existing stage "+food.name);
   for(int quest=4;quest<40;quest+=5){model.S.quest=quest;Check(model.QuestGoal<=5&&model.QuestDone,"growth quest remains achievable "+quest);}
   Check(model.Stage==5&&model.StageFraction==1,"guardian completes growth without out-of-range lookup");
  }
  static IEnumerator CheckWelcomeUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false});
   var pending=typeof(CapyIdleGame).GetField("welcomeOfflinePending",F);var gift=typeof(CapyIdleGame).GetField("welcomeGiftPending",F);var award=typeof(CapyIdleGame).GetField("offlineAward",F);
   object oldPending=pending.GetValue(g),oldGift=gift.GetValue(g),oldAward=award.GetValue(g);
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);pending.SetValue(g,false);gift.SetValue(g,false);award.SetValue(g,0d);
   try{
    g.Show("welcome");yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    Check(Find(g,"Welcome start").GetComponentInChildren<Text>().text=="НАЧАТЬ ЗАБОТУ","new player sees start-care invitation");
    Check(Clickable(Find(g,"Welcome start"))&&Clickable(Find(g,"Welcome pet")),"welcome main and pet controls are reachable");
    int cares=m.S.cares;g.Care();Check(m.S.cares==cares,"welcome screen blocks gameplay keyboard taps");
    Find(g,"Welcome pet").onClick.Invoke();Call(g,"AnimateWelcome",.4f);
    var pose=(RectTransform)typeof(CapyIdleGame).GetField("welcomePose",F).GetValue(g);var scale=pose.localScale;
    typeof(CapyIdleGame).GetField("platformPaused",F).SetValue(g,true);Call(g,"AnimateWelcome",1f);
    Check(pose.localScale==scale,"welcome breathing respects pause");typeof(CapyIdleGame).GetField("platformPaused",F).SetValue(g,false);
    m.S.calm=true;Call(g,"AnimateWelcome",0f);Check(pose.localScale==Vector3.one,"welcome reduced motion stays still");m.S.calm=false;
    Typography(g,"welcome-new");Frame("welcome-new");Find(g,"Welcome start").onClick.Invoke();Check(g.Page=="home","new player starts directly at pet");
    m.S.starter=true;m.Grant("set_gardener");m.Purchase("set_gardener");
    for(int stage=1;stage<=5;stage++){
     m.S.growth=IdleModel.Growth[stage-1];g.Show("welcome");yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();
     Typography(g,"welcome-stage-"+stage);Frame("welcome-stage-"+stage);
     Check(Find(g,"Welcome start").GetComponentInChildren<Text>().text=="ПРОДОЛЖИТЬ","returning player continues stage "+stage);
    }
    m.S.bed=1;m.S.lastUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-3600;
    double firstAward=m.Arrive(DateTimeOffset.UtcNow.ToUnixTimeSeconds());award.SetValue(g,firstAward);pending.SetValue(g,true);g.Show("welcome");
    Call(g,"Suspend",true);m.S.lastUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-60;Call(g,"Suspend",false);
    Check(g.Page=="welcome"&&(double)award.GetValue(g)>firstAward,"focus return keeps welcome and accumulates pending offline summary");
    double balance=m.S.coins;Find(g,"Welcome start").onClick.Invoke();Check(g.Page=="offline"&&m.S.coins==balance,"continue displays offline award without crediting twice");
    Find(g,"Continue idle").onClick.Invoke();Check(g.Page=="home"&&!(bool)pending.GetValue(g),"offline continue clears pending welcome reward");
   }finally{
    pending.SetValue(g,oldPending);gift.SetValue(g,oldGift);award.SetValue(g,oldAward);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");
   }
  }
  static Vector2 ScreenPoint(RectTransform r,Vector2 local)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(local));
  static IEnumerator CheckCareGestures(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=150000,coins=1000000,sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);
    for(int food=0;food<IdleModel.Foods.Length;food++){
     m.Tick(9);typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,food/4);g.Show("food");yield return new WaitForEndOfFrame();
     var b=Find(g,"Food "+food);var drag=b.GetComponent<CapyCareGesture>();
     var e=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=21,eligibleForClick=true,position=ScreenPoint((RectTransform)b.transform,((RectTransform)b.transform).rect.center)};
     double coins=m.S.coins,growth=m.S.growth;int feeds=m.S.feeds;
     drag.OnBeginDrag(e);Check(drag.Active&&!e.eligibleForClick,"food gesture captures without click "+food);
     Check(m.S.coins==coins&&m.S.growth==growth,"drag start is free "+food);
     var target=(RectTransform)Find(g,"Care on character").transform;e.position=ScreenPoint(target,target.rect.center);drag.OnDrag(e);
     // Newly created Graphics acquire their raycast depth during the render pass.
     yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();
     var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
     Check(hits.Count>0&&hits[0].gameObject.name=="Drag input shield","drag shields underlying controls "+food+"; hit="+(hits.Count>0?hits[0].gameObject.name:"none"));
     var wrong=new PointerEventData(EventSystem.current){pointerId=99,position=e.position};drag.OnEndDrag(wrong);Check(drag.Active,"second pointer cannot release meal "+food);
     if(food==4){yield return new WaitForEndOfFrame();Typography(g,"food-drag");Frame("care-drag-meal");}
     // Compare against the same model operation, including form and stage bonuses.
     var expected=new IdleModel(JsonUtility.FromJson<IdleSave>(JsonUtility.ToJson(m.S)));expected.Feed(food,out _);
     drag.OnEndDrag(e);Check(g.Page=="home"&&m.S.feeds==feeds+1,"drop feeds once "+food);
     Check(Math.Abs(m.S.coins-expected.S.coins)<.001&&m.S.growth==expected.S.growth,"drag has tap economics "+food);
     drag.OnEndDrag(e);Check(m.S.feeds==feeds+1,"repeated end cannot duplicate meal "+food);
    }
    typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,0);
    for(int mode=0;mode<5;mode++){
     m.Tick(9);g.Show("food");yield return new WaitForEndOfFrame();
     var drag=Find(g,"Food 0").GetComponent<CapyCareGesture>();
     var e=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=31,position=Vector2.zero};
     double growth=m.S.growth;int feeds=m.S.feeds;drag.OnBeginDrag(e);
     if(mode==0)drag.OnEndDrag(e); // Outside target.
     if(mode==1){Call(g,"PlatformPause",true);Call(g,"PlatformPause",false);}
     if(mode==2){Call(g,"Suspend",true);Call(g,"Suspend",false);}
     if(mode==3)g.Show("settings");
     if(mode==4)drag.enabled=false; // OnDisable must cancel.
     Check(!drag.Active&&m.S.feeds==feeds&&m.S.growth==growth,"cancel gesture preserves food "+mode);
     Check(g.transform.Find("Idle Canvas/Idle board/Food drag overlay")==null||!g.transform.Find("Idle Canvas/Idle board/Food drag overlay").gameObject.activeSelf,"cancel removes drag overlay "+mode);
    }
    m.Tick(9);g.Show("food");yield return new WaitForEndOfFrame();
    var locked=Find(g,"Food 0");locked.interactable=false;
    var blocked=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=11};
    locked.GetComponent<CapyCareGesture>().OnBeginDrag(blocked);Check(!locked.GetComponent<CapyCareGesture>().Active,"unavailable food cannot be dragged");
    m.S.starter=false;g.Show("food");yield return new WaitForEndOfFrame();
    var starter=Find(g,"Starter bowl").GetComponent<CapyCareGesture>();starter.OnBeginDrag(blocked);
    var pet=(RectTransform)Find(g,"Care on character").transform;blocked.position=ScreenPoint(pet,pet.rect.center);starter.OnEndDrag(blocked);Check(m.S.starter,"free first bowl supports drag");
    m.Tick(9);m.S.premiumFood=1;g.Show("food");yield return new WaitForEndOfFrame();
    var premium=Find(g,"Special meal").GetComponent<CapyCareGesture>();premium.OnBeginDrag(blocked);premium.OnEndDrag(blocked);Check(m.S.premiumFood==0,"premium drag consumes one bowl");
    for(int age=1;age<=5;age++){
     m.Tick(12);m.S.growth=IdleModel.Growth[age-1];g.Show("home");
     typeof(CapyIdleGame).GetField("mealAge",F).SetValue(g,10f);typeof(CapyIdleGame).GetField("evolutionAge",F).SetValue(g,10f);
     Call(g,"DoCare",1);Call(g,"AnimateCare",.2f);
     var hero=(RectTransform)g.transform.Find("Idle Canvas/Idle board/Home/Capybara");
     var hit=Find(g,"Care on character");var foam=hit.GetComponent<CapyCareGesture>();var hold=hit.GetComponent<IdleHold>();
     var swipe=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=42,position=ScreenPoint(hero,new Vector2(190,-165))};
     int taps=m.S.cares,feeds=m.S.feeds;double comfort=m.S.comfort,mood=m.S.mood;
     hold.OnPointerDown(swipe);foam.OnBeginDrag(swipe);foam.OnDrag(swipe);Call(g,"AnimateCare",.2f);hold.OnPointerUp(swipe);
     Check(foam.Active&&!hold.Held&&m.S.cares==taps,"foam swipe does not tap pet "+age);
     Check(m.S.comfort==comfort&&m.S.mood==mood&&m.S.feeds==feeds,"foam adds no extra rewards "+age);
     var offset=(Vector2)typeof(CapyIdleGame).GetField("foamOffset",F).GetValue(g);Check(offset.x>0&&offset.y>0,"foam follows finger "+age);
     if(age==2){yield return new WaitForEndOfFrame();Frame("care-bath-swipe");}
     foam.OnEndDrag(swipe);Call(g,"AnimateCare",.2f);Check(((Vector2)typeof(CapyIdleGame).GetField("foamOffset",F).GetValue(g)).magnitude<offset.magnitude,"foam returns softly "+age);
     m.S.calm=true;foam.OnBeginDrag(swipe);Call(g,"AnimateCare",.1f);Check(foam.Active,"reduced motion keeps direct gesture "+age);
     Call(g,"PlatformPause",true);Check(!foam.Active,"pause releases bath pointer "+age);Call(g,"PlatformPause",false);m.S.calm=false;
     Call(g,"AnimateCare",6f);foam.OnBeginDrag(swipe);Check(!foam.Active,"finished bath rejects swipe "+age);
    }
   }finally{Call(g,"Release");typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckTapUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=360,coins=100000,sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);g.Show("home");yield return new WaitForEndOfFrame();
    Call(g,"PlatformPause",false);m.Tick(1);
    var tapHold=Find(g,"Care hold").GetComponent<IdleHold>();var tapPointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=-1};
    int countBefore=m.S.cares;tapHold.OnPointerDown(tapPointer);Check(m.S.cares==countBefore+1,"green button press earns once");
    for(int heldFrame=0;heldFrame<12;heldFrame++){m.Tick(.3);Call(g,"Update");}
    Check(tapHold.Held&&m.S.cares==countBefore+1&&m.S.autoRemaining==0,"holding green button never produces free auto taps");
    tapHold.OnPointerUp(tapPointer);Check(m.S.cares==countBefore+1,"release cannot double count green button tap");
    m.Tick(1);tapHold.OnPointerDown(tapPointer);tapHold.OnPointerUp(tapPointer);Check(m.S.cares==countBefore+2,"second distinct press earns again");
    Check(Clickable(Find(g,"Tap upgrades")),"tap workshop accessible from home");Find(g,"Tap upgrades").onClick.Invoke();
    yield return new WaitForEndOfFrame();Typography(g,"tap-workshop");Frame("tap-workshop");
    for(int n=0;n<3;n++){
     yield return new WaitForEndOfFrame();
     var buy=Find(g,"Tap upgrade "+n);double balance=m.S.coins,price=m.TapPrice(n);
     Check(Clickable(buy),"tap upgrade raycast "+n);buy.onClick.Invoke();
     Check(m.TapLevel(n)==1&&Math.Abs(m.S.coins-(balance-price))<.01,"tap UI purchases once "+n);
    }
    m.S.tapPower=20;m.S.tapMultiplier=m.S.tapBonus=10;g.Show("tap");yield return new WaitForEndOfFrame();Typography(g,"tap-max");Frame("tap-max");
    for(int n=0;n<3;n++)Check(!Find(g,"Tap upgrade "+n).interactable,"max upgrade disabled "+n);
    m.S.tapPower=m.S.tapMultiplier=m.S.tapBonus=0;m.S.growth=0;m.S.coins=0;g.Show("tap");yield return new WaitForEndOfFrame();Typography(g,"tap-locked");
    for(int n=0;n<3;n++)Check(!Find(g,"Tap upgrade "+n).interactable,"poor or young upgrade disabled "+n);
    m.S.coins=1000;m.S.growth=6;
    // Exercise our adapter without opening a real ad: all calls remain inside .validation.
    for(int view=1;view<=5;view++){
     m.S.autoAdNextUtc=0;g.Show("tap");Call(g,"PrepareAutoAd");
     string id=(string)typeof(CapyIdleGame).GetField("autoAdId",F).GetValue(g);
     double seconds=m.S.activeSeconds;Call(g,"Update");Check(m.S.activeSeconds==seconds&&!g.InputAllowed,"pending ad freezes active gameplay");
     Call(g,"AutoAdReward","wrong-id");Check(m.S.autoAdCount==view-1,"unrelated reward ignored");
     Call(g,"AutoAdReward",id);Call(g,"AutoAdReward",id);
     Check(m.S.autoAdCount==(view==5?0:view)&&m.S.autoRemaining==(view==5?1200:0),"ad reward credited once "+view);
     Call(g,"AutoAdClosed");Check(!g.PresentationPaused,"ad close resumes game "+view);
     var persisted=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));
     Check(persisted.autoAdCount==m.S.autoAdCount&&persisted.autoRemaining==m.S.autoRemaining,"each ad progress is saved "+view);
    }
    g.Show("tap");yield return new WaitForEndOfFrame();Typography(g,"auto-active");Frame("tap-auto-active");
    Check(!Find(g,"Auto watch").interactable,"active auto cannot stack another ad series");
    g.Show("home");yield return new WaitForEndOfFrame();Typography(g,"auto-home");Frame("tap-auto-home");
    Call(g,"PlatformPause",true);double remaining=m.S.autoRemaining;Call(g,"Update");Check(m.S.autoRemaining==remaining,"platform pause preserves auto timer");Call(g,"PlatformPause",false);
    m.S.autoRemaining=0;m.S.autoAdNextUtc=0;Call(g,"PrepareAutoAd");Call(g,"AutoAdClosed");Check(m.S.autoAdCount==0&&!g.PresentationPaused,"early close grants nothing");
    Call(g,"PrepareAutoAd");Call(g,"AutoAdError");Check(m.S.autoAdCount==0&&!g.PresentationPaused,"ad error grants nothing and releases pending state");
    Call(g,"PrepareAutoAd");typeof(CapyIdleGame).GetField("adRequestedAt",F).SetValue(g,Time.realtimeSinceStartupAsDouble-31);Call(g,"CheckAutoAdLoading");
    Check(m.S.autoAdCount==0&&!g.PresentationPaused,"missing ad response times out without reward");
#if PLUGIN_YG_2
    // The official editor simulator verifies the real PluginYG event wiring.
    bool fail=YG.YG2.infoYG.Simulation.testFailAds;float duration=YG.YG2.infoYG.Simulation.durationAdv,delay=YG.YG2.infoYG.Simulation.loadAdv;
    try{
     YG.YG2.infoYG.Simulation.durationAdv=.1f;YG.YG2.infoYG.Simulation.loadAdv=0;
     YG.YG2.infoYG.Simulation.testFailAds=false;g.Show("tap");Call(g,"RequestAutoAd");yield return new WaitForSecondsRealtime(.5f);
     Check(m.S.autoAdCount==1&&!g.PresentationPaused,"official rewarded simulator credits and unpauses");
     m.S.autoAdNextUtc=0;YG.YG2.infoYG.Simulation.testFailAds=true;Call(g,"RequestAutoAd");yield return new WaitForSecondsRealtime(.5f);
     Check(m.S.autoAdCount==1&&!g.PresentationPaused,"official failure simulator does not credit");
    }finally{YG.YG2.infoYG.Simulation.testFailAds=fail;YG.YG2.infoYG.Simulation.durationAdv=duration;YG.YG2.infoYG.Simulation.loadAdv=delay;}
#endif
   }finally{Call(g,"AutoAdClosed");Call(g,"PlatformPause",false);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckCareUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=6,coins=1234,bed=2,sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    Check(g.Tweens!=null&&DOTween.Version=="1.3.045","official DOTween is initialized");
    var pressButton=Find(g,"Food");var press=pressButton.GetComponent<GardenButtonState>();
    var pressRect=(RectTransform)pressButton.transform;var centre=pressRect.TransformPoint(pressRect.rect.center);
    var pressPointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=9};
    press.OnPointerDown(pressPointer);g.Tweens.Step(.05f);
    Check(pressRect.localScale.x<1&&pressRect.localScale.x>=.97f,"DOTween button press animates");
    Check(Vector3.Distance(centre,pressRect.TransformPoint(pressRect.rect.center))<.01f,"button animation preserves centre");
    float frozen=pressRect.localScale.x;typeof(CapyIdleGame).GetField("platformPaused",F).SetValue(g,true);g.Tweens.Step(1);
    Check(pressRect.localScale.x==frozen,"SDK pause freezes owned tweens");typeof(CapyIdleGame).GetField("platformPaused",F).SetValue(g,false);
    press.OnPointerUp(pressPointer);g.Tweens.Step(1);Check(Mathf.Abs(pressRect.localScale.x-1)<.001f,"button returns to full size");
    m.S.calm=true;press.OnPointerDown(pressPointer);g.Tweens.Step(.05f);Check(pressRect.localScale==Vector3.one,"reduced motion skips press scaling");press.OnPointerUp(pressPointer);m.S.calm=false;
    for(int n=0;n<20;n++){g.Show("care");g.Show("food");}g.Show("home");g.Tweens.Step(1);
    Check(g.Tweens.ActiveCount==0,"rapid navigation kills menu tweens without accumulating work");
    m.S.fullness=40;Call(g,"AnimateCare",0f);g.Tweens.Step(.1f);
    var animatedFill=(RectTransform)Find(g,"Care need 0").transform.Find("Need fill");float firstWidth=animatedFill.sizeDelta.x;
    g.Tweens.Step(.1f);Check(animatedFill.sizeDelta.x<firstWidth&&animatedFill.sizeDelta.x>=44.79f,"need fill redraws each tween step without waiting for text refresh");
    m.S.fullness=75;Call(g,"AnimateCare",0f);g.Tweens.Step(1);
    typeof(CapyIdleGame).GetProperty("Model").SetValue(g,null);
    try{
     Call(g,"Suspend",true);Call(g,"Suspend",false);Call(g,"PlatformPause",false);Call(g,"Update");Call(g,"RefreshHome");
     Check(!g.InputAllowed,"editor focus callbacks tolerate model being unavailable during reload");
    }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);}
    for(int n=0;n<3;n++){
     var button=Find(g,"Care need "+n);Check(Clickable(button),"care HUD raycast "+n);
     var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
     Check((corners[1]-corners[0]).magnitude>=44&&(corners[2]-corners[1]).magnitude>=44,"actual care touch size "+n);
    }
    Typography(g,"care-home");Frame("care-home");
    m.S.careTutorial=1;m.S.starter=false;
    string hint=(string)typeof(CapyIdleGame).GetMethod("CareHint",F).Invoke(g,null);
    Check(hint.Contains("Сытость")&&hint.Contains("бесплатно"),"first meal guidance names existing control");
    m.S.starter=true;m.S.careTutorial=3;m.S.comfort=95;m.S.mood=95;
    Call(g,"DoCare",1);
    string feedback=(string)typeof(CapyIdleGame).GetField("toast",F).GetValue(g);
    Check(feedback.Contains("уют +5")&&feedback.Contains("настроение +5")&&!feedback.Contains("+40"),"care feedback reports capped actual gains");
    g.Show("care");yield return new WaitForEndOfFrame();
    var cooling=Find(g,"Care action 1");
    Check(!cooling.interactable&&cooling.transform.Find("Action effect").GetComponent<Text>().text.StartsWith("Ещё"),"action cooldown is explained on button");
    Typography(g,"care-cooldown");Frame("care-cooldown");
    m.Tick(4);Call(g,"DoCare",5);m.Tick(3);g.Show("care");yield return new WaitForEndOfFrame();
    var resting=Find(g,"Care action 5");
    Check(!resting.interactable&&resting.transform.Find("Action effect").GetComponent<Text>().text.StartsWith("Отдых"),"active rest cannot be restarted from care page");
    Check(((string)typeof(CapyIdleGame).GetMethod("CareHint",F).Invoke(g,null)).Contains("разбудить"),"rest guidance explains wake gesture");
    Typography(g,"care-resting");Frame("care-resting");m.Wake();g.Show("home");
    float noticeBefore=(float)typeof(CapyIdleGame).GetField("noticeAge",F).GetValue(g);
    float reactionBefore=(float)typeof(CapyIdleGame).GetField("reactionAge",F).GetValue(g);
    typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,true);Call(g,"Update");
    Check((float)typeof(CapyIdleGame).GetField("noticeAge",F).GetValue(g)==noticeBefore&&(float)typeof(CapyIdleGame).GetField("reactionAge",F).GetValue(g)==reactionBefore,"pause preserves feedback and tap animation clocks");
    typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,false);
    var hold=Find(g,"Care on character").GetComponent<IdleHold>();int taps=m.S.cares;
    var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=7};
    hold.OnPointerDown(pointer);hold.OnPointerUp(pointer);Check(m.S.cares==taps+1,"short portrait tap earns once");
    m.Tick(11);taps=m.S.cares;hold.OnPointerDown(pointer);
    yield return new WaitForSecondsRealtime(.7f);
    hold.OnPointerUp(pointer);Check(m.S.cares==taps&&m.S.lastPet>0,"long pet suppresses release click");
    for(int age=1;age<=5;age++){
     m.S.growth=IdleModel.Growth[age-1];m.Grant("set_gardener");m.Purchase("set_gardener");m.Grant("neon");m.Purchase("neon");
     for(int action=0;action<6;action++){
      m.Tick(12);m.S.fullness=m.S.comfort=m.S.mood=40;
      g.Show("care");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      Typography(g,"care-page");if(age==2&&action==0)Frame("care-page");
      var button=Find(g,"Care action "+action);Check(button.interactable&&Clickable(button),"care action reachable "+age+"/"+action);
      double oldGrowth=m.S.growth;int oldFeeds=m.S.feeds;button.onClick.Invoke();
      Check(g.Page=="home"&&g.CareReaction==action,"care action responds "+age+"/"+action);
      Check(m.S.growth==oldGrowth&&m.S.feeds==oldFeeds,"free care cannot farm growth "+action);
      Call(g,"AnimateCare",.8f);Call(g,"AnimateHero",.1f);
      yield return new WaitForEndOfFrame();Check(g.CareReactionVisible,"action visible "+age+"/"+action);
      Typography(g,"care-result-"+action);
      Frame("care-action-"+age+"-"+action);
      float time=g.CareReactionProgress;typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,true);Call(g,"AnimateCare",1f);
      Check(g.CareReactionProgress==time,"care effect freezes on pause");typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,false);
      m.S.calm=true;Call(g,"AnimateCare",.1f);
      var fx=g.transform.Find("Idle Canvas/Idle board/Home/Capybara/Care action visuals");
      foreach(Transform child in fx)if(child.name.StartsWith("Care particle"))Check(!child.gameObject.activeSelf,"reduced motion hides flying care particles");
      m.S.calm=false;m.Wake();
     }
    }
    m.S.coins=1000000;m.S.growth=150000;
    for(int menu=0;menu<3;menu++){
     typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,menu);g.Show("food");yield return new WaitForEndOfFrame();
     Typography(g,"food-menu-"+menu);Frame("food-menu-"+menu);
     Check(Find(g,"Food previous").interactable==(menu>0)&&Find(g,"Food next").interactable==(menu<2),"food page navigation bounds "+menu);
     for(int row=0;row<4;row++){
      int food=menu*4+row;m.Tick(9);g.Show("food");yield return new WaitForEndOfFrame();
      var button=Find(g,"Food "+food);Check(Clickable(button)&&button.interactable,"food reachable "+food);
      Check(button.transform.Find("Detailed food/Food illustration "+food)!=null,"distinct food illustration "+food);
      double before=m.S.growth;button.onClick.Invoke();Check(g.Page=="home"&&m.S.growth>before,"new food grows pet "+food);
      var portrait=g.transform.Find("Idle Canvas/Idle board/Home/Capybara/Meal reaction/Food illustration "+food);
      Check(portrait!=null,"feeding reaction uses chosen dish "+food);
      Call(g,"AnimateHero",.5f);yield return new WaitForEndOfFrame();Frame("food-feeding-"+food);
     }
    }
    m.Tick(9);m.S.premiumFood=1;g.Show("food");yield return new WaitForEndOfFrame();
    Find(g,"Special meal").onClick.Invoke();Check(m.S.premiumFood==0,"premium UI consumes holiday bowl");
    typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,0);
    m.S.fullness=12;m.S.comfort=23;m.S.mood=34;g.Save();
    var restored=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));
    Check(restored.S.fullness==12&&restored.S.comfort==23&&restored.S.mood==34,"care needs survive JSON save");
    Check(restored.S.equipped[0]==m.S.equipped[0],"care JSON keeps equipped outfit: "+restored.S.equipped[0]+" / "+m.S.equipped[0]);
    // JsonUtility decimal encoding can round the last binary double bit.
    Check(Math.Abs(restored.S.coins-m.S.coins)<=Math.Max(1e-8,Math.Abs(m.S.coins)*1e-14),"care JSON preserves coins within floating-point serialization precision");
    double utc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();m.S.lastUtc=utc-172800;double award=m.Arrive(utc);
    typeof(CapyIdleGame).GetField("offlineAward",F).SetValue(g,award);g.Show("offline");
    yield return new WaitForEndOfFrame();Typography(g,"care-offline");Frame("care-offline");Check(m.ReturnGift&&Math.Abs(m.S.fullness-50)<.05,"welcome gift and automatic recovery");
    Find(g,"Continue idle").onClick.Invoke();Check(g.Page=="home"&&!m.S.resting,"welcome returns to awake pet");
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckSurpriseUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=360,coins=20000,stars=100,sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);g.Show("home");yield return new WaitForEndOfFrame();
    Check(Find(g,"Album")==null&&Clickable(Find(g,"Surprises")),"tree replaces old album navigation");
    var viewport=g.transform.Find("Idle Canvas/Idle board/Home/Decor/Room scenery/Scenery viewport") as RectTransform;
    Check(viewport!=null&&viewport.rect.height==850&&viewport.Find("Sky continuation")&&viewport.Find("Ground continuation"),"full scene continues behind both HUD panels");Frame("full-height-home");
    Find(g,"Surprises").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(g.Page=="surprises"&&Clickable(Find(g,"Open surprise ten")),"tree and batch accessible");
    Typography(g,"surprises");Frame("surprise-tree");
    Find(g,"Surprise free gifts").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(Clickable(Find(g,"Daily tree ticket")),"daily ticket raycast");Find(g,"Daily tree ticket").onClick.Invoke();
    Check(m.S.surpriseTickets==1,"UI daily ticket credited");g.Show("surprises");yield return new WaitForEndOfFrame();
    Check(Clickable(Find(g,"Open surprise ticket")),"ticket open raycast");Find(g,"Open surprise ticket").onClick.Invoke();
    Check(g.Page=="surprise-opening"&&m.S.surpriseTickets==0&&m.S.surpriseResults.Count==1,"result credited before reveal");
    var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));Check(saved.surpriseResults.Count==1&&saved.surpriseTickets==0,"outcome immediately saved");
    yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Tweens.Step(5);yield return new WaitForEndOfFrame();Typography(g,"surprise-results");Frame("surprise-result");
    int total=m.S.surpriseTotal;g.Show("home");g.Show("surprise-results");Check(m.S.surpriseTotal==total,"navigation never rerolls result");
    foreach(string page in new[]{"surprise-gifts","surprise-odds","surprise-craft","surprise-results","settings","quests"}){g.Show(page);yield return new WaitForEndOfFrame();Typography(g,page);Frame(page);Check(Clickable(Find(g,"Back")),"tree navigation "+page);}
    for(int tier=0;tier<4;tier++){typeof(CapyIdleGame).GetField("surpriseTier",F).SetValue(g,tier);typeof(CapyIdleGame).GetField("surpriseShelf",F).SetValue(g,0);g.Show("surprise-pool");yield return new WaitForEndOfFrame();Typography(g,"pool-"+tier);Frame("pool-"+tier);}
    g.Show("surprise-craft");yield return new WaitForEndOfFrame();var preview=Find(g,"Tree preview set_tree_clover");Check(Clickable(preview),"exclusive collection preview raycast");preview.onClick.Invoke();yield return new WaitForEndOfFrame();Check(g.Page=="shop"&&!Find(g,"Equip or buy").interactable,"preview cannot bypass exclusive ownership");
    for(int rarity=0;rarity<4;rarity++){
     var item=m.SurprisePool(rarity)[0];m.S.surpriseResults=new List<GardenReward>{new GardenReward{tier=rarity,item=item.id,name=item.name,stars=1}};
     m.S.calm=false;g.Show("surprise-opening");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Call(g,"PlatformPause",false);
     int receipt=m.S.surpriseTotal;g.Tweens.Step(.3f);Call(g,"PlatformPause",true);g.Tweens.Step(5);Check(g.Page=="surprise-opening","opening pauses with platform "+rarity);Call(g,"PlatformPause",false);
     g.Tweens.Step(.9f);yield return new WaitForEndOfFrame();Typography(g,"opening-"+rarity);Frame("opening-"+rarity);
     Check(Clickable(Find(g,"Skip opening")),"opening can be skipped "+rarity);Find(g,"Skip opening").onClick.Invoke();g.Tweens.Step(5);Check(g.Page=="surprise-results"&&m.S.surpriseTotal==receipt,"skip does not charge or reroll "+rarity);
     g.Show("surprise-opening");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="surprise-results","opening completes automatically "+rarity);
     g.Show("surprise-opening");g.Show("home");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="home","closing opening kills callback "+rarity);
    }
    m.S.surpriseResults=new List<GardenReward>{new GardenReward{tier=0,name="Монеты для заботы",coins=50,stars=1}};
    g.Show("surprise-results");yield return new WaitForEndOfFrame();Typography(g,"gift-detail");Frame("surprise-resource-gift");Check(Find(g,"Cycle talent")==null,"resource gift has no talent control");
    m.S.calm=true;g.Show("surprise-opening");Check(g.Page=="surprise-results","reduced motion skips opening");m.S.calm=false;
    m.S.forms.Add("chill");m.S.form="chill";g.Show("surprise-results");yield return new WaitForEndOfFrame();Check(Find(g,"Cycle talent")==null&&m.S.form=="chill","talent button removed without changing owned talent");
    m.S.surpriseRun=0;m.S.surprisePity=9;g.Show("surprises");yield return new WaitForEndOfFrame();Find(g,"Open surprise one").onClick.Invoke();Check(m.S.surpriseResults[0].tier>=2,"UI guarantee uses authoritative model");
    yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Tweens.Step(5);yield return new WaitForEndOfFrame();Typography(g,"surprise-guarantee");Frame("surprise-guarantee");
    var roundTrip=new IdleModel(JsonUtility.FromJson<IdleSave>(JsonUtility.ToJson(m.S)));Check(roundTrip.S.surprisePity==m.S.surprisePity&&roundTrip.S.surpriseSpent==m.S.surpriseSpent&&roundTrip.S.surpriseResults.Count==m.S.surpriseResults.Count,"surprise save reload preserves budget pity and receipt");
#if PLUGIN_YG_2
    bool fail=YG.YG2.infoYG.Simulation.testFailAds;float duration=YG.YG2.infoYG.Simulation.durationAdv,delay=YG.YG2.infoYG.Simulation.loadAdv;
    try{YG.YG2.infoYG.Simulation.testFailAds=false;YG.YG2.infoYG.Simulation.durationAdv=.1f;YG.YG2.infoYG.Simulation.loadAdv=0;
     g.Show("surprise-gifts");m.S.autoAdNextUtc=0;int tickets=m.S.surpriseTickets;Call(g,"RequestSurpriseAd");yield return new WaitForSecondsRealtime(.7f);
     Check(m.S.surpriseTickets==tickets+1&&m.S.autoAdCount==0&&!g.PresentationPaused,"tree ad isolated from auto series and unpauses");
    }finally{YG.YG2.infoYG.Simulation.testFailAds=fail;YG.YG2.infoYG.Simulation.durationAdv=duration;YG.YG2.infoYG.Simulation.loadAdv=delay;}
#endif
   }finally{Call(g,"AutoAdClosed");Call(g,"PlatformPause",false);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator Run(CapyIdleGame g){
   running=true;checks=0;renderErrors=0;Application.logMessageReceived+=TrackRenderError;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-idle-checks.txt","RUNNING");File.WriteAllText("Logs/Capy67-layout.txt","");
   bool careOnly=File.Exists("Temp/Capy67-care-only");if(careOnly)File.Delete("Temp/Capy67-care-only");
   capture=File.Exists("Temp/Capy67-garden-capture");if(capture){File.Delete("Temp/Capy67-garden-capture");Directory.CreateDirectory("Logs/GardenReview");}
   string[] keys={CapyIdleGame.SaveKey,CapyIdleGame.SaveKey+".backup","capy67.save.v1","capy67.progress.v2"};var saves=new string[keys.Length];for(int n=0;n<keys.Length;n++)saves[n]=PlayerPrefs.GetString(keys[n],"");
   SessionState.SetBool("CapyIdle.validation",true);EditorApplication.LockReloadAssemblies();
   var original=g.Model;var model=new IdleModel();typeof(CapyIdleGame).GetProperty("Model").SetValue(g,model);model.S.sound=false;
   try{
    if(!careOnly){
    CheckFiveStageProgression();
    checks+=CapyCareChecks.Run();
    Check(CapyCompleteSets.All.Length==42,"42 complete outfit definitions");
    var daily=new IdleModel();daily.S.feeds=1;daily.S.coins=10000;daily.S.growth=150000;
    Check(!daily.Purchase("firefly_garden")&&!daily.Purchase("set_dawn"),"calendar exclusives cannot be bought early");
    for(int day=1;day<=7;day++){
     string date="2030-01-"+day.ToString("00");Check(daily.ClaimDaily(date),"calendar claim "+day);
     string state=JsonUtility.ToJson(daily.S);Check(!daily.ClaimDaily(date)&&state==JsonUtility.ToJson(daily.S),"duplicate gift has no side effects "+day);
     Check(daily.S.owned.Contains("firefly_garden")== (day>=3),"scene unlock threshold "+day);
     Check(daily.S.owned.Contains("set_dawn")== (day>=7),"outfit unlock threshold "+day);
    }
    Check(daily.S.stars==3&&daily.S.premiumFood==1&&daily.Purchase("set_dawn")&&daily.Purchase("firefly_garden"),"seventh gift keeps old bonuses and equips new rewards");
    Check(!daily.ClaimDaily("2029-12-31")&&!daily.ClaimDaily("bad"),"calendar rejects clock rollback and invalid dates");
    var returning=new IdleModel(new IdleSave{dailyCount=7,coins=5000,growth=6});string retro=JsonUtility.ToJson(returning.S);returning.Repair();
    Check(returning.S.owned.Contains("set_dawn")&&returning.S.owned.Contains("firefly_garden")&&JsonUtility.ToJson(returning.S)==retro&&returning.S.coins==5000&&returning.Stage==2,"retroactive milestones preserve profile and are idempotent");
    var migration=new IdleModel(new IdleSave{coins=5000,growth=6,owned=new List<string>{"cap","scarf"},equipped=new[]{"cap","scarf","moon","","","classic"}});
    string migrated=JsonUtility.ToJson(migration.S);migration.Repair();
    Check(JsonUtility.ToJson(migration.S)==migrated,"wardrobe migration is idempotent");
    Check(migration.S.coins==5000&&migration.Stage==2,"migration preserves balance and age");
    foreach(var set in CapyCompleteSets.All)Check(CapyOutfitCatalog.Find(set.id,"","classic",-1)!=null,"set has production art "+set.id);
    foreach(var legacy in IdleCatalog.Items)if(legacy.slot<2&&!legacy.IsSet) {
      var old=new IdleModel(new IdleSave{owned=new List<string>{legacy.id}});
       bool mapped=false;
       foreach(var set in CapyCompleteSets.All)if(Array.IndexOf(set.legacy,legacy.id)>=0){mapped=true;Check(old.S.owned.Contains(set.id),"legacy purchase grants matching set "+legacy.id);}
       Check(mapped,"legacy purchase has a set mapping "+legacy.id);
      Check(old.S.owned.Contains(legacy.id),"legacy receipt preserved "+legacy.id);
    }
    var transaction=new IdleModel();transaction.S.growth=150000;transaction.S.coins=10000;
    double funds=transaction.S.coins;Check(transaction.Purchase("set_baker")&&transaction.S.coins==funds-IdleCatalog.Find("set_baker").price,"whole set charged once");
    funds=transaction.S.coins;Check(transaction.Purchase("set_baker")&&transaction.S.coins==funds&&transaction.S.equipped[1]=="","re-equipping is free and removes old clothing");
    Check(CapyOutfitCatalog.Find("set_baker","","neon",3).head=="set_baker","skin and meme bonuses cannot break complete outfit rendering");
    transaction.Grant("neon");Check(transaction.Purchase("neon")&&transaction.S.equipped[0]=="set_baker"&&transaction.S.equipped[5]=="neon","skin preserves complete outfit");
    Check(transaction.Purchase("set_gardener")&&transaction.S.equipped[5]=="neon","outfit preserves selected skin");
    var savedCombination=new IdleModel(JsonUtility.FromJson<IdleSave>(JsonUtility.ToJson(transaction.S)));
    Check(savedCombination.S.equipped[0]=="set_gardener"&&savedCombination.S.equipped[5]=="neon","save repair preserves outfit plus skin");
    g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Care hold")),"home care raycast");Check(Clickable(Find(g,"Care on character")),"character raycast");g.Care();Check(model.S.cares==1,"care earns on home");double coins=model.S.coins;g.Care();Check(model.S.coins==coins,"spam input limit");
    Check(Clickable(Find(g,"Growth stages")),"growth map raycast");double growthBefore=model.S.growth;int ownedBefore=model.S.owned.Count;Find(g,"Growth stages").onClick.Invoke();Check(g.Page=="growth","growth map opens");Check(g.transform.Find("Idle Canvas/Idle board/Pages/Age preview 5")!=null,"all ages previewed through guardian");Check(g.transform.Find("Idle Canvas/Idle board/Pages/Age preview 6")==null,"removed ages absent from growth page");Check(model.S.growth==growthBefore&&model.S.owned.Count==ownedBefore,"age preview does not unlock progress");g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    var avatar=(RectTransform)typeof(CapyIdleGame).GetField("hero",F).GetValue(g);Check(avatar.anchoredPosition.y<0&&avatar.anchoredPosition.y>-400,"avatar inside portrait area");
    foreach(double growth in IdleModel.Growth){model.S.growth=growth;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);var painted=avatar.Find("Illustrated pose/Life sprite").GetComponent<Image>();Check(painted.sprite!=null&&!painted.canvasRenderer.cull,"visible reference sprite at stage "+model.Stage);Check(avatar.Find("Head")==null&&avatar.Find("Body")==null,"legacy geometry removed at stage "+model.Stage);Check(painted.sprite.name=="Capy life stage "+new[]{1,2,3,6,9}[model.Stage-1],"correct surviving portrait at stage "+model.Stage);Frame("stage-"+model.Stage);}model.S.growth=0;
    g.Show("growth");yield return new WaitForEndOfFrame();
    for(int age=1;age<=5;age++){
     var preview=g.transform.Find("Idle Canvas/Idle board/Pages/Age preview "+age);
     Check(preview.childCount==2&&preview.Find("Illustrated pose").childCount==1,"growth preview has only shadow and bare portrait "+age);
    }
    var previousScene=model.S.equipped[2];
    foreach(var scenery in IdleCatalog.Items)if(scenery.slot==2){
     model.S.equipped[2]=scenery.id;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     // Locate by component instead of depending on the decoration container's presentation name.
     var gradients=g.GetComponentsInChildren<CapySceneryGradient>();Check(gradients.Length==1,"one detailed scene "+scenery.id);
     var sceneryViewport=gradients[0].transform.parent.parent;
     Check(sceneryViewport.GetComponent<RectMask2D>()!=null&&((RectTransform)sceneryViewport).rect.height==850,"full height scene clipping "+scenery.id);
     foreach(var graphic in sceneryViewport.GetComponentsInChildren<Graphic>())Check(!graphic.raycastTarget,"scenery never blocks care "+scenery.id);
     Frame("scene-"+scenery.id);
    }
    model.S.equipped[2]=previousScene;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    var oldEquipment=(string[])model.S.equipped.Clone();
    // Exercise the actual illustrated eyes, pause/reduced-motion behaviour and outfit anchor.
    for(int age=1;age<=IdleModel.StageCount;age++){
     model.S.growth=IdleModel.Growth[age-1];g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     var pose=(RectTransform)avatar.Find("Illustrated pose");var painted=pose.Find("Life sprite").GetComponent<Image>();var motion=painted.GetComponent<CapyPortraitMotion>();
     Check(motion!=null,"illustrated eye animation exists at age "+age);
     typeof(CapyIdleGame).GetField("lifeAge",F).SetValue(g,2.82f);Call(g,"AnimateHero",0f);Canvas.ForceUpdateCanvases();
     Check(motion.Closure>.99f,"eyes close at blink peak "+age);
     Check(painted.material.shader.name=="Capybara67/UI Eyes Only","eye-only shader assigned "+age);
     yield return CheckEyePixels(g,painted,"base-"+age);
     var foot=avatar.InverseTransformPoint(pose.TransformPoint(new Vector3(120,-244,0)));
     Check(Vector2.Distance(foot,new Vector2(120,-244))<.01f,"breathing keeps feet planted "+age);
     Check(pose.localScale.x>0,"all five surviving poses face right "+age);
     typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,true);float frozen=motion.Closure;Call(g,"AnimateHero",.5f);Check(motion.Closure==frozen,"pause freezes blink "+age);
     yield return new WaitForEndOfFrame();Frame("blink-closed-"+age);
     typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,false);model.S.calm=true;Call(g,"AnimateHero",0f);
     Check(motion.Closure==0&&Mathf.Abs(pose.localScale.y-1)<.0001f&&Quaternion.Angle(pose.localRotation,Quaternion.identity)<.001f,"reduced motion resets pose and blink "+age);
     typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,true);yield return new WaitForEndOfFrame();Frame("blink-open-"+age);
     typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,false);model.S.calm=false;
    }
    bool starterBeforeSleep=model.S.starter;double activeBeforeSleep=model.S.activeSeconds,careBeforeSleep=model.S.lastCare,feedBeforeSleep=model.S.lastFeed;
    model.S.growth=IdleModel.Growth[1];model.S.starter=true;model.S.activeSeconds=100;model.S.lastCare=100;model.S.lastFeed=100;
    g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    var sleepPortrait=avatar.Find("Illustrated pose/Life sprite").GetComponent<Image>();var sleepMotion=sleepPortrait.GetComponent<CapyPortraitMotion>();
    typeof(CapyIdleGame).GetField("lifeAge",F).SetValue(g,1f);typeof(CapyIdleGame).GetField("sleepClosure",F).SetValue(g,0f);
    model.S.activeSeconds=191;Call(g,"AnimateHero",.1f);
    Check(g.IsSleeping&&sleepMotion.Closure>0&&sleepMotion.Closure<1,"sleep closes eyes gradually");
    Call(g,"AnimateHero",.3f);Check(sleepMotion.Closure>.99f&&avatar.Find("Sleep mark").gameObject.activeSelf,"sleep holds closed eyes and shows sleep mark");
    yield return CheckEyePixels(g,sleepPortrait,"base-sleep-2");
    model.S.lastCare=model.S.activeSeconds;typeof(CapyIdleGame).GetField("lifeAge",F).SetValue(g,1f);Call(g,"AnimateHero",.1f);
    Check(!g.IsSleeping&&sleepMotion.Closure>0&&sleepMotion.Closure<1,"waking opens eyes gradually");
    Call(g,"AnimateHero",.3f);Check(sleepMotion.Closure==0&&!avatar.Find("Sleep mark").gameObject.activeSelf,"awake eyes reopen without sleep marker");
    model.S.starter=starterBeforeSleep;model.S.activeSeconds=activeBeforeSleep;model.S.lastCare=careBeforeSleep;model.S.lastFeed=feedBeforeSleep;
    model.S.growth=0;
    for(int age=1;age<=IdleModel.StageCount;age++){
     model.S.growth=IdleModel.Growth[age-1];
     foreach(var fitting in IdleCatalog.Items){if(!fitting.IsSet)continue;model.S.equipped[0]="";model.S.equipped[1]="";model.S.equipped[fitting.slot]=fitting.id;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      var pose=avatar.Find("Illustrated pose");var wearable=pose.Find("Painted accessory "+fitting.id);var baked=pose.GetComponentInChildren<CapyBakedOutfit>();
      if(baked){
       var portrait=baked.GetComponent<Image>();Check((fitting.slot==0?baked.headId:baked.outfitId)==fitting.id&&portrait.sprite!=null&&!portrait.canvasRenderer.cull,"complete outfit rendered "+fitting.id+" at age "+age);
       Check(wearable==null&&pose.GetComponentsInChildren<Image>().Length==1,"complete outfit has no separate accessory layers "+age);
      }else{
       Check(wearable!=null&&wearable.GetComponent<Image>().sprite!=null&&!wearable.GetComponent<Image>().canvasRenderer.cull,"painted fitting "+fitting.id+" at age "+age);
       if(fitting.slot==1)Check(wearable.GetSiblingIndex()>pose.Find("Life sprite").GetSiblingIndex(),"clothing visible over body "+fitting.id+" at age "+age);
      }
      if(fitting.id=="cap"||fitting.id=="crown"||fitting.id=="beret"||fitting.id=="scarf"||fitting.id=="glasses"||fitting.id=="wizardhat"||fitting.id=="picnicpack"||fitting.id=="leafcape")Frame("fit-"+age+"-"+fitting.id);
     }
    }
    // Real combined portraits, including matching both slots, every age and animation.
    var outfitKeys=new HashSet<string>();
    foreach(var outfit in CapyOutfitCatalog.Entries){
     if(!CapyCompleteSets.IsSet(outfit.head))continue;
     Check(outfitKeys.Add(outfit.Key)&&outfit.stages.Length==IdleModel.StageCount,"unique complete five-age outfit "+outfit.Key);
     model.S.equipped=new[]{outfit.head,outfit.clothes,"moon","","",outfit.skin};
     for(int age=1;age<=IdleModel.StageCount;age++){
      model.S.growth=IdleModel.Growth[age-1];g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      var pose=(RectTransform)avatar.Find("Illustrated pose");var baked=pose.GetComponentInChildren<CapyBakedOutfit>();
      Check(baked&&baked.key==outfit.Key&&baked.headId==outfit.head&&baked.outfitId==outfit.clothes,"both equipped slots select exact complete outfit "+outfit.Key+" age "+age);
      var portrait=baked.GetComponent<Image>();Rect crop=outfit.stages[age-1].crop;
      Check(portrait.sprite&&portrait.sprite.texture&&!portrait.canvasRenderer.cull&&pose.GetComponentsInChildren<Image>().Length==1,"single visible combined illustration "+age);
      Check(crop.x>=0&&crop.y>=0&&crop.xMax<=outfit.sourceWidth&&crop.yMax<=outfit.sourceHeight,"outfit sprite inside atlas "+age);
      foreach(var eye in outfit.stages[age-1].eyes)Check(crop.Contains(new Vector2(eye.x,eye.y)),"blink landmark inside combined portrait "+age);
      typeof(CapyIdleGame).GetField("lifeAge",F).SetValue(g,2.82f);Call(g,"AnimateHero",0f);Canvas.ForceUpdateCanvases();
      Check(portrait.GetComponent<CapyPortraitMotion>().Closure>.99f&&portrait.canvasRenderer.GetMesh().vertexCount==4,"combined outfit blink keeps geometry unchanged "+age);
       yield return CheckEyePixels(g,portrait,outfit.head+"-"+age);
      var colourMotion=portrait.GetComponent<CapyPortraitMotion>();
      Check(portrait.material.GetFloat("_UseFurMask")==1&&portrait.material.GetTexture("_FurMask"),"outfit has dedicated fur selection "+outfit.head+" age "+age);
      for(int skin=1;skin<=9;skin++){colourMotion.SetOutfitSkin(skin,(Texture2D)portrait.material.GetTexture("_FurMask"));Check(colourMotion.SkinArt==skin&&portrait.materialForRendering.GetFloat("_Skin")==skin,"skin combines with outfit "+outfit.head+" age "+age+" colour "+skin);}
      colourMotion.SetOutfitSkin(3,(Texture2D)portrait.material.GetTexture("_FurMask"));
      yield return new WaitForEndOfFrame();Frame("outfit-skin-"+outfit.head+"-"+age);
      if(age==2)yield return CheckEyePixels(g,portrait,"outfit-neon-"+outfit.head);
      colourMotion.SetOutfitSkin(0,(Texture2D)portrait.material.GetTexture("_FurMask"));
      var foot=avatar.InverseTransformPoint(pose.TransformPoint(new Vector3(120,-244,0)));
      Check(Vector2.Distance(foot,new Vector2(120,-244))<.01f&&pose.localScale.x>0,"combined outfit breath anchor and facing "+age);
      if(age==1||age==3||age==5)Frame("combo-blink-"+outfit.head+"-"+outfit.clothes+"-"+age);
     }
     if(capture){model.S.equipped[5]="neon";Call(g,"ShowOutfitReview",outfit.head,outfit.clothes);yield return new WaitForEndOfFrame();Frame("outfit-colours-"+outfit.head);Call(g,"CloseAccessoryReview");model.S.equipped[5]="classic";}
    }
    IdleItem missingHat=null,missingClothes=null;
    foreach(var hat in IdleCatalog.Items){if(hat.slot!=0)continue;foreach(var clothes in IdleCatalog.Items){if(clothes.slot!=1||CapyOutfitCatalog.Find(hat.id,clothes.id,"classic",-1)!=null)continue;missingHat=hat;missingClothes=clothes;break;}if(missingHat!=null)break;}
    if(missingHat!=null){
     model.S.equipped=new[]{missingHat.id,missingClothes.id,"moon","","","classic"};g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     Check(avatar.GetComponentInChildren<CapyBakedOutfit>()==null&&avatar.Find("Illustrated pose/Painted accessory "+missingHat.id)&&avatar.Find("Illustrated pose/Painted accessory "+missingClothes.id),"unfinished pair retains both chosen items without substituting another outfit");
    }
    model.S.equipped=new[]{"cap","scarf","moon","stars","","classic"};g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    Check(avatar.GetComponentInChildren<CapyBakedOutfit>()&&avatar.childCount>2,"effects preserve combined outfit after atlas unload and reload");
    model.S.equipped=new[]{"set_gardener","","moon","","","classic"};
    var equippedBeforePreview=string.Join("|",model.S.equipped);model.S.growth=0;
    typeof(CapyIdleGame).GetField("category",F).SetValue(g,0);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,"set_baker");g.Show("shop");
    yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    bool combinedPreview=false;foreach(var baked in g.GetComponentsInChildren<CapyBakedOutfit>())if(baked.headId=="set_baker")combinedPreview=true;
    Check(combinedPreview&&string.Join("|",model.S.equipped)==equippedBeforePreview,"whole-set preview leaves equipped save unchanged");
    Find(g,"Category 0").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(Find(g,"Category 1")==null,"separate clothing category removed");
    for(int page=0;page<8;page++){Find(g,"Next").onClick.Invoke();yield return new WaitForEndOfFrame();Call(g,"Suspend",false);}
    var last=Find(g,"Shop item set_sun");Check(Clickable(last),"35th set reachable through pagination");last.onClick.Invoke();
    yield return new WaitForEndOfFrame();Frame("sets-last-page");
    Check(string.Join("|",model.S.equipped)==equippedBeforePreview,"pagination and preview never equip or purchase");
    model.S.equipped=oldEquipment;model.S.growth=0;
    // Inspect every cosmetic on a bare baby and verify animation policy without touching persistence.
    for(int age=1;age<=5;age++){
     model.S.growth=IdleModel.Growth[age-1];var hashes=new HashSet<long>();
     foreach(var skin in IdleCatalog.Items)if(skin.slot==5){
      model.S.equipped=new[]{"","","moon","","",skin.id};g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      var sprite=avatar.Find("Illustrated pose/Life sprite").GetComponent<Image>();Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
      Check(sprite.materialForRendering.GetFloat("_Skin")==(skin.art==0?10:skin.art),"render material has skin design "+skin.id+" age "+age);
      var shot=ScreenCapture.CaptureScreenshotAsTexture();long hash=17;var bounds=new Vector3[4];sprite.rectTransform.GetWorldCorners(bounds);
      int x0=Mathf.Clamp(Mathf.FloorToInt(bounds[0].x),0,shot.width-1),x1=Mathf.Clamp(Mathf.CeilToInt(bounds[2].x),0,shot.width);
      int y0=Mathf.Clamp(Mathf.FloorToInt(bounds[0].y),0,shot.height-1),y1=Mathf.Clamp(Mathf.CeilToInt(bounds[2].y),0,shot.height);
      for(int yy=y0;yy<y1;yy+=2)for(int xx=x0;xx<x1;xx+=2){Color32 c=shot.GetPixel(xx,yy);unchecked{hash=hash*31+c.r*65536+c.g*256+c.b;}}
      UnityEngine.Object.Destroy(shot);Check(hashes.Add(hash),"skin changes rendered portrait "+skin.id+" age "+age);Frame("skin-"+age+"-"+skin.id);
      if(age==2)yield return CheckEyePixels(g,sprite,"skin-"+skin.id);
     }
    }
    model.S.growth=6;
    foreach(var item in IdleCatalog.Items)if(item.slot>=3){
     model.S.equipped=new[]{"","","moon","","","classic"};model.S.equipped[item.slot]=item.id;
     g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     if(item.slot==5){var skin=avatar.Find("Illustrated pose/Life sprite").GetComponent<CapyPortraitMotion>();Check(skin.SkinArt==item.art,"patterned skin selected "+item.id);Check(!ShaderUtil.ShaderHasError(skin.GetComponent<Image>().material.shader),"skin shader compiles "+item.id);}
     else{
      var motions=avatar.GetComponentsInChildren<CapyCosmeticMotion>();Check(motions.Length>0,"cosmetic has animation "+item.id);var m=motions[0];float age=m.Age;
      yield return null;Check(m.Age>age,"cosmetic animation advances "+item.id);
      Call(g,"PlatformPause",true);age=m.Age;yield return null;Check(m.Age==age,"cosmetic animation respects platform pause "+item.id);Call(g,"PlatformPause",false);
      model.S.calm=true;age=m.Age;yield return null;Check(m.Age==age&&Quaternion.Angle(m.transform.localRotation,Quaternion.identity)<.001f,"reduced motion returns cosmetic to neutral "+item.id);model.S.calm=false;
      foreach(var graphic in avatar.GetComponentsInChildren<Graphic>())Check(!graphic.raycastTarget,"cosmetic does not block care "+item.id);
     }
     yield return new WaitForEndOfFrame();Frame("cosmetic-"+item.id);
    }
    model.S.equipped=oldEquipment;model.S.growth=0;
    g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Starter bowl")),"starter raycast");g.Care();Check(model.S.coins==coins,"modal blocks care");Find(g,"Starter bowl").onClick.Invoke();Check(model.Stage==2&&g.Page=="home","starter growth transition");
    model.S.coins=100;g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Bed upgrade").onClick.Invoke();Check(model.S.bed==1&&model.Passive>0,"buy bed UI");
    model.S.growth=360;model.S.coins=10000;model.S.pity=66;model.Tick(60);g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Food 2").onClick.Invoke();Check(model.S.forms.Count==1&&g.Page=="home","feed evolves and returns home");
    g.Show("surprise-results");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Find(g,"Cycle talent")==null,"no talent button on results");Check(model.SelectForm(""),"existing talent remains reversible in model");
    foreach(string page in new[]{"home","growth","food","shop","album","quests","daily","settings","offline","visitor"}){g.Show(page);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(page=="home"||Clickable(Find(g,"Back")),"navigation "+page);foreach(var button in g.GetComponentsInChildren<Button>()){var r=(RectTransform)button.transform;Check(r.rect.width>=60&&r.rect.height>=60,"touch area "+button.name);}Typography(g,page);Frame(page);}
    model.S.feeds=3;g.Show("visitor");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Welcome visitor")),"visitor gift raycast");Find(g,"Welcome visitor").onClick.Invoke();Check(!model.CanVisit,"visitor gift consumed");
    g.Show("home");model.Tick(91);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(g.IsSleeping,"safe idle sleep");g.Care();Check(!g.IsSleeping,"care wakes capybara");
    model.S.coins=1e8;model.S.growth=150000;model.S.stars=500;
    model.Grant("firefly_garden");model.Grant("set_dawn");foreach(var exclusive in IdleCatalog.Items)if(exclusive.TreeOnly)model.Grant(exclusive.id);
    foreach(var item in IdleCatalog.Items){if(!CapyCompleteSets.Visible(item))continue;typeof(CapyIdleGame).GetField("category",F).SetValue(g,item.slot);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,item.id);g.Show("shop");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Typography(g,"item-"+item.id);Frame("item-"+item.id);var buy=Find(g,"Equip or buy");Check(Clickable(buy),"item button "+item.id);buy.onClick.Invoke();Check(model.S.equipped[item.slot]==item.id,"equip UI "+item.id);}
    g.Show("daily");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Claim daily").onClick.Invoke();int count=model.S.dailyCount;Find(g,"Claim daily").onClick.Invoke();Check(model.S.dailyCount==count&&count==1,"daily UI once");
    var ownedForms=new List<string>(model.S.forms);foreach(var f in IdleModel.Forms)if(!model.S.forms.Contains(f.id))model.S.forms.Add(f.id);
    g.Show("album");yield return new WaitForEndOfFrame();Typography(g,"album-all");Frame("album-all");model.S.forms=ownedForms;
    g.Save();var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));Check(saved.owned.Count==103&&saved.forms.Count==1,"saved idle profile");Check(PlayerPrefs.GetString(keys[2],"")==saves[2]&&PlayerPrefs.GetString(keys[3],"")==saves[3],"legacy profile untouched");
    Call(g,"OnDisable");typeof(CapyIdleGame).GetProperty("Model").SetValue(g,null);Call(g,"OnEnable");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);model=g.Model;Check(model!=null&&model.S.owned.Count==103,"managed model restored after editor reload");g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Care hold")),"rebuilt UI accepts input after reload");Check(g.GetComponents<AudioSource>().Length==1,"editor reload reuses audio source");
    g.Show("home");Call(g,"PlatformPause",true);coins=model.S.coins;g.Care();Check(model.S.coins==coins,"platform pause blocks input");
    model.S.lastUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-100;Call(g,"Suspend",true);Call(g,"Suspend",false);Check(model.S.coins==coins,"platform pause excludes offline credit");Call(g,"PlatformPause",false);
    }
    yield return CheckWelcomeUI(g);
    yield return CheckCareGestures(g);
    yield return CheckCareUI(g);
    yield return CheckTapUI(g);
    yield return CheckSurpriseUI(g);
    Check(PlayerPrefs.GetString(keys[0],"")==saves[0]&&PlayerPrefs.GetString(keys[1],"")==saves[1],"player save and backup remain unchanged throughout validation");
    Check(new FileInfo("Logs/Capy67-layout.txt").Length==0,"all visible labels fit their text rectangles");
    Check(renderErrors==0,"no runtime or rendering errors during fitting and purchases");
    File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+(careOnly?" Unity care / DOTween assertions; care UI, food, pause, reduced motion, save and lifecycle.":" Unity idle integration assertions; all pages, raycasts, touch sizes, growth, feeding, evolution, 103 catalog items including 42 complete sets, daily, save and platform pause.")+" Profile restored.\n");Debug.Log("CAPY IDLE PASS: "+checks);
   }finally{
    Application.logMessageReceived-=TrackRenderError;
    try{
     typeof(CapyIdleGame).GetProperty("Model").SetValue(g,original);
     Call(g,"CloseAccessoryReview");g.Show("home");
    }finally{
     try{for(int n=0;n<keys.Length;n++){if(saves[n]=="")PlayerPrefs.DeleteKey(keys[n]);else PlayerPrefs.SetString(keys[n],saves[n]);}PlayerPrefs.Save();}
     finally{
      running=false;SessionState.SetBool("CapyIdle.validation",false);EditorApplication.UnlockReloadAssemblies();
      if(SessionState.GetBool("CapyIdle.auto",false)){SessionState.SetBool("CapyIdle.auto",false);Application.runInBackground=SessionState.GetBool("CapyIdle.background",false);EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();}
     }
    }
   }
  }
 }
}













