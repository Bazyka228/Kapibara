using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        const string Ink="65432D", Paper="FFF1CE", Wood="935F3E";
        static string UiColor(string c){switch(c){
            case "211B45":return "F4E5BE";case "19152F":return "214E42";
            case "39315B":return "E7C895";case "302751":return "FFF5D9";
            case "665781":return "F4CE74";case "4B3C67":return "E6B889";
            case "55E6C1":return "418355";case "FFF4D6":return "513821";
            case "C9BCD8":return "72543A";case "AEA2C6":return "7D644A";default:return c;}}
        static SoftShape InkBox(Transform p,string n,float x,float y,float w,float h,string c,float r=14,float line=3){
            var edge=Shape(p,n,x,y,w,h,Ink,r);Shape(edge.transform,"Paint",line,line,w-line*2,h-line*2,c,Mathf.Max(1,r-line));return edge;}
        static SoftShape Oval(Transform p,string n,float x,float y,float w,float h,string c,float line=3){
            var edge=Shape(p,n,x,y,w,h,Ink,0,true);Shape(edge.transform,"Paint",line,line,w-line*2,h-line*2,c,0,true);return edge;}
        static void Stroke(Transform p,string n,float x,float y,float endX,float endY,string c,float width=3){
            var d=new Vector2(endX-x,endY-y);var s=Shape(p,n,x,y,d.magnitude,width,c,width/2);s.transform.localEulerAngles=new Vector3(0,0,-Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);}
        static void Leaf(Transform p,float x,float y,float w,float h,string color,float angle=0){
            var root=Rect(p,"Garden leaf",x,y,w,h);root.localEulerAngles=new Vector3(0,0,angle);
            var points=Rounded(0,146,5,55,80,0,240,0,235,95,160,150);
            Contour(root,"Leaf edge",0,0,w,h,"365D35",points);Contour(root,"Leaf paint",3,3,w-6,h-6,color,points);
            Stroke(root,"Leaf vein",7,h-7,w-9,8,"D0E89B",2);
        }
        static void Star(Transform p,float x,float y,float size,string color){
            var points=new Vector2[10];for(int j=0;j<10;j++){float a=(-90+j*36)*Mathf.Deg2Rad,r=j%2==0?1:.45f;points[j]=new Vector2(120+115*Mathf.Cos(a)*r,75+72*Mathf.Sin(a)*r);}
            Contour(p,"Star outline",x,y,size,size,Ink,points);Contour(p,"Star",x+3,y+3,size-6,size-6,color,points);
        }
        static void Heart(Transform p,float x,float y,float size,string color){
            var points=Rounded(120,145,5,64,0,30,35,0,77,0,120,34,163,0,205,0,240,30,235,64);
            Contour(p,"Heart outline",x,y,size,size,Ink,points);Contour(p,"Heart",x+3,y+3,size-6,size-6,color,points);
        }
        static void Coin(Transform p,float x,float y,float size){
            Oval(p,"Coin",x,y,size,size,"EAA633");Oval(p,"Coin inset",x+5,y+5,size-10,size-10,"FFD877",2);
            Stroke(p,"Coin glint",x+size*.32f,y+size*.25f,x+size*.28f,y+size*.6f,"FFF2B7",3);
            Shape(p,"Coin stamp",x+size*.5f,y+size*.27f,4,size*.43f,"C7842E",2);
        }
        static void Gift(Transform p,float x,float y,float size){
            InkBox(p,"Gift box",x+3,y+size*.36f,size-6,size*.6f,"EB967B",7,2);
            InkBox(p,"Gift lid",x,y+size*.28f,size,size*.23f,"F7B092",5,2);
            Shape(p,"Gift ribbon",x+size*.44f,y+size*.3f,size*.14f,size*.66f,"FFE3A0",2);
            Oval(p,"Bow left",x+size*.14f,y+2,size*.37f,size*.3f,"FFDB7B",2);Oval(p,"Bow right",x+size*.49f,y+2,size*.37f,size*.3f,"FFDB7B",2);
        }
        static void Gear(Transform p,float x,float y){
            for(int n=0;n<8;n++){float a=n*Mathf.PI/4;var tooth=Shape(p,"Gear tooth",x+19+Mathf.Cos(a)*15,y+19+Mathf.Sin(a)*15,8,9,"68492E",2);tooth.rectTransform.pivot=new Vector2(.5f,.5f);tooth.transform.localEulerAngles=new Vector3(0,0,-n*45);}
            Oval(p,"Settings gear",x+6,y+6,27,27,"C99D65",3);Oval(p,"Gear centre",x+14,y+14,11,11,"F7D99A",2);
        }
        static void WoodPanel(Transform p,string n,float x,float y,float w,float h){
            var edge=InkBox(p,n,x,y,w,h,Wood,20,4);Shape(edge.transform,"Top bevel",7,7,w-14,7,"E0A064",3);
            Shape(edge.transform,"Bottom bevel",8,h-12,w-16,6,"945A35",3);
            for(int k=0;k<3;k++){float yy=20+k*(h-30)/3;Shape(edge.transform,"Wood grain",12,yy,20+k*11,2,"A3673B",1);Shape(edge.transform,"Wood grain",w-52,yy+6,30,2,"A3673B",1);}
            for(int k=0;k<2;k++){Shape(edge.transform,"Wood peg",10+k*(w-26),h/2,6,6,"71492E",0,true);}
        }
        static void PaperCard(Transform p,string n,float x,float y,float w,float h){
            InkBox(p,n,x,y,w,h,Paper,19,3);Shape(p,"Paper top edge",x+15,y+7,w-30,3,"FFF9E5",1);
        }
        void PageFrame(string title){
            Tile(panel,"Panel background",0,0,480,850,"214E42",0).raycastTarget=true;
            WoodPanel(panel,"Page frame",7,7,466,836);PaperCard(panel,"Page parchment",17,88,446,749);
            WoodPanel(panel,"Title board",18,14,444,71);
            Leaf(panel,0,1,40,53,"77A653",12);Leaf(panel,448,3,39,51,"96BC61",-12);
            Label(panel,"Title",title,35,23,348,58,26,"FFF0C8").font=bold;
            Button(panel,"Back","×",392,20,64,64,()=>Show("home"),"E79279",30);
        }
        void HomeFurniture(){
            WoodPanel(decor,"Resource board",12,10,456,102);Coin(decor,29,26,44);
            PaperCard(decor,"Growth card",16,119,448,109);
            PaperCard(decor,"Care speech",24,535,432,58);
            WoodPanel(decor,"Garden navigation",12,594,456,249);
            Leaf(decor,0,580,36,56,"8DB95B",13);Leaf(decor,455,577,34,57,"8DB95B",-14);
        }
        void ScenePreview(Transform parent,IdleItem selectedItem){
            var clip=Rect(parent,"Dressing garden",34,106,181,166);clip.gameObject.AddComponent<RectMask2D>();
            var landscape=Rect(clip,"Miniature scene",-24,-180,480,850);landscape.localScale=Vector3.one*.48f;
            DrawScene(landscape,selectedItem.slot==2?selectedItem:IdleCatalog.Find(Model.S.equipped[2]));
        }
        static void FormPortrait(Transform p,int art,string color){
            var t=Rect(p,"Form portrait",10,12,45,43);Oval(t,"Ear",3,0,10,13,color,2);Oval(t,"Ear",29,0,10,13,color,2);
            Oval(t,"Face",0,8,43,31,color,2);Shape(t,"Muzzle",26,19,15,17,"A67A5C",0,true);Shape(t,"Eye",15,19,3,4,Ink,0,true);Shape(t,"Nose",33,25,3,3,Ink,0,true);Shape(t,"Blush",7,27,8,5,"E69C85",0,true);
            if(art==0){InkBox(t,"Six",10,5,11,8,"A5CC99",2,1);InkBox(t,"Seven",23,5,11,8,"F2BF68",2,1);}
            if(art==1){Oval(t,"Orange",17,-4,15,14,"F2B34F",1);Leaf(t,26,-7,11,6,"75A653");}
            if(art==2)Shape(t,"Big smile",28,30,8,4,"F6B2A1",0,true);
            if(art==3)for(int n=0;n<3;n++)Shape(t,"Pixel",2+n*5,12,4,4,"F7DA83",0);
            if(art==4)Star(t,2,-4,18,"FFDB78");
            if(art==5){InkBox(t,"Headphone",0,15,7,15,"A6D6C2",3,1);InkBox(t,"Headphone",39,15,7,15,"E7A8A6",3,1);}
            if(art==6)Shape(t,"Ninja band",2,14,39,4,"D68A61",1);
            if(art==7){InkBox(t,"Spectacles",10,17,12,10,"B3D4B4",3,1);InkBox(t,"Spectacles",27,16,10,9,"B3D4B4",3,1);Stroke(t,"Bridge",21,20,28,19,Ink,1);}
        }
        void ItemPicture(Transform parent,IdleItem item){
            var badge=Oval(parent,"Item medallion",9,12,59,59,"FFF3CC",2);
            var drawing=Rect(badge.transform,"Item illustration",0,0,240,250);
            if(item.IsSet){drawing.localScale=Vector3.one*.25f;drawing.anchoredPosition=new Vector2(-1,-1);DrawAvatar(drawing,item.id);}
            else if(item.slot<2){AccessoryImage(badge.transform,item,new Rect(5,5,49,49),true);}

            else if(item.slot==5){drawing.localScale=Vector3.one*.22f;drawing.anchoredPosition=new Vector2(3,0);DrawAvatar(drawing,item.id);}
            else if(item.slot==2){var clip=Rect(badge.transform,"Scene thumbnail",5,9,49,41);clip.gameObject.AddComponent<RectMask2D>();var view=Rect(clip,"Scene thumbnail scale",-7,-46,480,850);view.localScale=Vector3.one*.13f;DrawScene(view,item);}
            else if(item.slot==3){var motif=Rect(badge.transform,"Effect sample",14,13,34,38);EffectMotif(motif,item.art,item.color);}
            else {var sample=Rect(badge.transform,"Emotion sample",6,7,47,45);AnimatedEmotion(sample,item,new Rect(34,10,0,0));}
        }
    }
    // Tint the entire illustrated button, including its non-interactive artwork.
    public sealed class GardenButtonState : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
        Button button;CanvasGroup group;CapyIdleGame game;bool pressed;
        RectTransform rect;Vector2 origin;Tween pressTween;float visualScale=1;
        void Awake(){button=GetComponent<Button>();group=gameObject.AddComponent<CanvasGroup>();game=GetComponentInParent<CapyIdleGame>();rect=(RectTransform)transform;origin=rect.anchoredPosition;}
        void LateUpdate(){if(!button||!group)return;group.alpha=button.interactable?1:.86f;if(pressed&&(!button.interactable||!game||game.PresentationPaused))Release();}
        void ApplyScale(float value){visualScale=value;rect.localScale=Vector3.one*value;rect.anchoredPosition=origin+new Vector2(rect.rect.width,-rect.rect.height)*((1-value)*.5f);}
        void Animate(bool down){
            if(pressTween.IsActive())pressTween.Kill();
            if(!game||game.Model==null||game.CosmeticReduced||!game.Tweens){ApplyScale(1);return;}
            pressTween=game.Tweens.Track(DOTween.To(()=>visualScale,ApplyScale,down?.97f:1,down?.09f:.16f).SetEase(Ease.OutCubic),gameObject);
        }
        void Release(){if(!pressed)return;pressed=false;Animate(false);}
        public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||!button.interactable||!game||game.PresentationPaused)return;pressed=true;Animate(true);}
        public void OnPointerUp(PointerEventData e){Release();}
        public void OnPointerExit(PointerEventData e){Release();}
        void OnDisable(){pressed=false;if(pressTween.IsActive())pressTween.Kill();if(rect)ApplyScale(1);}
    }
}

