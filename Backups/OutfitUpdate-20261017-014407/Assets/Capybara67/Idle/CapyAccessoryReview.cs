#if UNITY_EDITOR
using UnityEngine;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  // Editor-only contact sheet. It never changes growth, ownership, equipment or saves.
  void ShowAccessoryReview(string id){
   var old=transform.Find("Idle Canvas/Idle board/Accessory fitting review");if(old)DestroyImmediate(old.gameObject);
   var review=Rect(panel.parent,"Accessory fitting review",0,0,480,850);
   Shape(review,"Paper",0,0,480,850,"FFF3D9",0);
   var item=IdleCatalog.Find(id);Label(review,"Review heading",item.name,12,12,456,45,30,Ink,TextAnchor.MiddleCenter);
   for(int age=1;age<=IdleModel.StageCount;age++){
    float x=age<=3?8+(age-1)*158:87+(age-4)*158,y=age<=3?120:470;
    var drawing=Rect(review,"Review age "+age,x,y,240,250);DrawAvatar(drawing,id,false,age,true);drawing.localScale=Vector3.one*.60f;
    Label(review,"Age label",age+" · "+IdleModel.Stages[age-1],x,y+158,150,63,19,Ink,TextAnchor.MiddleCenter);
   }
  }
  void ShowOutfitReview(string head,string clothes){
   CloseAccessoryReview();
   var review=Rect(panel.parent,"Accessory fitting review",0,0,480,850);
   Shape(review,"Paper",0,0,480,850,"FFF3D9",0);
   string title=(string.IsNullOrEmpty(head)?"":IdleCatalog.Find(head).name+" + ")+IdleCatalog.Find(clothes).name;
   Label(review,"Review heading",title,12,12,456,55,25,Ink,TextAnchor.MiddleCenter);
   var selected=new[]{head,clothes,"moon","","","classic"};
   for(int age=1;age<=IdleModel.StageCount;age++){
    float x=age<=3?8+(age-1)*158:87+(age-4)*158,y=age<=3?120:470;
    var drawing=Rect(review,"Review age "+age,x,y,240,250);TryDrawBakedOutfit(drawing,selected,age,-1);drawing.localScale=Vector3.one*.60f;
    Label(review,"Age label",age+" · "+IdleModel.Stages[age-1],x,y+158,150,63,19,Ink,TextAnchor.MiddleCenter);
   }
  }
  void CloseAccessoryReview(){var old=panel.parent.Find("Accessory fitting review");if(old)DestroyImmediate(old.gameObject);}
 }
}
#endif

