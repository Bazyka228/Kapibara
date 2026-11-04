using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  RectTransform welcomePose;
  CapyPortraitMotion welcomeEyes;
  float welcomeAge,welcomePetAge=10;
  CanvasGroup welcomeHeart;
  Tween welcomeHeartTween;
  bool welcomeOfflinePending,welcomeGiftPending;

  void WelcomePage(){
   welcomeAge=0;welcomePetAge=10;
   var sky=Rect(panel,"Welcome sky",0,0,480,850).gameObject.AddComponent<CapySceneryGradient>();
   sky.top=sky.bottom=C("A9DAD0");sky.raycastTarget=false;
   var garden=Rect(panel,"Welcome garden",0,-111,480,850);DrawScene(garden,null);
   // Keep this entrance garden sunny even when the saved room is a night scene.
   Leaf(panel,7,52,39,68,"77A653",-23);Leaf(panel,439,49,37,70,"96BC79",23);
   WoodPanel(panel,"Welcome title board",28,73,424,143);
   Label(panel,"Welcome eyebrow","ТВОЙ УЮТНЫЙ САД",38,27,404,37,22,"513821",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome title","КАПИБАРА",42,86,396,69,46,"FFF0C8",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome subtitle","6–7 · ЛОВИ ВАЙБ",46,154,388,45,28,"FFF0C8",TextAnchor.MiddleCenter).font=bold;
   Stroke(panel,"Sign cord",66,0,66,73,"7F6247",3);Stroke(panel,"Sign cord",414,0,414,73,"7F6247",3);
   var portrait=Rect(panel,"Welcome capybara",120,300,240,250);DrawAvatar(portrait);
   welcomePose=portrait.Find("Illustrated pose") as RectTransform;
   welcomeEyes=portrait.GetComponentInChildren<CapyPortraitMotion>();
   var heart=Rect(portrait,"Welcome heart",211,53,40,44);EffectMotif(heart,2,"EBA2AD");
   welcomeHeart=heart.gameObject.AddComponent<CanvasGroup>();welcomeHeart.alpha=0;welcomeHeart.blocksRaycasts=false;
   Button(panel,"Welcome pet","",82,301,316,250,()=>{
    welcomePetAge=0;if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();welcomeHeart.alpha=1;
    if(!Model.S.calm)welcomeHeartTween=tweens.Track(DOTween.Sequence().AppendInterval(.65f).Append(welcomeHeart.DOFade(0,.4f)),heart.gameObject);
    Sound(careClip,.1f);
   },"FFFFFF00");
   Flower(panel,29,537,"E8AF9B",1.3f);Flower(panel,424,534,"FFF2C7",1.25f);
   PaperCard(panel,"Welcome invitation",20,581,440,244);
   Label(panel,"Welcome promise","Корми · заботься · наряжай",36,597,408,42,24,"513821",TextAnchor.MiddleCenter).font=bold;
   Label(panel,"Welcome progress",Model.S.starter?IdleModel.Stages[Model.Stage-1]+" · этап "+Model.Stage+" / 5":"Первая миска — в подарок",36,640,408,37,22,"72543A",TextAnchor.MiddleCenter);
   Button(panel,"Welcome start",Model.S.starter?"ПРОДОЛЖИТЬ":"НАЧАТЬ ЗАБОТУ",40,693,400,76,BeginFromWelcome,"55E6C1",26);
   Label(panel,"Welcome save note","Твой сад сохраняется автоматически",31,781,418,34,22,"72543A",TextAnchor.MiddleCenter);
   if(!Model.S.calm){pageOpacity.alpha=.75f;pageTween=tweens.Track(pageOpacity.DOFade(1,.35f).SetEase(Ease.OutQuad),panel.gameObject);}
  }
  void BeginFromWelcome(){
   if(Page!="welcome")return;
   Show(welcomeOfflinePending?"offline":"home");
  }
  void AnimateWelcome(float dt){
   if(Page!="welcome"||PresentationPaused||Model==null)return;
   welcomeAge+=dt;welcomePetAge+=dt;
   bool calm=Model.S.calm;
   if(calm&&welcomeHeart){if(welcomeHeartTween.IsActive())welcomeHeartTween.Kill();welcomeHeart.alpha=welcomePetAge<1?1:0;}
   if(welcomeEyes)welcomeEyes.SetBlink(calm?0:Mathf.Max(BlinkAt(welcomeAge),JoyBlink(welcomePetAge)));
   if(welcomePose){
    float breath=calm?0:Mathf.Sin(welcomeAge*1.8f)*.006f;
    var scale=new Vector3(1-breath*.3f,1+breath,1);welcomePose.localScale=scale;
    welcomePose.anchoredPosition=new Vector2(120*(1-scale.x),-244*(1-scale.y));
   }
  }
 }
}
