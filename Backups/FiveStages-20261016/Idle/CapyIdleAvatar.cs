using System.Collections.Generic;
using UnityEngine;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        void RebuildHero(){if(!hero)return;Clear(hero);Clear(decor);var scenery=Rect(decor,"Room scenery",0,-150,480,850);DrawScene(scenery,IdleCatalog.Find(Model.S.equipped[2]));
            if(Model.S.bed>0){Oval(decor,"Comfy cushion",97,487,286,45,Model.S.bed>3?"DB9B87":"A7C879",4);Shape(decor,"Cushion piping",112,493,256,26,"D4DF9D",0,true);for(int n=0;n<Model.S.bed;n++)Shape(decor,"Cushion stitch",142+n*23,518,10,2,"FFF1CE",1);}
            HomeFurniture();
            DrawAvatar(hero);BuildLifeVisuals();hero.localScale=Vector3.one*HeroScale;hero.anchoredPosition=new Vector2(240-120*HeroScale,-270-(1-HeroScale)*220);
        }
        static Vector2[] Rounded(params float[] coords){var corners=new List<Vector2>();for(int n=0;n<coords.Length;n+=2)corners.Add(new Vector2(coords[n],coords[n+1]));var points=new List<Vector2>();
            for(int n=0;n<corners.Count;n++){var p=corners[n];var a=Vector2.Lerp(p,corners[(n+corners.Count-1)%corners.Count],.32f);var b=Vector2.Lerp(p,corners[(n+1)%corners.Count],.32f);for(int k=0;k<7;k++){float t=k/6f;points.Add((1-t)*(1-t)*a+2*(1-t)*t*p+t*t*b);}}return points.ToArray();}
        static CapyContour Contour(Transform p,string n,float x,float y,float w,float h,string color,Vector2[] points){var c=Rect(p,n,x,y,w,h).gameObject.AddComponent<CapyContour>();c.points=points;c.sourceSize=new Vector2(240,150);c.color=C(color);c.raycastTarget=false;return c;}
        void DrawAvatar(Transform root,string preview=null,bool guest=false,int stagePreview=0,bool showcase=false){DrawLifeAvatar(root,preview,guest,stagePreview,showcase);}
        static void DrawFoodIcon(Transform parent,int kind,float x,float y){
            Oval(parent,"Bowl shadow",x-3,y+41,70,13,"CDB888",2);
            InkBox(parent,"Ceramic bowl",x,y+23,63,28,"7DBBB0",14,3);Shape(parent,"Bowl shine",x+10,y+34,20,3,"DDF0D1",1);
            if(kind==0)for(int n=0;n<3;n++)Leaf(parent,x+5+n*17,y+3+n%2*4,22,30,n%2==0?"97C96D":"6DA25F",10-n*10);
            if(kind==1)for(int n=0;n<3;n++){InkBox(parent,"Carrot",x+8+n*16,y+8+n%2*5,13,24,"F5AB5D",7,2);Leaf(parent,x+10+n*16,y+n%2*5,10,13,"87B764");Stroke(parent,"Carrot stripe",x+11+n*16,y+17+n%2*5,x+17+n*16,y+17+n%2*5,"C67B3E",2);}
            if(kind==2){Oval(parent,"Mandarin",x+13,y-1,37,35,"F4AC4C",3);Shape(parent,"Orange shine",x+21,y+7,9,6,"FFE09A",0,true);Leaf(parent,x+32,y-7,22,12,"7CA95B");}
            if(kind>=3){InkBox(parent,"Picnic cake",x+9,y+1,46,29,"F3BF8B",7,2);Shape(parent,"Cream",x+12,y+5,40,7,"FFF3D0",3);Shape(parent,"Berry jam",x+12,y+19,40,4,"CE7877",1);Oval(parent,"Cherry",x+29,y-5,12,12,"D87C73",2);}
            InkBox(parent,"Bowl rim",x-3,y+24,69,9,"D6E8BF",4,2);
        }
    }
}


