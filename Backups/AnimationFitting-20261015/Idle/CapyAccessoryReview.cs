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
   for(int age=1;age<=9;age++){
    float x=8+(age-1)%3*158,y=80+(age-1)/3*246;
    var drawing=Rect(review,"Review age "+age,x,y,240,250);DrawAvatar(drawing,id,false,age,true);drawing.localScale=Vector3.one*.60f;
    Label(review,"Age label",age+" · "+IdleModel.Stages[age-1],x,y+158,150,63,19,Ink,TextAnchor.MiddleCenter);
   }
  }
  void CloseAccessoryReview(){var old=panel.parent.Find("Accessory fitting review");if(old)DestroyImmediate(old.gameObject);}
 }
}
#endif
