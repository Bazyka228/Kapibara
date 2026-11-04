using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Capybara67.Idle {
 // One captured pointer; cancel never performs the release action.
 public sealed class CapyCareGesture : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler {
  public Func<PointerEventData,bool> begin;
  public Action<PointerEventData> move,finish;
  public Action cancelled;
  public bool Active {get;private set;}
  int pointer;
  public void OnBeginDrag(PointerEventData e){
   if(Active||e.button!=PointerEventData.InputButton.Left)return;
   e.eligibleForClick=false;
   if(begin==null||!begin(e))return;
   pointer=e.pointerId;Active=true;
  }
  public void OnDrag(PointerEventData e){if(Active&&e.pointerId==pointer)move?.Invoke(e);}
  public void OnEndDrag(PointerEventData e){if(!Active||e.pointerId!=pointer)return;Active=false;e.eligibleForClick=false;finish?.Invoke(e);}
  public void Cancel(){if(!Active)return;Active=false;cancelled?.Invoke();}
  void OnDisable(){Cancel();}
 }
}
