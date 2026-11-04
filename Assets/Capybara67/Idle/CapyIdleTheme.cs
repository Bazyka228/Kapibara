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
            if(protectedArtDepth==0&&w>=28&&h>=22){
                Stroke(root,"Short leaf vein",w*.43f,h*.56f,w*.28f,h*.31f,"91B967",1);
                Stroke(root,"Short leaf vein",w*.58f,h*.42f,w*.77f,h*.45f,"91B967",1);
            }
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
            var t=Rect(p,"Illustrated garden gift",x,y,120,120);t.localScale=Vector3.one*(size/120);
            Shape(t,"Parcel shadow",7,109,108,10,"65432D26",0,true);
            var front=Rounded(0,0,222,0,240,18,240,140,220,150,18,150,0,133);
            Contour(t,"Parcel outline",9,43,99,68,Ink,front);Contour(t,"Sage wrapping",12,46,93,62,"8CAE82",front);
            Shape(t,"Right fold",92,48,12,57,"6C946E",4);Stroke(t,"Box base piping",19,104,97,104,"C5D6A0",2);
            for(int n=0;n<3;n++){float xx=22+n*28;Stroke(t,"Paper stitch",xx,94,xx+8,94,"C6D8AC",1.5f);}
            InkBox(t,"Parcel lid rim",4,36,111,24,"C1D398",7,2.5f);Shape(t,"Lid shadow",10,59,97,4,"547B5A70",1);
            Contour(t,"Lid upper face",7,29,105,15,"DCE6B5",Rounded(18,0,219,0,240,150,0,150));
            Shape(t,"Ribbon side shadow",50,57,20,51,"B99356",1);Shape(t,"Woven gold ribbon",48,34,18,74,"E8BE74",2);
            Stroke(t,"Ribbon stitched left",51,39,51,104,"FFF0BB",1.4f);Stroke(t,"Ribbon stitched right",63,39,63,104,"C89750",1.3f);
            var tail=Rounded(5,0,226,0,240,150,122,116,20,148);
            Contour(t,"Left ribbon tail edge",34,24,19,38,Ink,tail);Contour(t,"Left ribbon tail",36,26,15,32,"E8B96B",tail);
            Contour(t,"Right ribbon tail edge",65,23,20,36,Ink,tail);Contour(t,"Right ribbon tail",67,25,16,30,"F2CF87",tail);
            var bow=Rounded(235,130,168,17,65,0,8,26,0,89,54,138,143,150);
            Contour(t,"Left bow outline",17,5,44,32,Ink,bow);Contour(t,"Left silk bow",20,8,38,26,"F0C87F",bow);
            Stroke(t,"Left bow fold",29,15,53,28,"FFF0BC",3);Stroke(t,"Left bow seam",27,24,48,30,"BC8B4F",1.5f);
            var right=Rounded(4,130,73,17,176,0,230,26,240,89,186,138,97,150);
            Contour(t,"Right bow outline",58,4,44,32,Ink,right);Contour(t,"Right silk bow",61,7,38,26,"F4D696",right);
            Stroke(t,"Right bow fold",70,26,91,13,"FFF2C6",3);Stroke(t,"Right bow seam",72,30,92,25,"D2A25C",1.5f);
            InkBox(t,"Bow knot",49,23,21,17,"E3B66A",6,2);Shape(t,"Knot light",53,26,12,3,"FFE7AB",1);
            Stroke(t,"Tag cord",72,48,88,67,"886747",1.5f);InkBox(t,"Gift tag",78,65,22,29,"FFF0CC",5,1.5f);
            Shape(t,"Tag eyelet",87,68,3,3,Ink,0,true);Leaf(t,83,75,12,11,"8FAB6F",-10);
            Oval(t,"Wax seal",48,76,19,19,"C98C62",1.5f);Oval(t,"Seal inset",52,80,11,11,"E7AC72",1);Stroke(t,"Seal leaf",55,87,62,83,"895A3C",1.5f);
            Leaf(t,13,58,19,12,"B6CB88",-18);Leaf(t,21,66,15,10,"D3DFA7",24);
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
            var heading=Label(panel,"Title",title,35,23,348,58,26,"FFF0C8");heading.font=bold;
            Button(panel,"Back",ParentPage=="home"?"×":"‹",392,20,64,64,GoBack,"E79279",30);AddSectionHelpButton(heading);
        }
        void HomeFurniture(){
            WoodPanel(decor,"Resource board",12,10,456,102);Coin(decor,29,26,44);
            Shape(decor,"Coin pocket",80,23,298,42,"805235",12);
            Shape(decor,"Income pocket",260,27,114,34,"996846",10);
            Shape(decor,"Mandarin pocket",84,70,294,38,"F3D7A4",10);
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
            protectedArtDepth++;try{NativeFormPortrait(p,art,color);}finally{protectedArtDepth--;}
        }
        static void NativeFormPortrait(Transform p,int art,string color){
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
            var clip=Shape(badge.transform,"Thumbnail round clip",2,2,55,55,"FFF3CC",0,true);
            clip.gameObject.AddComponent<Mask>().showMaskGraphic=true;
            if(item.IsSet||item.slot==5){
                var drawing=Rect(clip.transform,"Item illustration",0,0,240,250);
                // A catalogue thumbnail shows this item at the current age, without the
                // player's equipped aura, emotion or meme decoration.
                DrawAvatar(drawing,item.id,true,Model.Stage);
                var shadow=drawing.Find("Ground shadow");if(shadow)shadow.gameObject.SetActive(false);
                var sprite=drawing.Find("Illustrated pose/Life sprite") as RectTransform;
                // Authored frames differ by outfit and age. Fit the actual sprite frame,
                // including its hat and feet, inside the circle with a two-unit inset.
                var bounds=sprite?RectTransformUtility.CalculateRelativeRectTransformBounds(drawing,sprite):new Bounds(new Vector3(120,-125,0),new Vector3(240,250,0));
                float scale=51/Mathf.Max(1,new Vector2(bounds.size.x,bounds.size.y).magnitude);
                drawing.localScale=Vector3.one*scale;
                drawing.anchoredPosition=new Vector2(27.5f,-27.5f)-new Vector2(bounds.center.x,bounds.center.y)*scale;
            }
            else if(item.slot<2){AccessoryImage(clip.transform,item,new Rect(9,9,37,37),true);}
            else if(item.slot==2){
                // DrawScene's visible stage is y=375..684, not the full 850-unit board.
                // Cover the circular aperture with that stage instead of letterboxing it.
                float scale=55f/309f;
                var view=Rect(clip.transform,"Scene thumbnail scale",(55-480*scale)*.5f,-375*scale,480,850);
                view.localScale=Vector3.one*scale;DrawScene(view,item);
            }
            else if(item.slot==3){var motif=Rect(clip.transform,"Effect sample",12,11,34,38);EffectMotif(motif,item.art,item.color);}
            else {var sample=Rect(clip.transform,"Emotion sample",4,5,47,45);AnimatedEmotion(sample,item,new Rect(34,10,0,0));}
        }
    }
    // Tint the entire illustrated button, including its non-interactive artwork.
    public sealed class GardenButtonState : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IPointerEnterHandler {
        Button button;CanvasGroup group;CapyIdleGame game;bool pressed,hovered;
        RectTransform rect;Vector2 origin;Tween pressTween;float visualScale=1;
        void Awake(){button=GetComponent<Button>();group=gameObject.AddComponent<CanvasGroup>();game=GetComponentInParent<CapyIdleGame>();rect=(RectTransform)transform;origin=rect.anchoredPosition;}
        void LateUpdate(){if(!button||!group)return;group.alpha=button.interactable?1:.86f;if(pressed&&(!button.interactable||!game||game.PresentationPaused))Release();}
        void ApplyScale(float value){visualScale=value;rect.localScale=Vector3.one*value;rect.anchoredPosition=origin+new Vector2(rect.rect.width,-rect.rect.height)*((1-value)*.5f);}
        void Animate(bool down){
            if(pressTween.IsActive())pressTween.Kill();
            if(!game||game.Model==null||game.CosmeticReduced||!game.Tweens){ApplyScale(1);return;}
            pressTween=game.Tweens.Track(DOTween.To(()=>visualScale,ApplyScale,down?.97f:hovered?1.012f:1,down?.1f:.18f).SetEase(down?Ease.OutCubic:Ease.OutBack),gameObject);
        }
        void Release(){if(!pressed)return;pressed=false;Animate(false);}
        public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||!button.interactable||!game||game.PresentationPaused)return;pressed=true;Animate(true);}
        public void OnPointerUp(PointerEventData e){Release();}
        public void OnPointerEnter(PointerEventData e){if(e.pointerId>=0||!button.interactable||!game||game.PresentationPaused)return;hovered=true;if(!pressed)Animate(false);}
        public void OnPointerExit(PointerEventData e){hovered=false;pressed=false;Animate(false);}
        void OnDisable(){pressed=hovered=false;if(pressTween.IsActive())pressTween.Kill();if(rect)ApplyScale(1);}
    }
}

