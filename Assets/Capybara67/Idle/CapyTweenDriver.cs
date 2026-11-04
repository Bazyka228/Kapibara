using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Capybara67.Idle {
 // Advance only this game's tweens. Focus / SDK pause never changes global DOTween state.
 public sealed class CapyTweenDriver : MonoBehaviour {
  struct OwnedTween {public Tween tween;public GameObject owner;}
  readonly List<OwnedTween> active=new List<OwnedTween>(32);
  CapyIdleGame game;
  public int ActiveCount {get {int count=0;foreach(var entry in active)if(entry.tween.IsActive())count++;return count;}}
  void Awake(){Initialize(GetComponent<CapyIdleGame>());}
  public void Initialize(CapyIdleGame owner){game=owner;DOTween.Init(false,true,LogBehaviour.Default);}
  public T Track<T>(T tween,GameObject owner) where T:Tween {
   tween.SetUpdate(UpdateType.Manual).SetLink(owner,LinkBehaviour.KillOnDisable);
   active.Add(new OwnedTween{tween=tween,owner=owner});return tween;
  }
  void Update(){Step(Mathf.Min(Time.unscaledDeltaTime,.1f));}
  public void Step(float dt){
   // ManualUpdate can run before DOTween's own link cleanup, even during page teardown.
   // Remove dead owners before callbacks/setters and while the game is paused too.
   for(int n=active.Count-1;n>=0;n--){
    var entry=active[n];var tween=entry.tween;
    if(!entry.owner||!entry.owner.activeInHierarchy){if(tween.IsActive())tween.Kill();active.RemoveAt(n);continue;}
    if(!game||game.Model==null||game.PresentationPaused)continue;
    if(tween.IsActive()){
     if(game.CosmeticReduced)tween.Complete(true);
     else tween.ManualUpdate(Mathf.Max(0,dt),Mathf.Max(0,dt));
    }
    if(!tween.IsActive())active.RemoveAt(n);
   }
  }
  void OnDisable(){foreach(var entry in active)if(entry.tween.IsActive())entry.tween.Kill();active.Clear();}
 }
}
