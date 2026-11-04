using System;
using System.Collections.Generic;
namespace Capybara67.Idle {
 [Serializable] public sealed class GardenReward {
  public int tier,stars;public double coins;public string item="",form="",name="";public bool duplicate;
 }
 public sealed partial class IdleModel {
  public const int SurpriseGuarantee=10;
  public static readonly string[] RarityNames={"Обычный","Редкий","Эпический","Легендарный"};
  public static readonly string[] RarityColors={"99B584","88BBD2","B9A2D3","E9C36D"};
  public double SurprisePrice=>100*Stage;
  public double SurpriseBudgetLeft=>Math.Max(0,S.surpriseBudget-S.surpriseSpent);
  public double SurpriseBuffLeft=>Math.Max(0,S.surpriseBuffUntil-S.activeSeconds);
  public int SurpriseLeft=>Math.Max(0,10-S.surprisePaid);
  public static int SurpriseTier(double roll,int pity)=>pity>=9? (roll<.03?3:2):roll<.6?0:roll<.85?1:roll<.97?2:3;
  void RepairSurprises(){
   S.surprisePity=Math.Max(0,Math.Min(9,S.surprisePity));S.surprisePaid=Math.Max(0,Math.Min(10,S.surprisePaid));
   S.surpriseTickets=Math.Max(0,Math.Min(100,S.surpriseTickets));S.surpriseTotal=Math.Max(0,S.surpriseTotal);
   S.surpriseBudget=Clean(S.surpriseBudget);S.surpriseSpent=Clean(S.surpriseSpent);S.surpriseBuffUntil=Math.Min(Clean(S.surpriseBuffUntil),S.activeSeconds+900);
   S.surpriseBreakUntil=Math.Min(Clean(S.surpriseBreakUntil),S.activeSeconds+120);S.surpriseRun=Math.Max(0,Math.Min(9,S.surpriseRun));
   if(!ValidDate(S.surpriseDate,out _))S.surpriseDate="";
   if(!ValidDate(S.surpriseGiftDate,out _))S.surpriseGiftDate="";
   if(!ValidDate(S.surpriseAdDate,out _))S.surpriseAdDate="";
   if(!ValidDate(S.seedDate,out _))S.seedDate="";
   if(S.surpriseResults==null)S.surpriseResults=new List<GardenReward>();
   S.surpriseResults.RemoveAll(r=>r==null);
   if(S.surpriseResults.Count>10)S.surpriseResults.RemoveRange(10,S.surpriseResults.Count-10);
  }
  public void RefreshSurprises(string date){
   if(!ValidDate(date,out _)||string.CompareOrdinal(date,S.surpriseDate)<=0)return;
   S.surpriseDate=date;S.surpriseBudget=Math.Floor(S.coins*.2);S.surpriseSpent=0;S.surprisePaid=0;
  }
  public bool CanDailySurprise(string date)=>Stage>=2&&ValidDate(date,out _)&&string.CompareOrdinal(date,S.surpriseGiftDate)>0;
  public bool ClaimSurpriseTicket(string date){if(!CanDailySurprise(date))return false;S.surpriseGiftDate=date;S.surpriseTickets=Math.Min(100,S.surpriseTickets+1);return true;}
  public bool CanAdSurprise(string date)=>Stage>=2&&S.surpriseTickets<100&&ValidDate(date,out _)&&string.CompareOrdinal(date,S.surpriseAdDate)>0;
  public bool ClaimAdSurprise(string date){if(!CanAdSurprise(date))return false;S.surpriseAdDate=date;S.surpriseTickets++;return true;}
  public double SurpriseWait=>Math.Max(0,S.surpriseBreakUntil-S.activeSeconds);
  public bool CanOpenSurprise(int count,bool ticket=false){
   if(Stage<2||SurpriseWait>0||(count!=1&&count!=10)||S.surpriseRun+count>10)return false;
   if(ticket)return count==1&&S.surpriseTickets>0;
   double cost=SurprisePrice*count;
   return S.surprisePaid+count<=10&&cost<=SurpriseBudgetLeft&&S.coins>=cost+100;
  }
  // Fixed categories; each eligible item within a category has equal probability.
  // Calendar exclusives and unreached stages never enter the random pool.
  public List<IdleItem> SurprisePool(int tier){
   var list=new List<IdleItem>();foreach(var i in IdleCatalog.Items){
    if(i.TreeOnly&&i.stage<=Stage&&i.TreeTier==tier)list.Add(i);
   }return list;
  }
  public List<GardenReward> OpenSurprises(int count,bool ticket=false){
   if(!CanOpenSurprise(count,ticket))return null;
   if(ticket)S.surpriseTickets--;else{double cost=SurprisePrice*count;S.coins-=cost;S.surpriseSpent+=cost;S.surprisePaid+=count;}
   var results=new List<GardenReward>();
   for(int n=0;n<count;n++){
    int tier=SurpriseTier(Next(),S.surprisePity);S.surprisePity=tier>=2?0:S.surprisePity+1;
    var r=new GardenReward{tier=tier,stars=1};
    if(tier==0&&Next()<.5){
     int resource=Math.Min(2,(int)(Next()*3));
     if(resource==0){r.coins=SurprisePrice*.25;Earn(r.coins);r.name="Монеты для заботы";}
     else if(resource==1){S.premiumFood=Math.Min(10000,S.premiumFood+1);r.name="Праздничная миска";}
     else {S.surpriseBuffUntil=Math.Max(S.surpriseBuffUntil,S.activeSeconds+900);r.name="Тёплые лапки · +10% тапа, 15 мин";}
    }else{
     var pool=SurprisePool(tier);
     if(pool.Count>0){var i=pool[Math.Min(pool.Count-1,(int)(Next()*pool.Count))];r.item=i.id;r.name=i.name;r.duplicate=S.owned.Contains(i.id);if(!r.duplicate)Grant(i.id);}
     else {r.name="Звёздный подарок";r.stars+=tier*2;}
     if(r.duplicate)r.stars+=new[]{1,2,5,10}[tier];
    }
    S.stars=(int)Math.Min(int.MaxValue,(long)S.stars+r.stars);results.Add(r);S.surpriseTotal=Math.Min(int.MaxValue-1,S.surpriseTotal+1);
    if(++S.surpriseRun>=10){S.surpriseRun=0;S.surpriseBreakUntil=S.activeSeconds+120;}
   }
   S.surpriseResults=results;return results;
  }
  public bool CraftSurprise(string id){
   var item=IdleCatalog.Find(id);if(item==null||item.TreeOnly||item.RewardDay>0||!CapyCompleteSets.Visible(item)||item.price<=0||Stage<item.stage||S.owned.Contains(id))return false;
   int price=SurpriseCraftPrice(item);if(S.stars<price)return false;S.stars-=price;Grant(id);return true;
  }
  public static int SurpriseCraftPrice(IdleItem i)=>i.IsSet?40:i.slot==5?20:10;
  public bool CanChooseSeed(string date)=>Stage>=2&&ValidDate(date,out _)&&string.CompareOrdinal(date,S.seedDate)>0;
  public GardenReward ChooseSeed(string date,int choice){
   if(choice<0||choice>2||!CanChooseSeed(date))return null;S.seedDate=date;
   int outcome=Math.Min(2,(int)(Next()*3));var r=new GardenReward{tier=0,stars=1,name="Мандариновый росток"};
   if(outcome==0){r.coins=25*Stage;Earn(r.coins);r.name="Монеты для заботы";}
   else if(outcome==1){S.premiumFood=Math.Min(10000,S.premiumFood+1);r.name="Праздничная миска";}
   else{r.stars=3;r.name="Три звёздочки";}
   S.stars=(int)Math.Min(int.MaxValue,(long)S.stars+r.stars);S.surpriseResults=new List<GardenReward>{r};return r;
  }
 }
}

