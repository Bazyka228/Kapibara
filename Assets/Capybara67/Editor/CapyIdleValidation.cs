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
  static void TrackRenderError(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert||type==LogType.Warning&&message.IndexOf("DOTWEEN",StringComparison.OrdinalIgnoreCase)>=0&&message.IndexOf("target",StringComparison.OrdinalIgnoreCase)>=0)renderErrors++;}
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
    Check(!System.Text.RegularExpressions.Regex.IsMatch(text.text,@"\bзв(?:езда|езду|езды|ёзды|ёзд|езд)\b",System.Text.RegularExpressions.RegexOptions.IgnoreCase),"removed currency absent on "+page+"/"+text.name);
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
    Check(restored.S.coins==12770&&restored.S.stars==0&&restored.S.bed==8&&restored.S.quest==24&&restored.S.cares==123&&restored.S.feeds==45&&restored.S.starter,"old economy and care progress retained "+i);
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
  // New presentation QA deliberately avoids ad simulation and never reads fixture data from live keys.
  static IEnumerator CheckDesktopLayout(CapyIdleGame g){
   var desktop=g.GetComponentInChildren<CapyDesktopLayout>();Check(desktop,"adaptive desktop component exists");
   Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Show("home");yield return new WaitForEndOfFrame();
   bool wide=Screen.safeArea.width>=900&&Screen.safeArea.height>=600&&Screen.safeArea.width/Screen.safeArea.height>=1.3f;
   Check(desktop.IsDesktop==wide,"desktop mode matches available viewport");
   if(!wide){Check(!Find(g,"Desktop nav care"),"mobile has no desktop navigation overlay");Frame("adaptive-home");yield break;}
   foreach(string page in new[]{"home","care","activities","shop","surprises","quests"}){
    g.Show(page);yield return new WaitForEndOfFrame();Typography(g,"desktop-"+page);Frame("desktop-"+page);
    foreach(var b in desktop.GetComponentsInChildren<Button>()){
     var r=(RectTransform)b.transform;var corners=new Vector3[4];r.GetWorldCorners(corners);
     foreach(var corner in corners)Check(corner.x>=0&&corner.x<=Screen.width&&corner.y>=0&&corner.y<=Screen.height,"desktop button remains on screen "+b.name);
     Check(Vector3.Distance(corners[0],corners[1])>=44,"desktop physical target >=44px "+b.name);
     if(b.interactable)Check(Clickable(b),"desktop navigation raycast "+b.name);
    }
   }
   g.Show("home");yield return new WaitForEndOfFrame();Find(g,"Desktop nav care").onClick.Invoke();Check(g.Page=="care","desktop care opens by click");
   yield return new WaitForEndOfFrame();Find(g,"Desktop nav activities").onClick.Invoke();Check(g.Page=="activities","desktop activities opens by click");
   Call(g,"PlatformPause",true);Find(g,"Desktop nav shop").onClick.Invoke();Check(g.Page=="activities","desktop cannot navigate through pause");Call(g,"PlatformPause",false);
   foreach(string page in new[]{"welcome","offline"}){g.Show(page);yield return new WaitForEndOfFrame();Check(!Clickable(Find(g,"Desktop nav care")),"desktop navigation suppressed on "+page);Frame("desktop-"+page);}
   g.Show("home");
  }
  static IEnumerator CheckBirthIntro(CapyIdleGame g){
   Check(CapyIdleGame.PersistenceKey.EndsWith(".validation"),"birth tests use isolated persistence");
   var previous=g.Model;
   string[] fields={"welcomeBirthPending","welcomeBirthActive","welcomeOfflinePending","welcomeGiftPending","offlineAward","suspended","platformPaused"};
   var before=new object[fields.Length];for(int n=0;n<fields.Length;n++)before[n]=typeof(CapyIdleGame).GetField(fields[n],F).GetValue(g);
   string[] fixtureKeys={CapyIdleGame.PersistenceKey,CapyIdleGame.PersistenceKey+".backup","capy67.progress.v2.validation","capy67.save.v1.validation"};
   var fixtureValues=new string[fixtureKeys.Length];var fixtureExists=new bool[fixtureKeys.Length];
   for(int n=0;n<fixtureKeys.Length;n++){fixtureExists[n]=PlayerPrefs.HasKey(fixtureKeys[n]);fixtureValues[n]=PlayerPrefs.GetString(fixtureKeys[n],"");}
   var birthPending=typeof(CapyIdleGame).GetField("welcomeBirthPending",F);
   var birthActive=typeof(CapyIdleGame).GetField("welcomeBirthActive",F);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    typeof(CapyIdleGame).GetField("welcomeOfflinePending",F).SetValue(g,false);
    typeof(CapyIdleGame).GetField("welcomeGiftPending",F).SetValue(g,false);
    typeof(CapyIdleGame).GetField("offlineAward",F).SetValue(g,0d);
    foreach(string key in fixtureKeys)PlayerPrefs.DeleteKey(key);
    Call(g,"Load");Check((bool)birthPending.GetValue(g)&&!g.Model.S.birthIntroSeen,"empty isolated installation schedules birth");
    var fresh=g.Model;double coins=fresh.S.coins;int food=fresh.S.premiumFood;bool starter=fresh.S.starter;
    g.Show("welcome");
    Check((bool)birthActive.GetValue(g)&&fresh.S.birthIntroSeen,"birth begins once and marks seen immediately");
    var persisted=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));
    var backup=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey+".backup"));
    Check(persisted.birthIntroSeen&&backup.birthIntroSeen&&!persisted.starter,"birth flag committed to primary and backup without starter grant");
    yield return new WaitForEndOfFrame();
    Check(Clickable(Find(g,"Birth skip"))&&Clickable(Find(g,"Welcome start")),"birth can be skipped or continued immediately");
    Typography(g,"birth-closed");Frame("birth-closed");
    var left=(RectTransform)typeof(CapyIdleGame).GetField("welcomeBirthLeft",F).GetValue(g);
    var right=(RectTransform)typeof(CapyIdleGame).GetField("welcomeBirthRight",F).GetValue(g);
    var initialLeft=left.anchoredPosition;var initialRight=right.anchoredPosition;
    Call(g,"AnimateWelcome",1.1f);yield return new WaitForEndOfFrame();Frame("birth-unfolding");
    Check(left.anchoredPosition.x<initialLeft.x&&right.anchoredPosition.x>initialRight.x,"birth surrounding leaves unfold in opposite directions");
    var pausedLeft=left.anchoredPosition;var pausedRight=right.anchoredPosition;
    Call(g,"PlatformPause",true);Call(g,"AnimateWelcome",4f);
    Check(left.anchoredPosition==pausedLeft&&right.anchoredPosition==pausedRight,"birth leaves stay still while platform paused");
    Call(g,"PlatformPause",false);Call(g,"AnimateWelcome",3f);yield return new WaitForEndOfFrame();Typography(g,"birth-revealed");Frame("birth-revealed");
    Check(((Text)typeof(CapyIdleGame).GetField("welcomeBirthCaption",F).GetValue(g)).text=="Теперь вы вместе!","birth finishes with a clear invitation");
    Check(g.Page=="welcome","birth has no timed forced dismissal");
    Check(fresh.S.coins==coins&&fresh.S.premiumFood==food&&fresh.S.starter==starter,"birth animation grants no coins food or starter bowl");
    var skip=Find(g,"Birth skip").onClick;for(int n=0;n<30;n++)skip.Invoke();
    Check(g.Page=="welcome"&&!((bool)birthActive.GetValue(g))&&fresh.S.birthNamePending,"rapid birth skip opens naming once");
    Check(fresh.S.coins==coins&&fresh.S.premiumFood==food&&fresh.S.starter==starter,"rapid birth skip grants nothing");
    var input=g.GetComponentInChildren<InputField>();
    Check(input&&input.characterLimit==20&&!input.isFocused,"name field has twenty character limit without forced keyboard");
    yield return new WaitForEndOfFrame();Typography(g,"birth-name-empty");Frame("birth-name-empty");
    Check(Clickable(Find(g,"Birth name confirm"))&&Clickable(Find(g,"Birth name later")),"both naming actions reachable");
    EventSystem.current.SetSelectedGameObject(input.gameObject);input.ActivateInputField();yield return null;yield return new WaitForEndOfFrame();
    Check(g.WelcomeNameInputFocused,"focused naming field suppresses gameplay keyboard shortcuts");
    input.text="  Мандарин  Друг-7  ";
    Check(g.Model.S.petName=="Мандарин Друг-7","name normalizes leading trailing and repeated spaces");
    input.text=new string('W',25);Check(input.text.Length==20&&g.Model.S.petName.Length==20,"pasted long name is capped at twenty");
    input.DeactivateInputField();input.onEndEdit.Invoke(input.text);
    yield return new WaitForEndOfFrame();Typography(g,"birth-name-long");Frame("birth-name-long");
    Check(IdleModel.NormalizePetName(" \n\t ")==IdleModel.DefaultPetName,"empty control-only name uses safe default");
    Check(IdleModel.NormalizePetName("Капи<>\n🙂")=="Капи","markup delimiters controls and unsupported surrogate pairs are removed");
    Call(g,"Load");g.Show("welcome");input=g.GetComponentInChildren<InputField>();
    Check(input&&input.text==new string('W',20)&&g.Model.S.birthNamePending,"unfinished naming survives reload");
    var confirm=Find(g,"Birth name confirm").onClick;for(int n=0;n<30;n++)confirm.Invoke();
    Check(g.Page=="home"&&!g.Model.S.birthNamePending&&!g.Model.S.starter,"confirm finishes once without starter grant");
    yield return new WaitForEndOfFrame();Typography(g,"birth-name-home");Frame("birth-name-home");
    persisted=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));
    backup=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey+".backup"));
    Check(persisted.petName==new string('W',20)&&backup.petName==persisted.petName&&!backup.birthNamePending,"confirmed name committed to primary and backup");
    g.Show("welcome");Check(!Find(g,"Birth skip"),"same-session welcome does not replay birth");
    yield return new WaitForEndOfFrame();Typography(g,"birth-name-return");Frame("birth-name-return");
    Call(g,"Load");g.Show("welcome");Check(!(bool)birthActive.GetValue(g)&&!Find(g,"Birth skip"),"reload after birth does not replay");
    // This is a genuinely old JSON with no new flag, not a freshly serialized default of the new schema.
    var old=new IdleModel(new IdleSave{coins=5000,growth=6,starter=true,sound=false});old.Grant("set_gardener");old.Purchase("set_gardener");
    string legacyJson=JsonUtility.ToJson(old.S).Replace("\"birthIntroSeen\":false,","").Replace(",\"birthIntroSeen\":false","");
    Check(!legacyJson.Contains("birthIntroSeen"),"old profile fixture has no birth flag");
    legacyJson=legacyJson.Replace("\"birthNamePending\":false,","").Replace("\"petName\":\"Капи\",","");
    Check(!legacyJson.Contains("petName")&&!legacyJson.Contains("birthNamePending"),"old profile fixture has no naming fields");
    PlayerPrefs.SetString(fixtureKeys[0],legacyJson);PlayerPrefs.DeleteKey(fixtureKeys[1]);
    Call(g,"Load");Check(!(bool)birthPending.GetValue(g)&&g.Model.Stage==2&&g.Model.S.coins==5000&&g.Model.S.owned.Contains("set_gardener"),"old profile keeps stage coins and purchases without birth replay");
    Check(g.Model.S.petName=="Капи"&&!g.Model.S.birthNamePending,"old profiles receive safe name without interrupted onboarding");
    string repaired=JsonUtility.ToJson(g.Model.S);g.Model.Repair();Check(repaired==JsonUtility.ToJson(g.Model.S),"old profile repeated repair is idempotent");
    g.Show("welcome");Check(!Find(g,"Birth skip"),"old profile uses returning welcome");
    PlayerPrefs.SetString(fixtureKeys[0],"{broken json");PlayerPrefs.SetString(fixtureKeys[1],legacyJson);
    Call(g,"Load");Check(!(bool)birthPending.GetValue(g)&&g.Model.S.coins==5000&&g.Model.Stage==2&&g.Model.S.owned.Contains("set_gardener"),"damaged primary recovers old backup without birth or purchase loss");
    Call(g,"Load");Check(!(bool)birthPending.GetValue(g)&&g.Model.S.coins==5000,"backup recovery repeated load remains stable");
    // Legacy installations must not be mistaken for a newborn even without a modern save.
    PlayerPrefs.DeleteKey(fixtureKeys[0]);PlayerPrefs.DeleteKey(fixtureKeys[1]);
    PlayerPrefs.SetString(fixtureKeys[2],"{\"owned\":[\"set_gardener\"],\"reducedEffects\":true}");
    Call(g,"Load");Check(!(bool)birthPending.GetValue(g)&&g.Model.S.owned.Contains("set_gardener")&&g.Model.S.calm,"isolated legacy cosmetics import suppresses birth and preserves ownership");
    foreach(string key in fixtureKeys)PlayerPrefs.DeleteKey(key);
    PlayerPrefs.SetString(fixtureKeys[3],"{\"sound\":false}");Call(g,"Load");
    Check(!(bool)birthPending.GetValue(g)&&!g.Model.S.sound,"legacy settings installation does not replay birth");
    foreach(string key in fixtureKeys)PlayerPrefs.DeleteKey(key);
    Call(g,"Load");g.Model.S.calm=true;g.Model.S.sound=false;g.Show("welcome");yield return new WaitForEndOfFrame();
    left=(RectTransform)typeof(CapyIdleGame).GetField("welcomeBirthLeft",F).GetValue(g);var calmPosition=left.anchoredPosition;
    Call(g,"AnimateWelcome",10f);Check(left.anchoredPosition==calmPosition,"calm birth is fully revealed without motion");
    Typography(g,"birth-calm");Frame("birth-calm");Check(Clickable(Find(g,"Welcome start")),"calm birth start remains reachable");
    Find(g,"Welcome start").onClick.Invoke();Check(g.Page=="welcome"&&g.Model.S.birthNamePending,"calm birth leads to naming");
    Find(g,"Birth name later").onClick.Invoke();Check(g.Page=="home"&&!g.Model.S.starter&&g.Model.S.petName=="Капи"&&!g.Model.S.birthNamePending,"default name enters home without claiming starter");
    // Leaving during anticipation persists completion; no stale timer should force a page change.
    foreach(string key in fixtureKeys)PlayerPrefs.DeleteKey(key);Call(g,"Load");g.Show("welcome");g.Show("settings");Call(g,"AnimateWelcome",10f);
    Check(g.Page=="settings","closing birth cancels its page activity");Call(g,"Load");g.Show("welcome");Check(!Find(g,"Birth skip"),"interrupted birth is not replayed after reload");
   }finally{
    for(int n=0;n<fixtureKeys.Length;n++){if(fixtureExists[n])PlayerPrefs.SetString(fixtureKeys[n],fixtureValues[n]);else PlayerPrefs.DeleteKey(fixtureKeys[n]);}
    PlayerPrefs.Save();typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);
    for(int n=0;n<fields.Length;n++)typeof(CapyIdleGame).GetField(fields[n],F).SetValue(g,before[n]);
    g.Show("home");
   }
  }
  static IEnumerator CheckGiftLidPresentation(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=150000,coins=100000,sound=false});
   bool wasPaused=(bool)typeof(CapyIdleGame).GetField("platformPaused",F).GetValue(g),wasSuspended=(bool)typeof(CapyIdleGame).GetField("suspended",F).GetValue(g);
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    for(int rarity=0;rarity<4;rarity++){
     var item=m.SurprisePool(rarity)[0];m.S.surpriseResults=new List<GardenReward>{new GardenReward{tier=rarity,item=item.id,name=item.name,coins=25}};
     m.Grant(item.id);m.S.surpriseTotal=20+rarity;g.Save();
     int total=m.S.surpriseTotal,owned=m.S.owned.Count,food=m.S.premiumFood;uint random=m.S.randomState;double coins=m.S.coins;
     string receipt=string.Join("|",m.S.surpriseResults.ConvertAll(r=>JsonUtility.ToJson(r)));
     g.Show("surprise-opening");yield return new WaitForEndOfFrame();
     var stage=g.transform.Find("Idle Canvas/Idle board/Pages/Opening stage");
     Check(stage!=null,"gift stage exists "+rarity);
     RectTransform lid=null,body=null,reward=null;
     foreach(var rect in stage.GetComponentsInChildren<RectTransform>(true)){if(rect.name=="Gift lid")lid=rect;if(rect.name=="Gift body")body=rect;if(rect.name=="Gift reward medallion")reward=rect;}
     Check(lid&&body&&reward&&lid!=body,"gift lid body and revealed reward are independent "+rarity);
     foreach(var graphic in stage.GetComponentsInChildren<Graphic>(true))Check(!graphic.raycastTarget,"gift decoration does not intercept input "+rarity+"/"+graphic.name);
     var bodyStart=body.anchoredPosition;var lidStart=lid.anchoredPosition;
     g.Tweens.Step(.3f);yield return new WaitForEndOfFrame();Typography(g,"gift-anticipation-"+rarity);Frame("gift-anticipation-"+rarity);
     var lidPause=lid.anchoredPosition;var lidRotation=lid.localRotation;var rewardPause=reward.anchoredPosition;
     Call(g,"PlatformPause",true);g.Tweens.Step(5);
     Check(g.Page=="surprise-opening"&&lid.anchoredPosition==lidPause&&lid.localRotation==lidRotation&&reward.anchoredPosition==rewardPause,"gift animation freezes during platform pause "+rarity);
     Call(g,"PlatformPause",false);g.Tweens.Step(.65f);yield return new WaitForEndOfFrame();Frame("gift-lid-flight-"+rarity);
     Check((lid.anchoredPosition-lidStart).sqrMagnitude>100&&body.anchoredPosition==bodyStart,"only lid lifts independently from box body "+rarity);
     g.Tweens.Step(.7f);yield return new WaitForEndOfFrame();Typography(g,"gift-revealed-"+rarity);Frame("gift-revealed-"+rarity);
     Check(reward.GetComponent<CanvasGroup>().alpha>.9f,"gift reward visible after lid opens "+rarity);
     Check(Clickable(Find(g,"Skip opening")),"gift reveal skip remains reachable "+rarity);
     var skip=Find(g,"Skip opening").onClick;for(int n=0;n<30;n++)skip.Invoke();g.Tweens.Step(5);
     Check(g.Page=="surprise-results"&&m.S.surpriseTotal==total&&m.S.randomState==random&&m.S.owned.Count==owned,"rapid reveal skip cannot roll or grant again "+rarity);
     g.Show("surprise-opening");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="surprise-results","gift auto reveal completes "+rarity);
     g.Show("surprise-opening");g.Show("home");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="home","closed gift has no stale result callback "+rarity);
     m.S.calm=true;g.Show("surprise-opening");Check(g.Page=="surprise-results","calm gift directly reveals saved result "+rarity);m.S.calm=false;
     Check(m.S.surpriseTotal==total&&m.S.randomState==random&&m.S.owned.Count==owned&&m.S.coins==coins&&m.S.premiumFood==food&&receipt==string.Join("|",m.S.surpriseResults.ConvertAll(r=>JsonUtility.ToJson(r))),"gift lifecycle preserves receipt coins food and ownership "+rarity);
    }
    // Commit a real model batch before replaying its presentation; no fixture grants by the animation.
    m.S.surpriseRun=0;m.S.surprisePaid=0;m.S.surpriseBreakUntil=0;m.S.coins=100000;
    m.RefreshSurprises(DateTime.UtcNow.ToString("yyyy-MM-dd"));m.S.surpriseBudget=100000;m.S.surpriseSpent=0;
    Call(g,"CompleteGarden",10,false,false);
    Check(m.S.surpriseResults.Count==10&&g.Page=="surprise-opening","actual ten-gift purchase enters presentation after commit");
    var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));
    Check(saved.surpriseResults.Count==10&&saved.surpriseTotal==m.S.surpriseTotal&&saved.surprisePaid==10,"ten-gift receipt persisted before animation");
    int batchTotal=m.S.surpriseTotal,batchOwned=m.S.owned.Count,batchFood=m.S.premiumFood;uint batchRandom=m.S.randomState;double batchCoins=m.S.coins;
    string batchReceipt=string.Join("|",m.S.surpriseResults.ConvertAll(r=>JsonUtility.ToJson(r)));
    yield return new WaitForEndOfFrame();Frame("gift-ten-closed");g.Tweens.Step(1.7f);yield return new WaitForEndOfFrame();Frame("gift-ten-open");
    Find(g,"Skip opening").onClick.Invoke();g.Tweens.Step(5);
    for(int n=0;n<10;n++){if(n>0)Find(g,"Next reward").onClick.Invoke();yield return new WaitForEndOfFrame();Check(g.Page=="surprise-results","ten-gift result navigable "+n);}
    g.Show("surprise-opening");yield return new WaitForEndOfFrame();g.Tweens.Step(5);
    Check(m.S.surpriseTotal==batchTotal&&m.S.owned.Count==batchOwned&&m.S.randomState==batchRandom&&m.S.coins==batchCoins&&m.S.premiumFood==batchFood&&batchReceipt==string.Join("|",m.S.surpriseResults.ConvertAll(r=>JsonUtility.ToJson(r))),"batch replay and navigation never grant or reroll");
    var roundTrip=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));
    Check(roundTrip.S.surpriseResults.Count==10&&roundTrip.S.surpriseTotal==batchTotal&&roundTrip.S.randomState==batchRandom,"batch reload preserves all ten saved outcomes");
   }finally{
    Call(g,"PlatformPause",false);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");
    typeof(CapyIdleGame).GetField("platformPaused",F).SetValue(g,wasPaused);typeof(CapyIdleGame).GetField("suspended",F).SetValue(g,wasSuspended);
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
     drag.OnEndDrag(e);PayCoins(g);Check(g.Page=="home"&&m.S.feeds==feeds+1,"drop feeds once "+food);
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
     Call(g,"DoCare",1);PayCoins(g);Call(g,"AnimateCare",.2f);
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
     Check(Clickable(buy),"tap upgrade raycast "+n);buy.onClick.Invoke();PayCoins(g);
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
    m.S.careTutorial=1;m.S.starter=false;typeof(CapyIdleGame).GetField("noticeAge",F).SetValue(g,5f);Call(g,"RefreshHome");
    string hint=(string)typeof(CapyIdleGame).GetMethod("CareHint",F).Invoke(g,null);
    Check(hint.Contains("Нажми здесь")&&hint.Contains("бесплатно")&&Find(g,"Next goal")!=null,"first meal guidance names existing control");
    g.Show("care");double cancelCoins=m.S.coins,cancelComfort=m.S.comfort;Find(g,"Care action 1").onClick.Invoke();
    Check(m.S.coins==cancelCoins&&m.S.comfort==cancelComfort,"opening care quote does not charge or heal");Find(g,"Cancel currency choice").onClick.Invoke();
    Check(m.S.coins==cancelCoins&&m.S.comfort==cancelComfort,"cancel care quote preserves wallet and needs");
    m.S.starter=true;m.S.careTutorial=3;m.S.comfort=95;m.S.mood=95;
    Call(g,"DoCare",1);PayCoins(g);
    string feedback=(string)typeof(CapyIdleGame).GetField("toast",F).GetValue(g);
    Check(feedback.Contains("уют +5")&&feedback.Contains("настроение +5")&&!feedback.Contains("+40"),"care feedback reports capped actual gains");
    g.Show("care");yield return new WaitForEndOfFrame();
    var cooling=Find(g,"Care action 1");
    Check(!cooling.interactable&&cooling.transform.Find("Action effect").GetComponent<Text>().text.StartsWith("Ещё"),"action cooldown is explained on button");
    Typography(g,"care-cooldown");Frame("care-cooldown");
    m.Tick(4);Call(g,"DoCare",5);PayCoins(g);m.Tick(3);g.Show("care");yield return new WaitForEndOfFrame();
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
    var advance=typeof(IdleHold).GetMethod("AdvanceHold",F);Check(advance!=null,"hold exposes its production time step for deterministic verification");
    int longCallbacks=0;var originalLong=hold.longPressed;
    hold.longPressed=()=>{longCallbacks++;originalLong();};
    try{
     advance.Invoke(hold,new object[]{.59f});Check(hold.Held&&m.S.cares==taps&&longCallbacks==0&&!Find(g,"Pay with coins"),"hold below threshold does not tap or quote");
     advance.Invoke(hold,new object[]{.02f});var payment=Find(g,"Pay with coins");
     Check(m.S.cares==taps&&payment&&longCallbacks==1,"long pet requests payment once without a tap");
     advance.Invoke(hold,new object[]{1f});hold.OnPointerUp(pointer);
     Check(m.S.cares==taps&&longCallbacks==1&&Find(g,"Pay with coins")==payment,"consumed hold and release cannot repeat the quote or tap");
     PayCoins(g);Check(m.S.lastPet>0,"paid long pet applies the reaction");
     m.Tick(11);g.Show("home");Call(g,"Suspend",false);taps=m.S.cares;
     hold.OnPointerDown(pointer);advance.Invoke(hold,new object[]{.3f});Call(g,"Suspend",true);
     advance.Invoke(hold,new object[]{1f});hold.OnPointerUp(pointer);
     Check(!hold.Held&&m.S.cares==taps&&longCallbacks==1&&!Find(g,"Pay with coins"),"focus suspension cancels hold without payment or tap");
     Call(g,"Suspend",false);
    }finally{hold.longPressed=originalLong;Call(g,"Suspend",false);}
    for(int age=1;age<=5;age++){
     m.S.growth=IdleModel.Growth[age-1];m.Grant("set_gardener");m.Purchase("set_gardener");m.Grant("neon");m.Purchase("neon");
     for(int action=0;action<6;action++){
      m.Tick(12);m.S.fullness=m.S.comfort=m.S.mood=40;
      g.Show("care");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      Typography(g,"care-page");if(age==2&&action==0)Frame("care-page");
      var button=Find(g,"Care action "+action);Check(button.interactable&&Clickable(button),"care action reachable "+age+"/"+action);
      double oldGrowth=m.S.growth,oldCoins=m.S.coins;int oldFeeds=m.S.feeds;button.onClick.Invoke();
      Check(m.S.coins==oldCoins&&g.Page=="care","care waits for payment "+action);PayCoins(g);
      Check(Math.Abs(m.S.coins-oldCoins+IdleModel.CarePrice(action))<.001,"care charges exact price "+action);
      Check(g.Page=="home"&&g.CareReaction==action,"care action responds "+age+"/"+action);
      Check(m.S.growth==oldGrowth&&m.S.feeds==oldFeeds,"paid care does not buy growth "+action);
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
      double before=m.S.growth;button.onClick.Invoke();PayCoins(g);Check(g.Page=="home"&&m.S.growth>before,"new food grows pet "+food);
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
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=360,coins=20000,sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);g.Show("home");yield return new WaitForEndOfFrame();
    Check(Find(g,"Album")==null&&Clickable(Find(g,"Surprises")),"tree replaces old album navigation");
    var viewport=g.transform.Find("Idle Canvas/Idle board/Home/Decor/Room scenery/Scenery viewport") as RectTransform;
    Check(viewport!=null&&viewport.rect.height==850&&viewport.Find("Sky continuation")&&viewport.Find("Ground continuation"),"full scene continues behind both HUD panels");Frame("full-height-home");
    Find(g,"Surprises").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(g.Page=="surprises"&&Clickable(Find(g,"Open surprise ten")),"tree and batch accessible");
    Typography(g,"surprises");Frame("surprise-tree");
    Check(Find(g,"Open surprise one").GetComponentInChildren<Text>().text=="ОТКРЫТЬ 1 РАЗ"&&Find(g,"Open surprise ten").GetComponentInChildren<Text>().text=="ОТКРЫТЬ 10 РАЗ","opening buttons have stable action labels");
    var priceLabel=g.transform.Find("Idle Canvas/Idle board/Pages/Single opening price").GetComponent<Text>();
    Check(priceLabel.text=="300"&&priceLabel.rectTransform.anchoredPosition.y>Find(g,"Open surprise one").GetComponent<RectTransform>().anchoredPosition.y,"single price separately above button");
    var budgetSaved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));
    Check(budgetSaved.surpriseBudget==4000&&budgetSaved.surpriseDate==m.S.surpriseDate,"tree budget committed on first entry");
    Check(Clickable(Find(g,"Surprise spending details")),"budget details reachable");Find(g,"Surprise spending details").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(g.Page=="surprise-budget","budget explanation opens");Typography(g,"tree-budget");Frame("tree-budget");Find(g,"Back").onClick.Invoke();Check(g.Page=="surprises","budget back returns to tree");
    double budget=m.S.surpriseBudget;
    foreach(var blocked in new[]{GardenBlock.Budget,GardenBlock.Reserve,GardenBlock.Daily,GardenBlock.Series,GardenBlock.Pause}){
     m.S.coins=20000;m.S.surpriseBudget=budget;m.S.surprisePaid=0;m.S.surpriseRun=0;m.S.surpriseBreakUntil=0;
     if(blocked==GardenBlock.Budget)m.S.surpriseBudget=0;
     if(blocked==GardenBlock.Reserve)m.S.coins=399;
     if(blocked==GardenBlock.Daily)m.S.surprisePaid=10;
     if(blocked==GardenBlock.Series)m.S.surpriseRun=2;
     if(blocked==GardenBlock.Pause)m.S.surpriseBreakUntil=m.S.activeSeconds+120;
     g.Show("surprises");yield return new WaitForEndOfFrame();Typography(g,"tree-block-"+blocked);Frame("tree-block-"+blocked);
     Check(!Find(g,"Open surprise ten").interactable&&Find(g,"Open surprise ten").GetComponentInChildren<Text>().text=="ОТКРЫТЬ 10 РАЗ","blocked batch keeps action label "+blocked);
     double funds=m.S.coins;uint random=m.S.randomState;Call(g,"OpenGarden",10,false);Check(m.S.coins==funds&&m.S.randomState==random,"blocked batch cannot spend or reroll "+blocked);
    }
    m.S.coins=20000;m.S.surpriseBudget=budget;m.S.surprisePaid=0;m.S.surpriseRun=0;m.S.surpriseBreakUntil=0;
    m.Grant(m.SurprisePool(1)[0].id);
    foreach(int pity in new[]{0,9}){
     m.S.surprisePity=pity;
     for(int tier=0;tier<4;tier++){typeof(CapyIdleGame).GetField("surpriseTier",F).SetValue(g,tier);typeof(CapyIdleGame).GetField("surpriseShelf",F).SetValue(g,0);g.Show("surprise-pool");yield return new WaitForEndOfFrame();Typography(g,"tree-odds-"+pity+"-"+tier);Frame("tree-odds-"+pity+"-"+tier);}
    }
    m.S.surprisePity=0;g.Show("surprise-odds");yield return new WaitForEndOfFrame();Typography(g,"tree-protection-explanation");Frame("tree-protection-explanation");g.Show("surprises");
    Check(!Find(g,"Surprise free gifts"),"tickets and sprouts shortcut removed");Find(g,"Surprise spending details").onClick.Invoke();Find(g,"Budget free tickets").onClick.Invoke();yield return new WaitForEndOfFrame();
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
     var item=m.SurprisePool(rarity)[0];m.S.surpriseResults=new List<GardenReward>{new GardenReward{tier=rarity,item=item.id,name=item.name,coins=25}};
     m.S.calm=false;g.Show("surprise-opening");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Call(g,"PlatformPause",false);
     int receipt=m.S.surpriseTotal;g.Tweens.Step(.3f);Call(g,"PlatformPause",true);g.Tweens.Step(5);Check(g.Page=="surprise-opening","opening pauses with platform "+rarity);Call(g,"PlatformPause",false);
     g.Tweens.Step(.9f);yield return new WaitForEndOfFrame();Typography(g,"opening-"+rarity);Frame("opening-"+rarity);
     Check(Clickable(Find(g,"Skip opening")),"opening can be skipped "+rarity);Find(g,"Skip opening").onClick.Invoke();g.Tweens.Step(5);Check(g.Page=="surprise-results"&&m.S.surpriseTotal==receipt,"skip does not charge or reroll "+rarity);
     g.Show("surprise-opening");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="surprise-results","opening completes automatically "+rarity);
     g.Show("surprise-opening");g.Show("home");yield return new WaitForEndOfFrame();g.Tweens.Step(5);Check(g.Page=="home","closing opening kills callback "+rarity);
    }
    m.S.surpriseResults=new List<GardenReward>{new GardenReward{tier=0,name="Монеты для заботы",coins=75}};
    g.Show("surprise-results");yield return new WaitForEndOfFrame();Typography(g,"gift-detail");Frame("surprise-resource-gift");Check(Find(g,"Cycle talent")==null,"resource gift has no talent control");
    m.S.calm=true;g.Show("surprise-opening");Check(g.Page=="surprise-results","reduced motion skips opening");m.S.calm=false;
    m.S.forms.Add("chill");m.S.form="chill";g.Show("surprise-results");yield return new WaitForEndOfFrame();Check(Find(g,"Cycle talent")==null&&m.S.form=="chill","talent button removed without changing owned talent");
    m.S.surpriseRun=0;m.S.surprisePity=9;g.Show("surprises");yield return new WaitForEndOfFrame();Find(g,"Open surprise one").onClick.Invoke();PayCoins(g);Check(m.S.surpriseResults[0].tier>=2,"UI guarantee uses authoritative model");
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
  static IEnumerator CheckSectionGuides(CapyIdleGame g){
   var previous=g.Model;typeof(CapyIdleGame).GetProperty("Model").SetValue(g,new IdleModel(new IdleSave{sound=false,starter=true,growth=360,coins=10000}));
   try{
   string[] pages={"food","care","shop","growth","tap","activities","surprises","daily","quests","mandarins","offline","settings"};
   foreach(string page in pages){
    g.Show(page);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    var open=Find(g,"Section help");Check(Clickable(open),"section help reachable "+page);
    double coins=g.Model.S.coins;int mandarins=g.Model.M.balance;string outfit=g.Model.S.equipped[0];
    open.onClick.Invoke();yield return new WaitForEndOfFrame();
    var close=Find(g,"Close section help");Check(Clickable(close),"guide close reachable "+page);
    Check(!Clickable(Find(g,"Back")),"guide shields page controls "+page);
    var overlay=g.transform.Find("Idle Canvas/Idle board/Section picture guide");
    for(int n=0;n<3;n++)Check(overlay.Find("Guide picture "+n)!=null,"three illustrated steps "+page+"/"+n);
    Typography(g,"help-"+page);Frame("help-"+page);
    Check(g.Model.S.coins==coins&&g.Model.M.balance==mandarins&&g.Model.S.equipped[0]==outfit,"help does not buy or equip "+page);
    Call(g,"GoBack");Check(g.Page==page&&Find(g,"Close section help")==null,"back dismisses guide only "+page);
    open.onClick.Invoke();yield return new WaitForEndOfFrame();Find(g,"Close section help").onClick.Invoke();
    Check(g.Page==page&&Find(g,"Close section help")==null,"understood keeps current section "+page);
    yield return new WaitForEndOfFrame();Typography(g,"guided-section-"+page);Frame("guided-section-"+page);
   }
   g.Show("food");Find(g,"Section help").onClick.Invoke();g.Show("home");yield return new WaitForEndOfFrame();
   Check(Find(g,"Close section help")==null&&Clickable(Find(g,"Care hold")),"navigation disposes overlay without blocking home");
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckProductUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Show("home");m.Tick(1);g.Care();Call(g,"RefreshHome");yield return new WaitForEndOfFrame();
    typeof(CapyIdleGame).GetField("noticeAge",F).SetValue(g,5f);Call(g,"RefreshHome");yield return new WaitForEndOfFrame();Frame("goal-hit-area");
    Check(Find(g,"Next goal")!=null,"meal goal visible after feedback");
    Check(Clickable(Find(g,"Next goal")),"first-session meal shortcut is reachable");Find(g,"Next goal").onClick.Invoke();Check(g.Page=="food","first meal opens directly");
    Find(g,"Back").onClick.Invoke();Check(g.Page=="care","food returns to care");Find(g,"Back").onClick.Invoke();Check(g.Page=="home","care returns home");
    m.S.feeds=1;m.S.cares=10;Call(g,"RefreshHome");
    Check(g.transform.Find("Idle Canvas/Idle board/Home/Daily ready badge").gameObject.activeSelf&&g.transform.Find("Idle Canvas/Idle board/Home/Quest ready badge").gameObject.activeSelf,"ready rewards appear on home");
    g.Show("quests");yield return new WaitForEndOfFrame();Check(!Find(g,"Open visitor"),"visitor shortcut removed from quests");Typography(g,"simplified-quests");Frame("simplified-quests");Find(g,"Claim quest").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(m.S.quest==1&&m.S.surpriseTickets==1,"quest reward once");Check(Clickable(Find(g,"Quest action")),"quest action available when not complete");Find(g,"Quest action").onClick.Invoke();Check(g.Page=="food","feeding quest routes to food");
    Call(g,"OpenWardrobe","set_gardener","home");yield return new WaitForEndOfFrame();
    Check(!Find(g,"Previous").interactable,"first wardrobe page has no backward action");Find(g,"Owned filter").onClick.Invoke();Find(g,"Category 3").onClick.Invoke();yield return new WaitForEndOfFrame();
    Check(!Find(g,"Equip or buy").interactable&&!Find(g,"Next").interactable,"empty owned category has no purchase or paging");Typography(g,"wardrobe-empty");Frame("product-wardrobe-empty");
    Call(g,"OpenWardrobe","set_tree_sun","surprise-craft");yield return new WaitForEndOfFrame();
    Check(Find(g,"Shop item set_tree_sun")!=null&&!Find(g,"Equip or buy").interactable,"tree preview selects its actual page and exits owned filter");Find(g,"Back").onClick.Invoke();Check(g.Page=="surprise-craft","tree preview returns to collection");
    m.Grant("set_gardener");m.Purchase("set_gardener");Call(g,"OpenWardrobe","set_gardener","home");yield return new WaitForEndOfFrame();
    Check(!Find(g,"Equip or buy").interactable&&Find(g,"Equip or buy").GetComponentInChildren<Text>().text=="УЖЕ НАДЕТО","equipped item cannot be bought again");Typography(g,"wardrobe-owned");Frame("product-wardrobe-owned");
    foreach(int prior in new[]{0,2,6}){
     m.S.dailyCount=prior;m.S.dailyDate="";m.S.calm=prior==0;m.S.owned.Remove("firefly_garden");m.S.owned.Remove("set_dawn");
     g.Show("daily");yield return new WaitForEndOfFrame();Typography(g,"calendar-before-"+prior);Frame("product-calendar-"+(prior+1));
     double coins=m.S.coins;Find(g,"Claim daily").onClick.Invoke();Check(g.Page=="daily-reward"&&m.S.dailyCount==prior+1,"calendar opens saved reward "+prior);
     int count=m.S.dailyCount;double after=m.S.coins;Call(g,"ClaimCalendar");Check(m.S.dailyCount==count&&m.S.coins==after,"reward cannot duplicate through retry "+prior);
     if(prior==2)Check(m.S.owned.Contains("firefly_garden"),"day three grants scene");if(prior==6)Check(m.S.owned.Contains("set_dawn")&&m.S.premiumFood>0,"day seven grants outfit and bowl");
     g.Tweens.Step(1);yield return new WaitForEndOfFrame();Typography(g,"daily-reward-"+prior);Frame("product-daily-reward-"+(prior+1));
     if(prior>0){Find(g,"View daily reward").onClick.Invoke();yield return new WaitForEndOfFrame();Check(g.Page=="shop","unique daily reward preview opens");Find(g,"Back").onClick.Invoke();Check(g.Page=="daily","daily preview returns to calendar");}
     else Find(g,"Daily reward return").onClick.Invoke();
     yield return new WaitForEndOfFrame();Check(!Find(g,"Claim daily").interactable,"claimed calendar disabled "+prior);Typography(g,"calendar-claimed-"+prior);Frame("product-calendar-claimed-"+(prior+1));
    }
    m.S.calm=false;g.Show("home");Call(g,"RefreshHome");Check(!g.transform.Find("Idle Canvas/Idle board/Home/Daily ready badge").gameObject.activeSelf,"daily marker clears after claim");
    g.Show("surprise-pool");Call(g,"GoBack");Check(g.Page=="surprise-odds","odds detail back navigation");Call(g,"GoBack");Check(g.Page=="surprises","odds back to tree");
   }finally{typeof(CapyIdleGame).GetField("ownedOnly",F).SetValue(g,false);typeof(CapyIdleGame).GetField("wardrobeReturn",F).SetValue(g,"home");typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckFirstSessionUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,calm=true});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    for(int step=0;step<5;step++){
     g.Show("home");typeof(CapyIdleGame).GetField("noticeAge",F).SetValue(g,5f);Call(g,"RefreshHome");yield return new WaitForEndOfFrame();
     var goal=Find(g,"Next goal");Check(m.FirstSessionStep==step&&Clickable(goal),"onboarding action reachable "+step);
     Check(goal.transform.Find("Goal title").GetComponent<Text>().text.StartsWith((step+1)+"/5"),"one current onboarding step "+step);
     Typography(g,"first-session-"+step);Frame("first-session-"+step);
     double coins=m.S.coins;Call(g,"PlatformPause",true);goal.onClick.Invoke();Check(g.Page=="home"&&m.S.coins==coins,"paused goal has no side effect "+step);Call(g,"PlatformPause",false);
     goal.onClick.Invoke();
     if(step==0){Check(m.S.cares==1,"greeting performs one manual tap");Check(!goal.gameObject.activeSelf,"tap feedback temporarily takes priority");}
     if(step==1){Check(g.Page=="food","bowl route");Find(g,"Starter bowl").onClick.Invoke();Check(m.Stage==2,"free bowl grows baby");}
     if(step==2){Check(g.Page=="food"&&!Find(g,"Bed upgrade").interactable,"bed shortage does not spend coins");m.S.coins=100;g.Show("food");Find(g,"Bed upgrade").onClick.Invoke();PayCoins(g);Check(m.S.bed==1,"guided bed purchase");}
     if(step==3){Check(g.Page=="tap","upgrade route");Find(g,"Tap upgrade 0").onClick.Invoke();PayCoins(g);Check(m.S.tapPower==1,"guided tap purchase");}
     if(step==4){Check(g.Page=="activities","activity hub route");Find(g,"Start activity 1").onClick.Invoke();m.StepActivity(25);Call(g,"TickActivity",0f);Check(g.Page=="activity-result"&&m.S.activityCompleted,"guided activity completes");}
    }
    var saved=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));
    Check(saved.FirstSessionStep==5&&saved.S.activityCompleted,"onboarding completion survives saving");
    m.S.growth=100;g.Show("home");typeof(CapyIdleGame).GetField("noticeAge",F).SetValue(g,5f);Call(g,"RefreshHome");yield return new WaitForEndOfFrame();Typography(g,"first-session-gift");Frame("first-session-gift");
    Find(g,"Next goal").onClick.Invoke();Check(g.Page=="growth","completed onboarding leads to earned gift");
    m.S.fullness=10;g.Show("home");Call(g,"RefreshHome");Find(g,"Next goal").onClick.Invoke();Check(g.Page=="care","care takes priority over growth reward");
    m.S.resting=true;m.S.restRemaining=20;g.Show("home");Check(Find(g,"Next goal")==null&&((string)typeof(CapyIdleGame).GetMethod("CareHint",F).Invoke(g,null)).Contains("разбудить"),"rest keeps wake instruction instead of goal even before bathing tutorial");m.Wake();
    m.S.fullness=75;m.S.growth=150000;m.S.growthGiftMask=511;m.S.quest=40;m.S.dailyDate=DateTime.UtcNow.ToString("yyyy-MM-dd");
    g.Show("home");Call(g,"RefreshHome");yield return new WaitForEndOfFrame();Typography(g,"first-session-grown");Frame("first-session-grown");
    Find(g,"Next goal").onClick.Invoke();Check(g.Page=="shop","fully grown capy gets decoration goal rather than nonexistent age");
    foreach(int level in new[]{1,4,8}){m.S.bed=level;g.Show("home");yield return new WaitForEndOfFrame();Frame("resting-place-"+level);}
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckPersonalityUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,starter=true,growth=6});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Show("home");
    // Cleanup is required before the next manual tick, not after a DOTween global update.
    var owner=new GameObject("Disposable tween test",typeof(RectTransform));owner.transform.SetParent(g.transform);
    bool callback=false;var tween=g.Tweens.Track(((RectTransform)owner.transform).DOScale(2,1).OnComplete(()=>callback=true),owner);
    owner.SetActive(false);Call(g,"PlatformPause",true);g.Tweens.Step(.1f);
    Check(!tween.IsActive()&&!callback,"disabled tween owner is killed even while paused");
    Call(g,"PlatformPause",false);UnityEngine.Object.Destroy(owner);
    owner=new GameObject("Destroyed tween test",typeof(RectTransform));owner.transform.SetParent(g.transform);
    tween=g.Tweens.Track(((RectTransform)owner.transform).DOScale(2,1),owner);UnityEngine.Object.Destroy(owner);
    yield return null;g.Tweens.Step(.1f);Check(!tween.IsActive(),"destroyed owner cannot reach tween setter");
    foreach(string name in new[]{"careClip","feedClip","evolveClip","purchaseClip","washClip","playClip","petClip"}){
     var clip=(AudioClip)typeof(CapyIdleGame).GetField(name,F).GetValue(g);Check(clip&&clip.channels==1&&clip.frequency==22050,"mono audio clip "+name);
     var samples=new float[clip.samples];Check(clip.GetData(samples,0),"read audio "+name);float peak=0;double energy=0;
     bool valid=true;foreach(float v in samples){valid&=!float.IsNaN(v)&&Math.Abs(v)<=.701f;peak=Math.Max(peak,Math.Abs(v));energy+=v*v;}Check(valid,"finite bounded samples "+name);
     Check(peak>.05f&&energy/samples.Length>.0001,"audible nonempty cue "+name);
     Check(samples[0]==0&&samples[samples.Length-1]==0,"cue has zero endpoints "+name);
    }
    g.Show("settings");yield return new WaitForEndOfFrame();Typography(g,"audio-settings");Frame("audio-settings");
    Check(!m.S.music&&Clickable(Find(g,"Music setting")),"music opt in is accessible");Find(g,"Music setting").onClick.Invoke();yield return null;
    var music=(AudioSource)typeof(CapyIdleGame).GetField("musicSource",F).GetValue(g);
    Check(m.S.music&&music.isPlaying&&music.loop&&music.clip.samples==423360,"original loop plays independently of effects");
    var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));Check(saved.music&&!saved.sound,"separate music setting persists");
    Call(g,"PlatformPause",true);Check(!music.isPlaying,"SDK pauses music immediately");Call(g,"PlatformPause",false);Check(music.isPlaying,"music resumes after pause");
    Call(g,"PrepareAutoAd");Check(!music.isPlaying,"pending rewarded ad silences music before SDK callback");Call(g,"AutoAdClosed");Call(g,"UpdateAudioState");Check(music.isPlaying,"ad closure resumes permitted music");
    g.Show("settings");Find(g,"Music setting").onClick.Invoke();Check(!m.S.music&&!music.isPlaying,"music stops on toggle");
    g.Show("home");yield return new WaitForEndOfFrame();
    for(int i=0;i<3;i++){
     m.Tick(1);g.Care();Call(g,"AnimateHero",.1f);yield return new WaitForEndOfFrame();Frame("personality-tap-"+i);
     var pose=(RectTransform)typeof(CapyIdleGame).GetField("livingPose",F).GetValue(g);
     Check(Math.Abs(pose.localScale.y-1)<.01f&&Math.Abs(pose.localScale.x-1)<.01f,"reaction preserves portrait proportions "+i);
     Vector2 at=pose.anchoredPosition;Quaternion rotation=pose.localRotation;
     Call(g,"PlatformPause",true);Call(g,"AnimateHero",1f);Check(pose.anchoredPosition==at&&pose.localRotation==rotation,"reaction freezes on pause "+i);Call(g,"PlatformPause",false);
    }
    m.S.calm=true;Call(g,"AnimateHero",0f);
    var calmPose=(RectTransform)typeof(CapyIdleGame).GetField("livingPose",F).GetValue(g);
    Check(calmPose.localScale==Vector3.one&&calmPose.localRotation==Quaternion.identity,"calm reactions leave portrait still");
    var heart=(RectTransform)typeof(CapyIdleGame).GetField("thanksVisual",F).GetValue(g);Check(heart.localScale==Vector3.one,"calm gratitude has no pulsing");
   }finally{Call(g,"AutoAdClosed");Call(g,"PlatformPause",false);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);Call(g,"UpdateAudioState");g.Show("home");}
  }
  static IEnumerator CheckEconomyUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,starter=true,growth=151,coins=5000});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    g.Show("home");yield return new WaitForEndOfFrame();Typography(g,"economy-home");Frame("economy-home");
    Find(g,"Growth stages").onClick.Invoke();yield return new WaitForEndOfFrame();Typography(g,"economy-growth");Frame("economy-growth");
    var gift=Find(g,"Growth gift");Check(Clickable(gift),"growth gift reachable");
    Call(g,"PlatformPause",true);gift.onClick.Invoke();Check(m.S.coins==5000&&m.PendingGrowthGifts==1,"paused growth gift cannot claim");Call(g,"PlatformPause",false);
    gift.onClick.Invoke();Check(g.Page=="home"&&m.S.coins==5050&&m.S.stars==0,"growth UI pays exact gift");
    var restored=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));
    Check(restored.S.growthGiftMask==1&&!restored.ClaimGrowthGifts(),"growth gift saved before presentation");
    g.Show("growth");yield return new WaitForEndOfFrame();Check(!Find(g,"Growth gift").interactable,"claimed growth gift disabled");Typography(g,"economy-growth-claimed");Frame("economy-growth-claimed");
    for(int stage=2;stage<=5;stage++){
     m.S.growth=IdleModel.Growth[stage-1];m.S.bed=stage==2?1:stage==3?3:stage==4?5:7;
     for(int p=0;p<3;p++){typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,p);g.Show("food");yield return new WaitForEndOfFrame();Typography(g,"economy-food-"+stage+"-"+p);if(stage==4)Frame("economy-food-"+p);}
    }
    m.S.bed=8;g.Show("food");yield return new WaitForEndOfFrame();Typography(g,"economy-bed-max");Check(!Find(g,"Bed upgrade").interactable,"max bed disabled");
    g.Show("growth");yield return new WaitForEndOfFrame();Typography(g,"economy-all-gifts");Frame("economy-all-gifts");
   }finally{typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,0);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckActivitiesUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,starter=true,growth=6});
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);
    g.Show("home");yield return new WaitForEndOfFrame();Typography(g,"home-activities");Frame("activities-home");
    Check(Clickable(Find(g,"Activities")),"home activities button is reachable");Find(g,"Activities").onClick.Invoke();Check(g.Page=="activities","home activities opens hub");Find(g,"Back").onClick.Invoke();Check(g.Page=="home","hub returns to home context");
    g.Show("care");yield return new WaitForEndOfFrame();Check(!Find(g,"Care activities"),"removed care activities button absent");g.Show("home");Find(g,"Activities").onClick.Invoke();
    yield return new WaitForEndOfFrame();Typography(g,"activities-hub");Frame("activities-hub");
    Check(Clickable(Find(g,"Start activity 0")),"basket entry reachable");
    for(int variant=0;variant<3;variant++)for(int kind=0;kind<2;kind++){
     g.Show("activities");Find(g,"Start activity "+kind).onClick.Invoke();yield return new WaitForEndOfFrame();
     Check(g.Page=="activity-play","activity starts");
     Check(m.S.activity.variant==variant,"UI starts next variant");
     if(kind==0){var slider=g.GetComponentInChildren<Slider>();Check(slider!=null,"basket has touch control");slider.value=(float)IdleModel.FruitX(m.S.activity,0);Call(g,"TickActivity",(float)IdleModel.FruitFallTime(m.S.activity)+.1f);Check(m.S.activity.score>=1,"basket control catches fruit");}
     if(kind==0){var field=(RectTransform)typeof(CapyIdleGame).GetField("activityField",F).GetValue(g);var touch=field.Find("Touch the meadow").GetComponent<EventTrigger>();var point=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,field.TransformPoint(new Vector3(330,-220,0)))};touch.triggers[0].callback.Invoke(point);Check(m.S.activity.basket>.75,"direct meadow touch steers basket");double basket=m.S.activity.basket;Call(g,"PlatformPause",true);point.position=RectTransformUtility.WorldToScreenPoint(null,field.TransformPoint(new Vector3(40,-220,0)));touch.triggers[0].callback.Invoke(point);Check(m.S.activity.basket==basket,"meadow touch respects pause");Call(g,"PlatformPause",false);}
     if(kind==1){var helper=Find(g,"Search helper");Check(helper.interactable&&Clickable(helper),"search helper reachable");helper.onClick.Invoke();Check(m.S.activity.helps==1&&m.S.activity.hintUntil>m.S.activity.elapsed,"helper starts without collecting fruit");}
     yield return new WaitForEndOfFrame();Typography(g,"activity-play-"+kind+"-"+variant);Frame("activity-play-"+kind+"-"+variant);
     CheckNoGlossyArt(g.transform,"activity "+kind+"/"+variant);
     CheckNativeArt(ArtRoot(g.transform,"Garden butterfly"),3,"animated garden butterfly");
     if(kind==0)CheckNativeArt(ArtRoot(g.transform,"Woven picnic basket"),10,"catching basket");
     m.S.calm=true;Call(g,"RefreshActivity");
     var butterfly=(RectTransform)typeof(CapyIdleGame).GetField("activityButterfly",F).GetValue(g);Check(butterfly.localScale==Vector3.one,"calm mode disables butterfly flutter");
     var glow=(RectTransform)typeof(CapyIdleGame).GetField("activityCatchGlow",F).GetValue(g);Check(!glow.gameObject.activeSelf,"calm mode disables harvest particles");m.S.calm=false;
     double elapsed=m.S.activity.elapsed;Call(g,"PlatformPause",true);Call(g,"TickActivity",1f);Check(m.S.activity.elapsed==elapsed,"activity paused with SDK");Call(g,"PlatformPause",false);
     Find(g,"Back").onClick.Invoke();Check(g.Page=="activity-exit","exit requires confirmation");yield return new WaitForEndOfFrame();Typography(g,"activity-exit");Frame("activity-exit");
     Find(g,"Leave activity").onClick.Invoke();Check(g.Page=="home"&&!m.S.activity.complete,"leaving preserves run");
     g.Show("activities");Find(g,"Start activity "+kind).onClick.Invoke();Check(m.S.activity.elapsed>=elapsed,"resume preserves time");
     yield return new WaitForEndOfFrame();
     if(kind==1)for(int i=0;i<6;i++){var target=Find(g,"Hidden mandarin "+i);Check(Clickable(target),"search target reachable in variant "+variant);target.onClick.Invoke();}
     if(kind==1){Call(g,"TickActivity",.2f);yield return new WaitForEndOfFrame();Frame("activity-found-flight");}
     m.StepActivity(25);Call(g,"TickActivity",0f);yield return new WaitForEndOfFrame();
     Check(g.Page=="activity-result"&&m.S.activity.reward>0,"activity result paid");Typography(g,"activity-result");Frame("activity-result-"+kind);
     string json=JsonUtility.ToJson(m.S);var restored=new IdleModel(JsonUtility.FromJson<IdleSave>(json));double funds=restored.S.coins;restored.StepActivity(25);Check(restored.S.coins==funds,"saved completion not repaid");
     Check(restored.S.activityRuns[kind]==variant+1&&restored.S.activityBest[kind]>=m.S.activity.score,"records survive serialized save");
     g.Show("activities");yield return new WaitForEndOfFrame();Typography(g,"activities-records-"+variant);Frame("activities-records");
    }
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckPlayroomUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,starter=true,growth=6});typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Show("activities");yield return new WaitForEndOfFrame();
    for(int k=0;k<6;k++)Check(Clickable(Find(g,"Start activity "+k))&&Find(g,"Start activity "+k).interactable,"all six hub cards reachable "+k);
    Check(!Find(g,"Start activity 6"),"exactly six hub games");Typography(g,"six-game-hub");Frame("six-game-hub");
    for(int kind=2;kind<6;kind++){
     g.Show("activities");Find(g,"Start activity "+kind).onClick.Invoke();yield return new WaitForEndOfFrame();var a=m.S.activity;
     Check(g.Page=="activity-play"&&a.kind==kind,"new game entered through hub "+kind);Typography(g,"playroom-start-"+kind);Frame("playroom-start-"+kind);
     CheckNoGlossyArt(g.transform,"activity "+kind);
     if(kind==3)for(int note=0;note<4;note++)CheckNativeArt(Find(g,"Music note "+note)?.transform.Find("Marimba instrument"),6,"marimba note "+note);
     if(kind==4)CheckNativeArt(ArtRoot(g.transform,"Picnic food"),6,"picnic food sorting");
     if(kind==5)CheckNativeArt(ArtRoot(g.transform,"Easel leg"),1,"painting easel");
     double elapsed=a.elapsed;Call(g,"PlatformPause",true);Call(g,"TickActivity",2f);Check(a.elapsed==elapsed,"new game SDK pause "+kind);Call(g,"PlatformPause",false);
     Find(g,"Back").onClick.Invoke();Find(g,"Leave activity").onClick.Invoke();g.Show("activities");Find(g,"Start activity "+kind).onClick.Invoke();yield return new WaitForEndOfFrame();Check(object.ReferenceEquals(a,m.S.activity),"new game resume preserves round "+kind);
     if(kind==2){
      Check(!Find(g,"Memory card 0").interactable,"preview cannot be clicked");Call(g,"TickActivity",2f);g.GetComponent<CapyTweenDriver>().Step(.4f);
      for(int face=0;face<3;face++){
       for(int n=0;n<6;n++)if(IdleModel.MemoryFace(a,n)==face){var b=Find(g,"Memory card "+n);Check(Clickable(b)&&b.interactable,"memory card reachable "+n);b.onClick.Invoke();}
       if(face==0){yield return new WaitForEndOfFrame();Typography(g,"memory-match");Frame("playroom-memory-match");}
       Call(g,"TickActivity",1f);
      }
     }else if(kind==3){
      Call(g,"TickActivity",3f);int wrong=(IdleModel.MusicNote(a,0)+1)%4;Find(g,"Music note "+wrong).onClick.Invoke();
      Check(a.score==0&&a.step==0&&IdleModel.MusicShowing(a),"wrong note gently restarts without score");
      Check(ArtRoot(g.transform,"Orchestra banner").GetComponent<Text>().text=="ДАВАЙ ЕЩЁ РАЗ","wrong note has distinct retry prompt");
      yield return new WaitForEndOfFrame();Frame("playroom-music-retry");
      Call(g,"TickActivity",3f);var repeat=Find(g,"Repeat melody");Check(repeat.interactable&&Clickable(repeat),"repeat tune reachable");repeat.onClick.Invoke();Check(a.helps==1&&IdleModel.MusicShowing(a),"repeat tune replays current round");
      for(int round=0;round<3;round++){
       Call(g,"TickActivity",(float)(a.sequenceAt+IdleModel.MusicLength(a)*.7+.3-a.elapsed));int length=IdleModel.MusicLength(a);
       for(int n=0;n<length;n++){var b=Find(g,"Music note "+IdleModel.MusicNote(a,n));Check(Clickable(b)&&b.interactable,"musical note reachable");b.onClick.Invoke();Check(ArtRoot(g.transform,"Melody step "+n).Find("Melody check").gameObject.activeSelf,"correct musical step has visible check");}
       if(round==0){yield return new WaitForEndOfFrame();Typography(g,"music-repeat");Frame("playroom-music-repeat");}
      }Call(g,"TickActivity",.1f);
     }else if(kind==4){
      int correctGroup=IdleModel.PicnicGroup(a);var wrongBasket=Find(g,"Picnic basket "+((correctGroup+1)%3));
      for(int tap=0;tap<20;tap++)wrongBasket.onClick.Invoke();
      Check(a.score==0,"rapid wrong picnic choices never score");
      for(int basket=0;basket<3;basket++)Check(Find(g,"Picnic basket "+basket).transform.Find("Picnic basket guide").gameObject.activeSelf==(basket==correctGroup),"only matching food group is suggested");
      Call(g,"PlatformPause",true);double pausedTime=a.elapsed;Call(g,"TickActivity",1f);wrongBasket.onClick.Invoke();Check(a.elapsed==pausedTime&&a.score==0,"picnic hint respects pause");Call(g,"PlatformPause",false);
      g.GetComponent<CapyTweenDriver>().Step(.3f);var foodArt=(RectTransform)typeof(CapyIdleGame).GetField("picnicFoodArt",F).GetValue(g);Check(Vector3.Distance(foodArt.localScale,Vector3.one)<.001f,"rapid error bounce settles exactly");
      yield return new WaitForEndOfFrame();Typography(g,"picnic-hint");Frame("playroom-picnic-hint");
      var serving=(RectTransform)typeof(CapyIdleGame).GetField("picnicPlate",F).GetValue(g);var drag=serving.GetComponentInChildren<CapyCareGesture>();Check(drag!=null,"picnic food supports dragging");
      var target=Find(g,"Picnic basket "+IdleModel.PicnicGroup(a));var targetRect=(RectTransform)target.transform;var pointer=new PointerEventData(EventSystem.current){pointerId=73,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,serving.TransformPoint(serving.rect.center))};
      drag.OnBeginDrag(pointer);pointer.position=RectTransformUtility.WorldToScreenPoint(null,targetRect.TransformPoint(targetRect.rect.center));drag.OnDrag(pointer);drag.OnEndDrag(pointer);Check(a.score==1,"drag food to matching basket sorts once");Call(g,"TickActivity",.2f);
      Check(target.transform.Find("Picnic basket success").gameObject.activeSelf,"accepted food shows check");
      var reaction=(RectTransform)target.transform.Find("Basket reaction root");Check(reaction.anchoredPosition.y>0,"basket reacts locally to accepted food");
      m.S.calm=true;Call(g,"RefreshPlayroom");Check(reaction.anchoredPosition==Vector2.zero&&target.transform.Find("Picnic basket success").gameObject.activeSelf,"calm keeps confirmation without bounce");m.S.calm=false;Call(g,"RefreshPlayroom");
      yield return new WaitForEndOfFrame();Typography(g,"picnic-flight");Frame("polish-picnic-flight");Call(g,"TickActivity",.3f);
      for(int n=1;n<6;n++){var b=Find(g,"Picnic basket "+IdleModel.PicnicGroup(a));Check(Clickable(b),"picnic basket reachable");b.onClick.Invoke();Call(g,"TickActivity",.5f);}
     }else{
      var brush=Find(g,"Paint brush");Check(brush.interactable&&Clickable(brush),"pattern brush reachable");brush.onClick.Invoke();Check(a.brush==1,"brush toggles dots");
      Check(ArtRoot(brush.transform,"Painted dot"),"brush shows actual dotted pattern");
      foreach(string touchName in new[]{"Paint brush","Paint colour 0","Finish painting"}){var corners=new Vector3[4];((RectTransform)Find(g,touchName).transform).GetWorldCorners(corners);Check((corners[1]-corners[0]).magnitude>=44,"painting actual touch height "+touchName);}
      Call(g,"TickActivity",2.6f);Check(Find(g,"Paint petal 0").transform.Find("Paint region guide").gameObject.activeSelf,"idle painter gets unpainted region hint");
      m.S.calm=true;Call(g,"RefreshPlayroom");Check(Find(g,"Paint petal 0").transform.Find("Paint region guide").localScale==Vector3.one,"calm painting hint remains still");m.S.calm=false;
      yield return new WaitForEndOfFrame();Typography(g,"paint-hint");Frame("playroom-paint-hint");
      for(int n=0;n<6;n++){var colour=Find(g,"Paint colour "+n%5);Check(Clickable(colour),"paint palette reachable");colour.onClick.Invoke();var petal=Find(g,"Paint petal "+n);Check(Clickable(petal),"petal touch reachable "+n);petal.onClick.Invoke();}
      Check(ArtRoot(g.transform,"Paint colour confirmation").GetComponent<Text>().text.Contains("готово"),"painting confirms colour application");
      Call(g,"TickActivity",3f);yield return new WaitForEndOfFrame();Typography(g,"painted-flower");Frame("playroom-painted-flower");
      var saved=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));Check(saved.S.activity.colors==a.colors&&saved.S.activity.mask==63,"painting progress serialized");
      var done=Find(g,"Finish painting");Check(done.interactable&&Clickable(done),"finish painted flower reachable");done.onClick.Invoke();
     }
     yield return new WaitForEndOfFrame();Check(a.complete&&g.Page=="activity-result"&&a.score==IdleModel.ActivityGoal(kind),"new game completes with full score "+kind);Typography(g,"playroom-result-"+kind);Frame("playroom-result-"+kind);
     double paid=m.S.coins;var receipt=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));receipt.StepActivity(100);Check(receipt.S.coins==paid&&receipt.S.activityRuns[kind]==1,"new reward saved once "+kind);
     Find(g,"Activity finish").onClick.Invoke();Check(g.Page=="activities","result returns to all games");
    }
    for(int theme=1;theme<=2;theme++){
     m.S.activityRuns[5]=theme;g.Show("activities");Find(g,"Start activity 5").onClick.Invoke();yield return new WaitForEndOfFrame();var a=m.S.activity;Check(a.variant==theme,"alternate painting theme entered");
     Find(g,"Paint brush").onClick.Invoke();Find(g,"Paint brush").onClick.Invoke();
     for(int n=0;n<6;n++){Find(g,"Paint colour "+n%5).onClick.Invoke();var region=Find(g,"Paint petal "+n);Check(Clickable(region),"alternate painting region reachable "+theme+":"+n);region.onClick.Invoke();}
     Call(g,"TickActivity",3f);g.GetComponent<CapyTweenDriver>().Step(.4f);yield return new WaitForEndOfFrame();Typography(g,"paint-theme-"+theme);Frame("polish-paint-theme-"+theme);
     int patterns=a.patterns;Find(g,"Back").onClick.Invoke();Find(g,"Resume activity").onClick.Invoke();yield return new WaitForEndOfFrame();Check(a.patterns==patterns&&a.mask==63,"patterns survive rebuilt painting page");
     Find(g,"Finish painting").onClick.Invoke();yield return new WaitForEndOfFrame();Typography(g,"paint-result-theme-"+theme);Frame("polish-paint-result-"+theme);Find(g,"Activity finish").onClick.Invoke();
    }
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static void PayCoins(CapyIdleGame g){var pay=Find(g,"Pay with coins");Check(pay&&pay.interactable,"gold confirmation available");pay.onClick.Invoke();}
  static IEnumerator CheckMandarinsUI(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{sound=false,starter=true,growth=150000,coins=10000,calm=true});
   string date=DateTime.UtcNow.ToString("yyyy-MM-dd");m.RefreshDay(date);m.RefreshSurprises(date);
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    Call(g,"Suspend",false);Call(g,"PlatformPause",false);g.Show("home");yield return new WaitForEndOfFrame();
    var plus=Find(g,"Mandarin shop plus");var wallet=Find(g,"Mandarin wallet");Check(Clickable(plus)&&Clickable(wallet),"both wallet controls reachable");
    Check(plus.GetComponent<RectTransform>().anchoredPosition.x+60<=wallet.GetComponent<RectTransform>().anchoredPosition.x,"plus left of fruit with distinct touch areas");Typography(g,"currency-home");Frame("currency-home");
    var hud=g.transform.Find("Idle Canvas/Idle board/Home");var coinIcon=(RectTransform)hud.Find("Decor/Coin");var plusFace=(RectTransform)plus.transform.Find("Compact plus visual/Compact plus face");
    Check(Math.Abs(coinIcon.TransformPoint(coinIcon.rect.center).x-plusFace.TransformPoint(plusFace.rect.center).x)<.1,"plus centered exactly below coin icon");
    var count=hud.Find("Coins").GetComponent<Text>();var rate=hud.Find("Income").GetComponent<Text>();
    Check(count.fontSize<32&&rate.rectTransform.anchoredPosition.y==count.rectTransform.anchoredPosition.y&&rate.rectTransform.anchoredPosition.x>=count.rectTransform.anchoredPosition.x+count.rectTransform.rect.width,"smaller coin amount and rate share row without overlap");
    plus.onClick.Invoke();yield return new WaitForEndOfFrame();Check(g.Page=="mandarins","plus opens new store");
    Check(IdleModel.MandarinPacks.Length==6,"six packs");foreach(var pack in IdleModel.MandarinPacks){var b=Find(g,"Pack unavailable "+pack.id);Check(b&&!b.interactable,"unconfigured checkout disabled "+pack.id);b.onClick.Invoke();}
    Check(m.M.balance==0,"draft prices never grant paid currency");Check(!Find(g,"Mandarin daily")&&!Find(g,"Mandarin stock"),"no legacy offers in store");Typography(g,"currency-packs");Frame("currency-packs");
    g.Show("mandarin-confirm");Check(g.Page=="mandarins","legacy purchase route redirects safely");
    m.M.balance=10;double price=m.TapPrice(0),coins=m.S.coins;g.Show("tap");Find(g,"Tap upgrade 0").onClick.Invoke();
    Check(m.S.coins==coins&&m.M.balance==10&&m.S.tapPower==0,"opening chooser does not pay");yield return new WaitForEndOfFrame();
    Check(Clickable(Find(g,"Pay with coins"))&&Clickable(Find(g,"Pay with mandarins"))&&Clickable(Find(g,"Cancel currency choice")),"currency options reachable");
    Check(!Clickable(Find(g,"Back")),"overlay blocks underlying page");Find(g,"Tap upgrade 1").onClick.Invoke();Check(m.S.tapMultiplier==0,"underlying action blocked even direct invocation");
    Typography(g,"currency-choice");Frame("currency-choice");Find(g,"Cancel currency choice").onClick.Invoke();Check(m.S.coins==coins&&m.M.balance==10,"cancel never spends");
    Find(g,"Tap upgrade 0").onClick.Invoke();Call(g,"GoBack");Check(g.Page=="tap"&&!Find(g,"Pay with coins")&&m.M.balance==10,"back closes only chooser");
    Find(g,"Tap upgrade 0").onClick.Invoke();var fruit=Find(g,"Pay with mandarins");Call(g,"PlatformPause",true);fruit.onClick.Invoke();Check(m.S.tapPower==0,"paused confirmation cannot spend");Call(g,"PlatformPause",false);
    fruit.onClick.Invoke();fruit.onClick.Invoke();Check(m.S.tapPower==1&&m.M.balance==9&&Math.Abs(m.S.coins-coins-IdleModel.MandarinChange(price))<.01,"single purchase and exact change on double click");
    var saved=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));Check(saved.M.balance==9&&saved.S.tapPower==1&&saved.S.coins==m.S.coins,"wallet change and upgrade persist together");
    m.S.coins=0;m.M.balance=1;g.Show("food");Find(g,"Bed upgrade").onClick.Invoke();yield return new WaitForEndOfFrame();Check(!Find(g,"Pay with coins").interactable&&Find(g,"Pay with mandarins").interactable,"mandarins work without gold");Typography(g,"currency-no-gold");Frame("currency-no-gold");
    m.M.balance=0;Find(g,"Pay with mandarins").onClick.Invoke();Check(m.S.bed==0&&m.S.coins==0,"confirmation rechecks balance");Call(g,"GoBack");
    m.S.coins=0;m.M.balance=2;typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,0);g.Show("food");Find(g,"Food 0").onClick.Invoke();Check(m.S.feeds==0,"food waits for payment");
    Find(g,"Pay with mandarins").onClick.Invoke();Check(m.S.feeds==1&&m.M.balance==1&&m.S.coins==990&&g.Page=="home","food button pays exact mandarins and feeds");
    var outfit=IdleCatalog.Find("set_baker");m.S.coins=0;m.M.balance=10;typeof(CapyIdleGame).GetField("category",F).SetValue(g,0);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,outfit.id);g.Show("shop");
    Find(g,"Equip or buy").onClick.Invoke();Check(!m.S.owned.Contains(outfit.id),"wardrobe preview does not purchase");yield return new WaitForEndOfFrame();Typography(g,"currency-outfit");Frame("currency-outfit");
    Find(g,"Pay with mandarins").onClick.Invoke();Check(m.S.owned.Contains(outfit.id)&&m.S.equipped[0]==outfit.id&&m.M.balance==10-IdleModel.MandarinCost(outfit.price)&&m.S.coins==IdleModel.MandarinChange(outfit.price),"wardrobe confirmation grants and equips once");
    Find(g,"Remove item").onClick.Invoke();coins=m.S.coins;int remaining=m.M.balance;Find(g,"Equip or buy").onClick.Invoke();Check(!Find(g,"Pay with mandarins")&&m.S.coins==coins&&m.M.balance==remaining&&m.S.equipped[0]==outfit.id,"owned outfit re-equips without payment");
    m.S.coins=5000;m.M.balance=10;m.S.surpriseBudget=10000;m.S.surpriseSpent=0;g.Show("surprises");var random=m.S.randomState;int total=m.S.surpriseTotal;
    Find(g,"Open surprise one").onClick.Invoke();Check(m.S.randomState==random&&m.S.surpriseTotal==total,"tree chooser does not roll");Find(g,"Cancel currency choice").onClick.Invoke();Check(m.S.randomState==random,"cancel cannot reroll tree");
    Find(g,"Open surprise one").onClick.Invoke();fruit=Find(g,"Pay with mandarins");fruit.onClick.Invoke();fruit.onClick.Invoke();Check(m.S.surpriseTotal==total+1&&m.S.surprisePaid==1&&m.M.balance==9,"tree shared paid limit and one reward");
    saved=new IdleModel(JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey)));Check(saved.M.balance==9&&saved.S.surpriseResults.Count==1,"tree debit and reward saved together");
    m.S.fullness=30;m.S.mood=20;m.M.stock=3;g.Show("food");Check(Find(g,"Saved food stock"),"legacy food inventory still accessible");Find(g,"Saved food stock").onClick.Invoke();yield return new WaitForEndOfFrame();Typography(g,"saved-food-stock");Find(g,"Use mandarin stock").onClick.Invoke();Check(m.M.stock==2,"legacy stock remains usable");
    m.M.balance=1000000;g.Show("home");yield return new WaitForEndOfFrame();Typography(g,"currency-wallet-max");
   }finally{typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static Transform ArtRoot(Transform parent,string name){
   foreach(var child in parent.GetComponentsInChildren<Transform>())if(child.name==name&&child.gameObject.activeInHierarchy)return child;
   return null;
  }
  static void CheckNativeArt(Transform root,int minimum,string label){
   Check(root&&root.gameObject.activeInHierarchy,"native illustration root exists: "+label);
   int visible=0;
   foreach(var graphic in root.GetComponentsInChildren<Graphic>()){
    if(!(graphic is SoftShape)&&!(graphic is CapyContour))continue;
    if(!graphic.isActiveAndEnabled||graphic.color.a<=0||graphic.rectTransform.rect.width<=0||graphic.rectTransform.rect.height<=0)continue;
    Check(!graphic.raycastTarget,"native illustration cannot intercept input: "+label+"/"+graphic.name);visible++;
   }
   Check(visible>=minimum,"native illustration retains meaningful geometry: "+label+" ("+visible+")");
  }
  static void CheckNoGlossyArt(Transform root,string label){
   foreach(var picture in root.GetComponentsInChildren<Image>()){
    string n=picture.name;
    bool replaced=n.StartsWith("Garden artwork ")||n=="Painted leaf icon"||n=="Painted mandarin icon"||n=="Painted ball icon"||n=="Painted brush icon";
    Check(!replaced,"restored cartoon art has no glossy replacement: "+label+"/"+n);
   }
  }
  static IEnumerator CheckGardenFinish(CapyIdleGame g){
   int previousFoodPage=(int)typeof(CapyIdleGame).GetField("foodPage",F).GetValue(g);
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=6,coins=1234,sound=false});typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    foreach(var item in IdleCatalog.Items){if(item.slot!=2)continue;m.S.equipped[2]=item.id;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     Check(g.GetComponentInChildren<GardenAmbientMotion>()!=null,"scenery has bounded ambient controller "+item.id);
     CheckNoGlossyArt(g.transform,"scene "+item.id);
     CheckNativeArt(ArtRoot(g.transform,"Detailed scenery "+Mathf.Clamp(item.art,0,14)),10,"scene "+item.id);
     var pose=g.transform.Find("Idle Canvas/Idle board/Home/Capybara/Illustrated pose");Check(pose!=null,"protected pose still present "+item.id);
     foreach(var shape in pose.GetComponentsInChildren<SoftShape>(true))Check(!shape.polished,"environment finish never reaches capy or outfit "+item.id);
     Check(pose.GetComponentInChildren<GardenContourLight>(true)==null,"illustrated shading never modifies character contours "+item.id);
     Frame("finish-scene-"+item.id);
    }
    m.S.equipped[2]="pond";g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    var pool=(RectTransform)typeof(CapyIdleGame).GetField("finishLayer",F).GetValue(g);int before=pool.GetComponentsInChildren<Transform>(true).Length;string save=JsonUtility.ToJson(m.S);
    for(int i=0;i<200;i++)Call(g,"TouchFeedback",3d);
    Check(pool.GetComponentsInChildren<Transform>(true).Length==before,"fast tap feedback reuses fixed pool");
    Check(JsonUtility.ToJson(m.S)==save,"visual feedback never changes economy or save data");
    foreach(var graphic in pool.GetComponentsInChildren<Graphic>(true))Check(!graphic.raycastTarget,"feedback cannot capture taps");
    Call(g,"UpdateFinishVfx",.12f);yield return new WaitForEndOfFrame();Frame("finish-tap-reward");
    float age=(float)typeof(CapyIdleGame).GetField("lastRewardAge",F).GetValue(g);Call(g,"PlatformPause",true);Call(g,"UpdateFinishVfx",1f);
    Check((float)typeof(CapyIdleGame).GetField("lastRewardAge",F).GetValue(g)==age,"SDK pause freezes reward feedback");Call(g,"PlatformPause",false);
    m.S.calm=true;Call(g,"UpdateFinishVfx",.1f);Check(g.DecorativeReduced,"calm mode reduces decorative effects");m.S.calm=false;
    for(int i=0;i<15;i++)Call(g,"UpdateFinishVfx",.1f);
    foreach(var text in pool.GetComponentsInChildren<Text>(true))Check(!text.gameObject.activeSelf,"reward labels expire and return to pool");
    g.Show("care");yield return new WaitForEndOfFrame();Frame("finish-care-icons");
    foreach(string page in new[]{"food","activities","shop","daily","quests","surprises","settings"}){
     g.Show(page);yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Frame("cartoon-"+page);
     CheckNoGlossyArt(g.transform,page);
     if(page=="surprises"){
      var tree=ArtRoot(g.transform,"Mandarin surprise tree");CheckNativeArt(tree,30,"surprise tree");
      for(int n=0;n<7;n++)CheckNativeArt(tree.Find("Leaf crown "+n),2,"tree crown "+n);
     }
    }
    for(int menu=0;menu<3;menu++){
     typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,menu);g.Show("food");yield return new WaitForEndOfFrame();
     CheckNoGlossyArt(g.transform,"food menu "+menu);
     for(int row=0;row<4;row++){int food=menu*4+row;CheckNativeArt(ArtRoot(g.transform,"Food illustration "+food),10,"food "+food);}
     Frame("cartoon-food-"+menu);
    }
   }finally{typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,previousFoodPage);typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");}
  }
  static IEnumerator CheckCardClarity(CapyIdleGame g){
   var previous=g.Model;var m=new IdleModel(new IdleSave{starter=true,growth=6,coins=1234,sound=false});
   var names=new[]{"category","pageIndex","selected","ownedOnly","offlineAward"};var old=new object[names.Length];
   for(int i=0;i<names.Length;i++)old[i]=typeof(CapyIdleGame).GetField(names[i],F).GetValue(g);
   typeof(CapyIdleGame).GetProperty("Model").SetValue(g,m);
   try{
    foreach(double amount in new[]{0d,260d,9999d,999999d,1e12}){
     m.S.coins=amount;m.M.balance=amount>int.MaxValue?int.MaxValue:(int)amount;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     Typography(g,"card-wallet-"+amount);
     foreach(string name in new[]{"Mandarin shop plus","Mandarin wallet","Settings"}){
      var b=Find(g,name);Check(Clickable(b),"card wallet raycast "+name);
      var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);
      Check((corners[1]-corners[0]).magnitude>=44&&(corners[2]-corners[1]).magnitude>=44,"card wallet actual touch size "+name);
     }
     if(amount==260)Frame("card-wallet");
     typeof(CapyIdleGame).GetField("offlineAward",F).SetValue(g,amount);g.Show("offline");yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();
     Typography(g,"card-offline-"+amount);if(amount==260)Frame("card-offline");
     Call(g,"Suspend",false);Call(g,"PlatformPause",false);
     double coins=m.S.coins;var button=Find(g,"Continue idle");button.onClick.Invoke();button.onClick.Invoke();
     Check(m.S.coins==coins&&g.Page=="home","continue cannot grant offline coins again (page="+g.Page+", before="+coins+", after="+m.S.coins+", paused="+g.PresentationPaused+")");
     g.Show("offline");Check(m.S.coins==coins,"reopening offline cannot grant coins");
    }
    g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    Check(!ArtRoot(g.transform,"Mandarin caption"),"wallet has no shop caption");
    var plus=Find(g,"Mandarin shop plus");var hit=(RectTransform)plus.transform;
    var face=(RectTransform)plus.transform.Find("Compact plus visual");
    Check(face!=null,"plus has separate animated face");
    var press=plus.GetComponent<MandarinPlusPress>();Check(press!=null,"plus pointer feedback attached");
    var hitPosition=hit.anchoredPosition;var hitSize=hit.sizeDelta;var facePosition=face.anchoredPosition;
    var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=23};
    press.OnPointerDown(pointer);g.Tweens.Step(.11f);
    Check(face.localScale.x<.91f&&face.anchoredPosition.y<facePosition.y,"plus visibly presses down");
    Check(hit.localScale==Vector3.one&&hit.anchoredPosition==hitPosition&&hit.sizeDelta==hitSize&&Clickable(plus),"plus hitbox stays fixed during press");
    yield return new WaitForEndOfFrame();Frame("card-plus-pressed");
    press.OnPointerUp(pointer);g.Tweens.Step(.2f);
    Check(face.localScale==Vector3.one&&face.anchoredPosition==facePosition,"plus release restores pose");
    for(int i=0;i<100;i++){press.OnPointerDown(pointer);press.OnPointerUp(pointer);}g.Tweens.Step(.2f);
    Check(face.localScale==Vector3.one&&face.anchoredPosition==facePosition,"rapid plus gestures have no drift");
    press.OnPointerDown(pointer);press.OnPointerExit(pointer);g.Tweens.Step(.2f);
    Check(face.localScale==Vector3.one,"leaving plus cancels feedback");
    press.OnPointerDown(pointer);Call(g,"PlatformPause",true);yield return new WaitForEndOfFrame();
    Check(face.localScale==Vector3.one&&face.anchoredPosition==facePosition,"pause cancels plus press");Call(g,"PlatformPause",false);
    m.S.calm=true;press.OnPointerDown(pointer);g.Tweens.Step(.2f);
    Check(face.localScale==Vector3.one&&face.GetComponent<CanvasGroup>().alpha<1,"calm plus uses static feedback");press.OnPointerUp(pointer);m.S.calm=false;
    press.OnPointerDown(pointer);plus.gameObject.SetActive(false);
    Check(face.localScale==Vector3.one&&face.anchoredPosition==facePosition,"disabled plus restores visual");plus.gameObject.SetActive(true);
    var ordinary=Array.Find(IdleCatalog.Items,i=>i.IsSet&&!i.TreeOnly&&i.RewardDay==0&&i.price>0);
    foreach(var item in new[]{ordinary,Array.Find(IdleCatalog.Items,i=>i.IsSet&&i.TreeOnly),IdleCatalog.Find("set_dawn")}){
     Check(item!=null,"card state fixture exists");typeof(CapyIdleGame).GetField("category",F).SetValue(g,item.slot);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,item.id);typeof(CapyIdleGame).GetField("ownedOnly",F).SetValue(g,false);
     m.S.owned.Remove(item.id);m.S.equipped[item.slot]="";m.S.coins=1e8;m.S.growth=150000;
     g.Show("shop");yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();Typography(g,"card-unowned-"+item.id);
     Check(Find(g,"Equip or buy").interactable==(!item.TreeOnly&&item.RewardDay==0),"exclusive cards cannot be bought");Frame("card-unowned-"+item.id);
     m.S.owned.Add(item.id);g.Show("shop");yield return new WaitForEndOfFrame();Check(Find(g,"Equip or buy").interactable,"owned card can be equipped");Typography(g,"card-owned-"+item.id);
     m.S.equipped[item.slot]=item.id;g.Show("shop");yield return new WaitForEndOfFrame();Check(!Find(g,"Equip or buy").interactable,"worn card does not offer a second purchase");Typography(g,"card-worn-"+item.id);Frame("card-worn-"+item.id);
    }
    for(int age=1;age<=5;age++){
     m.S.growth=IdleModel.Growth[age-1];typeof(CapyIdleGame).GetField("category",F).SetValue(g,0);typeof(CapyIdleGame).GetField("pageIndex",F).SetValue(g,0);
     g.Show("shop");yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();
     foreach(var b in g.GetComponentsInChildren<Button>()){
      if(!b.name.StartsWith("Shop item "))continue;
      var clip=ArtRoot(b.transform,"Thumbnail round clip") as RectTransform;Check(clip&&clip.GetComponent<Mask>(),"thumbnail has circular mask");
      var sprite=ArtRoot(clip,"Life sprite") as RectTransform;Check(sprite!=null,"outfit thumbnail has portrait");
      var corners=new Vector3[4];sprite.GetWorldCorners(corners);
      foreach(var corner in corners){var local=(Vector2)clip.InverseTransformPoint(corner);Check(Vector2.Distance(local,clip.rect.center)<=26,"entire outfit frame fits circle at age "+age);}
      Check(Clickable(b),"thumbnail does not intercept card input");
     }
     Frame("card-thumbnails-age-"+age);
    }
    typeof(CapyIdleGame).GetField("category",F).SetValue(g,2);typeof(CapyIdleGame).GetField("pageIndex",F).SetValue(g,0);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,"");
    g.Show("shop");yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();Typography(g,"card-scene-thumbnails");Frame("card-scene-thumbnails");
    foreach(var b in g.GetComponentsInChildren<Button>())if(b.name.StartsWith("Shop item ")){
     var clip=ArtRoot(b.transform,"Thumbnail round clip");Check(clip&&clip.GetComponent<Mask>()&&ArtRoot(clip,"Scene thumbnail scale"),"scene fills a circular masked aperture");Check(Clickable(b),"scene card input remains accessible");
    }
   }finally{
    Call(g,"PlatformPause",false);
    for(int i=0;i<names.Length;i++)typeof(CapyIdleGame).GetField(names[i],F).SetValue(g,old[i]);
    typeof(CapyIdleGame).GetProperty("Model").SetValue(g,previous);g.Show("home");
   }
  }
  static IEnumerator Run(CapyIdleGame g){
   running=true;checks=0;renderErrors=0;Application.logMessageReceived+=TrackRenderError;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-idle-checks.txt","RUNNING");File.WriteAllText("Logs/Capy67-layout.txt","");
   bool careOnly=File.Exists("Temp/Capy67-care-only");if(careOnly)File.Delete("Temp/Capy67-care-only");
   capture=File.Exists("Temp/Capy67-garden-capture");if(capture){File.Delete("Temp/Capy67-garden-capture");Directory.CreateDirectory("Logs/GardenReview");}
   string[] keys={CapyIdleGame.SaveKey,CapyIdleGame.SaveKey+".backup","capy67.save.v1","capy67.progress.v2"};var saves=new string[keys.Length];for(int n=0;n<keys.Length;n++)saves[n]=PlayerPrefs.GetString(keys[n],"");
   SessionState.SetBool("CapyIdle.validation",true);EditorApplication.LockReloadAssemblies();
   int originalFoodPage=(int)typeof(CapyIdleGame).GetField("foodPage",F).GetValue(g);typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,0);
   var original=g.Model;var model=new IdleModel();typeof(CapyIdleGame).GetProperty("Model").SetValue(g,model);model.S.sound=false;
   try{
    if(File.Exists("Temp/Capy67-birth-gifts-only")){
     File.Delete("Temp/Capy67-birth-gifts-only");
     yield return CheckBirthIntro(g);
     yield return CheckGiftLidPresentation(g);
     yield return CheckDesktopLayout(g);
     for(int n=0;n<keys.Length;n++)Check(PlayerPrefs.GetString(keys[n],"")==saves[n],"birth and gift tests preserve player key "+n);
     Check(new FileInfo("Logs/Capy67-layout.txt").Length==0,"birth and gift text fits");
     Check(renderErrors==0,"birth and gifts have no runtime rendering errors");
     File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+" birth and gift assertions at "+Screen.width+"x"+Screen.height+"; isolated profile restored.\n");
     yield break;
    }
    if(File.Exists("Temp/Capy67-desktop-resize-only")){
     File.Delete("Temp/Capy67-desktop-resize-only");
     var sizes=new HashSet<string>();float until=Time.realtimeSinceStartup+120;
     while(sizes.Count<3&&Time.realtimeSinceStartup<until){
      string size=Screen.width+"x"+Screen.height;
      if(!sizes.Contains(size)){
       yield return new WaitForSecondsRealtime(.4f);
       yield return CheckDesktopLayout(g);sizes.Add(size);
       yield return new WaitForEndOfFrame();Frame("resize-"+size);
      }
      yield return null;
     }
     Check(sizes.Count==3,"runtime resize observed three distinct viewport sizes");
     for(int n=0;n<keys.Length;n++)Check(PlayerPrefs.GetString(keys[n],"")==saves[n],"resize preserves player profile "+n);
     Check(new FileInfo("Logs/Capy67-layout.txt").Length==0,"resized text fits");Check(renderErrors==0,"runtime resize has no rendering errors");
     File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+" runtime resize assertions across "+string.Join(", ",sizes)+"; isolated profile restored.\n");yield break;
    }
    if(File.Exists("Temp/Capy67-cartoon-only")){
     File.Delete("Temp/Capy67-cartoon-only");
     model.S.starter=true;model.S.growth=6;model.S.coins=10000;
     yield return CheckGardenFinish(g);
     yield return CheckCardClarity(g);
     yield return CheckActivitiesUI(g);
     yield return CheckPlayroomUI(g);
     foreach(string page in new[]{"home","offline","growth","care","food","activities","shop","daily","quests","surprises","settings"}){
      g.Show(page);yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
      Typography(g,page);Frame("viewport-"+page);
      foreach(var button in g.GetComponentsInChildren<Button>()){
       var rect=(RectTransform)button.transform;
       Check(rect.rect.width>=60&&rect.rect.height>=60,"viewport touch size "+button.name);
       if(button.interactable)Check(Clickable(button),"viewport raycast "+button.name);
      }
     }
     Check(PlayerPrefs.GetString(keys[0],"")==saves[0]&&PlayerPrefs.GetString(keys[1],"")==saves[1],"viewport check preserves player profile");
     Check(new FileInfo("Logs/Capy67-layout.txt").Length==0,"viewport labels fit");
     Check(renderErrors==0,"viewport has no rendering errors");
     File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+" cartoon viewport assertions at "+Screen.width+"x"+Screen.height+"; isolated profile restored.\n");
     yield break;
    }
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
    Check(daily.S.stars==0&&daily.S.coins==11650&&daily.S.premiumFood==1&&daily.Purchase("set_dawn")&&daily.Purchase("firefly_garden"),"seventh gift keeps old bonuses and equips new rewards");
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
    model.S.coins=100;g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Bed upgrade").onClick.Invoke();PayCoins(g);Check(model.S.bed==1&&model.Passive>0,"buy bed UI");
    model.S.growth=360;model.S.coins=10000;model.S.pity=66;model.Tick(60);g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Food 2").onClick.Invoke();PayCoins(g);Check(model.S.forms.Count==1&&g.Page=="home","feed evolves and returns home");
    g.Show("surprise-results");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Find(g,"Cycle talent")==null,"no talent button on results");Check(model.SelectForm(""),"existing talent remains reversible in model");
    foreach(string page in new[]{"home","growth","food","shop","album","quests","daily","settings","offline","visitor"}){g.Show(page);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(page=="home"||Clickable(Find(g,"Back")),"navigation "+page);foreach(var button in g.GetComponentsInChildren<Button>()){var r=(RectTransform)button.transform;Check(r.rect.width>=60&&r.rect.height>=60,"touch area "+button.name);}Typography(g,page);Frame(page);}
    model.S.feeds=3;g.Show("visitor");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Welcome visitor")),"visitor gift raycast");Find(g,"Welcome visitor").onClick.Invoke();Check(!model.CanVisit,"visitor gift consumed");
    g.Show("home");model.Tick(91);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(g.IsSleeping,"safe idle sleep");g.Care();Check(!g.IsSleeping,"care wakes capybara");
    model.S.coins=1e8;model.S.growth=150000;
    model.Grant("firefly_garden");model.Grant("set_dawn");foreach(var exclusive in IdleCatalog.Items)if(exclusive.TreeOnly)model.Grant(exclusive.id);
    foreach(var item in IdleCatalog.Items){if(!CapyCompleteSets.Visible(item))continue;typeof(CapyIdleGame).GetField("category",F).SetValue(g,item.slot);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,item.id);g.Show("shop");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Typography(g,"item-"+item.id);Frame("item-"+item.id);var buy=Find(g,"Equip or buy");Check(Clickable(buy),"item button "+item.id);buy.onClick.Invoke();if(Find(g,"Pay with coins"))PayCoins(g);Check(model.S.equipped[item.slot]==item.id,"equip UI "+item.id);}
    g.Show("daily");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Claim daily").onClick.Invoke();int count=model.S.dailyCount;Check(g.Page=="daily-reward","calendar reward presentation");Find(g,"Daily reward return").onClick.Invoke();Find(g,"Claim daily").onClick.Invoke();Check(model.S.dailyCount==count&&count==1,"daily UI once");
    var ownedForms=new List<string>(model.S.forms);foreach(var f in IdleModel.Forms)if(!model.S.forms.Contains(f.id))model.S.forms.Add(f.id);
    g.Show("album");yield return new WaitForEndOfFrame();Typography(g,"album-all");Frame("album-all");model.S.forms=ownedForms;
    g.Save();var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.PersistenceKey));Check(saved.owned.Count==103&&saved.forms.Count==1,"saved idle profile");Check(PlayerPrefs.GetString(keys[2],"")==saves[2]&&PlayerPrefs.GetString(keys[3],"")==saves[3],"legacy profile untouched");
    Call(g,"OnDisable");typeof(CapyIdleGame).GetProperty("Model").SetValue(g,null);Call(g,"OnEnable");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);model=g.Model;Check(model!=null&&model.S.owned.Count==103,"managed model restored after editor reload");g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Care hold")),"rebuilt UI accepts input after reload");Check(g.GetComponents<AudioSource>().Length==1,"editor reload reuses audio source");
    g.Show("home");Call(g,"PlatformPause",true);coins=model.S.coins;g.Care();Check(model.S.coins==coins,"platform pause blocks input");
    model.S.lastUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-100;Call(g,"Suspend",true);Call(g,"Suspend",false);Check(model.S.coins==coins,"platform pause excludes offline credit");Call(g,"PlatformPause",false);
    }
    yield return CheckProductUI(g);
    yield return CheckSectionGuides(g);
    yield return CheckFirstSessionUI(g);
    yield return CheckPersonalityUI(g);
    yield return CheckEconomyUI(g);
    yield return CheckActivitiesUI(g);
    yield return CheckPlayroomUI(g);
    yield return CheckMandarinsUI(g);
    yield return CheckWelcomeUI(g);
    yield return CheckBirthIntro(g);
    yield return CheckGiftLidPresentation(g);
    yield return CheckCareGestures(g);
    yield return CheckCareUI(g);
    yield return CheckTapUI(g);
    yield return CheckSurpriseUI(g);
    yield return CheckGardenFinish(g);
    yield return CheckCardClarity(g);
    Check(PlayerPrefs.GetString(keys[0],"")==saves[0]&&PlayerPrefs.GetString(keys[1],"")==saves[1],"player save and backup remain unchanged throughout validation");
    Check(new FileInfo("Logs/Capy67-layout.txt").Length==0,"all visible labels fit their text rectangles");
    Check(renderErrors==0,"no runtime or rendering errors during fitting and purchases");
    File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+(careOnly?" Unity care / DOTween assertions; care UI, food, pause, reduced motion, save and lifecycle.":" Unity idle integration assertions; all pages, raycasts, touch sizes, growth, feeding, evolution, 103 catalog items including 42 complete sets, daily, save and platform pause.")+" Profile restored.\n");Debug.Log("CAPY IDLE PASS: "+checks);
   }finally{
    Application.logMessageReceived-=TrackRenderError;
    try{
     typeof(CapyIdleGame).GetProperty("Model").SetValue(g,original);
     typeof(CapyIdleGame).GetField("foodPage",F).SetValue(g,originalFoodPage);
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













