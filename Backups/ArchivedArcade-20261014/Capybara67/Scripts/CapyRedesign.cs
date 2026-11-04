using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    public sealed partial class CapyGame
    {
        bool redesignReady;
        int visibleCollectionColumns;
        GameObject settingsPanel,dailyPanel,redesignPause;
        Text settingsSound,settingsMotion,dailyMessage,dailyClaimLabel;
        Button dailyClaim;
        string dailyReceivedId;
        bool collectionFromResults,collectionFromDaily;
        readonly List<SoftShape[]> lifePaws=new List<SoftShape[]>();
        SoftShape timeFill;
        CapyContour newHead,newHeadOutline,newBody,newBodyOutline;
        RectTransform menuTag;
        GameObject[] menuObjects,playObjects;
        readonly List<SoftShape> redesignDetails=new List<SoftShape>();
        bool RedesignModal => settingsPanel && settingsPanel.activeSelf || dailyPanel && dailyPanel.activeSelf;
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        void PlaceBoard(string name,float x,float y,float w,float h){var r=board.Find(name) as RectTransform;if(r)Place(r,x,y,w,h);}
        void HideBoard(params string[] names){foreach(var n in names){var t=board.Find(n);if(t)t.gameObject.SetActive(false);}}
        static Vector2[] RoundedPolygon(Vector2[] corners,float fraction=.32f)
        {
            var pts=new List<Vector2>();for(int n=0;n<corners.Length;n++){
                var p=corners[n];var before=Vector2.Lerp(p,corners[(n+corners.Length-1)%corners.Length],fraction);var after=Vector2.Lerp(p,corners[(n+1)%corners.Length],fraction);
                for(int k=0;k<7;k++){float t=k/6f;pts.Add((1-t)*(1-t)*before+2*(1-t)*t*p+t*t*after);}
            }return pts.ToArray();
        }
        static Vector2[] Outline(params float[] coords){var p=new Vector2[coords.Length/2];for(int n=0;n<p.Length;n++)p[n]=new Vector2(coords[n*2],coords[n*2+1]);return RoundedPolygon(p);}
        CapyContour Contour(string name,float x,float y,float w,float h,Vector2[] points,string color)
        {
            var old=capy.Find(name).GetComponent<SoftShape>();old.enabled=false;Place(old.rectTransform,x,y,w,h);
            var c=Rect(old.transform,"Redesigned contour",0,0,w,h).gameObject.AddComponent<CapyContour>();c.sourceSize=new Vector2(240,150);c.points=points;c.color=C(color);c.raycastTarget=false;return c;
        }
        void ArtPart(string name,float x,float y,float w,float h,string color,float radius=8)
        {
            var t=capy.Find(name);if(!t)return;t.gameObject.SetActive(true);Place((RectTransform)t,x,y,w,h);
            var s=t.GetComponent<SoftShape>();if(s){s.color=C(color);s.radius=radius;s.SetVerticesDirty();}
        }
        void InitRedesign()
        {
            if(progress.visualVersion<3){progress.silhouette=0;progress.visualVersion=3;Persist();}
            // Replace the old head with an independently authored three-quarter silhouette.
            foreach(var s in capy.GetComponentsInChildren<SoftShape>(true)){
                if(s.name=="Forehead soft light" || s.name=="Ear rim light" || s.name=="Foot nail" || s.name=="Forehead tuft" || s.name=="Muzzle lower light" || s.name=="Nose soft shine" || s.name=="Belly shine")s.gameObject.SetActive(false);
            }
            var head=Outline(4,63,20,21,59,8,180,8,224,23,239,56,237,107,219,132,153,145,66,140,12,120);
            var body=Outline(42,4,163,3,208,49,225,116,200,146,38,145,9,112,14,57);
            newBodyOutline=Contour("Body outline",35,127,175,116,body,"493047");newBody=Contour("Body",40,130,165,106,body,"DDA779");
            ArtPart("Belly",99,159,76,65,"F5C998",27);
            ArtPart("Foot left",39,225,63,24,"A66F60",9);ArtPart("Foot right",148,225,62,24,"A66F60",9);
            ArtPart("Ear left outline",29,6,41,40,"493047",12);ArtPart("Ear left",34,11,31,29,"DDA779",10);ArtPart("Ear inner left",40,17,16,15,"A66F60",7);
            ArtPart("Ear right outline",122,4,33,31,"493047",10);ArtPart("Ear right",127,9,23,21,"DDA779",8);ArtPart("Ear inner right",132,13,12,10,"A66F60",5);
            newHeadOutline=Contour("Head outline",2,22,240,144,head,"493047");newHead=Contour("Head",7,27,230,134,head,"DDA779");
            capy.Find("Face light").gameObject.SetActive(false);
            ArtPart("Muzzle",155,52,72,93,"F5C998",29);
            ArtPart("Nose left",189,72,7,11,"493047",4);ArtPart("Nose right",211,70,7,11,"493047",4);
            ArtPart("Blush left",64,100,33,15,"EB7C87",7);ArtPart("Blush right",204,110,14,9,"EB7C87",4);
            ArtPart("Mouth",128,122,24,6,"493047",3);
            mouthShape.enabled=false;var smileRoot=Rect(mouthShape.transform,"Curved expression",0,0,24,6);smileRoot.anchorMax=Vector2.one;smileRoot.offsetMin=smileRoot.offsetMax=Vector2.zero;
            var smile=smileRoot.gameObject.AddComponent<CapySmile>();smile.color=C("493047");smile.raycastTarget=false;
            foreach(string footName in new[]{"Foot left","Foot right"}){
                var foot=capy.Find(footName) as RectTransform;var edge=Tile(capy,"Foot contour",foot.anchoredPosition.x-3,-foot.anchoredPosition.y-3,foot.sizeDelta.x+6,foot.sizeDelta.y+6,"493047",11);edge.transform.SetSiblingIndex(foot.GetSiblingIndex());
            }
            for(int n=0;n<eyes.Length;n++){
                Place(eyes[n],n==0?79:157,n==0?76:69,n==0?25:13,8);
                foreach(Transform child in eyes[n])child.gameObject.SetActive(false);
                var eye=eyes[n].Find("Eye outline");eye.gameObject.SetActive(true);Place((RectTransform)eye,0,0,n==0?25:13,8);eye.GetComponent<SoftShape>().color=C("493047");
                if(n<brows.Length)Place(brows[n],n==0?79:157,n==0?60:55,n==0?23:13,4);
            }
            foreach(var paw in new[]{leftPaw,rightPaw}){
                foreach(var s in paw.GetComponentsInChildren<SoftShape>())if(s.name.StartsWith("Paw ") && s.name!="Paw outline" || s.name.StartsWith("Toe"))s.gameObject.SetActive(false);
                Place(paw.Find("Paw outline") as RectTransform,0,0,62,57);Place(paw.Find("Paw") as RectTransform,4,3,54,49);Place(paw.Find("Palm") as RectTransform,10,6,41,18);
                paw.Find("Paw outline").GetComponent<SoftShape>().color=C("493047");
                for(int n=0;n<2;n++)Tile(paw,"Finger crease",24+n*14,35,3,12,"493047",1);
            }
            Place(leftPaw,149,591,62,57);Place(rightPaw,278,581,62,57);
            HideBoard("Moonlit garden","Pond and flowers","Fireflies","Brand pill","Brand","Score panel","Score label","Time label","Lives label","Island","Island top","Top ambient","Bottom ambient");
            foreach(var dot in comboDots)dot.gameObject.SetActive(false);
            board.Find("Board").GetComponent<SoftShape>().color=C("211B45");
            board.parent.Find("Full background").GetComponent<SoftShape>().color=C("19152F");
            PlaceBoard("Title",28,119,292,63);board.Find("Title").GetComponent<Text>().fontSize=41;
            PlaceBoard("67 pill",333,128,117,49);board.Find("67 pill").GetComponent<SoftShape>().color=C("55E6C1");PlaceBoard("67",334,126,112,49);
            PlaceBoard("Subtitle",28,188,420,35);board.Find("Subtitle").GetComponent<Text>().fontSize=19;
            Place(soundButton.GetComponent<RectTransform>(),392,24,64,64);Place(soundLabel.rectTransform,0,0,64,64);soundLabel.fontSize=13;
            soundButton.onClick.RemoveAllListeners();soundButton.onClick.AddListener(OpenSettings);
            Place(pauseButton.GetComponent<RectTransform>(),392,24,64,64);Place(pauseLabel.rectTransform,0,0,64,64);
            Place(scoreText.rectTransform,126,24,254,80);scoreText.fontSize=64;scoreText.alignment=TextAnchor.MiddleCenter;
            Place(bestText.rectTransform,24,25,110,60);bestText.fontSize=16;bestText.alignment=TextAnchor.MiddleLeft;
            Place(comboText.rectTransform,140,115,212,39);comboText.fontSize=23;comboText.color=C("55E6C1");
            Place(timerText.rectTransform,367,118,89,34);timerText.fontSize=23;timerText.alignment=TextAnchor.MiddleRight;
            for(int n=0;n<3;n++){
                Place(lifeTexts[n].rectTransform,24+n*34,115,32,36);lifeTexts[n].fontSize=24;
                var paw=Rect(board,"Life paw "+n,24+n*34,118,29,29);
                Tile(paw,"Pad",6,13,20,14,"FF5D8F",7);
                for(int toe=0;toe<3;toe++)Tile(paw,"Toe "+toe,3+toe*9,toe==1?1:5,7,9,"FF5D8F",4);
                lifePaws.Add(paw.GetComponentsInChildren<SoftShape>());
            }
            Tile(board,"Time track",24,165,432,5,"39315B",2);timeFill=Tile(board,"Time remaining",24,165,432,5,"55E6C1",2);
            Place(hintText.rectTransform,24,682,432,33);hintText.fontSize=18;
            Place(missionLabel.rectTransform,24,717,432,28);missionLabel.fontSize=15;
            Place(eventLabel.rectTransform,24,235,432,37);eventLabel.fontSize=15;
            Place(sparkText.rectTransform,24,190,432,30);sparkText.alignment=TextAnchor.MiddleCenter;sparkText.fontSize=18;
            Place(actionButton.GetComponent<RectTransform>(),24,762,432,64);Place(actionText.rectTransform,0,0,432,64);actionText.fontSize=24;
            // Keep the invisible hit mesh registered; switch input eligibility, not its GameObject.
            tapButton.targetGraphic.color=new Color(1,1,1,0);
            tapButton.targetGraphic.canvasRenderer.cullTransparentMesh=false;
            actionButton.GetComponent<SoftShape>().color=C("55E6C1");actionText.color=C("211B45");
            Place(collectionButton.GetComponent<RectTransform>(),24,749,210,72);Place(collectionText.rectTransform,0,0,210,72);collectionText.fontSize=16;collectionButton.GetComponent<SoftShape>().color=C("39315B");collectionText.color=C("FFF4D6");
            Place(dailyButton.GetComponent<RectTransform>(),246,749,210,72);Place(dailyLabel.rectTransform,0,0,210,72);dailyLabel.fontSize=16;dailyButton.GetComponent<SoftShape>().color=C("39315B");
            dailyButton.onClick.RemoveAllListeners();dailyButton.onClick.AddListener(OpenDaily);
            Place(walletText.rectTransform,146,27,225,57);walletText.fontSize=18;walletText.alignment=TextAnchor.MiddleRight;
            menuTag=Rect(board,"Menu rhythm",203,253,75,28);Tile(menuTag,"Six beat",0,10,26,9,"55E6C1",4);Tile(menuTag,"Seven beat",37,0,26,19,"FF9F1C",5);
            menuObjects=new[]{board.Find("Title").gameObject,board.Find("Subtitle").gameObject,board.Find("67 pill").gameObject,board.Find("67").gameObject,collectionButton.gameObject,dailyButton.gameObject,walletText.gameObject,soundButton.gameObject,menuTag.gameObject};
            playObjects=new[]{scoreText.gameObject,timerText.gameObject,comboText.gameObject,pauseButton.gameObject,timeFill.gameObject,board.Find("Time track").gameObject,sparkText.gameObject,eventLabel.gameObject,missionLabel.gameObject};
            BuildRedesignPanels();RedesignResults();redesignReady=true;ApplyAppearance();RefreshRedesign();
        }
        void ApplyRedesignAppearance(CapyItem skin)
        {
            string coat=skin.id=="classic"?"DDA779":skin.color;newHead.color=newBody.color=C(coat);
            foreach(var paw in new[]{leftPaw,rightPaw}){paw.Find("Paw").GetComponent<SoftShape>().color=C(coat);paw.Find("Palm").GetComponent<SoftShape>().color=Color.Lerp(C(coat),C("F5C998"),.65f);}
            ArtPart("Muzzle",155,52,72,93,"F5C998",29);capy.Find("Body").localScale=Vector3.one;
            int sil=progress.silhouette;
            var points=sil==1?Outline(4,62,26,17,110,0,203,18,239,66,226,114,180,147,57,143,7,112):sil==2?Outline(3,52,28,12,185,12,233,37,239,109,216,136,49,145,6,115):Outline(4,63,20,21,59,8,180,8,224,23,239,56,237,107,219,132,153,145,66,140,12,120);
            newHead.points=newHeadOutline.points=points;newHead.SetVerticesDirty();newHeadOutline.SetVerticesDirty();
            bodyLayer.anchoredPosition=new Vector2(0,-22);skinLayer.anchoredPosition=new Vector2(0,-12);
        }
        void LateUpdate(){if(redesignReady){RefreshRedesign();if(CollectionOpen && visibleCollectionColumns!=CollectionColumns){visibleCollectionColumns=CollectionColumns;RefreshCollection();}if(timeFill)timeFill.rectTransform.sizeDelta=new Vector2(432*Mathf.Clamp01(round.Remaining/67),5);}}
        void RefreshRedesign()
        {
            if(!redesignReady)return;bool playing=round.Running;bool home=!playing && !CollectionOpen && !resultPanel.activeSelf && !RedesignModal;
            foreach(var g in menuObjects)if(g.activeSelf!=home)g.SetActive(home);
            foreach(var g in playObjects)if(g.activeSelf!=playing)g.SetActive(playing);
            foreach(var t in lifeTexts)t.gameObject.SetActive(false);
            for(int n=0;n<lifePaws.Count;n++){
                lifePaws[n][0].transform.parent.gameObject.SetActive(playing);
                foreach(var part in lifePaws[n])part.color=C(n<round.Lives?"FF5D8F":"554568");
            }
            redesignPause.SetActive(playing && paused && !RedesignModal);
            if(redesignPause.activeSelf)redesignPause.transform.SetAsLastSibling();
            for(int n=0;n<rings.Length;n++){rings[n].gameObject.SetActive(playing);board.Find("Ring hole "+n).gameObject.SetActive(playing);board.Find("Palm number "+n).gameObject.SetActive(false);}
            foreach(var guide in flightGuides)guide.gameObject.SetActive(playing);
            fruit.gameObject.SetActive(playing);hintText.gameObject.SetActive(playing);tapButton.gameObject.SetActive(true);
            tapButton.targetGraphic.raycastTarget=playing;tapButton.targetGraphic.canvasRenderer.cullTransparentMesh=false;
            actionButton.gameObject.SetActive(home || playing);bestText.gameObject.SetActive(home || playing);
            capy.gameObject.SetActive(home || playing);leftPaw.gameObject.SetActive(home || playing);rightPaw.gameObject.SetActive(home || playing);
            capy.anchoredPosition=new Vector2(240,home?-453:-550);
            if(home){capy.localScale=Vector3.one*1.2f;Place(leftPaw,145,495,62,57);Place(rightPaw,286,484,62,57);}else{Place(leftPaw,149,591,62,57);Place(rightPaw,278,581,62,57);}
            Place(actionButton.GetComponent<RectTransform>(),24,home?650:762,432,64);
            bestText.text="РЕКОРД\n"+best;collectionText.text="КОЛЛЕКЦИЯ";dailyLabel.text="ПОДАРОК ДНЯ";soundLabel.text="ОПЦИИ";
            if(playing)comboText.text="СЕРИЯ "+round.Combo+"  ·  "+(round.Combo%7)+"/7";
            if(home)actionText.text="ИГРАТЬ";
        }
        GameObject Modal(string name,string title)
        {
            var root=Rect(board,name,0,0,480,850).gameObject;Tile(root.transform,"Backdrop",0,0,480,850,"211B45",0).raycastTarget=true;
            Label(root.transform,"Heading",title,24,42,432,74,32,"FFF4D6",TextAnchor.MiddleCenter,true);return root;
        }
        void BuildRedesignPanels()
        {
            redesignPause=Modal("Redesign pause","ПАУЗА");
            Label(redesignPause.transform,"Pause note","Мандарин подождёт",24,260,432,72,28,"FFF4D6",TextAnchor.MiddleCenter);
            var resume=Button(redesignPause.transform,"Resume game",24,580,432,76,"55E6C1",out var rt,"ПРОДОЛЖИТЬ",24);rt.color=C("211B45");resume.onClick.AddListener(TogglePause);
            var options=Button(redesignPause.transform,"Pause options",24,686,432,76,"39315B",out var ot,"НАСТРОЙКИ",23);options.onClick.AddListener(OpenSettings);redesignPause.SetActive(false);
            settingsPanel=Modal("Redesign settings","ТВОЙ КОМФОРТ");
            var s=Button(settingsPanel.transform,"Toggle effects sound",24,200,432,76,"39315B",out settingsSound,"",22);s.onClick.AddListener(()=>{sound=!sound;Persist();RefreshSettings();});
            var m=Button(settingsPanel.transform,"Toggle calm",24,300,432,76,"39315B",out settingsMotion,"",21);m.onClick.AddListener(()=>{progress.reducedEffects=!progress.reducedEffects;Persist();RefreshSettings();});
            Label(settingsPanel.transform,"Music status","Музыка — пока нет дорожки",24,418,432,45,19,"AEA2C6",TextAnchor.MiddleCenter);
            Label(settingsPanel.transform,"Language status","Язык: Русский",24,480,432,45,21,"FFF4D6",TextAnchor.MiddleCenter);
            var done=Button(settingsPanel.transform,"Settings done",24,744,432,76,"55E6C1",out var dt,"ГОТОВО",24);dt.color=C("211B45");done.onClick.AddListener(()=>{settingsPanel.SetActive(false);Refresh();});settingsPanel.SetActive(false);
            dailyPanel=Modal("Redesign daily","ПОДАРОК ДНЯ");
            Tile(dailyPanel.transform,"Gift box",153,222,174,140,"39315B",26);Tile(dailyPanel.transform,"Gift lid",143,218,194,29,"55E6C1",12);Tile(dailyPanel.transform,"Gift stripe",227,217,26,145,"FF9F1C",8);
            dailyMessage=Label(dailyPanel.transform,"Daily details","",36,411,408,155,24,"FFF4D6",TextAnchor.MiddleCenter);dailyMessage.horizontalOverflow=HorizontalWrapMode.Wrap;
            dailyClaim=Button(dailyPanel.transform,"Claim daily",24,630,432,76,"55E6C1",out dailyClaimLabel,"ЗАБРАТЬ",24);dailyClaimLabel.color=C("211B45");dailyClaim.onClick.AddListener(ClaimRedesignDaily);
            var back=Button(dailyPanel.transform,"Daily back",24,744,432,64,"39315B",out var bt,"НАЗАД",22);back.onClick.AddListener(()=>{dailyPanel.SetActive(false);Refresh();});dailyPanel.SetActive(false);
        }
        void OpenSettings(){if(round.Running && !paused)return;settingsPanel.SetActive(true);settingsPanel.transform.SetAsLastSibling();RefreshSettings();RefreshRedesign();}
        void RefreshSettings(){settingsSound.text="ЗВУКИ: "+(sound?"ВКЛ.":"ВЫКЛ.");settingsMotion.text="МЕНЬШЕ ДВИЖЕНИЯ: "+(progress.reducedEffects?"ВКЛ.":"ВЫКЛ.");}
        void OpenDaily(){if(round.Running)return;dailyPanel.SetActive(true);dailyPanel.transform.SetAsLastSibling();RefreshDailyPanel();RefreshRedesign();}
        void RefreshDailyPanel()
        {
            if(!string.IsNullOrEmpty(dailyReceivedId)){
                dailyMessage.text="ТВОЙ ПОДАРОК\n"+CapyCatalog.Find(dailyReceivedId).name;
                dailyClaim.interactable=true;dailyClaimLabel.text="ПРИМЕРИТЬ";return;
            }
            string date=DateTime.UtcNow.ToString("yyyy-MM-dd");bool today=string.CompareOrdinal(progress.dailyDate,date)>=0;
            bool left=false;foreach(var i in CapyCatalog.Items)if(CanBuy(i) && !progress.Has(i.id)){left=true;break;}
            dailyMessage.text=progress.rounds==0?"Сначала сыграй один раунд":today?"Сегодня подарок уже получен.\nНовый день — в 00:00 UTC":left?"Обычный предмет,\nкоторого у тебя ещё нет":"Все обычные подарки собраны!";
            dailyClaim.interactable=progress.rounds>0 && !today && left;dailyClaimLabel.text=today?"УЖЕ ПОЛУЧЕНО":"ЗАБРАТЬ";
        }
        void ClaimRedesignDaily(){
            if(!string.IsNullOrEmpty(dailyReceivedId)){
                var item=CapyCatalog.Find(dailyReceivedId);dailyPanel.SetActive(false);
                collectionFromDaily=true;collectionFromResults=false;selectedId=item.id;category=item.slot;
                collectionPage=ItemsForCategory().FindIndex(i=>i.id==item.id)/CollectionCapacity;OpenCollection();return;
            }
            string id=progress.rounds>0?progress.ClaimDaily(DateTime.UtcNow):null;
            if(id==null){RefreshDailyPanel();return;}
            dailyReceivedId=id;Persist();Journal("daily_item_claimed",id);RefreshDailyPanel();
        }
        void CloseCollection(){
            collectionPanel.SetActive(false);ApplyAppearance();
            if(collectionFromResults)resultPanel.SetActive(true);
            if(collectionFromDaily)OpenDaily();
            collectionFromResults=collectionFromDaily=false;Refresh();RefreshRedesign();
        }
        void RedesignResults()
        {
            var r=resultPanel.transform;r.Find("Result dim").GetComponent<SoftShape>().color=C("211B45");
            Place((RectTransform)r.Find("Result card"),24,257,432,346);r.Find("Result card").GetComponent<SoftShape>().color=C("302751");
            Place((RectTransform)r.Find("Round complete"),24,274,432,32);
            Place(resultTitle.rectTransform,36,320,408,88);Place(resultStats.rectTransform,42,423,396,76);resultStats.fontSize=19;
            Place(resultReward.rectTransform,42,505,396,81);resultReward.fontSize=17;resultReward.horizontalOverflow=HorizontalWrapMode.Wrap;
            Place(resultAgain.GetComponent<RectTransform>(),24,640,432,76);Place(resultAgain.GetComponentInChildren<Text>().rectTransform,0,0,432,76);
            resultAgain.GetComponent<SoftShape>().color=C("55E6C1");resultAgain.GetComponentInChildren<Text>().color=C("211B45");
            Place(resultClose.GetComponent<RectTransform>(),246,744,210,64);Place(resultClose.GetComponentInChildren<Text>().rectTransform,0,0,210,64);
            var c=Button(r,"Result collection",24,744,210,64,"39315B",out var ct,"КОЛЛЕКЦИЯ",17);c.onClick.AddListener(ChangeOutfit);
        }
        void RestyleCollection()
        {
            var r=collectionPanel.transform;
            Place((RectTransform)r.Find("Collection heading"),24,24,356,54);r.Find("Collection heading").GetComponent<Text>().text="ТВОЯ КОЛЛЕКЦИЯ";
            Place((RectTransform)r.Find("Close collection"),392,20,64,64);Place(r.Find("Close collection/Label") as RectTransform,0,0,64,64);
            Place((RectTransform)r.Find("Preview card"),24,100,432,215);r.Find("Preview card").GetComponent<SoftShape>().color=C("302751");
            Place(previewHolder,24,94,200,216);Place(itemTitle.rectTransform,232,112,208,55);Place(itemInfo.rectTransform,234,178,200,134);itemInfo.fontSize=17;
            for(int n=0;n<6;n++){var b=r.Find("Category "+n) as RectTransform;Place(b,24+n%3*148,332+n/3*66,136,56);Place(b.Find("Label") as RectTransform,0,0,136,56);b.Find("Label").GetComponent<Text>().fontSize=17;}
            Place(itemGrid,0,476,480,224);
            foreach(string name in new[]{"Previous page","Next page"}){var b=r.Find(name) as RectTransform;Place(b,name=="Previous page"?24:392,702,64,64);Place(b.Find("Label") as RectTransform,0,0,64,64);}
            Place(pageLabel.rectTransform,90,706,300,48);
            Place(equipItemButton.GetComponent<RectTransform>(),24,778,284,64);Place(equipLabel.rectTransform,0,0,284,64);equipItemButton.GetComponent<SoftShape>().color=C("55E6C1");
            equipLabel.fontSize=18;pageLabel.fontSize=16;
            var clear=r.Find("Clear slot") as RectTransform;Place(clear,320,778,136,64);Place(clear.Find("Label") as RectTransform,0,0,136,64);
            clear.Find("Label").GetComponent<Text>().fontSize=17;
            r.Find("Character silhouette").gameObject.SetActive(false);r.Find("Reduced effects").gameObject.SetActive(false);
            // Middle silhouette is the default; existing selections remain valid in saved profiles.
        }
    }
}
