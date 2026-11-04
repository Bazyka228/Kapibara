using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        // Atlas rectangles are top-left pixel coordinates in the 1254px source.
        // Crops are deliberately authored per silhouette rather than an equal grid.
        static readonly Rect[] LifeCrops={new Rect(55,211,360,198),new Rect(507,88,275,320),new Rect(895,54,330,360),new Rect(42,512,411,253),new Rect(490,430,335,354),new Rect(894,450,335,334),new Rect(20,825,423,353),new Rect(457,835,453,344),new Rect(912,815,326,371)};
        static readonly Rect[] LifeFrames={new Rect(12,125,216,119),new Rect(43,43,172,200),new Rect(40,26,190,217),new Rect(4,104,232,143),new Rect(35,30,200,211),new Rect(37,40,196,201),new Rect(2,48,235,196),new Rect(2,65,235,178),new Rect(48,25,190,216)};
        // Legacy atlas coordinates; SourcePose selects only original poses 1, 2, 3, 6 and 9.
        static readonly Rect[] LifeHeads={new Rect(128,134,91,98),new Rect(70,60,136,106),new Rect(90,32,131,101),new Rect(97,111,133,102),new Rect(94,40,133,93),new Rect(85,50,142,89),new Rect(126,61,103,102),new Rect(125,77,105,91),new Rect(108,39,121,94)};
        static readonly Rect[] LifeTorsos={new Rect(31,179,113,48),new Rect(63,156,118,68),new Rect(70,136,127,88),new Rect(29,177,127,46),new Rect(65,129,131,92),new Rect(68,137,129,86),new Rect(30,139,154,84),new Rect(28,145,160,77),new Rect(79,129,127,96)};
        static readonly Sprite[] LifeSprites=new Sprite[9];
        // Eye centres and deformation radii in the original atlas, before stage-eight mirroring.
        // A negative vertical radius marks an already closed eye: keep its relaxed expression.
        static readonly Vector4[][] LifeEyes={
            new[]{new Vector4(292,317,18,-16),new Vector4(390,316,13,-13)},
            new[]{new Vector4(646,190,16,19),new Vector4(744,173,14,18)},
            new[]{new Vector4(1087,140,16,18)},
            new[]{new Vector4(306,598,19,-18),new Vector4(413,576,14,17)},
            new[]{new Vector4(691,507,16,18)},
            new[]{new Vector4(1083,526,18,-14)},
            new[]{new Vector4(311,904,18,-14)},
            new[]{new Vector4(593,925,18,-14)},
            new[]{new Vector4(1118,894,18,-14),new Vector4(1198,891,16,-14)}
        };
        static Sprite LifeSprite(int age){
            if(LifeSprites[age-1])return LifeSprites[age-1];
            var texture=Resources.Load<Texture2D>("Characters/CapyLifeAtlas");
            if(!texture)throw new System.InvalidOperationException("Missing CapyLifeAtlas character artwork");
            Rect r=LifeCrops[age-1];float scale=texture.width/1254f;
            var sprite=Sprite.Create(texture,new Rect(r.x*scale,texture.height-(r.y+r.height)*scale,r.width*scale,r.height*scale),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name="Capy life stage "+age;LifeSprites[age-1]=sprite;return sprite;
        }
        static RectTransform FitArt(Transform p,string name,Rect target,float sourceX,float sourceY,float sourceW,float sourceH){
            float sx=target.width/sourceW,sy=target.height/sourceH;var layer=Rect(p,name,target.x-sourceX*sx,target.y-sourceY*sy,240,250);layer.localScale=new Vector3(sx,sy,1);return layer;
        }
        void DrawLifeAvatar(Transform root,string preview,bool guest,int stagePreview,bool showcase){
            var selected=guest||showcase?new[]{"","","moon","","","classic"}:(string[])Model.S.equipped.Clone();
            var item=IdleCatalog.Find(preview);if(item!=null){selected[item.slot]=item.id;if(item.IsSet)selected[1]="";else if(item.slot==5&&CapyCompleteSets.IsSet(selected[0]))selected[0]="";}
            int age=Mathf.Clamp(stagePreview>0?stagePreview:guest?4:Model.Stage,1,IdleModel.StageCount);
            int form=!guest&&!showcase&&Model.S.memeVisuals?Model.FormArt:-1;
            if(!showcase&&TryDrawBakedOutfit(root,selected,age,form))return;
            int stage=IdleModel.SourcePose(age);
            Rect f=LifeFrames[stage-1],h=LifeHeads[stage-1],b=LifeTorsos[stage-1];
            Shape(root,"Ground shadow",16,241,216,9,"65432D20",0,true);
            var pose=Rect(root,"Illustrated pose",0,0,240,250);
            var clothing=IdleCatalog.Find(selected[1]);
            var rearHat=IdleCatalog.Find(selected[0]);if(rearHat!=null&&rearHat.art==5)PaintedHeadphoneBack(pose,rearHat,h,stage);
            var portrait=Rect(pose,"Life sprite",f.x,f.y,f.width,f.height).gameObject.AddComponent<Image>();portrait.sprite=LifeSprite(stage);portrait.raycastTarget=false;
            // Keep the freshly created objects. Destroy() leaves the previous pose in the
            // hierarchy until end of frame, so Transform.Find can return a dying instance.
            if(root==hero){livingPose=pose;poseOrigin=pose.anchoredPosition;portraitMotion=portrait.gameObject.AddComponent<CapyPortraitMotion>();portraitMotion.Configure(LifeCrops[stage-1],LifeEyes[stage-1]);}
            // The growth map shows anatomy and age only, regardless of equipped items.
            if(showcase)return;
            var skin=IdleCatalog.Find(selected[5]);
            if(skin!=null){var motion=portrait.GetComponent<CapyPortraitMotion>();if(!motion){motion=portrait.gameObject.AddComponent<CapyPortraitMotion>();motion.Configure(LifeCrops[stage-1],LifeEyes[stage-1]);}motion.SetSkin(skin.art);}
            if(form>=0)portrait.color=Color.Lerp(portrait.color,C(IdleModel.Forms[form].color),.22f);
            Rect neck=stage==1?new Rect(116,174,61,55):stage==4?new Rect(101,164,78,58):new Rect(h.x-10,h.yMax-13,h.width*.93f,65);
            if(clothing!=null)PaintedClothes(pose,clothing,b,neck,stage);
            var hatBounds=new Rect(h.x,h.y+8,h.width,h.height);var headwear=FitArt(pose,"Age headwear",hatBounds,24,48,192,130);
            // Fur patterns live inside the sprite alpha; no old accessory-shaped skin overlays.
            var hat=IdleCatalog.Find(selected[0]);
            if(hat!=null)PaintedHat(pose,hat,h,stage);
            LifeForm(pose,headwear,form,b,h,stage);
            LifeEmotion(pose,IdleCatalog.Find(selected[4]),h);
            LifeAura(root,IdleCatalog.Find(selected[3]));
        }
        static void LifeAura(Transform root,IdleItem effect){AnimatedAura(root,effect);}
        static void LifeDefaultOutfit(Transform p,int stage,Rect b,Rect h,Rect neck){
            if(stage==2){
                Stroke(p,"Bib cord",h.x+14,h.yMax-14,b.xMax-8,b.y+8,"FFF1DB",3);
                var bib=Rounded(0,0,80,25,170,26,240,0,228,106,172,150,66,141,12,98);
                Contour(p,"Bib edging",b.x+45,b.y+3,54,61,"548E9E",bib);Contour(p,"Bib cotton",b.x+49,b.y+5,46,54,"FFF5DD",bib);
                Oval(p,"Bear ear",b.x+57,b.y+24,9,9,"EBA85F",1);Oval(p,"Bear ear",b.x+78,b.y+24,9,9,"EBA85F",1);Oval(p,"Bear bib badge",b.x+59,b.y+28,26,22,"EDAD69",1);
                Shape(p,"Bear eye",b.x+65,b.y+35,3,3,Ink,0,true);Shape(p,"Bear eye",b.x+77,b.y+35,3,3,Ink,0,true);Shape(p,"Bear nose",b.x+70,b.y+40,4,3,Ink,0,true);
            }
            if(stage==3||stage==5)LifeBackpack(p,b,stage==3?"83AD60":"536F72",stage==3);
            if(stage==6)LifeScarf(p,neck,"5D9AB4",true);
        }
        static void LifePacifier(Transform p,Rect h){
            float x=h.x+h.width*.72f,y=h.y+h.height*.79f;
            var shield=Rounded(0,72,16,6,87,0,120,35,165,0,226,12,240,76,207,143,155,150,117,113,70,150,16,122);
            Contour(p,"Pacifier shield edge",x,y,36,25,Ink,shield);Contour(p,"Pacifier shield",x+3,y+3,30,19,"5DB8CF",shield);
            Oval(p,"Pacifier ring",x+10,y+12,21,24,"C79051",2.5f);Oval(p,"Pacifier ring opening",x+15,y+16,11,14,"78C8D7",2);Shape(p,"Pacifier shine",x+5,y+7,4,5,"D1F7EF",0,true);
        }
        static void LifeBackpack(Transform p,Rect b,string color,bool frog){
            float x=b.x-5,y=b.y+5;
            Stroke(p,"Backpack shoulder",x+22,y-2,b.x+b.width*.64f,b.y+17,Ink,8);Stroke(p,"Backpack strap",x+22,y-3,b.x+b.width*.64f,b.y+15,color,4);
            var bag=Rect(p,"Fitted backpack",x,y,56,77);bag.localEulerAngles=new Vector3(0,0,-18);
            Oval(bag,"Backpack leather piping",0,0,49,69,color,3);Stroke(bag,"Backpack seam",25,8,22,62,Ink,2.5f);Stroke(bag,"Flap seam",6,22,39,24,Ink,2.5f);
            if(frog){Oval(bag,"Frog face",9,-5,33,27,"AEC874",2.5f);Oval(bag,"Frog eye",10,-11,11,12,"AEC874",2);Oval(bag,"Frog eye",29,-11,11,12,"AEC874",2);Shape(bag,"Frog pupil",14,-7,3,4,Ink,0,true);Shape(bag,"Frog pupil",33,-7,3,4,Ink,0,true);Stroke(bag,"Frog grin",21,11,29,12,Ink,2);}
            else{InkBox(bag,"Bag pocket",8,35,30,24,"72878B",7,2);Oval(bag,"Bag clasp",23,23,6,9,"D4AE73",1);}
        }
        static void LifeScarf(Transform p,Rect b,string color,bool striped){
            float x=b.x+10,y=b.y-4,w=b.width-10;
            InkBox(p,"Scarf hanging end",x+w-29,y+13,26,59,color,7,2.5f);
            for(int n=0;n<4;n++){if(striped)Shape(p,"Woven stripe",x+w-26,y+20+n*12,20,6,"FFF5E0",1);Stroke(p,"Scarf fringe",x+w-24+n*5,y+68,x+w-24+n*5,y+76,Ink,2);}
            var edge=Rounded(0,5,83,27,173,31,240,0,239,106,172,150,78,141,0,92);
            Contour(p,"Scarf wrap edge",x,y,w,29,Ink,edge);Contour(p,"Scarf wrap",x+3,y+3,w-6,23,color,edge);
            for(int n=0;n<4;n++)Shape(p,"Scarf cotton stripe",x+13+n*(w-24)/4,y+7,10,15,"FFF0D7",2);
        }
        static void LifeProneScarf(Transform p,int stage,string color){
            var collar=Rect(p,"Prone neck scarf",stage==1?119:108,stage==1?181:168,18,55);
            collar.localEulerAngles=new Vector3(0,0,-12);
            InkBox(collar,"Prone scarf wrap",0,0,17,51,color,7,2);
            for(int n=0;n<4;n++)Shape(collar,"Cotton stripe",3,5+n*11,11,4,"FFF0D7",1);
            InkBox(collar,"Short scarf end",-5,34,15,24,color,5,2);
            for(int n=0;n<3;n++)Stroke(collar,"Short fringe",-2+n*4,55,-2+n*4,61,Ink,1.5f);
        }
        static void LifeCape(Transform p,Rect b,string color,bool back){
            if(back){
                var edge=Rounded(166,0,227,12,240,140,198,150,159,136,106,149,48,136,0,119,57,82,111,19);
                Contour(p,"Cloak silhouette",b.x-80,b.y-9,b.width+102,121,Ink,edge);Contour(p,"Cloak silk",b.x-76,b.y-6,b.width+94,114,color,edge);
                Stroke(p,"Cloak gold hem",b.x-41,b.y+100,b.x+18,b.y+110,"DCA251",4);Stroke(p,"Cloak fold",b.x+6,b.y+16,b.x-20,b.y+92,"D5AF86",2);Stroke(p,"Cloak fold",b.x+25,b.y+11,b.x+8,b.y+92,"F6D7A8",3);
            }else{
                var shoulder=Rounded(0,0,69,38,153,51,240,26,217,90,129,150,42,119,0,45);
                Contour(p,"Guardian collar edge",b.x+2,b.y-11,b.width,39,Ink,shoulder);Contour(p,"Guardian collar",b.x+5,b.y-9,b.width-6,32,color,shoulder);
                Stroke(p,"Guardian gold trim",b.x+13,b.y+1,b.x+b.width-20,b.y+18,"DDA34D",3);
                Oval(p,"Guardian brooch",b.x+b.width-38,b.y+12,24,26,"E9B85C",2.5f);Oval(p,"Guardian brooch inset",b.x+b.width-32,b.y+18,12,14,"FFD88B",1.5f);
            }
        }
        static void LifeClothes(Transform p,IdleItem item,Rect b,Rect h,Rect neck){
            if(item.art==0){LifeScarf(p,neck,item.color,false);return;}
            if(item.art==6){LifeBackpack(p,b,item.color,false);return;}
            if(item.art==3){LifeCape(p,b,item.color,false);return;}
            if(item.art==4||item.art==5||item.art==7||item.art==9){LifeTunic(p,b,item);return;}
            // Garments share the new torso anchors; legs and the illustrated face remain unobscured.
            var layer=FitArt(p,"Fitted garment",b,55,153,142,81);DrawBody(layer,item);
        }
        static void LifeTunic(Transform p,Rect b,IdleItem item){
            var garment=Rect(p,"Tailored torso garment",b.x,b.y,b.width,b.height);garment.localScale=Vector3.one;
            var shape=Rounded(86,0,190,0,216,34,233,76,240,126,218,150,50,150,3,130,0,87,26,34);
            Contour(garment,"Tunic outline",0,0,b.width,b.height,Ink,shape);Contour(garment,"Tunic fabric",3,3,b.width-6,b.height-6,item.color,shape);
            Stroke(garment,"Shoulder seam",b.width*.63f,9,b.width*.79f,20,"FFF1D080",2);
            Stroke(garment,"Fabric fold",b.width*.22f,b.height*.6f,b.width*.17f,b.height*.82f,"65432D40",2);
            Stroke(garment,"Tunic hem",b.width*.25f,b.height-7,b.width*.86f,b.height-7,"65432D60",2);
            if(item.art==4){for(int n=0;n<5;n++){float x=b.width*.24f+n*b.width*.12f;Stroke(garment,"Wool stitch",x,b.height*.65f,x+3,b.height*.72f,"F1E0E9",1.5f);Stroke(garment,"Wool stitch",x+3,b.height*.72f,x+6,b.height*.65f,"F1E0E9",1.5f);}Star(garment,b.width*.47f,b.height*.24f,23,"F5D68E");}
            if(item.art==5){Stroke(garment,"Team piping",b.width*.2f,b.height*.37f,b.width*.88f,b.height*.37f,"FFF1DB",4);Mark(garment,"6–7",b.width*.36f,b.height*.4f,b.width*.48f,b.height*.4f,23,Ink);}
            if(item.art==7){Stroke(garment,"Raincoat closure",b.width*.65f,8,b.width*.65f,b.height-7,"A87C3C",2);for(int n=0;n<3;n++)Oval(garment,"Raincoat button",b.width*.72f,17+n*16,5,5,"FFF5D5",1);InkBox(garment,"Raincoat pocket",b.width*.26f,b.height*.56f,23,19,"E5B25C",6,2);}
            if(item.art==9){Oval(garment,"Mandarin applique",b.width*.43f,b.height*.38f,29,26,"EEA04C",2);Leaf(garment,b.width*.53f,b.height*.3f,16,12,"7AA464");}
        }
        static void LifeCap(Transform p,Rect h){
            var layer=FitArt(p,"Backwards cap",new Rect(h.x-7,h.y+4,h.width*.87f,h.height),24,48,192,130);
            layer.localScale=new Vector3(-layer.localScale.x,layer.localScale.y,1);layer.anchoredPosition+=new Vector2(h.width*1.25f,0);DrawHead(layer,IdleCatalog.Find("cap"));
        }
        static void LifeLaurel(Transform p,Rect h){
            float x=h.x-9,y=h.y+25;
            Stroke(p,"Laurel stem",x,y+18,x+h.width*.76f,y-31,"745133",3);
            for(int n=0;n<7;n++){float u=x+n*h.width*.105f,v=y+12-n*7;Leaf(p,u,v,18,12,n%2==0?"79A552":"95B368",-24);Leaf(p,u+4,v-8,12,18,"7BA44E",12);}
        }
        static void LifeGlasses(Transform p,Rect h,int stage){
            float x=h.x+h.width*.35f,y=h.y+h.height*.39f;
            var lens=Oval(p,"Near spectacle",x,y,30,27,"A8D6C7",2.8f);Shape(lens.transform,"Glass reflection",6,5,5,12,"E8F4DF",2);
            Oval(p,"Far spectacle",x+h.width*.39f,y-6,22,23,"A8D6C7",2.8f);
            Stroke(p,"Spectacle bridge",x+29,y+9,x+h.width*.39f,y+5,Ink,2.5f);Stroke(p,"Spectacle arm",x,y+9,h.x+7,y-6,Ink,2.5f);
        }
        static void LifeSpark(Transform p,float x,float y,float size){
            var points=Rounded(120,0,155,55,240,75,155,98,120,150,88,98,0,75,88,55);
            Contour(p,"Guardian sparkle",x,y,size,size,"DDA354",points);Contour(p,"Sparkle centre",x+3,y+3,size-6,size-6,"FFF5DA",points);
        }
        static void LifeEmotion(Transform p,IdleItem emotion,Rect h){AnimatedEmotion(p,emotion,h);}
        static void LifeForm(Transform p,Transform headArt,int form,Rect b,Rect h,int stage){
            if(form<0)return;
            if(form==0){InkBox(p,"Six seven badge",b.x+31,b.y+33,38,25,"CBE2AD",6,2);Mark(p,"6–7",b.x+34,b.y+33,32,25,18,Ink);}
            if(form==1){Oval(headArt,"Zen orange",106,16,36,32,"F4B652",3);Leaf(headArt,124,9,23,14,"88B76A");}
            if(form==2)Heart(p,216,123,20,"F2A1A0");
            if(form==3)for(int n=0;n<3;n++)InkBox(p,"Pixel coat",b.x+24+n*10,b.y+38+n%2*10,9,9,"F3D9A3",1,1);
            if(form==4){Star(p,20,130,22,"FFF0AD");Star(p,221,88,20,"FFF0AD");}
            if(form==5){InkBox(p,"Disco charm",b.x+37,b.y+44,20,16,"9DCCB9",5,2);Star(p,b.x+58,b.y+42,20,"E9A3A6");}
            if(form==6){Stroke(headArt,"Ninja band",35,65,178,57,"BC794C",6);Leaf(headArt,45,49,29,17,"7AA65D");}
            if(form==7)LifeGlasses(p,h,stage);
        }
    }
}


