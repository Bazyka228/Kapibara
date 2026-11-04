using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  public bool CosmeticMotionAllowed=>Model!=null&&!suspended&&!platformPaused&&!Model.S.calm;
  public bool CosmeticReduced=>Model!=null&&Model.S.calm;
  public float CarePulse=>Page=="home"?Mathf.Clamp01(1-reactionAge/1.2f):0;
  static void AnimateDetail(RectTransform t,int kind,float seed,float distance=12){
   var motion=t.gameObject.AddComponent<CapyCosmeticMotion>();motion.Configure(kind,seed,distance);
  }
  static void EffectMotif(Transform p,int kind,string color){
   if(kind>=13){TreeEffectMotif(p,kind);return;}switch(kind){
    case 0:
     Shape(p,"Bubble halo",-4,-4,38,38,"BCEFE52C",0,true);
     Oval(p,"Iridescent bubble",0,0,30,30,"A7DCD8",1.5f);
     Shape(p,"Lavender reflection",5,5,20,20,"B8B8E7",0,true);Shape(p,"Rose reflection",7,10,16,14,"EBC1D8",0,true);
     Shape(p,"Clear centre",8,7,13,12,"D8F3E8",0,true);Stroke(p,"Bubble crescent",6,8,9,4,"FFFCE6",3);Shape(p,"Bubble glint",21,20,4,4,"FFFCE6",0,true);break;
    case 1:
     Shape(p,"Starlight",-6,-6,42,42,"FFE6A32C",0,true);Star(p,0,0,30,"EDBD59");Star(p,6,5,17,"FFEEC2");Shape(p,"Star glint",12,7,4,4,"FFFFFF",0,true);break;
    case 2:
     Heart(p,0,0,30,"D97B95");Heart(p,4,2,23,"F5A7BA");Stroke(p,"Heart gloss",7,7,10,5,"FFE8E5",3);Shape(p,"Heart gleam",21,11,3,3,"FFF6E3",0,true);break;
    case 3:
     InkBox(p,"Crystal pixel",1,4,25,25,"79B6B9",3,2);Shape(p,"Pixel bevel",4,7,19,5,"C9EFE2",0);Shape(p,"Pixel facet",17,12,6,14,"45878F",0);Shape(p,"Pixel light",8,14,7,7,"FFF2BD",0);Shape(p,"Orbit pixel",27,-3,6,6,"E9B777",1);break;
    case 4:
     Shape(p,"Firefly glow",-5,-5,40,40,"F9D57930",0,true);Shape(p,"Firefly inner glow",2,2,26,26,"FFE3A34A",0,true);
     Oval(p,"Firefly wing",-1,8,14,8,"B3DCD0",1);Oval(p,"Firefly wing",17,8,14,8,"B3DCD0",1);
     Oval(p,"Amber lantern",9,7,13,20,"F3BC5D",1.5f);Shape(p,"Lantern light",12,14,7,8,"FFF4B2",0,true);Shape(p,"Firefly head",12,4,7,7,Ink,0,true);break;
    case 5:case 12:
     InkBox(p,"Enamel number charm",0,0,29,34,color,9,2);Shape(p,"Charm highlight",5,4,18,3,"FFF2D1",1);Mark(p,kind==5?"6":"7",3,3,23,29,24,"65432D");break;
    case 6:
     for(int n=0;n<5;n++){float a=n*Mathf.PI*.4f;Oval(p,"Silk sakura petal",10+Mathf.Cos(a)*9,10+Mathf.Sin(a)*9,12,17,"F2B9CC",1);}
     Oval(p,"Sakura heart",11,14,9,9,"F5D68C",1);Shape(p,"Pollen dust",15,17,3,3,"FFF6DB",0,true);break;
    case 7:
     Oval(p,"Butterfly wing",0,3,15,19,"B6B3DE",1.5f);Oval(p,"Butterfly wing",16,3,15,19,"A6D5D4",1.5f);Oval(p,"Butterfly wing",4,19,11,10,"D9B5D8",1);Oval(p,"Butterfly wing",16,19,11,10,"CBE2CB",1);
     Stroke(p,"Butterfly body",15,10,15,27,Ink,3);Stroke(p,"Antenna",14,11,10,6,Ink,1);Stroke(p,"Antenna",17,11,21,6,Ink,1);Shape(p,"Wing pearl",5,9,4,6,"FFF3D7",1);Shape(p,"Wing pearl",23,9,4,6,"FFF3D7",1);break;
    case 8:
     var drop=Rounded(120,0,236,95,225,134,120,150,17,133,0,94);
     Contour(p,"Dew outline",5,0,23,32,Ink,drop);Contour(p,"Dew glass",7,3,19,27,"A9DFE4",drop);Shape(p,"Dew light",11,15,4,10,"F0FFED",2);Shape(p,"Dew reflection",19,24,4,3,"78B5C5",1);break;
    case 9:Leaf(p,1,2,29,25,"C6C981",-12);Leaf(p,16,18,16,12,"E6B576",32);break;
    case 10:
     Stroke(p,"Note stem",21,3,21,24,Ink,4);Stroke(p,"Note flag",21,4,29,10,Ink,4);Oval(p,"Musical pearl",8,20,15,10,"C8B0DF",1.5f);LifeSpark(p,0,0,11);break;
    default:
     var moon=Rounded(190,0,84,15,0,73,36,127,146,150,240,106,146,110,95,65,124,20);
     Contour(p,"Moon outline",0,0,28,32,Ink,moon);Contour(p,"Pearl moon",3,3,22,26,"F3E6AD",moon);LifeSpark(p,23,0,12);break;
   }
  }
  static void AnimatedAura(Transform root,IdleItem effect){
   if(effect==null)return;
   var layer=Rect(root,"Animated effect "+effect.id,0,0,240,250);
   for(int n=0;n<7;n++){
    float x=n%2==0?-13:225,y=37+(n/2)*52;
    var particle=Rect(layer,"Effect particle "+n,x,y,34,38);float s=.53f+(n%3)*.16f;particle.localScale=Vector3.one*s;
    EffectMotif(particle,effect.art==5&&n%2==1?12:effect.art,effect.color);AnimateDetail(particle,effect.art==7||effect.art==18?3:effect.art==17?6:0,n*.77f,10+n%3*4);
   }
  }
  static void AnimatedEmotion(Transform p,IdleItem emotion,Rect h){
   if(emotion==null)return;
   float x=Mathf.Clamp(h.x-34,4,190),y=Mathf.Max(6,h.y-10);
   var bubble=Rect(p,"Emotion vignette "+emotion.id,x,y,47,45);
   Shape(bubble,"Thought shadow",2,3,45,38,"65432D24",0,true);Oval(bubble,"Thought pearl",0,0,43,37,"FFF5D9",1.5f);
   Oval(bubble,"Thought tail",28,33,7,7,"FFF5D9",1);Shape(bubble,"Thought glint",9,5,12,3,"FFFFFF",1);
   if(emotion.art>=12)TreeEmotionMotif(bubble,emotion.art);else switch(emotion.art){
    case 0:
     Stroke(bubble,"Happy eye",9,17,13,13,Ink,2);Stroke(bubble,"Happy eye",13,13,17,17,Ink,2);Stroke(bubble,"Happy eye",26,17,30,13,Ink,2);Stroke(bubble,"Happy eye",30,13,34,17,Ink,2);
     Oval(bubble,"Laugh mouth",15,21,14,10,"9D5B4B",1);Shape(bubble,"Tongue",19,26,7,3,"F1A09D",1);break;
    case 1:
     Star(bubble,6,10,14,"E6B455");Star(bubble,25,10,14,"E6B455");Oval(bubble,"Amazed mouth",18,24,7,8,"A77B5C",1);break;
    case 2:
     Stroke(bubble,"Rested eyelid",9,17,17,18,Ink,2);Stroke(bubble,"Rested eyelid",26,18,33,17,Ink,2);Oval(bubble,"Yawn",17,23,10,8,"A57D65",1);break;
    case 3:
     Heart(bubble,6,10,15,"E99FA6");Heart(bubble,24,10,15,"E99FA6");Stroke(bubble,"Joy smile",16,27,28,28,Ink,2);break;
    case 4:
     Stroke(bubble,"Raised brow",8,10,17,8,Ink,2);Shape(bubble,"Side eye",12,17,4,5,Ink,0,true);Stroke(bubble,"Knowing eye",26,16,34,17,Ink,2);Stroke(bubble,"Wry grin",18,27,28,24,Ink,2);break;
    case 5:
     Stroke(bubble,"Content eye",9,17,16,18,Ink,2);Stroke(bubble,"Content eye",26,18,33,17,Ink,2);Shape(bubble,"Warm blush",6,23,8,4,"EBA69D",0,true);Shape(bubble,"Warm blush",30,23,8,4,"EBA69D",0,true);Stroke(bubble,"Gentle smile",18,25,25,27,Ink,2);break;
    case 6:
     Heart(bubble,13,10,22,"E9A0B4");Stroke(bubble,"Hugging arm",7,27,17,29,"BC8567",3);Stroke(bubble,"Hugging arm",35,27,26,29,"BC8567",3);break;
    case 7:
     Stroke(bubble,"Wink",8,18,17,16,Ink,2);Shape(bubble,"Bright eye",29,14,5,7,Ink,0,true);Stroke(bubble,"Wink smile",18,25,28,23,Ink,2);break;
    case 8:
     Shape(bubble,"Curious eye",11,16,5,7,Ink,0,true);Shape(bubble,"Curious eye",28,14,5,7,Ink,0,true);Stroke(bubble,"Curious brow",27,10,34,8,Ink,2);Oval(bubble,"Question mouth",20,26,5,5,"D3A281",1);break;
    case 9:
     Star(bubble,12,3,20,"F0C775");Stroke(bubble,"Proud eye",8,22,16,20,Ink,2);Stroke(bubble,"Proud eye",28,20,36,22,Ink,2);Stroke(bubble,"Proud smile",18,28,26,28,Ink,2);break;
    case 10:
     Stroke(bubble,"Zen eye",9,17,17,18,Ink,2);Stroke(bubble,"Zen eye",27,18,34,17,Ink,2);Leaf(bubble,16,24,16,9,"97B8A0");break;
    default:
     Oval(bubble,"Idea glass",14,7,16,18,"FFE49C",1.5f);InkBox(bubble,"Idea base",17,25,10,5,"B5C6BB",2,1);Stroke(bubble,"Idea filament",18,16,24,20,"C79A53",1.5f);break;
   }
   AnimateDetail(bubble,1,emotion.art*.8f,emotion.art==2?4:6);
   for(int n=0;n<2;n++){
    var accent=Rect(p,"Emotion accent",x-9+n*42,y+8+n*31,15,20);
    if(emotion.art==2)Mark(accent,n==0?"z":"Z",0,0,20,23,16,"7C88A4");
    else if(emotion.art==3||emotion.art==6)Heart(accent,0,0,14,"EBA09E");
    else if(emotion.art==4||emotion.art==8)Mark(accent,"?",0,0,18,24,18,"8E7961");
    else LifeSpark(accent,0,0,14);
    AnimateDetail(accent,2,n+emotion.art,5);
   }
  }
 }
 // One cached transform per particle: no spawning, physics or per-frame allocations.
 public sealed class CapyCosmeticMotion:MonoBehaviour {
  RectTransform rect;CanvasGroup opacity;CapyIdleGame game;Vector2 origin;Vector3 scale;int kind;float seed,distance,age;
  public float Age=>age;
  public void Configure(int mode,float phase,float travel){
   rect=(RectTransform)transform;origin=rect.anchoredPosition;scale=rect.localScale;kind=mode;seed=phase;distance=travel;
   opacity=gameObject.AddComponent<CanvasGroup>();opacity.blocksRaycasts=false;opacity.interactable=false;game=GetComponentInParent<CapyIdleGame>();Apply(true);
  }
  void Update(){if(!game||game.Model==null)return;if(game.CosmeticReduced){Apply(true);return;}if(!game.CosmeticMotionAllowed)return;age+=Mathf.Min(Time.unscaledDeltaTime,.1f);Apply(false);}
  void Apply(bool still){
   float t=still?0:age,phase=t*(kind==1?1.6f:.85f)+seed,pulse=still?0:game?game.CarePulse:0;
   if(kind==4){rect.localRotation=Quaternion.Euler(0,0,still?0:Mathf.Sin(phase)*distance);rect.anchoredPosition=origin;rect.localScale=scale;opacity.alpha=1;return;}
   if(kind==5){rect.anchoredPosition=origin+new Vector2(0,still?0:Mathf.Sin(phase*2)*distance);opacity.alpha=still?1:.7f+.25f*Mathf.Sin(phase);return;}
   float dx=still?0:Mathf.Sin(phase*.8f)*distance*.45f,dy=still?0:Mathf.Sin(phase)*distance;
   if(kind==6&&!still){dx=Mathf.Cos(phase)*distance;dy=Mathf.Sin(phase)*distance*.55f;}rect.anchoredPosition=origin+new Vector2(dx,dy+pulse*8);rect.localRotation=Quaternion.Euler(0,0,still?0:Mathf.Sin(phase*.7f)*(kind==1?4:11));
   rect.localScale=scale*(1+(still?0:Mathf.Sin(phase)*.055f)+pulse*.12f);if(kind==3&&!still)rect.localScale=Vector3.Scale(rect.localScale,new Vector3(.78f+.22f*Mathf.Cos(t*6+seed),1,1));
   opacity.alpha=still?1:kind==1?.87f+.13f*Mathf.Sin(phase):.66f+.28f*Mathf.Sin(phase);
  }
 }
}
