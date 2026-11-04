using System;
using System.Collections.Generic;
namespace Capybara67.Idle {
 [Serializable] public sealed class MandarinProgress {
  public int balance,claims,careActions,careStart,feedStart,activityStart,treeBought,stock,growthCharges,growthStage;
  public string day="",claimedDay="",coinsDay="",growthDay="";
  public double tapUntil,growthLeft;
 }
 public sealed class MandarinPack {
  public readonly string id,name;public readonly int amount,bonus,price;
  public MandarinPack(string i,string n,int a,int b,int p){id=i;name=n;amount=a;bonus=b;price=p;}
  public int Total=>amount+bonus;
 }
 public sealed partial class IdleModel {
  // Payment catalogue only. No grant-by-product-ID or live payment callback is enabled.
  public static readonly MandarinPack[] MandarinPacks={
   new MandarinPack("mandarin_10","Горсточка",10,0,29),new MandarinPack("mandarin_32","Мешочек",30,2,79),
   new MandarinPack("mandarin_66","Корзинка",60,6,149),new MandarinPack("mandarin_138","Ящик урожая",120,18,279),
   new MandarinPack("mandarin_240","Большой урожай",200,40,449),
   new MandarinPack("mandarin_500","Сад мандаринов",400,100,849)};
  public const int CoinsPerMandarin=1000;
  // Whole mandarins, exact coin change. Conversion is spending, not earned income.
  public static int MandarinCost(double coins)=>double.IsNaN(coins)||double.IsInfinity(coins)||coins<0||coins>1e9?-1:(int)Math.Ceiling(coins/CoinsPerMandarin);
  public static double MandarinChange(double coins){int cost=MandarinCost(coins);return cost<0?0:cost*(double)CoinsPerMandarin-coins;}
  public bool CanPay(double coins,bool mandarins=false){
   if(double.IsNaN(coins)||double.IsInfinity(coins)||coins<0||coins>1e12)return false;
   if(!mandarins)return S.coins>=coins;
   int count=MandarinCost(coins);return count>=0&&M.balance>=count&&S.coins<=1e12-MandarinChange(coins);
  }
  public bool CanPayEither(double coins)=>CanPay(coins)||CanPay(coins,true);
  bool Pay(double coins,bool mandarins){
   if(!CanPay(coins,mandarins))return false;
   if(mandarins){M.balance-=MandarinCost(coins);S.coins+=MandarinChange(coins);}else S.coins-=coins;
   return true;
  }
  public MandarinProgress M=>S.mandarins;
  void RepairMandarins(){
   if(S.mandarins==null)S.mandarins=new MandarinProgress();
   M.balance=Math.Max(0,Math.Min(1000000,M.balance));M.claims=Math.Max(0,Math.Min(100000000,M.claims));
   M.careActions=Math.Max(0,Math.Min(1000000000,M.careActions));M.careStart=Math.Max(0,Math.Min(M.careActions,M.careStart));
   M.feedStart=Math.Max(0,Math.Min(S.feeds,M.feedStart));M.activityStart=Math.Max(0,Math.Min(ActivityTotal,M.activityStart));
   M.treeBought=Math.Max(0,Math.Min(3,M.treeBought));M.stock=Math.Max(0,Math.Min(99,M.stock));
   M.growthCharges=Math.Max(0,Math.Min(5,M.growthCharges));M.growthLeft=Clean(M.growthLeft,15000);
   if(M.growthStage!=Stage||Stage>=StageCount){M.growthCharges=0;M.growthLeft=0;}
   M.tapUntil=Math.Min(Clean(M.tapUntil),S.activeSeconds+600);
   if(!ValidDate(M.day,out _))M.day="";
   if(!ValidDate(M.claimedDay,out _))M.claimedDay="";
   if(!ValidDate(M.coinsDay,out _))M.coinsDay="";
   if(!ValidDate(M.growthDay,out _))M.growthDay="";
  }
  int ActivityTotal{get{long total=0;foreach(int count in S.activityRuns)total+=count;return (int)Math.Min(1000000000,total);}}
  public void RefreshMandarins(string date){
   if(!ValidDate(date,out _)||string.CompareOrdinal(date,M.day)<=0)return;
   M.day=date;M.careStart=M.careActions;M.feedStart=S.feeds;M.activityStart=ActivityTotal;M.treeBought=0;
  }
  public int MandarinCareProgress=>Math.Min(3,Math.Max(0,M.careActions-M.careStart));
  public int MandarinFeedProgress=>Math.Min(2,Math.Max(0,S.feeds-M.feedStart));
  public int MandarinActivityProgress=>Math.Min(1,Math.Max(0,ActivityTotal-M.activityStart));
  public bool MandarinTaskDone=>ValidDate(M.day,out _)&&string.CompareOrdinal(M.claimedDay,M.day)<0&&
   (MandarinCareProgress>=3||MandarinFeedProgress>=2||MandarinActivityProgress>=1);
  public int MandarinTaskReward=>3+((M.claims+1)%5==0?5:0);
  public bool ClaimMandarins(string date){
   RefreshMandarins(date);
   if(date!=M.day||!MandarinTaskDone||M.balance>1000000-MandarinTaskReward)return false;
   M.balance+=MandarinTaskReward;M.claims++;M.claimedDay=date;return true;
  }
  public double MandarinTapLeft=>Math.Max(0,M.tapUntil-S.activeSeconds);
  public double ManualCareValue=>CareValue*(MandarinTapLeft>1e-8?1.25:1);
  public double MandarinCoinAmount=>Math.Floor(Math.Min(5000,Math.Max(100,300*BasePassive)));
  double MandarinGrowthBonus(double growth)=>M.growthCharges>0&&M.growthStage==Stage&&Stage<StageCount?Math.Min(M.growthLeft,growth*.25):0;
  public static int MandarinItemPrice(IdleItem i)=>i==null||i.TreeOnly||i.RewardDay>0||!CapyCompleteSets.Visible(i)||i.price<=0?0:
   i.IsSet?40:i.slot==2?60:i.slot==3?20:i.slot==4?15:i.slot==5?15:0;
  public static int MandarinPrice(string id){
   switch(id){case "tap_boost":case "growth_boost":case "tree_gift":return 5;case "care_stock":case "coin_pouch":return 10;case "feast":return 15;}
   return id!=null&&id.StartsWith("item:")?MandarinItemPrice(IdleCatalog.Find(id.Substring(5))):0;
  }
  public string MandarinBlock(string id,string date){
   int price=MandarinPrice(id);if(price<=0)return "Недоступный товар";
   if(!ValidDate(date,out _)||date!=M.day)return "Обнови день в магазине";
   if(!S.starter||Stage<2)return "После первой миски";
   if(id.StartsWith("item:")){
    var item=IdleCatalog.Find(id.Substring(5));if(S.owned.Contains(item.id))return "Уже в коллекции";
    if(Stage<item.stage)return "Со стадии "+item.stage;
   }
   switch(id){
    case "tap_boost":if(MandarinTapLeft>1e-8)return "Ускоритель уже работает";break;
    case "growth_boost":if(Stage>=StageCount)return "Все этапы пройдены";
     if(M.growthCharges>0&&M.growthStage==Stage&&M.growthLeft>0)return "Добавка уже есть";
     if(string.CompareOrdinal(M.growthDay,date)>=0)return "Уже куплено сегодня";break;
    case "care_stock":if(M.stock>96)return "Запас заполнен";break;
    case "feast":if(Stage<3)return "Со стадии 3";if(S.premiumFood>=10000)return "Запас заполнен";break;
    case "coin_pouch":if(string.CompareOrdinal(M.coinsDay,date)>=0)return "Уже куплено сегодня";if(S.coins>1e12-MandarinCoinAmount)return "Кошелёк заполнен";break;
    case "tree_gift":if(M.treeBought>=3||S.surprisePaid>=10)return "Лимит подарков на сегодня";
     if(S.surpriseDate!=date)return "Обнови день в магазине";
     if(SurpriseWait>0||S.surpriseRun>=10)return "Дерево отдыхает";break;
   }
   return M.balance<price?"Не хватает "+(price-M.balance)+" мандаринок":"";
  }
  public bool BuyMandarin(string id,string date){
   RefreshMandarins(date);if(id=="tree_gift")RefreshSurprises(date);
   if(MandarinBlock(id,date)!="")return false;
   M.balance-=MandarinPrice(id);
   switch(id){
    case "tap_boost":M.tapUntil=S.activeSeconds+600;break;
    case "growth_boost":M.growthCharges=5;M.growthStage=Stage;M.growthLeft=(Growth[Stage]-Growth[Stage-1])*.1;M.growthDay=date;break;
    case "care_stock":M.stock+=3;break;
    case "feast":S.premiumFood++;break;
    case "coin_pouch":Earn(MandarinCoinAmount);M.coinsDay=date;break;
    case "tree_gift":M.treeBought++;S.surprisePaid++;ResolveSurprises(1);break;
    default:Grant(id.Substring(5));break;
   }
   return true;
  }
  public bool UseMandarinStock(){
   if(M.stock<=0||ActionWait>1e-8||S.fullness>=100&&S.mood>=100)return false;
   M.stock--;S.fullness=Clean(S.fullness+40,100);S.mood=Clean(S.mood+10,100);S.careActionUntil=S.activeSeconds+1.8;Wake();return true;
  }
 }
}
