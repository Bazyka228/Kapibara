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
            int sil=skin.id=="pixel" || skin.id=="robot"?2:progress.silhouette;
            head.radius=outline.radius=sil==2?13:sil==1?59:34;head.SetVerticesDirty();outline.SetVerticesDirty();
            var muzzle=capy.Find("Muzzle").GetComponent<SoftShape>();muzzle.rectTransform.sizeDelta=new Vector2(sil==1?116:140,52);muzzle.rectTransform.anchoredPosition=new Vector2(sil==1?66:54,-96);muzzle.radius=sil==2?10:24;muzzle.SetVerticesDirty();
            capy.Find("Body").localScale=new Vector3(sil==1?1.08f:skin.id=="sleepy"?1.04f:.96f,skin.id=="sleepy"?.91f:1,1);
            ClearLayer(headLayer);ClearLayer(bodyLayer);ClearLayer(skinLayer);ClearLayer(sceneLayer);
            DrawHead(headLayer,CapyCatalog.Find(chosen[0]),progress.goldenCrown);
            DrawBody(bodyLayer,CapyCatalog.Find(chosen[1]));
            DrawSignature(skinLayer,skin,chosen[0]=="");
            DrawScene(sceneLayer,CapyCatalog.Find(chosen[2]));
            foreach(var old in cosmetics)if(old)old.SetActive(false);
        }
        static void DrawHead(Transform t,CapyItem i,bool gold=false)
        {
            if(i==null)return;string c=i.id=="crown" && gold?"F3D183":i.color;
            switch(i.art){
                case 0:Tile(t,"Panama crown",55,-7,137,38,c,14);Tile(t,"Broad brim",35,24,174,14,"6BAC99");Tile(t,"Hat ribbon",62,13,120,6,"DFF5D5",3);Flower(t,123,7,6);break;
                case 1:Tile(t,"Cap dome",63,-4,122,37,c,18);Tile(t,"Cap visor",135,22,80,12,"B76C66",6);Mark(t,"7",106,0,36,25,22,"FFF0DC");break;
                case 2:Tile(t,"Crown band",55,17,141,14,c,5);for(int n=0;n<5;n++){var leaf=Tile(t,"Crown leaf",61+n*26,-9+n%2*7,20,32,c,10);leaf.transform.localEulerAngles=new Vector3(0,0,18-n*9);}Tile(t,"Crown gem",116,13,14,14,"DEF3D0",5);break;
                case 3:Shape(t,"Duck body",85,2,83,31,c,0,true);Shape(t,"Duck head",83,-19,37,38,c,0,true);Tile(t,"Duck bill",67,-4,22,10,"F4A05C",4);Tile(t,"Duck eye",92,-10,5,5,"493A3B",3);Shape(t,"Wing",121,9,28,12,"E6B85B",0,true);break;
                case 4:Tile(t,"Detective crown",70,-16,109,46,c,12);Tile(t,"Detective band",69,10,110,10,"715665",3);Tile(t,"Detective brim",37,22,170,13,c,6);break;
                case 5:Tile(t,"Headphone arch",39,9,171,14,c,7);Tile(t,"Left phone",26,23,26,53,c,10);Tile(t,"Right phone",195,23,26,53,c,10);Tile(t,"Phone cushion L",34,30,16,37,"4B435F");Tile(t,"Phone cushion R",195,30,16,37,"4B435F");break;
                case 6:Tile(t,"Fur cap",49,-12,153,46,c,18);Tile(t,"Left flap",40,21,30,56,c,10);Tile(t,"Right flap",180,21,30,56,c,10);Tile(t,"Fluffy brim",47,13,157,22,"EFE3D4",10);break;
                case 7:for(int n=0;n<7;n++){Tile(t,"Vine",43+n*24,22,24,8,"81B598",3);Flower(t,49+n*24,13+n%2*5,7);}break;
                case 8:Tile(t,"Hard hat",55,-9,143,43,c,18);Tile(t,"Hard brim",42,23,169,13,c,5);Tile(t,"Ridge",118,-13,13,43,"FFE5A0",4);break;
                case 9:Tile(t,"Chef base",76,-1,100,32,c,7);for(int n=0;n<3;n++)Shape(t,"Chef puff",60+n*37,-34,61,46,c,0,true);Tile(t,"Chef crease",112,-5,4,29,"DBD6CE",2);break;
                case 10:var a=Tile(t,"Boat left",76,-5,62,41,c,0);a.transform.localEulerAngles=new Vector3(0,0,25);var b=Tile(t,"Boat right",127,-5,62,41,"E6F1F2",0);b.transform.localEulerAngles=new Vector3(0,0,-25);Tile(t,"Boat rim",58,20,137,18,c,2);break;
                case 11:for(int n=0;n<2;n++){Tile(t,"Glasses rim",43+n*112,74,49,32,c,12);Tile(t,"Lens",47+n*112,78,41,24,"40516C",9);Tile(t,"Reflection",53+n*112,81,20,4,"8CC8D0",2);}Tile(t,"Bridge",91,81,65,5,c,2);break;
            }
        }
        static void DrawBody(Transform t,CapyItem i)
        {
            if(i==null)return;string c=i.color;
            if(i.art==0){Tile(t,"Scarf collar",64,153,124,19,c);Tile(t,"Scarf tail",154,167,25,42,"D17F6C");Tile(t,"Knot",145,158,24,23,c);return;}
            if(i.art==1){Tile(t,"Bow left",97,155,27,20,c,5);Tile(t,"Bow right",126,155,27,20,c,5);Tile(t,"Bow knot",121,156,10,17,"F1C3D1",3);return;}
            Tile(t,"Clothing fabric",55,137,139,70,c,i.art==8?7:24);
            if(i.art==2){Tile(t,"Apron pocket",94,178,66,20,"AAC59A",6);Tile(t,"Apron strap",78,137,9,43,"F3EDD3",4);}
            if(i.art==3){Tile(t,"Cape clasp",116,152,21,18,"F5D78E",7);Mark(t,"7",104,174,45,27,23,"E2F1D7");}
            if(i.art==4)for(int n=0;n<3;n++)Tile(t,"Sweater stripe",64,162+n*14,120,5,"EEE2D4",2);
            if(i.art==5)Mark(t,"6–7",74,161,106,42,30,"655242");
            if(i.art==6){Tile(t,"Hoodie pocket",83,177,85,22,"647998",7);for(int n=0;n<2;n++)Tile(t,"Cord",108+n*32,153,3,20,"EEE1CC",1);}
            if(i.art==7){Tile(t,"Rain seam",123,143,4,58,"C0A54F",2);for(int n=0;n<3;n++)Tile(t,"Button",132,154+n*16,5,5,"F8F1CB",3);}
            if(i.art==8){for(int n=0;n<3;n++)Tile(t,"Cardboard plate",62,153+n*17,122,13,n%2==0?"DBBE97":"B98E68",3);Mark(t,"+",100,163,52,33,28,"ECE0BB");}
            if(i.art==9){Shape(t,"Orange emblem",110,167,31,28,"F5AC57",0,true);Tile(t,"Leaf emblem",129,163,13,6,"4D8E6B",3);}
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
            if(i==null || i.art==0)return;
            // Scene overlays preserve a dark, quiet flight lane and fixed landing positions.
            Tile(t,"Scenery field",0,324,480,344,i.color,0);
            Tile(t,"Quiet flight lane",59,330,360,132,i.color,42);
            int a=i.art;
            if(a==1){Shape(t,"Setting sun",351,339,70,70,"DDA982",0,true);Shape(t,"Sunset hill",-50,532,350,150,"655166",0,true);}
            if(a==2){for(int n=0;n<6;n++){Shape(t,"Jungle leaf",n<3?-25:420,354+n%3*77,76,127,n%2==0?"40745B":"538765",0,true);}}
            if(a==3){for(int n=0;n<16;n++)Tile(t,"Space star",22+n*71%430,334+n*59%290,3,3,"C0B7DD",2);Shape(t,"Planet",352,350,60,60,"8274AC",0,true);Tile(t,"Planet ring",338,374,89,7,"B3A0C4",3);}
            if(a==4){for(int n=0;n<5;n++)Tile(t,"Wallpaper stripe",n*108,324,4,313,"685B77",1);Tile(t,"Shelf",21,572,439,12,"8A748C",4);}
            if(a==5){for(int n=0;n<4;n++){Tile(t,"Kitchen tile",24+n*117,544,103,60,"92838C",4);Shape(t,"Fruit bowl",32+n*117,578,80,29,"C5AE8F",0,true);Shape(t,"Bowl orange",53+n*117,567,23,21,"F5B05E",0,true);}}
            if(a==6){Shape(t,"Snow bank",-21,609,524,50,"DFE8EC",0,true);for(int n=0;n<12;n++)Tile(t,"Snowflake",25+n*83%432,347+n*41%229,4,4,"DCE8ED",2);}
            if(a==7){Tile(t,"Sea horizon",0,525,480,106,"679BA5",0);Tile(t,"Sand",0,622,480,44,"D4BC96",0);for(int n=0;n<4;n++)Tile(t,"Wave",24+n*117,569+n%2*23,80,3,"ACD7CF",2);}
        }
        static void DrawCatalogThumbnail(Transform parent,CapyItem i)
        {
            Tile(parent,"Thumbnail well",7,12,52,59,"29243C",12);
            var t=Rect(parent,"Item illustration",8,24,232,220);t.localScale=Vector3.one*.21f;
            if(i.slot==ItemSlot.Head || i.slot==ItemSlot.Skin || i.slot==ItemSlot.Emotion){
                string coat=i.slot==ItemSlot.Skin?i.color:"C98A52";
                Tile(t,"Mini ear L",30,5,34,42,coat,14);Tile(t,"Mini ear R",172,5,34,42,coat,14);
                Tile(t,"Mini head",15,29,210,130,coat,i.id=="pixel" || i.id=="robot"?12:36);
                Tile(t,"Mini muzzle",67,97,120,50,"EBC496",20);
                Tile(t,"Mini eye L",55,80,17,9,"41313A",4);Tile(t,"Mini eye R",166,80,17,9,"41313A",4);
                Tile(t,"Mini mouth",114,130,22,i.slot==ItemSlot.Emotion && i.art!=4?13:4,"725144",5);
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
                else Mark(t,i.art==2?"♥":i.art==5?"6 7":"+",0,0,45,39,25,i.color);
            }
        }
    }
}
