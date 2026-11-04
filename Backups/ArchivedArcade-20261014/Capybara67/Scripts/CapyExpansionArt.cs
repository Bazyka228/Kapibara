using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    public sealed partial class CapyGame
    {
        RectTransform headLayer,bodyLayer,skinLayer,sceneLayer;
        SoftShape mouthShape; RectTransform[] brows;
        static void ClearLayer(Transform t){for(int n=t.childCount-1;n>=0;n--){var g=t.GetChild(n).gameObject;g.SetActive(false);Destroy(g);}}
        static SoftShape Tile(Transform t,string n,float x,float y,float w,float h,string c,float r=8)=>Shape(t,n,x,y,w,h,c,r);
        static void Mark(Transform t,string text,float x,float y,float w,float h,int size,string color)=>Label(t,"Emblem",text,x,y,w,h,size,color,TextAnchor.MiddleCenter,true);
        void BuildExpansionArt()
        {
            bodyLayer=Rect(capy,"Clothing slot",0,0,232,220);bodyLayer.SetSiblingIndex(capy.Find("Head outline").GetSiblingIndex());
            skinLayer=Rect(capy,"Character signature",0,0,232,220);
            headLayer=Rect(capy,"Head slot",0,0,232,220);
            sceneLayer=Rect(board,"Scenery slot",0,0,480,850);sceneLayer.SetSiblingIndex(capy.GetSiblingIndex());
            for(int n=0;n<17;n++)board.Find("Flight guide "+n).SetSiblingIndex(sceneLayer.GetSiblingIndex()+1);
            mouthShape=capy.Find("Mouth").GetComponent<SoftShape>();
            var bs=new System.Collections.Generic.List<RectTransform>();
            foreach(var s in capy.GetComponentsInChildren<SoftShape>(true)){
                if(s.name=="Eyebrow")bs.Add(s.rectTransform);
                if(s.name=="Muzzle freckle" || s.name=="Cheek fur")s.gameObject.SetActive(false);
            }
            brows=bs.ToArray();
        }
        void ApplyAppearance(string preview=null)
        {
            if(progress==null || !headLayer)return;
            var chosen=(string[])progress.equipped.Clone();var pi=CapyCatalog.Find(preview);if(pi!=null)chosen[(int)pi.slot]=preview;
            var skin=CapyCatalog.Find(chosen[5])??CapyCatalog.Find("classic");
            Color coat=C(skin.color),light=Color.Lerp(coat,C("FFF0CE"),.46f),shadow=Color.Lerp(coat,C("51394C"),.35f);
            foreach(var s in capy.GetComponentsInChildren<SoftShape>(true)){
                if(s.transform.IsChildOf(headLayer) || s.transform.IsChildOf(bodyLayer) || s.transform.IsChildOf(skinLayer))continue;
                switch(s.name){case "Body":case "Head":case "Ear left":case "Ear right":case "Lid":s.color=coat;break;
                    case "Face light":case "Muzzle":case "Belly":s.color=light;break;
                    case "Foot left":case "Foot right":case "Eyebrow":s.color=shadow;break;}
            }
            foreach(var paw in new[]{leftPaw,rightPaw}){paw.Find("Paw").GetComponent<SoftShape>().color=coat;paw.Find("Palm").GetComponent<SoftShape>().color=light;}
            var head=capy.Find("Head").GetComponent<SoftShape>();var outline=capy.Find("Head outline").GetComponent<SoftShape>();
            int sil=progress.silhouette;
            head.radius=outline.radius=sil==2?13:sil==1?59:34;head.SetVerticesDirty();outline.SetVerticesDirty();
            var muzzle=capy.Find("Muzzle").GetComponent<SoftShape>();muzzle.rectTransform.sizeDelta=new Vector2(sil==1?116:140,52);muzzle.rectTransform.anchoredPosition=new Vector2(sil==1?66:54,-96);muzzle.radius=sil==2?10:24;muzzle.SetVerticesDirty();
            capy.Find("Body").localScale=new Vector3(sil==1?1.08f:skin.id=="sleepy"?1.04f:.96f,skin.id=="sleepy"?.91f:1,1);
            ClearLayer(headLayer);ClearLayer(bodyLayer);ClearLayer(skinLayer);ClearLayer(sceneLayer);
            DrawHead(headLayer,CapyCatalog.Find(chosen[0]),progress.goldenCrown);
            DrawBody(bodyLayer,CapyCatalog.Find(chosen[1]));
            DrawSignature(skinLayer,skin,chosen[0]=="");
            DrawScene(sceneLayer,CapyCatalog.Find(chosen[2]));
            foreach(var old in cosmetics)if(old)old.SetActive(false);
            if(redesignReady)ApplyRedesignAppearance(skin);
        }
        static void DrawHead(Transform t,CapyItem i,bool gold=false)
        {
            if(i==null)return;string c=i.color;
            switch(i.art){
                case 0:Tile(t,"Panama crown",49,1,140,35,"77C6B5",12);Tile(t,"Asymmetric brim",28,25,180,12,"55978F",6);Tile(t,"Stitch one",111,15,20,3,"493047",1);Tile(t,"Stitch two",141,15,11,3,"493047",1);break;
                case 1:Tile(t,"Rhythm band",29,35,180,12,"E98C79",5);Tile(t,"Band tail",17,40,15,34,"B96F73",5);Mark(t,"6 7",75,27,87,23,16,"493047");break;
                case 2:Tile(t,"Open headband",40,24,155,8,gold?"D9AE65":"7FAB9A",4);for(int n=0;n<2;n++){var l=Shape(t,"Mandarin leaf",41+n*24,3-n*8,38,18,"77B8A1",0,true);l.transform.localEulerAngles=new Vector3(0,0,20+n*20);}break;
                case 3:for(int n=0;n<2;n++){var stalk=Tile(t,"Antenna",62+n*102,-11,5,41,"A998CD",2);stalk.transform.localEulerAngles=new Vector3(0,0,n==0?20:-20);var tip=Tile(t,"Ping diamond",49+n*123,-26,23,23,"9E90D5",5);tip.transform.localEulerAngles=new Vector3(0,0,45);}break;
                case 4:Tile(t,"Low cowboy crown",67,-8,104,40,"BD8874",15);Tile(t,"Dipped brim",27,24,192,13,"966C67",6);Tile(t,"Upturned end",27,14,32,15,"BD8874",6);Tile(t,"Hat seam",78,17,91,5,"493047",2);break;
                case 5:Tile(t,"Headphone arch",37,18,173,11,"75639B",5);for(int n=0;n<2;n++){Tile(t,"Square ear cup",24+n*172,30,31,43,"55B7A7",10);Tile(t,"Cushion",30+n*172,37,19,29,"493047",6);}break;
                case 6:Tile(t,"Winter crown",42,0,162,36,"A79ABF",14);Tile(t,"Winter seam",66,18,107,5,"D3BCD2",2);Tile(t,"Left flap",30,25,27,41,"9285AB",10);Tile(t,"Right flap",191,25,27,35,"9285AB",10);break;
                case 7:Tile(t,"Open ninja band",33,29,177,14,"77688F",5);Tile(t,"Ninja knot",19,37,20,19,"9D8BB0",6);Tile(t,"Ninja tail",8,48,18,34,"77688F",5);break;
                case 8:Tile(t,"Builder dome",48,-1,149,33,"D9AE65",16);Tile(t,"Builder brim",30,27,183,10,"B48A55",5);Tile(t,"Long ridge",107,-5,10,32,"F0D09A",4);Tile(t,"Short ridge",127,3,8,24,"F0D09A",4);break;
                case 9:Tile(t,"Chef folded cap",57,-27,134,58,"E9DCC3",20);Tile(t,"Chef crease one",93,-12,5,32,"C7B8A8",2);Tile(t,"Chef crease two",144,-18,5,39,"C7B8A8",2);Tile(t,"Chef band",52,23,146,12,"756079",4);break;
                case 10:var a=Tile(t,"Fold left",64,0,62,35,"B5D8DA",2);a.transform.localEulerAngles=new Vector3(0,0,20);var b=Tile(t,"Fold right",123,-8,67,41,"D0E4DF",2);b.transform.localEulerAngles=new Vector3(0,0,-24);Tile(t,"Paper rim",44,27,158,14,"9ABFC4",3);Tile(t,"Route line",76,30,70,3,"D98B7A",1);break;
                case 11:Tile(t,"Orbital arch",22,15,215,8,"ABC3D4",4);Tile(t,"Orbital rear",14,24,11,121,"7897B1",5);Tile(t,"Orbital collar",24,154,210,10,"ABC3D4",5);Tile(t,"Glass edge",232,32,4,111,"A6D7DF",2);Tile(t,"Glass glint",224,42,4,24,"D9EFEB",2);break;
            }
        }
        static void DrawBody(Transform t,CapyItem i)
        {
            if(i==null)return;
            switch(i.art){
                case 0:Tile(t,"Closed snood",63,143,125,24,"C888A8",12);Tile(t,"Snood fold",85,156,72,4,"A06990",2);break;
                case 1:Tile(t,"Vest left",55,150,60,67,"89B7AA",13);Tile(t,"Vest right",128,151,63,67,"70A293",13);Tile(t,"Vest pocket",143,182,31,23,"A5D0BD",5);break;
                case 2:Tile(t,"Apron",74,151,107,67,"E9DCC3",14);for(int n=0;n<2;n++)Tile(t,"Apron strap",80+n*78,134,8,35,"C6B79A",3);Shape(t,"Mandarin emblem",112,169,31,27,"DC9C61",0,true);Tile(t,"Slice cut",126,170,3,24,"E9DCC3",1);break;
                case 3:Tile(t,"Cape left",45,161,50,62,"7A91C7",15);Tile(t,"Cape right",164,155,38,62,"6379AF",12);Tile(t,"Cape clasp",114,145,13,8,"E5C99B",3);Tile(t,"Cape clasp high",134,141,13,12,"E5C99B",3);break;
                case 4:Tile(t,"Sweater",55,142,140,75,"AF98C3",22);Mark(t,"6–7",79,164,100,35,28,"493047");Tile(t,"Ribbing",70,209,112,5,"8A78A2",2);break;
                case 5:Tile(t,"Sport top",64,144,125,72,"62517E",20);Tile(t,"Mint track",81,162,30,42,"77C6B5",13);Tile(t,"Coral track",139,153,30,48,"D89080",13);break;
                case 6:Tile(t,"Backpack side",42,158,29,55,"B59270",12);for(int n=0;n<2;n++)Tile(t,"Bag strap",78+n*81,145,12,58,"C5A888",5);Tile(t,"Bag buckle",81,187,7,10,"746271",2);break;
                case 7:Tile(t,"Raincoat",53,144,143,75,"8BBDAE",22);Tile(t,"Raincoat seam",124,150,4,60,"679A90",2);Tile(t,"Raincoat clasp",137,170,13,10,"E4CEAC",4);break;
                case 8:Tile(t,"Tool belt",57,189,141,14,"926C60",5);for(int n=0;n<2;n++)Tile(t,"Soft tool pocket",74+n*75,180,32,32,"B68B70",7);Tile(t,"Rounded tool",156,166,9,28,"8CABB0",4);break;
                case 9:Tile(t,"Smile shirt",60,145,133,70,"D6A393",20);Tile(t,"Smile eye",105,170,6,6,"493047",3);Tile(t,"Smile eye two",135,168,7,7,"493047",3);Tile(t,"Smile mark",114,184,24,5,"493047",2);break;
            }
        }
        static void DrawSignature(Transform t,CapyItem s,bool bareHead)
        {
            switch(s.art){
                case 1:if(bareHead){Tile(t,"Sport band",24,43,195,12,"F3E6C8",5);Tile(t,"Sport knot",215,37,12,30,"D88771",4);}break;
                case 2:Tile(t,"Sleepy tuft",86,38,29,6,"D6C5B8",3);break;
                case 3:for(int n=0;n<2;n++)Tile(t,"Neon stripe",40+n*144,118,18,33,"64E3D3",6);break;
                case 4:for(int n=0;n<4;n++)Tile(t,"Pixel cheek",36+n%2*9,111+n/2*9,7,7,"E7BD83",0);break;
                case 5:Shape(t,"Sunny cheek L",38,103,26,12,"F1B28B",0,true);Shape(t,"Sunny cheek R",181,103,26,12,"F1B28B",0,true);break;
                case 6:if(bareHead){var glass=Shape(t,"Helmet glass",-2,12,250,168,"BFE8EF22",42);Tile(t,"Helmet rim",14,154,223,10,"D4DBE8",5);Tile(t,"Helmet shine",29,41,7,35,"DEF6F6",3);}Mark(t,"+",182,135,30,30,25,"E8E0F4");break;
                case 7:for(int n=0;n<5;n++)Shape(t,"Fluffy lock",25+n*40,137+n%2*8,34,25,"F3E7D0",0,true);break;
                case 8:Tile(t,"Robot panel",103,143,49,10,"657F91",4);Tile(t,"Robot light",120,145,9,6,"F6D783",3);for(int n=0;n<2;n++)Tile(t,"Rivet",33+n*172,103,6,6,"D8E5E6",3);break;
                case 9:Tile(t,"Golden facet",43,52,12,43,"FFE6A0",5);Tile(t,"Golden chest",90,185,57,5,"F1D389",2);break;
            }
        }
        static void DrawScene(Transform t,CapyItem i)
        {
            if(i==null)return;Tile(t,"Scenery field",0,274,480,400,"211B45",0);
            int a=i.art;
            if(a==0){for(int n=0;n<6;n++){float x=n<3?n*37:371+(n-3)*39;Tile(t,"Distant building",x,460-n%3*39,31,203,"302A4C",5);for(int k=0;k<3;k++)Tile(t,"Quiet window",x+12,480+k*37-n%3*24,6,10,"50706B",2);}}
            if(a==1){Tile(t,"Distant skyline",0,590,480,80,"3C3153",0);Tile(t,"Rooftop ledge",0,642,480,19,"65516C",4);Tile(t,"Roof antenna",433,518,3,112,"6C607F",1);Tile(t,"Antenna arm",414,535,39,3,"6C607F",1);}
            if(a==2){for(int n=0;n<6;n++)Shape(t,"Quiet jungle leaf",n<3?-32:433,469+n%3*50,74,118,n%2==0?"31584D":"365D58",0,true);}
            if(a==3){for(int n=0;n<10;n++)Tile(t,"Edge star",n<5?14+n*8:438+n%5*7,331+n%5*52,3,3,"736888",1);Shape(t,"Distant planet",399,540,89,59,"4E416E",0,true);}
            if(a==4){for(int n=0;n<7;n++){Tile(t,"Wallpaper mark",20+n*72,568,4,14,"574A70",2);Tile(t,"Wallpaper partner",29+n*72,563,4,19,"574A70",2);}Tile(t,"Low shelf",24,651,432,9,"6B5875",4);}
            if(a==5){Tile(t,"Counter",0,645,480,19,"7B6370",4);Tile(t,"Side shelf",8,540,63,7,"6B5875",3);Tile(t,"Ceramic jar",19,503,22,37,"84758C",7);}
            if(a==6){Shape(t,"Snow bank",-21,632,524,43,"78859D",0,true);for(int n=0;n<4;n++){Tile(t,"Tree trunk",21+n%2*429,480+n/2*30,6,156,"57617F",2);Shape(t,"Tree canopy",-8+n%2*429,456+n/2*56,71,92,"424C70",0,true);}}
            if(a==7){Tile(t,"Night sea",0,578,480,88,"354C66",0);Tile(t,"Dusk sand",0,651,480,16,"8E7C85",0);for(int n=0;n<3;n++)Tile(t,"Low wave",20+n*172,615+n%2*14,96,3,"698A9A",2);}
        }
        static void DrawCatalogThumbnail(Transform parent,CapyItem i)
        {
            Tile(parent,"Thumbnail well",7,12,52,59,"29243C",12);
            var t=Rect(parent,"Item illustration",8,24,232,220);t.localScale=Vector3.one*.21f;
            if(i.slot==ItemSlot.Head || i.slot==ItemSlot.Skin || i.slot==ItemSlot.Emotion){
                string coat=i.slot==ItemSlot.Skin?i.color:"C98A52";
                Tile(t,"Mini ear L",30,5,34,42,coat,14);Tile(t,"Mini ear R",172,5,34,42,coat,14);
                Tile(t,"Mini head",15,29,210,130,coat,i.id=="pixel" || i.id=="robot"?12:36);
                Tile(t,"Mini muzzle",154,57,64,84,"F5C998",24);
                Tile(t,"Mini eye L",73,77,24,8,"41313A",4);Tile(t,"Mini eye R",148,70,12,7,"41313A",4);
                Tile(t,"Mini mouth",122,121,22,i.slot==ItemSlot.Emotion && i.art!=4?13:4,"725144",5);
                if(i.slot==ItemSlot.Head)DrawHead(t,i);
                if(i.slot==ItemSlot.Skin)DrawSignature(t,i,true);
                if(i.slot==ItemSlot.Emotion && i.art==4){var brow=Tile(t,"Raised brow",53,63,24,4,"725144",2);brow.transform.localEulerAngles=new Vector3(0,0,18);}
            }else if(i.slot==ItemSlot.Body){t.localScale=Vector3.one*.34f;t.anchoredPosition=new Vector2(-8,25);DrawBody(t,i);}
            else if(i.slot==ItemSlot.Scene){t.localScale=Vector3.one*.102f;t.anchoredPosition=new Vector2(9,13);DrawScene(t,i);if(i.art==0){Tile(t,"Night",0,324,480,344,i.color,0);Shape(t,"Moon",320,350,94,94,"F1D6A0",0,true);Shape(t,"Pond",30,580,420,56,"619397",0,true);}}
            else {
                t.localScale=Vector3.one;t.anchoredPosition=new Vector2(9,-21);
                if(i.art==0)for(int n=0;n<4;n++)Tile(t,"Rainbow ribbon",3+n*8,14-n*3,7,21,BurstPalette[n],3);
                else if(i.art==3)for(int n=0;n<5;n++)Tile(t,"Pixel",4+n*7,20-n%3*8,5,5,i.color,0);
                else if(i.art==4)for(int n=0;n<3;n++)Shape(t,"Flame",5+n*10,8+n%2*6,12,28,i.color,0,true);
                else Mark(t,i.art==2?"~":i.art==5?"6 7":"+",0,0,45,39,25,i.color);
            }
        }
    }
}
