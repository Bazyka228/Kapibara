using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  readonly RectTransform[] activityLeaves=new RectTransform[8],activitySparks=new RectTransform[7],basketHarvest=new RectTransform[8];
  readonly float[] activityFoundAt=new float[6];
  RectTransform activityButterfly,activityGuide,activityCatchGlow;
  float activityCelebrate=-100;
  Vector2 activityBurst;
  string activityReturn="care";
  void OpenActivities(string from){activityReturn=from;Show("activities");}
  static void ActivityMandarin(Transform p,float x,float y,float size){
   var r=Rect(p,"Painted mandarin",x,y,64,64);r.localScale=Vector3.one*(size/64);
   Oval(r,"Peel contour",0,3,64,57,"D97B32",2.6f);
   Shape(r,"Golden flesh",4,5,56,47,"F3A144",0,true);
   Shape(r,"Sunny cheek",8,8,40,29,"F9B95B",0,true);
   Stroke(r,"Soft highlight",16,17,26,12,"FFE1A0",3);
   Stroke(r,"Peel fold",33,8,35,16,"E99038",1.5f);
   for(int n=0;n<7;n++){float xx=29+(n*11)%24,yy=32+(n*7)%17;Shape(r,"Pore",xx,yy,2.2f,2.6f,"C97432A0",0,true);}
   Stroke(r,"Stem",33,7,35,-2,"735532",3);Leaf(r,31,-7,30,15,"6A9651",-8);Stroke(r,"Leaf midrib",35,3,54,-2,"D1D98B",1.4f);
  }
  static RectTransform ActivityBasketArt(Transform p,float x,float y,float scale){
   var r=Rect(p,"Woven picnic basket",x,y,108,80);r.localScale=Vector3.one*scale;
   Shape(r,"Basket shadow",-6,56,122,15,"65432D26",0,true);
   Oval(r,"Handle outline",15,-37,78,70,"C39351",5);Oval(r,"Handle opening",23,-29,62,55,"D5E5BB",2);
   Oval(r,"Inner rim",-3,-5,114,33,"694A30",3);Oval(r,"Linen lining",3,0,102,23,"F9E7BD",1.5f);
   var body=Rounded(0,0,240,0,226,122,197,150,40,150,14,125);
   Contour(r,"Basket silhouette",0,10,108,52,Ink,body);Contour(r,"Wicker paint",3,12,102,46,"CA9455",body);
   for(int row=0;row<5;row++)for(int col=0;col<8;col++){
    float xx=8+col*11.4f,yy=17+row*8;Shape(r,"Woven strip",xx,yy,9.7f,5,row%2==col%2?"E7B979":"A87343",2);
   }
   InkBox(r,"Braided rim",-4,6,116,12,"E8B979",5,2);Stroke(r,"Rim light",4,9,101,9,"FFDB9A",1.5f);
   Contour(r,"Linen corner edge",67,10,31,34,Ink,Rounded(0,0,240,0,220,130,118,150,9,126));
   Contour(r,"Linen corner",69,11,27,30,"FFF0C9",Rounded(0,0,240,0,220,130,118,150,9,126));
   for(int n=0;n<3;n++)Stroke(r,"Napkin embroidery",73+n*7,17,72+n*7,34,"B8C99B",1.4f);
   Oval(r,"Basket badge",40,29,26,23,"F3D19A",2);Leaf(r,46,34,15,11,"7D9C59",-16);
   return r;
  }
  static void ActivityFlower(Transform p,float x,float y,float scale,string color){
   var f=Rect(p,"Meadow flower",x,y,20,30);f.localScale=Vector3.one*scale;
   Stroke(f,"Stem",9,11,10,28,"648853",2);Leaf(f,10,17,12,8,"94B578",-22);
   for(int n=0;n<5;n++){float a=n*Mathf.PI*2/5;Shape(f,"Petal",6+Mathf.Cos(a)*5,6+Mathf.Sin(a)*5,8,8,color,0,true);}
   Oval(f,"Pollen",6,6,8,8,"E6B25D",1);
  }
  static void ActivityMushroom(Transform p,float x,float y,float scale){
   var r=Rect(p,"Little garden mushroom",x,y,34,37);r.localScale=Vector3.one*scale;
   InkBox(r,"Cream stalk",13,17,12,21,"F5DFB4",5,1.8f);
   Oval(r,"Round cap",0,0,36,24,"C98569",2);Shape(r,"Cap light",4,3,24,9,"EAB58F",0,true);
   Oval(r,"Cap fleck",7,11,5,4,"FFF0CB",0);Oval(r,"Cap fleck",23,8,6,5,"FFF0CB",0);
  }
  void BuildActivityGarden(){
   activityCelebrate=-100;for(int i=0;i<6;i++)activityFoundAt[i]=-100;
   int variant=Model.S.activity.variant;
   Shape(activityField,"Morning sky",0,0,432,444,variant==1?"DCE5D0":variant==2?"CADCDE":"C5DFD1",18);
   Shape(activityField,"Sun halo",292,9,96,96,"F6E6B879",0,true);Shape(activityField,"Morning sun",314,26,54,54,"FFE9AD",0,true);
   for(int i=0;i<3;i++){float x=26+i*143;Shape(activityField,"Cloud",x,41+i%2*23,72,17,"FFF5DC",0,true);Shape(activityField,"Cloud crown",x+15,30+i%2*23,32,25,"FFF5DC",0,true);}
   Shape(activityField,"Distant hill",-40,132,330,180,"B2CC9D",0,true);Shape(activityField,"Distant hill",192,126,290,188,"A3C294",0,true);
   Shape(activityField,"Meadow",0,233,432,211,"C7D69F",16);
   Shape(activityField,"Picnic clearing",32,299,366,133,"E2DFAD",0,true);Shape(activityField,"Clearing light",55,310,316,105,"EBE5BB",0,true);
   for(int n=0;n<6;n++){float x=32+n*69;InkBox(activityField,"Fence post",x,211,13,75,"D4AE7B",5,2);Shape(activityField,"Post sheen",x+3,217,3,55,"F1D3A1",1);}
   InkBox(activityField,"Fence upper rail",14,229,404,10,"D8B985",4,2);InkBox(activityField,"Fence lower rail",14,261,404,9,"CBA675",4,2);
   for(int n=0;n<7;n++){float x=5+n*63,y=390+(n%2)*17;Oval(activityField,"Path pebble",x,y,38,12,"C9C9A3",1);Stroke(activityField,"Pebble highlight",x+8,y+3,x+25,y+3,"F9EDC9",1.5f);}
   for(int n=0;n<8;n++){
    float x=n%2==0?4:384,y=20+(n/2)*106;var r=Rect(activityField,"Breeze foliage",x,y,46,65);activityLeaves[n]=r;
    Shape(r,"Bush shadow",-6,23,58,34,"719568",0,true);Leaf(r,0,8,30,50,"729A63",-16);Leaf(r,17,6,32,53,"94B779",14);Leaf(r,6,36,42,22,"B2CA88",-8);
   }
   for(int n=0;n<10;n++)ActivityFlower(activityField,10+(n*47)%410,312+(n%3)*43,.65f+(n%2)*.15f,n%3==0?"E9B5A7":"FFF0CB");
   if(variant==1)for(int n=0;n<5;n++)ActivityFlower(activityField,37+n*80,282+(n%2)*28,1.05f,"D8ABD0");
   if(variant==2)for(int n=0;n<4;n++)ActivityMushroom(activityField,24+n*115,290+(n%2)*23,.8f);
   if(Model.S.activity.kind<2){
    Stroke(activityField,"Festival string",21,8,411,26,"A5865C",2);
    for(int n=0;n<8;n++){float x=30+n*49;Contour(activityField,"Garden pennant",x,9+n*2,27,30,new[]{"E9B2A2","EBD593","AAD1C4","C7B6D6"}[n%4],Rounded(0,0,240,0,120,150));Stroke(activityField,"Pennant seam",x+5,12+n*2,x+21,12+n*2,"FFF1CE",1.5f);}
    for(int n=0;n<10;n++){float x=7+n*44,y=365+(n%3)*13;Stroke(activityField,"Meadow blade",x,y,x-4,y-10,"89AC71",2);Stroke(activityField,"Meadow blade",x,y,x+5,y-14,"A5BD7E",2);}
    if(Model.S.activity.kind==0){var coach=Rect(activityField,"Picnic capy friend",337,258,240,250);coach.localScale=Vector3.one*.33f;DrawAvatar(coach,null,false,2,true);ActivityFlower(activityField,343,316,.7f,"E9B5C9");}
   }
   activityButterfly=Rect(activityField,"Garden butterfly",212,88,28,25);
   Oval(activityButterfly,"Wing left",0,0,15,20,"EBC890",1.5f);Oval(activityButterfly,"Wing right",13,0,15,20,"E4AA85",1.5f);Stroke(activityButterfly,"Body",14,6,14,21,Ink,2);
   activityCatchGlow=Rect(activityField,"Catch celebration",0,0,1,1);
   for(int n=0;n<7;n++){var spark=Rect(activityCatchGlow,"Harvest sparkle",0,0,15,15);Star(spark,0,0,15,n%2==0?"FFE6A5":"F3B887");activitySparks[n]=spark;}activityCatchGlow.gameObject.SetActive(false);
  }
  void HarvestFeedback(Vector2 position){
   activityCelebrate=(float)Model.S.activity.elapsed;activityBurst=position;
   if(!Model.S.calm&&activityCatchGlow){activityCatchGlow.gameObject.SetActive(true);activityCatchGlow.SetAsLastSibling();}
  }
  void AnimateActivityGarden(){
   var a=Model.S.activity;float t=(float)a.elapsed;bool calm=Model.S.calm;
   for(int n=0;n<8;n++)if(activityLeaves[n])activityLeaves[n].localEulerAngles=new Vector3(0,0,calm?0:Mathf.Sin(t*1.3f+n)*2);
   if(activityButterfly){activityButterfly.anchoredPosition=new Vector2(212+(calm?0:Mathf.Sin(t*.7f)*36),-88+(calm?0:Mathf.Cos(t*1.8f)*9));activityButterfly.localScale=new Vector3(calm?1:.8f+.2f*Mathf.Cos(t*7),1,1);}
   float age=t-activityCelebrate;
   if(activityCatchGlow){activityCatchGlow.gameObject.SetActive(!calm&&age>=0&&age<.65f);for(int n=0;n<7;n++){float angle=n*Mathf.PI*2/7;activitySparks[n].anchoredPosition=activityBurst+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(12+age*65);activitySparks[n].localScale=Vector3.one*Mathf.Clamp01(1-age/.65f);}}
   if(a.kind==0&&activityBasket){
    activityBasket.localEulerAngles=new Vector3(0,0,calm?0:Mathf.Sin(t*2)*.7f);
    activityBasket.localScale=Vector3.one*(1+(calm?0:Mathf.Max(0,1-age/.35f)*.075f));
    for(int n=0;n<8;n++)if(basketHarvest[n])basketHarvest[n].gameObject.SetActive(n<a.score);
   }
   if(activityGuide){activityGuide.gameObject.SetActive(t<5);activityGuide.anchoredPosition=new Vector2(170+(calm?0:Mathf.Sin(t*2)*57),-730);}
  }
 }
}
