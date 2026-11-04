using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  // Original flat cartoon geometry; retain animation without changing its material.
  static void FinishScenery(Transform root,bool animate=true){
   if(animate&&!root.GetComponent<GardenAmbientMotion>())root.gameObject.AddComponent<GardenAmbientMotion>().Collect();
  }
  float budgetAge,budgetSlow,budgetFast;bool lightDecor;
  public bool DecorativeReduced=>CosmeticReduced||lightDecor;
  void UpdateVisualBudget(float dt){
   if(PresentationPaused)return;
   if(budgetAge==0)lightDecor=SystemInfo.systemMemorySize>0&&SystemInfo.systemMemorySize<3000;
   budgetAge+=dt;
   if(budgetAge<3||Time.unscaledDeltaTime>.25f)return; // Ignore imports and focus stalls.
   budgetSlow=Time.unscaledDeltaTime>.029f?budgetSlow+dt:Mathf.Max(0,budgetSlow-dt*.5f);
   budgetFast=Time.unscaledDeltaTime<.023f?budgetFast+dt:0;
   if(budgetSlow>4){lightDecor=true;budgetSlow=0;}
   if(lightDecor&&budgetFast>15&&SystemInfo.systemMemorySize>=3000){lightDecor=false;budgetFast=0;}
  }
  void OpenPageFinish(){
   if(!panel||Page=="home")return;
   // Alpha never moves hit boxes. Every page, including early-return pages, gets the same transition.
   if(pageTween.IsActive())pageTween.Kill();
   pageOpacity.alpha=1;
   if(CosmeticReduced)return;
   pageOpacity.alpha=.72f;
   pageTween=tweens.Track(pageOpacity.DOFade(1,.26f).SetEase(Ease.OutCubic),panel.gameObject);
  }
  void OpenOverlayFinish(RectTransform overlay){
   if(CosmeticReduced)return;
   var opacity=overlay.gameObject.GetComponent<CanvasGroup>();if(!opacity)opacity=overlay.gameObject.AddComponent<CanvasGroup>();opacity.alpha=.35f;
   tweens.Track(opacity.DOFade(1,.24f).SetEase(Ease.OutCubic),overlay.gameObject);
  }
  void GrowthCelebration(){
   RewardFeedback(new Vector2(240,-320),0,false);
   if(lastNumber>=0){var n=rewardNumbers[lastNumber];n.text.text="Новый этап!";rewardNumbers[lastNumber]=n;}
  }
  struct RewardMote {public RectTransform rect;public CanvasGroup opacity;public Vector2 from,to;public float age,duration;public bool active;}
  struct RewardNumber {public Text text;public CanvasGroup opacity;public Vector2 from;public float age;public double amount;public bool active;}
  RewardMote[] rewardMotes;RewardNumber[] rewardNumbers;
  int nextMote,nextNumber,lastNumber=-1;float lastRewardAge=10,coinPulse;
  RectTransform finishLayer;SoftShape clickHalo;CanvasGroup haloOpacity;float haloAge=10;
  Vector2 haloOrigin;
  void BuildFinishVfx(){
   finishLayer=Rect(board,"Pooled garden feedback",0,0,480,850);
   var layerGroup=finishLayer.gameObject.AddComponent<CanvasGroup>();layerGroup.blocksRaycasts=false;layerGroup.interactable=false;
   rewardMotes=new RewardMote[12];rewardNumbers=new RewardNumber[4];
   clickHalo=Shape(finishLayer,"Warm touch halo",0,0,94,94,"FFDC8340",0,true);
   haloOpacity=clickHalo.gameObject.AddComponent<CanvasGroup>();haloOpacity.alpha=0;
   Shape(clickHalo.transform,"Halo centre",14,14,66,66,"FFF1B326",0,true);
   for(int i=0;i<rewardMotes.Length;i++){
    var r=Rect(finishLayer,"Reusable gold mote "+i,0,0,18,18);Coin(r,0,0,18);FinishScenery(r,false);
    var opacity=r.gameObject.AddComponent<CanvasGroup>();opacity.alpha=0;
    rewardMotes[i]=new RewardMote{rect=r,opacity=opacity};r.gameObject.SetActive(false);
   }
   for(int i=0;i<rewardNumbers.Length;i++){
    var text=Label(finishLayer,"Reusable reward number "+i,"",0,0,190,44,25,"FFF3C7",TextAnchor.MiddleCenter);text.font=bold;
    var shadow=text.gameObject.AddComponent<Shadow>();shadow.effectColor=C("65432DE0");shadow.effectDistance=new Vector2(1,-2);shadow.useGraphicAlpha=true;
    var opacity=text.gameObject.AddComponent<CanvasGroup>();opacity.alpha=0;
    rewardNumbers[i]=new RewardNumber{text=text,opacity=opacity};text.gameObject.SetActive(false);
   }
  }
  void TouchFeedback(double amount){
   if(!finishLayer)return;
   Vector2 origin=new Vector2(235,-390);
   // Aggregate fast taps into one legible amount; earning itself is never throttled.
   if(lastRewardAge<.13f&&lastNumber>=0&&rewardNumbers[lastNumber].active){
    var n=rewardNumbers[lastNumber];n.amount+=amount;n.text.text="+"+Number(n.amount);rewardNumbers[lastNumber]=n;return;
   }
   RewardFeedback(origin,amount,false);lastRewardAge=0;
  }
  void ClearFinishVfx(){
   if(rewardMotes==null)return;
   for(int i=0;i<rewardMotes.Length;i++){var m=rewardMotes[i];m.active=false;m.rect.gameObject.SetActive(false);rewardMotes[i]=m;}
   for(int i=0;i<rewardNumbers.Length;i++){var n=rewardNumbers[i];n.active=false;n.text.gameObject.SetActive(false);rewardNumbers[i]=n;}
   haloAge=lastRewardAge=10;lastNumber=-1;if(haloOpacity)haloOpacity.alpha=0;
  }
  void RewardFeedback(Vector2 position,double amount,bool purchase){
   if(!finishLayer||PresentationPaused)return;
   finishLayer.SetAsLastSibling();
   position.x=Mathf.Clamp(position.x,98,382);position.y=Mathf.Clamp(position.y,-740,-125);
   int index=nextNumber++%rewardNumbers.Length;lastNumber=index;
   var number=rewardNumbers[index];number.from=position+new Vector2(-95,25);number.age=0;number.amount=amount;number.active=true;
   number.text.text=purchase?"Готово!":amount>0?"+"+Number(amount):"Красота!";number.text.gameObject.SetActive(true);number.opacity.alpha=1;number.text.rectTransform.anchoredPosition=number.from;rewardNumbers[index]=number;
   coinPulse=1;
   if(DecorativeReduced)return;
   haloAge=0;haloOrigin=position-new Vector2(47,-47);clickHalo.rectTransform.anchoredPosition=haloOrigin;haloOpacity.alpha=1;
   for(int i=0;i<4;i++){
    int m=nextMote++%rewardMotes.Length;var mote=rewardMotes[m];mote.from=position+new Vector2((i-1.5f)*9,0);mote.to=purchase?position+new Vector2((i-1.5f)*42,45+i%2*25):new Vector2(42+i*2,-32);
    mote.age=-i*.045f;mote.duration=purchase?.7f:.8f;mote.active=true;mote.rect.anchoredPosition=mote.from;mote.rect.gameObject.SetActive(true);mote.opacity.alpha=0;rewardMotes[m]=mote;
   }
  }
  void UpdateFinishVfx(float dt){
   UpdateVisualBudget(dt);if(PresentationPaused||rewardMotes==null)return;
   dt=Mathf.Min(dt,.1f);lastRewardAge+=dt;haloAge+=dt;coinPulse=Mathf.Max(0,coinPulse-dt*3);
   if(coinText)coinText.color=Color.Lerp(C("FFF0C8"),C("FFFFFF"),coinPulse*.6f);
   if(haloOpacity){haloOpacity.alpha=DecorativeReduced?0:Mathf.Max(0,1-haloAge/.36f)*.7f;float scale=1+haloAge*.45f;clickHalo.rectTransform.localScale=Vector3.one*scale;clickHalo.rectTransform.anchoredPosition=haloOrigin+new Vector2(47,-47)*(1-scale);}
   for(int i=0;i<rewardMotes.Length;i++){
    var m=rewardMotes[i];if(!m.active)continue;m.age+=dt;
    float t=Mathf.Clamp01(m.age/m.duration),ease=t*t*(3-2*t);
    m.opacity.alpha=DecorativeReduced?0:m.age<0?0:Mathf.Min(1,(1-t)*5);
    m.rect.anchoredPosition=Vector2.Lerp(m.from,m.to,ease)+new Vector2(Mathf.Sin(t*Mathf.PI)*(i%2==0?-18:18),Mathf.Sin(t*Mathf.PI)*36);
    m.rect.localScale=Vector3.one*(.75f+Mathf.Sin(t*Mathf.PI)*.25f);
    if(t>=1){m.active=false;m.rect.gameObject.SetActive(false);}rewardMotes[i]=m;
   }
   for(int i=0;i<rewardNumbers.Length;i++){
    var n=rewardNumbers[i];if(!n.active)continue;n.age+=dt;float t=Mathf.Clamp01(n.age/.9f);
    n.opacity.alpha=Mathf.Min(1,(1-t)*4);n.text.rectTransform.anchoredPosition=n.from+new Vector2(0,DecorativeReduced?0:34*(1-(1-t)*(1-t)));
    if(t>=1){n.active=false;n.text.gameObject.SetActive(false);}rewardNumbers[i]=n;
   }
  }
 }
 // One controller per scene, rather than one Update per grass blade.
 public sealed class GardenAmbientMotion:MonoBehaviour {
  struct Detail {public RectTransform rect;public Vector2 origin;public Quaternion rotation;public float phase,angle,drift;}
  readonly List<Detail> details=new List<Detail>(32);CapyIdleGame game;float age,interval;
  public void Collect(){
   game=GetComponentInParent<CapyIdleGame>();details.Clear();
   foreach(var r in GetComponentsInChildren<RectTransform>(true)){
    string n=r.name;float angle=0,drift=0;
    if(n=="Garden leaf"||n=="Meadow flower"||n=="Grass tuft")angle=1.6f;
    else if(n=="Cloud cluster")drift=2.3f;
    else if(n=="Far shrubs"||n=="Distant canopy")angle=.35f;
    else continue;
    if(r.GetComponentInParent<CapyCosmeticMotion>())continue;
    details.Add(new Detail{rect=r,origin=r.anchoredPosition,rotation=r.localRotation,phase=details.Count*2.399f,angle=angle,drift=drift});
   }
  }
  void Update(){
   if(!game||game.PresentationPaused)return;
   if(game.CosmeticReduced){OnDisable();return;}
   if(!game.CosmeticMotionAllowed)return;
   float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);age+=dt;interval+=dt;
   if(interval<(game.DecorativeReduced?1f/15:1f/30))return;interval=0;
   foreach(var d in details){if(!d.rect)continue;float wave=Mathf.Sin(age*(.42f+(d.phase%3)*.09f)+d.phase),power=game.DecorativeReduced?.3f:1;
    d.rect.localRotation=d.rotation*Quaternion.Euler(0,0,wave*d.angle*power);d.rect.anchoredPosition=d.origin+new Vector2(wave*d.drift*power,0);
   }
  }
  void OnDisable(){foreach(var d in details)if(d.rect){d.rect.anchoredPosition=d.origin;d.rect.localRotation=d.rotation;}}
 }
 // Opt-in contour lighting for leaves only. Default capybara contour meshes are untouched.
 public sealed class GardenContourLight:BaseMeshEffect {
  public override void ModifyMesh(VertexHelper vh){
   if(!IsActive())return;var r=((RectTransform)transform).rect;UIVertex v=default;
   for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);Color c=v.color;
    float y=Mathf.InverseLerp(r.yMin,r.yMax,v.position.y);Color tint=Color.Lerp(new Color(.73f,.82f,.59f),new Color(1.15f,1.13f,.95f),y);c.r*=tint.r;c.g*=tint.g;c.b*=tint.b;v.color=c;vh.SetUIVertex(v,i);
   }
  }
 }
}
