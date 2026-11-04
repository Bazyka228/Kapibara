using System.Collections.Generic;
using UnityEngine;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        void RebuildHero(){if(!hero)return;Clear(hero);Clear(decor);var scenery=Rect(decor,"Room scenery",0,0,480,850);DrawScene(scenery,IdleCatalog.Find(Model.S.equipped[2]),true);
            DrawRestingPlace();
            HomeFurniture();
            DrawAvatar(hero);BuildLifeVisuals();hero.localScale=Vector3.one*HeroScale;hero.anchoredPosition=new Vector2(240-120*HeroScale,-270-(1-HeroScale)*220);
        }
        void DrawRestingPlace(){
            int level=Model.S.bed;if(level<=0)return;
            string[] fabric={"A8BF7C","91B68C","81ABA0","82A8B5","A89EB7","CA9E9B","DEB379","E4C583"};
            string[] seam={"DCE7AE","D1E5B7","BEE0D1","CEE5E5","E3D8EB","F0D5CC","FFE0A0","FFF0BB"};
            var bed=Rect(decor,"Resting place level "+level,94,483,292,51);
            Shape(bed,"Ground shadow",0,37,292,13,"68492D24",0,true);
            if(level>=3){
                InkBox(bed,"Woven base",6,19,280,28,level>=7?"AC784C":"B49367",12,2);
                for(int n=0;n<17;n++)Stroke(bed,"Wicker thread",15+n*16,31,20+n*16,42,"EACB96",1.5f);
                Stroke(bed,"Wicker rim",18,39,275,39,"805A3B",1.2f);
            }
            Oval(bed,"Comfy cushion",3,3,286,42,fabric[level-1],3);
            Oval(bed,"Cushion piping",10,7,272,31,seam[level-1],1.5f);
            Shape(bed,"Soft fabric centre",15,9,262,25,fabric[level-1],0,true);
            Shape(bed,"Fabric sheen",35,10,220,7,"FFF5D340",0,true);
            for(int n=0;n<12;n++)Stroke(bed,"Hand sewn hem",28+n*21,34,34+n*21,34,seam[level-1],1.2f);
            if(level>=2)for(int side=0;side<2;side++){
                float x=24+side*238;Oval(bed,"Tuft shadow",x,17,7,5,"74583A35",0);Shape(bed,"Tuft stitch",x+1,17,4,2,seam[level-1],1);
            }
            if(level>=4)for(int side=0;side<2;side++){
                float x=12+side*254;Leaf(bed,x,17,17,8,seam[level-1],side==0?-25:25);
                if(level>=5)Leaf(bed,x+4,25,14,7,seam[level-1],side==0?25:-25);
            }
            if(level>=6){Oval(bed,"Cushion seal",134,33,24,14,"B18451",1);Oval(bed,"Seal enamel",138,35,16,9,"F5D892",0);}
            if(level>=7)for(int side=0;side<2;side++){float x=6+side*273;Stroke(bed,"Golden tassel cord",x,31,x,45,"E9C777",2);Oval(bed,"Golden tassel",x-3,42,7,7,"F6D990",1);}
            if(level==8){Star(bed,136,34,15,"FFF1B5");Star(bed,23,17,10,"FFF1B5");Star(bed,259,17,10,"FFF1B5");}
        }
        static Vector2[] Rounded(params float[] coords){var corners=new List<Vector2>();for(int n=0;n<coords.Length;n+=2)corners.Add(new Vector2(coords[n],coords[n+1]));var points=new List<Vector2>();
            for(int n=0;n<corners.Count;n++){var p=corners[n];var a=Vector2.Lerp(p,corners[(n+corners.Count-1)%corners.Count],.32f);var b=Vector2.Lerp(p,corners[(n+1)%corners.Count],.32f);for(int k=0;k<7;k++){float t=k/6f;points.Add((1-t)*(1-t)*a+2*(1-t)*t*p+t*t*b);}}return points.ToArray();}
        static CapyContour Contour(Transform p,string n,float x,float y,float w,float h,string color,Vector2[] points){var c=Rect(p,n,x,y,w,h).gameObject.AddComponent<CapyContour>();c.points=points;c.sourceSize=new Vector2(240,150);c.color=C(color);c.raycastTarget=false;return c;}
        void DrawAvatar(Transform root,string preview=null,bool guest=false,int stagePreview=0,bool showcase=false){protectedArtDepth++;try{DrawLifeAvatar(root,preview,guest,stagePreview,showcase);}finally{protectedArtDepth--;}}
        static void DrawFoodIcon(Transform parent,int kind,float x,float y){
            var p=Rect(parent,"Food illustration "+kind,x,y,72,64);
            string[] glazes={"82BBA3","E3A272","73BDB9","B98B59","AA8EB4","81B78C","BD825D","E3B372","91B7A6","86ABA9","BE9869","C3965F"};
            int art=kind==IdleModel.PremiumMeal?12:Mathf.Clamp(kind,0,11);
            string glaze=art==12?"A1BEBE":glazes[art];
            Shape(p,"Dish soft shadow",0,51,72,11,"65432D29",0,true);
            if(art==3||art==10||art==11){
                InkBox(p,"Woven basket",4,24,64,34,glaze,11,2);
                for(int n=0;n<7;n++)Stroke(p,"Basket weave",9+n*8,29,9+n*8,53,"E7C392",1.3f);
                for(int n=0;n<3;n++)Stroke(p,"Basket cross weave",8,34+n*7,64,34+n*7,"8E613E",1.2f);
                Oval(p,"Basket interior",4,18,64,19,"70543A",2);
            }else{
                InkBox(p,"Glazed ceramic bowl",4,24,64,34,glaze,15,2);
                Shape(p,"Bowl foot",18,54,36,5,"65432D",2);Shape(p,"Foot glaze",21,54,30,3,glaze,1);
                Shape(p,"Bowl reflected light",12,35,6,12,"FFFFFF65",3);
                Stroke(p,"Bowl lower glaze",26,51,56,49,"FFF5CF88",2);
                Oval(p,"Dish interior",3,17,66,22,"F5E3BC",2);
                Oval(p,"Dish glaze inside",8,20,56,14,art==6?"E6AB62":"B69E73",1);
            }
            switch(art){
                case 0:
                    Leaf(p,7,6,24,30,"6BA767",-19);Leaf(p,24,0,24,33,"9DC875",9);Leaf(p,41,6,22,28,"80B97D",28);
                    for(int n=0;n<3;n++){FoodSlice(p,16+n*15,25+n%2*3,13,"83B778","DCF0AE");Shape(p,"Leaf dew",26+n*13,13,3,5,"F3FFE4",1);}
                    break;
                case 1:
                    Carrot(p,10,2,-13);Carrot(p,29,-3,5);Carrot(p,47,3,19);
                    FoodSlice(p,10,29,15,"C47A43","FFD391");FoodSlice(p,47,30,14,"C47A43","FFD391");
                    break;
                case 2:
                    Mandarin(p,8,7,29);Mandarin(p,33,-2,31);
                    FoodSlice(p,27,27,19,"D88E40","FFD882");
                    for(int n=0;n<5;n++){float a=n*1.256f;Stroke(p,"Citrus segment",36,36,36+Mathf.Cos(a)*6,36+Mathf.Sin(a)*6,"FFF0B6",1);}
                    break;
                case 3:
                    InkBox(p,"Linen napkin",12,19,48,24,"F5DCB2",4,1);
                    for(int n=0;n<4;n++)Stroke(p,"Napkin checks",17+n*10,21,17+n*10,40,"DB9D85",1);
                    FoodToast(p,11,3);FoodToast(p,32,7);Berry(p,54,24,12,"CE7D87");Leaf(p,4,18,13,21,"89B278",-20);break;
                case 4:
                    for(int n=0;n<9;n++)Berry(p,8+n%4*13,5+n/4*11,15,n%3==0?"8F8EBC":"DA8097");
                    Leaf(p,39,0,24,13,"7FAD76",8);Shape(p,"Yoghurt curl",18,29,29,5,"FFF0D5",2);break;
                case 5:
                    Leaf(p,4,5,28,24,"6FA369",-18);Leaf(p,37,4,26,26,"97C481",12);
                    for(int n=0;n<5;n++){float cx=10+n*10,cy=14+n%2*10;FoodSlice(p,cx,cy,21,"568B59","D8E8AB");
                        for(int k=0;k<3;k++)Shape(p,"Cucumber seed",cx+7+k*3,cy+8+k%2*3,2,4,"F9F1C4",1);}
                    break;
                case 6:
                    Oval(p,"Soup handle",0,25,13,16,"C99464",2);Oval(p,"Soup handle",59,25,13,16,"C99464",2);
                    Oval(p,"Pumpkin soup",8,18,56,20,"EBB15F",1);
                    Oval(p,"Cream swirl",21,23,26,10,"FFE7AD",0);Oval(p,"Soup centre",26,25,15,5,"EBB15F",0);
                    Leaf(p,37,17,17,10,"7CA76B",10);
                    for(int n=0;n<3;n++){InkBox(p,"Golden crouton",13+n*17,23+n%2*7,6,6,"EBCB89",1,1);Stroke(p,"Soup steam",20+n*16,11,23+n*16,3,"E3CFA2",2);}
                    break;
                case 7:
                    Leaf(p,7,4,21,23,"8CB875",-15);Mandarin(p,33,3,24);
                    FoodSlice(p,8,19,20,"82B16D","E5E8AC");
                    for(int n=0;n<4;n++){InkBox(p,"Melon cube",23+n%2*16,15+n/2*12,13,12,"F4CF81",3,1);Shape(p,"Melon gleam",26+n%2*16,17+n/2*12,5,2,"FFF0BF",1);}
                    Berry(p,47,27,14,"D78891");break;
                case 8:
                    Leaf(p,7,17,22,26,"74A169",-28);Leaf(p,42,18,25,24,"8EB16C",28);
                    for(int cob=0;cob<2;cob++){
                        var corn=Rect(p,"Sweet corn",13+cob*23,1+cob*6,21,35);corn.localEulerAngles=new Vector3(0,0,cob==0?-12:12);
                        InkBox(corn,"Corn outline",0,0,22,37,"D7A84F",9,1.5f);
                        for(int row=0;row<5;row++)for(int col=0;col<3;col++)Shape(corn,"Corn kernel",4+col*5,4+row*6,4,5,(row+col)%2==0?"F9DD82":"EDC568",2);
                    }break;
                case 9:
                    Oval(p,"Rice mound",10,6,51,31,"F5E6BC",2);
                    for(int n=0;n<18;n++)Shape(p,"Rice grain",16+(n*13)%38,11+(n*7)%21,5,2,n%2==0?"FFF7DC":"D9C89F",1);
                    for(int n=0;n<6;n++){float cx=13+n*8,cy=14+n%3*6;Shape(p,"Carrot dice",cx,cy,6,5,"E6A267",1);Oval(p,"Green pea",cx+2,cy+7,5,5,"8DB478",.6f);}break;
                case 10:
                    Leaf(p,5,12,23,25,"819E63",-20);Leaf(p,45,13,20,23,"9AB578",25);
                    Mushroom(p,10,4,23);Mushroom(p,32,0,27);Berry(p,20,27,10,"BD8091");Berry(p,43,29,11,"9A8CAA");break;
                case 11:
                    Leaf(p,3,8,24,28,"94B36C",-18);Carrot(p,47,1,18);Mandarin(p,7,14,23);
                    Oval(p,"Harvest pumpkin",27,9,28,30,"E6A057",1.5f);
                    for(int n=0;n<3;n++)Stroke(p,"Pumpkin rib",33+n*6,14,32+n*7,32,"BE8246",1);
                    InkBox(p,"Pumpkin stem",38,4,5,9,"83A366",2,1);Berry(p,47,30,10,"AE7C92");break;
                default:
                    Oval(p,"Celebration rice",10,7,51,28,"F5E9C4",2);
                    for(int n=0;n<4;n++)Berry(p,8+n*14,21+n%2*5,12,n%2==0?"DA8793":"A5A1C8");
                    Star(p,23,-1,27,"EFCC7B");Leaf(p,51,9,15,20,"8EBA85",24);
                    Shape(p,"Pearl rice glint",16,14,5,3,"FFFBE7",1);break;
            }
            InkBox(p,"Dish lip",3,37,66,7,art==3||art==10||art==11?"D4AA75":"F5DFAE",3,1.4f);
            if(art==12){Star(p,29,43,12,"F5D78B");Stroke(p,"Gold bowl line",12,48,59,48,"E7BC69",1);}
            else if(art!=3&&art!=10&&art!=11){Shape(p,"Ceramic maker mark",32,45,7,4,"FFF4CB",2);Shape(p,"Maker dot",41,46,2,2,"FFF4CB",1);}
        }
        static void FoodSlice(Transform p,float x,float y,float size,string rind,string flesh){
            Oval(p,"Slice rind",x,y,size,size,rind,1.2f);Oval(p,"Slice flesh",x+3,y+3,size-6,size-6,flesh,.6f);
            Shape(p,"Slice gloss",x+size*.27f,y+size*.24f,size*.3f,2,"FFF8D5",1);
        }
        static void Berry(Transform p,float x,float y,float size,string color){
            Oval(p,"Juicy berry",x,y,size,size,color,1);
            Shape(p,"Berry gloss",x+size*.25f,y+size*.2f,size*.3f,size*.18f,"FFE6D5",1);
            for(int n=0;n<3;n++)Shape(p,"Berry seeds",x+size*.3f+n*size*.2f,y+size*.58f+n%2*2,1.4f,2,"F3CE9F",.6f);
        }
        static void Mandarin(Transform p,float x,float y,float size){
            Oval(p,"Ripe mandarin",x,y,size,size*.9f,"EDAD59",1.5f);
            Shape(p,"Citrus light",x+size*.2f,y+size*.2f,size*.32f,size*.18f,"FFE1A2",2);
            for(int n=0;n<3;n++)Shape(p,"Citrus pores",x+size*.55f+n%2*4,y+size*.5f+n*3,1.6f,1.6f,"CD8D46",.8f);
            Leaf(p,x+size*.45f,y-3,size*.45f,size*.27f,"7AA569",8);
        }
        static void Carrot(Transform p,float x,float y,float angle){
            var r=Rect(p,"Whole carrot",x,y,18,37);r.localEulerAngles=new Vector3(0,0,angle);
            Leaf(r,3,0,9,13,"7AA267",-14);Leaf(r,9,-1,9,13,"9BB77A",25);
            var outline=Rounded(0,8,240,8,180,110,120,150,55,110);
            Contour(r,"Carrot edge",1,9,17,28,Ink,outline);Contour(r,"Carrot flesh",3,11,13,23,"EFB471",outline);
            for(int n=0;n<3;n++)Stroke(r,"Carrot grooves",6,15+n*6,12-n,15+n*6,"C68C52",1);
        }
        static void FoodToast(Transform p,float x,float y){
            InkBox(p,"Wholegrain toast",x,y,25,27,"D5A671",7,1.5f);InkBox(p,"Bread crumb",x+3,y+3,19,20,"F4D9A7",5,1);
            for(int n=0;n<4;n++)Shape(p,"Bread pores",x+6+n%2*8,y+7+n/2*7,2,2,"CAA77A",1);
            Leaf(p,x+3,y+16,21,10,"85AD71",0);
        }
        static void Mushroom(Transform p,float x,float y,float size){
            InkBox(p,"Mushroom stem",x+size*.35f,y+size*.35f,size*.3f,size*.8f,"E8D5AA",3,1);
            Oval(p,"Chestnut mushroom",x,y,size,size*.62f,"BF9369",1.3f);
            Shape(p,"Mushroom shine",x+size*.2f,y+size*.14f,size*.35f,3,"E9C79A",1);
            Stroke(p,"Mushroom gills",x+4,y+size*.48f,x+size-4,y+size*.48f,"8F6849",1);
        }
    }
}


