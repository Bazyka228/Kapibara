using System;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle
{
    // A landscape surround, never a replacement or a second scale for the portrait board.
    // BoardFit remains the sole owner of the game board transform and safe area.
    [DefaultExecutionOrder(100)]
    public sealed class CapyDesktopLayout : MonoBehaviour
    {
        RectTransform board, surround, left, right, frame;
        CapyIdleGame game;
        Font regular, bold;
        Action<string> navigate;
        Func<bool> mayNavigate;
        CanvasGroup navigationGroup, helpGroup;
        readonly string[] pages={"home","care","activities","shop","surprises","quests"};
        readonly string[] names={"Мой сад","Забота","Занятия","Гардероб","Дерево","Задания"};
        readonly Button[] buttons=new Button[6];
        readonly SoftShape[] faces=new SoftShape[6];
        readonly Text[] labels=new Text[6];
        Text helpTitle,helpBody;
        string previousPage;
        bool previousAllowed;
        public bool IsDesktop { get; private set; }

        public void Initialize(RectTransform gameBoard,CapyIdleGame owner,Font bodyFont,Font titleFont,Action<string> openPage,Func<bool> navigationAllowed)
        {
            board=gameBoard;game=owner;regular=bodyFont;bold=titleFont;navigate=openPage;mayNavigate=navigationAllowed;
            surround=Rect(transform,"Desktop garden surround",0,0,1,1);
            surround.anchorMin=Vector2.zero;surround.anchorMax=Vector2.one;surround.offsetMin=surround.offsetMax=Vector2.zero;
            // The canvas backdrop remains first, then this surround, then the unchanged board.
            surround.SetSiblingIndex(board.GetSiblingIndex());
            var sky=Shape(surround,"Wide garden sky",0,0,1,1,"C6DDD0");Stretch(sky.rectTransform);
            LandscapeShape("Far garden hill",-.2f,-.1f,.65f,.63f,"AEC9A1");
            LandscapeShape("Right garden hill",.5f,-.12f,1.2f,.7f,"BDD2A3");
            LandscapeShape("Near meadow",-.3f,-.52f,1.3f,.32f,"91B27B");
            LandscapeShape("Sunlit meadow",.38f,-.4f,1.32f,.25f,"A7C38B");
            LandscapeShape("Quiet cloud left",.03f,.82f,.22f,.88f,"E9EFDC");
            LandscapeShape("Quiet cloud right",.77f,.73f,.96f,.78f,"E9EFDC");
            LandscapeShape("Small cloud",.86f,.87f,.97f,.9f,"DDE8D5");
            // These little plants belong to the surround only; no character art or common helpers change.
            for(int i=0;i<8;i++){
                var plant=Rect(surround,"Border plant "+i,0,0,55,66);
                plant.anchorMin=plant.anchorMax=new Vector2(i<4?.025f+i*.07f:.77f+(i-4)*.07f,.07f+(i%3)*.035f);
                plant.pivot=new Vector2(.5f,0);
                Shape(plant,"Stem",26,19,3,42,"648B57",2);
                var a=Shape(plant,"Left leaf",5,29,25,13,"769E63",0,true);a.rectTransform.localEulerAngles=new Vector3(0,0,-24);
                var b=Shape(plant,"Right leaf",27,22,25,13,"87AA6B",0,true);b.rectTransform.localEulerAngles=new Vector3(0,0,24);
                if(i%2==0){Shape(plant,"Petals",17,5,22,22,"F2DEAF",0,true);Shape(plant,"Flower heart",24,12,8,8,"D6AD62",0,true);}
            }
            frame=Rect(surround,"Desktop board shadow",0,0,496,866);Centered(frame);
            Shape(frame,"Board contact shadow",0,0,496,866,"355E4C55",17);
            left=Rect(surround,"Desktop navigation",0,0,230,650);Centered(left);
            navigationGroup=left.gameObject.AddComponent<CanvasGroup>();
            Text(left,"Garden sign","ТВОЙ САД",0,0,230,54,29,true);
            Text(left,"Garden subtitle","Всё рядом",0,51,230,35,23,false);
            for(int i=0;i<pages.Length;i++)BuildButton(i);
            right=Rect(surround,"Desktop help",0,0,230,400);Centered(right);
            helpGroup=right.gameObject.AddComponent<CanvasGroup>();helpGroup.blocksRaycasts=false;
            Shape(right,"Help ink",0,0,230,388,"65432D",22);
            Shape(right,"Help paper",3,3,224,382,"FFF1CE",19);
            Shape(right,"Help top stitch",20,16,190,3,"E1CDA0",1);
            helpTitle=Text(right,"Desktop control title","КЛИКАЙ И ИГРАЙ",16,24,198,76,22,true);
            helpBody=Text(right,"Desktop control hint","Гладь капибару\nкликом мыши",18,106,194,88,21,false);
            Key(right,"ПРОБЕЛ",24,205,182);Text(right,"Space hint","Погладить в саду",12,249,206,47,22,false);
            Key(right,"ESC",68,302,94);Text(right,"Escape hint","Назад",20,345,190,30,22,false);
            surround.gameObject.SetActive(false);
        }

        void BuildButton(int index)
        {
            float y=109+index*84;
            var edge=Shape(left,"Desktop nav "+pages[index],0,y,230,72,"65432D",17);edge.raycastTarget=true;
            var button=edge.gameObject.AddComponent<Button>();button.targetGraphic=edge;button.navigation=new Navigation{mode=Navigation.Mode.None};
            Shape(edge.transform,"Button depth",3,5,224,64,"AE895A",14);
            faces[index]=Shape(edge.transform,"Button face",4,3,222,60,"E7C895",13);
            Shape(edge.transform,"Button bevel",15,8,200,3,"F8E5AF",1);
            labels[index]=Text(edge.transform,"Label",names[index],8,0,214,65,27,true);
            edge.gameObject.AddComponent<GardenButtonState>();
            button.onClick.AddListener(()=>{if(IsDesktop&&game&&!game.PresentationPaused&&mayNavigate!=null&&mayNavigate())navigate?.Invoke(pages[index]);});
            buttons[index]=button;
        }

        void LateUpdate()
        {
            if(!board||!surround||!game)return;
            var parent=board.parent as RectTransform;if(!parent)return;
            Rect safe=Screen.safeArea;if(safe.width<=0||safe.height<=0)safe=new Rect(0,0,Screen.width,Screen.height);
            // Leave phone portrait and short landscape screens completely unchanged.
            IsDesktop=safe.width>=900&&safe.height>=600&&safe.width/safe.height>=1.3f;
            if(surround.gameObject.activeSelf!=IsDesktop)surround.gameObject.SetActive(IsDesktop);
            if(!IsDesktop)return;
            float scale=board.localScale.x;
            left.localScale=right.localScale=frame.localScale=Vector3.one*scale;
            Vector2 center=board.anchoredPosition;
            left.anchoredPosition=center+new Vector2(-380,5)*scale;
            right.anchoredPosition=center+new Vector2(380,0)*scale;
            frame.anchoredPosition=center+new Vector2(0,-3)*scale;
            bool allowed=mayNavigate!=null&&mayNavigate()&&!game.PresentationPaused;
            // Welcome, reward reveal and active games keep attention on the central screen.
            bool contextual=game.Page!="welcome"&&game.Page!="offline"&&game.Page!="surprise-opening"&&!game.Page.StartsWith("activity-");
            navigationGroup.alpha=helpGroup.alpha=contextual?1:0;
            navigationGroup.blocksRaycasts=contextual&&allowed;
            navigationGroup.interactable=contextual&&allowed;
            if(previousPage==game.Page&&previousAllowed==allowed)return;
            previousPage=game.Page;previousAllowed=allowed;
            for(int i=0;i<buttons.Length;i++){
                bool selected=game.Page==pages[i]||(pages[i]=="surprises"&&game.Page.StartsWith("surprise"));
                buttons[i].interactable=allowed&&!selected;
                faces[i].color=Color(selected?"A7C680":"E7C895");
                labels[i].text=(selected?"• ":"")+names[i];
            }
            bool home=game.Page=="home";
            helpTitle.text=home?"КЛИКАЙ И ИГРАЙ":"ВЫБЕРИ РАЗДЕЛ";
            helpBody.text=home?"Гладь капибару\nкликом мыши":"Выбери карточку:\nвнутри подсказка";
        }

        void LandscapeShape(string name,float x0,float y0,float x1,float y1,string color)
        {
            var shape=Shape(surround,name,0,0,1,1,color,0,true);var r=shape.rectTransform;
            r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;
        }
        void Key(Transform p,string caption,float x,float y,float w)
        {
            Shape(p,caption+" key edge",x,y,w,42,"A58A5D",8);Shape(p,caption+" key paper",x+2,y+2,w-4,35,"F7E7BD",6);
            Text(p,caption+" key",caption,x,y-2,w,40,23,true);
        }
        Text Text(Transform p,string name,string caption,float x,float y,float w,float h,int size,bool strong)
        {
            var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<Text>();t.font=strong?bold:regular;t.fontSize=size;t.text=caption;
            t.color=Color("513821");t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.lineSpacing=1.06f;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Centered(RectTransform r){r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;}
        static Color Color(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
        static RectTransform Rect(Transform p,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static SoftShape Shape(Transform p,string name,float x,float y,float w,float h,string color,float radius=0,bool oval=false)
        {
            var s=Rect(p,name,x,y,w,h).gameObject.AddComponent<SoftShape>();s.color=Color(color);s.radius=radius;s.ellipse=oval;s.raycastTarget=false;return s;
        }
    }
}
