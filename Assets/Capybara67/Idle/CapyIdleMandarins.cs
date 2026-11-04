using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  string mandarinMessage="";
  Text mandarinHud;
  RectTransform currencyChoice;
  Action<bool> pendingPurchase;
  Action currencyRefresh;
  static string MandarinAmount(int count){int tens=count%100,ones=count%10;return count+" "+(tens>=11&&tens<=14?"мандаринов":ones==1?"мандарин":ones>=2&&ones<=4?"мандарина":"мандаринов");}
  void PrepareMandarinShop(){Model.RefreshDay(Date);Model.RefreshSurprises(Date);Save();}
  void BuildMandarinHud(){
   var b=Button(home,"Mandarin wallet","",84,52,294,67,()=>Show("mandarins"),"FFFFFF00");
   ActivityMandarin(b.transform,6,22,32);mandarinHud=Label(b.transform,"Mandarin balance","",46,15,132,46,24,"513821");
   mandarinHud.font=bold;
   var plus=Button(home,"Mandarin shop plus","",17,52,67,67,()=>Show("mandarins"),"FFFFFF00");
   var visual=Rect(plus.transform,"Compact plus visual",16,20,36,36);
   visual.pivot=new Vector2(.5f,.5f);visual.anchoredPosition+=new Vector2(18,-18);
   InkBox(visual,"Compact plus face",0,0,36,36,"F4CF80",10,2);
   Shape(visual,"Compact plus highlight",8,4,20,2,"FFF0C8",1);
   Mark(visual,"+",0,-2,36,36,24,"513821");
   plus.gameObject.AddComponent<MandarinPlusPress>().Initialize(this,plus,visual);
  }
  void MandarinShopPage(){
   PrepareMandarinShop();
   Label(panel,"Mandarin shop balance","У тебя: "+Model.M.balance+" мандаринов",24,98,432,38,25,align:TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Mandarin exchange rate","1 мандарин = 1 000 монет",24,139,432,34,23,align:TextAnchor.MiddleCenter);
   for(int n=0;n<IdleModel.MandarinPacks.Length;n++){
    var pack=IdleModel.MandarinPacks[n];float x=24+n%2*222,y=184+n/2*183;
    PaperCard(panel,"Mandarin pack "+pack.id,x,y,210,174);
    var art=Rect(panel,"Pack picture "+pack.id,x+12,y+10,76,68);
    if(n<2){ActivityMandarin(art,5,8,48);if(n==1)ActivityMandarin(art,32,28,35);}
    else {InkBox(art,"Harvest basket",4,30,68,34,n<4?"BB8F5D":"A3BA78",6,2);for(int j=0;j<3;j++)ActivityMandarin(art,4+j*20,9+(j%2)*8,30);Stroke(art,"Basket rim",6,34,69,34,Ink,3);}
    Label(panel,"Pack amount",pack.Total.ToString(),x+89,y+13,109,53,30).font=bold;
    Label(panel,"Pack coin value",Number(pack.Total*(double)IdleModel.CoinsPerMandarin)+" монет",x+10,y+78,190,34,21,align:TextAnchor.MiddleCenter);
    // Draft RUB prices are not a payment catalogue. Checkout stays disabled until SDK products exist.
    Button(panel,"Pack unavailable "+pack.id,"Проект: "+pack.price+" ₽",x+10,y+118,190,46,null,"D4BD95",22).interactable=false;
   }
   Label(panel,"Payment unavailable","Оплата пока недоступна.\nЦены наборов — предварительные.",24,744,432,78,22,align:TextAnchor.MiddleCenter);
  }
  void CloseCurrencyChoice(){
   pendingPurchase=null;if(currencyRefresh!=null){liveLabels.Remove(currencyRefresh);currencyRefresh=null;}if(!currencyChoice)return;currencyChoice.gameObject.SetActive(false);Destroy(currencyChoice.gameObject);currencyChoice=null;
  }
  void ChooseCurrency(string title,double price,Action<bool> purchase,Func<bool,bool> allowed=null){
   if(!loaded||PresentationPaused||currencyChoice||sectionHelp||purchase==null||!Model.CanPayEither(price))return;
   Release();currencyChoice=Rect(board,"Currency choice",0,0,480,850);pendingPurchase=purchase;
   Shape(currencyChoice,"Payment backdrop",0,0,480,850,"172B24D9",0).raycastTarget=true;
   PaperCard(currencyChoice,"Payment paper",16,130,448,590);
   Label(currencyChoice,"Payment title","ЧЕМ ЗАПЛАТИМ?",30,151,420,49,28,align:TextAnchor.MiddleCenter).font=bold;
   Label(currencyChoice,"Payment item",title,37,203,406,75,25,align:TextAnchor.MiddleCenter);
   var gold=Button(currencyChoice,"Pay with coins","",36,299,408,92,()=>ConfirmCurrency(false,price,allowed),"F4CF80");
   Coin(gold.transform,17,20,52);Label(gold.transform,"Coin payment",Number(price)+" монет",88,7,307,40,26).font=bold;
   var goldBalance=Label(gold.transform,"Coin wallet","",88,49,307,34,21);
   var fruit=Button(currencyChoice,"Pay with mandarins","",36,409,408,92,()=>ConfirmCurrency(true,price,allowed),"A7CF91");
   ActivityMandarin(fruit.transform,17,20,52);Label(fruit.transform,"Mandarin payment",MandarinAmount(IdleModel.MandarinCost(price)),88,7,307,40,26).font=bold;
   var fruitBalance=Label(fruit.transform,"Mandarin wallet amount","",88,49,307,34,21);
   Label(currencyChoice,"Payment change","1 мандарин = 1 000 монет\nСдача: "+Number(IdleModel.MandarinChange(price))+" монет",36,519,408,76,22,align:TextAnchor.MiddleCenter);
   Button(currencyChoice,"Cancel currency choice","ОТМЕНА",36,619,408,72,CloseCurrencyChoice,size:25);
   // Keep affordability live while passive income changes the wallet. The detached callback is harmless.
   Action refresh=()=>{if(!currencyChoice||!gold||!fruit)return;gold.interactable=Model.CanPay(price)&&(allowed==null||allowed(false));fruit.interactable=Model.CanPay(price,true)&&(allowed==null||allowed(true));goldBalance.text="У тебя: "+Number(Model.S.coins);fruitBalance.text="У тебя: "+Model.M.balance;};
   currencyRefresh=refresh;liveLabels.Add(refresh);refresh();
   OpenOverlayFinish(currencyChoice);
  }
  void ConfirmCurrency(bool mandarins,double price,Func<bool,bool> allowed){
   if(pendingPurchase==null||!currencyChoice||!Model.CanPay(price,mandarins)||(allowed!=null&&!allowed(mandarins)))return;
   // Invalidate before callbacks or navigation: a double click cannot buy twice.
   var purchase=pendingPurchase;CloseCurrencyChoice();purchase(mandarins);
  }
  void MandarinStockPage(){
   PrepareMandarinShop();ActivityMandarin(panel,174,125,132);
   var info=Label(panel,"Mandarin stock info","",24,291,432,170,23,align:TextAnchor.MiddleCenter);
   var use=Button(panel,"Use mandarin stock","ДАТЬ ПОРЦИЮ",24,486,432,72,()=>{if(Model.UseMandarinStock()){Save();mandarinMessage="Капибара перекусила!\nСытость и настроение пополнены.";Show("mandarin-result");}},"55E6C1",23);
   liveLabels.Add(()=>{info.text="Порций в запасе: "+Model.M.stock+"\nСытость: "+Math.Ceiling(Model.S.fullness)+" / 100\nНастроение: "+Math.Ceiling(Model.S.mood)+" / 100\n+40 сытости · +10 настроения";use.interactable=Model.M.stock>0&&Model.ActionWait<=0&&(Model.S.fullness<100||Model.S.mood<100);});
   var boosts=Label(panel,"Mandarin active boosts","",24,596,432,115,22,align:TextAnchor.MiddleCenter);
   liveLabels.Add(()=>boosts.text="Усиление тапа: "+Minutes(Model.MandarinTapLeft)+"\nКормлений с добавкой: "+(Model.M.growthStage==Model.Stage?Model.M.growthCharges:0)+"\nТап по капибаре приносит монеты.");
   Button(panel,"Mandarin free care","К ЗАБОТЕ",24,750,432,76,()=>Show("care"));
  }
  void MandarinResultPage(){
   ActivityMandarin(panel,174,180,132);Star(panel,119,180,32,"F4CF80");Star(panel,321,273,26,"F4CF80");
   Label(panel,"Mandarin receipt",mandarinMessage,30,360,420,140,26,align:TextAnchor.MiddleCenter);
   Label(panel,"Mandarin receipt balance","Баланс: "+Model.M.balance+" мандаринок",24,530,432,58,24,align:TextAnchor.MiddleCenter);
   Button(panel,"Mandarin result shop","В МАГАЗИН",24,650,432,72,()=>Show("mandarins"),"55E6C1");
   Button(panel,"Mandarin result home","К КАПИБАРЕ",24,750,432,76,()=>Show("home"));
  }
 }
 // Only the small illustrated face moves; the generous touch target remains stationary.
 public sealed class MandarinPlusPress : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  CapyIdleGame game;Button button;RectTransform visual;CanvasGroup opacity;
  Vector2 origin;Tween motion;float amount;bool pressed;int pointer;
  public void Initialize(CapyIdleGame owner,Button target,RectTransform face){
   ResetVisual();game=owner;button=target;visual=face;origin=face.anchoredPosition;
   opacity=face.GetComponent<CanvasGroup>();if(!opacity)opacity=face.gameObject.AddComponent<CanvasGroup>();
   opacity.blocksRaycasts=false;opacity.interactable=false;opacity.alpha=1;
   Apply(0);
  }
  bool Available=>isActiveAndEnabled&&game&&game.Model!=null&&!game.PresentationPaused&&Time.timeScale>0&&button&&button.IsInteractable()&&visual&&visual.gameObject.activeInHierarchy;
  void Apply(float value){
   amount=value;if(!visual)return;
   visual.localScale=Vector3.one*(1-.12f*value);visual.anchoredPosition=origin+Vector2.down*(2*value);
  }
  void Stop(){if(motion.IsActive())motion.Kill();motion=null;}
  void ResetVisual(){Stop();pressed=false;Apply(0);if(opacity)opacity.alpha=1;}
  void Animate(bool down){
   Stop();
   if(!Available){ResetVisual();return;}
   if(game.CosmeticReduced||!game.Tweens){Apply(0);opacity.alpha=down?.82f:1;return;}
   opacity.alpha=1;
   // Immediate feedback still reads when down/up arrive before the next animation frame.
   if(down&&amount<.3f)Apply(.3f);
   motion=game.Tweens.Track(DOTween.To(()=>amount,Apply,down?1:0,down?.1f:.18f).SetEase(Ease.OutCubic),gameObject);
  }
  public void OnPointerDown(PointerEventData e){
   if(e.button!=PointerEventData.InputButton.Left||pressed||!Available)return;
   pointer=e.pointerId;pressed=true;Animate(true);
  }
  public void OnPointerUp(PointerEventData e){if(!pressed||pointer!=e.pointerId)return;pressed=false;Animate(false);}
  public void OnPointerExit(PointerEventData e){if(!pressed||pointer!=e.pointerId)return;pressed=false;Animate(false);}
  void LateUpdate(){
   if(!Available){ResetVisual();return;}
   if(game.CosmeticReduced){Stop();Apply(0);opacity.alpha=pressed?.82f:1;}
  }
  void OnApplicationFocus(bool focused){if(!focused)ResetVisual();}
  void OnApplicationPause(bool paused){if(paused)ResetVisual();}
  void OnDisable(){ResetVisual();}
  void OnDestroy(){ResetVisual();}
 }
}
