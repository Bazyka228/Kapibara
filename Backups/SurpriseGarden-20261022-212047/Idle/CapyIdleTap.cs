using System;
using UnityEngine;
using UnityEngine.UI;
#if PLUGIN_YG_2
using YG;
#endif
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  Text tapButtonText;
  bool adPending,adCredited;
  int autoRequestSerial;
  double adRequestedAt;
  string autoAdId;
  static string Minutes(double seconds){int value=(int)Math.Ceiling(Math.Max(0,seconds));return (value/60).ToString("00")+":"+(value%60).ToString("00");}
  bool RewardedAvailable {
   get {
#if PLUGIN_YG_2 && UNITY_EDITOR
    return true; // The official PluginYG editor simulator, never a real ad.
#elif PLUGIN_YG_2 && UNITY_WEBGL
    return YG2.isSDKEnabled;
#else
    return false;
#endif
   }
  }
  void TapPage(){
   var summary=Label(panel,"Tap summary","",24,93,432,69,24,"513821",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>summary.text="За тап: "+Number(Model.CareValue)+" монет\nДо бонуса: "+(10-Model.S.tapCombo)+" тапов");
   for(int n=0;n<3;n++){
    int kind=n;float y=173+n*113;
    var buy=Button(panel,"Tap upgrade "+n,"",24,y,432,102,()=>{
     if(Model.UpgradeTap(kind)){Save();Show("tap");Toast("Тап стал сильнее!");Sound(careClip,.13f);}
    },"F4CF80");
    var icon=Rect(buy.transform,"Tap upgrade icon",13,15,64,64);
    if(n==0){Oval(icon,"Paw pad",17,28,33,27,"D69872",2);for(int toe=0;toe<3;toe++)Oval(icon,"Paw toe",12+toe*17,10+(toe==1?-5:0),14,18,"E4B18A",1.5f);}
    else if(n==1){Star(icon,4,7,53,"E9BB63");Star(icon,40,39,18,"FFF0B6");}
    else {Oval(icon,"Coin medal",7,4,49,49,"E8BA60",2);Oval(icon,"Coin centre",14,11,35,35,"FFE295",1);Mark(icon,"10",14,12,35,31,22,"78522E");}
    var title=Label(buy.transform,"Upgrade title","",88,3,329,31,23);
    string[] effects={"+1 к основе каждого тапа","+10% к силе каждого тапа","Каждый 10-й тап: ещё +50%"};
    Label(buy.transform,"Upgrade effect",effects[n],88,35,329,30,22,"72543A");
    var price=Label(buy.transform,"Upgrade price","",88,67,329,30,22,"418355");
    liveLabels.Add(()=>{int level=Model.TapLevel(kind);title.text=IdleModel.TapUpgradeNames[kind]+" · "+level+"/"+IdleModel.TapUpgradeCaps[kind];
     price.text=level>=IdleModel.TapUpgradeCaps[kind]?"Полностью улучшено":Model.Stage<IdleModel.TapUpgradeStages[kind]?"Со стадии "+IdleModel.TapUpgradeStages[kind]:Number(Model.TapPrice(kind))+" монет";buy.interactable=Model.CanUpgradeTap(kind);});
   }
   PaperCard(panel,"Auto clicker card",24,522,432,306);
   Label(panel,"Auto title","АВТОКЛИКЕР · 20 МИНУТ",34,530,412,39,25,"513821",TextAnchor.MiddleCenter).font=bold;
   var progress=Label(panel,"Auto progress","",36,568,408,34,22,"72543A",TextAnchor.MiddleCenter);
   var pips=new SoftShape[5];for(int n=0;n<5;n++)pips[n]=Shape(panel,"Auto stamp "+n,119+n*51,607,38,10,"C8B68C",5);
   var status=Label(panel,"Auto status","",35,624,410,48,22,"513821",TextAnchor.MiddleCenter);
   var watch=Button(panel,"Auto watch","",40,683,400,64,RequestAutoAd,"55E6C1",23);
   Label(panel,"Auto explanation","1 автотап в секунду.\nПауза и офлайн не тратят время.",35,757,410,62,22,"72543A",TextAnchor.MiddleCenter);
   liveLabels.Add(()=>{
    bool active=Model.S.autoRemaining>0;double wait=Model.AutoAdWait(Utc);
    progress.text=active?"Помощник работает · "+Minutes(Model.S.autoRemaining):"Просмотры: "+Model.S.autoAdCount+" / 5 · прогресс сохранён";
    for(int n=0;n<5;n++)pips[n].color=C(active||n<Model.S.autoAdCount?"79A957":"C8B68C");
    status.text=adPending?"Ожидаем завершения видео":active?"+"+Number(Model.CareValue)+" монет / с":wait>0?"Следующее видео через "+Minutes(wait):RewardedAvailable?"Каждое видео запускаешь сам":"Видео сейчас недоступно";
    watch.interactable=!active&&!adPending&&wait<=0&&RewardedAvailable;
    watch.GetComponentInChildren<Text>().text=active?"АВТОКЛИКЕР ВКЛЮЧЁН":"ВИДЕО · ШАГ "+(Model.S.autoAdCount+1)+" ИЗ 5";
   });
  }
  bool PrepareAutoAd(){
   if(Model==null||PresentationPaused||!RewardedAvailable)return false;
#if PLUGIN_YG_2
   if(YG2.nowAdsShow)return false;
#endif
   autoRequestSerial=Model.BeginAutoAd(Utc);if(autoRequestSerial==0)return false;
   autoAdId="capy_auto_"+autoRequestSerial;adCredited=false;adPending=true;adRequestedAt=Time.realtimeSinceStartupAsDouble;Release();Save();return true;
  }
  void RequestAutoAd(){
   if(!PrepareAutoAd())return;
#if PLUGIN_YG_2
   try{YG2.RewardedAdvShow(autoAdId);}catch(Exception){AutoAdError();}
#else
   AutoAdError();
#endif
  }
  void AutoAdReward(string id){
   if(!adPending||adCredited||id!=autoAdId||Model==null)return;
   if(!Model.CompleteAutoAd(autoRequestSerial,Utc))return;
   adCredited=true;Save();Toast(Model.S.autoRemaining>0?"Автокликер включён на 20 минут!":"Просмотр засчитан · "+Model.S.autoAdCount+" / 5");
  }
  void AutoAdClosed(){
   if(!adPending)return;
   if(!adCredited)Toast("Просмотр не завершён. Прогресс сохранён");
   adPending=false;Model?.CancelAutoAd();Save();
  }
  void AutoAdError(){
   if(!adPending)return;adPending=false;Model?.CancelAutoAd();Save();Toast("Видео сейчас недоступно. Попробуй позже");
#if PLUGIN_YG_2
   // This SDK version reports errors separately from close: release our opened ad's pause.
   if(YG2.nowRewardAdv)YG.Insides.YGInsides.CloseRewardedAdv();
#endif
  }
  void CheckAutoAdLoading(){
   if(!adPending||adCredited||Time.realtimeSinceStartupAsDouble-adRequestedAt<30)return;
#if PLUGIN_YG_2
   if(YG2.nowAdsShow)return; // Never time out an opened video.
#endif
   AutoAdError();
  }
  void BindAutoAds(){
#if PLUGIN_YG_2
   YG2.onRewardAdv-=AutoAdReward;YG2.onCloseRewardedAdv-=AutoAdClosed;YG2.onErrorRewardedAdv-=AutoAdError;
   YG2.onRewardAdv+=AutoAdReward;YG2.onCloseRewardedAdv+=AutoAdClosed;YG2.onErrorRewardedAdv+=AutoAdError;
#endif
  }
  void UnbindAutoAds(){
#if PLUGIN_YG_2
   YG2.onRewardAdv-=AutoAdReward;YG2.onCloseRewardedAdv-=AutoAdClosed;YG2.onErrorRewardedAdv-=AutoAdError;
#endif
   adPending=false;Model?.CancelAutoAd();
  }
 }
}
