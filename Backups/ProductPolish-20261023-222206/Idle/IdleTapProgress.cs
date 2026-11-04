using System;
namespace Capybara67.Idle {
 public sealed partial class IdleModel {
  public const double AutoDuration=1200,AutoAdInterval=120;
  public static readonly string[] TapUpgradeNames={"Ловкие лапки","Монетная искра","Золотое касание"};
  public static readonly int[] TapUpgradeCaps={20,10,10};
  public static readonly int[] TapUpgradeStages={1,2,3};
  static readonly double[] TapPrices={25,150,500},TapPriceGrowth={1.55,1.7,1.9};
  int pendingAutoAd;bool pendingSurpriseAd;
  void RepairTapProgress(){
   S.tapPower=Math.Max(0,Math.Min(20,S.tapPower));S.tapMultiplier=Math.Max(0,Math.Min(10,S.tapMultiplier));S.tapBonus=Math.Max(0,Math.Min(10,S.tapBonus));
   S.tapCombo=Math.Max(0,Math.Min(9,S.tapCombo));S.autoAdCount=Math.Max(0,Math.Min(4,S.autoAdCount));
   S.autoAdSerial=Math.Max(0,Math.Min(1000000000,S.autoAdSerial));S.autoAdRewardSerial=Math.Max(0,Math.Min(S.autoAdSerial,S.autoAdRewardSerial));
   S.autoRemaining=Clean(S.autoRemaining,AutoDuration);S.autoPhase=S.autoRemaining>0?Clean(S.autoPhase,.999999):0;S.autoAdNextUtc=Clean(S.autoAdNextUtc,253402300799);
  }
  public int TapLevel(int kind)=>kind==0?S.tapPower:kind==1?S.tapMultiplier:kind==2?S.tapBonus:0;
  public double TapPrice(int kind)=>kind<0||kind>2||TapLevel(kind)>=TapUpgradeCaps[kind]?0:Math.Ceiling(TapPrices[kind]*Math.Pow(TapPriceGrowth[kind],TapLevel(kind)));
  public bool CanUpgradeTap(int kind)=>kind>=0&&kind<3&&TapLevel(kind)<TapUpgradeCaps[kind]&&Stage>=TapUpgradeStages[kind]&&S.coins>=TapPrice(kind);
  public bool UpgradeTap(int kind){if(!CanUpgradeTap(kind))return false;S.coins-=TapPrice(kind);if(kind==0)S.tapPower++;else if(kind==1)S.tapMultiplier++;else S.tapBonus++;return true;}
  public double AutoAdWait(double utc)=>Math.Max(0,S.autoAdNextUtc-utc);
  public int BeginAutoAd(double utc){
   if(double.IsNaN(utc)||double.IsInfinity(utc)||utc<=0||pendingAutoAd!=0||S.autoRemaining>0||AutoAdWait(utc)>0||S.autoAdSerial>=1000000000)return 0;
   pendingSurpriseAd=false;pendingAutoAd=++S.autoAdSerial;return pendingAutoAd;
  }
  public bool CompleteAutoAd(int serial,double utc){
   if(pendingSurpriseAd||serial==0||serial!=pendingAutoAd||serial<=S.autoAdRewardSerial||S.autoRemaining>0||double.IsNaN(utc)||double.IsInfinity(utc))return false;
   S.autoAdRewardSerial=serial;S.autoAdCount++;S.autoAdNextUtc=Clean(utc,253402300799)+AutoAdInterval;
   if(S.autoAdCount>=5){S.autoAdCount=0;S.autoRemaining=AutoDuration;S.autoPhase=0;}
   return true;
  }
  public int BeginSurpriseAd(double utc,string date){
   if(!S.surpriseAds||!CanAdSurprise(date)||double.IsNaN(utc)||double.IsInfinity(utc)||utc<=0||pendingAutoAd!=0||AutoAdWait(utc)>0||S.autoAdSerial>=1000000000)return 0;
   pendingSurpriseAd=true;pendingAutoAd=++S.autoAdSerial;return pendingAutoAd;
  }
  public bool CompleteSurpriseAd(int serial,double utc,string date){
   if(!pendingSurpriseAd||serial==0||serial!=pendingAutoAd||serial<=S.autoAdRewardSerial||double.IsNaN(utc)||double.IsInfinity(utc)||!ClaimAdSurprise(date))return false;
   S.autoAdRewardSerial=serial;S.autoAdNextUtc=Clean(utc,253402300799)+AutoAdInterval;return true;
  }
  public void CancelAutoAd(){pendingAutoAd=0;pendingSurpriseAd=false;}
 }
}
