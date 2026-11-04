using UnityEngine;
namespace Capybara67.Idle { public sealed partial class CapyIdleGame {
    static void DrawHead(Transform t,IdleItem i,bool gold=false){
        if(i==null)return;string c=i.color;
        switch(i.art){
            case 0:
                InkBox(t,"Linen panama",57,14,133,38,c,16,3);InkBox(t,"Wide brim",34,43,182,14,"D6DB97",7,3);
                Shape(t,"Hat band",61,33,125,8,"6F9471",3);Leaf(t,153,24,23,16,"FFF0B5",-12);
                for(int n=0;n<6;n++)Shape(t,"Hat stitching",72+n*17,22,7,2,"F9F0CF",1);break;
            case 1:
                Contour(t,"Cap edge",46,10,148,45,Ink,Rounded(0,150,12,45,92,0,182,6,240,72,240,150));
                Contour(t,"Cap fabric",50,14,140,37,c,Rounded(0,150,12,45,92,0,182,6,240,72,240,150));
                InkBox(t,"Cap brim",124,43,94,13,"C36D57",6,3);Stroke(t,"Cap seam",118,14,118,44,"FCD3A7",2);Oval(t,"Cap button",112,6,13,10,"EDB285",2);break;
            case 2:
                var crown=new[]{new Vector2(0,140),new Vector2(6,30),new Vector2(65,71),new Vector2(117,0),new Vector2(170,71),new Vector2(231,30),new Vector2(240,140)};
                Contour(t,"Crown outline",67,-2,114,57,Ink,crown);Contour(t,"Golden crown",71,3,106,48,"F6CA69",crown);
                InkBox(t,"Crown band",67,44,114,12,"E3A744",5,2);Oval(t,"Crown gem",115,30,16,17,"83B88B",2);Shape(t,"Crown shine",83,34,3,9,"FFF1B3",1);break;
            case 3:
                Oval(t,"Duck body",94,20,72,35,"F4D273",3);Oval(t,"Duck head",138,1,36,34,"FBE192",3);
                Shape(t,"Duck eye",160,11,5,5,Ink,0,true);InkBox(t,"Duck beak",168,20,22,11,"E79C54",5,2);Shape(t,"Duck wing",108,30,28,12,"E6B952",0,true);break;
            case 4:
                InkBox(t,"Explorer hat",64,0,123,48,c,18,3);Shape(t,"Hat crease",86,7,69,7,"AE815C",3);
                Shape(t,"Leather hat band",68,34,115,9,"885F41",2);InkBox(t,"Explorer brim",31,43,190,14,"D6B480",7,3);InkBox(t,"Hat buckle",130,33,17,13,"F7D484",3,2);break;
            case 5:
                var arc=new[]{new Vector2(0,150),new Vector2(8,49),new Vector2(45,10),new Vector2(120,0),new Vector2(195,10),new Vector2(232,49),new Vector2(240,150),new Vector2(222,150),new Vector2(213,55),new Vector2(184,32),new Vector2(120,23),new Vector2(55,32),new Vector2(27,55),new Vector2(18,150)};
                Contour(t,"Headphone arc",29,12,187,65,Ink,arc);Contour(t,"Headphone padding",33,16,179,61,"93A9C8",arc);
                for(int n=0;n<2;n++){InkBox(t,"Ear pad",24+n*176,47,33,56,"587E84",15,3);InkBox(t,"Ear cushion",30+n*176,54,20,41,c,9,2);Shape(t,"Pad glint",35+n*176,61,4,18,"E2EFE1",2);}break;
            case 6:
                InkBox(t,"Winter cap",47,7,153,48,c,22,3);InkBox(t,"Fur brim",43,34,162,24,"FFF1D8",10,3);
                for(int n=0;n<2;n++)InkBox(t,"Warm ear flap",38+n*153,49,27,36,"E1D8C4",13,3);
                Oval(t,"Pompom",106,-10,37,30,"FFF2D6",3);for(int n=0;n<6;n++)Stroke(t,"Knitted stitch",64+n*23,19,70+n*23,27,"EFF0D9",2);break;
            case 7:
                Shape(t,"Wreath stem",35,42,174,7,"6C8B4D",3);
                for(int n=0;n<5;n++){Leaf(t,39+n*33,36-n%2*8,29,17,"91B96B",n%2==0?15:-10);}
                for(int n=0;n<5;n++)Oval(t,"Flower petal",108+Mathf.Cos(n*1.256f)*11,24+Mathf.Sin(n*1.256f)*11,17,17,"F4C0A5",2);
                Oval(t,"Flower centre",109,25,14,14,"F3CC6A",2);break;
            case 8:
                var dome=Rounded(0,150,8,65,54,10,120,0,192,10,235,65,240,150);
                Contour(t,"Helmet outline",49,4,148,44,Ink,dome);Contour(t,"Safety helmet",53,8,140,38,"F3C366",dome);
                InkBox(t,"Helmet brim",34,43,177,15,"DBA644",7,3);InkBox(t,"Helmet ridge",115,1,17,43,"FFDE87",7,2);Star(t,80,19,23,"FFF0AE");break;
            case 9:
                for(int n=0;n<3;n++)Oval(t,"Chef cloud",57+n*39,-7-n%2*9,61,48,"FFF5DC",3);
                InkBox(t,"Chef band",63,22,129,34,"F6E9C9",8,3);Stroke(t,"Chef fold",94,23,94,43,"C8B38B",2);Stroke(t,"Chef fold",153,23,153,43,"C8B38B",2);Shape(t,"Chef band seam",69,47,117,3,"D3BD93",1);break;
            case 10:
                var paper=new[]{new Vector2(0,150),new Vector2(123,0),new Vector2(240,150)};
                Contour(t,"Paper triangle edge",43,-1,163,54,Ink,paper);Contour(t,"Paper triangle",48,4,153,45,"FFF1CB",paper);
                Stroke(t,"Fold",125,9,124,47,"BCA77D",2);InkBox(t,"Paper boat rim",39,42,173,17,"A8C8C1",5,3);Stroke(t,"Fold highlight",49,48,197,48,"EDF2CF",2);break;
            case 11:
                InkBox(t,"Round glasses",59,81,55,40,"A2CBB5",17,4);InkBox(t,"Round glasses",131,73,48,37,"A2CBB5",16,4);
                Stroke(t,"Bridge",110,92,134,87,Ink,4);Stroke(t,"Glasses arm",60,87,34,79,Ink,4);
                Stroke(t,"Lens shine",71,89,80,84,"F2F4D1",3);Stroke(t,"Lens shine",143,82,151,78,"F2F4D1",3);break;
        }
    }
    static void DrawBody(Transform t,IdleItem i){
        if(i==null)return;string c=i.color;
        switch(i.art){
            case 0:
                InkBox(t,"Scarf tail",139,163,29,64,c,10,3);InkBox(t,"Scarf wrap",59,156,127,24,c,12,3);
                for(int n=0;n<4;n++)Shape(t,"Scarf stripe",72+n*28,161,11,13,"F8DDB2",2);
                for(int n=0;n<3;n++)Stroke(t,"Scarf fringe",146+n*8,219,146+n*8,230,Ink,2);break;
            case 1:
                Contour(t,"Bow left",79,165,43,32,Ink,Rounded(0,0,240,65,240,92,0,150));Contour(t,"Bow left silk",83,169,35,24,c,Rounded(0,0,240,65,240,92,0,150));
                Contour(t,"Bow right",120,165,43,32,Ink,Rounded(240,0,0,65,0,92,240,150));Contour(t,"Bow right silk",124,169,35,24,c,Rounded(240,0,0,65,0,92,240,150));Oval(t,"Bow knot",112,174,20,19,"F4BAA4",3);break;
            case 2:
                InkBox(t,"Apron bib",76,161,100,71,"F4E5B8",16,3);for(int n=0;n<2;n++)InkBox(t,"Apron strap",82+n*71,147,11,37,"B3C995",4,2);
                InkBox(t,"Apron pocket",101,196,47,26,c,8,2);Leaf(t,112,201,25,15,"83A35D");Stroke(t,"Apron stitching",85,224,167,224,"C6B48E",2);break;
            case 3:
                Contour(t,"Cape border",39,147,169,85,Ink,Rounded(55,0,185,0,240,150,167,140,122,150,71,140,0,150));
                Contour(t,"Cape cloth",43,151,161,77,c,Rounded(55,0,185,0,240,150,167,140,122,150,71,140,0,150));
                Stroke(t,"Cape fold",81,163,64,217,"4F8262",3);Stroke(t,"Cape fold",168,163,187,217,"4F8262",3);Oval(t,"Cape clasp",110,159,24,19,"F9D47C",3);break;
            case 4:
                InkBox(t,"Knitted sweater",55,153,139,78,c,23,3);InkBox(t,"Sweater hem",68,220,114,12,"998BAE",5,2);
                for(int n=0;n<6;n++){Stroke(t,"Knit V",77+n*17,196,82+n*17,201,"E3CFE6",2);Stroke(t,"Knit V",82+n*17,201,87+n*17,196,"E3CFE6",2);}Star(t,110,171,25,"F5D68E");break;
            case 5:
                InkBox(t,"Sport shirt",58,153,134,78,c,22,3);Shape(t,"Team stripe",64,178,121,10,"FFF0C3",3);Mark(t,"6–7",86,185,81,34,26,Ink);break;
            case 6:
                InkBox(t,"Backpack",29,160,45,68,c,16,3);InkBox(t,"Side pocket",29,187,31,27,"C4B2D3",8,2);
                for(int n=0;n<2;n++){InkBox(t,"Shoulder strap",75+n*84,156,13,65,"D4B897",5,2);InkBox(t,"Buckle",76+n*84,193,11,13,"F4D783",3,2);}break;
            case 7:
                InkBox(t,"Raincoat",54,153,142,81,c,23,3);Stroke(t,"Raincoat zip",125,164,125,229,"B98B42",3);
                for(int n=0;n<3;n++)Oval(t,"Raincoat snap",135,172+n*16,7,7,"FFF3BC",1.5f);
                InkBox(t,"Rain pocket",78,200,25,19,"E7AE52",6,2);Leaf(t,147,201,24,14,"FFF0B9");break;
            case 8:
                InkBox(t,"Tool belt",57,200,139,15,"A4774D",5,3);InkBox(t,"Belt buckle",114,198,23,21,"F3D080",5,3);
                for(int n=0;n<2;n++)InkBox(t,"Tool pouch",72+n*74,204,30,26,c,8,2);InkBox(t,"Trowel handle",155,181,7,29,"D7A76F",3,2);Oval(t,"Trowel",150,172,18,20,"9EB5AD",3);break;
            case 9:
                InkBox(t,"Orange tee",58,154,133,77,c,22,3);Oval(t,"Orange emblem",104,183,37,31,"EFA951",2);Leaf(t,122,179,20,11,"7AA464");Stroke(t,"Sleeve seam",68,174,81,180,"719D8A",2);break;
        }
    }
    static void DrawSignature(Transform t,IdleItem s,bool bareHead){switch(s.art){
        case 0:if(bareHead){Oval(t,"Little mandarin",108,25,35,30,"EFA34C",3);Leaf(t,126,18,23,14,"86AD61");}break;
        case 1:if(bareHead){InkBox(t,"Sports band",37,57,154,12,"F7ECC9",6,2);Stroke(t,"Sport tie",36,63,22,82,"DF937A",6);}break;
        case 2:if(bareHead){InkBox(t,"Sleep cap",61,27,113,25,"B3B7C6",12,3);Oval(t,"Sleep cap pompom",171,40,21,21,"F9EDD1",2);}break;
        case 3:for(int n=0;n<2;n++)Leaf(t,27+n*177,127,23,15,"A1DFBC",n==0?20:-20);break;
        case 4:for(int n=0;n<4;n++)InkBox(t,"Pixel patch",30+n%2*10,123+n/2*10,9,9,"F2D28C",1,1);break;
        case 5:for(int n=0;n<3;n++)Stroke(t,"Sun fleck",40+n*10,77,43+n*10,70,"FFE39C",3);break;
        case 6:Star(t,33,112,23,"FFF0B3");Star(t,213,73,17,"FFF0B3");if(bareHead)Oval(t,"Orbit moon",108,21,32,28,"E5DFAB",3);break;
        case 7:for(int n=0;n<5;n++)Shape(t,"Fluffy cheek",24+n*39,153+n%2*8,32,22,"F9ECD2",0,true);break;
        case 8:InkBox(t,"Robot panel",102,184,47,30,"6B9196",8,2);for(int n=0;n<3;n++)Shape(t,"Robot meter",110+n*11,194,6,10,"D5E9A7",2);Oval(t,"Rivet",28,109,8,8,"E1E6CE",2);break;
        case 9:Star(t,33,111,22,"FFE9A2");Shape(t,"Golden glow",55,60,17,8,"FFE6A0",0,true);break;
    }}
}}
