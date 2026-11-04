using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  // Shared 480 x 309 stage, also used by shop previews. Keep the centre quiet for the capybara.
  static void DrawScene(Transform parent,IdleItem item,bool fullHeight=false){
   int scene=Mathf.Clamp(item?.art??0,0,14);
   var viewport=Rect(parent,"Scenery viewport",0,fullHeight?0:375,480,fullHeight?850:309);
   viewport.gameObject.AddComponent<RectMask2D>();
   string[] top={"A9DAD0","E5A990","6BAE95","243957","E4C8A0","F1D6AF","B2D4DC","9DDCD8","274B60","C2DBBF","9AC4BB","696980","B6D6BE","C6AF8D","AECFDB"};
   string[] bottom={"F6EDBD","FFE8B7","D5E3B0","869FA5","FFF0C8","FFF0CC","F3F2DE","FFF1C8","B9D9AC","F3E8BD","E0EDD0","DECEB6","F2ECC2","ECDABD","FFF0C5"};
   if(fullHeight){
    Shape(viewport,"Sky continuation",0,0,480,225,top[scene],0);
    string[] ground={"CCD99E","DCB98A","A8C78D","7E9C92","DDBD91","DEC19A","E8EFDC","E9D4A1","BDD49B","DCC297","A1BE9D","A7B69C","BDCF93","D0B18A","DDD8B1"};
    Shape(viewport,"Ground continuation",0,534,480,316,ground[scene],0);
    for(int n=0;n<9;n++){
     float y=548+n*36;
     if(scene==1||scene==4||scene==5){Stroke(viewport,"Extended floor seam",0,y,480,y,"AB85584A",2);Stroke(viewport,"Floor joint",n%2==0?98:311,y,n%2==0?98:311,y+36,"AB855840",2);}
     else {Rock(viewport,4+n%2*461,y,11,scene==6?"F7F5E7":"DBD9AB");if(scene!=6&&scene!=7)Tuft(viewport,8+n%2*462,y+21,"91AD6C");}
    }
    if(scene==3||scene==8)for(int n=0;n<16;n++)Shape(viewport,"Upper garden star",9+(n*61)%462,8+(n*43)%207,3,3,"F6E6BB",0,true);
    else if(scene==4||scene==5)for(int n=0;n<9;n++)Stroke(viewport,"Extended wall seam",n*60,0,n*60,225,"BD9A702A",2);
    else {Cloud(viewport,13,43,.8f);Cloud(viewport,314,156,.7f);}
   }
   var t=Rect(viewport,"Detailed scenery "+scene,0,fullHeight?225:0,480,309);
   var sky=Rect(t,"Atmosphere",0,0,480,309).gameObject.AddComponent<CapySceneryGradient>();sky.top=C(top[scene]);sky.bottom=C(bottom[scene]);sky.raycastTarget=false;
   switch(scene){
    case 0:PondGarden(t);break;case 1:SunsetTerrace(t);break;case 2:JungleGarden(t);break;case 3:StarGarden(t);break;
    case 4:CozyVeranda(t);break;case 5:TeaHouse(t);break;case 6:WinterGarden(t);break;case 7:CoralBeach(t);break;case 8:FireflySanctuary(t);break;default:TreeScene(t,scene);break;
   }
  }
  static void FireflySanctuary(Transform t){
   SunDisc(t,355,18,"FFF1BD");Hills(t,"497D7D","76A68C");
   Shape(t,"Moss meadow",0,210,480,99,"BDD49B",0);Pond(t);
   for(int side=0;side<2;side++){
    float x=side==0?29:417;InkBox(t,"Garden arch pillar",x,72,24,155,"698E80",7,2);
    Stroke(t,"Pillar vine",x+6,210,x+14,75,"456B53",4);
    for(int n=0;n<5;n++){Leaf(t,x-9,85+n*25,27,17,n%2==0?"8CB985":"B4CB8C",side==0?12:-20);Flower(t,x+14,94+n*24,n%2==0?"E7B8D0":"F0DEC0",.75f);}
   }
   var arc=Rounded(0,145,23,71,75,17,120,0,175,18,222,75,240,145,220,145,197,82,163,41,120,25,80,40,44,83,20,145);
   Contour(t,"Flower arch",34,19,403,123,"587C66",arc);
   for(int n=0;n<11;n++){float a=Mathf.PI*(.08f+.084f*n);float x=239-Mathf.Cos(a)*184,y=132-Mathf.Sin(a)*101;Leaf(t,x,y,29,17,"96BC87",-n*9);Flower(t,x+4,y+1,n%3==0?"EAAFC9":"F7E6BC",.7f);}
   for(int n=0;n<9;n++){Rock(t,25+n*53,270+n%2*15,23,"D4D9B3");Flower(t,10+n*57,249+n%2*14,n%2==0?"D9B7D5":"F5DDA5",.8f);}
   Lantern(t,36,199);Lantern(t,413,201);
   for(int n=0;n<12;n++){
    float x=18+(n*73)%440,y=47+(n*37)%208;var glow=Rect(t,"Sanctuary firefly "+n,x,y,20,20);glow.localScale=Vector3.one*.45f;
    EffectMotif(glow,4,"F5D67F");AnimateDetail(glow,0,n*.7f,13);
   }
  }
  static void Cloud(Transform p,float x,float y,float scale,string color="FFF8DE"){
   var t=Rect(p,"Cloud cluster",x,y,100,40);t.localScale=Vector3.one*scale;
   Shape(t,"Cloud",0,14,100,21,color,0,true);Shape(t,"Cloud",18,2,45,29,color,0,true);Shape(t,"Cloud",48,8,32,26,color,0,true);
  }
  static void SunDisc(Transform p,float x,float y,string color){
   Shape(p,"Sun halo",x-10,y-10,82,82,color+"30",0,true);Shape(p,"Sun",x,y,62,62,color,0,true);Shape(p,"Sun light",x+12,y+9,25,8,"FFF7D580",0,true);
  }
  static void Hills(Transform t,string rear,string front){
   Shape(t,"Far hill",-70,90,340,174,rear,0,true);Shape(t,"Far hill",200,65,350,191,rear,0,true);
   Shape(t,"Near hill",-100,138,380,155,front,0,true);Shape(t,"Near hill",235,131,350,154,front,0,true);
  }
  static void Flower(Transform t,float x,float y,string color,float s=1){
   var p=Rect(t,"Meadow flower",x,y,20,28);p.localScale=Vector3.one*s;Stroke(p,"Stem",10,9,9,28,"729256",2);
   for(int n=0;n<5;n++){float a=n*Mathf.PI*.4f;Shape(p,"Petal",6+Mathf.Cos(a)*6,5+Mathf.Sin(a)*6,8,8,color,0,true);}Shape(p,"Pollen",7,6,6,6,"E4AD53",0,true);
  }
  static void Rock(Transform p,float x,float y,float w,string c){
   Shape(p,"Pebble shadow",x-2,y+7,w+5,w*.32f,"52644D25",0,true);Shape(p,"Smooth pebble",x,y,w,w*.48f,c,0,true);Shape(p,"Pebble light",x+w*.22f,y+2,w*.47f,3,"FFF6DD70",2);
  }
  static void Tuft(Transform p,float x,float y,string c){
   Stroke(p,"Grass",x,y,x-5,y-12,c,2);Stroke(p,"Grass",x+2,y,x+4,y-16,c,2);Stroke(p,"Grass",x+5,y,x+12,y-9,c,2);
  }
  static void Lily(Transform p,float x,float y,float size){
   Shape(p,"Lily reflection",x-2,y+4,size+7,size*.4f,"467E6E40",0,true);Shape(p,"Lily pad",x,y,size,size*.45f,"80B47D",0,true);Stroke(p,"Pad fold",x+size*.5f,y+size*.22f,x+size*.9f,y+1,"B6D397",1.5f);
  }
  static void Pond(Transform t,bool winter=false){
   Shape(t,"Pond shore",37,211,409,77,winter?"ADC6C9":"7BAA85",0,true);Shape(t,"Pond water",46,216,390,64,winter?"B6D9DE":"80C4B5",0,true);
   for(int n=0;n<6;n++)Shape(t,"Water highlight",57+n*63,229+n%3*10,34+n%2*11,2,winter?"F2F9ED":"D9F1CD",1);
   Shape(t,"Resting island edge",96,249,289,44,winter?"BCD3CD":"A6B67D",0,true);Shape(t,"Resting island",101,247,279,39,winter?"F6F5E5":"E6DEAB",0,true);
  }
  static void Pot(Transform p,float x,float y,string color,bool flowers=false){
   var t=Rect(p,"Garden planter",x,y,62,90);
   Shape(t,"Pot shadow",-3,71,67,13,"65432D25",0,true);
   Contour(t,"Terracotta edge",4,40,51,37,"795638",Rounded(0,0,240,0,210,150,40,150));
   Contour(t,"Glazed pot",7,43,45,31,color,Rounded(0,0,240,0,210,150,40,150));
   InkBox(t,"Pot lip",0,35,59,12,color,5,2);Stroke(t,"Pot highlight",13,49,17,66,"FFF4D080",3);
   Leaf(t,25,0,23,42,"779A5F",18);Leaf(t,6,9,28,29,"97B976",-18);Leaf(t,32,11,24,26,"A9C483",30);
   if(flowers){Flower(t,3,-3,"F3B8A0",.8f);Flower(t,29,-10,"FFE2A0",.9f);}
  }
  static void Lantern(Transform t,float x,float y){
   Stroke(t,"Lantern cord",x+16,y-26,x+16,y,"806648",2);Shape(t,"Lamplight",x-18,y-14,70,75,"FFE4A328",0,true);
   InkBox(t,"Lantern frame",x,y,32,43,"C29863",8,2);InkBox(t,"Lantern glass",x+5,y+7,22,29,"FFE3A1",5,1);
   Shape(t,"Flame",x+13,y+17,7,13,"FFF8D8",0,true);Stroke(t,"Lantern rail",x+16,y+6,x+16,y+37,"C09152",2);
  }
  static void Planks(Transform t,float y,string fill){
   Shape(t,"Timber floor",0,y,480,309-y,fill,0);
   for(int n=0;n<4;n++){float yy=y+9+n*23;Stroke(t,"Board seam",0,yy,480,yy,"B7855540",1.5f);for(int k=0;k<3;k++)Stroke(t,"Wood grain",22+k*176+n%2*30,yy+8,52+k*176+n%2*30,yy+9,"9F734330",1.3f);}
   for(int n=0;n<5;n++)Stroke(t,"Plank join",n*113, y+(n%3)*23,n*113,y+(n%3+1)*23,"A97E5350",1.5f);
   Shape(t,"Woven rug edge",90,257,300,41,"A68A5F",0,true);Shape(t,"Woven rug",95,258,290,35,"DBC48B",0,true);
   for(int n=0;n<6;n++)Shape(t,"Rug weave",128+n*39,284,20,2,"F3DCA6",1);
  }
  static void PondGarden(Transform t){
   Cloud(t,48,28,.95f);Cloud(t,257,62,.55f);SunDisc(t,355,25,"F8D781");Hills(t,"B1D3A1","95BD87");
   for(int n=0;n<7;n++){float x=n*81-12;Shape(t,"Far shrubs",x,179+n%2*9,86,45,n%2==0?"85AD75":"A4C78B",0,true);}
   Shape(t,"Lawn",0,222,480,87,"CCD99E",0);Pond(t);Lily(t,66,236,36);Lily(t,394,245,30);Flower(t,404,229,"F9CEBC",.7f);
   // Small footbridge in the distance, clear of the face and body.
   InkBox(t,"Footbridge",21,166,106,14,"C99767",6,2);for(int n=0;n<5;n++)Stroke(t,"Bridge plank",35+n*18,169,35+n*18,178,"A37550",1);
   Stroke(t,"Bridge rail",24,155,122,155,"92724C",3);Stroke(t,"Bridge post",28,151,28,182,"92724C",4);Stroke(t,"Bridge post",117,151,117,182,"92724C",4);
   for(int n=0;n<3;n++){float x=14+n*16;Stroke(t,"Reed",x,274,x+5,202-n*11,"6A8E59",2.5f);InkBox(t,"Reed head",x+1,193-n*11,8,19,"B58B51",4,1);}
   Leaf(t,425,194,42,76,"7BA25D",-8);Leaf(t,449,230,38,42,"A3BD70",25);
   for(int n=0;n<5;n++){float x=n<3?13+n*25:411+(n-3)*30;Tuft(t,x,300-n%2*6,"91AD6C");Flower(t,x+7,277-n%2*12,n%2==0?"FFF2CA":"E9AE99",.65f);}
   Rock(t,69,291,22,"AAB28C");Rock(t,399,285,27,"B7BD98");
  }
  static void SunsetTerrace(Transform t){
   SunDisc(t,327,39,"FFD092");Cloud(t,47,28,.9f,"FFE2B9");Cloud(t,237,17,.6f,"F7D1B2");Hills(t,"C6B2A0","A5AA88");
   Shape(t,"River beyond terrace",0,174,480,52,"C2CEAC",0);for(int n=0;n<5;n++)Shape(t,"Sunlit river",274-n*9,184+n*7,80+n*17,2,"FFE6AB",1);
   Planks(t,225,"DCB98A");
   for(int n=0;n<9;n++)InkBox(t,"Balustrade spindle",15+n*56,191,7,61,"BC8C60",3,1);
   InkBox(t,"Balustrade rail",0,184,480,12,"D2A371",5,2);InkBox(t,"Balustrade base",0,237,480,9,"BB8C5F",4,1);
   Stroke(t,"Overhead string",0,16,240,39,"987958",2);Stroke(t,"Overhead string",240,39,480,15,"987958",2);
   for(int n=0;n<7;n++){float x=33+n*68,y=22+Mathf.Sin(n*.52f)*16;Shape(t,"Fairy light halo",x-7,y-6,25,25,"FFE9A52A",0,true);Oval(t,"Fairy bulb",x,y,10,14,"FFE9AC",1);}
   Pot(t,16,213,"BA7D61",true);Pot(t,405,202,"95AD83");Lantern(t,419,53);Rock(t,74,285,15,"C69E76");
  }
  static void JungleGarden(Transform t){
   for(int n=0;n<7;n++){Shape(t,"Distant trunk",18+n*76,0,12,233,"659B8135",5);Shape(t,"Distant canopy",n*81-40,13+n%2*31,145,96,"7CB195",0,true);}
   Shape(t,"Soft clearing",83,44,311,203,"E2EDBD35",0,true);Hills(t,"7CA88A","6D9775");
   Shape(t,"Jungle floor",0,229,480,80,"A8C78D",0);Pond(t);
   Contour(t,"Waterfall rock",367,107,85,128,"7E9F88",Rounded(45,0,190,10,240,150,0,150));
   Shape(t,"Waterfall",397,119,28,105,"A6D8C3",12);Stroke(t,"Falling water",405,131,405,210,"DBEFCE",3);Stroke(t,"Falling water",417,128,417,214,"D0E8BD",2);
   Shape(t,"Waterfall foam",382,215,59,13,"DBEBC8",0,true);
   for(int n=0;n<4;n++){Leaf(t,-11+n*135,-6+n%2*6,73,74,n%2==0?"467D5B":"659665",-13+n*7);}
   Stroke(t,"Hanging vine",65,0,77,120,"507E51",3);Leaf(t,63,61,23,36,"80AA64",20);Leaf(t,67,93,21,31,"97B572",-30);
   Leaf(t,-15,192,63,100,"467E58",-10);Leaf(t,30,249,41,54,"88B36A",-25);Leaf(t,424,237,56,78,"537F51",14);
   Flower(t,29,258,"F0B39B",1.2f);Flower(t,433,264,"F0C88C",.8f);Rock(t,65,288,28,"85A27F");Lily(t,82,228,32);
  }
  static void StarGarden(Transform t){
   Shape(t,"Nebula",-80,18,352,114,"C4BCDF16",0,true);Shape(t,"Nebula",197,59,370,108,"9CDCD719",0,true);
   for(int n=0;n<19;n++){float x=17+(n*79)%450,y=13+(n*31)%132;Shape(t,"Distant star",x,y,n%3==0?3:2,n%3==0?3:2,"F6E6BB",0,true);}
   Stroke(t,"Constellation",34,64,77,43,"C8DCCE60",1);Stroke(t,"Constellation",77,43,109,76,"C8DCCE60",1);Stroke(t,"Constellation",109,76,63,104,"C8DCCE60",1);
   Star(t,28,58,13,"FAE8B8");Star(t,71,37,13,"FAE8B8");Star(t,103,70,13,"FAE8B8");
   Shape(t,"Moon halo",339,23,94,94,"FBE3B11C",0,true);Oval(t,"Moon",356,34,63,63,"EFDDB6",2);Shape(t,"Moon crater",368,50,16,12,"D2C6A3",0,true);Shape(t,"Moon crater",389,71,13,10,"D9C9A7",0,true);
   Hills(t,"687F92","557A80");Shape(t,"Night grass",0,235,480,74,"7E9C92",0);Pond(t);
   for(int side=0;side<2;side++)for(int n=0;n<3;n++){float x=20+side*408+n*10;Contour(t,"Moon crystal",x,235-n*9,14,42+n*7,n%2==0?"B9D8C3":"93C6BB",Rounded(120,0,240,90,190,150,40,150,0,90));Stroke(t,"Crystal facet",x+7,243-n*9,x+7,268,"E9F1D8",1);}
   for(int n=0;n<6;n++){float x=n<3?28+n*26:388+(n-3)*26;Shape(t,"Glow seed",x-5,284-n%2*15,17,17,"E5ECAE22",0,true);Shape(t,"Firefly",x,289-n%2*15,4,4,"E9EAAE",0,true);}
  }
  static void Window(Transform t,float x,float y,float w,float h){
   InkBox(t,"Window frame",x,y,w,h,"A68259",9,3);InkBox(t,"Window glass",x+6,y+6,w-12,h-12,"BDDBCC",5,2);
   Shape(t,"Window landscape",x+9,y+h*.66f,w-18,h*.24f,"9FBE91",0);Stroke(t,"Window reflection",x+w*.68f,y+13,x+w*.45f,y+h*.49f,"F2F2D660",5);
   Stroke(t,"Window mullion",x+w*.5f,y+5,x+w*.5f,y+h-5,"A68259",4);Stroke(t,"Window transom",x+5,y+h*.52f,x+w-5,y+h*.52f,"A68259",4);
   InkBox(t,"Window sill",x-6,y+h-4,w+12,10,"C89D6B",4,2);
  }
  static void CozyVeranda(Transform t){
   for(int n=0;n<9;n++)Stroke(t,"Wall boards",n*60,0,n*60,230,"BD9A702A",2);
   Window(t,27,29,126,118);Window(t,326,29,126,118);
   InkBox(t,"Curtain rod",15,19,150,7,"8E6A48",3,1);InkBox(t,"Curtain rod",314,19,150,7,"8E6A48",3,1);
   for(int side=0;side<2;side++){float x=17+side*421;Contour(t,"Linen curtain",x,27,28,113,"E7B793",Rounded(0,0,240,0,160,65,240,150,0,150));Stroke(t,"Curtain fold",x+9,32,x+8,129,"F4D0AC",2);}
   Shape(t,"Wall skirting",0,219,480,11,"AC8257",0);Planks(t,230,"DDBD91");
   InkBox(t,"Left bench",9,207,91,15,"B58B60",5,2);Stroke(t,"Bench leg",20,221,20,269,"99744C",5);Stroke(t,"Bench leg",87,221,87,269,"99744C",5);
   InkBox(t,"Bench cushion",15,194,78,16,"A4B58D",7,2);for(int n=0;n<4;n++)Shape(t,"Cushion seam",24+n*17,202,8,2,"DDE3B6",1);
   Pot(t,402,205,"B98C69",true);Lantern(t,224,35);
   InkBox(t,"Woven basket",19,269,61,28,"BB9769",9,2);for(int n=0;n<5;n++)Stroke(t,"Basket weave",25+n*10,273,25+n*10,292,"E4C797",2);
  }
  static void TeaHouse(Transform t){
   for(int n=0;n<7;n++)for(int k=0;k<3;k++)Shape(t,"Wallpaper blossom",22+n*73,23+k*61,5,5,"CFA77C35",0,true);
   Window(t,26,31,105,135);for(int n=0;n<3;n++)Stroke(t,"Paper window lattice",32,62+n*30,125,62+n*30,"A5825E",2);
   InkBox(t,"Tea cabinet",341,25,118,155,"C59B6F",8,3);InkBox(t,"Cabinet recess",349,32,102,140,"AF855D",3,1);
   for(int n=0;n<2;n++){float y=92+n*65;InkBox(t,"Tea shelf",343,y,114,8,"DFB988",3,1);for(int k=0;k<3;k++){float x=354+k*31;InkBox(t,"Tea tin",x,y-38,23,34,k%2==0?"AAC3A0":"DDAF8D",4,1);Shape(t,"Tea tin lid",x-1,y-40,25,5,"EAD6A7",2);Shape(t,"Tea label",x+5,y-27,13,13,"F9E8BC",2);}}
   Shape(t,"Tea room skirting",0,220,480,10,"AD825A",0);Planks(t,230,"DEC19A");
   InkBox(t,"Side tea table",14,238,102,12,"B78C5E",4,2);Stroke(t,"Table foot",25,249,21,291,"A17A50",5);Stroke(t,"Table foot",104,249,108,291,"A17A50",5);
   Oval(t,"Teapot handle",72,208,24,25,"BC9071",3);InkBox(t,"Teapot body",34,207,47,31,"D79F79",12,2);Oval(t,"Teapot lid",37,203,42,10,"E7B98C",2);Oval(t,"Lid knob",54,198,9,9,"C69770",1);
   Stroke(t,"Tea steam",55,193,60,177,"FFF8DD90",3);Stroke(t,"Tea steam",68,196,71,186,"FFF8DD70",2);
   Pot(t,402,214,"9CAF8D");Lantern(t,214,29);Shape(t,"Tea saucer",87,229,22,5,"F1D6AA",0,true);InkBox(t,"Tea cup",88,216,17,14,"DCE1B9",4,1);
  }
  static void Pine(Transform t,float x,float y,float scale){
   var p=Rect(t,"Snow pine",x,y,74,159);p.localScale=Vector3.one*scale;Stroke(p,"Pine trunk",37,85,37,152,"967F63",7);
   for(int n=0;n<3;n++){float w=42+n*16,yy=n*30;Contour(p,"Pine bough",37-w/2,yy,w,77,n%2==0?"7DABA0":"91BAB0",new[]{new Vector2(120,0),new Vector2(240,150),new Vector2(0,150)});Contour(p,"Snow bough",37-w/2+5,yy+1,w-10,34,"EEF3E4",Rounded(120,0,240,150,170,124,115,145,54,122,0,150));}
  }
  static void WinterGarden(Transform t){
   Cloud(t,129,23,.8f,"F2F6E9");SunDisc(t,335,37,"F1E2AF");Hills(t,"B8D2CC","A0C3BF");
   for(int n=0;n<4;n++)Pine(t,20+n*126,97+n%2*17,.67f);
   Shape(t,"Snow field",0,218,480,91,"E8EFDC",0);Shape(t,"Snow drift",-20,197,262,69,"F2F4E5",0,true);Shape(t,"Snow drift",292,212,220,61,"F7F5E7",0,true);Pond(t,true);
   Stroke(t,"Ice crack",68,243,90,233,"88B9C05C",1);Stroke(t,"Ice crack",90,233,115,239,"88B9C05C",1);
   Pine(t,-14,129,1);Pine(t,409,118,1.12f);
   for(int n=0;n<15;n++)Shape(t,"Falling snow",17+(n*79)%460,16+(n*43)%194,3+n%2,3+n%2,"FFFBEA",0,true);
   for(int n=0;n<4;n++)Shape(t,"Snow pawprint",42+n*15,297-n*6,7,4,"B7CECA",0,true);
   Lantern(t,71,236);Rock(t,389,290,29,"B6C9C3");Shape(t,"Stone snowcap",391,287,24,6,"FFF7E7",0,true);
  }
  static void CoralBeach(Transform t){
   Cloud(t,37,27,1);Cloud(t,248,58,.67f);SunDisc(t,347,22,"F9DF9A");
   Shape(t,"Distant sea",0,143,480,106,"72BCAF",0);Shape(t,"Near sea",0,186,480,63,"91D3BE",0);
   for(int n=0;n<7;n++){float x=n*79-14;Shape(t,"Sea shimmer",x,155+n%3*19,45,2,"D8F3DA",1);Shape(t,"Foam scallop",x,231+n%2*6,98,17,"ECF1CF",0,true);}
   Shape(t,"Warm sand",0,249,480,60,"E9D4A1",0);Shape(t,"Beach dune",-31,224,246,66,"EFDAA7",0,true);Shape(t,"Beach dune",285,235,223,61,"F4DFB0",0,true);
   Shape(t,"Sand rest",104,266,274,29,"F6E5B7",0,true);
   Stroke(t,"Palm trunk edge",439,264,414,88,"8C7951",13);Stroke(t,"Palm trunk light",437,261,412,89,"B49A66",7);
   for(int n=0;n<6;n++)Stroke(t,"Palm bark",423+n*3,131+n*20,433+n*3,128+n*20,"947A51",2);
   for(int n=0;n<5;n++)Leaf(t,412,83,70,32,n%2==0?"82A863":"659764",-25+n*62);
   Oval(t,"Coconut",403,104,17,20,"AF8A59",2);Oval(t,"Coconut",419,105,17,20,"C39A67",2);
   for(int n=0;n<3;n++){float x=28+n*25;Stroke(t,"Beach grass",x,289,x-5,263,"A5AE6F",2);Stroke(t,"Beach grass",x+4,289,x+7,256,"A5AE6F",2);}
   Oval(t,"Seashell",37,297,24,13,"F3BA9B",1.5f);for(int n=0;n<3;n++)Stroke(t,"Shell ridge",42+n*5,300,46+n*3,307,"D7987F",1);
   Star(t,392,280,21,"E6AF82");Rock(t,80,290,14,"D3BF96");
  }
 }
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class CapySceneryGradient:MaskableGraphic {
  public Color top,bottom;
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();Rect r=rectTransform.rect;vh.AddVert(new Vector3(r.xMin,r.yMin),bottom,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),top,Vector2.up);vh.AddVert(new Vector3(r.xMax,r.yMax),top,Vector2.one);vh.AddVert(new Vector3(r.xMax,r.yMin),bottom,Vector2.right);vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);}
 }
}
