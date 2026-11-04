using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  bool ownedOnly;
  string wardrobeReturn="home",dailyRewardItem="";
  int dailyRewardDay,dailyRewardCoins;
  SoftShape dailyBadge,questBadge;
  Button nextGoal;
  Text goalTitle,goalDetail;
  string goalRoute;

  string ParentPage => Page=="mandarin-stock"?"food":Page.StartsWith("mandarin-")?"mandarins":Page=="activities"?activityReturn:Page.StartsWith("activity-")?"activities":Page=="food"?"care":Page=="visitor"?"quests":Page=="daily-reward"?"daily":Page=="shop"?wardrobeReturn:Page=="surprise-pool"?"surprise-odds":Page.StartsWith("surprise-")?"surprises":"home";
  void GoBack(){if(currencyChoice){CloseCurrencyChoice();return;}if(sectionHelp){CloseSectionHelp();return;}if(Page=="activity-play"){Show("activity-exit");return;}if(Page=="activity-exit"){Show("activity-play");return;}if(Page=="welcome")BeginFromWelcome();else Show(ParentPage);}
  void OpenWardrobe(string id,string returnTo="home"){
   var item=IdleCatalog.Find(id);if(item==null)return;
   ownedOnly=false;wardrobeReturn=returnTo;category=item.slot;selected=id;
   pageIndex=Math.Max(0,CategoryItems().FindIndex(i=>i.id==id))/4;Show("shop");
  }
  bool DailyReady=>Model.S.feeds>0&&string.CompareOrdinal(Model.S.dailyDate,Date)<0;
  void BuildHomeSignals(){
   dailyBadge=Oval(home,"Daily ready badge",436,758,26,26,"F4CF80",2);Mark(dailyBadge.transform,"!",1,1,24,24,22,Ink);
   questBadge=Oval(home,"Quest ready badge",288,758,26,26,"F4CF80",2);Mark(questBadge.transform,"!",1,1,24,24,22,Ink);
   nextGoal=Button(home,"Next goal","",24,230,432,60,OpenNextGoal,"FFFFFF00");
   goalTitle=Label(nextGoal.transform,"Goal title","",10,0,386,30,22,align:TextAnchor.MiddleCenter);goalTitle.font=bold;
   goalDetail=Label(nextGoal.transform,"Goal detail","",10,29,386,30,21,"72543A",TextAnchor.MiddleCenter);
   Label(nextGoal.transform,"Goal arrow","›",400,7,22,44,28,"418355",TextAnchor.MiddleCenter);
  }
  void RefreshHomeSignals(){
   if(dailyBadge)dailyBadge.gameObject.SetActive(DailyReady);
   if(questBadge)questBadge.gameObject.SetActive(Model.QuestDone);
   RefreshNextGoal();
  }
  void RefreshNextGoal(){
   if(!nextGoal)return;
   bool visible=noticeAge>=4&&!BathInteractive&&!Model.S.resting&&!IsSleeping;
   nextGoal.gameObject.SetActive(visible);notice.gameObject.SetActive(!visible);
   if(!visible){goalRoute="";return;}
   int step=Model.FirstSessionStep;string title,detail;
   switch(step){
    case 0:title="1/5 · Первое знакомство";detail="Тапни по капибаре";goalRoute="pet";break;
    case 1:title="2/5 · Бесплатная миска";detail="Накорми — малыш подрастёт";goalRoute="food";break;
    case 2:title="3/5 · Своя подстилка";detail=Model.CanBed?"10 монет · доход без тапов":"Ещё "+Number(Math.Ceiling(Model.BedPrice-Model.S.coins))+" монет · тапай по капибаре";goalRoute="food";break;
    case 3:title="4/5 · Усиль свой тап";detail=Model.CanUpgradeTap(0)?"Ловкие лапки · 25 монет":"Ещё "+Number(Math.Ceiling(Model.TapPrice(0)-Model.S.coins))+" монет до улучшения";goalRoute="tap";break;
    case 4:title="5/5 · Первое занятие";detail=Model.S.activity!=null&&!Model.S.activity.complete?"Продолжи игру с капибарой":"Короткая игра — награда монетами";goalRoute="activities";break;
    default:
     if(Model.S.fullness<35||Model.S.comfort<35||Model.S.mood<35){title="Минутка заботы";detail="Листочек, купание и игра · от 5 монет";goalRoute="care";}
     else if(Model.PendingGrowthGifts>0){title="Подарок за рост готов";detail="Забери монеты за рост";goalRoute="growth";}
     else if(Model.QuestDone){title="Поручение выполнено";detail="Забери награду за свои успехи";goalRoute="quests";}
     else if(DailyReady){title="Тебя ждёт подарок дня";detail="Загляни в календарь";goalRoute="daily";}
     else if(Model.Stage<IdleModel.StageCount){title="Следующий этап · "+(Model.Stage+1)+"/5";detail="Выбери еду для роста";goalRoute="food";}
     else {title="Укрась свой сад";detail="Примерь наряд или новую сцену";goalRoute="shop";}
     break;
   }
   goalTitle.text=title;goalDetail.text=detail;
  }
  void OpenNextGoal(){
   if(string.IsNullOrEmpty(goalRoute))return;
   if(goalRoute=="pet"){Care();return;}
   if(goalRoute=="activities"){OpenActivities("home");return;}
   if(goalRoute=="food")foodPage=0;
   if(goalRoute=="shop"){wardrobeReturn="home";ownedOnly=false;category=0;pageIndex=0;selected="";}
   Show(goalRoute);
  }
  void ShopPage(){
   var items=CategoryItems();pageIndex=Mathf.Clamp(pageIndex,0,Math.Max(0,(items.Count-1)/4));
   var item=items.Find(i=>i.id==selected);if(item==null&&items.Count>0)item=items[0];selected=item==null?"":item.id;
   PaperCard(panel,"Preview card",24,99,432,178);
   if(item!=null){
    ScenePreview(panel,item);
    var preview=Rect(panel,"Outfit preview",49,104,220,200);preview.localScale=Vector3.one*.63f;DrawAvatar(preview,item.id);
    Label(panel,"Selected item",item.name,228,102,214,70,24).font=bold;
    bool own=Model.S.owned.Contains(item.id),worn=Model.S.equipped[item.slot]==item.id;
    Shape(panel,"Selected status backing",226,182,218,39,own?"DFE9C6":"F3DFB1",12);
    Label(panel,"Selected terms",worn?"Надето":own?"Есть у тебя":item.TreeOnly?"Из дерева":item.RewardDay>0?"Подарок":Number(item.price)+" монет",232,183,206,36,24,own?"365D35":Ink,TextAnchor.MiddleCenter).font=bold;
    Label(panel,"Item bonus",worn?"Текущий образ":own?"Нажми «Надеть»":item.TreeOnly?IdleModel.RarityNames[item.TreeTier]:item.RewardDay>0?"За день "+item.RewardDay:Model.Stage<item.stage?"Со стадии "+item.stage:"Выбор оплаты",228,231,214,34,24,"72543A",TextAnchor.MiddleCenter);
   }else{
    Gift(panel,58,119,112);Label(panel,"Empty wardrobe","Твоя коллекция",191,109,246,60,25).font=bold;
    Label(panel,"Empty wardrobe hint","Выбирай во вкладке\n«Все вещи»",191,171,246,67,24,"72543A");
   }
   string[] cats={"Наряд","Сцена","Эффект","Эмоция","Окрас"};int[] slots={0,2,3,4,5};
   for(int n=0;n<cats.Length;n++){
    int c=slots[n];var tab=Button(panel,"Category "+c,"",24+n%3*148,282+n/3*66,136,66,()=>{category=c;pageIndex=0;selected="";Show("shop");},c==category?"665781":"39315B",24);
    var art=Rect(tab.transform,"Category picture",5,17,80,80);art.localScale=Vector3.one*.42f;GuideArt(art,new[]{"hat","scene","sparkles","heart","fur"}[n]);
    Label(tab.transform,"Category caption",cats[n],40,14,92,38,22,align:TextAnchor.MiddleCenter).font=bold;
   }
   Button(panel,"Owned filter",ownedOnly?"Мои":"Все вещи",320,348,136,66,()=>{ownedOnly=!ownedOnly;pageIndex=0;Show("shop");},ownedOnly?"55E6C1":"39315B",24);
   for(int n=0;n<4;n++){int ix=pageIndex*4+n;if(ix>=items.Count)break;var cardItem=items[ix];float x=24+n%2*222,y=420+n/2*138;
    bool cardOwned=Model.S.owned.Contains(cardItem.id),cardWorn=Model.S.equipped[cardItem.slot]==cardItem.id,cardSelected=cardItem.id==selected;
    var b=Button(panel,"Shop item "+cardItem.id,"",x,y,210,134,()=>{selected=cardItem.id;Show("shop");},cardSelected?"F4CE74":"F5E4BB");
    Label(b.transform,"Name",cardItem.name,7,0,196,62,22,align:TextAnchor.MiddleCenter).font=bold;
    var thumb=Rect(b.transform,"Compact thumbnail",0,51,78,78);thumb.localScale=Vector3.one*1.05f;ItemPicture(thumb,cardItem);
    bool priced=!cardOwned&&!cardItem.TreeOnly&&cardItem.RewardDay==0;
    string status=cardWorn?"Надето":cardOwned?"Есть":cardItem.TreeOnly?"Дерево":cardItem.RewardDay>0?"День "+cardItem.RewardDay:Number(cardItem.price);
    Label(b.transform,"Owned",status,priced?112:83,77,priced?89:119,38,22,cardOwned?"365D35":Ink,TextAnchor.MiddleCenter).font=bold;
    if(priced)Coin(b.transform,85,82,28);
   }
   if(items.Count==0)Label(panel,"Empty category","В этой категории пока нет\nоткрытых вещей.\n\nНаряды, сцены и украшения\nсохраняются в твоей коллекции.",38,436,404,198,24,"C9BCD8",TextAnchor.MiddleCenter);
   Button(panel,"Previous","←",24,694,66,66,()=>{pageIndex--;Show("shop");}).interactable=pageIndex>0;
   Button(panel,"Next","→",390,694,66,66,()=>{pageIndex++;Show("shop");}).interactable=(pageIndex+1)*4<items.Count;
   var balance=Label(panel,"Shop balance","",99,694,282,64,22,align:TextAnchor.MiddleCenter);
   liveLabels.Add(()=>balance.text=(items.Count==0?"Нет вещей":(pageIndex+1)+" / "+((items.Count+3)/4)+" · "+items.Count+" вещей")+"\n"+Number(Model.S.coins)+" монет");
   var equip=Button(panel,"Equip or buy","",24,764,282,72,()=>{if(item==null)return;System.Action<bool> purchase=m=>{if(Model.Purchase(item.id,m)){
    Save();Sound(purchaseClip,.26f);Show("shop");
    var pose=panel.Find("Outfit preview") as RectTransform;
    if(pose&&!Model.S.calm){pose.localEulerAngles=new Vector3(0,0,-3);tweens.Track(pose.DOLocalRotate(Vector3.zero,.5f).SetEase(Ease.OutSine),pose.gameObject);}
   }};if(Model.S.owned.Contains(item.id))purchase(false);else ChooseCurrency(item.name,item.price,purchase);},"55E6C1",22);
   liveLabels.Add(()=>{
    bool own=item!=null&&Model.S.owned.Contains(item.id),worn=item!=null&&Model.S.equipped[item.slot]==item.id;
    equip.GetComponentInChildren<Text>().text=item==null?"ВЫБЕРИ ВЕЩЬ":worn?"УЖЕ НАДЕТО":own?"НАДЕТЬ":item.TreeOnly?"ТОЛЬКО ИЗ ДЕРЕВА":item.RewardDay>0?"ПОДАРОК ЗА ДЕНЬ "+item.RewardDay:Model.Stage<item.stage?"СО СТАДИИ "+item.stage:!Model.CanPayEither(item.price)?"НЕ ХВАТАЕТ МОНЕТ":"ОТКРЫТЬ И НАДЕТЬ";
    equip.interactable=item!=null&&!worn&&(own||!item.TreeOnly&&item.RewardDay==0&&Model.Stage>=item.stage&&Model.CanPayEither(item.price));
   });
   var remove=Button(panel,"Remove item","СНЯТЬ",318,764,138,72,()=>{Model.S.equipped[category]=category==2?"moon":category==5?"classic":"";Save();Show("shop");},size:22);
   remove.interactable=!string.IsNullOrEmpty(Model.S.equipped[category])&&Model.S.equipped[category]!=(category==2?"moon":category==5?"classic":"");
  }
  void ClaimCalendar(){
   int day=Model.S.dailyCount%7+1;bool scene=Model.S.owned.Contains("firefly_garden"),outfit=Model.S.owned.Contains("set_dawn");
   if(!Model.ClaimDaily(Date))return;
   dailyRewardDay=day;dailyRewardCoins=new[]{25,50,100,150,250,400,675}[day-1];
   dailyRewardItem=!outfit&&Model.S.owned.Contains("set_dawn")?"set_dawn":!scene&&Model.S.owned.Contains("firefly_garden")?"firefly_garden":"";
   Save();Sound(evolveClip,.2f);Show("daily-reward");
  }
  void DailyPage(){
   Label(panel,"Calendar note","Подарок каждый день.\nПропуски ничего не отнимают.",24,99,432,78,22,"C9BCD8",TextAnchor.MiddleCenter);
   bool today=string.CompareOrdinal(Model.S.dailyDate,Date)>=0;int current=today?Math.Max(0,Model.S.dailyCount-1)%7:Model.S.dailyCount%7;
   for(int n=0;n<7;n++){
    float x=n==6?24:24+n%3*148,y=n==6?403:184+n/3*109,w=n==6?432:136;
    bool collected=n<current||(today&&n==current);
    InkBox(panel,"Day "+n,x,y,w,99,n==current?"F3CF79":collected?"DFE9C6":"EDDBB2",14,3);
    if(n==6){var p=Rect(panel,"Calendar outfit preview",40,y-3,240,250);p.localScale=Vector3.one*.37f;DrawAvatar(p,"set_dawn");Label(panel,"Day label","День 7 · Особый наряд",143,y+3,302,37,22).font=bold;Label(panel,"Day reward","675 монет · наряд\nи праздничная миска",143,y+38,302,61,21);}
    else{Gift(panel,x+9,y+8,27);Label(panel,"Day label","День "+(n+1),x+40,y+4,90,35,22);Label(panel,"Day reward",n==2?"100 монет\n+ сцена":new[]{25,50,100,150,250,400}[n]+" монет",x+5,y+36,126,61,22,align:TextAnchor.MiddleCenter);}
    if(collected){Oval(panel,"Collected seal "+n,x+w-25,y-5,27,27,"A9C684",1.5f);Mark(panel,"✓",x+w-24,y-5,25,27,21,"365D35");}
   }
   PaperCard(panel,"Calendar scene card",24,516,432,169);
   var clip=Rect(panel,"Calendar scene preview",34,527,162,145);clip.gameObject.AddComponent<RectMask2D>();var scenery=Rect(clip,"Calendar scene",-14,-125,480,850);scenery.localScale=Vector3.one*.4f;DrawScene(scenery,IdleCatalog.Find("firefly_garden"));
   Label(panel,"Calendar bonus","ДЕНЬ 3\nСад светлячков",211,529,229,75,24).font=bold;
   Label(panel,"Calendar promise","Твоя сцена.\nНавсегда в саду.",211,610,229,64,22,"C9BCD8");
   var claim=Button(panel,"Claim daily","",24,711,432,76,ClaimCalendar,"55E6C1");
   liveLabels.Add(()=>{bool done=string.CompareOrdinal(Model.S.dailyDate,Date)>=0;claim.interactable=DailyReady;claim.GetComponentInChildren<Text>().text=done?"СЕГОДНЯ УЖЕ ПОЛУЧЕНО":Model.S.feeds==0?"СНАЧАЛА НАКОРМИ":"ЗАБРАТЬ ПОДАРОК";});
   var reset=Label(panel,"Daily reset","",24,794,432,36,22,"AEA2C6",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>{var wait=DateTime.UtcNow.Date.AddDays(1)-DateTime.UtcNow;reset.text=today?"Новый подарок через "+(int)wait.TotalHours+" ч "+wait.Minutes+" мин":"Один подарок в день · без спешки";});
  }
  void DailyRewardPage(){
   Label(panel,"Daily reward heading","ПОДАРОК ЗА ДЕНЬ "+dailyRewardDay,24,110,432,54,28,align:TextAnchor.MiddleCenter).font=bold;
   var art=Rect(panel,"Daily reward art",110,181,260,260);Oval(art,"Reward halo",7,9,246,231,"DFE9C6",2);
   if(dailyRewardItem=="set_dawn"){var pet=Rect(art,"Calendar new outfit",16,-5,240,250);DrawAvatar(pet,dailyRewardItem);}
   else if(dailyRewardItem=="firefly_garden"){var clip=Rect(art,"Calendar new scene",17,25,226,195);clip.gameObject.AddComponent<RectMask2D>();var scene=Rect(clip,"New scene",-7,-170,480,850);scene.localScale=Vector3.one*.5f;DrawScene(scene,IdleCatalog.Find(dailyRewardItem));}
   else Gift(art,44,39,174);
   if(!Model.S.calm){art.pivot=new Vector2(.5f,.5f);art.anchoredPosition+=new Vector2(130,-130);art.localScale=Vector3.one*.75f;tweens.Track(art.DOScale(1,.48f).SetEase(Ease.OutBack),art.gameObject);}
   Label(panel,"Daily reward coins","+"+dailyRewardCoins+" монет",24,451,432,54,33,"418355",TextAnchor.MiddleCenter).font=bold;
   string note=dailyRewardItem.Length>0?"Открыто: "+IdleCatalog.Find(dailyRewardItem).name:dailyRewardDay==7?"Праздничная миска":"Спасибо, что заглянул в сад!";
   if(dailyRewardItem.Length>0&&dailyRewardDay==7)note+="\nПраздничная миска";
   Label(panel,"Daily reward detail",note+"\nУже сохранено в твоём саду",30,512,420,115,24,"C9BCD8",TextAnchor.MiddleCenter);
   Button(panel,"View daily reward",dailyRewardItem.Length>0?"ПОСМОТРЕТЬ ПОДАРОК":"К КАПИБАРЕ",24,659,432,72,()=>{if(dailyRewardItem.Length>0)OpenWardrobe(dailyRewardItem,"daily");else Show("home");},"55E6C1",24);
   Button(panel,"Daily reward return","К КАЛЕНДАРЮ",24,751,432,72,()=>Show("daily"),size:23);
  }
  void QuestPage(){
   PaperCard(panel,"Quest card",24,118,432,272);Star(panel,32,126,29,"F6CF78");Star(panel,419,126,29,"F6CF78");
   var taskArt=Rect(panel,"Quest picture",42,149,80,80);taskArt.localScale=Vector3.one*.7f;GuideArt(taskArt,Model.QuestType==0?"paw":Model.QuestType==3?"hat":Model.QuestType==4?"growth":"food");
   var title=Label(panel,"Quest name","",113,143,321,76,25,align:TextAnchor.MiddleCenter);title.font=bold;
   Shape(panel,"Quest track",48,231,384,12,"CABB90",6);var fill=Shape(panel,"Quest progress",48,231,1,12,"79A957",6);
   var details=Label(panel,"Quest details","",46,258,388,117,23,"C9BCD8",TextAnchor.MiddleCenter);
   var claim=Button(panel,"Claim quest","ЗАБРАТЬ НАГРАДУ",24,412,432,72,()=>{if(Model.ClaimQuest()){Save();Sound(evolveClip,.18f);Show("quests");}},"55E6C1",23);
   var go=Button(panel,"Quest action","",24,499,432,64,()=>{if(Model.QuestType==3){ownedOnly=false;wardrobeReturn="quests";category=0;pageIndex=0;selected="";Show("shop");}else Show(Model.QuestType==0?"home":"food");},size:23);
   liveLabels.Add(()=>{
    title.text=Model.S.quest>=40?"Все поручения выполнены":Model.QuestType==0?"Тапни по капибаре":Model.QuestName;
    details.text=Model.S.quest>=40?"Ты настоящий мастер заботы":Math.Min(Model.QuestValue,Model.QuestGoal)+" / "+Model.QuestGoal+"\n"+Number(Model.QuestReward)+" монет\n1 билет для дерева";
    fill.rectTransform.sizeDelta=new Vector2(384*(Model.S.quest>=40?1:Mathf.Clamp01((float)Model.QuestValue/Model.QuestGoal)),12);claim.interactable=Model.QuestDone;
    go.gameObject.SetActive(!Model.QuestDone&&Model.S.quest<40);go.GetComponentInChildren<Text>().text=new[]{"К КАПИБАРЕ","НАКОРМИТЬ","УЛУЧШИТЬ ПОДСТИЛКУ","В ГАРДЕРОБ","ЕДА ДЛЯ РОСТА"}[Model.QuestType];
   });
   Label(panel,"Personal record","ТВОЯ ИСТОРИЯ\nКормлений: "+Model.S.feeds+" · Подарков: "+Model.S.dailyCount+"\nВещей: "+Model.S.owned.FindAll(id=>CapyCompleteSets.Visible(IdleCatalog.Find(id))).Count+" / "+Array.FindAll(IdleCatalog.Items,CapyCompleteSets.Visible).Length+"\nЗаработано: "+Number(Model.S.totalCoins)+" монет",40,590,400,132,22);
  }
 }
}
