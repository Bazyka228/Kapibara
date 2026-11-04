using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
#if PLUGIN_YG_2
using YG;
#endif
namespace Capybara67.Idle
{
    public sealed partial class CapyIdleGame : MonoBehaviour
    {
        public const string SaveKey="capy67.idle.v1";
        public static string PersistenceKey {
            get {
#if UNITY_EDITOR
                if(UnityEditor.SessionState.GetBool("CapyIdle.validation",false))return SaveKey+".validation";
#endif
                return SaveKey;
            }
        }
        public static string LegacyPersistenceKey(string key) {
#if UNITY_EDITOR
            if(UnityEditor.SessionState.GetBool("CapyIdle.validation",false))return key+".validation";
#endif
            return key;
        }
        public IdleModel Model {get;private set;}
        public string Page {get;private set;}="home";
        RectTransform board,home,panel,hero,decor;
        Text coinText,stageText,growthText,rateText,notice,petStageText;
        SoftShape growthFill;
        IdleHold careHold,heroHold;
        CapyTweenDriver tweens;
        Tween pageTween;
        CanvasGroup pageOpacity;
        public bool PresentationPaused=>suspended||platformPaused||adPending;
        public CapyTweenDriver Tweens=>tweens;
        AudioSource audioSource;AudioClip careClip,feedClip,evolveClip;
        float refreshAge,saveAge,reactionAge=10,noticeAge;
        double offlineAward;bool suspended,platformPaused,loaded;
        int category,pageIndex,foodPage;string selected="set_gardener",toast="Погладь капибару, затем дай первую миску";
        Font regular,bold;
        readonly List<Action> liveLabels=new List<Action>();
        static double Utc=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        static string Date=>DateTime.UtcNow.ToString("yyyy-MM-dd");
        public bool InputAllowed=>loaded&&Model!=null&&!PresentationPaused&&!currencyChoice&&!sectionHelp&&Page=="home";
        void OnEnable(){
#if UNITY_EDITOR
            if(!Application.isPlaying)return;
            // Editor domain reload keeps Unity objects but drops the managed model and delegates.
            if(loaded&&Model==null){
                loaded=false;suspended=false;platformPaused=false;liveLabels.Clear();UnbindPlatform();
                var previous=transform.Find("Idle Canvas");if(previous){previous.gameObject.SetActive(false);Destroy(previous.gameObject);}
                Start();
            }
#endif
            if(loaded&&Model!=null)BindAutoAds();
        }
        void OnDisable(){PauseAudio();UnbindAutoAds();CancelCareGestures();if(loaded&&Model!=null)Save();}
        void Start(){
            regular=Resources.Load<Font>("Fonts/Nunito-SemiBold")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");bold=Resources.Load<Font>("Fonts/Nunito-ExtraBold")??regular;
            Load();InitializeAudio();
            tweens=GetComponent<CapyTweenDriver>();if(!tweens)tweens=gameObject.AddComponent<CapyTweenDriver>();
            tweens.Initialize(this);
            BuildUI();loaded=true;if(Model.S.starter)toast="С возвращением! Капибара рада тебя видеть";offlineAward=Model.Arrive(Utc);Model.RefreshDay(Date);Save();
            welcomeOfflinePending=offlineAward>=1||Model.ReturnGift||Model.LastAbsence>=3600;welcomeGiftPending=Model.ReturnGift;
            BindPlatform();Show("welcome");
        }
        [Serializable] sealed class OldProfile {public List<string> owned;public string[] equipped;public bool reducedEffects;}
        [Serializable] sealed class OldSettings {public bool sound=true;}
        void Load(){
            IdleSave data=null;foreach(string key in new[]{PersistenceKey,PersistenceKey+".backup"}){
                string raw=PlayerPrefs.GetString(key,"");if(string.IsNullOrEmpty(raw))continue;
                try{var candidate=JsonUtility.FromJson<IdleSave>(raw);if(candidate!=null&&candidate.version==1){data=candidate;break;}}catch(Exception){Debug.LogWarning("Idle: damaged local save; checking backup.");}
            }
            Model=new IdleModel(data);
            if(data==null){
                try{var old=JsonUtility.FromJson<OldProfile>(PlayerPrefs.GetString(LegacyPersistenceKey("capy67.progress.v2"),""));
                    if(old!=null){if(old.owned!=null)foreach(string id in old.owned)Model.Grant(id);if(old.equipped!=null)Model.S.equipped=old.equipped;Model.S.calm=old.reducedEffects;}
                    var settings=JsonUtility.FromJson<OldSettings>(PlayerPrefs.GetString(LegacyPersistenceKey("capy67.save.v1"),""));if(settings!=null)Model.S.sound=settings.sound;
                }catch(Exception){Debug.LogWarning("Idle: old cosmetics could not be read; old save kept untouched.");}
                Model.S.randomState=(uint)(DateTime.UtcNow.Ticks^Environment.TickCount);Model.S.wardrobeVersion=0;Model.Repair();
            }
            welcomeBirthPending=data==null&&!PlayerPrefs.HasKey(LegacyPersistenceKey("capy67.progress.v2"))&&!PlayerPrefs.HasKey(LegacyPersistenceKey("capy67.save.v1"))&&!Model.S.birthIntroSeen;
        }
        public void Save(){if(Model==null)return;if(!suspended&&!platformPaused)Model.Stamp(Utc);string json=JsonUtility.ToJson(Model.S);PlayerPrefs.SetString(PersistenceKey,json);PlayerPrefs.SetString(PersistenceKey+".backup",json);PlayerPrefs.Save();saveAge=0;}
        void Update(){
            CheckAutoAdLoading();
            UpdateAudioState();
            if(!loaded||Model==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,1);
            if(!PresentationPaused){Model.Tick(dt);saveAge+=dt;reactionAge+=dt;noticeAge+=dt;
                bool keyboard=false;
#if ENABLE_INPUT_SYSTEM
                keyboard=!WelcomeNameInputFocused&&Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame;
                if(!WelcomeNameInputFocused&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){GoBack();}
#else
                keyboard=!WelcomeNameInputFocused&&Input.GetKeyDown(KeyCode.Space);if(!WelcomeNameInputFocused&&Input.GetKeyDown(KeyCode.Escape)){GoBack();}
#endif
                // One physical press earns one tap. Only the rewarded timer generates automatic taps.
                if(InputAllowed && keyboard)Care();
                if(saveAge>=10){Model.RefreshDay(Date);Save();}
            }
            TickActivity(dt);
            AnimateHero(dt);
            AnimateCare(dt);
            AnimateWelcome(dt);
            UpdateFinishVfx(dt);
            refreshAge+=dt;if(refreshAge>.15f){refreshAge=0;RefreshHome();foreach(var update in liveLabels)update();}
        }
        float HeroScale=>.88f+(Model.Stage-1)*.03f+(float)Model.StageFraction*.018f;
        public void Care(){if(!InputAllowed||!Model.Care())return;reactionAge=0;tapReaction=Model.S.cares%3;toast="+"+Number(Model.LastTapEarned)+" монет · "+TapReplies[tapReaction];noticeAge=0;Sound(careClip,.22f);TouchFeedback(Model.LastTapEarned);RefreshHome();}
        void Feed(int index){
            if(index>=0&&index<IdleModel.Foods.Length){int stage=Model.Stage;ChooseCurrency(IdleModel.Foods[index].name,IdleModel.Foods[index].price,m=>{if(Model.Stage==stage)CompleteFeed(index,m);});return;}
            CompleteFeed(index,false);
        }
        void CompleteFeed(int index,bool mandarins){
            if(!loaded||Model==null||PresentationPaused)return;
            int before=Model.Stage;double beforeGrowth=Model.S.growth,beforeFullness=Model.S.fullness;float oldScale=HeroScale;string form=null;bool ok=index<0?Model.Starter():Model.Feed(index,out form,mandarins);
            if(!ok){Toast(Model.FeedWait>0?"Миска будет готова через "+Math.Ceiling(Model.FeedWait)+" с":"Пока не хватает монет или стадии");return;}
            Save();reactionAge=0;mealAge=0;mealKind=index<0?0:index;if(form!=null||Model.Stage>before){evolutionAge=0;evolutionFromScale=oldScale;}Sound(form!=null||Model.Stage>before?evolveClip:feedClip,.25f);
            string message=form!=null?"НОВАЯ ФОРМА · "+IdleModel.Form(form).name:Model.Stage>before?"НОВАЯ СТАДИЯ · "+IdleModel.Stages[Model.Stage-1]:"Вкусно! +"+(Model.S.growth-beforeGrowth).ToString("0.#")+" роста";
            if(form==null&&Model.Stage==before){string gain=CareGain("Сытость",Model.S.fullness-beforeFullness);message+="\n"+(gain.Length>0?gain:"Сытость уже полная");}
            Show("home");Toast(message);if(Model.Stage>before)GrowthCelebration();
        }
        void BuyBed(){int level=Model.S.bed;ChooseCurrency("Мягкая подстилка",Model.BedPrice,m=>{if(Model.S.bed==level)CompleteBed(m);});}
        void CompleteBed(bool mandarins){double gain=Model.BedIncomeGain;if(!Model.BuyBed(mandarins)){Toast("Для улучшения нужна стадия "+Model.BedStage+" и "+Number(Model.BedPrice)+" монет");return;}Save();Sound(purchaseClip,.22f);RebuildHero();Show("food");Toast("Уютнее! Доход +"+Number(gain)+" монет/с");}
        void Toast(string text){toast=text;noticeAge=0;RefreshHome();}
        void Sound(AudioClip clip,float volume){if(Model!=null&&Model.S.sound&&!PresentationPaused&&isActiveAndEnabled&&audioSource&&clip)audioSource.PlayOneShot(clip,volume);}
        void OnApplicationFocus(bool focus){Suspend(!focus);}
        void OnApplicationPause(bool pause){Suspend(pause);}
        void Suspend(bool value){if(!loaded||Model==null||suspended==value)return;if(value)Save();suspended=value;UpdateAudioState();Release();if(value){PlatformPlaying(false);}else{
                double earned=platformPaused?0:Model.Arrive(Utc);Model.RefreshDay(Date);Save();if(!platformPaused&&(earned>=1||Model.ReturnGift||Model.LastAbsence>=3600)){
                    if(Page=="welcome"){offlineAward+=earned;welcomeOfflinePending=true;welcomeGiftPending|=Model.ReturnGift;}
                    else{offlineAward=earned;Show("offline");}
                }PlatformPlaying(!platformPaused);
            }}
        void OnApplicationQuit(){Save();}
        void Release(){CancelCareGestures();if(careHold)careHold.Release();if(heroHold)heroHold.Release();}
        void OnDestroy(){if(loaded)Save();UnbindPlatform();DisposeAudio();}
        void BindPlatform(){BindAutoAds();
#if PLUGIN_YG_2
            YG2.onPauseGame+=PlatformPause;YG2.onGetSDKData+=PlatformReady;if(YG2.isSDKEnabled)PlatformReady();
#endif
        }
        void PlatformReady(){
#if PLUGIN_YG_2
            YG2.GameReadyAPI();PlatformPlaying(!suspended&&!platformPaused);
#endif
        }
        void PlatformPlaying(bool value){
#if PLUGIN_YG_2
            if(!YG2.isSDKEnabled)return;if(value)YG2.GameplayStart();else YG2.GameplayStop();
#endif
        }
        void PlatformPause(bool value){platformPaused=value;UpdateAudioState();Release();if(Model==null)return;if(value){Save();}else{Model.Stamp(Utc);Save();}PlatformPlaying(!value&&!suspended);}
        void UnbindPlatform(){UnbindAutoAds();
#if PLUGIN_YG_2
            YG2.onPauseGame-=PlatformPause;YG2.onGetSDKData-=PlatformReady;if(YG2.isSDKEnabled)YG2.GameplayStop();
#endif
        }
        // Only the explicitly requested autoclicker uses rewarded ads; no automatic ads.
        // Premium food is obtainable through the calendar without advertising.
        static string Number(double value)=>value>=1e9?(value/1e9).ToString("0.#")+" млрд":value>=1e6?(value/1e6).ToString("0.#")+" млн":value>=10000?(value/1000).ToString("0.#")+" тыс":value.ToString(value<10?"0.#":"0");
        static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        static SoftShape Shape(Transform p,string n,float x,float y,float w,float h,string color,float radius,bool ellipse=false){var s=Rect(p,n,x,y,w,h).gameObject.AddComponent<SoftShape>();s.color=C(color);s.radius=radius;s.ellipse=ellipse;s.raycastTarget=false;return s;}
        static SoftShape Tile(Transform p,string n,float x,float y,float w,float h,string c,float r=8)=>Shape(p,n,x,y,w,h,UiColor(c),r);
        static void Mark(Transform p,string text,float x,float y,float w,float h,int size,string color){var t=Rect(p,"Emblem",x,y-(Mathf.Max(h,size*1.5f)-h)*.5f,w,Mathf.Max(h,size*1.5f)).gameObject.AddComponent<Text>();t.font=Resources.Load<Font>("Fonts/Nunito-ExtraBold")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.lineSpacing=1.04f;t.text=text;t.color=C(color);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;}
        Text Label(Transform p,string n,string text,float x,float y,float w,float h,int size=22,string color="FFF4D6",TextAnchor align=TextAnchor.MiddleLeft){var t=Rect(p,n,x,y,w,h).gameObject.AddComponent<Text>();t.font=regular;t.fontSize=size;t.lineSpacing=1.04f;t.text=text;t.color=C(UiColor(color));t.raycastTarget=false;t.alignment=align;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        Button Button(Transform p,string n,string text,float x,float y,float w,float h,Action click,string color="39315B",int size=23){
            bool invisible=color=="FFFFFF00",primary=color=="55E6C1";string face=primary?"79A957":UiColor(color);
            var s=Shape(p,n,x,y,w,h,invisible?color:Ink,17);s.raycastTarget=true;s.canvasRenderer.cullTransparentMesh=false;
            var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.navigation=new Navigation{mode=Navigation.Mode.None};
            if(!invisible){Shape(b.transform,"Button depth",3,5,w-6,h-8,primary?"4E733D":"B08855",14);Shape(b.transform,"Button face",4,3,w-8,h-12,face,13);
                Shape(b.transform,"Button bevel",13,7,w-26,4,primary?"B9D683":"F8E5AF",2);Shape(b.transform,"Button foot",14,h-9,w-28,3,primary?"365B33":"946238",1);
                b.gameObject.AddComponent<GardenButtonState>();}
            var t=Label(b.transform,"Label",text,9,-2,w-18,h,size,primary?"253C26":"513821",TextAnchor.MiddleCenter);t.font=bold;
            if(click!=null)b.onClick.AddListener(()=>{if(!PresentationPaused&&!FoodDragging&&(!sectionHelp||p==sectionHelp||p.IsChildOf(sectionHelp))&&(!currencyChoice||p==currencyChoice||p.IsChildOf(currencyChoice)))click();});return b;
        }
        void Clear(Transform parent){for(int n=parent.childCount-1;n>=0;n--){var child=parent.GetChild(n).gameObject;child.SetActive(false);Destroy(child);}}
        void BuildUI(){
            var cv=new GameObject("Idle Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));cv.transform.SetParent(transform,false);cv.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=cv.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(480,850);scaler.matchWidthOrHeight=.5f;
            var bg=Tile(cv.transform,"Backdrop",0,0,480,850,"19152F",0).rectTransform;bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;
            board=Rect(cv.transform,"Idle board",0,0,480,850);board.anchorMin=board.anchorMax=board.pivot=new Vector2(.5f,.5f);board.gameObject.AddComponent<BoardFit>();board.gameObject.AddComponent<RectMask2D>();Tile(board,"Board",0,0,480,850,"211B45",0);
            if(!FindAnyObjectByType<EventSystem>()){
                var es=new GameObject("Idle EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
            home=Rect(board,"Home",0,0,480,850);decor=Rect(home,"Decor",0,0,480,850);
            coinText=Label(home,"Coins","",90,18,164,52,26,"FFF0C8");coinText.font=bold;rateText=Label(home,"Income","",262,18,110,52,22,"FFF0C8",TextAnchor.MiddleCenter);BuildMandarinHud();
            var settings=Button(home,"Settings","",386,20,70,67,()=>Show("settings"));Gear(settings.transform,15,10);
            Button(home,"Growth stages","",16,119,448,109,()=>Show("growth"),"FFFFFF00");stageText=Label(home,"Pet name","",32,122,386,32,26);stageText.font=bold;stageText.supportRichText=false;stageText.resizeTextForBestFit=true;stageText.resizeTextMinSize=18;stageText.resizeTextMaxSize=26;petStageText=Label(home,"Growth stage","",32,153,386,31,22);Label(home,"Open growth hint","›",425,122,22,42,28);Tile(home,"Growth track",24,185,432,8,"CABB90",5);growthFill=Tile(home,"Growth fill",24,185,432,8,"55E6C1",5);
            growthText=Label(home,"Growth progress","",30,194,420,33,21,"C9BCD8",TextAnchor.MiddleCenter);
            hero=Rect(home,"Capybara",120,270,250,260);RebuildHero();
            var hit=Button(home,"Care on character","",70,278,340,252,null,"FFFFFF00");heroHold=hit.gameObject.AddComponent<IdleHold>();heroHold.pressed=()=>{if(!BathInteractive)Care();};heroHold.longPressed=()=>{if(!BathInteractive)DoCare(3);};BindBathGesture(hit);
            Tile(home,"Care caption paper",24,230,432,60,"FFF1CEE8",12);
            notice=Label(home,"Care feedback","",34,228,412,64,22,"FFF4D6",TextAnchor.MiddleCenter);
            BuildCareHud();
            var care=Button(home,"Care hold","ТАП · МОНЕТЫ",24,606,284,64,null,"55E6C1",20);tapButtonText=care.GetComponentInChildren<Text>();Button(home,"Tap upgrades","СИЛА ТАПА",320,606,136,64,()=>Show("tap"),"F4CF80",22);careHold=care.gameObject.AddComponent<IdleHold>();careHold.pressed=Care;
            Button(home,"Food","ЗАБОТА",24,686,136,64,()=>Show("care"),"F4CF80",22);Button(home,"Activities","ЗАНЯТИЯ",172,686,136,64,()=>OpenActivities("home"),"55E6C1",22);Button(home,"Shop","ГАРДЕРОБ",320,686,136,64,()=>{wardrobeReturn="home";Show("shop");},"E9B49A",20);
            Button(home,"Surprises","ДЕРЕВО",24,770,136,64,()=>Show("surprises"),size:22);Button(home,"Tasks","ЗАДАЧИ",172,770,136,64,()=>Show("quests"),size:22);Button(home,"Daily","ПОДАРКИ",320,770,136,64,()=>Show("daily"),size:22);
            BuildHomeSignals();
            panel=Rect(board,"Pages",0,0,480,850);pageOpacity=panel.gameObject.AddComponent<CanvasGroup>();
            BuildFinishVfx();
            cv.AddComponent<CapyDesktopLayout>().Initialize(board,this,regular,bold,DesktopNavigate,()=>loaded&&!PresentationPaused&&!FoodDragging&&!sectionHelp&&!currencyChoice&&Page!="welcome"&&Page!="offline"&&Page!="surprise-opening"&&!Page.StartsWith("activity-"));
        }
        void DesktopNavigate(string page){if(page=="activities")OpenActivities("home");else{if(page=="shop")wardrobeReturn="home";Show(page);}}
        void RefreshHome(){if(!loaded||Model==null)return;
            coinText.text=Number(Model.S.coins);rateText.text="+"+Number(Model.Passive)+" / с";if(mandarinHud)mandarinHud.text=Number(Model.M.balance);
            stageText.text=Model.S.petName;petStageText.text=Model.Stage+" / "+IdleModel.StageCount+"   "+IdleModel.Stages[Model.Stage-1];growthFill.rectTransform.sizeDelta=new Vector2(432*(float)Model.StageFraction,8);
            growthText.text=Model.PendingGrowthGifts>0?"Подарок за рост готов ›":Model.NextGrowthGift>0?"Ещё "+Number(Math.Ceiling(Model.NextGrowthGift-Model.S.growth))+" роста до подарка":Model.Stage==IdleModel.StageCount?"Хранитель сада · все этапы пройдены":Number(Model.S.growth)+" / "+Number(IdleModel.Growth[Model.Stage])+" роста";
            notice.text=BathInteractive&&noticeAge>1.4f?"Можно водить пальцем по пене":noticeAge<4?toast:CareHint();
            if(tapButtonText)tapButtonText.text="ТАП · +"+Number(Model.ManualCareValue)+(Model.S.autoRemaining>0?"\nАВТО "+Minutes(Model.S.autoRemaining):" монет");RefreshCareHud();RefreshHomeSignals();
        }
        public void Show(string page){if(!loaded)return;ClearFinishVfx();BuildPage(page);OpenPageFinish();}
        void BuildPage(string page){if(!loaded)return;CloseSectionHelp();CloseCurrencyChoice();if(page=="album")page="surprises";Release();if(surpriseReveal.IsActive())surpriseReveal.Kill();if(pageTween.IsActive())pageTween.Kill();pageOpacity.alpha=1;Page=page;liveLabels.Clear();Clear(panel);home.gameObject.SetActive(page=="home");panel.gameObject.SetActive(page!="home");if(page=="home"){RebuildHero();RefreshHome();return;}
            if(page=="welcome"){WelcomePage();return;}
            if(page=="mandarins"||page.StartsWith("mandarin-")){if(page!="mandarin-stock"&&page!="mandarin-result")Page="mandarins";PageFrame("МАНДАРИНЫ");if(Page=="mandarin-stock")MandarinStockPage();else if(Page=="mandarin-result")MandarinResultPage();else MandarinShopPage();foreach(var update in liveLabels)update();return;}
            if(page=="activities"||page.StartsWith("activity-")){PageFrame("ЗАНЯТИЯ В САДУ");if(page=="activities")ActivityHub();else if(page=="activity-play")ActivityPlayPage();else if(page=="activity-exit")ActivityExitPage();else ActivityResultPage();return;}
            string title=page.StartsWith("surprise")?"ДЕРЕВО СЮРПРИЗОВ":page=="tap"?"МАСТЕРСКАЯ ТАПА":page=="care"?"ЗАБОТА О КАПИБАРЕ":page=="growth"?"ЭТАПЫ РОСТА":page=="food"?"ЕДА И УЮТ":page=="shop"?"ТВОИ ВЕЩИ":page.StartsWith("daily")?"ПОДАРОК ДНЯ":page=="quests"?"ТВОИ ПОРУЧЕНИЯ":page=="visitor"?"ГОСТЬ В САДУ":page=="offline"?"ПОКА ТЫ ОТДЫХАЛ":"ТВОЙ КОМФОРТ";
            PageFrame(title);
            if(page=="surprise-budget")SurpriseBudgetPage();else if(page=="surprise-opening")SurpriseOpeningPage();else if(page=="surprises")SurprisePage();else if(page=="surprise-results")SurpriseResultsPage();else if(page=="surprise-gifts")SurpriseGiftsPage();else if(page=="surprise-odds")SurpriseOddsPage();else if(page=="surprise-pool")SurprisePoolPage();else if(page=="surprise-craft")SurpriseCraftPage();else if(page=="tap")TapPage();else if(page=="care")CarePage();else if(page=="growth")GrowthPage();else if(page=="food")FoodPage();else if(page=="shop")ShopPage();else if(page=="daily")DailyPage();else if(page=="daily-reward")DailyRewardPage();else if(page=="quests")QuestPage();else if(page=="visitor")VisitorPage();else if(page=="offline")OfflinePage();else SettingsPage();
            foreach(var update in liveLabels)update();
        }
        void FoodPage(){
            var wallet=Label(panel,"Food balance","",24,91,432,36);liveLabels.Add(()=>wallet.text=Number(Model.S.coins)+" монет · нажми на миску");
            if(!Model.S.starter)BindFoodGesture(Button(panel,"Starter bowl","ПЕРВАЯ МИСКА · 0 МОНЕТ",24,134,432,68,()=>Feed(-1),"55E6C1",22),-1);
            else {var bed=Button(panel,"Bed upgrade","",24,134,432,68,BuyBed,"55E6C1",20);liveLabels.Add(()=>{bed.GetComponentInChildren<Text>().text=Model.S.bed==8?"УЮТ НА МАКСИМУМЕ\nДоход "+Number(Model.Passive)+" монет/с":"ПОДСТИЛКА "+(Model.S.bed+1)+" · "+Number(Model.BedPrice)+" монет\n"+(Model.Stage<Model.BedStage?"Со стадии "+Model.BedStage+" · ":"")+"Доход +"+Number(Model.BedIncomeGain)+" монет/с";bed.interactable=Model.CanBed||Model.CanBuyBed(true);});}
            foodPage=Mathf.Clamp(foodPage,0,(IdleModel.Foods.Length-1)/4);
            for(int n=0;n<4;n++){int index=foodPage*4+n;if(index>=IdleModel.Foods.Length)break;var f=IdleModel.Foods[index];float y=212+n*101;
                var b=Button(panel,"Food "+index,"",24,y,432,94,()=>Feed(index));BindFoodGesture(b,index);
                Oval(b.transform,"Meal medallion",10,6,83,75,"FFF2D0",1.5f);var icon=Rect(b.transform,"Detailed food",17,14,70,64);icon.localScale=Vector3.one*1.04f;DrawFoodIcon(icon,index,0,0);
                Label(b.transform,"Food name",f.name,108,2,310,32,23);var text=Label(b.transform,"Food cost","",108,32,310,30,22,"C9BCD8");
                Leaf(b.transform,109,66,18,19,"8DB95B",-10);
                var benefits=Label(b.transform,"Meal fullness","",135,61,129,30,20,"418355");
                var bowl=Rect(b.transform,"Fullness picture",270,62,80,80);bowl.localScale=Vector3.one*.32f;GuideArt(bowl,"food");
                Label(b.transform,"Fullness gain","+"+f.fullness,305,61,110,30,20,"418355");
                liveLabels.Add(()=>{text.text=Model.Stage<f.stage?"Откроется: этап "+f.stage:Model.FeedWait>0?"Готовим · "+Math.Ceiling(Model.FeedWait)+" с":Number(f.price)+" монет";benefits.text="+"+Number(Model.MealGrowth(index));b.interactable=Model.Stage>=f.stage&&Model.CanPayEither(f.price)&&Model.FeedWait<=0;});
            }
            var prev=Button(panel,"Food previous","←",24,619,64,64,()=>{foodPage--;Show("food");});prev.interactable=foodPage>0;
            var next=Button(panel,"Food next","→",392,619,64,64,()=>{foodPage++;Show("food");});next.interactable=foodPage<(IdleModel.Foods.Length-1)/4;
            int featured=Model.FeaturedFood;
            Label(panel,"Menu page",(foodPage+1)+" / "+((IdleModel.Foods.Length+3)/4)+(featured<0?" · 12 блюд":" · сегодня +10%\n"+IdleModel.Foods[featured].name),97,617,286,66,22,"C9BCD8",TextAnchor.MiddleCenter);
            var premium=Button(panel,"Special meal","",24,691,432,66,()=>Feed(IdleModel.PremiumMeal),"4B3C67",22);
            BindFoodGesture(premium,IdleModel.PremiumMeal);
            var specialIcon=Rect(premium.transform,"Celebration dish",16,7,68,57);specialIcon.localScale=Vector3.one*.8f;DrawFoodIcon(specialIcon,IdleModel.PremiumMeal,0,0);
            var premiumText=Label(premium.transform,"Special meal label","",86,0,333,62,20);
            liveLabels.Add(()=>{premiumText.text="ПРАЗДНИЧНАЯ · "+Model.S.premiumFood+" шт.\n+"+Number(Model.MealGrowth(IdleModel.PremiumMeal))+" роста · +60 сытости";premium.interactable=Model.Stage>=3&&Model.S.premiumFood>0&&Model.FeedWait<=0;});
            Button(panel,"Free leaf food",Model.M.stock>0?"ЛИСТОЧЕК · 5 МОНЕТ":"ЛИСТОЧЕК · 5 МОНЕТ\n+25 сытости · без роста",24,766,Model.M.stock>0?210:432,64,()=>DoCare(0),"55E6C1",20);
            if(Model.M.stock>0)Button(panel,"Saved food stock","ЗАПАС · "+Model.M.stock,246,766,210,64,()=>Show("mandarin-stock"),size:22);
        }
        List<IdleItem> CategoryItems(){var list=new List<IdleItem>();foreach(var i in IdleCatalog.Items)if(i.slot==category&&CapyCompleteSets.Visible(i)&&(!ownedOnly||Model.S.owned.Contains(i.id)))list.Add(i);return list;}
        void OfflinePage(){
            Label(panel,"Offline greeting","Капибара ждала тебя!",40,110,400,48,28,"513821",TextAnchor.MiddleCenter).font=bold;
            PaperCard(panel,"Offline garden card",48,170,384,264);
            Shape(panel,"Offline meadow",71,348,338,62,"DCE4AF",0,true);
            Leaf(panel,75,322,24,39,"9BB878",-24);Leaf(panel,378,326,22,36,"B5CA89",24);
            var avatar=Rect(panel,"Sleeping capy",151,190,240,250);DrawAvatar(avatar);avatar.localScale=Vector3.one*.75f;
            Label(panel,"Offline earnings caption","Монеты за отдых",64,442,352,42,24,"72543A",TextAnchor.MiddleCenter);
            PaperCard(panel,"Offline earnings card",48,493,384,122);
            Coin(panel,70,520,48);
            Label(panel,"Offline amount","+"+Number(offlineAward),132,501,279,58,36,"418355").font=bold;
            Label(panel,"Offline credited","Уже в кошельке",132,558,279,40,22,"72543A");
            Label(panel,"Offline note",Model.ReturnGift||welcomeGiftPending?"Ещё +25 за возвращение!":"Пора кормить и играть",40,636,400,56,25,"72543A",TextAnchor.MiddleCenter);
            Button(panel,"Continue idle","К КАПИБАРЕ",24,730,432,76,()=>{welcomeOfflinePending=welcomeGiftPending=false;offlineAward=0;Model.Wake();Show("home");BeginCareReaction(6);},"55E6C1",25);
        }
        void SettingsPage(){
            Button(panel,"Sound setting","ЗВУКИ: "+(Model.S.sound?"ВКЛ.":"ВЫКЛ."),24,125,432,70,()=>{Model.S.sound=!Model.S.sound;UpdateAudioState();Save();Show("settings");});
            Button(panel,"Music setting","МУЗЫКА: "+(Model.S.music?"ВКЛ.":"ВЫКЛ."),24,211,432,70,()=>{Model.S.music=!Model.S.music;UpdateAudioState();Save();Show("settings");});
            Button(panel,"Motion setting","МЕНЬШЕ ДВИЖЕНИЯ: "+(Model.S.calm?"ВКЛ.":"ВЫКЛ."),24,297,432,70,()=>{Model.S.calm=!Model.S.calm;Save();Show("settings");},size:22);
            Button(panel,"Meme setting","МЕМНЫЙ ВИД: "+(Model.S.memeVisuals?"ВКЛ.":"ВЫКЛ."),24,383,432,70,()=>{Model.S.memeVisuals=!Model.S.memeVisuals;Save();Show("settings");},size:22);
            Button(panel,"Surprise ads setting","ВИДЕО ДЛЯ ДЕРЕВА: "+(Model.S.surpriseAds?"ВКЛ.":"ВЫКЛ."),24,469,432,70,()=>{Model.S.surpriseAds=!Model.S.surpriseAds;Save();Show("settings");},size:22);
            Label(panel,"Settings info","Тапай — получай монеты.\nТвои вещи сохраняются.\nПрогресс — на этом устройстве.",32,550,416,185,22,"C9BCD8",TextAnchor.MiddleCenter);
            Button(panel,"Settings done","ГОТОВО",24,750,432,76,()=>Show("home"),"55E6C1",25);
        }
    }
}










