using UnityEngine;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  static void TreeScene(Transform t,int scene){
   if(scene==9){
    SunDisc(t,350,18,"F6D391");Hills(t,"B9CF9A","96B58C");Planks(t,232,"DCC297");
    Contour(t,"Orangery roof edge",22,6,436,91,Ink,Rounded(0,150,25,49,120,0,216,48,240,150));
    Contour(t,"Orangery glass roof",27,11,426,82,"B8D6BE",Rounded(0,150,25,49,120,0,216,48,240,150));
    for(int n=0;n<7;n++){float x=34+n*68;Stroke(t,"Greenhouse copper rib",240,16,x,88,"A88257",3);Stroke(t,"Greenhouse upright",x,87,x,234,"AD8B5D",4);}
    Stroke(t,"Glass horizontal frame",24,87,455,87,"C19D6D",6);Stroke(t,"Glass transom",29,153,450,153,"BDA077",3);
    for(int n=0;n<2;n++){var plant=Rect(t,"Citrus espalier",n==0?10:380,113,90,144);Stroke(plant,"Citrus trunk",44,109,44,10,"9C7652",7);for(int k=0;k<4;k++){Oval(plant,"Citrus foliage",4+k%2*24,k*20,61,40,k%2==0?"9DB97A":"BDD18F",1);TreeFruit(plant,17+k%2*33,k*22+9,19,"EAB06E");}InkBox(plant,"Citrus tub",13,104,63,30,"C6A274",8,2);}
    Pot(t,20,226,"BA8B6A",true);Pot(t,399,222,"BC9A72");
   }else if(scene==10){
    SunDisc(t,66,21,"F1DFA9");Hills(t,"9DBEAD","7AA99C");Shape(t,"Lotus banks",0,221,480,88,"A1BE9D",0);Pond(t);
    for(int s=0;s<2;s++){float x=s==0?15:390;Contour(t,"Waterfall cliff",x,58,76,173,"7F9B8A",Rounded(45,0,174,8,240,150,0,150));InkBox(t,"Waterfall ribbon",x+22,77,32,146,"A9D7D1",12,1);for(int k=0;k<3;k++){var flow=Rect(t,"Moving waterfall light",x+27+k*8,89+k*17,3,72);Shape(flow,"Water streak",0,0,3,72,"E1EFDB",1);AnimateDetail(flow,5,k*1.6f,5);}Shape(t,"Water mist",x+5,211,68,17,"D5E8CD",0,true);}
    for(int n=0;n<8;n++){float x=17+n*61,y=234+n%2*38;Lily(t,x,y,37);Flower(t,x+12,y-13,n%2==0?"EABBCD":"F8E3CA",.9f);}
    Rock(t,18,279,34,"9EAB92");Rock(t,421,280,42,"91A590");
   }else if(scene==11){
    SunDisc(t,351,18,"EFD8AA");Hills(t,"7D829C","6C8A8A");Shape(t,"Dusk meadow",0,223,480,86,"A7B69C",0);Pond(t);
    for(int n=0;n<5;n++){float x=25+n*105,y=27+n%2*28;Stroke(t,"Festival cord",x-24,y-8,x+80,y+13,"8D735F",2);var lamp=Rect(t,"Hanging festival lantern",x,y,40,55);Lantern(lamp,0,0);AnimateDetail(lamp,4,n*.8f,3);}
    for(int n=0;n<5;n++){var light=Rect(t,"Distant sky lantern",49+n*91,101+n%2*34,30,35);light.localScale=Vector3.one*.5f;TreeEffectMotif(light,15);AnimateDetail(light,0,n,8);}
    for(int n=0;n<6;n++){Flower(t,12+n*85,274,n%2==0?"D4B1CE":"E8D5A8",.8f);Tuft(t,25+n*85,299,"7E9976");}
   }else if(scene==12){
    Cloud(t,38,28,.9f);Cloud(t,300,52,.8f);SunDisc(t,356,18,"F5DB98");Hills(t,"C3D8A7","ACC892");Shape(t,"Clover field",0,219,480,90,"BDCF93",0);Pond(t);
    for(int s=0;s<2;s++){float x=s==0?35:407;Stroke(t,"Picnic post",x,212,x,103,"AF8B60",5);InkBox(t,"Little birdhouse",x-16,109,34,38,"DDB987",8,2);Contour(t,"Birdhouse roof",x-22,93,46,22,"AA7855",Rounded(0,150,120,0,240,150));Oval(t,"Birdhouse door",x-5,119,11,13,"7E6645",1);}
    for(int n=0;n<14;n++){float x=n<7?6+n*13:385+(n-7)*14,y=251+n%3*18;var clover=Rect(t,"Meadow clover",x,y,20,22);clover.localScale=Vector3.one*.6f;TreeEffectMotif(clover,16);if(n%3==0)AnimateDetail(clover,4,n,2);}
    Shape(t,"Picnic mat",132,278,216,23,"E9D6AD",0,true);for(int n=0;n<7;n++)Stroke(t,"Woven mat",150+n*25,284,150+n*25,295,"C6B080",1);
   }else if(scene==13){
    Shape(t,"Oak interior",0,0,480,309,"B79A76",0);Window(t,160,28,160,147);Planks(t,232,"D0B18A");
    for(int s=0;s<2;s++){float x=s==0?9:355;InkBox(t,"Oak bookcase",x,20,116,216,"9E7957",12,3);for(int row=0;row<3;row++){float y=55+row*59;for(int n=0;n<5;n++){string[] c={"8DA894","C5A38B","9FABBD","D4C18C","B09BAF"};InkBox(t,"Garden book",x+10+n*19,y-17+n%2*6,15,43-n%2*6,c[(n+row)%5],3,1);Stroke(t,"Book spine",x+14+n*19,y+14,x+20+n*19,y+14,"F4DEC0",1.5f);}InkBox(t,"Book shelf",x+4,y+28,108,7,"D5AF7B",2,1);}}
    Leaf(t,15,3,51,23,"90A873",-12);Leaf(t,406,4,53,24,"9CB77C",16);Lantern(t,334,173);Pot(t,20,238,"B68A6C",true);InkBox(t,"Reading cushion",352,267,91,30,"B3B991",12,2);InkBox(t,"Open journal",365,260,62,15,"EFE0B9",4,1);Stroke(t,"Journal crease",396,263,396,273,"B19876",1);
   }else{
    SunDisc(t,352,22,"FFE9AE");Cloud(t,-26,74,1.8f);Cloud(t,292,118,1.5f);Cloud(t,144,21,.8f);Hills(t,"D8D4C5","BECBC0");Shape(t,"Cloud meadow",0,224,480,85,"DDD8B1",0);Pond(t);
    for(int s=0;s<2;s++){float x=s==0?26:401;InkBox(t,"Sky gate pillar",x,86,38,146,"E7DBB2",11,2);InkBox(t,"Pillar cap",x-8,81,54,14,"E4BF7C",5,2);for(int k=0;k<3;k++)Stroke(t,"Pillar fluting",x+9+k*9,106,x+9+k*9,212,"C6BB95",2);Star(t,x+8,48,22,"E8BE6A");}
    Stroke(t,"Sky garland",61,88,239,32,"B9A875",3);Stroke(t,"Sky garland",239,32,420,88,"B9A875",3);
    for(int n=0;n<7;n++){float x=65+n*52,y=75-Mathf.Sin(n*Mathf.PI/6)*39;Leaf(t,x,y,27,15,"B4C688",n*12-36);}
    for(int n=0;n<6;n++){var mote=Rect(t,"Sun wing mote",22+n*84,135+n%2*36,34,34);mote.localScale=Vector3.one*.5f;TreeEffectMotif(mote,18);AnimateDetail(mote,3,n,6);}
    Cloud(t,-43,280,1.4f);Cloud(t,369,275,1.6f);Flower(t,85,281,"F3E3B2");Flower(t,376,282,"F4DEBD");
   }
  }
 }
}
