using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if PLUGIN_YG_2
using YG;
#endif
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  bool surpriseAdRequest;int surpriseResultIndex,surpriseShelf,surpriseTier=1;Tween surpriseReveal;
  void SurprisePage(){
   Model.RefreshSurprises(Date);
   var balance=Label(panel,"Surprise balance","",26,94,428,37,24,"513821",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>balance.text=Number(Model.S.coins)+" монет · "+Model.S.stars+" звёзд");
   var tree=Rect(panel,"Mandarin surprise tree",115,137,278,205);tree.localScale=Vector3.one*.9f;DrawSurpriseTree(tree);
   Shape(panel,"Guarantee track",85,325,310,9,"CCB78C",4);
   var guarantee=Shape(panel,"Guarantee fill",85,325,1,9,"87AC65",4);
   liveLabels.Add(()=>guarantee.rectTransform.sizeDelta=new Vector2(310*Model.S.surprisePity/10f,9));
   var pity=Label(panel,"Surprise guarantee","",28,342,424,60,22,"513821",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>pity.text=Model.Stage<2?"Дерево откроется на стадии 2":Model.SurpriseWait>0?"Подарки сохранены. Отдохнём · "+Minutes(Model.SurpriseWait):"Эпический или лучше — не позднее\nчем через "+(10-Model.S.surprisePity)+" открытий");
   var ticket=Button(panel,"Open surprise ticket","",24,408,432,64,()=>OpenGarden(1,true),"55E6C1",23);
   liveLabels.Add(()=>{ticket.GetComponentInChildren<Text>().text="ОТКРЫТЬ ЗА БИЛЕТ · "+Model.S.surpriseTickets;ticket.interactable=Model.CanOpenSurprise(1,true);});
   var one=Button(panel,"Open surprise one","",24,482,210,64,()=>OpenGarden(1,false),"F4CF80",22);
   var ten=Button(panel,"Open surprise ten","",246,482,210,64,()=>OpenGarden(10,false),"F4CF80",22);
   liveLabels.Add(()=>{one.GetComponentInChildren<Text>().text="1 · "+Number(Model.SurprisePrice)+" монет";ten.GetComponentInChildren<Text>().text="10 · "+Number(Model.SurprisePrice*10)+" монет";one.interactable=Model.CanOpenSurprise(1);ten.interactable=Model.CanOpenSurprise(10);});
   var limits=Label(panel,"Surprise spending limit","",28,550,424,66,22,"72543A",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>limits.text="На сегодня: "+Number(Model.SurpriseBudgetLeft)+" монет\nОткрытий за монеты: "+Model.SurpriseLeft+" / 10");
   Button(panel,"Surprise free gifts","БИЛЕТЫ И РОСТКИ",24,624,432,62,()=>Show("surprise-gifts"),"55E6C1",23);
   Button(panel,"Surprise chances","ШАНСЫ",24,696,210,62,()=>Show("surprise-odds"),size:22);
   Button(panel,"Surprise crafting","ЗА ЗВЁЗДЫ",246,696,210,62,()=>{surpriseShelf=0;Show("surprise-craft");},size:22);
   Button(panel,"Surprise collection","МОИ НАХОДКИ",24,768,432,62,()=>Show("surprise-results"),size:23);
  }
  static void DrawSurpriseTree(Transform p){
   Shape(p,"Tree shadow",17,178,245,23,"65432D20",0,true);
   Stroke(p,"Tree trunk edge",140,180,133,72,"65432D",26);Stroke(p,"Tree trunk",140,177,133,72,"BD895B",18);
   Stroke(p,"Tree branch",138,121,76,73,"9B704B",12);Stroke(p,"Tree branch",139,126,205,74,"9B704B",12);
   for(int n=0;n<5;n++){float x=18+n*40,y=19+Mathf.Abs(n-2)*14;Oval(p,"Round canopy",x,y,85,84,n%2==0?"91B66C":"ACC780",2);}
   Leaf(p,95,8,41,22,"BDD68B",-14);Leaf(p,157,6,38,22,"CCE09D",13);
   for(int n=0;n<3;n++){float x=53+n*67,y=61+n%2*18;Oval(p,"Mandarin capsule",x,y,35,35,"EAB067",2);Shape(p,"Capsule shine",x+7,y+7,10,5,"FFE3A1",3);Leaf(p,x+14,y-6,19,11,"6B9556",15);}
   InkBox(p,"Tree planter",100,156,80,39,"D39A70",13,3);InkBox(p,"Planter lip",94,151,92,14,"E7B58A",5,2);
   Star(p,128,166,22,"F9D987");Flower(p,18,163,"EAAEB0");Flower(p,241,163,"F6DB8D");
  }
  void OpenGarden(int count,bool ticket){
   Model.RefreshSurprises(Date);var rewards=Model.OpenSurprises(count,ticket);if(rewards==null)return;
   Save();surpriseResultIndex=0;Show("surprise-results");
   // The outcome is already committed. Skipping or closing animation cannot reroll it.
   var reveal=panel.Find("Reward reveal") as RectTransform;
   if(reveal&&!Model.S.calm){reveal.localScale=Vector3.one*.7f;surpriseReveal=tweens.Track(reveal.DOScale(1,1.2f).SetEase(Ease.OutBack),reveal.gameObject);
    for(int n=0;n<8;n++){float angle=n*Mathf.PI*.25f;var spark=Rect(reveal,"Opening sparkle",116,94,18,18);Star(spark,0,0,18,"FFE0A1");
     tweens.Track(spark.DOAnchorPos(new Vector2(116+Mathf.Cos(angle)*112,-94+Mathf.Sin(angle)*91),.9f).SetEase(Ease.OutCubic),reveal.gameObject);
     tweens.Track(spark.DOScale(0,.4f).SetDelay(.8f),reveal.gameObject);
    }
   }
   Sound(evolveClip,.2f);
  }
  void SurpriseResultsPage(){
   var rewards=Model.S.surpriseResults;
   if(rewards.Count>0){
    surpriseResultIndex=Mathf.Clamp(surpriseResultIndex,0,rewards.Count-1);var r=rewards[surpriseResultIndex];int tier=Mathf.Clamp(r.tier,0,3);
    Label(panel,"Reward rarity",IdleModel.RarityNames[tier]+" подарок",30,103,420,42,27,align:TextAnchor.MiddleCenter).font=bold;
    var reveal=Rect(panel,"Reward reveal",116,156,248,210);Oval(reveal,"Reward halo",12,5,224,194,IdleModel.RarityColors[tier],3);
    var item=IdleCatalog.Find(r.item);var form=IdleModel.Form(r.form);
    if(item!=null&&(item.IsSet||item.slot==5)){var portrait=Rect(reveal,"Gift illustration",34,12,240,250);portrait.localScale=Vector3.one*.72f;DrawAvatar(portrait,item.id);}
    else if(item!=null){var portrait=Rect(reveal,"Gift illustration",42,25,90,90);portrait.localScale=Vector3.one*2;ItemPicture(portrait,item);}
    else if(form!=null){var portrait=Rect(reveal,"Gift form",42,28,90,90);portrait.localScale=Vector3.one*3;FormPortrait(portrait,form.art,form.color);}
    else Gift(reveal,66,47,117);
    Label(panel,"Reward name",r.name,30,374,420,69,26,align:TextAnchor.MiddleCenter);
    Label(panel,"Reward credited",(r.duplicate?"Повтор → ":"")+"+"+r.stars+" звёзд"+(r.coins>0?" · +"+Number(r.coins)+" монет":"")+"\nУже сохранено",30,447,420,65,23,"418355",TextAnchor.MiddleCenter);
    var prev=Button(panel,"Previous reward","←",24,525,64,64,()=>{surpriseResultIndex--;Show("surprise-results");});prev.interactable=surpriseResultIndex>0;
    var next=Button(panel,"Next reward","→",392,525,64,64,()=>{surpriseResultIndex++;Show("surprise-results");});next.interactable=surpriseResultIndex<rewards.Count-1;
    Label(panel,"Reward position",(surpriseResultIndex+1)+" / "+rewards.Count,101,525,278,64,24,align:TextAnchor.MiddleCenter);
   }else Label(panel,"No recent finds","Здесь появятся твои находки.\nОткрытые вещи уже в гардеробе.",34,195,412,170,25,align:TextAnchor.MiddleCenter);
   Button(panel,"Surprise wardrobe","В ГАРДЕРОБ",24,611,432,64,()=>Show("shop"),"55E6C1",23);
   Button(panel,"Cycle talent","ТАЛАНТ: "+(IdleModel.Form(Model.S.form)?.name??"Обычный"),24,686,432,64,()=>{var ids=new List<string>{""};ids.AddRange(Model.S.forms);int ix=ids.IndexOf(Model.S.form);Model.SelectForm(ids[(ix+1)%ids.Count]);Save();Show("surprise-results");},size:22);
   Button(panel,"Return to tree","К ДЕРЕВУ",24,762,432,64,()=>Show("surprises"),"55E6C1",24);
  }
  void SurpriseGiftsPage(){
   Label(panel,"Free gifts note","Подарки без спешки.\nПропуск дня ничего не отнимает.",30,102,420,78,24,align:TextAnchor.MiddleCenter);
   var daily=Button(panel,"Daily tree ticket","",24,192,432,64,()=>{if(Model.ClaimSurpriseTicket(Date)){Save();Show("surprise-gifts");}},"55E6C1",23);
   liveLabels.Add(()=>{daily.interactable=Model.CanDailySurprise(Date);daily.GetComponentInChildren<Text>().text=Model.Stage<2?"ДЕРЕВО СО СТАДИИ 2":daily.interactable?"БИЛЕТ ДНЯ · БЕСПЛАТНО":"БИЛЕТ ДНЯ УЖЕ ПОЛУЧЕН";});
   Button(panel,"Quest tree tickets","ПОРУЧЕНИЯ · +1 БИЛЕТ",24,268,432,64,()=>Show("quests"),size:23);
   Label(panel,"Seed explanation","ТРИ МАНДАРИНОВЫХ РОСТКА\nВыбери один: каждый даёт подарок.\nБез ставки · один раз в день",28,349,424,101,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<3;n++){int choice=n;var b=Button(panel,"Seed "+n,"",24+n*148,461,136,92,()=>{if(Model.ChooseSeed(Date,choice)!=null){Save();surpriseResultIndex=0;Show("surprise-results");Sound(feedClip,.18f);}},"F4CF80");Gift(b.transform,48,9,38);Label(b.transform,"Seed number",(n+1).ToString(),10,49,116,34,23,align:TextAnchor.MiddleCenter);liveLabels.Add(()=>b.interactable=Model.CanChooseSeed(Date));}
   Label(panel,"Seed chances","По 1/3: монеты, миска или 3 звезды.\nМонеты и миска: ещё +1 звезда.",28,559,424,69,22,"72543A",TextAnchor.MiddleCenter);
   var ad=Button(panel,"Surprise video ticket","",24,640,432,64,RequestSurpriseAd,"55E6C1",22);
   liveLabels.Add(()=>{double wait=Model.AutoAdWait(Utc);ad.interactable=Model.S.surpriseAds&&Model.CanAdSurprise(Date)&&wait<=0&&RewardedAvailable&&!adPending;ad.GetComponentInChildren<Text>().text=!Model.S.surpriseAds?"ВИДЕО ДЛЯ ДЕРЕВА ВЫКЛЮЧЕНО":!Model.CanAdSurprise(Date)?"ВИДЕО-БИЛЕТ УЖЕ ПОЛУЧЕН":wait>0?"ВИДЕО ЧЕРЕЗ "+Minutes(wait):"ВИДЕО → 1 БИЛЕТ";});
   Label(panel,"Video limits","Не более одного видео-билета в день.\nШансы такие же, как за монеты.",28,710,424,65,22,"72543A",TextAnchor.MiddleCenter);
   Button(panel,"Gift return","К ДЕРЕВУ",24,778,432,60,()=>Show("surprises"),size:23);
  }
  void RequestSurpriseAd(){
   if(Model==null||PresentationPaused||!RewardedAvailable)return;
#if PLUGIN_YG_2
   if(YG2.nowAdsShow)return;
#endif
   autoRequestSerial=Model.BeginSurpriseAd(Utc,Date);if(autoRequestSerial==0)return;
   surpriseAdRequest=true;autoAdId="capy_tree_"+autoRequestSerial;adCredited=false;adPending=true;adRequestedAt=Time.realtimeSinceStartupAsDouble;Release();Save();
#if PLUGIN_YG_2
   try{YG2.RewardedAdvShow(autoAdId);}catch(Exception){AutoAdError();}
#else
   AutoAdError();
#endif
  }
  void SurpriseOddsPage(){
   Label(panel,"Odds introduction","Шансы — параметры первой версии.\nКаждое открытие даёт +1 звезду.",28,98,424,75,23,align:TextAnchor.MiddleCenter);
   string[] odds={"60% · монеты / миска / +10% тапа","25% · эффект или эмоция","12% · окрас","3% · наряд или талант"};
   for(int n=0;n<4;n++){int tier=n;float y=185+n*83;var b=Button(panel,"Odds tier "+n,"",24,y,432,74,()=>{surpriseTier=tier;surpriseShelf=0;Show("surprise-pool");},IdleModel.RarityColors[n]);Label(b.transform,"Tier name",IdleModel.RarityNames[n],14,1,405,33,24);Label(b.transform,"Tier odds",odds[n],14,36,405,34,22);}
   Label(panel,"Guarantee explanation","На 10-м открытии без эпического:\n97% эпический, 3% легендарный.\nУспех сбрасывает счётчик.\nПовторы: +2 / +5 / +10 звёзд.\nПокупка за звёзды — без случайности.",28,526,424,172,22,"72543A",TextAnchor.MiddleCenter);
   Label(panel,"Wallet protection","Бюджет дня — 20% монет при входе\nк дереву. Остаток не меньше 100.\nПосле 10 подарков — пауза 2 минуты.",28,701,424,101,22,"72543A",TextAnchor.MiddleCenter);
  }
  void SurprisePoolPage(){
   var pool=Model.SurprisePool(surpriseTier);var entries=new List<string>();
   if(surpriseTier==0)entries.AddRange(new[]{"20% · вернуть 25% цены в монетах","20% · праздничная миска","20% · +10% тапа на 15 минут"});
   else {double group=surpriseTier==1?25:surpriseTier==2?12:3;if(surpriseTier==3&&Model.Stage>=3)group=1.5;
    foreach(var item in pool)entries.Add(item.name+"\n"+(group/Math.Max(1,pool.Count)).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"% за открытие");
    if(surpriseTier==3&&Model.Stage>=3)foreach(var f in IdleModel.Forms)entries.Add(f.name+"\n0.1875% за открытие");
   }
   surpriseShelf=Mathf.Clamp(surpriseShelf,0,Math.Max(0,(entries.Count-1)/5));
   Label(panel,"Pool note",IdleModel.RarityNames[surpriseTier]+" · текущая стадия "+Model.Stage+"\nВнутри группы шансы равные.",27,99,426,74,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<5;n++){int ix=surpriseShelf*5+n;if(ix>=entries.Count)break;PaperCard(panel,"Pool item",24,185+n*96,432,88);Label(panel,"Pool name",entries[ix],36,190+n*96,408,77,23,align:TextAnchor.MiddleCenter);}
   Button(panel,"Pool previous","←",24,679,64,64,()=>{surpriseShelf--;Show("surprise-pool");}).interactable=surpriseShelf>0;
   Button(panel,"Pool next","→",392,679,64,64,()=>{surpriseShelf++;Show("surprise-pool");}).interactable=(surpriseShelf+1)*5<entries.Count;
   Label(panel,"Pool page",(surpriseShelf+1)+" / "+Math.Max(1,(entries.Count+4)/5),99,679,282,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Pool return","ВСЕ ВЕРОЯТНОСТИ",24,761,432,64,()=>Show("surprise-odds"),"55E6C1",23);
  }
  void SurpriseCraftPage(){
   var items=new List<IdleItem>();foreach(var i in IdleCatalog.Items)if(CapyCompleteSets.Visible(i)&&i.RewardDay==0&&i.price>0&&i.stage<=Model.Stage)items.Add(i);
   surpriseShelf=Mathf.Clamp(surpriseShelf,0,Math.Max(0,(items.Count-1)/5));
   Label(panel,"Craft wallet",Model.S.stars+" звёзд · выбирай точный предмет\nБез случайности и дубликатов",26,98,428,76,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<5;n++){int ix=surpriseShelf*5+n;if(ix>=items.Count)break;var item=items[ix];bool own=Model.S.owned.Contains(item.id);int price=IdleModel.SurpriseCraftPrice(item);
    var b=Button(panel,"Craft "+item.id,"",24,185+n*96,432,87,()=>{if(Model.CraftSurprise(item.id)){Save();Show("surprise-craft");Sound(feedClip,.15f);}},"F4CF80");ItemPicture(b.transform,item);
    Label(b.transform,"Craft name",item.name,84,2,334,47,22);Label(b.transform,"Craft price",own?"Уже открыто":price+" звёзд",84,48,334,34,22,"418355");b.interactable=!own&&Model.S.stars>=price;
   }
   Button(panel,"Craft previous","←",24,681,64,64,()=>{surpriseShelf--;Show("surprise-craft");}).interactable=surpriseShelf>0;
   Button(panel,"Craft next","→",392,681,64,64,()=>{surpriseShelf++;Show("surprise-craft");}).interactable=(surpriseShelf+1)*5<items.Count;
   Label(panel,"Craft page",(surpriseShelf+1)+" / "+Math.Max(1,(items.Count+4)/5),99,681,282,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Craft return","К ДЕРЕВУ",24,762,432,64,()=>Show("surprises"),"55E6C1",23);
  }
 }
}
