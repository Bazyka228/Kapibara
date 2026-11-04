using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Capybara67.Idle {
 // Advance only this game's tweens. Focus / SDK pause never changes global DOTween state.
 public sealed class CapyTweenDriver : MonoBehaviour {
  readonly List<Tween> active=new List<Tween>(32);
  CapyIdleGame game;
  public int ActiveCount {get {int count=0;foreach(var tween in active)if(tween.IsActive())count++;return count;}}
  void Awake(){Initialize(GetComponent<CapyIdleGame>());}
  public void Initialize(CapyIdleGame owner){game=owner;DOTween.Init(false,true,LogBehaviour.ErrorsOnly);}
  public T Track<T>(T tween,GameObject owner) where T:Tween {
   tween.SetUpdate(UpdateType.Manual).SetLink(owner,LinkBehaviour.KillOnDisable);
   active.Add(tween);return tween;
  }
  void Update(){Step(Mathf.Min(Time.unscaledDeltaTime,.1f));}
  public void Step(float dt){
   if(!game||game.Model==null||game.PresentationPaused)return;
   for(int n=active.Count-1;n>=0;n--){
    var tween=active[n];
    if(tween.IsActive()){
     if(game.CosmeticReduced)tween.Complete(true);
     else tween.ManualUpdate(Mathf.Max(0,dt),Mathf.Max(0,dt));
    }
    if(!tween.IsActive())active.RemoveAt(n);
   }
  }
  void OnDisable(){foreach(var tween in active)if(tween.IsActive())tween.Kill();active.Clear();}
 }
}
