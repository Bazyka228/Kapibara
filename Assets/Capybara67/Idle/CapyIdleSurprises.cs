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
  string GardenBlockText(GardenBlock reason){
   switch(reason){
    case GardenBlock.Stage:return "Со стадии 2";
    case GardenBlock.Pause:return "Пауза · "+Minutes(Model.SurpriseWait);
    case GardenBlock.Series:return "В серии: "+(10-Model.S.surpriseRun);
    case GardenBlock.Tickets:return "Нужен билет";
    case GardenBlock.Daily:return "Лимит дня";
    case GardenBlock.Budget:return "Лимит бюджета";
    case GardenBlock.Reserve:return "Резерв: 100";
    default:return "";
   }
  }
  void SurprisePage(){
   string budgetDate=Model.S.surpriseDate;Model.RefreshSurprises(Date);if(budgetDate!=Model.S.surpriseDate)Save();
   var balance=Label(panel,"Surprise balance","",26,94,428,37,24,"513821",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>balance.text=Number(Model.S.coins)+" монет");
   var tree=Rect(panel,"Mandarin surprise tree",115,137,278,205);tree.localScale=Vector3.one*.9f;DrawSurpriseTree(tree);
   Shape(panel,"Guarantee track",85,325,310,9,"CCB78C",4);
   var guarantee=Shape(panel,"Guarantee fill",85,325,1,9,"87AC65",4);
   liveLabels.Add(()=>guarantee.rectTransform.sizeDelta=new Vector2(310*Model.S.surprisePity/10f,9));
   var pity=Label(panel,"Surprise guarantee","",28,342,424,60,22,"513821",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>pity.text=Model.Stage<2?"Дерево откроется на этапе 2":Model.SurpriseWait>0?"Отдохнём · "+Minutes(Model.SurpriseWait):"Особый подарок — ещё до "+(10-Model.S.surprisePity)+" открытий\nКакой попадётся? Нажми «Шансы»");
   var ticket=Button(panel,"Open surprise ticket","",24,408,432,64,()=>OpenGarden(1,true),"55E6C1",23);
   liveLabels.Add(()=>{var block=Model.SurpriseBlock(1,true);ticket.GetComponentInChildren<Text>().text=block==GardenBlock.None?"ОТКРЫТЬ ЗА БИЛЕТ · "+Model.S.surpriseTickets:GardenBlockText(block);ticket.interactable=block==GardenBlock.None;});
   Coin(panel,50,491,25);Coin(panel,264,491,25);
   var singlePrice=Label(panel,"Single opening price","",86,481,146,43,23,"513821");singlePrice.font=bold;
   var batchPrice=Label(panel,"Ten openings price","",300,481,154,43,23,"513821");batchPrice.font=bold;
   var one=Button(panel,"Open surprise one","ОТКРЫТЬ 1 РАЗ",24,531,210,68,()=>OpenGarden(1,false),"F4CF80",21);
   var ten=Button(panel,"Open surprise ten","ОТКРЫТЬ 10 РАЗ",246,531,210,68,()=>OpenGarden(10,false),"F4CF80",21);
   var limits=Button(panel,"Surprise spending details","",24,611,432,68,()=>Show("surprise-budget"),"FFFFFF00",21).GetComponentInChildren<Text>();
   liveLabels.Add(()=>{
    singlePrice.text=Number(Model.SurprisePrice);batchPrice.text=Number(Model.SurprisePrice*10);
    var a=Model.SurpriseBlock(1);var b=Model.SurpriseBlock(10);
    if(Model.SurpriseBlock(1,false,true)==GardenBlock.None)a=GardenBlock.None;if(Model.SurpriseBlock(10,false,true)==GardenBlock.None)b=GardenBlock.None;
    one.interactable=a==GardenBlock.None;ten.interactable=b==GardenBlock.None;
    limits.text=(a!=GardenBlock.None?GardenBlockText(a):"За монеты: до "+Model.SurprisePaidAvailable+" открытий")+"\nБюджет: "+Number(Model.SurpriseBudgetLeft)+" · подробнее ›";
   });
   Button(panel,"Surprise chances","ШАНСЫ",24,696,210,62,()=>Show("surprise-odds"),size:22);
   Button(panel,"Surprise crafting","КОЛЛЕКЦИЯ",246,696,210,62,()=>{surpriseShelf=0;Show("surprise-craft");},size:22);
   Button(panel,"Surprise collection","МОИ НАХОДКИ",24,768,432,62,()=>Show("surprise-results"),size:23);
  }
  void SurpriseBudgetPage(){
   Label(panel,"Budget heading","МОНЕТЫ ДЛЯ ЗАБОТЫ",24,105,432,49,27,align:TextAnchor.MiddleCenter).font=bold;
   PaperCard(panel,"Budget summary card",24,170,432,155);
   var summary=Label(panel,"Budget summary","",38,178,404,137,23,align:TextAnchor.MiddleCenter);
   liveLabels.Add(()=>summary.text="Бюджет дня: "+Number(Model.S.surpriseBudget)+" монет\nПотрачено: "+Number(Model.S.surpriseSpent)+" · осталось: "+Number(Model.SurpriseBudgetLeft)+"\nПокупок осталось: "+Model.SurpriseLeft+" из 10\nСейчас за монеты: "+Model.SurprisePaidAvailable);
   Label(panel,"Budget explanation","Бюджет — 20% монет при входе.\nОн обновится завтра.\n\nПосле траты остаётся 100 монет.\nБилеты и мандарины не тратят\nэтот монетный бюджет.",32,344,416,230,22,"72543A",TextAnchor.MiddleCenter);
   var status=Label(panel,"Budget status","",32,586,416,96,22,align:TextAnchor.MiddleCenter);
   liveLabels.Add(()=>{var wait=DateTime.UtcNow.Date.AddDays(1)-DateTime.UtcNow;status.text="Новый бюджет через "+(int)wait.TotalHours+" ч "+wait.Minutes+" мин\nПосле 10 любых открытий —\nпауза на 2 минуты в игре.";});
   Button(panel,"Budget free tickets","БЕСПЛАТНЫЕ БИЛЕТЫ",24,690,432,64,()=>Show("surprise-gifts"),"55E6C1",23);
   Button(panel,"Budget return","К ДЕРЕВУ",24,769,432,64,()=>Show("surprises"),size:23);
  }
  void OpenGarden(int count,bool ticket){
   if(ticket){CompleteGarden(count,true,false);return;}
   Model.RefreshSurprises(Date);ChooseCurrency(count==1?"Подарок дерева":"10 подарков дерева",Model.SurprisePrice*count,m=>CompleteGarden(count,false,m),m=>Model.SurpriseBlock(count,false,m)==GardenBlock.None);
  }
  void CompleteGarden(int count,bool ticket,bool mandarins){
   Model.RefreshSurprises(Date);var rewards=Model.OpenSurprises(count,ticket,mandarins);if(rewards==null)return;
   // Commit once before any animation: navigation, skip, pause and reload cannot reroll.
   Save();surpriseResultIndex=0;Show(Model.S.calm?"surprise-results":"surprise-opening");
  }
  void SurpriseResultsPage(){
   var rewards=Model.S.surpriseResults;
   if(rewards.Count>0){
    surpriseResultIndex=Mathf.Clamp(surpriseResultIndex,0,rewards.Count-1);var r=rewards[surpriseResultIndex];int tier=Mathf.Clamp(r.tier,0,3);
    Label(panel,"Reward rarity",IdleModel.RarityNames[tier]+" подарок",30,103,420,42,27,align:TextAnchor.MiddleCenter).font=bold;
    var reveal=Rect(panel,"Reward reveal",116,156,248,210);Oval(reveal,"Reward halo",12,5,224,194,IdleModel.RarityColors[tier],3);
    var item=IdleCatalog.Find(r.item);var form=IdleModel.Form(r.form);
    if(item!=null&&(item.IsSet||item.slot==5)){var portrait=Rect(reveal,"Gift illustration",34,12,240,250);portrait.localScale=Vector3.one*.72f;DrawAvatar(portrait,item.id);}
    else if(item!=null&&item.slot==2){var clip=Rect(reveal,"Reward landscape",4,28,240,155);clip.gameObject.AddComponent<RectMask2D>();var scene=Rect(clip,"Reward scene",0,-187.5f,480,850);scene.localScale=Vector3.one*.5f;DrawScene(scene,item);}
    else if(item!=null){var portrait=Rect(reveal,"Gift illustration",42,25,90,90);portrait.localScale=Vector3.one*2;ItemPicture(portrait,item);}
    else if(form!=null){var portrait=Rect(reveal,"Gift form",42,28,90,90);portrait.localScale=Vector3.one*3;FormPortrait(portrait,form.art,form.color);}
    else Gift(reveal,66,47,117);
    Label(panel,"Reward name",r.name,30,374,420,69,26,align:TextAnchor.MiddleCenter);
    Label(panel,"Reward credited",(r.duplicate?"Повтор → ":"")+(r.coins>0?"+"+Number(r.coins)+" монет":"Подарок получен")+"\nУже сохранено",30,447,420,65,23,"418355",TextAnchor.MiddleCenter);
    var prev=Button(panel,"Previous reward","←",24,525,64,64,()=>{surpriseResultIndex--;Show("surprise-results");});prev.interactable=surpriseResultIndex>0;
    var next=Button(panel,"Next reward","→",392,525,64,64,()=>{surpriseResultIndex++;Show("surprise-results");});next.interactable=surpriseResultIndex<rewards.Count-1;
    Label(panel,"Reward position",(surpriseResultIndex+1)+" / "+rewards.Count,101,525,278,64,24,align:TextAnchor.MiddleCenter);
   }else Label(panel,"No recent finds","Здесь появятся твои находки.\nОткрытые вещи уже в гардеробе.",34,195,412,170,25,align:TextAnchor.MiddleCenter);
   var found=rewards.Count>0?IdleCatalog.Find(rewards[Mathf.Clamp(surpriseResultIndex,0,rewards.Count-1)].item):null;
   Button(panel,"Surprise wardrobe",found!=null?"ПОСМОТРЕТЬ НАХОДКУ":"К КАПИБАРЕ",24,611,432,64,()=>{if(found!=null)OpenWardrobe(found.id,"surprise-results");else Show("home");},"55E6C1",23);
   Button(panel,"Return to tree","К ДЕРЕВУ",24,705,432,72,()=>Show("surprises"),"55E6C1",24);
  }
  void SurpriseGiftsPage(){
   Label(panel,"Free gifts note","Подарки без спешки.\nПропуск дня ничего не отнимает.",30,102,420,78,24,align:TextAnchor.MiddleCenter);
   var daily=Button(panel,"Daily tree ticket","",24,192,432,64,()=>{if(Model.ClaimSurpriseTicket(Date)){Save();Show("surprise-gifts");}},"55E6C1",23);
   liveLabels.Add(()=>{daily.interactable=Model.CanDailySurprise(Date);daily.GetComponentInChildren<Text>().text=Model.Stage<2?"ДЕРЕВО СО СТАДИИ 2":daily.interactable?"БИЛЕТ ДНЯ · БЕСПЛАТНО":"БИЛЕТ ДНЯ УЖЕ ПОЛУЧЕН";});
   Button(panel,"Quest tree tickets","ПОРУЧЕНИЯ · +1 БИЛЕТ",24,268,432,64,()=>Show("quests"),size:23);
   Label(panel,"Seed explanation","ТРИ МАНДАРИНОВЫХ РОСТКА\nВыбери один: каждый даёт подарок.\nБез ставки · один раз в день",28,349,424,101,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<3;n++){int choice=n;var b=Button(panel,"Seed "+n,"",24+n*148,461,136,92,()=>{if(Model.ChooseSeed(Date,choice)!=null){Save();surpriseResultIndex=0;Show("surprise-results");Sound(feedClip,.18f);}},"F4CF80");Gift(b.transform,48,9,38);Label(b.transform,"Seed number",(n+1).ToString(),10,49,116,34,23,align:TextAnchor.MiddleCenter);liveLabels.Add(()=>b.interactable=Model.CanChooseSeed(Date));}
   Label(panel,"Seed chances","По 1/3: монеты, миска или 75 монет.\nМонеты и миска: ещё +25 монет.",28,559,424,69,22,"72543A",TextAnchor.MiddleCenter);
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
   surpriseAdRequest=true;autoAdId="capy_tree_"+autoRequestSerial;adCredited=false;adPending=true;UpdateAudioState();adRequestedAt=Time.realtimeSinceStartupAsDouble;Release();Save();
#if PLUGIN_YG_2
   try{YG2.RewardedAdvShow(autoAdId);}catch(Exception){AutoAdError();}
#else
   AutoAdError();
#endif
  }
  void SurpriseOddsPage(){
   Label(panel,"Odds introduction","Шансы — параметры первой версии.\nКаждое открытие даёт +25 монет.",28,98,424,75,23,align:TextAnchor.MiddleCenter);
   string[] odds={"30% · ресурсы, 30% · вещь дерева","25% · редкая вещь дерева","12% · эпическая вещь дерева","3% · легендарная вещь дерева"};
   for(int n=0;n<4;n++){int tier=n;float y=185+n*83;var b=Button(panel,"Odds tier "+n,"",24,y,432,74,()=>{surpriseTier=tier;surpriseShelf=0;Show("surprise-pool");},IdleModel.RarityColors[n]);Label(b.transform,"Tier name",IdleModel.RarityNames[n],14,1,405,33,24);Label(b.transform,"Tier odds",odds[n],14,36,405,34,22);}
   Label(panel,"Guarantee explanation","На 10-м открытии без эпического:\n97% эпический, 3% легендарный.\nУспех сбрасывает счётчик.\nСначала — новые вещи этой редкости.\nГруппа собрана? Повтор даст монеты.",28,526,424,172,22,"72543A",TextAnchor.MiddleCenter);
   Label(panel,"Wallet protection","За повтор: +25 / 50 / 125 / 250 монет.\nНажми редкость: увидишь точные\nшансы следующего подарка.",28,701,424,101,22,"72543A",TextAnchor.MiddleCenter);
  }
  void SurprisePoolPage(){
   var pool=Model.SurprisePool(surpriseTier);var entries=new List<string>();
   string resourceChance=Model.S.surprisePity>=9?"0%":"10%";
   if(surpriseTier==0)entries.AddRange(new[]{resourceChance+" · вернуть 25% цены в монетах",resourceChance+" · праздничная миска",resourceChance+" · +10% тапа на 15 минут"});
   foreach(var item in pool)entries.Add(item.name+"\n"+(100*Model.SurpriseItemChance(item.id)).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"% · "+(Model.S.owned.Contains(item.id)?"уже открыто":"новая вещь"));
   surpriseShelf=Mathf.Clamp(surpriseShelf,0,Math.Max(0,(entries.Count-1)/5));
   Label(panel,"Pool note",IdleModel.RarityNames[surpriseTier]+" · шансы сейчас\n"+(Model.S.surprisePity>=9?"Сейчас действует гарант":"Учтены твоя стадия и коллекция"),27,99,426,74,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<5;n++){int ix=surpriseShelf*5+n;if(ix>=entries.Count)break;PaperCard(panel,"Pool item",24,185+n*96,432,88);Label(panel,"Pool name",entries[ix],36,190+n*96,408,77,23,align:TextAnchor.MiddleCenter);}
   Button(panel,"Pool previous","←",24,679,64,64,()=>{surpriseShelf--;Show("surprise-pool");}).interactable=surpriseShelf>0;
   Button(panel,"Pool next","→",392,679,64,64,()=>{surpriseShelf++;Show("surprise-pool");}).interactable=(surpriseShelf+1)*5<entries.Count;
   Label(panel,"Pool page",(surpriseShelf+1)+" / "+Math.Max(1,(entries.Count+4)/5),99,679,282,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Pool return","ВСЕ ВЕРОЯТНОСТИ",24,761,432,64,()=>Show("surprise-odds"),"55E6C1",23);
  }
  void SurpriseCraftPage(){
   var items=new List<IdleItem>();foreach(var i in IdleCatalog.Items)if(i.TreeOnly)items.Add(i);
   items.Sort((a,b)=>{int tier=a.TreeTier.CompareTo(b.TreeTier);return tier!=0?tier:a.slot.CompareTo(b.slot);});
   surpriseShelf=Mathf.Clamp(surpriseShelf,0,System.Math.Max(0,(items.Count-1)/5));
   int owned=0;foreach(var i in items)if(Model.S.owned.Contains(i.id))owned++;
   Label(panel,"Tree collection count",owned+" / 24 находки · только из дерева\nПо 6 вещей в четырёх категориях",26,98,428,76,22,align:TextAnchor.MiddleCenter);
   for(int n=0;n<5;n++){int ix=surpriseShelf*5+n;if(ix>=items.Count)break;var item=items[ix];bool own=Model.S.owned.Contains(item.id);
    var b=Button(panel,"Tree preview "+item.id,"",24,185+n*96,432,87,()=>OpenWardrobe(item.id,"surprise-craft"),IdleModel.RarityColors[item.TreeTier]);ItemPicture(b.transform,item);
    Label(b.transform,"Tree reward name",item.name,84,2,334,47,22);Label(b.transform,"Tree reward rarity",IdleModel.RarityNames[item.TreeTier]+(own?" · открыто":" · посмотреть"),84,48,334,34,22,"65432D");
   }
   Button(panel,"Collection previous","←",24,681,64,64,()=>{surpriseShelf--;Show("surprise-craft");}).interactable=surpriseShelf>0;
   Button(panel,"Collection next","→",392,681,64,64,()=>{surpriseShelf++;Show("surprise-craft");}).interactable=(surpriseShelf+1)*5<items.Count;
   Label(panel,"Collection page",(surpriseShelf+1)+" / "+System.Math.Max(1,(items.Count+4)/5),99,681,282,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Collection return","К ДЕРЕВУ",24,762,432,64,()=>Show("surprises"),"55E6C1",23);
  }
 }
}
