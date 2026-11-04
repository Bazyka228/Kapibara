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
namespace Capybara67.Editor {
 [InitializeOnLoad] public static class CapyIdleValidation {
  static bool running,capture;static int checks;const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
  static int renderErrors;
  static void TrackRenderError(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)renderErrors++;}
  static CapyIdleValidation(){EditorApplication.update+=Request;}
  static void Request(){if(!File.Exists("Temp/Capy67-idle-checks")||running||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   if(!EditorApplication.isPlaying){if(EditorApplication.isPlayingOrWillChangePlaymode)return;CapyIdleSetup.Open();SessionState.SetBool("CapyIdle.auto",true);SessionState.SetBool("CapyIdle.background",Application.runInBackground);Application.runInBackground=true;EditorApplication.EnterPlaymode();return;}
   var game=UnityEngine.Object.FindAnyObjectByType<CapyIdleGame>();if(!game||game.Model==null)return;File.Delete("Temp/Capy67-idle-checks");game.StartCoroutine(Run(game));
  }
  static void Check(bool ok,string label){if(!ok){File.WriteAllText("Logs/Capy67-idle-checks.txt","FAIL: "+label+" after "+checks);throw new Exception("Idle integration: "+label);}checks++;}
  static void Call(CapyIdleGame g,string method,params object[] args)=>typeof(CapyIdleGame).GetMethod(method,F).Invoke(g,args);
  static Button Find(CapyIdleGame g,string name){foreach(var b in g.GetComponentsInChildren<Button>(true))if(b.name==name&&b.gameObject.activeInHierarchy)return b;return null;}
  static bool Clickable(Button b){if(!b)return false;Canvas.ForceUpdateCanvases();var r=(RectTransform)b.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);return hits.Count>0&&hits[0].gameObject==b.gameObject;}
  // Export only the game's rendered frame, never the desktop. Explicit opt-in for visual QA.
  static void Frame(string name){if(!capture)return;var texture=ScreenCapture.CaptureScreenshotAsTexture();try{File.WriteAllBytes("Logs/GardenReview/"+name+".png",texture.EncodeToPNG());}finally{UnityEngine.Object.Destroy(texture);}}
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
    if(capture&&(label=="base-2"||label=="base-sleep-2"||label=="duck-apron-2")){
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
    Check(restored.S.form=="chill"&&restored.S.owned.Count==4&&restored.S.equipped[0]=="cap"&&restored.S.equipped[1]=="scarf","old form and outfit retained "+i);
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
  static IEnumerator Run(CapyIdleGame g){
   running=true;checks=0;renderErrors=0;Application.logMessageReceived+=TrackRenderError;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-idle-checks.txt","RUNNING");
   capture=File.Exists("Temp/Capy67-garden-capture");if(capture){File.Delete("Temp/Capy67-garden-capture");Directory.CreateDirectory("Logs/GardenReview");}
   string[] keys={CapyIdleGame.SaveKey,CapyIdleGame.SaveKey+".backup","capy67.save.v1","capy67.progress.v2"};var saves=new string[keys.Length];for(int n=0;n<keys.Length;n++)saves[n]=PlayerPrefs.GetString(keys[n],"");
   var original=g.Model;var model=new IdleModel();typeof(CapyIdleGame).GetProperty("Model").SetValue(g,model);model.S.sound=false;
   try{
    CheckFiveStageProgression();
    g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Care hold")),"home care raycast");Check(Clickable(Find(g,"Care on character")),"character raycast");g.Care();Check(model.S.cares==1,"care earns on home");double coins=model.S.coins;g.Care();Check(model.S.coins==coins,"spam input limit");
    Check(Clickable(Find(g,"Growth stages")),"growth map raycast");double growthBefore=model.S.growth;int ownedBefore=model.S.owned.Count;Find(g,"Growth stages").onClick.Invoke();Check(g.Page=="growth","growth map opens");Check(g.transform.Find("Idle Canvas/Idle board/Pages/Age preview 5")!=null,"all ages previewed through guardian");Check(g.transform.Find("Idle Canvas/Idle board/Pages/Age preview 6")==null,"removed ages absent from growth page");Check(model.S.growth==growthBefore&&model.S.owned.Count==ownedBefore,"age preview does not unlock progress");g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    var avatar=(RectTransform)typeof(CapyIdleGame).GetField("hero",F).GetValue(g);Check(avatar.anchoredPosition.y<0&&avatar.anchoredPosition.y>-400,"avatar inside portrait area");
    foreach(double growth in IdleModel.Growth){model.S.growth=growth;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);var painted=avatar.Find("Illustrated pose/Life sprite").GetComponent<Image>();Check(painted.sprite!=null&&!painted.canvasRenderer.cull,"visible reference sprite at stage "+model.Stage);Check(avatar.Find("Head")==null&&avatar.Find("Body")==null,"legacy geometry removed at stage "+model.Stage);Check(painted.sprite.name=="Capy life stage "+new[]{1,2,3,6,9}[model.Stage-1],"correct surviving portrait at stage "+model.Stage);Frame("stage-"+model.Stage);}model.S.growth=0;
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
    model.S.activeSeconds=146;Call(g,"AnimateHero",.1f);
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
     foreach(var fitting in IdleCatalog.Items){if(fitting.slot>1)continue;model.S.equipped[0]="";model.S.equipped[1]="";model.S.equipped[fitting.slot]=fitting.id;g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
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
      yield return CheckEyePixels(g,portrait,outfit.resource.Substring("Outfits/".Length)+"-"+age);
      var foot=avatar.InverseTransformPoint(pose.TransformPoint(new Vector3(120,-244,0)));
      Check(Vector2.Distance(foot,new Vector2(120,-244))<.01f&&pose.localScale.x>0,"combined outfit breath anchor and facing "+age);
      if(age==1||age==3||age==5)Frame("combo-blink-"+outfit.head+"-"+outfit.clothes+"-"+age);
     }
     if(capture){Call(g,"ShowOutfitReview",outfit.head,outfit.clothes);yield return new WaitForEndOfFrame();Frame("combo-"+(outfit.head==""?"none":outfit.head)+"-"+(outfit.clothes==""?"none":outfit.clothes));Call(g,"CloseAccessoryReview");}
    }
    IdleItem missingHat=null,missingClothes=null;
    foreach(var hat in IdleCatalog.Items){if(hat.slot!=0)continue;foreach(var clothes in IdleCatalog.Items){if(clothes.slot!=1||CapyOutfitCatalog.Find(hat.id,clothes.id,"classic",-1)!=null)continue;missingHat=hat;missingClothes=clothes;break;}if(missingHat!=null)break;}
    if(missingHat!=null){
     model.S.equipped=new[]{missingHat.id,missingClothes.id,"moon","","","classic"};g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     Check(avatar.GetComponentInChildren<CapyBakedOutfit>()==null&&avatar.Find("Illustrated pose/Painted accessory "+missingHat.id)&&avatar.Find("Illustrated pose/Painted accessory "+missingClothes.id),"unfinished pair retains both chosen items without substituting another outfit");
    }
    model.S.equipped=new[]{"cap","scarf","moon","stars","","classic"};g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    Check(avatar.GetComponentInChildren<CapyBakedOutfit>()&&avatar.childCount>2,"effects preserve combined outfit after atlas unload and reload");
    var equippedBeforePreview=string.Join("|",model.S.equipped);model.S.growth=0;
    typeof(CapyIdleGame).GetField("category",F).SetValue(g,1);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,"sweater");g.Show("shop");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    bool combinedPreview=false;foreach(var baked in g.GetComponentsInChildren<CapyBakedOutfit>())if(baked.headId=="cap"&&baked.outfitId=="sweater")combinedPreview=true;
    Check((CapyOutfitCatalog.Find("cap","sweater","classic",-1)==null||combinedPreview)&&string.Join("|",model.S.equipped)==equippedBeforePreview,"shop preview changes clothes while retaining hat and equipped save");
    model.S.equipped=oldEquipment;model.S.growth=0;
    if(capture){
     foreach(var fitting in IdleCatalog.Items){if(fitting.slot>1)continue;Call(g,"ShowAccessoryReview",fitting.id);yield return new WaitForEndOfFrame();Frame("review-"+fitting.id);}Call(g,"CloseAccessoryReview");
     model.S.growth=IdleModel.Growth[1];typeof(CapyIdleGame).GetField("category",F).SetValue(g,0);
     foreach(string hat in new[]{"crown","beret"}){model.S.equipped[0]=hat;model.S.equipped[1]=hat=="beret"?"cape":"";typeof(CapyIdleGame).GetField("selected",F).SetValue(g,hat);g.Show("shop");yield return new WaitForEndOfFrame();Frame("example-baby-"+hat);}
     model.S.equipped=(string[])oldEquipment.Clone();model.S.growth=0;
    }
    g.Show("shop");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
    for(int slot=0;slot<2;slot++){
     Find(g,"Category "+slot).onClick.Invoke();yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     for(int page=0;page<(slot==0?6:5);page++){Find(g,"Next").onClick.Invoke();yield return new WaitForEndOfFrame();Call(g,"Suspend",false);}
     var last=Find(g,"Shop item "+(slot==0?"leaftiara":"picnicpack"));Check(Clickable(last),"new last accessory reachable through pagination "+slot);last.onClick.Invoke();yield return new WaitForEndOfFrame();Call(g,"Suspend",false);
     Check((string)typeof(CapyIdleGame).GetField("selected",F).GetValue(g)==(slot==0?"leaftiara":"picnicpack"),"last accessory preview selection "+slot);Frame("last-page-"+slot);
    }
    g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Starter bowl")),"starter raycast");g.Care();Check(model.S.coins==coins,"modal blocks care");Find(g,"Starter bowl").onClick.Invoke();Check(model.Stage==2&&g.Page=="home","starter growth transition");
    model.S.coins=100;g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Bed upgrade").onClick.Invoke();Check(model.S.bed==1&&model.Passive>0,"buy bed UI");
    model.S.growth=360;model.S.coins=10000;model.S.pity=66;model.Tick(60);g.Show("food");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Food 2").onClick.Invoke();Check(model.S.forms.Count==1&&g.Page=="home","feed evolves and returns home");
    g.Show("album");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Classic form")),"album revert raycast");Find(g,"Classic form").onClick.Invoke();Check(model.S.form=="","form reversible");
    foreach(string page in new[]{"home","growth","food","shop","album","quests","daily","settings","offline","visitor"}){g.Show(page);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(page=="home"||Clickable(Find(g,"Back")),"navigation "+page);foreach(var button in g.GetComponentsInChildren<Button>()){var r=(RectTransform)button.transform;Check(r.rect.width>=60&&r.rect.height>=60,"touch area "+button.name);}Frame(page);}
    model.S.feeds=3;g.Show("visitor");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Welcome visitor")),"visitor gift raycast");Find(g,"Welcome visitor").onClick.Invoke();Check(!model.CanVisit,"visitor gift consumed");
    g.Show("home");model.Tick(46);yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(g.IsSleeping,"safe idle sleep");g.Care();Check(!g.IsSleeping,"care wakes capybara");
    model.S.coins=1e8;model.S.growth=150000;model.S.stars=500;
    foreach(var item in IdleCatalog.Items){typeof(CapyIdleGame).GetField("category",F).SetValue(g,item.slot);typeof(CapyIdleGame).GetField("selected",F).SetValue(g,item.id);g.Show("shop");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Frame("item-"+item.id);var buy=Find(g,"Equip or buy");Check(Clickable(buy),"item button "+item.id);buy.onClick.Invoke();Check(model.S.equipped[item.slot]==item.id,"equip UI "+item.id);}
    g.Show("daily");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Find(g,"Claim daily").onClick.Invoke();int count=model.S.dailyCount;Find(g,"Claim daily").onClick.Invoke();Check(model.S.dailyCount==count&&count==1,"daily UI once");
    g.Save();var saved=JsonUtility.FromJson<IdleSave>(PlayerPrefs.GetString(CapyIdleGame.SaveKey));Check(saved.owned.Count==77&&saved.forms.Count==1,"saved idle profile");Check(PlayerPrefs.GetString(keys[2],"")==saves[2]&&PlayerPrefs.GetString(keys[3],"")==saves[3],"legacy profile untouched");
    Call(g,"OnDisable");typeof(CapyIdleGame).GetProperty("Model").SetValue(g,null);Call(g,"OnEnable");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);model=g.Model;Check(model!=null&&model.S.owned.Count==77,"managed model restored after editor reload");g.Show("home");yield return new WaitForEndOfFrame();Call(g,"Suspend",false);Check(Clickable(Find(g,"Care hold")),"rebuilt UI accepts input after reload");Check(g.GetComponents<AudioSource>().Length==1,"editor reload reuses audio source");
    g.Show("home");Call(g,"PlatformPause",true);coins=model.S.coins;g.Care();Check(model.S.coins==coins,"platform pause blocks input");
    model.S.lastUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-100;Call(g,"Suspend",true);Call(g,"Suspend",false);Check(model.S.coins==coins,"platform pause excludes offline credit");Call(g,"PlatformPause",false);
    Check(renderErrors==0,"no runtime or rendering errors during fitting and purchases");
    File.WriteAllText("Logs/Capy67-idle-checks.txt","PASS: "+checks+" Unity idle integration assertions; all pages, raycasts, touch sizes, growth, feeding, evolution, 77 purchases, daily, save and platform pause. Profile restored.\n");Debug.Log("CAPY IDLE PASS: "+checks);
   }finally{
    Application.logMessageReceived-=TrackRenderError;
    Call(g,"CloseAccessoryReview");
    typeof(CapyIdleGame).GetProperty("Model").SetValue(g,original);g.Show("home");
    for(int n=0;n<keys.Length;n++){if(saves[n]=="")PlayerPrefs.DeleteKey(keys[n]);else PlayerPrefs.SetString(keys[n],saves[n]);}PlayerPrefs.Save();running=false;
    if(SessionState.GetBool("CapyIdle.auto",false)){SessionState.SetBool("CapyIdle.auto",false);Application.runInBackground=SessionState.GetBool("CapyIdle.background",false);EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();}
   }
  }
 }
}








