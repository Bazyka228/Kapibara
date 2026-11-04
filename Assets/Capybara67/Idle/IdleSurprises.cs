using System;
using System.Collections.Generic;
namespace Capybara67.Idle {
 public enum GardenBlock { None, Stage, Pause, Count, Series, Tickets, Daily, Budget, Reserve }
 [Serializable] public sealed class GardenReward {
  public int tier,stars; // stars is read only to migrate old saved receipts; never awarded.
  public double coins;public string item="",form="",name="";public bool duplicate;
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
   foreach(var r in S.surpriseResults){
    r.coins=Clean(r.coins+25.0*Math.Max(0,r.stars));r.stars=0;
    if(r.name=="Звёздный подарок"||r.name=="Три звёздочки")r.name="Монетный подарок";
   }
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
  public GardenBlock SurpriseBlock(int count,bool ticket=false,bool mandarins=false){
   if(count!=1&&count!=10||ticket&&count!=1)return GardenBlock.Count;
   if(Stage<2)return GardenBlock.Stage;
   if(SurpriseWait>0)return GardenBlock.Pause;
   if(S.surpriseRun+count>10)return GardenBlock.Series;
   if(ticket)return S.surpriseTickets>0?GardenBlock.None:GardenBlock.Tickets;
   double cost=SurprisePrice*count;
   if(S.surprisePaid+count>10)return GardenBlock.Daily;
   if(mandarins)return CanPay(cost,true)?GardenBlock.None:GardenBlock.Reserve;
   if(cost>SurpriseBudgetLeft)return GardenBlock.Budget;
   return S.coins>=cost+100?GardenBlock.None:GardenBlock.Reserve;
  }
  public bool CanOpenSurprise(int count,bool ticket=false)=>SurpriseBlock(count,ticket)==GardenBlock.None;
  public int SurprisePaidAvailable=>Stage<2||SurpriseWait>0?0:(int)Math.Min(Math.Min(SurpriseLeft,10-S.surpriseRun),Math.Min(Math.Floor(SurpriseBudgetLeft/SurprisePrice),Math.Floor(Math.Max(0,S.coins-100)/SurprisePrice)));
  // Rarity odds stay fixed. Item selection prefers missing items of that rarity.
  // Calendar exclusives and unreached stages never enter the random pool.
  public List<IdleItem> SurprisePool(int tier){
   var list=new List<IdleItem>();foreach(var i in IdleCatalog.Items){
    if(i.TreeOnly&&i.stage<=Stage&&i.TreeTier==tier)list.Add(i);
   }return list;
  }
  public List<IdleItem> SurpriseDrawPool(int tier){
   var all=SurprisePool(tier);var missing=all.FindAll(i=>!S.owned.Contains(i.id));return missing.Count>0?missing:all;
  }
  public double SurpriseItemGroupChance(int tier)=>S.surprisePity>=9?(tier==2?.97:tier==3?.03:0):tier==0?.30:tier==1?.25:tier==2?.12:tier==3?.03:0;
  public double SurpriseItemChance(string id){
   var item=IdleCatalog.Find(id);if(item==null||!item.TreeOnly)return 0;
   var pool=SurpriseDrawPool(item.TreeTier);return pool.Exists(i=>i.id==id)?SurpriseItemGroupChance(item.TreeTier)/pool.Count:0;
  }
  public List<GardenReward> OpenSurprises(int count,bool ticket=false,bool mandarins=false){
   if(SurpriseBlock(count,ticket,mandarins)!=GardenBlock.None)return null;
   if(ticket)S.surpriseTickets--;else{double cost=SurprisePrice*count;if(!Pay(cost,mandarins))return null;if(!mandarins)S.surpriseSpent+=cost;S.surprisePaid+=count;}
   return ResolveSurprises(count);
  }
  List<GardenReward> ResolveSurprises(int count){
   var results=new List<GardenReward>();
   for(int n=0;n<count;n++){
    int tier=SurpriseTier(Next(),S.surprisePity);S.surprisePity=tier>=2?0:S.surprisePity+1;
    var r=new GardenReward{tier=tier,coins=25};
    if(tier==0&&Next()<.5){
     int resource=Math.Min(2,(int)(Next()*3));
     if(resource==0){r.coins+=SurprisePrice*.25;r.name="Монеты для заботы";}
     else if(resource==1){S.premiumFood=Math.Min(10000,S.premiumFood+1);r.name="Праздничная миска";}
     else {S.surpriseBuffUntil=Math.Max(S.surpriseBuffUntil,S.activeSeconds+900);r.name="Тёплые лапки · +10% тапа, 15 мин";}
    }else{
     var pool=SurpriseDrawPool(tier);
     if(pool.Count>0){var i=pool[Math.Min(pool.Count-1,(int)(Next()*pool.Count))];r.item=i.id;r.name=i.name;r.duplicate=S.owned.Contains(i.id);if(!r.duplicate)Grant(i.id);}
     else {r.name="Монетный подарок";r.coins+=tier*50;}
     if(r.duplicate)r.coins+=new[]{25,50,125,250}[tier];
    }
    Earn(r.coins);results.Add(r);S.surpriseTotal=Math.Min(int.MaxValue-1,S.surpriseTotal+1);
    if(++S.surpriseRun>=10){S.surpriseRun=0;S.surpriseBreakUntil=S.activeSeconds+120;}
   }
   S.surpriseResults=results;return results;
  }
  public bool CanChooseSeed(string date)=>Stage>=2&&ValidDate(date,out _)&&string.CompareOrdinal(date,S.seedDate)>0;
  public GardenReward ChooseSeed(string date,int choice){
   if(choice<0||choice>2||!CanChooseSeed(date))return null;S.seedDate=date;
   int outcome=Math.Min(2,(int)(Next()*3));var r=new GardenReward{tier=0,coins=25,name="Мандариновый росток"};
   if(outcome==0){r.coins+=25*Stage;r.name="Монеты для заботы";}
   else if(outcome==1){S.premiumFood=Math.Min(10000,S.premiumFood+1);r.name="Праздничная миска";}
   else{r.coins=75;r.name="Монетный подарок";}
   Earn(r.coins);S.surpriseResults=new List<GardenReward>{r};return r;
  }
 }
}

