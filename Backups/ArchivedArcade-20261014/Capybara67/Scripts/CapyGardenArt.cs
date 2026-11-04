using UnityEngine;
using UnityEngine.UI;

namespace Capybara67
{
    public sealed partial class CapyGame
    {
        [SerializeField] RectTransform[] eyes, pupils, fireflies;
        [SerializeField] RectTransform scarfEnd, waterHighlight;
        [SerializeField] GameObject[] cosmetics;
        [SerializeField] SoftShape[] comboDots;
        [SerializeField] RectTransform[] confetti;
        int feedbackKind;
        float gardenClock;

        void AddGardenDetails()
        {
            // Layered scenery stays behind the active game and never receives input.
            var garden = Rect(board,"Moonlit garden",0,0,480,850);
            garden.SetSiblingIndex(3);
            Shape(garden,"Moon halo",363,297,55,55,"443951",0,true);
            Shape(garden,"Moon",375,305,30,30,"F1D6A0",0,true);
            Shape(garden,"Moon shade",383,300,27,27,"443951",0,true);
            for(int i=0;i<18;i++) {
                float x=30+(i*83)%420, y=318+(i*37)%230;
                Shape(garden,"Distant star "+i,x,y,i%3==0?3:2,i%3==0?3:2,"9D91A755",1,true);
            }
            Shape(garden,"Far hill",-90,501,370,170,"303749",0,true);
            Shape(garden,"Far hill right",220,479,370,210,"2D3D46",0,true);
            Shape(garden,"Near hill",-130,547,420,142,"34464B",0,true);
            Shape(garden,"Near hill right",271,552,330,130,"354F50",0,true);
            for(int side=0;side<2;side++) for(int i=0;i<5;i++) {
                float x=side==0?30+i*11:428+i*6;
                var stem=Shape(garden,"Reed stem",x,491+i*14,3,100-i*9,"6B856B",1);
                stem.transform.localEulerAngles=new Vector3(0,0,(side==0?1:-1)*(12+i*4));
                var leaf=Shape(stem.transform,"Reed leaf",-4,13,13,44,"789571",0,true);
                leaf.transform.localEulerAngles=new Vector3(0,0,24);
                Shape(stem.transform,"Reed seed",-2,-8,7,28,"C8AA79",3);
            }
            var pond = Rect(board,"Pond and flowers",0,0,480,850);
            pond.SetSiblingIndex(capy.GetSiblingIndex());
            Shape(pond,"Water bank",41,616,398,52,"263139",0,true);
            Shape(pond,"Water surface",48,620,384,42,"42646B",0,true);
            Shape(pond,"Water inner",66,624,348,26,"54818A",0,true);
            waterHighlight=Shape(pond,"Water reflection",112,642,263,3,"9DC9BF",1,true).rectTransform;
            for(int i=0;i<7;i++) Shape(pond,"Water ripple "+i,73+i*46,635+(i%3)*7,15+i%3*8,2,"8AB2AF",1,true);
            Lotus(pond,69,629,.8f); Lotus(pond,392,644,.6f);
            for(int i=0;i<6;i++) {
                var pebble=Shape(pond,"River pebble",83+i*58,654+(i%2)*3,13+i%3*5,7,"6B7475",0,true);
                Shape(pebble.transform,"Pebble highlight",3,1,6,2,"A3B0A2",1,true);
            }
            // Face sculpting: warm planes, freckles, fur, brows and eye reflections.
            capy.Find("Eye left").gameObject.SetActive(false);
            capy.Find("Eye right").gameObject.SetActive(false);
            var forehead=Shape(capy,"Forehead soft light",55,43,139,13,"E7BF8C",6,true);
            forehead.transform.SetSiblingIndex(capy.Find("Muzzle").GetSiblingIndex());
            for(int side=0;side<2;side++) {
                float x=side==0?24:209;
                for(int i=0;i<3;i++) {
                    var fur=Shape(capy,"Cheek fur",x,108+i*8,11,3,"B58258",1);
                    fur.transform.localEulerAngles=new Vector3(0,0,side==0?14:-14);
                }
                for(int j=0;j<3;j++) Shape(capy,"Muzzle freckle",83+side*68+(j%2)*6,121+(j/2)*7,2.5f,2.5f,"B98D69",1,true);
                var brow=Shape(capy,"Eyebrow",53+side*115,67,23,4,"9D6C4B",2);
                brow.transform.localEulerAngles=new Vector3(0,0,side==0?-7:7);
                Shape(capy,"Ear rim light",40+side*142,11,21,5,"E6BC85",2,true);
                for(int toe=0;toe<3;toe++) Shape(capy,"Foot nail",48+side*110+toe*11,205,6,6,"D0A578",3);
            }
            eyes=new RectTransform[2]; pupils=new RectTransform[2];
            for(int i=0;i<2;i++) {
                eyes[i]=Rect(capy,"Expressive eye "+i,54+i*115,80,23,16);
                Shape(eyes[i],"Eye outline",0,0,23,16,"48323A",7);
                Shape(eyes[i],"Eye amber",5,2,14,12,"825E43",6);
                pupils[i]=Shape(eyes[i],"Pupil",8,2,10,12,"302A32",5).rectTransform;
                Shape(pupils[i],"Eye sparkle",2,1,4,4,"FFF0D3",2,true);
                Shape(eyes[i],"Lid",-1,-1,25,4,"BD8B61",2);
            }
            // Three tufts and a warm chin shadow give the flat character a little volume.
            for(int i=0;i<3;i++) { var tuft=Shape(capy,"Forehead tuft",111+i*7,24-i%2*4,5,15,"DFB581",2);tuft.transform.localEulerAngles=new Vector3(0,0,-15+i*15); }
            Shape(capy,"Muzzle lower light",89,143,66,3,"F4D7AD",1,true);
            Shape(capy,"Nose soft shine",101,110,4,2,"D6B087",1,true);
            Shape(capy,"Belly shine",91,166,57,5,"D7AD7C",2,true);
            foreach(var paw in new[]{leftPaw,rightPaw}) {
                Shape(paw,"Paw side shade",9,36,6,27,"B38459",3,true);
                Shape(paw,"Paw glint",19,7,29,3,"F1D2A3",1,true);
                for(int i=0;i<3;i++) Shape(paw,"Paw fur",49-i*6,50+i%2*4,3,7,"AA764F",1);
            }
            // Detailed fruit with peel stippling, stem and two-tone leaf.
            Shape(fruit,"Mandarin lower shade",8,30,28,5,"EB8932",2,true);
            for(int i=0;i<11;i++) Shape(fruit,"Peel pore",8+(i*7)%27,15+(i*5)%17,1.5f,1.5f,"E79037",1,true);
            Shape(fruit,"Fruit stem",21,-2,3,7,"71875C",1);
            var vein=Shape(fruit,"Leaf vein",26,-1,12,1,"D3F4C2",0);vein.transform.localEulerAngles=new Vector3(0,0,25);

            cosmetics=new GameObject[4];cosmetics[0]=null;cosmetics[1]=hat;
            hat.transform.SetAsLastSibling();
            Shape(hat.transform,"Hat band",29,9,117,7,"65B394",3);
            for(int i=0;i<8;i++) Shape(hat.transform,"Hat stitches",17+i*20,25,7,1,"C9F5D6",0);
            hat.transform.Find("Hat flower").gameObject.SetActive(false);
            Flower(hat.transform,88,6,7);
            var glasses=Rect(capy,"Sunset explorer",0,0,232,220);cosmetics[2]=glasses.gameObject;
            for(int i=0;i<2;i++) {
                Shape(glasses,"Lens rim",43+i*112,73,49,32,"F1C987",12);
                Shape(glasses,"Lens",47+i*112,77,41,24,"40516C",9);
                var shine=Shape(glasses,"Lens reflection",53+i*112,81,23,4,"8CC8D0",2);shine.transform.localEulerAngles=new Vector3(0,0,20);
            }
            Shape(glasses,"Glasses bridge",91,80,65,5,"F1C987",2);
            Shape(glasses,"Scarf neck",67,157,115,18,"EBA37B",7);
            scarfEnd=Shape(glasses,"Scarf tail",155,165,24,44,"D47B69",7).rectTransform;
            Shape(glasses,"Scarf knot",149,157,21,22,"F2B18A",8);
            var crown=Rect(capy,"Lotus royalty",0,0,232,220);cosmetics[3]=crown.gameObject;
            Shape(crown,"Crown shadow",63,8,123,37,"AE784B",6);
            Shape(crown,"Crown gold",66,5,117,33,"EAC071",6);
            for(int i=0;i<5;i++) {
                var tip=Shape(crown,"Crown ray",70+i*24,-14+(i%2)*6,12,31,"F4D895",4);
                tip.transform.localEulerAngles=new Vector3(0,0,12-i*6);
                Shape(crown,"Crown pearl",71+i*24,-17+(i%2)*6,9,9,"FFF0C5",0,true);
            }
            Shape(crown,"Crown band",65,26,120,9,"FFDFA2",3);
            Shape(crown,"Crown jewel",116,13,16,14,"92D9C0",5);
            foreach(var item in cosmetics) if(item) item.SetActive(false);
            fireflies=new RectTransform[7];
            var specks=Rect(board,"Fireflies",0,0,480,850);specks.SetSiblingIndex(tapButton.transform.GetSiblingIndex());
            for(int i=0;i<7;i++) {
                var f=Rect(specks,"Firefly "+i,45+i*62,377+(i%3)*91,14,14);fireflies[i]=f;
                Shape(f,"Glow",0,0,14,14,"F1D99518",0,true);Shape(f,"Light",5,5,4,4,"F2DDA2",0,true);
            }
            comboDots=new SoftShape[7];for(int i=0;i<7;i++) comboDots[i]=Shape(board,"Combo pearl "+i,182+i*18,311,8,8,"554965",4,true);
            confetti=new RectTransform[12];
            for(int i=0;i<12;i++) { var c=Shape(particles,"Confetti "+i,-43+i*24,-20+(i%3)*32,5,9,i%2==0?"F6D493":"A4F0CC",2);confetti[i]=c.rectTransform; }
            var subtitle=board.Find("Subtitle").GetComponent<Text>();subtitle.text="ЛОВИ ВАЙБ  /  ЛУННЫЙ САД";
        }

