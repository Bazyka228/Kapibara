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
        public IdleModel Model {get;private set;}
        public string Page {get;private set;}="home";
        RectTransform board,home,panel,hero,decor;
        Text coinText,stageText,growthText,rateText,notice;
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
        public bool InputAllowed=>loaded&&Model!=null&&!PresentationPaused&&Page=="home";
        void OnEnable(){
#if UNITY_EDITOR
            if(!Application.isPlaying)return;
            // Editor domain reload keeps Unity objects but drops the managed model and delegates.
            if(loaded&&Model==null){
                loaded=false;suspended=false;platformPaused=false;liveLabels.Clear();UnbindPlatform();
                var previous=transform.Find("Idle Canvas");if(previous){previous.gameObject.SetActive(false);Destroy(previous.gameObject);}
                if(careClip)Destroy(careClip);if(feedClip)Destroy(feedClip);if(evolveClip)Destroy(evolveClip);
                Start();
            }
#endif
            if(loaded&&Model!=null)BindAutoAds();
        }
        void OnDisable(){UnbindAutoAds();CancelCareGestures();if(loaded&&Model!=null)Save();}
        void Start(){
            regular=Resources.Load<Font>("Fonts/Nunito-SemiBold")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");bold=Resources.Load<Font>("Fonts/Nunito-ExtraBold")??regular;
            Load();audioSource=GetComponent<AudioSource>();if(!audioSource)audioSource=gameObject.AddComponent<AudioSource>();careClip=Tone(490,.065f);feedClip=Tone(710,.14f);evolveClip=Tone(980,.25f);
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
                try{var old=JsonUtility.FromJson<OldProfile>(PlayerPrefs.GetString("capy67.progress.v2",""));
                    if(old!=null){if(old.owned!=null)foreach(string id in old.owned)Model.Grant(id);if(old.equipped!=null)Model.S.equipped=old.equipped;Model.S.calm=old.reducedEffects;}
                    var settings=JsonUtility.FromJson<OldSettings>(PlayerPrefs.GetString("capy67.save.v1",""));if(settings!=null)Model.S.sound=settings.sound;
                }catch(Exception){Debug.LogWarning("Idle: old cosmetics could not be read; old save kept untouched.");}
                Model.S.randomState=(uint)(DateTime.UtcNow.Ticks^Environment.TickCount);Model.S.wardrobeVersion=0;Model.Repair();
            }
        }
        public void Save(){if(Model==null)return;if(!suspended&&!platformPaused)Model.Stamp(Utc);string json=JsonUtility.ToJson(Model.S);PlayerPrefs.SetString(PersistenceKey,json);PlayerPrefs.SetString(PersistenceKey+".backup",json);PlayerPrefs.Save();saveAge=0;}
        void Update(){
            CheckAutoAdLoading();
            if(!loaded||Model==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,1);
            if(!PresentationPaused){Model.Tick(dt);saveAge+=dt;reactionAge+=dt;noticeAge+=dt;
                bool keyboard=false;
#if ENABLE_INPUT_SYSTEM
                keyboard=Keyboard.current!=null&&Keyboard.current.spaceKey.isPressed;
                if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(Page=="welcome")BeginFromWelcome();else Show("home");}
#else
                keyboard=Input.GetKey(KeyCode.Space);if(Input.GetKeyDown(KeyCode.Escape)){if(Page=="welcome")BeginFromWelcome();else Show("home");}
#endif
                if(InputAllowed && (careHold.Held||keyboard))Care();
                if(saveAge>=10){Model.RefreshDay(Date);Save();}
            }
            AnimateHero(dt);
            AnimateCare(dt);
            AnimateWelcome(dt);
            refreshAge+=dt;if(refreshAge>.15f){refreshAge=0;RefreshHome();foreach(var update in liveLabels)update();}
        }
        float HeroScale=>.88f+(Model.Stage-1)*.03f+(float)Model.StageFraction*.018f;
        public void Care(){if(!InputAllowed||!Model.Care())return;reactionAge=0;toast="+"+Number(Model.LastTapEarned)+" монет · Спасибо за заботу!";noticeAge=0;Sound(careClip,.13f);RefreshHome();}
        void Feed(int index){
            if(!loaded||Model==null||PresentationPaused)return;
            int before=Model.Stage;double beforeGrowth=Model.S.growth,beforeFullness=Model.S.fullness;float oldScale=HeroScale;string form=null;bool ok=index<0?Model.Starter():Model.Feed(index,out form);
            if(!ok){Toast(Model.FeedWait>0?"Миска будет готова через "+Math.Ceiling(Model.FeedWait)+" с":"Пока не хватает монет или стадии");return;}
            Save();reactionAge=0;mealAge=0;mealKind=index<0?0:index;if(form!=null||Model.Stage>before){evolutionAge=0;evolutionFromScale=oldScale;}Sound(form!=null||Model.Stage>before?evolveClip:feedClip,.25f);
            string message=form!=null?"НОВАЯ ФОРМА · "+IdleModel.Form(form).name:Model.Stage>before?"НОВАЯ СТАДИЯ · "+IdleModel.Stages[Model.Stage-1]:"Вкусно! +"+(Model.S.growth-beforeGrowth).ToString("0.#")+" роста";
            if(form==null&&Model.Stage==before){string gain=CareGain("Сытость",Model.S.fullness-beforeFullness);message+="\n"+(gain.Length>0?gain:"Сытость уже полная");}
            Show("home");Toast(message);
        }
        void BuyBed(){if(!Model.BuyBed()){Toast("Для улучшения нужна стадия "+Model.BedStage+" и "+Number(Model.BedPrice)+" монет");return;}Save();RebuildHero();Show("food");Toast("Уголок стал уютнее — доход вырос");}
        void Toast(string text){toast=text;noticeAge=0;RefreshHome();}
        void Sound(AudioClip clip,float volume){if(Model.S.sound&&!platformPaused&&audioSource)audioSource.PlayOneShot(clip,volume);}
        void OnApplicationFocus(bool focus){Suspend(!focus);}
        void OnApplicationPause(bool pause){Suspend(pause);}
        void Suspend(bool value){if(!loaded||Model==null||suspended==value)return;if(value)Save();suspended=value;Release();if(value){PlatformPlaying(false);}else{
                double earned=platformPaused?0:Model.Arrive(Utc);Model.RefreshDay(Date);Save();if(!platformPaused&&(earned>=1||Model.ReturnGift||Model.LastAbsence>=3600)){
                    if(Page=="welcome"){offlineAward+=earned;welcomeOfflinePending=true;welcomeGiftPending|=Model.ReturnGift;}
                    else{offlineAward=earned;Show("offline");}
                }PlatformPlaying(!platformPaused);
            }}
        void OnApplicationQuit(){Save();}
        void Release(){CancelCareGestures();if(careHold)careHold.Release();if(heroHold)heroHold.Release();}
        void OnDestroy(){if(loaded)Save();UnbindPlatform();if(careClip)Destroy(careClip);if(feedClip)Destroy(feedClip);if(evolveClip)Destroy(evolveClip);}
        static AudioClip Tone(float hz,float length){int count=(int)(22050*length);var data=new float[count];for(int n=0;n<count;n++)data[n]=Mathf.Sin(n*hz*Mathf.PI*2/22050)*Mathf.Sin(Mathf.PI*n/count)*.35f;var c=AudioClip.Create("Idle note",count,1,22050,false);c.SetData(data,0);return c;}
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
        void PlatformPause(bool value){platformPaused=value;Release();if(Model==null)return;if(value){Save();if(audioSource)audioSource.Stop();}else{Model.Stamp(Utc);Save();}PlatformPlaying(!value&&!suspended);}
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
            if(click!=null)b.onClick.AddListener(()=>{if(!PresentationPaused&&!FoodDragging)click();});return b;
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
            coinText=Label(home,"Coins","",83,18,294,52,32,"FFF0C8");coinText.font=bold;rateText=Label(home,"Income","",34,70,340,32,22,"FFF0C8");
            var settings=Button(home,"Settings","",386,20,70,64,()=>Show("settings"));Gear(settings.transform,15,10);
            Button(home,"Growth stages","",16,119,448,109,()=>Show("growth"),"FFFFFF00");stageText=Label(home,"Growth stage","",32,122,386,37,26);stageText.font=bold;Label(home,"Open growth hint","›",425,122,22,37,28);Tile(home,"Growth track",24,168,432,11,"CABB90",5);growthFill=Tile(home,"Growth fill",24,168,432,11,"55E6C1",5);
            growthText=Label(home,"Growth progress","",30,183,420,41,22,"C9BCD8",TextAnchor.MiddleCenter);
            hero=Rect(home,"Capybara",120,270,250,260);RebuildHero();
            var hit=Button(home,"Care on character","",70,278,340,252,null,"FFFFFF00");heroHold=hit.gameObject.AddComponent<IdleHold>();heroHold.pressed=()=>{if(!BathInteractive)Care();};heroHold.longPressed=()=>{if(!BathInteractive)DoCare(3);};BindBathGesture(hit);
            Tile(home,"Care caption paper",24,230,432,60,"FFF1CEE8",12);
            notice=Label(home,"Care feedback","",34,228,412,64,22,"FFF4D6",TextAnchor.MiddleCenter);
            BuildCareHud();
            var care=Button(home,"Care hold","ТАП · МОНЕТЫ",24,606,284,64,null,"55E6C1",23);tapButtonText=care.GetComponentInChildren<Text>();Button(home,"Tap upgrades","СИЛА ТАПА",320,606,136,64,()=>Show("tap"),"F4CF80",22);careHold=care.gameObject.AddComponent<IdleHold>();careHold.pressed=Care;
            Button(home,"Food","ЗАБОТА",24,686,210,64,()=>Show("care"),"F4CF80");Button(home,"Shop","ГАРДЕРОБ",246,686,210,64,()=>Show("shop"),"E9B49A");
            Button(home,"Surprises","ДЕРЕВО",24,770,136,64,()=>Show("surprises"),size:22);Button(home,"Tasks","ЗАДАЧИ",172,770,136,64,()=>Show("quests"),size:22);Button(home,"Daily","ПОДАРКИ",320,770,136,64,()=>Show("daily"),size:22);
            panel=Rect(board,"Pages",0,0,480,850);pageOpacity=panel.gameObject.AddComponent<CanvasGroup>();
        }
        void RefreshHome(){if(!loaded||Model==null)return;
            coinText.text=Number(Model.S.coins)+" монет";rateText.text="+"+Number(Model.Passive)+" / с   ·   "+Model.S.stars+" звёзд";
            stageText.text=Model.Stage+" / "+IdleModel.StageCount+"   "+IdleModel.Stages[Model.Stage-1];growthFill.rectTransform.sizeDelta=new Vector2(432*(float)Model.StageFraction,11);
            growthText.text=Model.Stage==IdleModel.StageCount?"Хранитель сада · все этапы пройдены":Number(Model.S.growth)+" / "+Number(IdleModel.Growth[Model.Stage])+" роста";
            notice.text=BathInteractive&&noticeAge>1.4f?"Можно водить пальцем по пене":noticeAge<4?toast:CareHint();
            if(tapButtonText)tapButtonText.text="ТАП · +"+Number(Model.CareValue)+(Model.S.autoRemaining>0?"\nАВТО "+Minutes(Model.S.autoRemaining):" монет");RefreshCareHud();
        }
        public void Show(string page){if(!loaded)return;if(page=="album")page="surprises";Release();if(pageTween.IsActive())pageTween.Kill();pageOpacity.alpha=1;Page=page;liveLabels.Clear();Clear(panel);home.gameObject.SetActive(page=="home");panel.gameObject.SetActive(page!="home");if(page=="home"){RebuildHero();RefreshHome();return;}
            if(page=="welcome"){WelcomePage();return;}
            string title=page.StartsWith("surprise")?"ДЕРЕВО СЮРПРИЗОВ":page=="tap"?"МАСТЕРСКАЯ ТАПА":page=="care"?"ЗАБОТА О КАПИБАРЕ":page=="growth"?"ЭТАПЫ РОСТА":page=="food"?"ЕДА И УЮТ":page=="shop"?"ТВОИ ВЕЩИ":page=="daily"?"ПОДАРОК ДНЯ":page=="quests"?"ТВОИ ПОРУЧЕНИЯ":page=="visitor"?"ГОСТЬ В САДУ":page=="offline"?"ПОКА ТЫ ОТДЫХАЛ":"ТВОЙ КОМФОРТ";
            PageFrame(title);
            if(page=="surprises")SurprisePage();else if(page=="surprise-results")SurpriseResultsPage();else if(page=="surprise-gifts")SurpriseGiftsPage();else if(page=="surprise-odds")SurpriseOddsPage();else if(page=="surprise-pool")SurprisePoolPage();else if(page=="surprise-craft")SurpriseCraftPage();else if(page=="tap")TapPage();else if(page=="care")CarePage();else if(page=="growth")GrowthPage();else if(page=="food")FoodPage();else if(page=="shop")ShopPage();else if(page=="daily")DailyPage();else if(page=="quests")QuestPage();else if(page=="visitor")VisitorPage();else if(page=="offline")OfflinePage();else SettingsPage();
            foreach(var update in liveLabels)update();
            if(!Model.S.calm){pageOpacity.alpha=.75f;pageTween=tweens.Track(pageOpacity.DOFade(1,.18f).SetEase(Ease.OutQuad),panel.gameObject);}
        }
        void FoodPage(){
            var wallet=Label(panel,"Food balance","",24,91,432,36);liveLabels.Add(()=>wallet.text=Number(Model.S.coins)+" монет · тапни или тяни еду");
            if(!Model.S.starter)BindFoodGesture(Button(panel,"Starter bowl","ПЕРВАЯ МИСКА · БЕСПЛАТНО",24,134,432,68,()=>Feed(-1),"55E6C1",22),-1);
            else {var bed=Button(panel,"Bed upgrade","",24,134,432,68,BuyBed,"55E6C1",22);liveLabels.Add(()=>{bed.GetComponentInChildren<Text>().text=Model.S.bed==8?"УЮТ НА МАКСИМУМЕ":"ПОДСТИЛКА "+(Model.S.bed+1)+" · "+Number(Model.BedPrice)+" монет\nСо стадии "+Model.BedStage;bed.interactable=Model.CanBed;});}
            foodPage=Mathf.Clamp(foodPage,0,(IdleModel.Foods.Length-1)/4);
            for(int n=0;n<4;n++){int index=foodPage*4+n;if(index>=IdleModel.Foods.Length)break;var f=IdleModel.Foods[index];float y=212+n*101;
                var b=Button(panel,"Food "+index,"",24,y,432,94,()=>Feed(index));BindFoodGesture(b,index);
                Oval(b.transform,"Meal medallion",10,6,83,75,"FFF2D0",1.5f);var icon=Rect(b.transform,"Detailed food",17,14,70,64);icon.localScale=Vector3.one*1.04f;DrawFoodIcon(icon,index,0,0);
                Label(b.transform,"Food name",f.name,108,2,310,32,23);var text=Label(b.transform,"Food cost","",108,32,310,30,22,"C9BCD8");
                Label(b.transform,"Meal fullness","Сытость +"+f.fullness,108,61,310,30,22,"418355");
                liveLabels.Add(()=>{text.text=Model.Stage<f.stage?"Со стадии "+f.stage:Model.FeedWait>0?"Готовим · "+Math.Ceiling(Model.FeedWait)+" с":Number(f.price)+" монет → +"+Number(Model.MealGrowth(index))+" роста";b.interactable=Model.Stage>=f.stage&&Model.S.coins>=f.price&&Model.FeedWait<=0;});
            }
            var prev=Button(panel,"Food previous","←",24,619,64,64,()=>{foodPage--;Show("food");});prev.interactable=foodPage>0;
            var next=Button(panel,"Food next","→",392,619,64,64,()=>{foodPage++;Show("food");});next.interactable=foodPage<(IdleModel.Foods.Length-1)/4;
            int featured=Model.FeaturedFood;
            Label(panel,"Menu page",(foodPage+1)+" / "+((IdleModel.Foods.Length+3)/4)+(featured<0?" · 12 блюд":" · сегодня +10%\n"+IdleModel.Foods[featured].name),97,617,286,66,22,"C9BCD8",TextAnchor.MiddleCenter);
            var premium=Button(panel,"Special meal","",24,691,432,66,()=>Feed(IdleModel.PremiumMeal),"4B3C67",22);
            BindFoodGesture(premium,IdleModel.PremiumMeal);
            var specialIcon=Rect(premium.transform,"Celebration dish",16,7,68,57);specialIcon.localScale=Vector3.one*.8f;DrawFoodIcon(specialIcon,IdleModel.PremiumMeal,0,0);
            var premiumText=Label(premium.transform,"Special meal label","",86,0,333,62,22);
            liveLabels.Add(()=>{premiumText.text="ПРАЗДНИЧНАЯ · "+Model.S.premiumFood+" шт.\n+"+Number(Model.MealGrowth(IdleModel.PremiumMeal))+" роста · +60 сытости";premium.interactable=Model.Stage>=3&&Model.S.premiumFood>0&&Model.FeedWait<=0;});
            Button(panel,"Free leaf food","ЛИСТОЧЕК · БЕСПЛАТНО\n+25 сытости · без роста",24,766,432,64,()=>DoCare(0),"55E6C1",22);
        }
        void ShopPage(){
            var selectedItem=IdleCatalog.Find(selected)??IdleCatalog.Items[0];selected=selectedItem.id;
            PaperCard(panel,"Preview card",24,99,432,185);ScenePreview(panel,selectedItem);var preview=Rect(panel,"Outfit preview",36,101,220,200);DrawAvatar(preview,selectedItem.id);preview.localScale=Vector3.one*.63f;preview.anchoredPosition=new Vector2(49,-104);
            Label(panel,"Selected item",selectedItem.name,232,102,208,70,23);Label(panel,"Selected terms",Model.S.owned.Contains(selected)?"Есть в коллекции":selectedItem.RewardDay>0?"Подарок календаря\nза день "+selectedItem.RewardDay:Number(selectedItem.price)+" монет\n"+(selectedItem.stars>0?selectedItem.stars+(selectedItem.stars==4?" звезды":" звёзд")+" · этап ":"Стадия ")+selectedItem.stage,232,174,208,65,22,"C9BCD8");
            Label(panel,"Item bonus",selectedItem.IsSet?"+2% забота / доход":selectedItem.bonus==0?"Для красоты":selectedItem.slot==0?"+2% за заботу":selectedItem.slot==1?"+2% к пассиву":"+2% к монетам",232,240,208,32,22,"55E6C1");
            string[] cats={"НАРЯДЫ","СЦЕНЫ","ЭФФЕКТЫ","ЭМОЦИИ","ОКРАСЫ"};int[] slots={0,2,3,4,5};
            for(int n=0;n<cats.Length;n++){int c=slots[n];Button(panel,"Category "+c,cats[n],24+n%3*148,294+n/3*60,136,60,()=>{category=c;pageIndex=0;selected=CategoryItems()[0].id;Show("shop");},c==category?"665781":"39315B",22);}
            Label(panel,"Set count",CapyCompleteSets.All.Length+" цельных\nнарядов",320,352,136,64,22,"55E6C1",TextAnchor.MiddleCenter);
            var items=CategoryItems();pageIndex=Mathf.Clamp(pageIndex,0,(items.Count-1)/4);
            for(int n=0;n<4;n++){int ix=pageIndex*4+n;if(ix>=items.Count)break;var item=items[ix];float x=24+n%2*222,y=420+n/2*118;
                var b=Button(panel,"Shop item "+item.id,"",x,y,210,108,()=>{selected=item.id;Show("shop");},item.id==selected?"665781":"39315B");
                ItemPicture(b.transform,item);Label(b.transform,"Name",item.name,76,6,125,67,22);
                Label(b.transform,"Owned",Model.S.equipped[item.slot]==item.id?"НАДЕТО":Model.S.owned.Contains(item.id)?"ОТКРЫТО":item.RewardDay>0?"ДЕНЬ "+item.RewardDay:Number(item.price)+" монет",12,75,187,30,22,"55E6C1");
            }
            Button(panel,"Previous","←",24,666,64,64,()=>{pageIndex=Math.Max(0,pageIndex-1);Show("shop");});Button(panel,"Next","→",392,666,64,64,()=>{pageIndex=Math.Min((items.Count-1)/4,pageIndex+1);Show("shop");});
            var balance=Label(panel,"Shop balance","",99,666,282,65,22,align:TextAnchor.MiddleCenter);liveLabels.Add(()=>balance.text=(pageIndex+1)+" / "+((items.Count+3)/4)+" · "+Number(Model.S.coins)+" монет\n"+Model.S.stars+" звёзд");
            var equip=Button(panel,"Equip or buy","",24,750,282,76,()=>{if(Model.Purchase(selected)){Save();Show("shop");}},"55E6C1",22);
            liveLabels.Add(()=>{bool own=Model.S.owned.Contains(selected);equip.GetComponentInChildren<Text>().text=own?"НАДЕТЬ":selectedItem.RewardDay>0?"ПОДАРОК ЗА ДЕНЬ "+selectedItem.RewardDay:"ОТКРЫТЬ";equip.interactable=own||selectedItem.RewardDay==0&&Model.Stage>=selectedItem.stage&&Model.S.coins>=selectedItem.price&&Model.S.stars>=selectedItem.stars;});
            Button(panel,"Remove item","СНЯТЬ",318,750,138,76,()=>{Model.S.equipped[category]=category==2?"moon":category==5?"classic":"";Save();Show("shop");},size:22);
        }
        List<IdleItem> CategoryItems(){var list=new List<IdleItem>();foreach(var i in IdleCatalog.Items)if(i.slot==category&&CapyCompleteSets.Visible(i))list.Add(i);return list;}
        void DailyPage(){
            Label(panel,"Calendar note","Заходи, когда удобно.\nПропущенные дни не сбрасывают календарь.",24,99,432,104,23,"C9BCD8",TextAnchor.MiddleCenter);
            int current=Model.S.dailyCount%7;
            for(int n=0;n<7;n++){float x=24+n%3*148,y=215+n/3*117;InkBox(panel,"Day "+n,x,y,136,103,n==current?"F3CF79":"EDDBB2",14,3);Gift(panel,x+12,y+12,29);Label(panel,"Day label","День "+(n+1),x+45,y+7,85,34,22);Label(panel,"Day reward",n==2?"+ сцена":n==6?"+ наряд":new[]{25,50,100,150,250,400,600}[n]+" монет",x+5,y+51,126,42,22,align:TextAnchor.MiddleCenter);}
            Label(panel,"Calendar bonus","День 3 · Сад светлячков\nДень 7 · Хранитель рассвета\nМонеты каждый день. В день 7\nещё 3 звезды и миска.",24,564,432,132,23,"C9BCD8",TextAnchor.MiddleCenter);
            var claim=Button(panel,"Claim daily","",24,711,432,76,()=>{if(Model.ClaimDaily(Date)){Save();Show("daily");Sound(evolveClip,.2f);}},"55E6C1");liveLabels.Add(()=>{bool today=string.CompareOrdinal(Model.S.dailyDate,Date)>=0;claim.interactable=!today&&Model.S.feeds>0;claim.GetComponentInChildren<Text>().text=today?"СЕГОДНЯ УЖЕ ПОЛУЧЕНО":Model.S.feeds==0?"СНАЧАЛА НАКОРМИ":"ЗАБРАТЬ ПОДАРОК";});
            Label(panel,"Daily reset","Новый день начинается в 00:00 UTC",24,794,432,36,22,"AEA2C6",TextAnchor.MiddleCenter);
        }
        void QuestPage(){
            PaperCard(panel,"Quest card",24,118,432,272);Star(panel,32,126,29,"F6CF78");Star(panel,419,126,29,"F6CF78");var title=Label(panel,"Quest name","",46,143,388,86,27,align:TextAnchor.MiddleCenter);var details=Label(panel,"Quest details","",46,240,388,121,24,"C9BCD8",TextAnchor.MiddleCenter);
            var claim=Button(panel,"Claim quest","ЗАБРАТЬ НАГРАДУ",24,415,432,72,()=>{if(Model.ClaimQuest()){Save();Show("quests");}},"55E6C1",23);
            liveLabels.Add(()=>{title.text=Model.QuestName;details.text=Model.S.quest>=40?"Ты настоящий мастер заботы":Math.Min(Model.QuestValue,Model.QuestGoal)+" / "+Model.QuestGoal+"\nНаграда: "+Number(Model.QuestReward)+" монет + 1 звезда\n+1 билет для дерева";claim.interactable=Model.QuestDone;});
            Label(panel,"Personal record","ТВОЯ ИСТОРИЯ\nКормлений: "+Model.S.feeds+" · Вещей: "+Model.S.owned.FindAll(id=>CapyCompleteSets.Visible(IdleCatalog.Find(id))).Count+" / "+System.Array.FindAll(IdleCatalog.Items,CapyCompleteSets.Visible).Length+"\nФорм: "+Model.S.forms.Count+" / 8\nЗаработано: "+Number(Model.S.totalCoins)+" монет",40,518,400,150,22,"FFF4D6");
            var visit=Button(panel,"Open visitor","",24,715,432,76,()=>Show("visitor"),"4B3C67",23);liveLabels.Add(()=>visit.GetComponentInChildren<Text>().text=Model.CanVisit?"В ГОСТЯХ · "+Model.VisitorName:"ДРУЗЬЯ КАПИБАРЫ");
        }
        void OfflinePage(){
            var avatar=Rect(panel,"Sleeping capy",151,135,240,250);DrawAvatar(avatar);avatar.localScale=Vector3.one*.75f;
            Label(panel,"Offline amount","+"+Number(offlineAward)+" монет",24,370,432,75,38,"55E6C1",TextAnchor.MiddleCenter);
            Label(panel,"Offline note","С возвращением! Капибара отдохнула.\n\n"+(Model.ReturnGift||welcomeGiftPending?"Подарок: ещё +25 монет!\n":Model.LastAbsence>=3600?"Все шкалы — не ниже 50%.\n":"")+"Учтено до 4 часов дохода.\nМонеты уже зачислены.",40,460,400,205,24,"C9BCD8",TextAnchor.MiddleCenter);
            Button(panel,"Continue idle","К КАПИБАРЕ",24,730,432,76,()=>{welcomeOfflinePending=welcomeGiftPending=false;offlineAward=0;Model.Wake();Show("home");BeginCareReaction(6);},"55E6C1",25);
        }
        void SettingsPage(){
            Button(panel,"Sound setting","ЗВУКИ: "+(Model.S.sound?"ВКЛ.":"ВЫКЛ."),24,157,432,76,()=>{Model.S.sound=!Model.S.sound;Save();Show("settings");});
            Button(panel,"Motion setting","МЕНЬШЕ ДВИЖЕНИЯ: "+(Model.S.calm?"ВКЛ.":"ВЫКЛ."),24,258,432,76,()=>{Model.S.calm=!Model.S.calm;Save();Show("settings");},size:22);
            Button(panel,"Meme setting","МЕМНЫЙ ВИД: "+(Model.S.memeVisuals?"ВКЛ.":"ВЫКЛ."),24,359,432,76,()=>{Model.S.memeVisuals=!Model.S.memeVisuals;Save();Show("settings");},size:22);
            Button(panel,"Surprise ads setting","ВИДЕО ДЛЯ ДЕРЕВА: "+(Model.S.surpriseAds?"ВКЛ.":"ВЫКЛ."),24,454,432,64,()=>{Model.S.surpriseAds=!Model.S.surpriseAds;Save();Show("settings");},size:22);
            Label(panel,"Settings info","Бонус таланта сохраняется\nпри выключении мемного вида.\nЗабота доступна бесплатно.\nЯзык: русский.\nПрогресс хранится на устройстве.",32,524,416,214,22,"C9BCD8",TextAnchor.MiddleCenter);
            Button(panel,"Settings done","ГОТОВО",24,750,432,76,()=>Show("home"),"55E6C1",25);
        }
    }
}










