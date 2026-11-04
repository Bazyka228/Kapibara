using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  CapyCareGesture foodGesture,bathGesture;
  RectTransform foodDragLayer,foodGhost,foodDropTarget;
  SoftShape foodDropGlow;
  Text foodDragHint;
  Vector2 foamTarget,foamOffset;
  RectTransform bathFoam;
  bool FoodDragging=>foodDragLayer!=null;
  bool BathInteractive=>InputAllowed&&careReaction==1&&careReactionAge<5&&evolutionAge>=2.4f&&mealAge>=1.8f;

  void BindFoodGesture(Button button,int food){
   var gesture=button.gameObject.AddComponent<CapyCareGesture>();
   gesture.begin=e=>{
    if(!loaded||Model==null||PresentationPaused||Page!="food"||FoodDragging||!button.IsInteractable())return false;
    foodGesture=gesture;
    if(pageTween.IsActive())pageTween.Kill();pageOpacity.alpha=0;pageOpacity.blocksRaycasts=false;
    home.gameObject.SetActive(true);RebuildHero();RefreshHome();
    foodDropTarget=(RectTransform)home.Find("Care on character");
    foodDragLayer=Rect(board,"Food drag overlay",0,0,480,850);
    var shield=Shape(foodDragLayer,"Drag input shield",0,0,480,850,"FFFFFF00",0);shield.raycastTarget=true;shield.canvasRenderer.cullTransparentMesh=false;
    foodDropGlow=Shape(foodDragLayer,"Feed target",73,295,334,231,"AED89514",46);
    PaperCard(foodDragLayer,"Drag instruction paper",24,610,432,110);
    foodDragHint=Label(foodDragLayer,"Drag instruction","",35,620,410,86,24,"513821",TextAnchor.MiddleCenter);
    foodGhost=Rect(foodDragLayer,"Dragged meal",0,0,86,80);DrawFoodIcon(foodGhost,food<0?0:food,0,0);
    Canvas.ForceUpdateCanvases();MoveFoodGesture(e);return true;
   };
   gesture.move=MoveFoodGesture;
   gesture.finish=e=>{
    bool accept=FoodDragging&&!PresentationPaused&&Page=="food"&&RectTransformUtility.RectangleContainsScreenPoint(foodDropTarget,e.position,e.pressEventCamera);
    EndFoodGesture();if(accept)Feed(food);
   };
   gesture.cancelled=EndFoodGesture;
  }
  void MoveFoodGesture(PointerEventData e){
   if(!FoodDragging||PresentationPaused)return;
   if(RectTransformUtility.ScreenPointToLocalPointInRectangle(foodDragLayer,e.position,e.pressEventCamera,out var point))
    foodGhost.anchoredPosition=new Vector2(Mathf.Clamp(point.x-35,0,394),Mathf.Clamp(point.y+35,-760,-12));
   bool over=RectTransformUtility.RectangleContainsScreenPoint(foodDropTarget,e.position,e.pressEventCamera);
   foodDropGlow.color=C(over?"B4DE8C24":"AED89514");
   foodDragHint.text=over?"Отпусти — капибара поест":"Принеси миску к капибаре\nВ стороне — отмена";
  }
  void EndFoodGesture(){
   if(foodDragLayer){foodDragLayer.gameObject.SetActive(false);Destroy(foodDragLayer.gameObject);}
   foodDragLayer=null;foodGesture=null;
   if(pageOpacity){pageOpacity.alpha=1;pageOpacity.blocksRaycasts=true;}
   if(home&&Page=="food")home.gameObject.SetActive(false);
  }
  void BindBathGesture(Button hit){
   bathGesture=hit.gameObject.AddComponent<CapyCareGesture>();
   bathGesture.begin=e=>{heroHold.Release();if(!BathInteractive)return false;MoveBathGesture(e);return true;};
   bathGesture.move=MoveBathGesture;
   bathGesture.finish=e=>{foamTarget=Vector2.zero;};
   bathGesture.cancelled=()=>{foamTarget=Vector2.zero;};
  }
  void MoveBathGesture(PointerEventData e){
   if(!BathInteractive){bathGesture.Cancel();return;}
   if(RectTransformUtility.ScreenPointToLocalPointInRectangle(hero,e.position,e.pressEventCamera,out var point))
    foamTarget=new Vector2(Mathf.Clamp(point.x-125,-78,78),Mathf.Clamp(point.y+205,-14,52));
  }
  void CancelCareGestures(){
   if(foodGesture)foodGesture.Cancel();if(bathGesture)bathGesture.Cancel();
   if(FoodDragging)EndFoodGesture();foamTarget=Vector2.zero;
  }
 }
}
