using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  int mandarinPage,mandarinShelf,mandarinSlot;
  string mandarinSelected="",mandarinOffer="",mandarinMessage="";
  Text mandarinHud;
  static readonly string[] MandarinGoods={"tap_boost","growth_boost","care_stock","feast","coin_pouch","tree_gift","outfits","scenes","effects","emotions"};
  string MandarinName(string id){
   switch(id){case "tap_boost":return "Тёплые лапки";case "growth_boost":return "Витамины";case "care_stock":return "Запас заботы";
    case "feast":return "Праздничная миска";case "coin_pouch":return "Кошелёк монет";case "tree_gift":return "Подарок дерева";
    case "outfits":return "Наряды";case "scenes":return "Сцены";case "effects":return "Эффекты";case "emotions":return "Эмоции";}
   return id.StartsWith("item:")?IdleCatalog.Find(id.Substring(5))?.name??"Предмет":"Подарок";
  }
  string MandarinDetail(string id){
   switch(id){
    case "tap_boost":return "+25% монет от ручных тапов.\n10 минут активной игры.\nПауза и выход сохраняют время.\nАвтокликер не включается.";
    case "growth_boost":return "+25% роста от 5 кормлений.\nДобавочный рост — до "+Number(Model.Stage<5?(IdleModel.Growth[Model.Stage]-IdleModel.Growth[Model.Stage-1])*.1:0)+".\nДо перехода на следующий этап.\nНе более одной покупки в день.";
    case "care_stock":return "3 порции в запас. Каждая даёт:\n+40 сытости и +10 настроения.\nБез роста. Не портится.\nИспользуй кнопкой «Мой запас».";
    case "feast":return "Одна праздничная миска.\nБазовый рост: +240.\nСытость: +60.\nБудет ждать в меню еды.";
    case "coin_pouch":return Number(Model.MandarinCoinAmount)+" монет сразу в кошелёк.\nСумма зависит от подстилки.\nНе более одной покупки в день.\nБесплатный путь: тап и занятия.";
    case "tree_gift":return "Одно случайное открытие.\nШансы и гарант не меняются.\nДо 3 в день за Мандаринки,\nв пределах общего лимита 10.";
    default:return "Выбранная вещь навсегда.\nПокупка не меняет твой наряд.\nНадеть можно в гардеробе.\nМандаринки доступны бесплатно.";
   }
  }
  void PrepareMandarinShop(){Model.RefreshDay(Date);Model.RefreshSurprises(Date);Save();}
  void BuildMandarinHud(){
   var b=Button(home,"Mandarin wallet","",24,58,150,60,()=>{mandarinPage=0;Show("mandarins");},"FFFFFF00");
   ActivityMandarin(b.transform,6,18,30);mandarinHud=Label(b.transform,"Mandarin balance","",45,10,101,46,23);
   mandarinHud.color=new Color32(255,242,211,255);mandarinHud.font=bold;
   Button(home,"Mandarin shop plus","+",178,58,60,60,()=>{mandarinPage=0;Show("mandarins");},"F4CF80",29);
  }
  void MandarinShopPage(){
   PrepareMandarinShop();
   Label(panel,"Mandarin shop balance",Model.M.balance+" мандаринок",24,103,432,42,27,align:TextAnchor.MiddleCenter).font=bold;
   Button(panel,"Mandarin daily",Model.MandarinTaskDone?"ЗАБРАТЬ МАНДАРИНКИ":"ЗАРАБОТАТЬ БЕСПЛАТНО",24,160,432,66,()=>Show("mandarin-tasks"),"55E6C1",23);
   mandarinPage=Mathf.Clamp(mandarinPage,0,2);
   for(int n=0;n<4;n++){
    int ix=mandarinPage*4+n;if(ix>=MandarinGoods.Length)break;string id=MandarinGoods[ix];float x=24+n%2*222,y=246+n/2*184;
    var b=Button(panel,"Mandarin goods "+id,"",x,y,210,176,()=>{
     if(ix>=6){mandarinSlot=new[]{0,2,3,4}[ix-6];mandarinShelf=0;mandarinSelected="";Show("mandarin-items");}
     else{mandarinOffer=id;Show("mandarin-confirm");}
    });
    Label(b.transform,"Goods title",MandarinName(id),12,6,186,66,23).font=bold;
    string[] captions={"+25% тапа","5 кормлений","3 порции","1 миска",Number(Model.MandarinCoinAmount)+" монет","1 открытие","Цельный комплект","Новый уголок","Живые украшения","Новые эмоции"};
    Label(b.transform,"Goods note",captions[ix],12,78,186,42,21);
    int[] prices={5,5,10,15,10,5,40,60,20,15};
    Label(b.transform,"Goods price",prices[ix]+" мандаринок",12,130,186,34,22,"418355");
   }
   Button(panel,"Mandarin previous","←",24,635,64,64,()=>{mandarinPage--;Show("mandarins");}).interactable=mandarinPage>0;
   Label(panel,"Mandarin page",(mandarinPage+1)+" / 3",100,635,280,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin next","→",392,635,64,64,()=>{mandarinPage++;Show("mandarins");}).interactable=mandarinPage<2;
   Button(panel,"Mandarin stock","МОЙ ЗАПАС · "+Model.M.stock,24,750,432,76,()=>Show("mandarin-stock"),"55E6C1",23);
  }
  void MandarinTaskPage(){
   PrepareMandarinShop();
   bool claimed=Model.M.claimedDay==Date;int rewardToday=claimed?(Model.M.claims%5==0?8:3):Model.MandarinTaskReward;
   Label(panel,"Mandarin task intro",(claimed?"Сегодня награда получена":"Любое одно поручение за день")+"\nНаграда: "+rewardToday+" мандаринок",24,107,432,86,25,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin task care","ПОЗАБОТЬСЯ 3 РАЗА · "+Model.MandarinCareProgress+" / 3",24,218,432,78,()=>Show("care"),size:22);
   Button(panel,"Mandarin task food","НАКОРМИ 2 РАЗА · "+Model.MandarinFeedProgress+" / 2",24,313,432,78,()=>Show("food"),size:22);
   Button(panel,"Mandarin task activity","ЗАВЕРШИ ЗАНЯТИЕ · "+Model.MandarinActivityProgress+" / 1",24,408,432,78,()=>OpenActivities("mandarin-tasks"),size:22);
   var claim=Button(panel,"Claim mandarins",claimed?"СЕГОДНЯ УЖЕ ПОЛУЧЕНО":"ЗАБРАТЬ · "+Model.MandarinTaskReward,24,526,432,72,()=>{
    int reward=Model.MandarinTaskReward;if(Model.ClaimMandarins(Date)){Save();mandarinMessage="Получено: "+reward+" мандаринок.\nВыбирай подарок или копи дальше.";Show("mandarin-result");}
   },"55E6C1",22);claim.interactable=Model.MandarinTaskDone;
   Label(panel,"Mandarin task rules","До бонуса +5: ещё "+(5-Model.M.claims%5)+" поручений.\nПропуски не сбрасывают счётчик.\nРеклама и покупки не нужны.",24,624,432,100,22,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin task return","В МАГАЗИН",24,750,432,76,()=>Show("mandarins"));
  }
  void MandarinConfirmPage(){
   PrepareMandarinShop();string id=mandarinOffer;
   Label(panel,"Mandarin confirm title",MandarinName(id),24,103,432,78,28,align:TextAnchor.MiddleCenter).font=bold;
   if(id.StartsWith("item:")){var art=Rect(panel,"Mandarin item preview",140,178,80,80);art.localScale=Vector3.one*2.5f;ItemPicture(art,IdleCatalog.Find(id.Substring(5)));}
   else if(id=="tree_gift")Gift(panel,170,202,140);else ActivityMandarin(panel,174,205,132);
   Label(panel,"Mandarin confirm detail",MandarinDetail(id),34,365,412,155,23,align:TextAnchor.MiddleCenter);
   var note=Label(panel,"Mandarin confirm balance","",24,527,432,90,22,align:TextAnchor.MiddleCenter);
   var buy=Button(panel,"Confirm mandarin purchase","ПОТРАТИТЬ · "+IdleModel.MandarinPrice(id),24,635,432,72,()=>{
    if(!Model.BuyMandarin(id,Date)){Show("mandarin-confirm");return;}
    Save();Sound(purchaseClip,.2f);
    if(id=="tree_gift"){surpriseResultIndex=0;Show(Model.S.calm?"surprise-results":"surprise-opening");return;}
    mandarinMessage=MandarinName(id)+"\nПолучено и сохранено.";Show("mandarin-result");
   },"55E6C1",24);
   liveLabels.Add(()=>{string block=Model.MandarinBlock(id,Date);buy.interactable=block=="";note.text="Баланс: "+Model.M.balance+" · Цена: "+IdleModel.MandarinPrice(id)+"\n"+(block==""?"Подтверди трату мандаринок":block);});
   if(id=="tree_gift"){
    Button(panel,"Mandarin free alternative","БЕСПЛАТНО",24,750,210,76,()=>Show("surprise-gifts"),size:22);
    Button(panel,"Mandarin tree odds","ШАНСЫ",246,750,210,76,()=>Show("surprise-odds"),size:22);
   }else Button(panel,"Mandarin free alternative","ЗАРАБОТАТЬ БЕСПЛАТНО",24,750,432,76,()=>Show("mandarin-tasks"),size:22);
  }
  void MandarinItemsPage(){
   PrepareMandarinShop();var items=new List<IdleItem>();foreach(var item in IdleCatalog.Items)if(item.slot==mandarinSlot&&IdleModel.MandarinItemPrice(item)>0)items.Add(item);
   mandarinShelf=Mathf.Clamp(mandarinShelf,0,Math.Max(0,(items.Count-1)/4));
   var chosen=items.Find(i=>i.id==mandarinSelected);if(chosen==null&&items.Count>0)chosen=items[mandarinShelf*4];mandarinSelected=chosen?.id??"";
   Label(panel,"Mandarin selection balance",Model.M.balance+" мандаринок · выбери вещь",24,101,432,43,23,align:TextAnchor.MiddleCenter);
   PaperCard(panel,"Mandarin preview card",24,154,432,177);
   if(chosen!=null){var preview=Rect(panel,"Mandarin collection art",40,154,80,80);preview.localScale=Vector3.one*2.05f;ItemPicture(preview,chosen);
    Label(panel,"Mandarin selected name",chosen.name,216,168,224,78,24).font=bold;
    Label(panel,"Mandarin selected price",IdleModel.MandarinItemPrice(chosen)+" мандаринок\nСтадия "+chosen.stage,216,252,224,64,22);}
   for(int n=0;n<4;n++){int ix=mandarinShelf*4+n;if(ix>=items.Count)break;var item=items[ix];
    var b=Button(panel,"Mandarin item "+item.id,"",24+n%2*222,348+n/2*126,210,118,()=>{mandarinSelected=item.id;Show("mandarin-items");},item==chosen?"665781":"39315B");
    Label(b.transform,"Mandarin item name",item.name,12,6,186,72,22);
    Label(b.transform,"Mandarin item owned",Model.S.owned.Contains(item.id)?"ОТКРЫТО":IdleModel.MandarinItemPrice(item)+" мандаринок",12,81,186,32,21,"418355");
   }
   Button(panel,"Mandarin items previous","←",24,614,64,64,()=>{mandarinShelf--;mandarinSelected="";Show("mandarin-items");}).interactable=mandarinShelf>0;
   Label(panel,"Mandarin items page",(mandarinShelf+1)+" / "+Math.Max(1,(items.Count+3)/4),100,614,280,64,23,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin items next","→",392,614,64,64,()=>{mandarinShelf++;mandarinSelected="";Show("mandarin-items");}).interactable=(mandarinShelf+1)*4<items.Count;
   Button(panel,"Mandarin item select","ПОСМОТРЕТЬ ПОДРОБНЕЕ",24,708,432,72,()=>{if(chosen!=null){mandarinOffer="item:"+chosen.id;Show("mandarin-confirm");}},"55E6C1",23).interactable=chosen!=null;
  }
  void MandarinStockPage(){
   PrepareMandarinShop();ActivityMandarin(panel,174,125,132);
   var info=Label(panel,"Mandarin stock info","",24,291,432,170,23,align:TextAnchor.MiddleCenter);
   var use=Button(panel,"Use mandarin stock","ДАТЬ ПОРЦИЮ",24,486,432,72,()=>{if(Model.UseMandarinStock()){Save();mandarinMessage="Капибара перекусила!\nСытость и настроение пополнены.";Show("mandarin-result");}},"55E6C1",23);
   liveLabels.Add(()=>{info.text="Порций в запасе: "+Model.M.stock+"\nСытость: "+Math.Ceiling(Model.S.fullness)+" / 100\nНастроение: "+Math.Ceiling(Model.S.mood)+" / 100\n+40 сытости · +10 настроения";use.interactable=Model.M.stock>0&&Model.ActionWait<=0&&(Model.S.fullness<100||Model.S.mood<100);});
   var boosts=Label(panel,"Mandarin active boosts","",24,596,432,115,22,align:TextAnchor.MiddleCenter);
   liveLabels.Add(()=>boosts.text="Усиление тапа: "+Minutes(Model.MandarinTapLeft)+"\nКормлений с добавкой: "+(Model.M.growthStage==Model.Stage?Model.M.growthCharges:0)+"\nБесплатная забота доступна всегда.");
   Button(panel,"Mandarin free care","БЕСПЛАТНАЯ ЗАБОТА",24,750,432,76,()=>Show("care"));
  }
  void MandarinResultPage(){
   ActivityMandarin(panel,174,180,132);Star(panel,119,180,32,"F4CF80");Star(panel,321,273,26,"F4CF80");
   Label(panel,"Mandarin receipt",mandarinMessage,30,360,420,140,26,align:TextAnchor.MiddleCenter);
   Label(panel,"Mandarin receipt balance","Баланс: "+Model.M.balance+" мандаринок",24,530,432,58,24,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin result shop","В МАГАЗИН",24,650,432,72,()=>Show("mandarins"),"55E6C1");
   Button(panel,"Mandarin result home","К КАПИБАРЕ",24,750,432,76,()=>Show("home"));
  }
 }
}