        static void Flower(Transform p,float x,float y,float size)
        {
            for(int i=0;i<5;i++) {float a=i*Mathf.PI*2/5; Shape(p,"Petal",x+Mathf.Cos(a)*size-size/2,y+Mathf.Sin(a)*size-size/2,size,size,"FFE5C7",0,true);}
            Shape(p,"Flower center",x-size/2,y-size/2,size,size,"EDB65F",0,true);
        }
        static void Lotus(Transform p,float x,float y,float scale)
        {
            var r=Rect(p,"Lotus",x,y,42,24);r.localScale=Vector3.one*scale;
            Shape(r,"Lotus leaf",-16,0,45,13,"789C7E",0,true);
            for(int i=0;i<3;i++) {var petal=Shape(r,"Lotus petal",-4+i*8,-15,13,21,i==1?"F4C6B8":"DFA6AD",0,true);petal.transform.localEulerAngles=new Vector3(0,0,30-i*30);}
            Shape(r,"Lotus heart",5,-3,8,6,"F3D192",3,true);
        }
        void AnimateGarden()
        {
            if (paused || platformPaused) return;
            gardenClock+=Time.unscaledDeltaTime;
            float blink=Mathf.Repeat(gardenClock,4.7f);
            for(int i=0;i<eyes.Length;i++) {
                eyes[i].localScale=new Vector3(1,blink>4.53f?.16f:1,1);
                pupils[i].anchoredPosition=new Vector2(8+Mathf.Clamp((fruit.anchoredPosition.x-240)/100,-2,2),-2);
            }
            for(int i=0;i<fireflies.Length;i++) {
                fireflies[i].anchoredPosition=new Vector2(45+i*62+Mathf.Sin(gardenClock*.5f+i)*7,-(377+(i%3)*91+Mathf.Sin(gardenClock*.7f+i)*9));
                fireflies[i].localScale=Vector3.one*(.7f+Mathf.Sin(gardenClock*1.2f+i)*.25f);
            }
            if(scarfEnd) scarfEnd.localEulerAngles=new Vector3(0,0,Mathf.Sin(gardenClock*2)*4);
            waterHighlight.localScale=new Vector3(1+Mathf.Sin(gardenClock)*.035f,1,1);
            for(int i=0;i<confetti.Length;i++) confetti[i].localEulerAngles=new Vector3(0,0,gardenClock*100+i*30);
            capy.localEulerAngles=new Vector3(0,0,Time.unscaledTime<partyUntil?Mathf.Sin(gardenClock*15)*3:0);
        }
    }
}
