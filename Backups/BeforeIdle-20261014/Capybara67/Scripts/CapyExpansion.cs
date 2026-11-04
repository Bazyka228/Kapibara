using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    public sealed partial class CapyGame
    {
        const string ProgressKey="capy67.progress.v2";
        CapyProgress progress;
        GameObject collectionPanel;
        RectTransform itemGrid,previewHolder,previewAvatar,resultAvatar;
        Text itemTitle,itemInfo,pageLabel,equipLabel,dailyLabel,missionLabel,eventLabel,formLabel,effectsLabel;
        Button equipItemButton,dailyButton;
        ItemSlot category;int collectionPage;
        int CollectionColumns => Mathf.Min(Screen.width,Screen.height*480f/850f)<420?3:4;
        int CollectionCapacity => CollectionColumns*2;
        string selectedId="panama",unlockNotice="";
        readonly Queue<string> journal=new Queue<string>();
        bool missionAnnounced;FlightMood loggedMood;
        public bool CollectionOpen => collectionPanel && collectionPanel.activeSelf;
        void InitExpansion()
        {
            try{progress=JsonUtility.FromJson<CapyProgress>(PlayerPrefs.GetString(ProgressKey,""));}catch(Exception){progress=null;}
            if(progress==null){progress=new CapyProgress();progress.Migrate(pearls,outfit);
                if(!PlayerPrefs.HasKey("capy67.save.v1.backup"))PlayerPrefs.SetString("capy67.save.v1.backup",PlayerPrefs.GetString(SaveKey,"{}"));}
            progress.Repair();progress.Unlock();
            BuildExpansionArt();BuildCollection();BuildExpansionEffects();ApplyAppearance();
            // Daily rewards are explicitly claimed from their own screen.
            PersistExpansion();PlayerPrefs.Save();
        }
        void PersistExpansion(){if(progress!=null)PlayerPrefs.SetString(ProgressKey,JsonUtility.ToJson(progress));}
        void Journal(string kind,string details="")
        {if(journal.Count==200)journal.Dequeue();journal.Enqueue(DateTime.UtcNow.ToString("o")+" | rules=2 | "+kind+" | "+details);}
        public string ExportJournal()=>string.Join("\n",journal.ToArray());
        void BeginExpansionRound(){missionAnnounced=false;loggedMood=FlightMood.Normal;fxAge=1;Journal("round_start","round="+progress.rounds);if(progress.rounds>0)Journal("mission_assigned",round.MissionName);}
        void CompleteExpansionRound()
        {
            var added=progress.Complete(round);
            unlockNotice=added.Count>0?"Открыто: "+CapyCatalog.Find(added[0]).name+(added.Count>1?"  +"+(added.Count-1):""):round.MissionDone?"Поручение выполнено: +2 жемчужины":"До новой награды — ещё одна попытка";
            foreach(var id in added)Journal("cosmetic_unlocked",id+"; source=achievement");
            Journal("round_end","reason="+(round.Lives==0?"misses":"time")+"; seconds="+(67-round.Remaining).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+"; hits="+round.Hits+"; perfect="+round.PerfectHits+"; combo="+round.BestCombo+"; misses="+round.Misses);
            StartReaction(round.Lives==0?10:11,.38f);
        }
        void ShowExpandedResultAvatar()
        {
            if(resultAvatar){resultAvatar.gameObject.SetActive(false);Destroy(resultAvatar.gameObject);}
            resultAvatar=Instantiate(capy.gameObject,resultPanel.transform).GetComponent<RectTransform>();
            resultAvatar.name="Result character";resultAvatar.gameObject.SetActive(true);resultAvatar.anchoredPosition=new Vector2(240,-150);resultAvatar.localScale=Vector3.one*.68f;resultAvatar.localRotation=Quaternion.identity;
            var lp=Instantiate(leftPaw.gameObject,resultAvatar).GetComponent<RectTransform>();lp.gameObject.SetActive(true);lp.name="Result left paw";lp.anchoredPosition=new Vector2(56,-180);
            var rp=Instantiate(rightPaw.gameObject,resultAvatar).GetComponent<RectTransform>();rp.gameObject.SetActive(true);rp.name="Result right paw";rp.anchoredPosition=new Vector2(149,-171);
        }
        void DailyGift()
        {
            if(progress.rounds==0){dailyLabel.text="ПОСЛЕ 1 РАУНДА";return;}
            string id=progress.ClaimDaily(DateTime.UtcNow);
            dailyLabel.text=id!=null?"ПОДАРОК ПОЛУЧЕН":progress.dailyDate==DateTime.UtcNow.ToString("yyyy-MM-dd")?"ДО ЗАВТРА":"ВСЁ СОБРАНО";
            if(id!=null){unlockNotice="Подарок: "+CapyCatalog.Find(id).name;hintText.text=unlockNotice;feedbackUntil=Time.unscaledTime+3;Journal("daily_item_claimed",id);Persist();}
        }
        void BuildCollection()
        {
            board.Find("Subtitle").GetComponent<Text>().text="ЛОВИ ВАЙБ";
            dailyButton=Button(board,"Daily gift",270,132,178,27,"403A56",out dailyLabel,"ПОДАРОК ДНЯ",11);
            dailyButton.onClick.AddListener(()=>{DailyGift();if(CollectionOpen)RefreshCollection();});
            collectionPanel=Rect(board,"Expanded collection",0,0,480,850).gameObject;
            var root=collectionPanel.transform;
            Shape(root,"Collection backdrop",0,0,480,850,"211B45",0).raycastTarget=true;
            Label(root,"Collection heading","ТВОЙ МАЛЕНЬКИЙ МИР",22,25,362,31,23,"F7EBDD",TextAnchor.MiddleLeft,true);
            var close=Button(root,"Close collection",415,22,43,37,"493D63",out var closeText,"×",25);
            close.onClick.AddListener(CloseCollection);
            Tile(root,"Preview card",20,76,440,219,"38304F",22);
            previewHolder=Rect(root,"Character preview",23,78,203,212);
            itemTitle=Label(root,"Selected name","",228,95,212,52,21,"F7EBDD",TextAnchor.MiddleLeft,true);itemTitle.horizontalOverflow=HorizontalWrapMode.Wrap;
            itemInfo=Label(root,"Selected details","",230,155,208,123,13,"C9BBD8");itemInfo.horizontalOverflow=HorizontalWrapMode.Wrap;
            string[] tabs={"ГОЛОВА","ОДЕЖДА","СЦЕНЫ","СЛЕДЫ","ЭМОЦИИ","КАПИБАРЫ"};
            for(int n=0;n<6;n++){int ix=n;var b=Button(root,"Category "+n,20+n%3*149,308+n/3*39,142,33,"49405F",out var tx,tabs[n],12);b.onClick.AddListener(()=>{category=(ItemSlot)ix;collectionPage=0;selectedId=ItemsForCategory()[0].id;RefreshCollection();});}
            itemGrid=Rect(root,"Item grid",0,388,480,291);
            var prev=Button(root,"Previous page",20,689,67,34,"493D63",out var pl,"←",19);prev.onClick.AddListener(()=>{collectionPage=Mathf.Max(0,collectionPage-1);RefreshCollection();});
            pageLabel=Label(root,"Page","",100,689,280,34,13,"C9BBD8",TextAnchor.MiddleCenter);
            var next=Button(root,"Next page",393,689,67,34,"493D63",out var nl,"→",19);next.onClick.AddListener(()=>{collectionPage=Mathf.Min((ItemsForCategory().Count-1)/CollectionCapacity,collectionPage+1);RefreshCollection();});
            equipItemButton=Button(root,"Equip selected",20,735,287,45,"A4F0CC",out equipLabel,"НАДЕТЬ",15);equipLabel.color=C("241D38");equipItemButton.onClick.AddListener(EquipSelected);
            var remove=Button(root,"Clear slot",317,735,143,45,"493D63",out var rm,"СНЯТЬ",12);remove.onClick.AddListener(()=>{progress.equipped[(int)category]=category==ItemSlot.Scene?"moon":category==ItemSlot.Skin?"classic":"";Persist();ApplyAppearance();RefreshCollection();});
            var form=Button(root,"Character silhouette",20,794,213,35,"39324E",out formLabel,"",11);form.onClick.AddListener(()=>{progress.silhouette=(progress.silhouette+1)%3;Persist();RefreshCollection();});
            var effects=Button(root,"Reduced effects",242,794,218,35,"39324E",out effectsLabel,"",11);effects.onClick.AddListener(()=>{progress.reducedEffects=!progress.reducedEffects;Persist();RefreshCollection();});
            RestyleCollection();
            collectionPanel.SetActive(false);
        }
        List<CapyItem> ItemsForCategory(){var list=new List<CapyItem>();foreach(var i in CapyCatalog.Items)if(i.slot==category)list.Add(i);return list;}
        void OpenCollection(){collectionPanel.SetActive(true);collectionPanel.transform.SetAsLastSibling();RefreshCollection();}
        void RefreshCollection()
        {
            ClearLayer(itemGrid);var list=ItemsForCategory();collectionPage=Mathf.Clamp(collectionPage,0,(list.Count-1)/CollectionCapacity);
            int columns=CollectionColumns;float cardWidth=(432-(columns-1)*12)/columns;
            var selected=CapyCatalog.Find(selectedId)??list[0];
            for(int n=0;n<CollectionCapacity;n++){
                int ix=collectionPage*CollectionCapacity+n;if(ix>=list.Count)break;var i=list[ix];
                var b=Button(itemGrid,"Item "+i.id,24+n%columns*(cardWidth+12),n/columns*112,cardWidth,106,i.id==selectedId?"665781":"3B3354",out var blank,"",1);
                DrawCatalogThumbnail(b.transform,i);
                var well=b.transform.Find("Thumbnail well");if(well)well.gameObject.SetActive(false);
                var art=b.transform.Find("Item illustration") as RectTransform;art.anchoredPosition+=new Vector2((cardWidth-61)/2,8);
                var title=Label(b.transform,"Item name",i.name,4,46,cardWidth-8,40,columns==3?16:13,"F7EBDD",TextAnchor.UpperLeft,true);title.horizontalOverflow=HorizontalWrapMode.Wrap;title.verticalOverflow=VerticalWrapMode.Truncate;
                Label(b.transform,"Item status",progress.equipped[(int)i.slot]==i.id?"НАДЕТО":progress.Has(i.id)?"ОТКРЫТО":Math.Min(progress.Value(i),i.target)+" / "+i.target,4,89,cardWidth-8,17,columns==3?13:11,progress.Has(i.id)?"A4F0CC":"B8AAC9");
                b.onClick.AddListener(()=>{selectedId=i.id;RefreshCollection();});
            }
            itemTitle.text=selected.name;
            bool owned=progress.Has(selected.id);bool purchasable=CanBuy(selected);
            itemInfo.text=selected.Rarity+"\n"+(owned?"Открыто навсегда":selected.Requirement+(selected.id=="glasses"?"\nили 4 идеальных подряд":selected.id=="robot"?" + 300 ловель":""))+"\n"+(selected.slot==ItemSlot.Skin?"Характер меняется.\nПравила одинаковые.":"Только внешний вид");
            equipItemButton.interactable=owned || purchasable && pearls>=20;
            equipLabel.text=owned?progress.equipped[(int)selected.slot]==selected.id?"НАДЕТО · ПРИМЕРКА":"НАДЕТЬ":purchasable?"ОТКРЫТЬ · 20 ЖЕМЧ.":"НУЖНО ДОСТИЖЕНИЕ";
            pageLabel.text=((int)category+1)+" / 6   ·   "+(collectionPage+1)+" / "+((list.Count+CollectionCapacity-1)/CollectionCapacity)+"   ·   "+pearls+" жемч.";
            formLabel.text="ФОРМА: "+new[]{"ВЫТЯНУТАЯ","КРУГЛАЯ","УГЛОВАТАЯ"}[progress.silhouette];
            effectsLabel.text=progress.reducedEffects?"ЭФФЕКТЫ: СПОКОЙНО":"ЭФФЕКТЫ: ПОЛНЫЕ";
            ApplyAppearance(selected.id);ClearLayer(previewHolder);
            var copy=Instantiate(capy.gameObject,previewHolder).GetComponent<RectTransform>();copy.gameObject.SetActive(true);previewAvatar=copy;copy.name="Live outfit preview";copy.localScale=Vector3.one*.70f;copy.localRotation=Quaternion.identity;copy.anchoredPosition=new Vector2(99,-116);
            foreach(var eye in copy.GetComponentsInChildren<RectTransform>())if(eye.name.StartsWith("Expressive eye"))eye.localScale=Vector3.one;
            if(selected.slot==ItemSlot.Scene){var scene=Rect(previewHolder,"Scene sample",0,0,203,212);scene.SetAsFirstSibling();Tile(scene,"Color",0,0,203,212,selected.color,16);}
            if(selected.slot==ItemSlot.Trail)Mark(previewHolder,new[]{"~ ~ ~","+ + +","~ ~ ~","+ + +","~ ~ ~","6 7 6"}[selected.art],10,177,183,26,23,selected.color);
            if(selected.slot==ItemSlot.Emotion)SetPreviewExpression(copy,selected.art);
            if(redesignReady){var lp=Instantiate(leftPaw.gameObject,copy).GetComponent<RectTransform>();lp.gameObject.SetActive(true);lp.anchoredPosition=new Vector2(58,-179);var rp=Instantiate(rightPaw.gameObject,copy).GetComponent<RectTransform>();rp.gameObject.SetActive(true);rp.anchoredPosition=new Vector2(152,-169);}
            ApplyAppearance();StartReaction(9,.32f);
        }
        bool CanBuy(CapyItem i)=>i.rarity==0 && i.slot!=ItemSlot.Skin && i.rule!=UnlockRule.Free && i.id!="panama" && i.id!="boat";
        void EquipSelected()
        {
            var i=CapyCatalog.Find(selectedId);if(i==null)return;
            if(!progress.Has(i.id)){if(!CanBuy(i) || pearls<20)return;pearls-=20;progress.Grant(i.id);progress.Unlock();Journal("cosmetic_unlocked",i.id+"; source=pearls");}
            if(!progress.Equip(i.id))return;Journal("cosmetic_equipped",i.id);Persist();ApplyAppearance();RefreshCollection();Refresh();
        }
        static void SetPreviewExpression(RectTransform face,int type)
        {
            var mouth=face.Find("Mouth").GetComponent<SoftShape>();mouth.rectTransform.sizeDelta=new Vector2(type==5?42:type==1 || type==2?14:29,type==4?4:type==1 || type==2?19:10);mouth.radius=8;mouth.SetVerticesDirty();
            foreach(var s in face.GetComponentsInChildren<SoftShape>())if(s.name=="Eyebrow")s.transform.localEulerAngles=new Vector3(0,0,type==4?18:0);
        }
    }
}
